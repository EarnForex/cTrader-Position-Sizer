using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public partial class Model
{
    public void UpdateTakeProfitsFromEntryPriceChanged()
    {
        if (IsAtrModeActive && TakeProfitMultiplier != 0)
        {
            UpdateTakeProfitFromAtr();

            //the extra TP lines must be updated in terms of pips if the mode is price
            //but if the mode is pips, the extra TP lines must be updated in terms of price
            for (int i = 1; i < TakeProfits.List.Count; i++)
            {
                if (TakeProfits.Mode == TargetMode.Price)
                {
                    TakeProfits.List[i].Pips = Math.Abs((TakeProfits.List[i].Price - EntryPrice) / Symbol.PipSize).Round(1);
                }
                else
                {
                    TakeProfits.List[i].Price = TradeType == TradeType.Buy
                        ? EntryPrice + TakeProfits.List[i].Pips * Symbol.PipSize
                        : EntryPrice - TakeProfits.List[i].Pips * Symbol.PipSize;
                }
            }
        }
        else if (TakeProfits.LockedOnStopLoss)
        {
            UpdateTakeProfitPipsLockedOnStopLoss();
        }
        else if (TakeProfits.Mode == TargetMode.Price)
        {
            foreach (var takeProfit in TakeProfits.List)
            {
                takeProfit.Pips = takeProfit.Price == 0
                    ? 0
                    : Math.Abs((takeProfit.Price - EntryPrice) / Symbol.PipSize).Round(1);
            }
        }
        else
        {
            UpdateTakeProfitPriceFromPips();
        }
    }
    
    public void UpdateTakeProfitsFromTradeTypeChange()
    {
        UpdateTakeProfitPriceFromPips();
    }

    /// <summary>
    /// Effective ("real") take-profit distance for the TP at <paramref name="index"/>, used in every reward /
    /// risk-reward calculation and at order placement. With Spread Adjustment (TP) on, this narrows the displayed TP by
    /// the current spread while the panel keeps showing the base (user-entered or ATR-derived) distance. Applies in both
    /// ATR and non-ATR modes. Returns the displayed pips unchanged when SA is off, when there is no TP, or when narrowing
    /// would make the distance non-positive. Computed live from <see cref="Symbol"/>.Spread so it tracks spread changes.
    /// </summary>
    public double RealTakeProfitPips(int index)
    {
        var pips = TakeProfits.List[index].Pips;

        if (!TakeProfitSpreadAdjusted || pips <= 0)
            return pips;

        var adjusted = Math.Round(pips - Symbol.Spread / Symbol.PipSize, 1);

        return adjusted > 0 ? adjusted : pips;
    }

    public void ChangeTakeProfitPips(int id, double pips)
    {
        if (pips == 0)
        {
            TakeProfits.List[id].Pips = 0.0;
            TakeProfits.List[id].Price = 0.0000;
            return;
        }
        
        TakeProfits.List[id].Pips = pips;
        TakeProfits.List[id].Price = TradeType == TradeType.Buy
            ? EntryPrice + pips * Symbol.PipSize
            : EntryPrice - pips * Symbol.PipSize;
    }

    /// <summary>
    /// MQL5 <c>ProcessTPChange</c> (Position Sizer.mqh) when <c>UseCommissionToSetTPDistance</c> is enabled:
    /// <c>tp_distance = (OutputRiskMoney * TPMultiplier + OutputPositionSize * commission * 2)
    /// / (OutputPositionSize * UnitCost_reward / TickSize)</c>,
    /// where <c>OutputRiskMoney = (SL loss + 2 * commission) * position size</c> and <c>commission</c> is one-way per lot.
    /// Here: <c>slBasedPips</c> = <c>StopLoss.Pips * LockedMultiplier * (tpIndex + 1)</c>, so <c>TPMultiplier = slBasedPips / StopLoss.Pips</c>.
    /// Falls back to <paramref name="slBasedPips"/> when the parameter is off, commission is zero, or volume is unknown.
    /// cAlgo uses <see cref="StandardCommission"/> (symbol commission) instead of MQL5's editable <c>CommissionPerLot</c>.
    /// </summary>
    public double CalculateTakeProfitPipsWithCommission(double slBasedPips)
    {
        if (slBasedPips == 0 || StopLoss.Pips == 0)
            return slBasedPips;

        if (!InputUseCommissionToSetTpDistance || StandardCommission() <= 0 || TradeSize.Volume == 0)
            return slBasedPips;

        var pipValueOne = Symbol.AmountRisked(TradeSize.Volume, 1);
        if (pipValueOne <= 0)
            return slBasedPips;

        var tpMultiplier = slBasedPips / StopLoss.Pips;
        var roundTripCommission = 2 * StandardCommission() * TradeSize.Lots;
        var riskMoney = Symbol.AmountRisked(TradeSize.Volume, StopLoss.Pips) + roundTripCommission;

        return Math.Round((riskMoney * tpMultiplier + roundTripCommission) / pipValueOne, 1);
    }

    public void SetTakeProfitFromSlDistance(int id, double slBasedPips)
    {
        if (slBasedPips == 0)
        {
            TakeProfits.List[id].Pips = 0.0;
            TakeProfits.List[id].Price = 0.0000;
            return;
        }

        var pips = CalculateTakeProfitPipsWithCommission(slBasedPips);
        TakeProfits.List[id].Pips = pips;
        TakeProfits.List[id].Price = TradeType == TradeType.Buy
            ? EntryPrice + pips * Symbol.PipSize
            : EntryPrice - pips * Symbol.PipSize;
    }

    /// <summary>
    /// Updates the persisted <see cref="TakeProfits.CommissionPipsExtra"/> cache (extra pips at 1× SL). Requires trade size to be set.
    /// </summary>
    public void RefreshCommissionPipsExtra()
    {
        if (!InputUseCommissionToSetTpDistance || StopLoss.Pips == 0 || StandardCommission() <= 0)
        {
            TakeProfits.CommissionPipsExtra = 0;
            return;
        }

        TakeProfits.CommissionPipsExtra = CalculateTakeProfitPipsWithCommission(StopLoss.Pips) - StopLoss.Pips;
    }

    public void UpdateTakeProfitFromAtr()
    {
        if (TakeProfitMultiplier == 0)
            return;

        //Only sets the base ATR distance; Spread Adjustment is applied downstream via RealTakeProfitPips.
        if (TakeProfits.LockedOnStopLoss)
        {
            UpdateTakeProfitPipsLockedOnStopLoss();

            TakeProfits.List[0].Pips = Math.Abs(Math.Round(TakeProfits.List[0].Pips, 1));

            TakeProfits.List[0].Price = TradeType == TradeType.Buy
                ? EntryPrice + TakeProfits.List[0].Pips * Symbol.PipSize
                : EntryPrice - TakeProfits.List[0].Pips * Symbol.PipSize;
        }
        else
        {
            TakeProfits.List[0].Pips = Math.Abs(Math.Round(GetAtrPips() * TakeProfitMultiplier, 1));

            TakeProfits.List[0].Price = TradeType == TradeType.Buy
                ? EntryPrice + TakeProfits.List[0].Pips * Symbol.PipSize
                : EntryPrice - TakeProfits.List[0].Pips * Symbol.PipSize;
        }
    }

    /// <summary>
    /// If Pips are negative, it will be set to positive
    /// </summary>
    public void NormalizeTakeProfitPips()
    {
        foreach (var takeProfit in TakeProfits.List) 
            takeProfit.Pips = Math.Abs(takeProfit.Pips);
        
        UpdateTakeProfitPriceFromPips();
    }

    public void UpdateTakeProfitPrice(int id, double price)
    {
        if (price == 0)
        {
            TakeProfits.List[id].Pips = 0.0;
            TakeProfits.List[id].Price = 0.0000;
            return;
        }
        
        TakeProfits.List[id].Price = price;
        TakeProfits.List[id].Pips = Math.Abs((EntryPrice - price) / Symbol.PipSize).Round(1);
    }

    public void AddNewTakeProfit(bool prefillAdditionalTpsBasedOnMain)
    //todo adjust here for when TP is locked for the new TP
    {
        var takeProfit = new TakeProfit();
        var pips = TakeProfits.List[0].Pips * (TakeProfits.List.Count + 1);
        
        if (prefillAdditionalTpsBasedOnMain)
        {
            takeProfit.Price = TradeType == TradeType.Buy 
                ? EntryPrice + pips * Symbol.PipSize : EntryPrice - pips * Symbol.PipSize;
            takeProfit.Pips = pips;
        }
        else
        {
            takeProfit.Price = 0.0000;
            takeProfit.Pips = 0.0;
        }

        TakeProfits.List.Add(takeProfit);
    }

    public void UpdateTakeProfitPriceFromPips()
    {
        foreach (var takeProfit in TakeProfits.List)
        {
            takeProfit.Price = takeProfit.Pips == 0
                ? 0
                : TradeType == TradeType.Buy
                    ? EntryPrice + takeProfit.Pips * Symbol.PipSize
                    : EntryPrice - takeProfit.Pips * Symbol.PipSize;
        }   
    }

    public void UpdateTakeProfitPipsLockedOnStopLoss()
    {
        for (var i = 0; i < TakeProfits.List.Count; i++)
            SetTakeProfitFromSlDistance(i, StopLoss.Pips * TakeProfits.LockedMultiplier * (i + 1));

        RefreshCommissionPipsExtra();
    }
    
    public void UpdateTakeProfitPipsLockedOnStopLoss(int id)
    {
        SetTakeProfitFromSlDistance(id, StopLoss.Pips * TakeProfits.LockedMultiplier * (id + 1));
        RefreshCommissionPipsExtra();
    }

    public void SetTakeProfitLockedMultiplier(double value)
    {
        TakeProfits.LockedMultiplier = value;
    }

    public void SyncAtrTakeProfitMultiplierFromLocked()
    {
        if (!IsAtrModeActive || StopLossMultiplier <= 0)
            return;

        TakeProfitMultiplier = Math.Round(StopLossMultiplier * TakeProfits.LockedMultiplier, 2);
    }
    
    public bool IsAnyTakeProfitInvalid()
    {
        return TradeType == TradeType.Buy 
            ? TakeProfits.List.Any(takeProfit => takeProfit.Price <= EntryPrice) 
            : TakeProfits.List.Any(takeProfit => takeProfit.Price >= EntryPrice);
    }
}