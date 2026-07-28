using System;
using System.Diagnostics;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public partial class Model
{
    public void UpdateStopLossFromTradeTypeChange()
    {
        var sign = TradeType == TradeType.Buy ? -1 : +1;
        
        StopLoss.Price = EntryPrice + sign * StopLoss.Pips * Symbol.PipSize;
    }

    public void UpdateStopLossFromEntryPriceChange()
    {
        if (IsAtrModeActive && StopLossMultiplier != 0)
        {
            UpdateStopLossFromAtr();
            return;
        }

        if (StopLoss.Mode == TargetMode.Pips)
        {
            if (TradeType == TradeType.Buy)
            {
                //sometimes the pips are negative
                var newPotentialStopLossPrice = EntryPrice - StopLoss.Pips * Symbol.PipSize;

                if (newPotentialStopLossPrice > EntryPrice)
                {
                    //Don't change the price, only the pips
                    StopLoss.Pips = Math.Round((EntryPrice - StopLoss.Price) / Symbol.PipSize, 1);
                }
                else
                {
                    StopLoss.Price = newPotentialStopLossPrice;   
                }
            }
            else
            {
                var newPotentialStopLossPrice = EntryPrice + StopLoss.Pips * Symbol.PipSize;
                    
                if (newPotentialStopLossPrice < EntryPrice)
                {
                    //Don't change the price, only the pips
                    StopLoss.Pips = Math.Round((StopLoss.Price - EntryPrice) / Symbol.PipSize, 1);
                }
                else
                {
                    StopLoss.Price = newPotentialStopLossPrice;
                }
            }
        }
        else
        {
            if (TradeType == TradeType.Buy)
            {
                //Sl can be greater than entry price
                //Also equal
                //But if it's slightly lower than the entry price, and less than ticksize then it's a problem
                //because this means the sl price would be the same as the entry price after rounding
                //It would set SL Pips to zero and we don't want that
                if (StopLoss.Price > EntryPrice - Symbol.TickSize)
                    //Do not change the pips if the result would be negative
                    //This arises from Issue 55
                    //https://github.com/Waxavi/Andrii-moraru-ctrader/issues/55
                    ChangeStopLossPips(StopLoss.Pips);
                else
                    StopLoss.Pips = Math.Round((EntryPrice - StopLoss.Price) / Symbol.PipSize, 1);
            }
            else
            {
                if (StopLoss.Price < EntryPrice + Symbol.TickSize)
                    //Do not change the pips if the result would be negative
                    ChangeStopLossPips(StopLoss.Pips);
                else
                    StopLoss.Pips = Math.Round((StopLoss.Price - EntryPrice) / Symbol.PipSize, 1);
            }
        }
    }

    /// <summary>
    /// Effective ("real") stop-loss distance used in every position-size / risk / reward calculation and at order
    /// placement. With Spread Adjustment (SL) on, this widens the displayed SL by the current spread so the math
    /// reflects the SL that will actually be sent, while the panel keeps showing the base (user-entered or ATR-derived)
    /// distance. Applies in both ATR and non-ATR modes. Returns the displayed pips unchanged when SA is off or when
    /// there is no SL. Computed live from <see cref="Symbol"/>.Spread so it tracks spread changes.
    /// </summary>
    public double RealStopLossPips =>
        StopLossSpreadAdjusted && StopLoss.Pips > 0
            ? Math.Round(StopLoss.Pips + Symbol.Spread / Symbol.PipSize, 1)
            : StopLoss.Pips;

    public void UpdateStopLossFromEntryLineMoved()
    {
        //If the mode is in pips, the pips will not change, only the entry price
        if (StopLoss.Mode == TargetMode.Pips)
            UpdateStopLossPriceFromPips();
        //If the mode is in price, the price will not change, only the pips
        else
            UpdateStopLossPipsFromPrice();
    }

    public void UpdateStopLossFromAtr()
    {
        if (StopLossMultiplier == 0)
            return;

        //Only sets the base ATR distance; Spread Adjustment is applied downstream via RealStopLossPips.
        ChangeStopLossPips(GetAtrPips() * StopLossMultiplier);
    }

    public void ChangeStopLossPrice(double price)
    {
        StopLoss.Price = price;
        
        UpdateStopLossPipsFromPrice();
    }
    
    public void ChangeStopLossPips(double pips)
    {
        StopLoss.Pips = Math.Round(pips, 1);
        
        UpdateStopLossPriceFromPips();
    }

    /// <summary>
    /// Pip Values remain the same, only the price changes
    /// </summary>
    public void UpdateStopLossPriceFromPips()
    {
            StopLoss.Price = TradeType == TradeType.Buy 
                ? EntryPrice - StopLoss.Pips * Symbol.PipSize 
                : EntryPrice + StopLoss.Pips * Symbol.PipSize;
    }

    public void UpdateStopLossPipsFromPrice()
    {
        StopLoss.Pips = TradeType == TradeType.Buy 
            ? Math.Abs((EntryPrice - StopLoss.Price) / Symbol.PipSize).Round(1) 
            : Math.Abs((StopLoss.Price - EntryPrice) / Symbol.PipSize).Round(1);
    }
}