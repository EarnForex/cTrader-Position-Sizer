using System;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.Robots.RiskManagers;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    #region TradingViewEvents

    private void TradingViewOnTrailingStopValueChanged(object sender, TrailingStopValueChangedEventArgs e)
    {
        if (TrailingStop == null)
        {
            TrailingStop = new TrailingStop(this);
            _riskManagers.Add(TrailingStop);   
        }
        
        Model.TrailingStopPips = e.TrailingStop;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnBreakevenValueChanged(object sender, BreakevenValueChangedEventArgs e)
    {
        if (BreakEven == null)
        {
            BreakEven = new BreakEven(this);
            _riskManagers.Add(BreakEven);
        }
        
        Model.BreakEvenPips = e.Breakeven;
        
        BreakEven.UpdateTriggers();
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnLabelValueChanged(object sender, LabelValueChangedEventArgs e)
    {
        Model.Label = e.Label;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnExpiryValueChanged(object sender, ExpiryValueChangedEventArgs e)
    {
        Model.ExpirationSeconds = e.Expiry;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnOrderCommentValueChanged(object sender, OrderCommentValueChangedEventArgs e)
    {
        Model.Comment = e.OrderComment;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnAutoSuffixValueChanged(object sender, AutoSuffixValueChangedEventArgs e)
    {
        Model.AutoSuffix = e.AutoSuffix;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxNumberOfTradesTotalValueChanged(object sender, MaxNumberOfTradesTotalValueChangedEventArgs e)
    {
        Model.MaxNumberOfTradesTotal = e.MaxTradesTotal;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxNumberOfTradesPerSymbolValueChanged(object sender, MaxNumberOfTradesPerSymbolEventArgs e)
    {
        Model.MaxNumberOfTradesPerSymbol = e.MaxTradesPerSymbol;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxVolumeTotalValueChanged(object sender, MaxVolumeTotalValueChangedEventArgs e)
    {
        Model.MaxLotsTotal = e.MaxVolumeTotal;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxVolumePerSymbolValueChanged(object sender, MaxVolumePerSymbolValueChangedEventArgs e)
    {
        Model.MaxLotsPerSymbol = e.MaxVolumePerSymbol;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxRiskTotalValueChanged(object sender, MaxRiskTotalValueChangedEventArgs e)
    {
        Model.MaxRiskPctTotal = e.MaxRiskTotal;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxRiskPerSymbolValueChanged(object sender, MaxRiskPerSymbolValueChangedEventArgs e)
    {
        Model.MaxRiskPctPerSymbol = e.MaxRiskPerSymbol;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnDisableTradingWhenLinesHiddenCheckBoxChanged(object sender, DisableTradingWhenLinesHiddenEventArgs e)
    {
        Model.DisableTradingWhenLinesAreHidden = e.DisableTradingWhenLinesHidden;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxSlippageValueChanged(object sender, MaxSlippageValueChangedEventArgs e)
    {
        Model.MaxSlippagePips = e.MaxSlippagePips;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxSpreadValueChanged(object sender, MaxSpreadValueSpreadEventArgs e)
    {
        Model.MaxSpreadPips = e.MaxSpreadPips;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMaxEntrySlDistanceValueChanged(object sender, MaxEntrySlDistanceValueChangedEventArgs e)
    {
        Model.MaxEntryStopLossDistancePips = e.MaxEntrySlDistancePips;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnMinEntrySlDistanceValueChanged(object sender, MinEntrySlDistanceValueChangedEventArgs e)
    {
        Model.MinEntryStopLossDistancePips = e.MinEntrySlDistancePips;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnSubtractOpenPositionsVolumeCheckBoxChanged(object sender, SubtractOpenPositionsVolumeCheckBoxChangedEventArgs e)
    {
        Model.SubtractOpenPositionsVolume = e.SubtractOpenPositionsVolume;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnSubtractPendingOrdersVolumeCheckBoxChanged(object sender, SubtractPendingOrdersVolumeCheckBoxChangedEventArgs e)
    {
        Model.SubtractPendingOrdersVolume = e.SubtractPendingOrdersVolume;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnDoNotApplyStopLossCheckBoxChanged(object sender, DoNotApplyStopLossCheckBoxChangedEventArgs e)
    {
        Model.DoNotApplyStopLoss = e.DoNotApplyStopLoss;
        Model.StopLoss.Blocked = !e.DoNotApplyStopLoss;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnDoNotApplyTakeProfitCheckBoxChanged(object sender, DoNotApplyTakeProfitCheckBoxChangedEventArgs e)
    {
        Model.DoNotApplyTakeProfit = e.DoNotApplyTakeProfit;
        Model.TakeProfits.Blocked = !e.DoNotApplyTakeProfit;
                
        SetupWindowView.Update(Model);
    }

    private void TradingViewOnDoNotApplyBreakevenCheckBoxChanged(object sender, AskForConfirmationCheckBoxChangedEventArgs e)
    {
        Model.AskForConfirmation = e.AskForConfirmation;
                
        SetupWindowView.Update(Model);
    }
    
    private void TradingViewOnFillEquidistantButtonClick(object sender, EventArgs e)
    {
        Print($"Filling equidistant Button Clicked");
        
        var isFirstTpInvalid = Model.TradeType == TradeType.Buy 
            ? Model.TakeProfits.List[0].Price <= Model.EntryPrice
            : Model.TakeProfits.List[0].Price >= Model.EntryPrice;

        var mainDistancePips = isFirstTpInvalid
            ? Model.TakeProfits.LockedMultiplier * Model.StopLoss.Pips
            : Model.TakeProfits.List[0].Pips;
        var totalTps = Model.TakeProfits.List.Count;

        var switchModeLater = false;
        if (Model.IsAnyTakeProfitInvalid())
        {
            //If any of them are invalid, it must be the TP mode is in price
            SwitchTakeProfitBetweenPipsAndLevel();
            switchModeLater = true;
        }
        
        Model.ChangeTakeProfitPips(0, mainDistancePips);
        for (int i = Model.TakeProfits.List.Count - 1; i >= 1; i--)
        {
            var distance = mainDistancePips * i / totalTps;
            Model.ChangeTakeProfitPips(i, distance);
        }
        
        if (switchModeLater) 
            SwitchTakeProfitBetweenPipsAndLevel();
        
        SetupWindowView.Update(Model);
        SetupWindowView.TradingView.TpDistribution.UpdateTpRowValues(Model);
    }
    
    private void TradingViewOnPlaceBeyondMainTpButtonClick(object sender, EventArgs e)
    {
        Print($"Place Beyond Main TP Button Clicked");

        var isFirstTpInvalid = Model.TradeType == TradeType.Buy 
            ? Model.TakeProfits.List[0].Price <= Model.EntryPrice
            : Model.TakeProfits.List[0].Price >= Model.EntryPrice;

        var mainDistancePips = isFirstTpInvalid
            ? Model.TakeProfits.LockedMultiplier * Model.StopLoss.Pips
            : Model.TakeProfits.List[0].Pips;
        
        for (int i = 0; i < Model.TakeProfits.List.Count; i++)
        {
            Model.ChangeTakeProfitPips(i, mainDistancePips * (i + 1));
        }
        
        SetupWindowView.Update(Model);
        SetupWindowView.TradingView.TpDistribution.UpdateTpRowValues(Model);
    }
    
    private void TradingViewOnShareOrPercentageButtonClick(object sender, EventArgs e)
    {
        ChangeDistributionMode();
        ProcessVolumeDistribution();

        SetupWindowView.Update(Model);
        SetupWindowView.TradingView.TpDistribution.UpdateTpRowValues(Model);
    }

    private void TradingViewOnPriceChanged(object sender, TpDistributionPriceChangedEventArgs e)
    {
        Print($"Price Changed: {e.Id} - {e.Price}");
        
        Model.UpdateTakeProfitPrice(e.Id, e.Price);
        
        SetupWindowView.Update(Model);
    }
    
    private void TradingViewOnPercentageChanged(object sender, TpDistributionPercentageChangedEventArgs e)
    {
        Print($"Percentage Changed: {e.Id} - {e.Percentage}");
        
        //need to make sure that when I update TakeProfits from file, it is the same reference
        Model.TakeProfits.List[e.Id].Distribution = e.Percentage;
        
        SetupWindowView.TradingView.TpDistribution.UpdateTpRowValues(Model);
    }
    
    private void TradingViewAreaClicked(ButtonClickEventArgs obj)
    {
        Print($"Trading View Area Clicked");
    }
    
    #endregion
}