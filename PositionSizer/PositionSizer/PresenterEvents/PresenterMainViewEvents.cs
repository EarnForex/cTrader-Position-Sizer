using System;
using cAlgo.API;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private void MainViewOnTradeTypeChanged(object sender, TradeTypeChangedEventArgs e)
    {
        Model.TradeType = e.TradeType;
        //The sl-tp pips don't change, but the sl-tp price does
        var sign = Model.TradeType == TradeType.Buy ? 1 : -1;

        Model.UpdateStopLossFromTradeTypeChange();
        Model.UpdateTakeProfitsFromTradeTypeChange();

        if (Model.OrderType == OrderType.StopLimit)
        {
            var distance = Math.Abs(Model.StopLimitPrice - Model.EntryPrice);
            Model.StopLimitPrice = Model.EntryPrice + sign * distance;
        }
                
        SetupWindowView.Update(Model);
    }
    
    private void MainViewOnTargetPriceChanged(object sender, TargetPriceChangedEventArgs e)
    {
        Model.UpdateEntryPrice(e.TargetPrice, EntryPriceUpdateReason.TargetPriceChanged);
    }
    
    private void MainViewOnStopLossFieldValueChanged(object sender, StopLossPriceChangedEventArgs e)
    {
        if (Model.StopLoss.Mode == TargetMode.Price)
            Model.ChangeStopLossPrice(e.NewValue);
        else
            Model.ChangeStopLossPips(e.NewValue);

        if (InputShowAtrOptions)
        {
            //update the ATR SL multiplier
            Model.StopLossMultiplier = Model.StopLoss.Pips / Model.GetAtrPips();
            Model.StopLossSpreadAdjusted = false;
        }

        if (Model.TakeProfits.LockedOnStopLoss)
        {
            Model.UpdateTakeProfitPipsLockedOnStopLoss();
        }
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
                
        SetupWindowView.Update(Model);
    }
    
    private void MainViewOnTakeProfitPriceChanged(object sender, TakeProfitPriceChangedEventArgs e)
    {
        Print($"Setting TP to {e.TakeProfitValue:F5} from id {e.TakeProfitId}");

        if (Model.TakeProfits.LockedOnStopLoss && e.TakeProfitId == 0)
        {
            Print($"TP is Locked on SL - Updating TP Accordingly");
            Model.UpdateTakeProfitPipsLockedOnStopLoss();
        }
        else
        {
            if (Model.TakeProfits.Mode == TargetMode.Pips)
                Model.ChangeTakeProfitPips(e.TakeProfitId, e.TakeProfitValue);
            else
                Model.UpdateTakeProfitPrice(e.TakeProfitId, e.TakeProfitValue);   
        }

        if (InputShowAtrOptions)
        {
            if (e.TakeProfitId == 0)
            {
                if (e.TakeProfitValue == 0)
                {
                    Model.TakeProfitMultiplier = 0;
                    Model.TakeProfitSpreadAdjusted = false;
                }
                else
                {
                    //update the ATR TP multiplier
                    Model.TakeProfitMultiplier = Model.TakeProfits.List[0].Pips / Model.GetAtrPips();
                    Model.TakeProfitSpreadAdjusted = false;                
                }   
            }
        }
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        ValidateOrTruncateLotSizeCappedOnMargin();
                
        SetupWindowView.Update(Model);
    }

    private void MainViewOnOrderTypeChanged(object sender, OrderTypeChangedEventArgs e)
    {
        ChangeOrderTypeTo(e.OrderType);
        
        if (Model.OrderType == OrderType.Instant)
            Model.UpdateEntryPrice(Model.TradeType == TradeType.Buy ? Ask : Bid, EntryPriceUpdateReason.TickUpdate);
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnHideLinesClicked(object sender, HideLinesClickedEventArgs e)
    {
        if (e.IsHidden)
        {
            Model.HideLines = true;
            SetupWindowView.ChartLinesView.HideLines();
        }
        else
        {
            Model.HideLines = false;
            SetupWindowView.ChartLinesView.UpdateLines(Model);
            SetupWindowView.ChartLinesView.ShowLines();
        }
    }

    private void MainViewOnAccountSizeModeChanged(object sender, AccountValueTypeChangedEventArgs e)
    {
        Model.UpdateAccountSizeMode(e.AccountSizeMode, Model.CurrentPortfolio.RiskCurrency, InputRoundingPositionSizeAndPotentialReward);
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        Model.AccountSize.HasAdditionalFunds = false;
        Model.AccountSize.AdditionalFunds = 0;
        
        ValidateOrTruncateLotSizeCappedOnMargin();

        SetupWindowView.Update(Model);
    }

    private void MainViewOnAccountValueChanged(object sender, AccountValueChangedEventArgs e)
    {
        Model.UpdateAccountSizeValue(e.AccountValue, InputRoundingPositionSizeAndPotentialReward);
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        Model.AccountSize.HasAdditionalFunds = false;
        Model.AccountSize.AdditionalFunds = 0;
        Model.AccountSize.IsCustomBalance = true;
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnRiskPercentageChanged(object sender, RiskPercentageChangedEventArgs e)
    {
        Model.UpdateWithRiskPercentage(e.RiskPercentage, InputRoundingPositionSizeAndPotentialReward);
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);

        SetupWindowView.Update(Model);
    }

    private void MainViewOnRiskCashValueChanged(object sender, RiskCashValueChangedEventArgs e)
    {
        Model.UpdateWithRiskInCurrency(e.RiskCash, InputRoundingPositionSizeAndPotentialReward);

        ValidateOrTruncateLotSizeCappedOnMargin();
        
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnPositionSizeValueChanged(object sender, PositionSizeValueChangedEventArgs e)
    {
        Model.UpdateWithTradeSizeLots(InputCapPositionSizeBasedOnAvailableMargin 
            ? Math.Min(e.PositionSize, Model.MaxPositionSizeByMargin) 
            : e.PositionSize);

        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnPositionMaxSizeClicked(object sender, EventArgs e)
    {
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        Model.UpdateWithTradeSizeLots(Model.MaxPositionSizeByMargin);
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnTakeProfitLevelAdded(object sender, TakeProfitLevelAddedEventArgs e)
    {
        Model.AddNewTakeProfit(InputPrefillAdditionalTpsBasedOnMain);
        
        ProcessVolumeDistribution();
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnTakeProfitLevelRemoved(object sender, TakeProfitLevelRemovedEventArgs e)
    {
        Model.TakeProfits.List.RemoveAt(Model.TakeProfits.List.Count - 1);
        
        ProcessVolumeDistribution();
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }
    
    
    /// <summary>
    /// When locked on SL is false
    /// Nothing really happens
    /// 
    /// When Locked On SL is true
    /// - All TP's are adjusted to their respective multipliers according to the SL
    ///
    /// - Below are Not really enforced inside this method, but just good to take into account
    /// - If TP[n] is set to 0, the TP's are locked on SL, only TP[0] is read only, the others can be adjusted freely
    /// - When SL line is moved, all lines are moved as well
    /// - When TP[0] is moved, this update must be blocked (or the line could be made non-interactive)
    /// - TP[0] field must be set to read only
    /// - All other TP's can be adjusted freely
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainViewOnTpLockChanged(object sender, TpLockChangedEventArgs e)
    {
        Model.TakeProfits.LockedOnStopLoss = e.IsLocked;

        if (Model.TakeProfits.LockedOnStopLoss)
        {
            Model.UpdateTakeProfitPipsLockedOnStopLoss();
            Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
            
            ValidateOrTruncateLotSizeCappedOnMargin();
        }
        
        SetupWindowView.Update(Model);
    }

    /// <summary>
    /// If TP[0] is set to 0, the TP[0] is set to the same value as the SL
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainViewOnTakeProfitButtonClick(object sender, EventArgs e)
    {
        if (Model.TakeProfits.LockedOnStopLoss)
            return;
        
        Model.UpdateTakeProfitPipsLockedOnStopLoss(id: 0);
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        ValidateOrTruncateLotSizeCappedOnMargin();

        SetupWindowView.Update(Model);
    }

    private void MainViewOnAtrPeriodChanged(object sender, AtrPeriodChangedEventArgs e)
    {
        Model.Period = e.Period;
        var period = MarketData.GetBars(Model.AtrTimeFrame.ToTimeFrame()); 
        Model.SetAtrIndicator(Indicators.AverageTrueRange(period, e.Period, MovingAverageType.Simple));  
        
        Model.ChangeStopLossPips(Model.GetAtrPips() * Model.StopLossMultiplier);
        Model.TryAddStopLossSpreadAdjustment(Model.StopLossSpreadAdjusted);
        Model.UpdateTakeProfitFromAtr();
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnAtrStopLossMultiplierChanged(object sender, AtrStopLossMultiplierChangedEventArgs e)
    {
        Model.StopLossMultiplier = e.StopLossMultiplier;
        
        Model.UpdateStopLossFromAtr();
        
        if (Model.TakeProfits.LockedOnStopLoss)
            Model.UpdateTakeProfitPipsLockedOnStopLoss();
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnAtrStopLossSaChanged(object sender, AtrStopLossSaChangedEventArgs e)
    {
        Model.StopLossSpreadAdjusted = e.IsChecked;
        
        Model.UpdateStopLossSpreadAdjustment();
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnAtrTakeProfitMultiplierChanged(object sender, AtrTakeProfitMultiplierChangedEventArgs e)
    {
        Model.TakeProfitMultiplier = e.TakeProfitMultiplier;
        
        Model.UpdateTakeProfitFromAtr();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnAtrTakeProfitSaChanged(object sender, AtrTakeProfitSaChangedEventArgs e)
    {
        Model.TakeProfitSpreadAdjusted = e.IsChecked;
        
        Model.UpdateTakeProfitsFromSpreadAdjustment();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnAtrTimeFrameChanged(object sender, AtrTimeFrameChangedEventArgs e)
    {
        Model.GetNextAtr();
        
        var period = MarketData.GetBars(Model.AtrTimeFrame.ToTimeFrame());
        Model.SetAtrIndicator(Indicators.AverageTrueRange(period, Model.Period, MovingAverageType.Simple));
        
        Model.UpdateStopLossFromAtr();
        Model.UpdateTakeProfitFromAtr();
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.MainView.UpdateAtrTimeFrame(Model);
        SetupWindowView.Update(Model);
    }

    private void MainViewOnStopLimitPriceChanged(object sender, StopLimitPriceChangedEventArgs e)
    {
        Model.StopLimitPrice = e.StopLimitPrice;
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }

    private void MainViewOnStopLossDefaultClick(object sender, EventArgs e)
    {
        Model.ChangeStopLossPips(Model.StopLoss.InitialDefaultValuePips);
        Model.TryAddStopLossSpreadAdjustment(Model.StopLossSpreadAdjusted);
        
        if (Model.TakeProfits.LockedOnStopLoss)
            Model.UpdateTakeProfitPipsLockedOnStopLoss();
        
        ValidateOrTruncateLotSizeCappedOnMargin();
        
        SetupWindowView.Update(Model);
    }
    
    private void ValidateOrTruncateLotSizeCappedOnMargin()
    {
        if (!InputCapPositionSizeBasedOnAvailableMargin) 
            return;
        
        if (Model.TradeSize.Lots > Model.MaxPositionSizeByMargin)
            Model.UpdateWithTradeSizeLots(Model.MaxPositionSizeByMargin);
            
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
    }

    private void MainViewAreaClicked(ButtonClickEventArgs obj)
    {
        Print($"Main View Area Clicked");
    }
}