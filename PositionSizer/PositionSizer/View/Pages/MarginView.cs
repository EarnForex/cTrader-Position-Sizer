using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public interface IMarginViewResources
{
    IAccount Account { get; }
    Symbol Symbol { get; }
    CustomStyle CustomStyle { get; }
    IAssetConverter AssetConverter { get; }
    bool InputShowAdditionalMarginSettings { get; }
    XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxDoubleNumeric MakeTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxInt MakeTextBoxInt(int defaultValue, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxIntNumericUpDown MakeTextBoxIntNumeric(int defaultValue, int changeByFactor, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler);
    bool InputDarkMode { get; }
}

public class LeverageDisplayChangedEventArgs : EventArgs
{
    public double Leverage { get; }

    public LeverageDisplayChangedEventArgs(double leverage)
    {
        Leverage = leverage;
    }
}

public class MarginUtilizationBaseChangedEventArgs : EventArgs
{
    public MarginUtilizationBase Base { get; }

    public MarginUtilizationBaseChangedEventArgs(MarginUtilizationBase @base)
    {
        Base = @base;
    }
}

public class MarginView : Button, IMarginViewResources
{
    private const int UtilizationColumnWidth = 85;
    private const int UtilizationTextBoxWriteAreaWidth = UtilizationColumnWidth - 4;
    /// <summary>
    /// Pixel width of N utilization textboxes in adjacent <see cref="UtilizationColumnWidth"/> columns.
    /// Each box is <see cref="UtilizationTextBoxWriteAreaWidth"/> wide and left-aligned, leaving 4 px
    /// slack per column; only the last column's trailing slack is excluded from the combined span.
    /// </summary>
    private const int CombinedTwoUtilizationTextBoxesWidth = UtilizationColumnWidth * 2 - 4;
    private const int CombinedThreeUtilizationTextBoxesWidth = UtilizationColumnWidth * 3 - 4;
    private const int CompactValueColumnWidth = 110;
    private const int CompactTrailingColumnWidth = 130;
    private const int CombinedCompactValueColumnsWidth = CompactValueColumnWidth + CompactTrailingColumnWidth - 4;
    private const int LeverageInfoColumn = 3;
    private const string MarginUtilBaseGroupName = "MarginUtilBase";

    private readonly IMarginViewResources _resources;
    private readonly bool _showAdditionalMarginSettings;

    /// <summary>
    /// Position margin shows the margin that will be used for the calculated position.
    /// Negative value means that the future used margin will be lower than the current
    /// due to lower requirement for margin of the hedged positions.
    /// </summary>
    private readonly XTextBoxDouble _positionMarginTextBox;

    /// <summary>
    /// Future used margin is calculated based on the current used margin and position margin.
    /// </summary>
    private readonly XTextBoxDouble _futureUsedMarginTextBox;

    /// <summary>
    /// Future free margin shows how much free margin you will have left after opening the
    /// calculated position.
    /// </summary>
    private readonly XTextBoxDouble _futureFreeMarginTextBox;
    private readonly TextBlock _futureFreeMarginTextBlock;

    /// <summary>
    /// Maximum position size by margin displays the biggest trade you can take with your currently
    /// available free margin and leverage.
    /// </summary>
    private readonly XTextBoxDouble _maxPositionSizeByMarginTextBox;

    /// <summary>
    /// Custom leverage input lets you set your own leverage
    /// for all the margin calculations done by this expert advisor.
    /// </summary>
    private readonly XTextBoxDouble _customLeverageTextBox;

    private readonly XTextBoxDouble _marginUtilizedCurrentTextBox;
    private readonly XTextBoxDouble _marginUtilizedPositionTextBox;
    private readonly XTextBoxDouble _marginUtilizedFutureTextBox;
    private readonly RadioButton _balanceBaseRadioButton;
    private readonly RadioButton _startingBalanceBaseRadioButton;
    private readonly RadioButton _freeMarginBaseRadioButton;
    private readonly TextBlock _baseValueLabelTextBlock;
    private readonly XTextBoxDouble _baseValueTextBox;

    private readonly Grid _grid;
    private bool _isUpdating;

    public event EventHandler<LeverageDisplayChangedEventArgs> LeverageDisplayChanged;
    public event EventHandler<MarginUtilizationBaseChangedEventArgs> MarginUtilizationBaseChanged;
    public event EventHandler<ControlValueUpdatedEventArgs<double>> MubStartingBalanceChanged;

    public MarginView(IMarginViewResources resources)
    {
        _resources = resources;
        _showAdditionalMarginSettings = resources.InputShowAdditionalMarginSettings;
        _grid = new Grid();
        Content = _grid;

        var columnCount = _showAdditionalMarginSettings ? 5 : 4;
        var rowCount = _showAdditionalMarginSettings ? 11 : 7;

        _grid.AddColumns(columnCount);
        _grid.AddRows(rowCount);
        Width = 400;

        var row = 0;

        var positionMarginTextBlock = MakeTextBlock("Position Margin:");

        _grid.AddChild(positionMarginTextBlock, row, 0, 1, 2);

        _positionMarginTextBox = new XTextBoxDouble(0, 2);
        _positionMarginTextBox.SetCustomStyle(CustomStyle);
        _positionMarginTextBox.IsReadOnly = true;
        _positionMarginTextBox.ChangeWriteAreaWidth(
            _showAdditionalMarginSettings ? CombinedThreeUtilizationTextBoxesWidth : CombinedCompactValueColumnsWidth);
        _positionMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _positionMarginTextBox.TextAlignment = TextAlignment.Right;

        _grid.AddChild(_positionMarginTextBox, row, 2, 1, columnCount - 2);

        row++;

        var futureUsedMarginTextBlock = MakeTextBlock("Future Used Margin:");

        _grid.AddChild(futureUsedMarginTextBlock, row, 0, 1, 2);

        _futureUsedMarginTextBox = new XTextBoxDouble(0, 2);
        _futureUsedMarginTextBox.SetCustomStyle(CustomStyle);
        _futureUsedMarginTextBox.IsReadOnly = true;
        _futureUsedMarginTextBox.ChangeWriteAreaWidth(
            _showAdditionalMarginSettings ? CombinedThreeUtilizationTextBoxesWidth : CombinedCompactValueColumnsWidth);
        _futureUsedMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _futureUsedMarginTextBox.TextAlignment = TextAlignment.Right;

        _grid.AddChild(_futureUsedMarginTextBox, row, 2, 1, columnCount - 2);

        row++;

        _futureFreeMarginTextBlock = MakeTextBlock("Future Free Margin:");

        _grid.AddChild(_futureFreeMarginTextBlock, row, 0, 1, 2);

        _futureFreeMarginTextBox = new XTextBoxDouble(0, 2);
        _futureFreeMarginTextBox.SetCustomStyle(CustomStyle);
        _futureFreeMarginTextBox.IsReadOnly = true;
        _futureFreeMarginTextBox.ChangeWriteAreaWidth(
            _showAdditionalMarginSettings ? CombinedThreeUtilizationTextBoxesWidth : CombinedCompactValueColumnsWidth);
        _futureFreeMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _futureFreeMarginTextBox.TextAlignment = TextAlignment.Right;

        _grid.AddChild(_futureFreeMarginTextBox, row, 2, 1, columnCount - 2);

        row++;

        var customLeverageTextBlock = MakeTextBlock("Custom Leverage = 1:");

        _grid.AddChild(customLeverageTextBlock, row, 0, 1, 2);

        _customLeverageTextBox = MakeTextBoxDouble(0, 2, OnCustomLeverageTextBoxOnTextUpdatedAndValid);
        _customLeverageTextBox.ChangeWriteAreaWidth(UtilizationTextBoxWriteAreaWidth);
        _customLeverageTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _customLeverageTextBox.VerticalAlignment = VerticalAlignment.Center;

        _grid.AddChild(_customLeverageTextBox, row, 2, 1, _showAdditionalMarginSettings ? 2 : 1);

        var defaultLeverageTextBlock = MakeTextBlock($"(Default = 1:{Account.PreciseLeverage})");

        defaultLeverageTextBlock.Margin = new Thickness(0, 0, 0, 0);

        _grid.AddChild(defaultLeverageTextBlock, row, LeverageInfoColumn);

        row++;

        var marginLevelTextBlock = MakeTextBlock($"(Symbol = 1:{Symbol.DynamicLeverage[0].Leverage})");

        marginLevelTextBlock.Margin = new Thickness(0, 0, 0, 0);

        _grid.AddChild(marginLevelTextBlock, row, LeverageInfoColumn);

        row++;

        var maxPositionSizeByMargin = MakeTextBlock("Max Pos. Size By Margin:");

        _grid.AddChild(maxPositionSizeByMargin, row, 0, 1, 2);

        _maxPositionSizeByMarginTextBox = MakeTextBoxDouble(0, 2, (_, _) => { });
        _maxPositionSizeByMarginTextBox.SetCustomStyle(CustomStyle);
        _maxPositionSizeByMarginTextBox.ChangeWriteAreaWidth(UtilizationTextBoxWriteAreaWidth);
        _maxPositionSizeByMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _maxPositionSizeByMarginTextBox.TextAlignment = TextAlignment.Right;

        _grid.AddChild(_maxPositionSizeByMarginTextBox, row, 2, 1, _showAdditionalMarginSettings ? 2 : 1);

        row++;

        if (_showAdditionalMarginSettings)
        {
            var currentHeaderTextBlock = MakeTextBlock("Current");
            currentHeaderTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            _grid.AddChild(currentHeaderTextBlock, row, 2);

            var positionHeaderTextBlock = MakeTextBlock("Position");
            positionHeaderTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            _grid.AddChild(positionHeaderTextBlock, row, 3);

            var futureHeaderTextBlock = MakeTextBlock("Future");
            futureHeaderTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            _grid.AddChild(futureHeaderTextBlock, row, 4);

            row++;

            var marginUtilizedPercTextBlock = MakeTextBlock("Margin utilization, %");
            _grid.AddChild(marginUtilizedPercTextBlock, row, 0, 1, 2);

            _marginUtilizedCurrentTextBox = MakeUtilizationTextBox();
            _grid.AddChild(_marginUtilizedCurrentTextBox, row, 2);

            _marginUtilizedPositionTextBox = MakeUtilizationTextBox();
            _grid.AddChild(_marginUtilizedPositionTextBox, row, 3);

            _marginUtilizedFutureTextBox = MakeUtilizationTextBox();
            _grid.AddChild(_marginUtilizedFutureTextBox, row, 4);

            row++;

            var baseTextBlock = MakeTextBlock("Base:");
            _grid.AddChild(baseTextBlock, row, 0, 1, 2);

            _balanceBaseRadioButton = MakeBaseRadioButton("Balance", MarginUtilizationBase.Balance);
            _startingBalanceBaseRadioButton = MakeBaseRadioButton("Start. balance", MarginUtilizationBase.StartingBalance);
            _freeMarginBaseRadioButton = MakeBaseRadioButton("Free margin", MarginUtilizationBase.FreeMargin);

            var baseRadioPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            baseRadioPanel.AddChild(_balanceBaseRadioButton);
            baseRadioPanel.AddChild(_startingBalanceBaseRadioButton);
            baseRadioPanel.AddChild(_freeMarginBaseRadioButton);

            _grid.AddChild(baseRadioPanel, row, 2, 1, 3);

            row++;

            _baseValueLabelTextBlock = MakeTextBlock($"Base value, {Account.Asset.Name}:");
            _grid.AddChild(_baseValueLabelTextBlock, row, 0, 1, 2);

            _baseValueTextBox = MakeTextBoxDouble(0, Account.Asset.Digits, OnBaseValueTextBoxOnTextUpdatedAndValid);
            _baseValueTextBox.SetCustomStyle(CustomStyle);
            _baseValueTextBox.ChangeWriteAreaWidth(CombinedTwoUtilizationTextBoxesWidth);
            _baseValueTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            _baseValueTextBox.TextAlignment = TextAlignment.Right;

            _grid.AddChild(_baseValueTextBox, row, 2, 1, 2);

            row++;
        }
        else
        {
            _marginUtilizedCurrentTextBox = null;
            _marginUtilizedPositionTextBox = null;
            _marginUtilizedFutureTextBox = null;
            _balanceBaseRadioButton = null;
            _startingBalanceBaseRadioButton = null;
            _freeMarginBaseRadioButton = null;
            _baseValueLabelTextBlock = null;
            _baseValueTextBox = null;
        }

        var earnForexTextBlock = MakeTextBlock("www.earnforex.com");
        earnForexTextBlock.FontSize = 10;
        earnForexTextBlock.ForegroundColor = InputDarkMode ? Color.LightGreen : Color.Green;

        _grid.AddChild(earnForexTextBlock, row, 0);

        for (var i = 0; i < row; i++)
            _grid.Rows[i].SetHeightInPixels(28);

        _grid.Columns[0].SetWidthInPixels(100);
        _grid.Columns[1].SetWidthInPixels(25);

        if (_showAdditionalMarginSettings)
        {
            _grid.Columns[2].SetWidthInPixels(UtilizationColumnWidth);
            _grid.Columns[3].SetWidthInPixels(UtilizationColumnWidth);
            _grid.Columns[4].SetWidthInPixels(UtilizationColumnWidth);
        }
        else
        {
            _grid.Columns[2].SetWidthInPixels(CompactValueColumnWidth);
            _grid.Columns[3].SetWidthInPixels(CompactTrailingColumnWidth);
        }
    }

    private XTextBoxDouble MakeUtilizationTextBox()
    {
        var textBox = MakeTextBoxDouble(0, 2, (_, _) => { });
        textBox.SetCustomStyle(CustomStyle);
        textBox.IsReadOnly = true;
        textBox.ChangeWriteAreaWidth(UtilizationTextBoxWriteAreaWidth);
        textBox.HorizontalAlignment = HorizontalAlignment.Left;
        textBox.TextAlignment = TextAlignment.Right;
        return textBox;
    }

    private RadioButton MakeBaseRadioButton(string text, MarginUtilizationBase baseMode)
    {
        var radioButton = new RadioButton
        {
            Text = text,
            GroupName = MarginUtilBaseGroupName,
            FontSize = 10,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Style = CustomStyle.CheckBoxStyle
        };

        radioButton.Checked += _ =>
        {
            if (_isUpdating || radioButton.IsChecked != true)
                return;

            MarginUtilizationBaseChanged?.Invoke(this, new MarginUtilizationBaseChangedEventArgs(baseMode));
        };

        return radioButton;
    }

    private void OnCustomLeverageTextBoxOnTextUpdatedAndValid(object sender, ControlValueUpdatedEventArgs<double> args)
    {
        LeverageDisplayChanged?.Invoke(this, new LeverageDisplayChangedEventArgs(args.Value));
    }

    private void OnBaseValueTextBoxOnTextUpdatedAndValid(object sender, ControlValueUpdatedEventArgs<double> args)
    {
        MubStartingBalanceChanged?.Invoke(this, args);
    }

    public void Update(IModel model)
    {
        _isUpdating = true;

        try
        {
            _customLeverageTextBox.SetValueWithoutTriggeringEvent(model.CustomLeverage);
            _positionMarginTextBox.SetValueWithoutTriggeringEvent(model.PositionMargin);
            _futureUsedMarginTextBox.SetValueWithoutTriggeringEvent(model.FutureUsedMargin);

            _futureFreeMarginTextBox.SetValueWithoutTriggeringEvent(model.FutureFreeMargin);
            _futureFreeMarginTextBlock.ForegroundColor = model.FutureFreeMargin >= 0 ? Color.Black : Color.Red;
            _futureFreeMarginTextBox.ForegroundColor = model.FutureFreeMargin >= 0 ? Color.Black : Color.Red;

            _maxPositionSizeByMarginTextBox.SetValueWithoutTriggeringEvent(model.MaxPositionSizeByMargin);

            if (!_showAdditionalMarginSettings)
                return;

            _marginUtilizedCurrentTextBox.SetValueWithoutTriggeringEvent(model.MarginUtilizedCurrent);
            _marginUtilizedPositionTextBox.SetValueWithoutTriggeringEvent(model.MarginUtilizedPosition);
            _marginUtilizedFutureTextBox.SetValueWithoutTriggeringEvent(model.MarginUtilizedFuture);

            SyncBaseRadioButtons(model.MarginUtilizationBase);
            UpdateBaseValueControls(model);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void SyncBaseRadioButtons(MarginUtilizationBase selectedBase)
    {
        _balanceBaseRadioButton.IsChecked = selectedBase == MarginUtilizationBase.Balance;
        _startingBalanceBaseRadioButton.IsChecked = selectedBase == MarginUtilizationBase.StartingBalance;
        _freeMarginBaseRadioButton.IsChecked = selectedBase == MarginUtilizationBase.FreeMargin;
    }

    private void UpdateBaseValueControls(IModel model)
    {
        _baseValueLabelTextBlock.Text = $"Base value, {Account.Asset.Name}:";

        var isStartingBalance = model.MarginUtilizationBase == MarginUtilizationBase.StartingBalance;
        _baseValueTextBox.IsReadOnly = !isStartingBalance;

        if (isStartingBalance)
            _baseValueTextBox.SetValueWithoutTriggeringEvent(model.MubStartingBalance);
        else
            _baseValueTextBox.SetValueWithoutTriggeringEvent(model.MarginUtilizationBaseValue);
    }

    private TextBlock MakeTextBlock(string text) =>
        new()
        {
            Text = text,
            ForegroundColor = Color.Black,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(1)
        };

    public IAccount Account => _resources.Account;
    public Symbol Symbol => _resources.Symbol;
    public CustomStyle CustomStyle => _resources.CustomStyle;
    public IAssetConverter AssetConverter => _resources.AssetConverter;
    public bool InputShowAdditionalMarginSettings => _resources.InputShowAdditionalMarginSettings;

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

    public void TrySaveTextBoxesContent()
    {
        _maxPositionSizeByMarginTextBox.TryValidateText();
        _customLeverageTextBox.TryValidateText();

        if (_showAdditionalMarginSettings && _baseValueTextBox is { IsReadOnly: false })
            _baseValueTextBox.TryValidateText();
    }
}
