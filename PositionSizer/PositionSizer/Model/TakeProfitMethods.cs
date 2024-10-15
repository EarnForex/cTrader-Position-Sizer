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
        else if (TakeProfits.Mode == TargetMode.Pips || TakeProfits.LockedOnStopLoss)
        {
            UpdateTakeProfitPriceFromPips();
        }
        else
        {
            foreach (var takeProfit in TakeProfits.List)
            {
                takeProfit.Pips = takeProfit.Price == 0
                    ? 0
                    : Math.Abs((takeProfit.Price - EntryPrice) / Symbol.PipSize).Round(1);
            }
        }
    }
    
    public void UpdateTakeProfitsFromTradeTypeChange()
    {
        UpdateTakeProfitPriceFromPips();
    }

    public void UpdateTakeProfitsFromSpreadAdjustment()
    {
        if (TakeProfitSpreadAdjusted)
            TakeProfits.List[0].Pips -= Symbol.Spread / Symbol.PipSize;
        else
            TakeProfits.List[0].Pips += Symbol.Spread / Symbol.PipSize;

        if (TakeProfits.List[0].Pips <= 0)
        {
            TakeProfits.List[0].Pips = 0;
            TakeProfits.List[0].Price = 0.0;
            return;
        }
        
        TakeProfits.List[0].Price = TradeType == TradeType.Buy ? EntryPrice + TakeProfits.List[0].Pips * Symbol.PipSize : EntryPrice - TakeProfits.List[0].Pips * Symbol.PipSize;
    }
    
    public void ChangeTakeProfitPips(int id, double pips)
    {
        if (pips == 0)
        {
            TakeProfits.List[id].Pips = 0.0;
            TakeProfits.List[id].Price = 0.0000;
            return;
        }
        
        TakeProfits.List[id].Pips = pips + TakeProfits.CommissionPipsExtra;
        TakeProfits.List[id].Price = TradeType == TradeType.Buy ? EntryPrice + pips * Symbol.PipSize : EntryPrice - pips * Symbol.PipSize;
    }

    public void UpdateTakeProfitFromAtr()
    {
        if (TakeProfitMultiplier == 0)
            return;

        if (TakeProfits.LockedOnStopLoss)
        {
            UpdateTakeProfitPipsLockedOnStopLoss();
            
            if (TakeProfitSpreadAdjusted)
                TakeProfits.List[0].Pips -= Symbol.Spread / Symbol.PipSize;
            
            TakeProfits.List[0].Pips = Math.Abs(Math.Round(TakeProfits.List[0].Pips, 1));
            
            TakeProfits.List[0].Price = TradeType == TradeType.Buy
                ? EntryPrice + TakeProfits.List[0].Pips * Symbol.PipSize
                : EntryPrice - TakeProfits.List[0].Pips * Symbol.PipSize;
        }
        else
        {
            TakeProfits.List[0].Pips = GetAtrPips() * TakeProfitMultiplier + TakeProfits.CommissionPipsExtra;
        
            if (TakeProfitSpreadAdjusted)
                TakeProfits.List[0].Pips -= Symbol.Spread / Symbol.PipSize;
        
            TakeProfits.List[0].Pips = Math.Abs(Math.Round(TakeProfits.List[0].Pips, 1));
        
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
        
        var pipsExtra = TakeProfits.CommissionPipsExtra * Symbol.PipSize;
        
        TakeProfits.List[id].Price = TradeType == TradeType.Buy ? price + pipsExtra : price - pipsExtra;
        TakeProfits.List[id].Pips = Math.Abs((EntryPrice - price) / Symbol.PipSize).Round(1);
    }

    public void AddNewTakeProfit(bool prefillAdditionalTpsBasedOnMain)
    //todo adjust here for when TP is locked for the new TP
    {
        var takeProfit = new TakeProfit();
        var pips = TakeProfits.List[0].Pips * (TakeProfits.List.Count + 1) + TakeProfits.CommissionPipsExtra;
        
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
        {
            var tp = TakeProfits.List[i];
            ChangeTakeProfitPips(i, StopLoss.Pips * TakeProfits.LockedMultiplier * (i + 1));
        }
    }
    
    public void UpdateTakeProfitPipsLockedOnStopLoss(int id)
    {
        ChangeTakeProfitPips(id, StopLoss.Pips * TakeProfits.LockedMultiplier * (id + 1));
    }
    
    public bool IsAnyTakeProfitInvalid()
    {
        return TradeType == TradeType.Buy 
            ? TakeProfits.List.Any(takeProfit => takeProfit.Price <= EntryPrice) 
            : TakeProfits.List.Any(takeProfit => takeProfit.Price >= EntryPrice);
    }
}