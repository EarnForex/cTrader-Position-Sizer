using System;
using System.Diagnostics;
using cAlgo.API;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private void EntryLineMoved(object sender, ChartLineMovedEventArgs e)
    {
        if (Math.Abs(e.Price - Model.EntryPrice) < Symbol.TickSize)
            return;
        
        Model.UpdateEntryPrice(e.Price, EntryPriceUpdateReason.EntryLineMoved);
    }

    private void TargetLineMoved(object sender, TargetLineMovedEventArgs e)
    {
        if (e.TakeProfitId == 0 && Model.TakeProfits.LockedOnStopLoss)
        {
            SetupWindowView.Update(Model);
            return;
        }
        
        Model.UpdateTakeProfitPrice(e.TakeProfitId, e.Price);

        if (Model is { IsAtrModeActive: true })
        {
            Model.TakeProfitMultiplier = Model.TakeProfits.List[e.TakeProfitId].Pips / Model.GetAtrPips();
        }

        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);

        //Then update the views
        SetupWindowView.Update(Model);
        SetupWindowView.TradingView.TpDistribution.UpdateTpRowValues(Model);
    }
    
    private void StopLossLineMoved(object sender, ChartLineMovedEventArgs e)
    {
        var tradeTypeChanged = false;
        
        if (Model.TradeType == TradeType.Buy && e.Price >= Model.EntryPrice)
        {
            Model.TradeType = TradeType.Sell;
            tradeTypeChanged = true;
        }
        else if (Model.TradeType == TradeType.Sell && e.Price <= Model.EntryPrice)
        {
            Model.TradeType = TradeType.Buy;
            tradeTypeChanged = true;
        }

        if (e.Price.Is(Model.EntryPrice, Symbol.TickSize))
            Model.ChangeStopLossPips(Model.StopLoss.Pips);
        else
            Model.ChangeStopLossPrice(e.Price);
        
        if (Model.TakeProfits.LockedOnStopLoss) 
            Model.UpdateTakeProfitPipsLockedOnStopLoss();
        
        if (tradeTypeChanged)
            Model.UpdateTakeProfitsFromTradeTypeChange();

        if (Model is { IsAtrModeActive: true })
        {
            Model.TakeProfitMultiplier = Model.TakeProfits.List[0].Pips / Model.GetAtrPips();
            Model.StopLossMultiplier = Model.StopLoss.Pips / Model.GetAtrPips();
        }
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        //Then update the views
        SetupWindowView.Update(Model);
    }

    private void StopPriceLineMoved(object sender, ChartLineMovedEventArgs e)
    {
        Model.StopLimitPrice = e.Price;
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        SetupWindowView.Update(Model);
    }

    private void EntryLineRemoved(object sender, EventArgs e)
    {
        //Draw the entry line again
        SetupWindowView.ChartLinesView.DrawEntryLine(Model);
    }

    private void TargetLineRemoved(object sender, TargetLineRemovedEventArgs e)
    {
        SetupWindowView.ChartLinesView.RedrawLine(Model, e.TakeProfitId);
    }

    private void StopLossLineRemoved(object sender, EventArgs e)
    {
        //Draw the stop line again
        SetupWindowView.ChartLinesView.DrawStopLine(Model);
    }
    
    private void StopPriceLineRemoved(object sender, EventArgs e)
    {
        if (Model.OrderType != OrderType.StopLimit)
            return;
        
        //Draw the stop line again, doesn't matter if the text is redrawn
        SetupWindowView.ChartLinesView.DrawStopPriceLinesAndText(Model);
    }
}