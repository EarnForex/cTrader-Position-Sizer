// -------------------------------------------------------------------------------
//   Calculates risk-based position size for your account.
//   Allows trade execution based the calculation results.
//   WARNING: No warranty. This EA is offered "as is". Use at your own risk.
//   Note: Pressing Shift+T will open a trade.
//   
//   Version 1.20
//   Copyright 2024-2025, EarnForex.com
//   https://www.earnforex.com/ctrader-robots/cTrader-Position-Sizer/
// -------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using cAlgo.API;
using cAlgo.Robots.RiskManagers;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.None)]
public partial class PositionSizer : Robot,
    ISetupWindowResources,
    IModelResources,
    IBreakEvenResources,
    ITrailingStopResources
    //IKeyMultiplierFeature
{
    public SetupWindowView SetupWindowView { get; private set; }
    public event EventHandler TimerEvent;
    public event EventHandler StopEvent;
    public IModel Model { get; set; }
    public const string Version = "v1.20";
    public CustomStyle CustomStyle { get; private set; }
    public BreakEven BreakEven { get; private set; }
    public TrailingStop TrailingStop { get; private set; }
    public int IndexForLabelReference { get; set; }
    private readonly List<IRiskManager> _riskManagers = new();
    private List<string> _changedProperties;
    private string _fileTag;
    public string CleanBrokerName {get; private set;}
    // ReSharper disable once GrammarMistakeInComment
    //public KeyMultiplierFeature KeyMultiplierFeature { get; private set; }

    /// <summary>
    /// Used for KeepPanelAsIs
    /// Need a way to know if the symbol has changed
    /// </summary>
    private bool _symbolHasChanged;

    public const int ControlHeight = 26;

    protected override void OnStart()
    {
        if (Symbol.LotSize == 0)
        {
            var msgText = "This symbol reports a lot size of zero, which prevents the bot from operating correctly. Please select a symbol with a non-zero lot size or update the symbol's contract size and restart the bot.";
            MessageBox.Show(msgText, "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            Stop();
        }
        
        //System.Diagnostics.Debugger.Launch();

        // IndexForLabelReference = RunningMode == RunningMode.VisualBacktesting
        //     ? Bars.Count - 1 - 10
        //     : Chart.LastVisibleBarIndex - 10;
        Initialize();
    }

    protected override void OnBar()
    {
        base.OnBar();
    }

    protected override void OnTick()
    {
        _riskManagers.ForEach(rm => rm.Check());
    }

    protected override void OnTimer()
    {
        OnTimerEvent();
    }

    protected override void OnStop()
    {
        OnStopEvent();
    }

    protected override void OnException(Exception exception)
    {
        Print("Message ", exception.Message);
        Print("Stack Trace", exception.StackTrace);
        
        var sbException = new StringBuilder();
        sbException.AppendLine($"Message: {exception.Message}");
        sbException.AppendLine($"StackTrace: {exception.StackTrace}");
        
        Chart.DrawStaticText("Error",
            sbException.ToString(),
            VerticalAlignment.Top,
            HorizontalAlignment.Right,
            Color.Red);

        if (exception.InnerException != null)
        {
            var sbInnerException = new StringBuilder();
            sbInnerException.AppendLine($"Message: {exception.InnerException.Message}");
            sbInnerException.AppendLine($"StackTrace: {exception.InnerException.StackTrace}");
            
            Chart.DrawStaticText("Inner Error", sbInnerException.ToString(), VerticalAlignment.Bottom, HorizontalAlignment.Right, Color.OrangeRed);
        }
    }

    protected virtual void OnTimerEvent()
    {
        TimerEvent?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnStopEvent()
    {
        StopEvent?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<Position> PositionsByLabel
    {
        get
        {
            return Model.Label == string.Empty 
                ? Positions 
                : Positions.Where(p => p.Label == Model.Label);
        }
    }

    public IEnumerable<PendingOrder> PendingOrdersByLabel
    {
        get
        {
            return Model.Label == string.Empty 
                ? PendingOrders
                : PendingOrders.Where(p => p.Label == Model.Label);
        }
    }

    private void Initialize()
    {
        SetBrokerName();
        SetFileTag();
        ProcessOldAndNewParametersIfNeeded();
        SetIndexForLabels();
        SetStyles();
        SetLogger();
        //SetKeyMultiplier();
        
        Model = LoggingProxy<IModel>.Create(new Model(this));
        
        var storageModel = LocalStorage.GetObject<Model>($"{CleanBrokerName}-model{_fileTag}", LocalStorageScope.Instance);
        
        Model.Version = Version;
        Model.SymbolName = SymbolName.Replace(" ", "");
        
        var canImplementModel = CanImplementModelOrPropertyChange(storageModel);
        
        if (canImplementModel)
            storageModel.SetResources(this);
        
        SetTradeType(canImplementModel, storageModel);
        
        Model.OrderType = canImplementModel && !_changedProperties.Contains("InputOrderType")
            ? storageModel.OrderType
            : InputOrderType;
        
        SetEntryPrice(canImplementModel, storageModel);
        SetAccountSize(canImplementModel, storageModel);
        
        Model.HideLines = canImplementModel && !_changedProperties.Contains("InputShowLinesByDefault")
            ? storageModel.HideLines
            : !InputShowLinesByDefault;
        
        SetLastKnownState(canImplementModel, storageModel);
        SetTakeProfits(canImplementModel, storageModel);
        SetStopLoss(canImplementModel, storageModel);
        SetStopLimitPrice(canImplementModel, storageModel);
        SetTradeSize(canImplementModel, storageModel);
        SetBreakevenManager(canImplementModel, storageModel);
        SetTrailingStopManager(canImplementModel, storageModel);
        
        Model.Label = canImplementModel && !_changedProperties.Contains("InputLabel")
            ? storageModel.Label
            : InputLabel;
        
        Model.Comment = canImplementModel && !_changedProperties.Contains("InputCommentary")
            ? storageModel.Comment
            : InputCommentary;
        
        Model.AutoSuffix = canImplementModel && !_changedProperties.Contains("InputAutoSuffix")
            ? storageModel.AutoSuffix
            : InputAutoSuffix;
        
        Model.MaxSlippagePips = canImplementModel && !_changedProperties.Contains("InputMaxSlippagePips")
            ? storageModel.MaxSlippagePips
            : InputMaxSlippagePips;
        
        Model.MaxSpreadPips = canImplementModel && !_changedProperties.Contains("InputMaxSpreadPips")
            ? storageModel.MaxSpreadPips
            : InputMaxSpreadPips;
        
        Model.MaxEntryStopLossDistancePips =
            canImplementModel && !_changedProperties.Contains("InputMaxEntryStopLossDistancePips")
                ? storageModel.MaxEntryStopLossDistancePips
                : InputMaxEntryStopLossDistancePips;
        
        Model.MinEntryStopLossDistancePips =
            canImplementModel && !_changedProperties.Contains("InputMinEntryStopLossDistancePips")
                ? storageModel.MinEntryStopLossDistancePips
                : InputMinEntryStopLossDistancePips;
        
        Model.MaxRiskPercentage =
            canImplementModel && !_changedProperties.Contains("InputMaxRiskPercentage")
                ? storageModel.MaxRiskPercentage
                : InputMaxRiskPercentage;
        
        Model.MaxLotsTotal = canImplementModel && !_changedProperties.Contains("InputMaxPositionSizeTotalForTradingTab")
            ? storageModel.MaxLotsTotal
            : Symbol.QuantityToVolumeInUnits(InputMaxPositionSizeTotalForTradingTab);
        
        Model.MaxLotsPerSymbol =
            canImplementModel && !_changedProperties.Contains("InputMaxPositionSizePerSymbolForTradingTab")
                ? storageModel.MaxLotsPerSymbol
                : Symbol.QuantityToVolumeInUnits(InputMaxPositionSizePerSymbolForTradingTab);
        
        Model.SubtractOpenPositionsVolume = canImplementModel && !_changedProperties.Contains("InputSubtractOpv")
            ? storageModel.SubtractOpenPositionsVolume
            : InputSubtractOpv;
        
        Model.SubtractPendingOrdersVolume = canImplementModel && !_changedProperties.Contains("InputSubtractPov")
            ? storageModel.SubtractPendingOrdersVolume
            : InputSubtractPov;
        
        Model.DoNotApplyStopLoss = canImplementModel && !_changedProperties.Contains("InputDoNotApplyStopLoss")
            ? storageModel.DoNotApplyStopLoss
            : InputDoNotApplyStopLoss;
        
        Model.DoNotApplyTakeProfit = canImplementModel && !_changedProperties.Contains("InputDoNotApplyTakeProfit")
            ? storageModel.DoNotApplyTakeProfit
            : InputDoNotApplyTakeProfit;
        
        Model.AskForConfirmation = canImplementModel && !_changedProperties.Contains("InputAskForConfirmation")
            ? storageModel.AskForConfirmation
            : InputAskForConfirmation;
        
        Model.TrailingStopPips = canImplementModel && !_changedProperties.Contains("InputTrailingStopPips")
            ? storageModel.TrailingStopPips
            : InputTrailingStopPips;
        
        Model.BreakEvenPips = canImplementModel && !_changedProperties.Contains("InputBreakevenPips")
            ? storageModel.BreakEvenPips
            : InputBreakevenPips;
        
        Model.ExpirationSeconds = canImplementModel && !_changedProperties.Contains("InputExpirySeconds")
            ? storageModel.ExpirationSeconds
            : InputExpirySeconds;
        
        Model.MaxNumberOfTradesTotal = canImplementModel && !_changedProperties.Contains("InputMaxNumberOfTradesTotal")
            ? storageModel.MaxNumberOfTradesTotal
            : InputMaxNumberOfTradesTotal;
        
        Model.MaxNumberOfTradesPerSymbol =
            canImplementModel && !_changedProperties.Contains("InputMaxNumberOfTradesPerSymbol")
                ? storageModel.MaxNumberOfTradesPerSymbol
                : InputMaxNumberOfTradesPerSymbol;
        
        Model.MaxRiskPctTotal = canImplementModel && !_changedProperties.Contains("InputMaxRiskTotal")
            ? storageModel.MaxRiskPctTotal
            : InputMaxRiskTotal;
        
        Model.MaxRiskPctPerSymbol = canImplementModel && !_changedProperties.Contains("InputMaxRiskPerSymbol")
            ? storageModel.MaxRiskPctPerSymbol
            : InputMaxRiskPerSymbol;
        
        Model.DisableTradingWhenLinesAreHidden =
            canImplementModel && !_changedProperties.Contains("InputDisableTradingWhenLinesAreHidden")
                ? storageModel.DisableTradingWhenLinesAreHidden
                : InputDisableTradingWhenLinesAreHidden;
        
        //For Risk Model
        
        Model.IncludeOrdersMode = canImplementModel && !_changedProperties.Contains("InputIncludeOrdersMode")
            ? storageModel.IncludeOrdersMode
            : InputIncludeOrdersMode;

        Model.IgnoreOrdersWithoutStopLoss =
            canImplementModel && !_changedProperties.Contains("InputIgnoreOrdersWithoutStopLoss")
                ? storageModel.IgnoreOrdersWithoutStopLoss
                : InputIgnoreOrdersWithoutStopLoss;
        
        Model.IgnoreOrdersWithoutTakeProfit =
            canImplementModel && !_changedProperties.Contains("InputIgnoreOrdersWithoutTakeProfit")
                ? storageModel.IgnoreOrdersWithoutTakeProfit
                : InputIgnoreOrdersWithoutTakeProfit;
        
        Model.IncludeSymbolsMode = canImplementModel && !_changedProperties.Contains("InputIncludeSymbolsMode")
            ? storageModel.IncludeSymbolsMode
            : InputIncludeSymbolsMode;
        
        Model.IncludeDirectionsMode = canImplementModel && !_changedProperties.Contains("InputIncludeDirectionsMode")
            ? storageModel.IncludeDirectionsMode
            : InputIncludeDirectionsMode;
        
        SetCurrentPortfolio(canImplementModel, storageModel);
        SetPotentialPortfolio(canImplementModel, storageModel);
        
        //For Margin Model
        Model.CustomLeverage = canImplementModel && !_changedProperties.Contains("InputCustomLeverage")
            ? storageModel.CustomLeverage
            : InputCustomLeverage;

        SetAtrSettings(canImplementModel, storageModel);

        if (InputUseLastSavedSettings && RunningMode == RunningMode.RealTime)
        {
            StopEvent += (_, _) =>
            {
                Print($"Stop Event Fired...");
                LocalStorage.SetObject($"{CleanBrokerName}-model{_fileTag}", Model, LocalStorageScope.Instance);
                LocalStorage.Flush(LocalStorageScope.Instance);
            };
        }

        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView = new SetupWindowView(this);
        
        if (InputUseLastSavedSettings && RunningMode == RunningMode.RealTime)
            SetupWindowView.UpdateState(Model.LastKnownState);
        
        StartPresenter();
        Timer.Start(TimeSpan.FromMilliseconds(InputRefreshMilliseconds));
    }

    // private void SetKeyMultiplier()
    // {
    //     KeyMultiplierFeature = new KeyMultiplierFeature(this);
    // }
    
    public static string CleanName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        // Allow only Latin letters (A-Z, a-z), numbers (0-9), and spaces
        // Remove all other characters (like &, @, etc.)
        var cleaned = Regex.Replace(fileName, "[^a-zA-Z0-9 ]", "").Replace(" ", "");;
        
        // Trim leading and trailing spaces (LocalStorage requirement)
        return cleaned.Trim();
    }

    private void SetAtrSettings(bool canImplementModel, IModel storageModel)
    {
        if (canImplementModel)
        {
            Model.IsAtrModeActive = _changedProperties.Contains("InputShowAtrOptions")
                ? InputShowAtrOptions
                : storageModel.IsAtrModeActive;

            Model.Period = _changedProperties.Contains("InputAtrPeriod") || (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
                ? InputAtrPeriod
                : storageModel.Period;
            
            Model.AtrCandle = _changedProperties.Contains("InputAtrCandle") || (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
                ? InputAtrCandle
                : storageModel.AtrCandle;

            var bars = Model.AtrTimeFrame != null 
                ? MarketData.GetBars(Model.AtrTimeFrame.ToTimeFrame())
                : MarketData.GetBars(InputAtrTimeFrame);

            Model.SetAtrIndicator(Indicators.AverageTrueRange(bars, Model.Period == 0
                    ? InputAtrPeriod
                    : Model.Period,
                MovingAverageType.Simple));

            if (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
            {
                //Means the old state was not ATR, in this case we update based on the SL/TP
                Model.StopLossMultiplier = Model.StopLoss.Pips / Model.GetAtrPips();
            }
            else if (_changedProperties.Contains("InputDefaultAtrMultiplierStopLoss"))
                Model.StopLossMultiplier = InputDefaultAtrMultiplierStopLoss;
            else
                Model.StopLossMultiplier = storageModel.StopLossMultiplier;
            
            if (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
                Model.TakeProfitMultiplier = Model.TakeProfits.List[0].Pips / Model.GetAtrPips();
            else if (_changedProperties.Contains("InputDefaultAtrMultiplierTakeProfit"))
                Model.TakeProfitMultiplier = InputDefaultAtrMultiplierTakeProfit;
            else
                Model.TakeProfitMultiplier = storageModel.TakeProfitMultiplier;

            Model.AtrTimeFrame = _changedProperties.Contains("InputAtrTimeFrame") || storageModel.AtrTimeFrame == null || (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
                ? new SerializableTimeFrame().FromTimeFrame(InputAtrTimeFrame)
                : storageModel.AtrTimeFrame;

            Model.StopLossSpreadAdjusted = _changedProperties.Contains("InputSpreadAdjustmentStopLoss") || (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
                ? InputSpreadAdjustmentStopLoss
                : storageModel.StopLossSpreadAdjusted;

            Model.TakeProfitSpreadAdjusted = _changedProperties.Contains("InputSpreadAdjustmentTakeProfit") || (_changedProperties.Contains("InputShowAtrOptions") && InputShowAtrOptions)
                ? InputSpreadAdjustmentTakeProfit
                : storageModel.TakeProfitSpreadAdjusted;

            if (Model.IsAtrModeActive)
            {
                Model.UpdateStopLossFromAtr();
                Model.UpdateTakeProfitFromAtr();
            }
        }
        else
        {
            if (InputShowAtrOptions)
            {
                Model.IsAtrModeActive = true;
                Model.Period = InputAtrPeriod;
                Model.StopLossMultiplier = InputDefaultAtrMultiplierStopLoss;
                Model.TakeProfitMultiplier = InputDefaultAtrMultiplierTakeProfit;
                Model.AtrTimeFrame = new SerializableTimeFrame().FromTimeFrame(InputAtrTimeFrame);
                Model.StopLossSpreadAdjusted = InputSpreadAdjustmentStopLoss;
                Model.TakeProfitSpreadAdjusted = InputSpreadAdjustmentTakeProfit;
                Model.AtrCandle = InputAtrCandle;
                var bars = MarketData.GetBars(Model.AtrTimeFrame.ToTimeFrame());
                Model.SetAtrIndicator(Indicators.AverageTrueRange(bars, Model.Period, MovingAverageType.Simple));
                Model.ChangeStopLossPips(Model.GetAtrPips());
                Model.ChangeTakeProfitPips(0, Model.GetAtrPips());
            }
        }
    }

    private void SetTradeType(bool canImplementModel, IModel storageModel)
    {
        if (canImplementModel)
        {
            if (!_changedProperties.Contains("InputTradeType"))
            {
                Model.TradeType = storageModel.TradeType;
                return;
            }
            
            if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged)
            {
                Model.TradeType = storageModel.TradeType;
                return;
            }
        }
        
        Model.TradeType = InputTradeType;
    }

    private void SetStopLoss(bool canImplementModel, IModel storageModel)
    {
        if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged && canImplementModel)
        {
            Model.StopLoss = storageModel.StopLoss;
            Model.UpdateStopLossPriceFromPips();
            return;
        }

        if (!canImplementModel)
        {
            SetDefaultStopLoss();
            return;
        }

        Model.StopLoss = storageModel.StopLoss;

        Model.StopLoss.Mode = _changedProperties.Contains("InputStopLossDistancePipsInsteadOfLevel")
            ? InputStopLossDistancePipsInsteadOfLevel
                ? TargetMode.Pips
                : TargetMode.Price
            : Model.StopLoss.Mode;

        if (_changedProperties.Contains("InputDefaultStopLossPips") && InputDefaultStopLossPips != 0)
        {
            Model.StopLoss.HasDefaultSwitch = true;
            Model.StopLoss.InitialDefaultValuePips = InputDefaultStopLossPips;
            Model.ChangeStopLossPips(InputDefaultStopLossPips);
        }
        else
        {
            if (InputDefaultStopLossPips == 0)
                Model.StopLoss.HasDefaultSwitch = false;
        }

        if (_changedProperties != null && _changedProperties.Contains("InputTradeType"))
            Model.UpdateStopLossFromTradeTypeChange();
    }
    
    private void SetDefaultStopLoss()
    {
        Model.StopLoss = new StopLoss();
        
        double stopLossDefaultPips;

        if (InputDefaultStopLossPips == 0)
        {
            stopLossDefaultPips = Model.TradeType == TradeType.Buy 
                ? Model.EntryPrice > Bars.LowPrices.LastValue ? Math.Round(((Model.EntryPrice - Bars.LowPrices.LastValue) / Symbol.PipSize), 1) : 1
                : Model.EntryPrice < Bars.HighPrices.LastValue ? Math.Round(((Bars.HighPrices.LastValue - Model.EntryPrice) / Symbol.PipSize), 1) : 1;
            Model.StopLoss.HasDefaultSwitch = false;
        }
        else
        {
            stopLossDefaultPips = InputDefaultStopLossPips;
            Model.StopLoss.HasDefaultSwitch = true;
            Model.StopLoss.InitialDefaultValuePips = InputDefaultStopLossPips;
        }
        
        Model.ChangeStopLossPips(stopLossDefaultPips);
        Model.StopLoss.Mode = InputStopLossDistancePipsInsteadOfLevel
            ? TargetMode.Pips
            : TargetMode.Price;
        
        if (_changedProperties != null && _changedProperties.Contains("InputTradeType"))
            Model.UpdateStopLossFromTradeTypeChange();
    }


    private void SetPotentialPortfolio(bool canImplementModel, IModel storageModel)
    {
        if (!canImplementModel || (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged))
        {
            Model.PotentialPortfolio = new Portfolio();
            return;
        }

        Model.PotentialPortfolio = storageModel.PotentialPortfolio;
    }

    private void SetCurrentPortfolio(bool canImplementModel, IModel storageModel)
    {
        if (!canImplementModel || (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged))
        {
            Model.CurrentPortfolio = new Portfolio();
            return;
        }

        Model.CurrentPortfolio = storageModel.CurrentPortfolio;
    }
    
    private void SetTradeSize(bool canImplementModel, IModel storageModel)
    {
        if (canImplementModel)
        {
            SetTradeSizeWhenModelExists(storageModel);
        }
        else
        {
            SetTradeSizeDefault();
        }
    }
    
    private void SetTradeSizeWhenModelExists(IModel storageModel)
    {
        if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged)
        {
            Model.TradeSize = new TradeSize(Symbol)
            {
                LastRiskValueChanged = storageModel.TradeSize.LastRiskValueChanged
            };

            switch (storageModel.TradeSize.LastRiskValueChanged)
            {
                case LastRiskValueChanged.LotSize:
                    Model.UpdateWithTradeSizeLots(storageModel.TradeSize.Lots);
                    break;
                case LastRiskValueChanged.RiskCurrency:
                    Model.UpdateWithRiskInCurrency(storageModel.TradeSize.RiskInCurrency, InputRoundingPositionSizeAndPotentialReward);
                    break;
                case LastRiskValueChanged.RiskPercentage:
                    Model.UpdateWithRiskPercentage(storageModel.TradeSize.RiskPercentage, InputRoundingPositionSizeAndPotentialReward);
                    break;
                default:
                    Model.SetRiskDefaults(InputRoundingPositionSizeAndPotentialReward);
                    break;
            }
        }
        else
        {
            storageModel.TradeSize.SetSymbol(Symbol);
            Model.TradeSize = storageModel.TradeSize;

            if (_changedProperties.Contains("InputPositionSizeInLots"))
                Model.UpdateWithTradeSizeLots(InputPositionSizeInLots);
            else if (_changedProperties.Contains("InputMoneyRisk"))
                Model.UpdateWithRiskInCurrency(InputMoneyRisk, InputRoundingPositionSizeAndPotentialReward);
            else if (_changedProperties.Contains("InputRiskPercentage"))
                Model.UpdateWithRiskPercentage(InputRiskPercentage,
                    InputRoundingPositionSizeAndPotentialReward);
            else if (_changedProperties.Contains("InputPositionSizeInLots") &&
                     _changedProperties.Contains("InputMoneyRisk") &&
                     _changedProperties.Contains("InputRiskPercentage"))
                Model.SetRiskDefaults(InputRoundingPositionSizeAndPotentialReward);
        }
    }
    
    private void SetTradeSizeDefault()
    {
        Model.TradeSize = new TradeSize(Symbol);

        if (InputPositionSizeInLots > 0)
            Model.UpdateWithTradeSizeLots(InputPositionSizeInLots);
        else if (InputMoneyRisk > 0)
            Model.UpdateWithRiskInCurrency(InputMoneyRisk, InputRoundingPositionSizeAndPotentialReward);
        else if (InputRiskPercentage > 0)
            Model.UpdateWithRiskPercentage(InputRiskPercentage, InputRoundingPositionSizeAndPotentialReward);
        else if (InputPositionSizeInLots == 0 && InputMoneyRisk == 0 && InputRiskPercentage == 0)
            Model.SetRiskDefaults(InputRoundingPositionSizeAndPotentialReward);
    }

    private void SetTakeProfits(bool canImplementModel, IModel storageModel)
    {
        if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged && canImplementModel)
        {
            Model.TakeProfits = storageModel.TakeProfits;
            Model.UpdateTakeProfitPriceFromPips();
            return;
        }

        if (!canImplementModel)
        {
            SetDefaultTakeProfits();
            return;
        }

        Model.TakeProfits = storageModel.TakeProfits;
        
        Model.TakeProfits.LockedOnStopLoss = _changedProperties.Contains("InputTakeProfitLockedOnStopLoss")
            ? InputTakeProfitLockedOnStopLoss
            : storageModel.TakeProfits.LockedOnStopLoss;
        
        Model.TakeProfits.LockedMultiplier = _changedProperties.Contains("InputTakeProfitMultiplierForStopLossValue")
            ? InputTakeProfitMultiplierForStopLossValue
            : storageModel.TakeProfits.LockedMultiplier;
        
        Model.TakeProfits.Mode = _changedProperties.Contains("InputTakeProfitDistancePipsInsteadOfLevel")
            ? InputTakeProfitDistancePipsInsteadOfLevel
                ? TargetMode.Pips
                : TargetMode.Price
            : storageModel.TakeProfits.Mode;
        
        Model.TakeProfits.Blocked = _changedProperties.Contains("InputDoNotApplyTakeProfit")
            ? !InputDoNotApplyTakeProfit
            : storageModel.TakeProfits.Blocked;
        
        Model.TakeProfits.CommissionPipsExtra = _changedProperties.Contains("InputUseCommissionToSetTpDistance")
            ? InputUseCommissionToSetTpDistance
                ? Symbol.Commission / 1_000_000
                : 0
            : storageModel.TakeProfits.CommissionPipsExtra;
        
        if (_changedProperties.Contains("InputDefaultTakeProfitPips"))
            Model.ChangeTakeProfitPips(0, InputDefaultTakeProfitPips);
        
        if (!_changedProperties.Contains("InputTakeProfitsNumber"))
            return;
        
        if (InputTakeProfitsNumber == 1)
        {
            if (Model.TakeProfits.List.Count > 1)
                //delete all but the first one
                Model.TakeProfits.List.RemoveRange(1, Model.TakeProfits.List.Count - 1);
        }
        else
        {
            AdjustTakeProfitsNumber();
        }
    }

    private void SetDefaultTakeProfits()
    {
        Model.TakeProfits = new TakeProfits
        {
            Decimals = Symbol.Digits,
            LockedOnStopLoss = InputTakeProfitLockedOnStopLoss,
            LockedMultiplier = InputTakeProfitMultiplierForStopLossValue,
            Mode = InputTakeProfitDistancePipsInsteadOfLevel
                ? TargetMode.Pips
                : TargetMode.Price,
            Blocked = !InputDoNotApplyTakeProfit,
            CommissionPipsExtra = InputUseCommissionToSetTpDistance
                ? Symbol.Commission / 1_000_000
                : 0,
        };
        
        for (int i = 0; i < InputTakeProfitsNumber; i++)
        {
            Model.TakeProfits.List.Add(new TakeProfit
            {
                Pips = InputDefaultTakeProfitPips == 0 ? 0 : InputDefaultTakeProfitPips * (i + 1) + Model.TakeProfits.CommissionPipsExtra,
                Price = Model.TradeType == TradeType.Buy
                    ? InputDefaultTakeProfitPips == 0 ? 0 : Model.EntryPrice + InputDefaultTakeProfitPips * (i + 1) * Symbol.PipSize
                    : InputDefaultTakeProfitPips == 0 ? 0 : Model.EntryPrice - InputDefaultTakeProfitPips * (i + 1) * Symbol.PipSize,
                Distribution = (int)(100.0 / InputTakeProfitsNumber)
            });
        }
    }

    private void AdjustTakeProfitsNumber()
    {
        if (Model.TakeProfits.List.Count < InputTakeProfitsNumber)
        {
            for (int i = Model.TakeProfits.List.Count; i < InputTakeProfitsNumber; i++)
            {
                Model.TakeProfits.List.Add(new TakeProfit
                {
                    Pips = InputDefaultTakeProfitPips == 0 ? 0 : InputDefaultTakeProfitPips * (i + 1) + Model.TakeProfits.CommissionPipsExtra,
                    Price = InputDefaultTakeProfitPips == 0 ? 0 : InputTradeType == TradeType.Buy
                        ? Model.EntryPrice + InputDefaultTakeProfitPips * (i + 1) * Symbol.PipSize
                        : Model.EntryPrice - InputDefaultTakeProfitPips * (i + 1) * Symbol.PipSize,
                    Distribution = (int)(100.0 / InputTakeProfitsNumber)
                });
            }
        }
        else if (Model.TakeProfits.List.Count > InputTakeProfitsNumber)
        {
            Model.TakeProfits.List.RemoveRange(InputTakeProfitsNumber,
                Model.TakeProfits.List.Count - InputTakeProfitsNumber);
        }
    }

    private void SetLastKnownState(bool canImplementModel, IModel storageModel)
    {
        if (canImplementModel && !_changedProperties.Contains("InputPanelPositionX") &&
            !_changedProperties.Contains("InputPanelPositionY"))
        {
            Model.LastKnownState = storageModel.LastKnownState;
        }
        else
        {
            if (canImplementModel)
            {
                Model.LastKnownState = storageModel.LastKnownState;
                
                if (_changedProperties.Contains("InputPanelPositionX"))
                    Model.LastKnownState.X = InputPanelPositionX;
                
                if (_changedProperties.Contains("InputPanelPositionY"))
                    Model.LastKnownState.Y = InputPanelPositionY;
            }
            else
            {
                Model.LastKnownState = new LastKnownState
                {
                    X = InputPanelPositionX,
                    Y = InputPanelPositionY,
                    WindowActive = WindowActive.Main
                };
            }
        }
    }

    private void SetAccountSize(bool canImplementModel, IModel storageModel)
    {
        if (!canImplementModel)
        {
            SetDefaultAccountSize();
            return;
        }

        if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged)
        {
            SetDefaultAccountSize();
            return;
        }

        Model.AccountSize = storageModel.AccountSize;
        
        if (_changedProperties.Contains("InputAccountSizeMode"))
            Model.AccountSize.Mode = InputAccountSizeMode;
        
        Model.AccountSize.IsCustomBalance = InputCustomBalance != 0;
        
        if (_changedProperties.Contains("InputAdditionalFunds") && !Model.AccountSize.IsCustomBalance)
        {
            Model.AccountSize.AdditionalFunds = InputAdditionalFunds;
            Model.AccountSize.HasAdditionalFunds = InputAdditionalFunds != 0;
        }

        SetAccountSizeValue();
    }

    private void SetAccountSizeValue()
    {
        Model.AccountSize.Value = Model.AccountSize.Mode switch
        {
            AccountSizeMode.Equity => Account.Equity + (Model.AccountSize.HasAdditionalFunds
                                          ? InputAdditionalFunds
                                          : 0),
            AccountSizeMode.Balance => Model.AccountSize.IsCustomBalance
                ? InputCustomBalance
                : Account.Balance + (Model.AccountSize.HasAdditionalFunds 
                    ? InputAdditionalFunds 
                    : 0),
            AccountSizeMode.BalanceCpr => Model.AccountSize.IsCustomBalance
                ? InputCustomBalance
                : Account.Balance + (Model.AccountSize.HasAdditionalFunds
                      ? InputAdditionalFunds
                      : 0),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void SetDefaultAccountSize()
    {
        Model.AccountSize = new AccountSize
        {
            Mode = InputAccountSizeMode,
            IsCustomBalance = InputCustomBalance != 0,
            HasAdditionalFunds = InputAdditionalFunds != 0
        };
        
        SetAccountSizeValue();
    }

    private void SetStopLimitPrice(bool canImplementModel, IModel storageModel)
    {
        if (!canImplementModel)
        {
            SetDefaultStopLimitPrice();
            return;
        }

        if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged)
        {
            SetDefaultStopLimitPrice();
            return;
        }

        Model.StopLimitPrice = storageModel.StopLimitPrice;
    }

    private void SetDefaultStopLimitPrice()
    {
        Model.StopLimitPrice = Model.TradeType == TradeType.Buy
            ? Model.EntryPrice + Math.Min(5 * Symbol.PipSize, (Model.TakeProfits.List[0].Pips / 2.0) * Symbol.PipSize) 
            : Model.EntryPrice - Math.Min(5 * Symbol.PipSize, (Model.TakeProfits.List[0].Pips / 2.0) * Symbol.PipSize);
    }

    private void SetEntryPrice(bool canImplementModel,
        IModel storageModel)
    {
        if (Model.OrderType == OrderType.Instant)
        {
            //no need to check for symbol keep as is
            //because the bot always needs to update to current bid/ask
            Model.EntryPrice = Model.TradeType == TradeType.Buy ? Ask : Bid;
            return;
        }

        if (!canImplementModel)
        {
            SetDefaultEntryPrice();
            return;
        }

        if (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged)
        {
            SetDefaultEntryPrice();
            return;
        }

        Model.EntryPrice = storageModel.EntryPrice;
    }

    private void SetDefaultEntryPrice()
    {
        Model.EntryPrice = Model.TradeType == TradeType.Buy
            ? Ask + 10 * Symbol.PipSize
            : Bid - 10 * Symbol.PipSize;
    }

    private void ProcessOldAndNewParametersIfNeeded()
    {
        if (!InputUseLastSavedSettings)
            return;

        //Step 1: Load old parameters
        //Step 2: Copy new parameters from this current bot instance
        //Step 3: 
        //  Option A: (Old parameters are not null)
        //      - Compare old parameters with new parameters, in order to save only the changed properties
        //  Option B: (Old parameters are null)
        //Step 4: Save/Update new parameters to file
        //Step 5: Load Default Model
        //Step 6: Load Model from file
        //Step 7: Override Default Model with Model from File if needed
        //Step 8: Override Model from file with changed parameters where it applies
        var oldParameters = LocalStorage.GetObject<ExtractedParameters>($"{CleanBrokerName}-position-sizer-parameters{_fileTag}",
                LocalStorageScope.Instance);
        
        var newParameters = CopyParametersFromInstance();
        
        LoadChangedParametersIfAny(oldParameters, newParameters);
        
        LocalStorage.SetObject($"{CleanBrokerName}-position-sizer-parameters{_fileTag}", newParameters, LocalStorageScope.Instance);
        LocalStorage.Flush(LocalStorageScope.Instance);
        
        Print("Parameters updated to file...");
    }
    
    private void SetBrokerName()
    {
        CleanBrokerName = CleanName(Account.BrokerName);
    }

    private void SetFileTag()
    {
        _fileTag = string.Empty;
        switch (InputSymbolChangeAction)
        {
            case SymbolChangeAction.EachSymbolOwnSettings:
                _fileTag = $"-{CleanName(SymbolName)}";
                break;
            case SymbolChangeAction.ResetToDefaultsOnSymbolChange:
            case SymbolChangeAction.KeepPanelAsIs:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetLogger()
    {
        Logger.LogEvent += message => Print(message);
    }

    private void SetTrailingStopManager(bool canImplementModel, IModel storageModel)
    {
        if (!canImplementModel || (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged))
        {
            Model.TrailingStopPips = InputTrailingStopPips;
            return;
        }

        Model.TrailingStopPips = storageModel.TrailingStopPips;
        TrailingStop = new TrailingStop(this);
        _riskManagers.Add(TrailingStop);
    }

    private void SetBreakevenManager(bool canImplementModel, IModel storageModel)
    {
        if (!canImplementModel || (InputSymbolChangeAction == SymbolChangeAction.KeepPanelAsIs && _symbolHasChanged))
        {
            Model.BreakEvenPips = InputBreakevenPips;
            return;
        }

        Model.BreakEvenPips = storageModel.BreakEvenPips;
        BreakEven = new BreakEven(this);
        _riskManagers.Add(BreakEven);
    }

    private void SetIndexForLabels()
    {
        IndexForLabelReference = InputTradeButtonOffsetFromTheRight;
    }

    private void SetStyles()
    {
        if (InputDarkMode)
            SetDarkModeStyle();
        else
            SetLightModeStyle();
    }

    private bool CanImplementModelOrPropertyChange(IModel m)
    {
        if (!InputUseLastSavedSettings)
            return false;
        
        if (RunningMode != RunningMode.RealTime)
            return false;
        
        if (m == null)
            return false;
        
        if (m.Version != Model.Version)
            return false;
        
        switch (InputSymbolChangeAction)
        {
            case SymbolChangeAction.EachSymbolOwnSettings:
                if (m.SymbolName != Model.SymbolName)
                    throw new Exception(
                        $"Symbol Names are supposed to be the same, but they are different: {m.SymbolName} and {Model.SymbolName}");
                return true;
            case SymbolChangeAction.ResetToDefaultsOnSymbolChange:
                var areTheSame = m.SymbolName == Model.SymbolName;
                if (!areTheSame)
                {
                    LocalStorage.Remove($"{CleanBrokerName}-model");
                    LocalStorage.Flush(LocalStorageScope.Instance);
                }

                return areTheSame;
            case SymbolChangeAction.KeepPanelAsIs:
                _symbolHasChanged = m.SymbolName != Model.SymbolName;
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void LoadChangedParametersIfAny(ExtractedParameters oldParameters, ExtractedParameters newParameters)
    {
        if (oldParameters == null || oldParameters.Version != newParameters.Version)
        {
            Print("Old parameters not found or version has changed...");
            return;
        }

        _changedProperties = GetChangedProperties(oldParameters, newParameters);
        foreach (var changedProperty in _changedProperties)
        {
            Print(
                $"Property {changedProperty} has changed from {oldParameters.GetType().GetProperty(changedProperty).GetValue(oldParameters)} to {newParameters.GetType().GetProperty(changedProperty).GetValue(newParameters)}");
        }
    }

    public ExtractedParameters CopyParametersFromInstance()
    {
        var newParameters = new ExtractedParameters();
        var properties = typeof(ExtractedParameters).GetProperties();
        
        foreach (var property in properties)
        {
            if (property.Name == "Version")
            {
                property.SetValue(newParameters, Version);
                continue;
            }

            var sourceProperty = this.GetType().GetProperty(property.Name);
            
            if (sourceProperty == null)
                continue;

            //Tell if the property is a color or not
            if (sourceProperty.PropertyType == typeof(Color))
            {
                var value = sourceProperty.GetValue(this);
                var c = new SerializableColor();
                c.FromColor((Color)value); 
                property.SetValue(newParameters, c);
            }
            else if (sourceProperty.PropertyType == typeof(TimeFrame))
            {
                var value = sourceProperty.GetValue(this);
                var tf = new SerializableTimeFrame();
                tf.FromTimeFrame((TimeFrame)value);
                property.SetValue(newParameters, tf);
            }
            else
            {
                var value = sourceProperty.GetValue(this);
                property.SetValue(newParameters, value);
            }
        }

        Print($"Parameters copied...");
        return newParameters;
    }

    public List<string> GetChangedProperties(ExtractedParameters oldExtractedParameters,
        ExtractedParameters newExtractedParameters)
    {
        var changedProperties = new List<string>();
        var properties = typeof(ExtractedParameters).GetProperties();
        
        foreach (var property in properties)
        {
            var oldValue = property.GetValue(oldExtractedParameters);
            var newValue = property.GetValue(newExtractedParameters);
            
            if (oldValue != null && newValue != null)
            {
                if (!oldValue.Equals(newValue))
                {
                    changedProperties.Add(property.Name);
                }
            }
            else if (oldValue == null && newValue != null)
            {
                changedProperties.Add(property.Name);
            }
            else if (oldValue != null)
            {
                changedProperties.Add(property.Name);
            }
        }

        return changedProperties;
    }

    private void SetRisksOnModel()
    {
        if (InputPositionSizeInLots > 0)
            Model.UpdateWithTradeSizeLots(InputPositionSizeInLots);
        else if (InputMoneyRisk > 0)
            Model.UpdateWithRiskInCurrency(InputMoneyRisk, InputRoundingPositionSizeAndPotentialReward);
        else if (InputRiskPercentage > 0)
            Model.UpdateWithRiskPercentage(InputRiskPercentage, InputRoundingPositionSizeAndPotentialReward);
        else if (InputPositionSizeInLots == 0 && InputMoneyRisk == 0 && InputRiskPercentage == 0)
            Model.SetRiskDefaults(InputRoundingPositionSizeAndPotentialReward);
    }
    
    public XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler)
    {
        var xtbDouble = new XTextBoxDouble(defaultValue, digits);
        xtbDouble.SetCustomStyle(CustomStyle);
        xtbDouble.ValueUpdated += valueUpdatedHandler;
        xtbDouble.ControlClicked += (_, _) => TrySaveAllTextBoxesContent();  
        return xtbDouble;
    }

    public XTextBoxDoubleNumeric MakeTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler)
    {
        var xtb = new XTextBoxDoubleNumeric(defaultValue, digits, changeByFactor);
        xtb.SetCustomStyle(CustomStyle);
        xtb.ValueUpdated += valueUpdatedHandler;
        xtb.ControlClicked += (_, _) => TrySaveAllTextBoxesContent();
        xtb.IncrementButtonClicked += (_, _) => TrySaveAllTextBoxesContent();
        xtb.DecrementButtonClicked += (_, _) => TrySaveAllTextBoxesContent();
        return xtb;
    }

    public XTextBoxInt MakeTextBoxInt(int defaultValue, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler)
    {
        var xtb = new XTextBoxInt(defaultValue);
        xtb.SetCustomStyle(CustomStyle);
        xtb.ValueUpdated += valueUpdatedHandler;
        xtb.ControlClicked += (_, _) => TrySaveAllTextBoxesContent();
        return xtb;
    }

    public XTextBoxIntNumericUpDown MakeTextBoxIntNumeric(int defaultValue, int changeByFactor, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler)
    {
        var xtb = new XTextBoxIntNumericUpDown(defaultValue, changeByFactor);
        xtb.SetCustomStyle(CustomStyle);
        xtb.ValueUpdated += valueUpdatedHandler;
        xtb.ControlClicked += (_, _) => TrySaveAllTextBoxesContent();
        xtb.IncrementButtonClicked += (_, _) => TrySaveAllTextBoxesContent();
        xtb.DecrementButtonClicked += (_, _) => TrySaveAllTextBoxesContent();
        return xtb;
    }

    public XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler)
    {
        var xtb = new XTextBoxString(defaultValue);
        xtb.SetCustomStyle(CustomStyle);
        xtb.ValueUpdated += valueUpdatedHandler;
        xtb.ControlClicked += (_, _) => TrySaveAllTextBoxesContent();
        return xtb;
    }

    private void SetLightModeStyle()
    {
        CustomStyle = new CustomStyle();
        CustomStyle.BackgroundStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FFF7F7F7"));
        
        CustomStyle.ButtonStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FFDDE2EB"));
        CustomStyle.ButtonStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.ButtonStyle.Set(ControlProperty.BorderColor, Color.FromHex("FFB2C3CF"));
        CustomStyle.ButtonStyle.Set(ControlProperty.BorderThickness, 1);
        
        CustomStyle.LockedButtonStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FFC8C8C8"));
        CustomStyle.LockedButtonStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.LockedButtonStyle.Set(ControlProperty.BorderColor, Color.FromHex("FF888888"));
        CustomStyle.LockedButtonStyle.Set(ControlProperty.BorderThickness, 1);
        
        CustomStyle.TextBoxStyle.Set(ControlProperty.BackgroundColor, Color.White);
        CustomStyle.TextBoxStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.TextBoxStyle.Set(ControlProperty.BorderColor, Color.FromHex("FFB2C3CF"));
        CustomStyle.TextBoxStyle.Set(ControlProperty.BorderThickness, 1);
        CustomStyle.TextBoxStyle.Set(ControlProperty.CaretColor, Color.Black);

        // ReSharper disable once StringLiteralTypo
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FFDDDDD3"));
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.BorderColor, Color.FromHex("FFB2C3CF"));
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.BorderThickness, 1);
        
        CustomStyle.CheckBoxStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FFDDDDD3"));
        CustomStyle.CheckBoxStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.CheckBoxStyle.Set(ControlProperty.BorderThickness, 1);
    }

    private void SetDarkModeStyle()
    {
        CustomStyle = new CustomStyle();
        CustomStyle.BackgroundStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF666666"));
        CustomStyle.ButtonStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF9999A1"));
        CustomStyle.ButtonStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.ButtonStyle.Set(ControlProperty.BorderColor,  Color.FromHex("FF888888"));
        CustomStyle.ButtonStyle.Set(ControlProperty.BorderThickness, 1);
        CustomStyle.LockedButtonStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF909090"));
        CustomStyle.LockedButtonStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.LockedButtonStyle.Set(ControlProperty.BorderColor, Color.FromHex("FF888888"));
        CustomStyle.LockedButtonStyle.Set(ControlProperty.BorderThickness, 1);

        // ReSharper disable once StringLiteralTypo
        CustomStyle.TextBoxStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FFAAAAAA"));
        CustomStyle.TextBoxStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.TextBoxStyle.Set(ControlProperty.BorderColor, Color.FromHex("FF888888"));
        CustomStyle.TextBoxStyle.Set(ControlProperty.BorderThickness, 1);
        CustomStyle.TextBoxStyle.Set(ControlProperty.CaretColor, Color.Black);

        // ReSharper disable once StringLiteralTypo
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF999999"));
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.BorderColor, Color.FromHex("FF888888"));
        CustomStyle.ReadOnlyTextBoxStyle.Set(ControlProperty.BorderThickness, 1);
        
        CustomStyle.CheckBoxStyle.Set(ControlProperty.BackgroundColor, Color.FromHex("FF999999"));
        CustomStyle.CheckBoxStyle.Set(ControlProperty.ForegroundColor, Color.Black);
        CustomStyle.CheckBoxStyle.Set(ControlProperty.BorderThickness, 1);
    }

    public double PipsToTicks(double pips) =>
        pips * (Symbol.PipSize / Symbol.TickSize);

    public Button MakeButton(string text)
    {
        return new Button
        {
            Style = CustomStyle.ButtonStyle,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            // Margin = new Thickness(2, 4, 2, 4),
            Margin = 1,
            Height = ControlHeight
        };
    }
}