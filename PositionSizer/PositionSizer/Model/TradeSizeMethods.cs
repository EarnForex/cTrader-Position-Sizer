using System;
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
        TradeSize.RiskInCurrency = Symbol.AmountRisked(TradeSize.Volume, RealStopLossPips) + CommissionFromVolume();
        TradeSize.RiskPercentage = TradeSize.RiskInCurrency / AccountSize.Value * 100.0;
        TradeSize.RewardInCurrency = GrossRewardInCurrency();
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
        var perUnitRisk = RealStopLossPips * Symbol.PipValue + CommissionPerUnitVolume();
        var volumeUnits = TradeSize.RiskInCurrency / perUnitRisk;
        var volumeUnitsFinal = normalize ? Symbol.NormalizeVolumeInUnits(volumeUnits, roundingMode) : volumeUnits;
        TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(volumeUnitsFinal);
        
        // Gross reward (before commission); the Result reward below nets out one round-trip commission.
        TradeSize.RewardInCurrency = GrossRewardInCurrency();
        TradeSize.RewardRiskRatio = TradeSize.RewardInCurrency / TradeSize.RiskInCurrency;
        
        TradeSize.IsLotsValueInvalid = TradeSize.Lots > MaxPositionSizeByMargin;
        
        // Results should reflect actuals including commission
        TradeSize.RiskInCurrencyResult = Symbol.AmountRisked(TradeSize.Volume, RealStopLossPips) + CommissionFromVolume();
        TradeSize.RiskPercentageResult = TradeSize.RiskInCurrencyResult / AccountSize.Value * 100.0;
        TradeSize.RewardCurrencyResult = TakeProfits.List[0].Pips == 0 ? 0 : TakeProfits.List.Select((x, i) => Symbol.AmountRisked(Symbol.NormalizeVolumeInUnits(TradeSize.Volume * x.Distribution / 100.0, roundingMode), RealTakeProfitPips(i))).Sum() - CommissionFromVolume();
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
        var perUnitRisk = RealStopLossPips * Symbol.PipValue + CommissionPerUnitVolume();
        var volumeUnits = moneyRiskTarget / perUnitRisk;
        var volumeUnitsFinal = normalize ? Symbol.NormalizeVolumeInUnits(volumeUnits, roundingMode) : volumeUnits;
        TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(volumeUnitsFinal);
        TradeSize.RiskInCurrency = moneyRiskTarget;
        // Gross reward (before commission); the Result reward below nets out one round-trip commission.
        TradeSize.RewardInCurrency = GrossRewardInCurrency();
        TradeSize.RewardRiskRatio = TradeSize.RewardInCurrency / TradeSize.RiskInCurrency;
        
        TradeSize.IsLotsValueInvalid = TradeSize.Lots > MaxPositionSizeByMargin;
        
        // Results should reflect actuals including commission
        TradeSize.RiskInCurrencyResult = Symbol.AmountRisked(TradeSize.Volume, RealStopLossPips) + CommissionFromVolume();
        TradeSize.RiskPercentageResult = TradeSize.RiskInCurrencyResult / AccountSize.Value * 100.0;
        TradeSize.RewardCurrencyResult = TakeProfits.List[0].Pips == 0 ? 0 : TakeProfits.List.Select((x, i) => Symbol.AmountRisked(Symbol.NormalizeVolumeInUnits(TradeSize.Volume * x.Distribution / 100.0, roundingMode), RealTakeProfitPips(i))).Sum() - CommissionFromVolume();
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

    /// <summary>
    /// Gross reward (before commission) for the current trade size across all take-profit levels.
    /// Uses <see cref="RealTakeProfitPips"/> so it tracks the spread-adjusted TP that will actually be placed.
    /// This must NOT include commission: the "Result" reward derives from this and nets out one round-trip
    /// commission. Deriving the input reward from the commission-inclusive risk instead would make commission
    /// appear to be deducted twice from the reward.
    /// </summary>
    private double GrossRewardInCurrency()
    {
        if (TakeProfits.List[0].Pips == 0)
            return 0.0;

        return TakeProfits.List
            .Select((tp, i) => Symbol.AmountRisked(TradeSize.Volume * tp.Distribution / 100.0, RealTakeProfitPips(i)))
            .Sum();
    }

    private double CommissionPerUnitVolume()
    {
        // round-trip commission per unit volume in account currency
        // uses StandardCommission() per lot, then divides by lot size and doubles (entry + exit)
        return Symbol.LotSize == 0 
            ? 2.0 * StandardCommission() / InputFallbackLotSize
            : 2.0 * StandardCommission() / Symbol.LotSize;
    }
}