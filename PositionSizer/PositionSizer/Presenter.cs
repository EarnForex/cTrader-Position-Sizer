using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.RiskManagers;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private double _lastKnownMouseYPosition;
    private DateTime _oneSecondTimer;

    public void StartPresenter()
    {
        SetupWindowView.WindowActiveChanged += SetupWindowViewOnWindowActiveChanged;

        #region MainViewSubscriptions

        SetupWindowView.MainView.TradeButtonClicked += (_, _) => TrySendTrade();
        SetupWindowView.MainView.TradeTypeChanged += MainViewOnTradeTypeChanged;
        SetupWindowView.MainView.TargetPriceChanged += MainViewOnTargetPriceChanged;
        SetupWindowView.MainView.StopLossFieldValueChanged += MainViewOnStopLossFieldValueChanged;
        SetupWindowView.MainView.StopLossDefaultClick += MainViewOnStopLossDefaultClick;
        SetupWindowView.MainView.TakeProfitPriceChanged += MainViewOnTakeProfitPriceChanged;
        SetupWindowView.MainView.StopLimitPriceChanged += MainViewOnStopLimitPriceChanged;
        SetupWindowView.MainView.OrderTypeChanged += MainViewOnOrderTypeChanged;
        SetupWindowView.MainView.HideLinesClicked += MainViewOnHideLinesClicked;
        SetupWindowView.MainView.AccountSizeModeChanged += MainViewOnAccountSizeModeChanged;
        SetupWindowView.MainView.AccountValueChanged += MainViewOnAccountValueChanged;
        SetupWindowView.MainView.RiskPercentageChanged += MainViewOnRiskPercentageChanged;
        SetupWindowView.MainView.RiskCashValueChanged += MainViewOnRiskCashValueChanged;
        SetupWindowView.MainView.PositionSizeValueChanged += MainViewOnPositionSizeValueChanged;
        SetupWindowView.MainView.PositionMaxSizeClicked += MainViewOnPositionMaxSizeClicked;
        SetupWindowView.MainView.TakeProfitLevelAdded += MainViewOnTakeProfitLevelAdded;
        SetupWindowView.MainView.TakeProfitLevelRemoved += MainViewOnTakeProfitLevelRemoved;
        SetupWindowView.MainView.TakeProfitButtonClick += MainViewOnTakeProfitButtonClick;
        //atr
        SetupWindowView.MainView.AtrPeriodChanged += MainViewOnAtrPeriodChanged;
        SetupWindowView.MainView.AtrStopLossMultiplierChanged += MainViewOnAtrStopLossMultiplierChanged;
        SetupWindowView.MainView.AtrStopLossSaChanged += MainViewOnAtrStopLossSaChanged;
        SetupWindowView.MainView.AtrTakeProfitMultiplierChanged += MainViewOnAtrTakeProfitMultiplierChanged;
        SetupWindowView.MainView.AtrTakeProfitSaChanged += MainViewOnAtrTakeProfitSaChanged;
        SetupWindowView.MainView.AtrTimeFrameChanged += MainViewOnAtrTimeFrameChanged;
        SetupWindowView.MainView.Click += _ => SetupWindowView.MainView.TrySaveTextBoxesContent();;

        #endregion
        
        #region RiskViewSubscriptions

        SetupWindowView.RiskView.CountPendingOrdersCheckBoxChecked += CountPendingOrdersCheckBoxChecked;
        SetupWindowView.RiskView.CountPendingOrdersCheckBoxUnchecked += CountPendingOrdersCheckBoxUnchecked;
        SetupWindowView.RiskView.IgnoreOrdersWithoutStopLossCheckBoxChecked += IgnoreOrdersWithoutStopLossCheckBoxChecked;
        SetupWindowView.RiskView.IgnoreOrdersWithoutStopLossCheckBoxUnchecked += IgnoreOrdersWithoutStopLossCheckBoxUnchecked;
        SetupWindowView.RiskView.IgnoreOrdersWithoutTakeProfitCheckBoxChecked += IgnoreOrdersWithoutTakeProfitCheckBoxChecked;
        SetupWindowView.RiskView.IgnoreOrdersWithoutTakeProfitCheckBoxUnchecked += IgnoreOrdersWithoutTakeProfitCheckBoxUnchecked;
        SetupWindowView.RiskView.IgnoreOrdersInOtherSymbolsCheckBoxChecked += IgnoreOrdersInOtherSymbolsCheckBoxChecked;
        SetupWindowView.RiskView.IgnoreOrdersInOtherSymbolsCheckBoxUnchecked += IgnoreOrdersInOtherSymbolsCheckBoxUnchecked;

        #endregion

        #region MarginViewSubscriptions

        SetupWindowView.MarginView.LeverageDisplayChanged += MarginViewOnLeverageDisplayChanged;
        SetupWindowView.MarginView.Click += _ => SetupWindowView.MarginView.TrySaveTextBoxesContent();

        #endregion
        
        #region TradingViewSubscriptions

        SubscribeToTradingViewEvents();

        #endregion

        #region OrderSubscriptions

        Positions.Opened += PositionsOnOpened;
        Positions.Closed += PositionsOnClosed;
        Positions.Modified += PositionsOnModified;
        
        PendingOrders.Created += PendingOrdersOnCreated;
        PendingOrders.Modified += PendingOrdersOnModified;
        PendingOrders.Cancelled += PendingOrdersOnDeleted;
        PendingOrders.Filled += PendingOrdersOnFilled;

        #endregion

        #region Hotkeys

        Chart.AddHotkey(TrySendTrade, InputHotkeyExecuteTrade);
        Chart.AddHotkey(() => { SetupWindowView.MainView.ChangeOrderType(); }, InputHotkeySwitchOrderType);
        Chart.AddHotkey(() => { SetupWindowView.MainView.ChangeDirection(); }, InputHotkeySwitchEntryDirection);
        Chart.AddHotkey(() => { SetupWindowView.MainView.ChangeLinesStatus(); }, InputHotkeySwitchHideShowLines);
        Chart.AddHotkey(SetStopLossWhereMouseIs, InputHotkeySetStopLoss);
        Chart.AddHotkey(SetTakeProfitWhereMouseIs, InputHotkeySetTakeProfit);
        Chart.AddHotkey(SetEntryWhereMouseIs, InputHotkeySetEntry);
        Chart.AddHotkey(() => SetupWindowView.HideOrShow(), InputMinimizeMaximizeHotkeyPanel);
        Chart.AddHotkey(SwitchStopLossBetweenPipsAndLevel, InputHotkeySwitchStopLossPipsLevel);
        Chart.AddHotkey(SwitchTakeProfitBetweenPipsAndLevel, InputHotkeySwitchTakeProfitPipsLevel);

        #endregion
        
        #region ChartLinesSubscriptions

        SetupWindowView.ChartLinesView.EntryLineMoved += EntryLineMoved;
        SetupWindowView.ChartLinesView.TargetLineMoved += TargetLineMoved;
        SetupWindowView.ChartLinesView.StopLossLineMoved += StopLossLineMoved;
        SetupWindowView.ChartLinesView.StopPriceLineMoved += StopPriceLineMoved;
        
        SetupWindowView.ChartLinesView.EntryLineRemoved += EntryLineRemoved;
        SetupWindowView.ChartLinesView.TargetLineRemoved += TargetLineRemoved;
        SetupWindowView.ChartLinesView.StopLossLineRemoved += StopLossLineRemoved;
        SetupWindowView.ChartLinesView.StopPriceLineRemoved += StopPriceLineRemoved;
        
        SetupWindowView.ChartLinesView.TradeButtonClicked += (_, _) => TrySendTrade();
        
        Chart.MouseMove += args => _lastKnownMouseYPosition = args.YValue;
        Chart.MouseDown += ChartMouseDown;

        #endregion
        
        //Set Default Values
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);

        if (!Model.HideLines) 
            SetupWindowView.ChartLinesView.DrawLines(Model);
        
        if (Model is { IsAtrModeActive: true })
            SetupWindowView.MainView.UpdateAtrValue(Model, Symbol);
        
        SetupWindowView.Update(Model);
        SetupWindowView.UpdateSpread(Model);

        #region RobotUpdates

        StopEvent += OnStopEvent;
        Symbol.Tick += SymbolOnTick;
        TimerEvent += OnTimerEvent;

        #endregion

        #region ModelUpdates

        Model.EntryPriceUpdated += ModelOnEntryPriceUpdated;

        #endregion
    }

    private void OnTimerEvent(object sender, EventArgs e)
    {
        if (_oneSecondTimer == DateTime.MinValue)
        {
            _oneSecondTimer = DateTime.Now;
            return;
        }
        
        if (Server.Time.Subtract(_oneSecondTimer).TotalSeconds < 1)
            return;
        
        _oneSecondTimer = Server.Time;
        Model.UpdateAccountSizeValue(InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView.Update(Model);
    }

    private void ChartMouseDown(ChartMouseEventArgs obj)
    {
        TrySaveAllTextBoxesContent();
    }

    private void TrySaveAllTextBoxesContent() 
    {
        switch (Model.LastKnownState.WindowActive)
        {
            case WindowActive.Main:
                SetupWindowView.MainView.TrySaveTextBoxesContent();
                break;
            case WindowActive.Risk:
                break;
            case WindowActive.Margin:
                SetupWindowView.MarginView.TrySaveTextBoxesContent();
                break;
            case WindowActive.Swaps:
                break;
            case WindowActive.Trading:
                SetupWindowView.TradingView.TrySaveTextBoxesContent();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ModelOnEntryPriceUpdated(object sender, EntryPriceUpdatedEventArgs e)
    {
        //Reason: Entry Line Moved
        //Reason: Set Entry Where Mouse Is
        //Reason: Target Price Changed
        //Reason: Tick Update

        if (e.Reason == EntryPriceUpdateReason.EntryLineMoved)
        {
            if (Model.TradeType == TradeType.Buy && 
                e.EntryPrice < Model.StopLoss.Price &&
                Model.EntryPrice < Bid)
            {
                //This is now a sell stop order
                Model.TradeType = TradeType.Sell;
                Model.UpdateStopLossPriceFromPips();
                Model.UpdateTakeProfitPriceFromPips();
            }
            else if (Model.TradeType == TradeType.Buy && 
                     e.EntryPrice < Model.StopLoss.Price 
                     && Model.EntryPrice > Ask)
            {
                //This is now a sell limit order
                Model.TradeType = TradeType.Sell;
                Model.UpdateStopLossPriceFromPips();
                Model.UpdateTakeProfitPriceFromPips();
            }
            else if (Model.TradeType == TradeType.Sell && 
                     e.EntryPrice > Model.StopLoss.Price &&
                     Model.EntryPrice > Ask)
            {
                //This is now a buy stop order
                Model.TradeType = TradeType.Buy;
                Model.UpdateStopLossPriceFromPips();
                Model.UpdateTakeProfitPriceFromPips();
            }
            else if (Model.TradeType == TradeType.Sell && 
                     e.EntryPrice > Model.StopLoss.Price && 
                     Model.EntryPrice < Bid)
            {
                //This is now a buy limit order
                Model.TradeType = TradeType.Buy;
                Model.UpdateStopLossPriceFromPips();
                Model.UpdateTakeProfitPriceFromPips();
            }
            else
            {
                Model.UpdateStopLossFromEntryPriceChange();
                Model.UpdateTakeProfitsFromEntryPriceChanged();
            }
        }
        else
        {
            if (Model.TradeType == TradeType.Buy && Model.EntryPrice <= Model.StopLoss.Price)
            {
                Model.TradeType = TradeType.Sell;
            }
            else if (Model.TradeType == TradeType.Sell && Model.EntryPrice >= Model.StopLoss.Price)
            {
                Model.TradeType = TradeType.Buy;
            }
            
            Model.UpdateStopLossFromEntryPriceChange();
            Model.UpdateTakeProfitsFromEntryPriceChanged();
        }
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
        
        SetupWindowView.Update(Model);
    }

    private void SubscribeToTradingViewEvents()
    {
        SetupWindowView.TradingView.TradeButtonClicked += (_, _) => TrySendTrade();
        SetupWindowView.TradingView.TrailingStopValueChanged += TradingViewOnTrailingStopValueChanged;
        SetupWindowView.TradingView.BreakevenValueChanged += TradingViewOnBreakevenValueChanged;
        SetupWindowView.TradingView.LabelValueChanged += TradingViewOnLabelValueChanged;
        SetupWindowView.TradingView.ExpiryValueChanged += TradingViewOnExpiryValueChanged;
        SetupWindowView.TradingView.OrderCommentValueChanged += TradingViewOnOrderCommentValueChanged;
        SetupWindowView.TradingView.AutoSuffixValueChanged += TradingViewOnAutoSuffixValueChanged;
        SetupWindowView.TradingView.MaxNumberOfTradesTotalValueChanged += TradingViewOnMaxNumberOfTradesTotalValueChanged;
        SetupWindowView.TradingView.MaxNumberOfTradesPerSymbolValueChanged +=
            TradingViewOnMaxNumberOfTradesPerSymbolValueChanged;
        SetupWindowView.TradingView.MaxVolumeTotalValueChanged += TradingViewOnMaxVolumeTotalValueChanged;
        SetupWindowView.TradingView.MaxVolumePerSymbolValueChanged += TradingViewOnMaxVolumePerSymbolValueChanged;
        SetupWindowView.TradingView.MaxRiskTotalValueChanged += TradingViewOnMaxRiskTotalValueChanged;
        SetupWindowView.TradingView.MaxRiskPerSymbolValueChanged += TradingViewOnMaxRiskPerSymbolValueChanged;
        SetupWindowView.TradingView.DisableTradingWhenLinesHiddenCheckBoxChanged +=
            TradingViewOnDisableTradingWhenLinesHiddenCheckBoxChanged;
        SetupWindowView.TradingView.MaxSlippageValueChanged += TradingViewOnMaxSlippageValueChanged;
        SetupWindowView.TradingView.MaxSpreadValueChanged += TradingViewOnMaxSpreadValueChanged;
        SetupWindowView.TradingView.MaxEntrySlDistanceValueChanged += TradingViewOnMaxEntrySlDistanceValueChanged;
        SetupWindowView.TradingView.MinEntrySlDistanceValueChanged += TradingViewOnMinEntrySlDistanceValueChanged;
        SetupWindowView.TradingView.SubtractOpenPositionsVolumeCheckBoxChanged +=
            TradingViewOnSubtractOpenPositionsVolumeCheckBoxChanged;
        SetupWindowView.TradingView.SubtractPendingOrdersVolumeCheckBoxChanged +=
            TradingViewOnSubtractPendingOrdersVolumeCheckBoxChanged;
        SetupWindowView.TradingView.DoNotApplyStopLossCheckBoxChanged += TradingViewOnDoNotApplyStopLossCheckBoxChanged;
        SetupWindowView.TradingView.DoNotApplyTakeProfitCheckBoxChanged += TradingViewOnDoNotApplyTakeProfitCheckBoxChanged;
        SetupWindowView.TradingView.AskForConfirmationCheckBoxChanged += TradingViewOnDoNotApplyBreakevenCheckBoxChanged;
        SetupWindowView.TradingView.TpDistribution.FillEquidistantButtonClick += TradingViewOnFillEquidistantButtonClick;
        SetupWindowView.TradingView.TpDistribution.PlaceBeyondMainTpButtonClick += TradingViewOnPlaceBeyondMainTpButtonClick;
        SetupWindowView.TradingView.TpDistribution.ShareOrPercentageButtonClick += TradingViewOnShareOrPercentageButtonClick;
        SetupWindowView.TradingView.TpDistribution.PriceChanged += TradingViewOnPriceChanged;
        SetupWindowView.TradingView.TpDistribution.PercentageChanged += TradingViewOnPercentageChanged;
        SetupWindowView.TradingView.Click += _ => SetupWindowView.TradingView.TrySaveTextBoxesContent();
    }

    private void ProcessVolumeDistribution()
    {
        List<int> volumes;

        switch (Model.TakeProfits.SizeDistributionMode)
        {
            case SizeDistributionMode.Ascending:
                
                volumes = DistributeVolumes(Model.TakeProfits.List.Count);
                volumes.Reverse();

                for (int i = 0; i < Model.TakeProfits.List.Count; i++)
                {
                    Model.TakeProfits.List[i].Distribution = volumes[i];
                }

                break;
            case SizeDistributionMode.Descending:
                
                volumes = DistributeVolumes(Model.TakeProfits.List.Count);

                for (int i = 0; i < Model.TakeProfits.List.Count; i++)
                {
                    Model.TakeProfits.List[i].Distribution = volumes[i];
                }

                break;
            case SizeDistributionMode.EquallyDistributed:
                
                for (int i = 0; i < Model.TakeProfits.List.Count; i++)
                {
                    Model.TakeProfits.List[i].Distribution =
                        (int)(100.0 / Model.TakeProfits.List.Count);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ChangeDistributionMode()
    {
        if (Model.TakeProfits.SizeDistributionMode == SizeDistributionMode.Ascending)
            Model.TakeProfits.SizeDistributionMode = SizeDistributionMode.Descending;
        else if (Model.TakeProfits.SizeDistributionMode == SizeDistributionMode.Descending)
            Model.TakeProfits.SizeDistributionMode = SizeDistributionMode.EquallyDistributed;
        else if (Model.TakeProfits.SizeDistributionMode == SizeDistributionMode.EquallyDistributed)
            Model.TakeProfits.SizeDistributionMode = SizeDistributionMode.Ascending;
    }

    private List<int> DistributeVolumes(int numTrades)
    {
        // Predefined percentages
        int[] fixedPercentages = { 50, 25, 13, 6, 3, 2, 1 };
        int remainingPercentage = 100;
        List<int> distribution = new List<int>();

        // Assign fixed percentages
        for (int i = 0; i < Math.Min(numTrades, fixedPercentages.Length); i++)
        {
            distribution.Add(fixedPercentages[i]);
            remainingPercentage -= fixedPercentages[i];
        }

        // Adjust remaining percentages to ensure the total sums to 100
        if (numTrades > fixedPercentages.Length)
        {
            for (int i = distribution.Count; i < numTrades; i++)
            {
                distribution.Add(0);
            }
        }
        else
        {
            int lastIndex = distribution.Count - 1;
            distribution[lastIndex] += remainingPercentage; // Adjust the last percentage
        }

        return distribution;
    }

    private void SymbolOnTick(SymbolTickEventArgs obj)
    {
        SetupWindowView.UpdateSpread(Model);
        
        if (SetupWindowView.ViewUsed == SetupWindowView.RiskView)
        {
            Model.UpdateReadOnlyValues();
            SetupWindowView.RiskView.Update(Model);
        }

        if (Model is { IsAtrModeActive: true })
            SetupWindowView.MainView.UpdateAtrValue(Model, Symbol);

        if (Model.OrderType != OrderType.Instant)
            return;
        
        Model.UpdateEntryPrice(Model.TradeType == TradeType.Buy ? obj.Ask : obj.Bid, EntryPriceUpdateReason.TickUpdate);
    }

    private void OnStopEvent(object sender, EventArgs e)
    {
        SetupWindowView.ChartLinesView.RemoveLines();
    }
    
    private void SetupWindowViewOnWindowActiveChanged(object sender, WindowActiveChangedEventArgs e)
    {
        Model.LastKnownState.WindowActive = e.WindowActive;
        
        if (e.WindowActive == WindowActive.Trading)
        {
            SubscribeToTradingViewEvents();
            //SetupWindowView.TradingView.Refresh(PositionSizerModel.Model);
            SetupWindowView.TradingView.UpdateValues(Model);
        }
        else if (e.WindowActive == WindowActive.Risk)
        {
            Model.UpdateReadOnlyValues();
            SetupWindowView.RiskView.Update(Model);
        }
    }

    private void ChangeOrderTypeTo(OrderType orderType)
    {
        Model.OrderType = orderType;
        
        if (Model.OrderType == OrderType.StopLimit)
        {
            if (Model.StopLimitPrice == 0) 
                SetDefaultStopLimitPrice();

            SetupWindowView.ChartLinesView.DrawStopPriceLinesAndText(Model);
        }
        else
        {
            SetupWindowView.ChartLinesView.RemoveStopPriceLinesAndText();
        }
    }
}