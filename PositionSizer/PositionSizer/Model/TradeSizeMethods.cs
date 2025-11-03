using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots;
using static PositionSizer.Tools.VolumeTools;

namespace cAlgo.Robots;

public partial class Model
{
    //For Bugs and issues, please refer to MT5 function CalculateRiskAndPositionSize
    //to compare the logic
    
    public void UpdateWithTradeSizeLots(double lots)
    {
        TradeSize.Lots = lots;
        TradeSize.LastRiskValueChanged = LastRiskValueChanged.LotSize;
        TradeSize.RiskInCurrency = Symbol.AmountRisked(TradeSize.Volume, StopLoss.Pips) + CommissionFromVolume();
        TradeSize.RiskPercentage = TradeSize.RiskInCurrency / AccountSize.Value * 100.0;
        TradeSize.RewardInCurrency = TakeProfits.List[0].Pips == 0 ? 0 : TakeProfits.List.Sum(x => Symbol.AmountRisked(TradeSize.Volume * x.Distribution / 100.0, x.Pips));
        TradeSize.RewardRiskRatio = TradeSize.RewardInCurrency / TradeSize.RiskInCurrency;

        TradeSize.IsLotsValueInvalid = TradeSize.Lots > MaxPositionSizeByMargin;
        
        TradeSize.RiskInCurrencyResult = TradeSize.RiskInCurrency;
        TradeSize.RiskPercentageResult = TradeSize.RiskPercentage;
        TradeSize.RewardCurrencyResult = TradeSize.RewardInCurrency - CommissionFromVolume();
        TradeSize.RewardRiskRatioResult = TradeSize.RewardCurrencyResult / TradeSize.RiskInCurrencyResult;
    }

    public void UpdateWithRiskInCurrency(double moneyRisk, RoundingMode roundingMode)
    {
        TradeSize.RiskInCurrency = moneyRisk;
        TradeSize.LastRiskValueChanged = LastRiskValueChanged.RiskCurrency;
        TradeSize.RiskPercentage = TradeSize.RiskInCurrency / AccountSize.Value * 100.0;
        // Include commission in per-unit risk when sizing by money risk
        var normalize = !InputSurpassBrokerMaxPositionSizeWithMultipleTrades && !InputCalculateUnadjustedPositionSize;
        var perUnitRisk = StopLoss.Pips *Symbol.PipValue + CommissionPerUnitVolume();
        var volumeUnits = TradeSize.RiskInCurrency / perUnitRisk;
        var volumeUnitsFinal = normalize ? Symbol.NormalizeVolumeInUnits(volumeUnits, roundingMode) : volumeUnits;
        TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(volumeUnitsFinal);
        
        TradeSize.RewardInCurrency = GetRewardInCurrencyUsingRiskPercentage(TradeSize.RiskPercentage);
        TradeSize.RewardRiskRatio = TradeSize.RewardInCurrency / TradeSize.RiskInCurrency;
        
        TradeSize.IsLotsValueInvalid = TradeSize.Lots > MaxPositionSizeByMargin;
        
        // Results should reflect actuals including commission
        TradeSize.RiskInCurrencyResult = Symbol.AmountRisked(TradeSize.Volume, StopLoss.Pips) + CommissionFromVolume();
        TradeSize.RiskPercentageResult = TradeSize.RiskInCurrencyResult / AccountSize.Value * 100.0;
        TradeSize.RewardCurrencyResult = TakeProfits.List[0].Pips == 0 ? 0 : TakeProfits.List.Sum(x => Symbol.AmountRisked(Symbol.NormalizeVolumeInUnits(TradeSize.Volume * x.Distribution / 100.0, roundingMode), x.Pips)) - CommissionFromVolume();
        TradeSize.RewardRiskRatioResult = TradeSize.RewardCurrencyResult / TradeSize.RiskInCurrencyResult;
    }

    public void UpdateWithRiskPercentage(double riskPercentage, RoundingMode roundingMode)
    {
        TradeSize.RiskPercentage = riskPercentage;
        TradeSize.LastRiskValueChanged = LastRiskValueChanged.RiskPercentage;
        //Normalize only if SurpassBrokerMaxPositionSizeWithMultipleTrades is false
        //or also if CalculateUnadjustedPositionSize is false?
        var normalize = !InputSurpassBrokerMaxPositionSizeWithMultipleTrades && !InputCalculateUnadjustedPositionSize;
        // Include commission in per-unit risk when sizing by risk %
        var moneyRiskTarget = AccountSize.Value * TradeSize.RiskPercentage / 100.0;
        var perUnitRisk = StopLoss.Pips * Symbol.PipValue + CommissionPerUnitVolume();
        var volumeUnits = moneyRiskTarget / perUnitRisk;
        var volumeUnitsFinal = normalize ? Symbol.NormalizeVolumeInUnits(volumeUnits, roundingMode) : volumeUnits;
        TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(volumeUnitsFinal);
        TradeSize.RiskInCurrency = moneyRiskTarget;
        TradeSize.RewardInCurrency = GetRewardInCurrencyUsingRiskPercentage(riskPercentage);
        TradeSize.RewardRiskRatio = TradeSize.RewardInCurrency / TradeSize.RiskInCurrency;
        
        TradeSize.IsLotsValueInvalid = TradeSize.Lots > MaxPositionSizeByMargin;
        
        // Results should reflect actuals including commission
        TradeSize.RiskInCurrencyResult = Symbol.AmountRisked(TradeSize.Volume, StopLoss.Pips) + CommissionFromVolume();
        TradeSize.RiskPercentageResult = TradeSize.RiskInCurrencyResult / AccountSize.Value * 100.0;
        TradeSize.RewardCurrencyResult = TakeProfits.List[0].Pips == 0 ? 0 : TakeProfits.List.Sum(x => Symbol.AmountRisked(Symbol.NormalizeVolumeInUnits(TradeSize.Volume * x.Distribution / 100.0, roundingMode), x.Pips)) - CommissionFromVolume();
        TradeSize.RewardRiskRatioResult = TradeSize.RewardCurrencyResult / TradeSize.RiskInCurrencyResult;
    }

    public void SetRiskDefaults(RoundingMode roundingMode)
    {
        UpdateWithRiskPercentage(1.0, roundingMode);
    }

    public void UpdateTradeSizeValues(RoundingMode roundingMode)
    {
        switch (TradeSize.LastRiskValueChanged)
        {
            case LastRiskValueChanged.RiskPercentage:
                UpdateWithRiskPercentage(TradeSize.RiskPercentage, roundingMode);
                break;
            case LastRiskValueChanged.RiskCurrency:
                UpdateWithRiskInCurrency(TradeSize.RiskInCurrency, roundingMode);
                break;
            case LastRiskValueChanged.LotSize:
                UpdateWithTradeSizeLots(TradeSize.Lots);
                break;
            default:
                SetRiskDefaults(roundingMode);
                break;
        }
    }

    private double GetRewardInCurrencyUsingRiskPercentage(double riskPercentage)
    {
        if (TakeProfits.List[0].Pips == 0)
            return 0.0;
        
        var rInCurrency = 0.0;

        foreach (var t in TakeProfits.List)
        {
            //if with 20 pips of SL I risk 1%, then with 40 pips of TP I get a reward of 2%
            var proportion = t.Pips / StopLoss.Pips;

            rInCurrency += AccountSize.Value * riskPercentage / 100.0 * proportion * t.Distribution / 100.0;
        }

        return rInCurrency;
    }

    private double CommissionPerUnitVolume()
    {
        // round-trip commission per unit volume in account currency
        // uses StandardCommission() per lot, then divides by lot size and doubles (entry + exit)
        return 2.0 * StandardCommission() / Symbol.LotSize;
    }
}