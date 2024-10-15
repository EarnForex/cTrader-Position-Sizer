using cAlgo.API;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    #region Compactness

    /// <summary>
    /// Show pip distance for TP/SL near lines
    /// </summary>
    [Parameter("Show Line Labels", DefaultValue = true, Group = "Compactness")]
    public bool InputShowLineLabels { get; set; }

    /// <summary>
    /// Show SL $/% label
    /// </summary>
    [Parameter("Show Additional SL Label", DefaultValue = false, Group = "Compactness")]
    public bool InputShowAdditionalStopLossLabel { get; set; }

    /// <summary>
    /// Show TP $/% + R/R label?
    /// </summary>
    [Parameter("Show Additional TP Label", DefaultValue = false, Group = "Compactness")]
    public bool InputShowAdditionalTpLabel { get; set; }

    /// <summary>
    /// Show Position Size Label?
    /// </summary>
    [Parameter("Show Additional Entry Label", DefaultValue = false, Group = "Compactness")]
    public bool InputShowAdditionalEntryLabel { get; set; }
    
    /// <summary>
    /// Hide account size and its buttons on the Main View
    /// </summary>
    [Parameter("Hide Account Size", DefaultValue = false, Group = "Compactness")]
    public bool InputHideAccountSize { get; set; }
    
    [Parameter("Show Pip Value", DefaultValue = false, Group = "Compactness")]
    public bool InputShowPipValue { get; set; }

    [Parameter("Show Max Pos. Size Button", DefaultValue = false, Group = "Compactness")]
    public bool InputShowMaxPositionSizeButton { get; set; }

    [Parameter("Start Panel Minimized", DefaultValue = false, Group = "Compactness")]
    public bool InputStartPanelMinimized { get; set; }

    /// <summary>
    /// If true, SL and TP can be set via ATR
    /// </summary>
    [Parameter("Show ATR Options", DefaultValue = false, Group = "Compactness")]
    public bool InputShowAtrOptions { get; set; }

    [Parameter("Show Max Parameters on Trading Tab", DefaultValue = true, Group = "Compactness")]
    public bool InputShowMaxParametersOnTradingTab { get; set; }

    [Parameter("Show Trading Fuses On Trading Tab", DefaultValue = true, Group = "Compactness")]
    public bool InputShowTradingFusesOnTradingTab { get; set; }

    [Parameter("Show Checkboxes on Trading Tab", DefaultValue = true, Group = "Compactness")]
    public bool InputShowCheckBoxesOnTradingTab { get; set; }

    [Parameter("Hide Entry Line for Instant Orders", DefaultValue = false, Group = "Compactness")]
    public bool InputHideEntryLineForInstantOrders { get; set; }
    
    [Parameter("Additional Trade Buttons", DefaultValue = AdditionalTradeButtons.None, Group = "Compactness")]
    public AdditionalTradeButtons InputAdditionalTradeButtons { get; set; }

    #endregion

    #region Fonts

    [Parameter("SL Label Color", DefaultValue = "Green", Group = "Fonts")]
    public Color InputStopLossLabelColor { get; set; }

    [Parameter("TP Label Color", DefaultValue = "Goldenrod", Group = "Fonts")]
    public Color InputTpLabelColor { get; set; }

    [Parameter("Stop Price Label Color", DefaultValue = "Purple", Group = "Fonts")]
    public Color InputStopPriceLabelColor { get; set; }

    [Parameter("Entry Label Color", DefaultValue = "Blue", Group = "Fonts")]
    public Color InputEntryLabelColor { get; set; }

    [Parameter("Labels Font Size", DefaultValue = 13, MinValue = 10, MaxValue = 30, Group = "Fonts")]
    public int InputLabelsFontSize { get; set; }

    #endregion

    #region Lines

    [Parameter("Entry Line Color", DefaultValue = "Blue", Group = "Lines")]
    public Color InputEntryLineColor { get; set; }

    [Parameter("Stop Loss Line Color", DefaultValue = "Green", Group = "Lines")]
    public Color InputStopLossLineColor { get; set; }

    [Parameter("Take Profit Line Color", DefaultValue = "Goldenrod", Group = "Lines")]
    public Color InputTakeProfitLineColor { get; set; }

    [Parameter("Stop Price Line Color", DefaultValue = "Purple", Group = "Lines")]
    public Color InputStopPriceLineColor { get; set; }

    [Parameter("Breakeven Line Color", DefaultValue = "Transparent", Group = "Lines")]
    public Color InputBreakevenLineColor { get; set; }

    [Parameter("Entry Line Style", DefaultValue = LineStyle.Solid, Group = "Lines")]
    public LineStyle InputEntryLineStyle { get; set; }

    [Parameter("Stop Loss Line Style", DefaultValue = LineStyle.Solid, Group = "Lines")]
    public LineStyle InputStopLossLineStyle { get; set; }

    [Parameter("Take Profit Line Style", DefaultValue = LineStyle.Solid, Group = "Lines")]
    public LineStyle InputTakeProfitLineStyle { get; set; }

    [Parameter("Stop Price Line Style", DefaultValue = LineStyle.Dots, Group = "Lines")]
    public LineStyle InputStopPriceLineStyle { get; set; }

    [Parameter("Breakeven Line Style", DefaultValue = LineStyle.Dots, Group = "Lines")]
    public LineStyle InputBreakevenLineStyle { get; set; }

    [Parameter("Entry Line Width", DefaultValue = 1, MinValue = 1, Group = "Lines")]
    public int InputEntryLineWidth { get; set; }
    
    [Parameter("Stop Loss Line Width", DefaultValue = 1, MinValue = 1, Group = "Lines")]
    public int InputStopLossLineWidth { get; set; }
    
    [Parameter("Take Profit Line Width", DefaultValue = 1, MinValue = 1, Group = "Lines")]
    public int InputTakeProfitLineWidth { get; set; }
    
    [Parameter("Stop Price Line Width", DefaultValue = 1, MinValue = 1, Group = "Lines")]
    public int InputStopPriceLineWidth { get; set; }
    
    [Parameter("Breakeven Line Width", DefaultValue = 1, MinValue = 1, Group = "Lines")]
    public int InputBreakevenLineWidth { get; set; }

    #endregion

    #region Defaults

    [Parameter("Trade Direction", DefaultValue = TradeType.Buy, Group = "Defaults")]
    public TradeType InputTradeType { get; set; }

    [Parameter("Default SL (pips)", DefaultValue = 0, MinValue = 0, Step = 0.1, Group = "Defaults")]
    public double InputDefaultStopLossPips { get; set; }

    [Parameter("Default TP (pips)", DefaultValue = 0, MinValue = 0, Step = 0.1, Group = "Defaults")]
    public double InputDefaultTakeProfitPips { get; set; }
    
    /// <summary>
    /// More than 1 Splits trades
    /// </summary>
    [Parameter("Take Profits Number", DefaultValue = 1, MinValue = 1, MaxValue = 10, Group = "Defaults")]
    public int InputTakeProfitsNumber { get; set; }

    [Parameter("Entry Type", DefaultValue = OrderType.Instant, Group = "Defaults")]
    public OrderType InputOrderType { get; set; }

    [Parameter("Show Lines", DefaultValue = true, Group = "Defaults")]
    public bool InputShowLinesByDefault { get; set; }
    
    /// SL/TP (Entry In Pending) Lines Selected were removed because it is not needed

    [Parameter("ATR Period", DefaultValue = 14, MinValue = 1, Group = "Defaults")]
    public int InputAtrPeriod { get; set; }

    [Parameter("ATR Multiplier for SL", DefaultValue = 0.0, MinValue = 0, Group = "Defaults")]
    public double InputDefaultAtrMultiplierStopLoss { get; set; }

    [Parameter("ATR Multiplier for TP", DefaultValue = 0.0, MinValue = 0, Group = "Defaults")]
    public double InputDefaultAtrMultiplierTakeProfit { get; set; }

    [Parameter("ATR TimeFrame", DefaultValue = "Hour", Group = "Defaults")]
    public TimeFrame InputAtrTimeFrame { get; set; }

    /// <summary>
    /// Adjust SL by Spread value in ATR mode
    /// </summary>
    [Parameter("Spread Adjustment SL", DefaultValue = false, Group = "Defaults")]
    public bool InputSpreadAdjustmentStopLoss { get; set; }

    /// <summary>
    /// Adjust TP by Spread value in ATR mode
    /// </summary>
    [Parameter("Spread Adjustment TP", DefaultValue = false, Group = "Defaults")]
    public bool InputSpreadAdjustmentTakeProfit { get; set; }

    [Parameter("Account Button", DefaultValue = AccountSizeMode.Balance, Group = "Defaults")]
    public AccountSizeMode InputAccountSizeMode { get; set; }

    [Parameter("Risk (%)", DefaultValue = 1.0, MinValue = 0, Group = "Defaults")]
    public double InputRiskPercentage { get; set; }

    /// <summary>
    /// If > 0, money risk tolerance in currency
    /// </summary>
    [Parameter("Money Risk", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputMoneyRisk { get; set; }

    /// <summary>
    /// If > 0, position size in lots
    /// </summary>
    [Parameter("Position Size", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputPositionSizeInLots { get; set; }

    [Parameter("Count Pending Orders", DefaultValue = false, Group = "Defaults")]
    public bool InputCountPendingOrders { get; set; }

    /// <summary>
    /// Ignore Orders Without SL in portfolio risk
    /// </summary>
    [Parameter("Ignore Orders Without SL", DefaultValue = false, Group = "Defaults")]
    public bool InputIgnoreOrdersWithoutStopLoss { get; set; }

    /// <summary>
    /// Ignore Orders Without TP in portfolio risk
    /// </summary>
    [Parameter("Ignore Orders Without TP", DefaultValue = false, Group = "Defaults")]
    public bool InputIgnoreOrdersWithoutTakeProfit { get; set; }

    /// <summary>
    /// Ignore Other Symbol orders in portfolio r
    /// </summary>
    [Parameter("Ignore Other Symbols", DefaultValue = false, Group = "Defaults")]
    public bool InputIgnoreOtherSymbols { get; set; }

    /// <summary>
    /// Default Custom Leverage for Margin Tab
    /// </summary>
    [Parameter("Custom Leverage", DefaultValue = 0.0, MinValue = 0, Group = "Defaults")]
    public double InputCustomLeverage { get; set; }

    [Parameter("Label", DefaultValue = "PSLabel", Group = "Defaults")]
    public string InputLabel { get; set; }

    [Parameter("Commentary", DefaultValue = "", Group = "Defaults")]
    public string InputCommentary { get; set; }

    /// <summary>
    /// Automatic Suffix for order commentary
    /// </summary>
    [Parameter("AutoSuffix", DefaultValue = false, Group = "Defaults")]
    public bool InputAutoSuffix { get; set; }

    [Parameter("Disable Trading When Lines Are Hidden", DefaultValue = false, Group = "Defaults")]
    public bool InputDisableTradingWhenLinesAreHidden { get; set; }

    [Parameter("Max Slippage (pips)", DefaultValue = 0, Step = 0.1, MinValue = 0, Group = "Defaults")]
    public double InputMaxSlippagePips { get; set; }

    [Parameter("Max Spread (pips)", DefaultValue = 0, Step = 0.1, MinValue = 0, Group = "Defaults")]
    public double InputMaxSpreadPips { get; set; }

    [Parameter("Max Entry SL Distance (pips)", DefaultValue = 0, Step = 0.1, MinValue = 0, Group = "Defaults")]
    public double InputMaxEntryStopLossDistancePips { get; set; }

    [Parameter("Min Entry SL Distance (pips)", DefaultValue = 0, Step = 0.1, MinValue = 0, Group = "Defaults")]
    public double InputMinEntryStopLossDistancePips { get; set; }

    [Parameter("Maximum Position Size Total for Trading Tab", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputMaxPositionSizeTotalForTradingTab { get; set; }

    [Parameter("Max Position Size per Symbol for Trading Tab", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputMaxPositionSizePerSymbolForTradingTab { get; set; }

    /// <summary>
    /// Subtract Open Positions Volume (Trading Tab)
    /// </summary>
    [Parameter("Subtract OPV", DefaultValue = false, Group = "Defaults")]
    public bool InputSubtractOpv { get; set; }

    /// <summary>
    /// Subtract Pending Orders Volume (Trading Tab)
    /// </summary>
    [Parameter("Subtract POV", DefaultValue = false, Group = "Defaults")]
    public bool InputSubtractPov { get; set; }

    [Parameter("Do not Apply Stop Loss", DefaultValue = false, Group = "Defaults")]
    public bool InputDoNotApplyStopLoss { get; set; }

    [Parameter("Do not Apply Take Profit", DefaultValue = false, Group = "Defaults")]
    public bool InputDoNotApplyTakeProfit { get; set; }

    [Parameter("Ask for Confirmation", DefaultValue = true, Group = "Defaults")]
    public bool InputAskForConfirmation { get; set; }

    [Parameter("Panel Position (X)", DefaultValue = 10, MinValue = 0, Group = "Defaults")]
    public int InputPanelPositionX { get; set; }

    [Parameter("Panel Position (Y)", DefaultValue = 10, MinValue = 0, Group = "Defaults")]
    public int InputPanelPositionY { get; set; }

    /// <summary>
    /// Lock TP to (multiplied) SL distance
    /// </summary>
    [Parameter("TP Locked On SL", DefaultValue = false, Group = "Defaults")]
    public bool InputTakeProfitLockedOnStopLoss { get; set; }

    [Parameter("Trailing Stop (pips)", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputTrailingStopPips { get; set; }

    [Parameter("Breakeven (pips)", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputBreakevenPips { get; set; }

    [Parameter("Expiry Seconds", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public int InputExpirySeconds { get; set; }

    [Parameter("Max Number Of Trades Total", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public int InputMaxNumberOfTradesTotal { get; set; }

    [Parameter("Max Number Of Trades per Symbol", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public int InputMaxNumberOfTradesPerSymbol { get; set; }

    [Parameter("Max Risk Total (%)", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputMaxRiskTotal { get; set; }

    [Parameter("Max Risk Per Symbol (%)", DefaultValue = 0, MinValue = 0, Group = "Defaults")]
    public double InputMaxRiskPerSymbol { get; set; }

    [Parameter("SL Distance (pips) Instead of a Level", DefaultValue = false, Group = "Defaults")]
    public bool InputStopLossDistancePipsInsteadOfLevel { get; set; }

    [Parameter("TP Distance (pips) Instead of a Level", DefaultValue = false, Group = "Defaults")]
    public bool InputTakeProfitDistancePipsInsteadOfLevel { get; set; }
    
    #endregion

    #region KeyboardShortcuts

    [Parameter("Execute a Trade", DefaultValue = "Shift + T", Group = "Keyboard Shortcuts")]
    public string InputHotkeyExecuteTrade { get; set; }

    [Parameter("Switch Order Type", DefaultValue = "O", Group = "Keyboard Shortcuts")]
    public string InputHotkeySwitchOrderType { get; set; }

    [Parameter("Switch Entry Direction", DefaultValue = "Tab", Group = "Keyboard Shortcuts")]
    public string InputHotkeySwitchEntryDirection { get; set; }

    [Parameter("Switch Hide Show Lines", DefaultValue = "H", Group = "Keyboard Shortcuts")]
    public string InputHotkeySwitchHideShowLines { get; set; }

    /// <summary>
    /// Set SL to where mouse pointer is
    /// </summary>
    [Parameter("Set Stop Loss Hotkey", DefaultValue = "S", Group = "Keyboard Shortcuts")]
    public string InputHotkeySetStopLoss { get; set; }

    /// <summary>
    /// Set TP to where mouse pointer is
    /// </summary>
    [Parameter("Set Take Profit Hotkey", DefaultValue = "P", Group = "Keyboard Shortcuts")]
    public string InputHotkeySetTakeProfit { get; set; }

    /// <summary>
    /// Set Entry to where mouse pointer is
    /// </summary>
    [Parameter("Set Entry Hotkey", DefaultValue = "E", Group = "Keyboard Shortcuts")]
    public string InputHotkeySetEntry { get; set; }

    [Parameter("Minimize Maximize Hotkey", DefaultValue = "`", Group = "Keyboard Shortcuts")]
    public string InputMinimizeMaximizeHotkeyPanel { get; set; }

    /// <summary>
    /// Switch SL between points and level
    /// </summary>
    [Parameter("Switch SL Pips Level Hotkey", DefaultValue = "Shift + S", Group = "Keyboard Shortcuts")]
    public string InputHotkeySwitchStopLossPipsLevel { get; set; }

    [Parameter("Switch TP Pips Level Hotkey", DefaultValue = "Shift + P", Group = "Keyboard Shortcuts")]
    public string InputHotkeySwitchTakeProfitPipsLevel { get; set; }

    #endregion

    #region Miscellaneous

    /// <summary>
    /// Appears in Take-Profit button
    /// </summary>
    [Parameter("TP Multiplier for SL Value", DefaultValue = 1, Group = "Miscellaneous")]
    public double InputTakeProfitMultiplierForStopLossValue { get; set; }

    /// <summary>
    /// For TP Button
    /// </summary>
    [Parameter("Use Commission to Set TP Distance", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputUseCommissionToSetTpDistance { get; set; }

    /// <summary>
    /// Show Current Spread in Points or as an SL Ratio
    /// </summary>
    [Parameter("Show Spread", DefaultValue = ShowSpreadMode.None, Group = "Miscellaneous")]
    public ShowSpreadMode InputShowSpread { get; set; }

    /// <summary>
    /// Added to Account Balance for Risk Calculation
    /// </summary>
    [Parameter("Additional Funds", DefaultValue = 0.0, MinValue = 0, Group = "Miscellaneous")]
    public double InputAdditionalFunds { get; set; }
    
    /// <summary>
    /// Overrides Additional Funds Value
    /// </summary>
    [Parameter("Custom Balance", DefaultValue = 0.0, MinValue = 0, Group = "Miscellaneous")]
    public double InputCustomBalance { get; set; }

    /// <summary>
    /// Candle to get ATR Value from
    /// </summary>
    [Parameter("ATR Candle", DefaultValue = AtrCandle.CurrentCandle, Group = "Miscellaneous")]
    public AtrCandle InputAtrCandle { get; set; }

    [Parameter("Calculate Unadjusted Position Size", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputCalculateUnadjustedPositionSize { get; set; }
    
    [Parameter("Surpass Broker Max Position Size With Multiple Trades", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputSurpassBrokerMaxPositionSizeWithMultipleTrades { get; set; }

    /// <summary>
    /// Position Size and Potential Reward are Rounded Down
    /// </summary>
    [Parameter("Rounding", DefaultValue = RoundingMode.Down, Group = "Miscellaneous")]
    public RoundingMode InputRoundingPositionSizeAndPotentialReward { get; set; }

    /// <summary>
    /// First Quick Risk Percentage Points
    /// </summary>
    [Parameter("Quick Risk 1 (%)", DefaultValue = 0.0, MinValue = 0.0, Group = "Miscellaneous")]
    public double InputQuickRisk1Pct { get; set; }

    [Parameter("Quick Risk 2 (%)", DefaultValue = 0.0, MinValue = 0.0, Group = "Miscellaneous")]
    public double InputQuickRisk2Pct { get; set; }

    /// <summary>
    /// I think this parameter should not be saved, because it deals with the symbol change action and how the settings are restored
    /// </summary>
    [Parameter("Symbol Change Action", DefaultValue = SymbolChangeAction.EachSymbolOwnSettings, Group = "Miscellaneous")]
    public SymbolChangeAction InputSymbolChangeAction { get; set; }

    /// <summary>
    /// If true, Stop Limit will be skipped
    /// </summary>
    [Parameter("Disable Stop Limit", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputDisableStopLimit { get; set; }

    //Removed "Disable Trading Sounds" Because there are no custom sounds coming from the bot

    /// <summary>
    /// Apply SL-TP after all trades executed
    /// </summary>
    [Parameter("Apply SL-TP After all trades executed", DefaultValue = true, Group = "Miscellaneous")]
    public bool InputApplySlTpAfterAllTradesExecuted { get; set; }

    [Parameter("Dark Mode", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputDarkMode { get; set; }

    [Parameter("Restore Window Location on Chart Size Change", DefaultValue = true, Group = "Miscellaneous")]
    public bool InputRestoreWindowLocationOnChartSizeChange { get; set; }

    [Parameter("Use Last Saved Settings", DefaultValue = true, Group = "Miscellaneous")]
    public bool InputUseLastSavedSettings { get; set; }
    
    [Parameter("Prefill Additional TPs Based on Main", DefaultValue = true, Group = "Miscellaneous")]
    public bool InputPrefillAdditionalTpsBasedOnMain { get; set; }

    [Parameter("Ask for confirmation before closing the panel", DefaultValue = false, Group = "Miscellaneous")]
    public bool InputAskForConfirmationBeforeClosingThePanel { get; set; }
    
    #endregion
    
    //[Parameter("Refresh (ms)", DefaultValue = 20)]
    public int InputRefreshMilliseconds { get; set; } = 20;

    //[Parameter("IndexTest", DefaultValue = 50, Group = "Debugging")]
    public int InputIndexTest { get; set; }
}