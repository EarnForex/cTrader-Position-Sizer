using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using cAlgo.API;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public interface ITradingViewResources
{
    CustomStyle CustomStyle { get; }
    bool InputShowMaxParametersOnTradingTab { get; }
    bool InputShowTradingFusesOnTradingTab { get; }
    bool InputShowCheckBoxesOnTradingTab { get; }
    void Print(object obj);
    XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxDoubleNumeric MakeTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxInt MakeTextBoxInt(int defaultValue, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxIntNumericUpDown MakeTextBoxIntNumeric(int defaultValue, int changeByFactor, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler);
    bool InputDarkMode { get; }
    Color InputTradeButtonColor { get; }
}

public sealed class TradingView : Button, ITradingViewResources, ITpDistributionResources, IDisposable
{
    private readonly ITradingViewResources _resources;

    #region Events

    public event EventHandler TradeButtonClicked;
    public event EventHandler<TrailingStopValueChangedEventArgs> TrailingStopValueChanged;
    public event EventHandler<BreakevenValueChangedEventArgs> BreakevenValueChanged;
    public event EventHandler<LabelValueChangedEventArgs> LabelValueChanged;
    public event EventHandler<ExpiryValueChangedEventArgs> ExpiryValueChanged;
    public event EventHandler<OrderCommentValueChangedEventArgs> OrderCommentValueChanged;
    public event EventHandler<AutoSuffixValueChangedEventArgs> AutoSuffixValueChanged;
    public event EventHandler<MaxNumberOfTradesTotalValueChangedEventArgs> MaxNumberOfTradesTotalValueChanged;
    public event EventHandler<MaxNumberOfTradesPerSymbolEventArgs> MaxNumberOfTradesPerSymbolValueChanged;
    public event EventHandler<MaxVolumeTotalValueChangedEventArgs> MaxVolumeTotalValueChanged;
    public event EventHandler<MaxVolumePerSymbolValueChangedEventArgs> MaxVolumePerSymbolValueChanged;
    public event EventHandler<MaxRiskTotalValueChangedEventArgs> MaxRiskTotalValueChanged;
    public event EventHandler<MaxRiskPerSymbolValueChangedEventArgs> MaxRiskPerSymbolValueChanged;
    public event EventHandler<DisableTradingWhenLinesHiddenEventArgs> DisableTradingWhenLinesHiddenCheckBoxChanged;
    public event EventHandler<MaxSlippageValueChangedEventArgs> MaxSlippageValueChanged;
    public event EventHandler<MaxSpreadValueSpreadEventArgs> MaxSpreadValueChanged;
    public event EventHandler<MaxEntrySlDistanceValueChangedEventArgs> MaxEntrySlDistanceValueChanged;
    public event EventHandler<MinEntrySlDistanceValueChangedEventArgs> MinEntrySlDistanceValueChanged;
    public event EventHandler<MaxRiskPercentageValueChangedEventArgs> MaxRiskPercentageValueChanged;
    public event EventHandler<SubtractOpenPositionsVolumeCheckBoxChangedEventArgs> SubtractOpenPositionsVolumeCheckBoxChanged;
    public event EventHandler<SubtractPendingOrdersVolumeCheckBoxChangedEventArgs> SubtractPendingOrdersVolumeCheckBoxChanged;
    public event EventHandler<DoNotApplyStopLossCheckBoxChangedEventArgs> DoNotApplyStopLossCheckBoxChanged;
    public event EventHandler<DoNotApplyTakeProfitCheckBoxChangedEventArgs> DoNotApplyTakeProfitCheckBoxChanged;
    public event EventHandler<AskForConfirmationCheckBoxChangedEventArgs> AskForConfirmationCheckBoxChanged;

    #endregion

    #region PrivateFields

    private readonly Button _tradeButton;
    private readonly XTextBoxDouble _trailingStopTextBox;
    private readonly XTextBoxDouble _breakevenTextBox;
    private readonly XTextBoxString _labelTextBox;
    private readonly XTextBoxInt _expiryTextBox;
    private readonly XTextBoxString _orderCommentTextBox;
    private readonly CheckBox _autoSuffixCheckBox;
    private readonly XTextBoxInt _maxTradesTotalTextBox;
    private readonly XTextBoxInt _maxTradesPerSymbolTextBox;
    private readonly XTextBoxDouble _maxVolumeTotalTextBox;
    private readonly XTextBoxDouble _maxVolumePerSymbolTextBox;
    private readonly XTextBoxDouble _maxRiskTotalTextBox;
    private readonly XTextBoxDouble _maxRiskPerSymbolTextBox;
    private readonly CheckBox _disableTradingCheckBox;
    private readonly XTextBoxDouble _maxSlippageTextBox;
    private readonly XTextBoxDouble _maxSpreadTextBox;
    private readonly XTextBoxDouble _maxEntrySlDistanceTextBox;
    private readonly XTextBoxDouble _minEntrySlDistanceTextBox;
    private readonly XTextBoxDouble _maxRiskPercentageTextBox;
    private readonly CheckBox _subtractOpenPositionsVolumeCheckBox;
    private readonly CheckBox _subtractPendingOrdersVolumeCheckBox;
    private readonly CheckBox _doNotApplyStopLossCheckBox;
    private readonly CheckBox _doNotApplyTakeProfitCheckBox;
    private readonly CheckBox _askForConfirmationCheckBox;
    private int _rowIndex;

    #endregion

    private void NewRow()
    {
        _rowIndex++;
        _grid.AddRow();
    }

    public TradingView(ITradingViewResources resources, IModel model)
    {
        _resources = resources;
        _grid = new Grid();
        Content = _grid;
        
        Width = 400;
        //Height = 450;

        _grid.AddColumns(5);
        //AddRows(18);

        //_grid.ShowGridLines = true;

        //--ROW 0
        //Trade Button (Column 0)
        //Trailing Stop TextBlock (Column 1)
        //Trailing Stop TextBox (Column 2)
        //Breakeven TextBlock (Column 3)
        //Breakeven TextBox (Column 4)

        _tradeButton = MakeButton("Trade");
        
        if (InputTradeButtonColor != Color.Transparent)
            _tradeButton.BackgroundColor = InputTradeButtonColor;
        
        _tradeButton.Click += TradeButtonOnClick;
        
        var trailingStopTextBlock = MakeTextBlock("Trail. Stop");
        _trailingStopTextBox = MakeTextBoxDouble(model.TrailingStopPips, 1, TrailingStopTextBoxOnTextChanged);
        
        var breakevenTextBlock = MakeTextBlock("Breakeven");
        _breakevenTextBox = MakeTextBoxDouble(model.BreakEvenPips, 1, BreakevenTextBoxOnTextChanged);

        _grid.AddRow();
        _grid.AddChild(_tradeButton, _rowIndex, 0);
        _grid.AddChild(trailingStopTextBlock, _rowIndex, 1);
        _grid.AddChild(_trailingStopTextBox, _rowIndex, 2);
        _grid.AddChild(breakevenTextBlock, _rowIndex, 3);
        _grid.AddChild(_breakevenTextBox, _rowIndex, 4);

        //--ROW 1
        //Label TextBlock (Column 0-1)
        //Label TextBox (Column 2)
        //Expiry (min) TextBlock (Column 3)
        //Expiry (min) TextBox (Column 4)
        
        var labelTextBlock = MakeTextBlock("Label");
        _labelTextBox = new XTextBoxString(string.Empty);
        _labelTextBox = MakeTextBoxString(model.Label, LabelTextBoxOnValueUpdated);
        _labelTextBox.TextAlignment = TextAlignment.Left;
        
        var expiryTextBlock = MakeTextBlock("Expiry (sec)");
        _expiryTextBox = MakeTextBoxInt(model.ExpirationSeconds, ExpiryTextBoxOnTextChanged);
        
        NewRow();
        _grid.AddChild(labelTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_labelTextBox, _rowIndex, 2);
        _grid.AddChild(expiryTextBlock, _rowIndex, 3);
        _grid.AddChild(_expiryTextBox, _rowIndex, 4);

        //--ROW 2
        //Order Comment TextBlock (Column 0-1)
        //Order Comment TextBox (Column 2-3)
        //Auto Suffix Checkbox (Column 4)
        
        var orderCommentTextBlock = MakeTextBlock("Order Comment");
        _orderCommentTextBox = MakeTextBoxString(model.Comment, OrderCommentTextBoxOnTextChanged);
        _orderCommentTextBox.TextAlignment = TextAlignment.Left;
        
        _autoSuffixCheckBox = MakeCheckBox("Auto Suffix");
        _autoSuffixCheckBox.Checked += AutoSuffixCheckBoxOnChecked;
        _autoSuffixCheckBox.Unchecked += AutoSuffixCheckBoxOnUnchecked;

        NewRow();
        _grid.AddChild(orderCommentTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_orderCommentTextBox, _rowIndex, 2);
        _grid.AddChild(_autoSuffixCheckBox, _rowIndex, 4);

        if (InputShowMaxParametersOnTradingTab)
        {
            //--ROW 3
            //Max # of Trades Total TextBlock (Column 0-1)
            //Max # of Trades Total TextBox (Column 2)
            //Per Symbol TextBlock (Column 3)
            //Per Symbol TextBox (Column 4)
            
            var maxTradesTotalTextBlock = MakeTextBlock("Max # of Trades Total");
            _maxTradesTotalTextBox = MakeTextBoxInt(model.MaxNumberOfTradesTotal, MaxTradesTotalTextBoxOnTextChanged);
            
            var maxTradesPerSymbolTextBlock = MakeTextBlock("Per Symbol");
            _maxTradesPerSymbolTextBox = MakeTextBoxInt(model.MaxNumberOfTradesPerSymbol, MaxTradesPerSymbolTextBoxOnTextChanged);

            NewRow();
            _grid.AddChild(maxTradesTotalTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxTradesTotalTextBox, _rowIndex, 2);
            _grid.AddChild(maxTradesPerSymbolTextBlock, _rowIndex, 3);
            _grid.AddChild(_maxTradesPerSymbolTextBox, _rowIndex, 4);

            //--ROW 4
            //Max Volume Total TextBlock (Column 0-1)
            //Max Volume Total TextBox (Column 2)
            //Per Symbol TextBlock (Column 3)
            //Per Symbol TextBox (Column 4)
            
            var maxVolumeTotalTextBlock = MakeTextBlock("Max Volume Total");
            _maxVolumeTotalTextBox = MakeTextBoxDouble(model.MaxLotsTotal, 2, MaxVolumeTotalTextBoxOnTextChanged);
            
            var maxVolumePerSymbolTextBlock = MakeTextBlock("Per Symbol");
            _maxVolumePerSymbolTextBox = MakeTextBoxDouble(model.MaxLotsPerSymbol, 2, MaxVolumePerSymbolTextBoxOnTextChanged);
            
            NewRow();
            _grid.AddChild(maxVolumeTotalTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxVolumeTotalTextBox, _rowIndex, 2);
            _grid.AddChild(maxVolumePerSymbolTextBlock, _rowIndex, 3);
            _grid.AddChild(_maxVolumePerSymbolTextBox, _rowIndex, 4);
            
            //--ROW 5
            //Max Risk % Total TextBlock (Column 0-1)
            //Max Risk % Total TextBox (Column 2)
            //Per Symbol TextBlock (Column 3)
            //Per Symbol TextBox (Column 4)
            
            var maxRiskTotalTextBlock = MakeTextBlock("Max Risk % Total");
            _maxRiskTotalTextBox = MakeTextBoxDouble(model.MaxRiskPctTotal, 2, MaxRiskTotalTextBoxOnTextChanged);
            
            var maxRiskPerSymbolTextBlock = MakeTextBlock("Per Symbol");
            _maxRiskPerSymbolTextBox = MakeTextBoxDouble(model.MaxRiskPctPerSymbol, 2, MaxRiskPerSymbolTextBoxOnTextChanged);
            
            NewRow();
            _grid.AddChild(maxRiskTotalTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxRiskTotalTextBox, _rowIndex, 2);
            _grid.AddChild(maxRiskPerSymbolTextBlock, _rowIndex, 3);
            _grid.AddChild(_maxRiskPerSymbolTextBox, _rowIndex, 4);
        }

        //--ROW 6
        //Disable Trading when lines are hidden CheckBox (Column 0-4)
        
        _disableTradingCheckBox = MakeCheckBox("Disable Trading when lines are hidden");
        _disableTradingCheckBox.Checked += DisableTradingCheckBoxOnChecked;
        _disableTradingCheckBox.Unchecked += DisableTradingCheckBoxOnUnchecked;

        NewRow();
        _grid.AddChild(_disableTradingCheckBox, _rowIndex, 0, 1, 5);

        TpDistribution = new TpDistribution(this, model);
        
        NewRow();
        _grid.AddChild(TpDistribution, _rowIndex, 0, 1, 5);
        
        //
        if (InputShowTradingFusesOnTradingTab)
        {
            //--ROW 7
            //Points TextBlock (Column 2)
        
            var pointsTextBlock = MakeTextBlock("Pips");
        
            NewRow();
            _grid.AddChild(pointsTextBlock, _rowIndex, 2);
            
            //--ROW 8
            //Max Slippage TextBlock (Column 0-1)
            //Max Slippage TextBox (Column 2)
            
            var maxSlippageTextBlock = MakeTextBlock("Max Slippage");
            _maxSlippageTextBox = MakeTextBoxDouble(model.MaxSlippagePips, 1, MaxSlippageTextBoxOnTextChanged);

            NewRow();
            _grid.AddChild(maxSlippageTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxSlippageTextBox, _rowIndex, 2);

            //--ROW 9
            //Max Spread TextBlock (Column 0-1)
            //Max Spread TextBox (Column 2)
        
            var maxSpreadTextBlock = MakeTextBlock("Max Spread");
            _maxSpreadTextBox = MakeTextBoxDouble(model.MaxSpreadPips, 1, MaxSpreadTextBoxOnTextChanged);

            NewRow();
            _grid.AddChild(maxSpreadTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxSpreadTextBox, _rowIndex, 2);

            //--ROW 10
            //Max Entry/SL distance TextBlock (Column 0-1)
            //Max Entry/SL distance TextBox (Column 2)
        
            var maxEntrySlDistanceTextBlock = MakeTextBlock("Max Entry/SL distance");
            _maxEntrySlDistanceTextBox = MakeTextBoxDouble(model.MaxEntryStopLossDistancePips, 1, MaxEntrySlDistanceTextBoxOnTextChanged);

            NewRow();
            _grid.AddChild(maxEntrySlDistanceTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxEntrySlDistanceTextBox, _rowIndex, 2);
        
            //--ROW 11
            //Min Entry/SL distance TextBlock (Column 0-1)
            //Min Entry/SL distance TextBox (Column 2)
        
            var minEntrySlDistanceTextBlock = MakeTextBlock("Min Entry/SL distance");
            _minEntrySlDistanceTextBox = MakeTextBoxDouble(0.0, 1, MinEntrySlDistanceTextBoxOnTextChanged);
        
            NewRow();
            _grid.AddChild(minEntrySlDistanceTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_minEntrySlDistanceTextBox, _rowIndex, 2);
            
            
            var maxRiskPercentageTextBlock = MakeTextBlock("Max Risk %");
            _maxRiskPercentageTextBox = MakeTextBoxDouble(0.0, 1, MaxRiskPercentageTextBoxOnTextChanged);
            
            NewRow();
            _grid.AddChild(maxRiskPercentageTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_maxRiskPercentageTextBox, _rowIndex, 2);
        }

        if (InputShowCheckBoxesOnTradingTab)
        {
            //--ROW 12
            //Subtract open positions volume CheckBox (Column 0-4)
        
            _subtractOpenPositionsVolumeCheckBox = MakeCheckBox("Subtract open positions volume");
            _subtractOpenPositionsVolumeCheckBox.Checked += SubtractOpenPositionsVolumeCheckBoxOnChecked;
            _subtractOpenPositionsVolumeCheckBox.Unchecked += SubtractOpenPositionsVolumeCheckBoxOnUnChecked;
        
            NewRow();
            _grid.AddChild(_subtractOpenPositionsVolumeCheckBox, _rowIndex, 0, 1, 5);

            //--ROW 13
            //Subtract pending orders volume CheckBox (Column 0-4)
        
            _subtractPendingOrdersVolumeCheckBox = MakeCheckBox("Subtract pending orders volume");
            _subtractPendingOrdersVolumeCheckBox.Checked += SubtractPendingOrdersVolumeCheckBoxOnChecked;
            _subtractPendingOrdersVolumeCheckBox.Unchecked += SubtractPendingOrdersVolumeCheckBoxOnUnChecked;

            NewRow();
            _grid.AddChild(_subtractPendingOrdersVolumeCheckBox, _rowIndex, 0, 1, 5);
        
            //--ROW 14
            //Do not apply stop loss CheckBock (Column 0-4)
        
            _doNotApplyStopLossCheckBox = MakeCheckBox("Do not apply stop loss");
            _doNotApplyStopLossCheckBox.Checked += DoNotApplyStopLossCheckBoxOnChecked;
            _doNotApplyStopLossCheckBox.Unchecked += DoNotApplyStopLossCheckBoxOnUnChecked;

            NewRow();
            _grid.AddChild(_doNotApplyStopLossCheckBox, _rowIndex, 0, 1, 5);

            //--ROW 15
            //Do not apply take profit CheckBox (Column 0-4)
        
            _doNotApplyTakeProfitCheckBox = MakeCheckBox("Do not apply take profit");
            _doNotApplyTakeProfitCheckBox.Checked += DoNotApplyTakeProfitCheckBoxOnChecked;
            _doNotApplyTakeProfitCheckBox.Unchecked += DoNotApplyTakeProfitCheckBoxOnUnChecked;
        
            NewRow();
            _grid.AddChild(_doNotApplyTakeProfitCheckBox, _rowIndex, 0, 1, 5);
        }

        //--ROW 16
        //Ask for confirmation CheckBox (Column 0-4)
        
        _askForConfirmationCheckBox = MakeCheckBox("Ask for confirmation");
        _askForConfirmationCheckBox.Checked += AskForConfirmationCheckBoxOnChecked;
        _askForConfirmationCheckBox.Unchecked += AskForConfirmationCheckBoxOnUnChecked;

        NewRow();
        _grid.AddChild(_askForConfirmationCheckBox, _rowIndex, 0, 1, 5);
        
        //--ROW 17
        //EarnForex TextBlock (Column 0)
        
        var earnForexTextBlock = MakeTextBlock("www.earnforex.com");
        earnForexTextBlock.FontSize = 10;
        earnForexTextBlock.ForegroundColor = InputDarkMode ? Color.LightGreen : Color.Green;

        NewRow();
        _grid.AddChild(earnForexTextBlock, _rowIndex, 0, 1, 5);

        foreach (var t in _grid.Rows)
        {
            t.SetHeightToAuto();
        }
        
        foreach (var t in _grid.Columns)
        {
            t.SetWidthToAuto();
        }
    }

    public TpDistribution TpDistribution { get; set; }

    private void LabelTextBoxOnValueUpdated(object sender, ControlValueUpdatedEventArgs<string> e)
    {
        LabelValueChanged?.Invoke(this, new LabelValueChangedEventArgs(e.Value));
    }

    public void UpdateValues(IModel model)
    {
        _trailingStopTextBox.SetValueWithoutTriggeringEvent(model.TrailingStopPips);
        _breakevenTextBox.SetValueWithoutTriggeringEvent(model.BreakEvenPips);
        _labelTextBox.SetValueWithoutTriggeringEvent(model.Label);
        _expiryTextBox.SetValueWithoutTriggeringEvent(model.ExpirationSeconds);
        _orderCommentTextBox.SetValueWithoutTriggeringEvent(model.Comment);
        _autoSuffixCheckBox.IsChecked = model.AutoSuffix;

        if (InputShowMaxParametersOnTradingTab)
        {
            _maxTradesTotalTextBox.SetValueWithoutTriggeringEvent(model.MaxNumberOfTradesTotal);
            _maxTradesPerSymbolTextBox.SetValueWithoutTriggeringEvent(model.MaxNumberOfTradesPerSymbol);
            _maxVolumeTotalTextBox.SetValueWithoutTriggeringEvent(model.MaxLotsTotal);
            _maxVolumePerSymbolTextBox.SetValueWithoutTriggeringEvent(model.MaxLotsPerSymbol);
            _maxRiskTotalTextBox.SetValueWithoutTriggeringEvent(model.MaxRiskPctTotal);
            _maxRiskPerSymbolTextBox.SetValueWithoutTriggeringEvent(model.MaxRiskPctPerSymbol);   
        }

        if (InputShowCheckBoxesOnTradingTab)
        {
            _subtractOpenPositionsVolumeCheckBox.IsChecked = model.SubtractOpenPositionsVolume;
            _subtractPendingOrdersVolumeCheckBox.IsChecked = model.SubtractPendingOrdersVolume;
            _doNotApplyStopLossCheckBox.IsChecked = model.DoNotApplyStopLoss;
            _doNotApplyTakeProfitCheckBox.IsChecked = model.DoNotApplyTakeProfit;
        }
        
        _disableTradingCheckBox.IsChecked = model.DisableTradingWhenLinesAreHidden;
        
        if (InputShowTradingFusesOnTradingTab)
        {
            _maxSlippageTextBox.SetValueWithoutTriggeringEvent(model.MaxSlippagePips);
            _maxSpreadTextBox.SetValueWithoutTriggeringEvent(model.MaxSpreadPips);
            _maxEntrySlDistanceTextBox.SetValueWithoutTriggeringEvent(model.MaxEntryStopLossDistancePips);
            _minEntrySlDistanceTextBox.SetValueWithoutTriggeringEvent(model.MinEntryStopLossDistancePips);   
            _maxRiskPercentageTextBox.SetValueWithoutTriggeringEvent(model.MaxRiskPercentage);
        }
        
        _askForConfirmationCheckBox.IsChecked = model.AskForConfirmation;
    }
    
    private readonly Grid _grid;

    private void AskForConfirmationCheckBoxOnUnChecked(CheckBoxEventArgs obj)
    {
        AskForConfirmationCheckBoxChanged?.Invoke(this, new AskForConfirmationCheckBoxChangedEventArgs(false));
    }

    private void AskForConfirmationCheckBoxOnChecked(CheckBoxEventArgs obj)
    { 
        AskForConfirmationCheckBoxChanged?.Invoke(this, new AskForConfirmationCheckBoxChangedEventArgs(true));
    }

    private void DoNotApplyTakeProfitCheckBoxOnUnChecked(CheckBoxEventArgs obj)
    {
        DoNotApplyTakeProfitCheckBoxChanged?.Invoke(this, new DoNotApplyTakeProfitCheckBoxChangedEventArgs(false));
    }

    private void DoNotApplyTakeProfitCheckBoxOnChecked(CheckBoxEventArgs obj)
    {
        DoNotApplyTakeProfitCheckBoxChanged?.Invoke(this, new DoNotApplyTakeProfitCheckBoxChangedEventArgs(true));
    }

    private void DoNotApplyStopLossCheckBoxOnUnChecked(CheckBoxEventArgs obj)
    {
        DoNotApplyStopLossCheckBoxChanged?.Invoke(this, new DoNotApplyStopLossCheckBoxChangedEventArgs(false));
    }

    private void DoNotApplyStopLossCheckBoxOnChecked(CheckBoxEventArgs obj)
    {
        DoNotApplyStopLossCheckBoxChanged?.Invoke(this, new DoNotApplyStopLossCheckBoxChangedEventArgs(true));
    }

    private void SubtractPendingOrdersVolumeCheckBoxOnChecked(CheckBoxEventArgs obj)
    {
        SubtractPendingOrdersVolumeCheckBoxChanged?.Invoke(this, new SubtractPendingOrdersVolumeCheckBoxChangedEventArgs(true));
    }
    
    private void SubtractPendingOrdersVolumeCheckBoxOnUnChecked(CheckBoxEventArgs obj)
    {
        SubtractPendingOrdersVolumeCheckBoxChanged?.Invoke(this, new SubtractPendingOrdersVolumeCheckBoxChangedEventArgs(false));
    }

    private void SubtractOpenPositionsVolumeCheckBoxOnUnChecked(CheckBoxEventArgs obj)
    {
        SubtractOpenPositionsVolumeCheckBoxChanged?.Invoke(this, new SubtractOpenPositionsVolumeCheckBoxChangedEventArgs(false));
    }

    private void SubtractOpenPositionsVolumeCheckBoxOnChecked(CheckBoxEventArgs obj)
    {
        SubtractOpenPositionsVolumeCheckBoxChanged?.Invoke(this, new SubtractOpenPositionsVolumeCheckBoxChangedEventArgs(true));
    }
    
    private void MinEntrySlDistanceTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MinEntrySlDistanceValueChanged?.Invoke(this, new MinEntrySlDistanceValueChangedEventArgs(e.Value));
    }
    
    private void MaxEntrySlDistanceTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxEntrySlDistanceValueChanged?.Invoke(this, new MaxEntrySlDistanceValueChangedEventArgs(e.Value));
    }
    
    private void MaxRiskPercentageTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxRiskPercentageValueChanged?.Invoke(this, new MaxRiskPercentageValueChangedEventArgs(e.Value));
    }
    
    private void MaxSpreadTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxSpreadValueChanged?.Invoke(this, new MaxSpreadValueSpreadEventArgs(e.Value));
    }
    
    private void MaxSlippageTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxSlippageValueChanged?.Invoke(this, new MaxSlippageValueChangedEventArgs(e.Value));
    }

    private void DisableTradingCheckBoxOnUnchecked(CheckBoxEventArgs obj)
    {
        DisableTradingWhenLinesHiddenCheckBoxChanged?.Invoke(this, new DisableTradingWhenLinesHiddenEventArgs(false));
    }

    private void DisableTradingCheckBoxOnChecked(CheckBoxEventArgs obj)
    {
        DisableTradingWhenLinesHiddenCheckBoxChanged?.Invoke(this, new DisableTradingWhenLinesHiddenEventArgs(true));
    }
    
    private void MaxRiskPerSymbolTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxRiskPerSymbolValueChanged?.Invoke(this, new MaxRiskPerSymbolValueChangedEventArgs(e.Value));
    }
    
    private void MaxRiskTotalTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxRiskTotalValueChanged?.Invoke(this, new MaxRiskTotalValueChangedEventArgs(e.Value));
    }
    
    private void MaxVolumePerSymbolTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxVolumePerSymbolValueChanged?.Invoke(this, new MaxVolumePerSymbolValueChangedEventArgs(e.Value));
    }
    
    private void MaxVolumeTotalTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        MaxVolumeTotalValueChanged?.Invoke(this, new MaxVolumeTotalValueChangedEventArgs(e.Value));
    }
    
    private void MaxTradesPerSymbolTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<int> e)
    {
        MaxNumberOfTradesPerSymbolValueChanged?.Invoke(this, new MaxNumberOfTradesPerSymbolEventArgs(e.Value));
    }
    
    private void MaxTradesTotalTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<int> e)
    {
        MaxNumberOfTradesTotalValueChanged?.Invoke(this, new MaxNumberOfTradesTotalValueChangedEventArgs(e.Value));
    }

    private void AutoSuffixCheckBoxOnUnchecked(CheckBoxEventArgs obj)
    {
        AutoSuffixValueChanged?.Invoke(this, new AutoSuffixValueChangedEventArgs(false));
    }

    private void AutoSuffixCheckBoxOnChecked(CheckBoxEventArgs obj)
    {
        AutoSuffixValueChanged?.Invoke(this, new AutoSuffixValueChangedEventArgs(true));
    }
    
    private void OrderCommentTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<string> e)
    {
        OrderCommentValueChanged?.Invoke(this, new OrderCommentValueChangedEventArgs(e.Value));
    }
    
    private void ExpiryTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<int> e)
    {
        ExpiryValueChanged?.Invoke(this, new ExpiryValueChangedEventArgs(e.Value));
    }

    private void BreakevenTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        BreakevenValueChanged?.Invoke(this, new BreakevenValueChangedEventArgs(e.Value));
    }
    
    private void TrailingStopTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        TrailingStopValueChanged?.Invoke(this, new TrailingStopValueChangedEventArgs(e.Value));
    }

    private void TradeButtonOnClick(ButtonClickEventArgs obj)
    {
        OnTradeButtonClicked();
    }

    public Button MakeButton(string text)
    {
        return new Button
        {
            Text = text,
            Style = CustomStyle.ButtonStyle, 
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            //Margin = new Thickness(5)
        };
    }

    public TextBlock MakeTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            ForegroundColor = Color.Black,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(1)
        };
    }

    private CheckBox MakeCheckBox(string text)
    {
        return new CheckBox
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Style = CustomStyle.CheckBoxStyle,
            Margin = new Thickness(5)
        };
    }

    private void OnTradeButtonClicked()
    {
        TradeButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    public CustomStyle CustomStyle => _resources.CustomStyle;
    public bool InputShowMaxParametersOnTradingTab => _resources.InputShowMaxParametersOnTradingTab;
    public bool InputShowTradingFusesOnTradingTab => _resources.InputShowTradingFusesOnTradingTab;
    public bool InputShowCheckBoxesOnTradingTab => _resources.InputShowCheckBoxesOnTradingTab;
    public void Print(object obj)
    {
        _resources.Print(obj);
    }

    public XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxDouble(defaultValue, digits, valueUpdatedHandler);
    }

    public XTextBoxDoubleNumeric MakeTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxDoubleNumeric(defaultValue, digits, changeByFactor, valueUpdatedHandler);
    }

    public XTextBoxInt MakeTextBoxInt(int defaultValue, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxInt(defaultValue, valueUpdatedHandler);
    }

    public XTextBoxIntNumericUpDown MakeTextBoxIntNumeric(int defaultValue, int changeByFactor, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxIntNumeric(defaultValue, changeByFactor, valueUpdatedHandler);
    }

    public XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxString(defaultValue, valueUpdatedHandler);
    }

    public bool InputDarkMode => _resources.InputDarkMode;
    public Color InputTradeButtonColor => _resources.InputTradeButtonColor;

    public void Dispose()
    {
        //unsubscribe from all events
        _tradeButton.Click -= TradeButtonOnClick;
        _trailingStopTextBox.ValueUpdated -= TrailingStopTextBoxOnTextChanged;
        _breakevenTextBox.ValueUpdated -= BreakevenTextBoxOnTextChanged;
        _labelTextBox.ValueUpdated -= LabelTextBoxOnValueUpdated;
        _expiryTextBox.ValueUpdated -= ExpiryTextBoxOnTextChanged;
        _orderCommentTextBox.ValueUpdated -= OrderCommentTextBoxOnTextChanged;
        _autoSuffixCheckBox.Checked -= AutoSuffixCheckBoxOnChecked;
        _autoSuffixCheckBox.Unchecked -= AutoSuffixCheckBoxOnUnchecked;
        
        if (InputShowMaxParametersOnTradingTab)
        {
            _maxTradesTotalTextBox.ValueUpdated -= MaxTradesTotalTextBoxOnTextChanged;
            _maxTradesPerSymbolTextBox.ValueUpdated -= MaxTradesPerSymbolTextBoxOnTextChanged;
            _maxVolumeTotalTextBox.ValueUpdated -= MaxVolumeTotalTextBoxOnTextChanged;
            _maxVolumePerSymbolTextBox.ValueUpdated -= MaxVolumePerSymbolTextBoxOnTextChanged;
            _maxRiskTotalTextBox.ValueUpdated -= MaxRiskTotalTextBoxOnTextChanged;
            _maxRiskPerSymbolTextBox.ValueUpdated -= MaxRiskPerSymbolTextBoxOnTextChanged;
        }

        if (InputShowTradingFusesOnTradingTab)
        {
            _maxSlippageTextBox.ValueUpdated -= MaxSlippageTextBoxOnTextChanged;
            _maxSpreadTextBox.ValueUpdated -= MaxSpreadTextBoxOnTextChanged;
            _minEntrySlDistanceTextBox.ValueUpdated -= MinEntrySlDistanceTextBoxOnTextChanged;
            _maxEntrySlDistanceTextBox.ValueUpdated -= MaxEntrySlDistanceTextBoxOnTextChanged;
            _maxRiskPercentageTextBox.ValueUpdated -= MaxRiskPercentageTextBoxOnTextChanged;
        }
        
        _disableTradingCheckBox.Checked -= DisableTradingCheckBoxOnChecked;
        _disableTradingCheckBox.Unchecked -= DisableTradingCheckBoxOnUnchecked;
        _askForConfirmationCheckBox.Checked -= AskForConfirmationCheckBoxOnChecked;
        _askForConfirmationCheckBox.Unchecked -= AskForConfirmationCheckBoxOnUnChecked;

        if (InputShowCheckBoxesOnTradingTab)
        {
            _doNotApplyTakeProfitCheckBox.Checked -= DoNotApplyTakeProfitCheckBoxOnChecked;
            _doNotApplyTakeProfitCheckBox.Unchecked -= DoNotApplyTakeProfitCheckBoxOnUnChecked;
            _doNotApplyStopLossCheckBox.Checked -= DoNotApplyStopLossCheckBoxOnChecked;
            _doNotApplyStopLossCheckBox.Unchecked -= DoNotApplyStopLossCheckBoxOnUnChecked;
            _subtractPendingOrdersVolumeCheckBox.Checked -= SubtractPendingOrdersVolumeCheckBoxOnChecked;
            _subtractPendingOrdersVolumeCheckBox.Unchecked -= SubtractPendingOrdersVolumeCheckBoxOnUnChecked;
            _subtractOpenPositionsVolumeCheckBox.Checked -= SubtractOpenPositionsVolumeCheckBoxOnChecked;
            _subtractOpenPositionsVolumeCheckBox.Unchecked -= SubtractOpenPositionsVolumeCheckBoxOnUnChecked;
        }
        

        _tradeButton.Click -= TradeButtonOnClick;
        TpDistribution.ClearEvents();
    }

    public void TrySaveTextBoxesContent()
    {
        _trailingStopTextBox.TryValidateText();
        _breakevenTextBox.TryValidateText();
        _labelTextBox.TryValidateText();
        _expiryTextBox.TryValidateText();
        _orderCommentTextBox.TryValidateText();
        
        if (InputShowMaxParametersOnTradingTab)
        {
            _maxTradesTotalTextBox.TryValidateText();
            _maxTradesPerSymbolTextBox.TryValidateText();
            _maxVolumeTotalTextBox.TryValidateText();
            _maxVolumePerSymbolTextBox.TryValidateText();
            _maxRiskTotalTextBox.TryValidateText();
            _maxRiskPerSymbolTextBox.TryValidateText();
        }

        if (InputShowTradingFusesOnTradingTab)
        {
            _maxSlippageTextBox.TryValidateText();
            _maxSpreadTextBox.TryValidateText();
            _minEntrySlDistanceTextBox.TryValidateText();
            _maxEntrySlDistanceTextBox.TryValidateText();
            _maxRiskPercentageTextBox.TryValidateText();
        }
        
        foreach (var tpRow in TpDistribution.TpRows)
        {
            tpRow.PriceTextBox.TryValidateText();
            tpRow.PercentageTextBox.TryValidateText();
        }
    }
}

public class AskForConfirmationCheckBoxChangedEventArgs : EventArgs
{
    public bool AskForConfirmation { get; }
    
    public AskForConfirmationCheckBoxChangedEventArgs(bool askForConfirmation)
    {
        AskForConfirmation = askForConfirmation;
    }
}

public class DoNotApplyTakeProfitCheckBoxChangedEventArgs : EventArgs
{
    public bool DoNotApplyTakeProfit { get; }
    
    public DoNotApplyTakeProfitCheckBoxChangedEventArgs(bool doNotApplyTakeProfit)
    {
        DoNotApplyTakeProfit = doNotApplyTakeProfit;
    }
}

public class DoNotApplyStopLossCheckBoxChangedEventArgs : EventArgs
{
    public bool DoNotApplyStopLoss { get; }
    
    public DoNotApplyStopLossCheckBoxChangedEventArgs(bool doNotApplyStopLoss)
    {
        DoNotApplyStopLoss = doNotApplyStopLoss;
    }
}

public class SubtractPendingOrdersVolumeCheckBoxChangedEventArgs : EventArgs
{
    public bool SubtractPendingOrdersVolume { get; }
    
    public SubtractPendingOrdersVolumeCheckBoxChangedEventArgs(bool subtractPendingOrdersVolume)
    {
        SubtractPendingOrdersVolume = subtractPendingOrdersVolume;
    }
}

public class SubtractOpenPositionsVolumeCheckBoxChangedEventArgs : EventArgs
{
    public bool SubtractOpenPositionsVolume { get; }
    
    public SubtractOpenPositionsVolumeCheckBoxChangedEventArgs(bool subtractOpenPositionsVolume)
    {
        SubtractOpenPositionsVolume = subtractOpenPositionsVolume;
    }
}

public class MinEntrySlDistanceValueChangedEventArgs
{
    public double MinEntrySlDistancePips { get; }
    
    public MinEntrySlDistanceValueChangedEventArgs(double minEntrySlDistancePips)
    {
        MinEntrySlDistancePips = minEntrySlDistancePips;
    }
}

public class MaxEntrySlDistanceValueChangedEventArgs : EventArgs
{
    public double MaxEntrySlDistancePips { get; }
    
    public MaxEntrySlDistanceValueChangedEventArgs(double maxEntrySlDistancePips)
    {
        MaxEntrySlDistancePips = maxEntrySlDistancePips;
    }
}

public class MaxRiskPercentageValueChangedEventArgs : EventArgs
{
    public double MaxRiskPercentage { get; }
    
    public MaxRiskPercentageValueChangedEventArgs(double maxRiskPercentage)
    {
        MaxRiskPercentage = maxRiskPercentage;
    }
}

public class MaxSpreadValueSpreadEventArgs : EventArgs
{
    public double MaxSpreadPips { get; }

    public MaxSpreadValueSpreadEventArgs(double maxSpreadPips)
    {
        MaxSpreadPips = maxSpreadPips;
    }
}

public class MaxSlippageValueChangedEventArgs : EventArgs
{
    public double MaxSlippagePips { get; }
    
    public MaxSlippageValueChangedEventArgs(double maxSlippagePips)
    {
        MaxSlippagePips = maxSlippagePips;
    }
}

public class DisableTradingWhenLinesHiddenEventArgs : EventArgs
{
    public bool DisableTradingWhenLinesHidden { get; }
    
    public DisableTradingWhenLinesHiddenEventArgs(bool disableTradingWhenLinesHidden)
    {
        DisableTradingWhenLinesHidden = disableTradingWhenLinesHidden;
    }
}

public class MaxRiskPerSymbolValueChangedEventArgs : EventArgs
{
    public double MaxRiskPerSymbol { get; }
    
    public MaxRiskPerSymbolValueChangedEventArgs(double maxRiskPerSymbol)
    {
        MaxRiskPerSymbol = maxRiskPerSymbol;
    }
}

public class MaxRiskTotalValueChangedEventArgs : EventArgs
{
    public double MaxRiskTotal { get; }
    
    public MaxRiskTotalValueChangedEventArgs(double maxRiskTotal)
    {
        MaxRiskTotal = maxRiskTotal;
    }
}

public class MaxVolumePerSymbolValueChangedEventArgs : EventArgs
{
    public double MaxVolumePerSymbol { get; }
    
    public MaxVolumePerSymbolValueChangedEventArgs(double maxVolumePerSymbol)
    {
        MaxVolumePerSymbol = maxVolumePerSymbol;
    }
}

public class MaxVolumeTotalValueChangedEventArgs : EventArgs
{
    public double MaxVolumeTotal { get; }
    
    public MaxVolumeTotalValueChangedEventArgs(double maxVolumeTotal)
    {
        MaxVolumeTotal = maxVolumeTotal;
    }
}

public class MaxNumberOfTradesPerSymbolEventArgs : EventArgs
{
    public int MaxTradesPerSymbol { get; }
    
    public MaxNumberOfTradesPerSymbolEventArgs(int maxTradesPerSymbol)
    {
        MaxTradesPerSymbol = maxTradesPerSymbol;
    }
}

public class MaxNumberOfTradesTotalValueChangedEventArgs : EventArgs
{
    public int MaxTradesTotal { get; }
    
    public MaxNumberOfTradesTotalValueChangedEventArgs(int maxTradesTotal)
    {
        MaxTradesTotal = maxTradesTotal;
    }
}

public class AutoSuffixValueChangedEventArgs : EventArgs
{
    public bool AutoSuffix { get; }
    
    public AutoSuffixValueChangedEventArgs(bool autoSuffix)
    {
        AutoSuffix = autoSuffix;
    }
}

public class OrderCommentValueChangedEventArgs : EventArgs
{
    public string OrderComment { get; }
    
    public OrderCommentValueChangedEventArgs(string orderComment)
    {
        OrderComment = orderComment;
    }
}

public class ExpiryValueChangedEventArgs : EventArgs
{
    public int Expiry { get; }
    
    public ExpiryValueChangedEventArgs(int expiry)
    {
        Expiry = expiry;
    }
}

public class LabelValueChangedEventArgs : EventArgs
{
    public string Label { get; }
    
    public LabelValueChangedEventArgs(string label)
    {
        Label = label;
    }
}

public class BreakevenValueChangedEventArgs : EventArgs 
{
    public double Breakeven { get; }
    
    public BreakevenValueChangedEventArgs(double breakeven)
    {
        Breakeven = breakeven;
    }
}

public class TrailingStopValueChangedEventArgs : EventArgs
{
    public double TrailingStop { get; }
    
    public TrailingStopValueChangedEventArgs(double trailingStop)
    {
        TrailingStop = trailingStop;
    }
}