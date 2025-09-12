using System;
using System.Globalization;
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

public class MarginView : Button, IMarginViewResources
{
    private readonly IMarginViewResources _resources;
    
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

    private readonly Grid _grid;

    public event EventHandler<LeverageDisplayChangedEventArgs> LeverageDisplayChanged;

    public MarginView(IMarginViewResources resources)
    {
        _resources = resources;
        _grid = new Grid();
        Content = _grid;
        
        _grid.AddColumns(4);
        _grid.AddRows(7);
        //_grid.BackgroundColor = Color.Red;
        //ShowGridLines = true;
        Width = 400;

        var row = 0;
        
        var positionMarginTextBlock = MakeTextBlock("Position Margin:");
        
        _grid.AddChild(positionMarginTextBlock, row, 0, 1, 2);

        _positionMarginTextBox = new XTextBoxDouble(0, 2);
        _positionMarginTextBox.SetCustomStyle(CustomStyle);
        _positionMarginTextBox.IsReadOnly = true;
        _positionMarginTextBox.ChangeWriteAreaWidth(200);
        _positionMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _positionMarginTextBox.TextAlignment = TextAlignment.Right;
        
        _grid.AddChild(_positionMarginTextBox, row, 2, 1, 3);
        
        row++;
        
        var futureUsedMarginTextBlock = MakeTextBlock("Future Used Margin:");
        
        _grid.AddChild(futureUsedMarginTextBlock, row, 0, 1, 2);
        
        _futureUsedMarginTextBox = new XTextBoxDouble(0, 2);
        _futureUsedMarginTextBox.SetCustomStyle(CustomStyle);
        _futureUsedMarginTextBox.IsReadOnly = true;
        _futureUsedMarginTextBox.ChangeWriteAreaWidth(200);
        _futureUsedMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _futureUsedMarginTextBox.TextAlignment = TextAlignment.Right;
        
        _grid.AddChild(_futureUsedMarginTextBox, row, 2, 1, 2);
        
        row++;
        
        _futureFreeMarginTextBlock = MakeTextBlock("Future Free Margin:");
        
        _grid.AddChild(_futureFreeMarginTextBlock, row, 0, 1, 2);
        
        _futureFreeMarginTextBox = new XTextBoxDouble(0, 2);
        _futureFreeMarginTextBox.SetCustomStyle(CustomStyle);
        _futureFreeMarginTextBox.IsReadOnly = true;
        _futureFreeMarginTextBox.ChangeWriteAreaWidth(200);
        _futureFreeMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _futureFreeMarginTextBox.TextAlignment = TextAlignment.Right;
        
        _grid.AddChild(_futureFreeMarginTextBox, row, 2, 1, 2);
        
        row++;
        
        var customLeverageTextBlock = MakeTextBlock("Custom Leverage = 1:");
        
        _grid.AddChild(customLeverageTextBlock, row, 0, 1, 2);

        _customLeverageTextBox = MakeTextBoxDouble(0, 2, OnCustomLeverageTextBoxOnTextUpdatedAndValid);
        _customLeverageTextBox.ChangeWriteAreaWidth(50);
        //_customLeverageTextBox.IsHitTestVisible = true;
        _customLeverageTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        _customLeverageTextBox.VerticalAlignment = VerticalAlignment.Center;
        
        _grid.AddChild(_customLeverageTextBox, row, 2, 1, 2);
        
        var defaultLeverageTextBlock = MakeTextBlock($"(Default = 1:{Account.PreciseLeverage})");

        defaultLeverageTextBlock.Margin = new Thickness(0, 0, 0, 0);
        
        _grid.AddChild(defaultLeverageTextBlock, row, 3);
        
        row++;
        
        var marginLevelTextBlock = MakeTextBlock($"(Symbol = 1:{Symbol.DynamicLeverage[0].Leverage})");
        
        marginLevelTextBlock.Margin = new Thickness(0, 0, 0, 0);
        
        _grid.AddChild(marginLevelTextBlock, row, 3);
        
        row++;
        
        var maxPositionSizeByMargin = MakeTextBlock($"Max Pos. Size By Margin:");
        
        _grid.AddChild(maxPositionSizeByMargin, row, 0, 1, 2);
        
        _maxPositionSizeByMarginTextBox = MakeTextBoxDouble(0, 2, (_, _) => { });
        _maxPositionSizeByMarginTextBox.SetCustomStyle(CustomStyle);
        _maxPositionSizeByMarginTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        
        _grid.AddChild(_maxPositionSizeByMarginTextBox, row, 2, 1, 1);
        
        row++;
        
        //earnforex.com
        var earnForexTextBlock = MakeTextBlock("www.earnforex.com");
        earnForexTextBlock.FontSize = 10;
        earnForexTextBlock.ForegroundColor = InputDarkMode ? Color.LightGreen : Color.Green;
        
        _grid.AddChild(earnForexTextBlock, row, 0);
        //_grid.ShowGridLines = true;
        
        for (var i = 0; i < row; i++)
        {
            _grid.Rows[i].SetHeightInPixels(28);
        }
        
        _grid.Columns[0].SetWidthInPixels(100);
        _grid.Columns[1].SetWidthInPixels(25);
        _grid.Columns[2].SetWidthInPixels(110);
        _grid.Columns[3].SetWidthInPixels(130);
    }

    private void OnCustomLeverageTextBoxOnTextUpdatedAndValid(object sender, ControlValueUpdatedEventArgs<double> args)
    {
        LeverageDisplayChanged?.Invoke(this, new LeverageDisplayChangedEventArgs(args.Value));
    }

    public void Update(IModel model)
    {
        _customLeverageTextBox.SetValueWithoutTriggeringEvent(model.CustomLeverage);
        _positionMarginTextBox.SetValueWithoutTriggeringEvent(model.PositionMargin);
        _futureUsedMarginTextBox.SetValueWithoutTriggeringEvent(model.FutureUsedMargin);

        _futureFreeMarginTextBox.SetValueWithoutTriggeringEvent(model.FutureFreeMargin);
        _futureFreeMarginTextBlock.ForegroundColor = model.FutureFreeMargin >= 0 ? Color.Black : Color.Red;
        _futureFreeMarginTextBox.ForegroundColor = model.FutureFreeMargin >= 0 ? Color.Black : Color.Red;

        _maxPositionSizeByMarginTextBox.SetValueWithoutTriggeringEvent(model.MaxPositionSizeByMargin);
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
    }
}