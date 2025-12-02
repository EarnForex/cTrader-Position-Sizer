using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;

namespace cAlgo.Robots;

public interface ISwapsViewResources
{
    Symbol Symbol { get; }
    IAccount Account { get; }
    CustomStyle CustomStyle { get; }
    bool InputDarkMode { get; }
    double InputFallbackLotSize { get; }
}

public class SwapsView : Button, ISwapsViewResources
{
    private readonly ISwapsViewResources _resources;
    private readonly XTextBoxDouble _dailyLongPerPositionSize;
    private readonly XTextBoxDouble _dailyShortPerPositionSize;
    private readonly TextBlock _currencyPerPositionSizeValueTextBox;
    private readonly XTextBoxDouble _yearlyLongPerPositionSize;
    private readonly XTextBoxDouble _yearlyShortPerPositionSize;
    private readonly TextBlock _currencyPerPositionSizeYearlyValueTextBox;
    private readonly Grid _grid;

    public SwapsView(ISwapsViewResources resources)
    {
        _resources = resources;
        _grid = new Grid();
        _grid.AddColumns(4);
        _grid.AddRows(9);
        Content = _grid;
        Width = 400;
        
        var lotSizeValue = Symbol.LotSize == 0 
            ? InputFallbackLotSize
            : Symbol.LotSize;

        var row = 0;

        var typeTextBlock = MakeTextBlock("Type:");
        
        _grid.AddChild(typeTextBlock, row, 0);
        
        var typeTextBox = MakeTextBox($"{Symbol.SwapCalculationType}");
        typeTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        typeTextBox.TextAlignment = TextAlignment.Left;
        
        _grid.AddChild(typeTextBox, row, 1, 1, 2);
        
        row++;
        
        var tripleSwapTextBlock = MakeTextBlock("Triple Swap:");
        
        _grid.AddChild(tripleSwapTextBlock, row, 0);
        
        var tripleSwapTextBox = MakeTextBox(Symbol.Swap3DaysRollover.ToString());
        tripleSwapTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        tripleSwapTextBox.TextAlignment = TextAlignment.Left;
        
        _grid.AddChild(tripleSwapTextBox, row, 1, 1, 2);
        
        row++;
        
        var longTextBlock = MakeTextBlock("Long");
        
        _grid.AddChild(longTextBlock, row, 1);
        
        var shortTextBlock = MakeTextBlock("Short");
        
        _grid.AddChild(shortTextBlock, row, 2);
        
        row++;
        
        var nominalTextBlock = MakeTextBlock("Nominal:");
        
        _grid.AddChild(nominalTextBlock, row, 0);
        
        var longNominalTextBox = MakeTextBox(Symbol.SwapLong.ToString("F"));
        
        _grid.AddChild(longNominalTextBox, row, 1);
        
        var shortNominalTextBox = MakeTextBox(Symbol.SwapShort.ToString("F"));
        
        _grid.AddChild(shortNominalTextBox, row, 2);
        
        row++;
        
        var dailyTextBlock = MakeTextBlock("Daily:");
        
        _grid.AddChild(dailyTextBlock, row, 0);
        
        var longDailyTextBox = MakeTextBox((Symbol.SwapLong * Symbol.PipValue * lotSizeValue).ToString("F"));
        
        _grid.AddChild(longDailyTextBox, row, 1);
        
        var shortDailyTextBox = MakeTextBox((Symbol.SwapShort * Symbol.PipValue * lotSizeValue).ToString("F"));
        
        _grid.AddChild(shortDailyTextBox, row, 2);
        
        var currencyPerLotTextBlock = MakeTextBlock($"{Account.Asset.Name} per Lot");
        
        _grid.AddChild(currencyPerLotTextBlock, row, 3);
        
        row++;
        
        //todo needs MainModel to be updated
        _dailyLongPerPositionSize = MakeTextBox(0.0);
        
        _grid.AddChild(_dailyLongPerPositionSize, row, 1);
        
        _dailyShortPerPositionSize = MakeTextBox(0.0);
        
        _grid.AddChild(_dailyShortPerPositionSize, row, 2);
        
        _currencyPerPositionSizeValueTextBox = MakeTextBlock($"{Account.Asset.Name} per PS (N/A)");
        
        _grid.AddChild(_currencyPerPositionSizeValueTextBox, row, 3);
        
        row++;
        
        var yearlyTextBlock = MakeTextBlock("Yearly:");
        
        _grid.AddChild(yearlyTextBlock, row, 0);
        
        var longYearlyTextBox = MakeTextBox($"{Symbol.SwapLong * Symbol.PipValue * lotSizeValue * 360:F2}");
        
        _grid.AddChild(longYearlyTextBox, row, 1);
        
        var shortYearlyTextBox = MakeTextBox($"{Symbol.SwapShort * Symbol.PipValue * lotSizeValue * 360:F2}");
        
        _grid.AddChild(shortYearlyTextBox, row, 2);
        
        var currencyPerYearTextBlock = MakeTextBlock($"{Account.Asset.Name} per Lot");
        
        _grid.AddChild(currencyPerYearTextBlock, row, 3);
        
        row++;
        
        //todo needs MainModel to be updated
        _yearlyLongPerPositionSize = MakeTextBox(0);
        
        _grid.AddChild(_yearlyLongPerPositionSize, row, 1);
        
        _yearlyShortPerPositionSize = MakeTextBox(0);
        
        _grid.AddChild(_yearlyShortPerPositionSize, row, 2);
        
        _currencyPerPositionSizeYearlyValueTextBox = MakeTextBlock($"{Account.Asset.Name} per PS (N/A)");
        
        _grid.AddChild(_currencyPerPositionSizeYearlyValueTextBox, row, 3);
        
        row++;
        
        //earnforex.com
        var earnForexTextBlock = MakeTextBlock("www.earnforex.com");
        earnForexTextBlock.FontSize = 10;
        earnForexTextBlock.ForegroundColor = InputDarkMode ? Color.LightGreen : Color.Green;
        
        _grid.AddChild(earnForexTextBlock, row, 0, 1, 2);
        
        for (var i = 0; i < row; i++)
        {
            _grid.Rows[i].SetHeightToAuto();
        }
        
        _grid.Columns[0].SetWidthInPixels(80);
        
        for (var i = 1; i < 4; i++)
        {
            _grid.Columns[i].SetWidthToAuto();
        }
    }

    public void Update(IModel model)
    {
        //multiplier should be calculated based on the position size
        //since swaps are valued in pips, we need to estimate the $ value of a pip, times it by the swap value and then by the position size
        var multiplier = Symbol.SwapCalculationType == SymbolSwapCalculationType.Pips 
            ? Symbol.PipValue * model.TradeSize.Volume
            : model.TradeSize.Volume / Symbol.VolumeInUnitsMin;

        _dailyLongPerPositionSize.SetValueWithoutTriggeringEvent(Symbol.SwapLong * multiplier);
        _dailyShortPerPositionSize.SetValueWithoutTriggeringEvent(Symbol.SwapShort * multiplier);
        _currencyPerPositionSizeValueTextBox.Text = $"{Account.Asset.Name} per PS ({model.TradeSize.Lots:F2})";

        _yearlyLongPerPositionSize.SetValueWithoutTriggeringEvent(Symbol.SwapLong * 360 * multiplier);
        _yearlyShortPerPositionSize.SetValueWithoutTriggeringEvent(Symbol.SwapShort * 360 * multiplier);
        _currencyPerPositionSizeYearlyValueTextBox.Text = $"{Account.Asset.Name} per PS ({model.TradeSize.Lots:F2})";
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

    private XTextBoxString MakeTextBox(string value)
    {
        var tb = new XTextBoxString(value);
        tb.SetCustomStyle(CustomStyle);
        //tb.Border.Width = 100;
        tb.IsReadOnly = true;
        tb.IsHitTestVisible = false;
        
        return tb;
        
        // return new TextBox
        // {
        //     Text = text,
        //     ForegroundColor = Color.Black,
        //     BackgroundColor = Color.Gray,
        //     Width = 100,
        //     HorizontalAlignment = HorizontalAlignment.Left,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(1),
        //     IsReadOnly = true,
        //     IsHitTestVisible = false
        // };
    }
    
    private XTextBoxDouble MakeTextBox(double value)
    {
        var tb = new XTextBoxDouble(value, 2);
        
        tb.SetCustomStyle(CustomStyle);
        tb.IsReadOnly = true;
        tb.IsHitTestVisible = false;
        
        return tb;
        
        // return new TextBox
        // {
        //     Text = text,
        //     ForegroundColor = Color.Black,
        //     BackgroundColor = Color.Gray,
        //     Width = 100,
        //     HorizontalAlignment = HorizontalAlignment.Left,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(1),
        //     IsReadOnly = true,
        //     IsHitTestVisible = false
        // };
    }

    public Symbol Symbol => _resources.Symbol;
    public IAccount Account => _resources.Account;
    public CustomStyle CustomStyle => _resources.CustomStyle;
    public bool InputDarkMode => _resources.InputDarkMode;
    public double InputFallbackLotSize => _resources.InputFallbackLotSize;
}