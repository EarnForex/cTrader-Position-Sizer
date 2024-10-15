using System;
using System.Linq;
using cAlgo.API;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public partial class Model
{
    #region ForRiskView
    
    public bool CountPendingOrders { get; set; }
    public bool IgnoreOrdersWithoutStopLoss { get; set; }
    public bool IgnoreOrdersWithoutTakeProfit { get; set; }
    public bool IgnoreOrdersInOtherSymbols { get; set; }
    
    public Portfolio CurrentPortfolio { get; set; }
    public Portfolio PotentialPortfolio { get; set; }
    
    public void UpdateReadOnlyValues()
    {
        var updatedRiskCurrency = GetUpdatedRiskCurrency();
        
        CurrentPortfolio.RiskCurrency = updatedRiskCurrency;
        CurrentPortfolio.RiskPercentage = ((CurrentPortfolio.RiskCurrency / AccountSize.Value) * 100.0).Round();
        CurrentPortfolio.Lots = GetUpdatedRiskLots();

        var updatedRewardCurrency = GetUpdatedRewardCurrency();

        CurrentPortfolio.RewardCurrency = updatedRewardCurrency;
        CurrentPortfolio.RewardPercentage = CurrentPortfolio.RewardCurrency == 0 ? 0 : ((CurrentPortfolio.RewardCurrency / AccountSize.Value) * 100.0).Round();
        CurrentPortfolio.RewardRiskRatio = CurrentPortfolio.RewardCurrency == 0 || CurrentPortfolio.RiskCurrency == 0 ? double.NaN : (CurrentPortfolio.RewardCurrency / CurrentPortfolio.RiskCurrency).Round();

        double potentialRiskInCurrency;

        if (StopLoss.Pips == 0)
        {
            potentialRiskInCurrency = TradeType == TradeType.Buy 
                ? Symbol.AmountRisked(TradeSize.Volume, Symbol.Ask / Symbol.PipSize) 
                : double.PositiveInfinity;
        }
        else
        {
            potentialRiskInCurrency = Symbol.AmountRisked(TradeSize.Volume, StopLoss.Pips);
        }
        
        PotentialPortfolio.RiskCurrency = updatedRiskCurrency + potentialRiskInCurrency;
        PotentialPortfolio.RiskPercentage = ((PotentialPortfolio.RiskCurrency / AccountSize.Value) * 100.0).Round();
        PotentialPortfolio.Lots = GetUpdatedRiskLots() + TradeSize.Lots;

        double potentialRewardInCurrency = 0;

        if (TakeProfits.List.Any(x => x.Pips == 0))
        {
            if (TradeType == TradeType.Buy)
                potentialRewardInCurrency = double.PositiveInfinity;
            else
                foreach (var tp in TakeProfits.List)
                {
                    if (tp.Pips == 0)
                        potentialRewardInCurrency += Symbol.AmountRisked(TradeSize.Volume * tp.Distribution / 100.0, Symbol.Bid / Symbol.PipSize);
                    else
                        potentialRewardInCurrency += Symbol.AmountRisked(TradeSize.Volume * tp.Distribution / 100.0, tp.Pips);
                }
        }
        else
        {
            potentialRewardInCurrency = TakeProfits.List.Sum(x => Symbol.AmountRisked(TradeSize.Volume * x.Distribution / 100.0, x.Pips));
        }

        PotentialPortfolio.RewardCurrency = updatedRewardCurrency + potentialRewardInCurrency;
        PotentialPortfolio.RewardPercentage = PotentialPortfolio.RewardCurrency == 0 ? 0 : BotTools.PercentageIncrease(AccountSize.Value, AccountSize.Value + PotentialPortfolio.RewardCurrency).Round();
        PotentialPortfolio.RewardRiskRatio = PotentialPortfolio.RewardCurrency == 0 || PotentialPortfolio.RiskCurrency == 0 ?  double.NaN : (PotentialPortfolio.RewardCurrency / PotentialPortfolio.RiskCurrency).Round();
        
        //this was used to debug the model when updated 
        //ModelUpdated?.Invoke(this, EventArgs.Empty);
    }

    public double GetUpdatedRiskLots()
    {
        double lots = 0;
        
        foreach (var position in Positions)
        {
            if (IgnoreOrdersInOtherSymbols && position.Symbol != Symbol)
                continue;

            if (IgnoreOrdersWithoutStopLoss && !position.StopLoss.HasValue)
                continue;

            if (IgnoreOrdersWithoutTakeProfit && !position.TakeProfit.HasValue)
                continue;

            lots += position.Quantity;
        }
        
        if (CountPendingOrders)
        {
            foreach (var order in PendingOrders)
            {
                if (IgnoreOrdersInOtherSymbols && order.Symbol != Symbol)
                    continue;

                if (IgnoreOrdersWithoutStopLoss && !order.StopLossPips.HasValue)
                    continue;

                if (IgnoreOrdersWithoutTakeProfit && !order.TakeProfitPips.HasValue)
                    continue;

                lots += order.Quantity;
            }
        }

        return lots;
    }

    public double GetUpdatedRiskCurrency()
    {
        double riskCurrency = 0;

        foreach (var pos in Positions)
        {
            if (IgnoreOrdersInOtherSymbols && pos.Symbol != Symbol)
                continue;

            if (IgnoreOrdersWithoutStopLoss && !pos.StopLoss.HasValue)
                continue;

            if (!pos.StopLoss.HasValue && pos.TradeType == TradeType.Sell)
                return double.PositiveInfinity;

            if (!pos.StopLoss.HasValue)
                riskCurrency += pos.Symbol.AmountRisked(pos.VolumeInUnits, pos.Symbol.Bid / pos.Symbol.PipSize) + Math.Abs(pos.Commissions * 2);
            else
                riskCurrency += pos.Symbol.AmountRisked(pos.VolumeInUnits, pos.StopLossPips()) + Math.Abs(pos.Commissions * 2);
        }

        if (CountPendingOrders)
        {
            foreach (var order in PendingOrders)
            {
                if (IgnoreOrdersInOtherSymbols && order.Symbol != Symbol)
                    continue;

                if (IgnoreOrdersWithoutStopLoss && !order.StopLossPips.HasValue)
                    continue;
                
                if (!order.StopLossPips.HasValue && order.TradeType == TradeType.Sell)
                    return double.PositiveInfinity;

                if (!order.StopLossPips.HasValue)
                    riskCurrency += order.Symbol.AmountRisked(order.VolumeInUnits, order.TargetPrice / order.Symbol.PipSize);
                else
                    riskCurrency += order.Symbol.AmountRisked(order.VolumeInUnits, order.StopLossPips.Value);
            }
        }

        return riskCurrency;
    }

    public double GetUpdatedRewardCurrency()
    {
        double rewardCurrency = 0;
        
        foreach (var pos in Positions)
        {
            if (IgnoreOrdersInOtherSymbols && pos.Symbol != Symbol)
                continue;

            if (IgnoreOrdersWithoutTakeProfit && !pos.TakeProfit.HasValue)
                continue;

            if (!pos.TakeProfit.HasValue)
            {
                rewardCurrency += pos.TradeType == TradeType.Buy
                    ? double.PositiveInfinity
                    : pos.Symbol.AmountRisked(pos.VolumeInUnits, pos.Symbol.Ask / pos.Symbol.PipSize) - Math.Abs(pos.Commissions * 2);
            }
            else
            {
                rewardCurrency += pos.Symbol.AmountRisked(pos.VolumeInUnits, pos.TakeProfitPips()) - Math.Abs(pos.Commissions * 2);
            }
        }
        
        if (CountPendingOrders)
        {
            foreach (var order in PendingOrders)
            {
                if (IgnoreOrdersInOtherSymbols && order.Symbol != Symbol)
                    continue;

                if (IgnoreOrdersWithoutTakeProfit && !order.TakeProfitPips.HasValue)
                    continue;

                if (!order.TakeProfitPips.HasValue)
                {
                    rewardCurrency += order.TradeType == TradeType.Buy
                        ? double.PositiveInfinity
                        : order.Symbol.AmountRisked(order.VolumeInUnits, order.TargetPrice / order.Symbol.PipSize);
                }
                else
                {
                    rewardCurrency += order.Symbol.AmountRisked(order.VolumeInUnits, order.TakeProfitPips.Value);   
                }
            }
        }
        
        return rewardCurrency;
    }

    // public override string ToString()
    // {
    //     var sb = new StringBuilder();
    //     
    //     sb.AppendLine($"Count Pending Orders: {CountPendingOrders}");
    //     sb.AppendLine($"Ignore Orders Without Stop Loss: {IgnoreOrdersWithoutStopLoss}");
    //     sb.AppendLine($"Ignore Orders Without Take Profit: {IgnoreOrdersWithoutTakeProfit}");
    //     sb.AppendLine($"Ignore Orders In Other Symbols: {IgnoreOrdersInOtherSymbols}");
    //     
    //     sb.AppendLine("Current Portfolio:");
    //     sb.AppendLine(CurrentPortfolio.ToString());
    //     sb.AppendLine();
    //     sb.AppendLine("Potential Portfolio:");
    //     sb.AppendLine(PotentialPortfolio.ToString());
    //     
    //     return sb.ToString();
    //
    // }

    #endregion
}