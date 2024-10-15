using System;
using System.Globalization;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public interface IRiskViewResources
{
    IAccount Account { get; }
    CustomStyle CustomStyle { get; }
    void Print(object obj);
    bool InputCalculateUnadjustedPositionSize { get; }
    bool InputDarkMode { get; }
    XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler);
}

public class RiskView : Button, IRiskViewResources
{
    private readonly Grid _grid;
    private readonly IRiskViewResources _resources;
    private readonly CheckBox _countPendingOrdersCheckBox;
    private readonly CheckBox _ignoreOrdersWithoutStopLossCheckBox;
    private readonly CheckBox _ignoreOrdersWithoutTakeProfitCheckBox;
    private readonly CheckBox _ignoreOrdersInOtherSymbolsCheckBox;
    private readonly XTextBoxDouble _currentPortfolioRiskCurrencyTextBox;
    private readonly XTextBoxDouble _currentPortfolioRiskPercentageTextBox;
    private readonly XTextBoxDouble _currentPortfolioLotsTextBox;
    private readonly XTextBoxDouble _currentPortfolioRewardCurrencyTextBox;
    private readonly XTextBoxDouble _currentPortfolioRewardPercentageTextBox;
    private readonly XTextBoxString _currentPortfolioRewardRiskRatioTextBox;
    private readonly XTextBoxDouble _potentialPortfolioRiskCurrencyTextBox;
    private readonly XTextBoxDouble _potentialPortfolioRiskPercentageTextBox;
    private readonly XTextBoxDouble _potentialPortfolioLotsTextBox;
    private readonly XTextBoxDouble _potentialPortfolioRewardCurrencyTextBox;
    private readonly XTextBoxDouble _potentialPortfolioRewardPercentageTextBox;
    private readonly XTextBoxString _potentialPortfolioRewardRiskRatioTextBox;

    public event EventHandler CountPendingOrdersCheckBoxChecked;
    public event EventHandler CountPendingOrdersCheckBoxUnchecked; 
    public event EventHandler IgnoreOrdersWithoutStopLossCheckBoxChecked;
    public event EventHandler IgnoreOrdersWithoutStopLossCheckBoxUnchecked;
    public event EventHandler IgnoreOrdersWithoutTakeProfitCheckBoxChecked;
    public event EventHandler IgnoreOrdersWithoutTakeProfitCheckBoxUnchecked;
    public event EventHandler IgnoreOrdersInOtherSymbolsCheckBoxChecked;
    public event EventHandler IgnoreOrdersInOtherSymbolsCheckBoxUnchecked;

    public RiskView(IRiskViewResources resources)
    {
        _resources = resources;
        _grid = new Grid();
        //_grid.ShowGridLines = true;
        Content = _grid;
        
        _grid.AddColumns(4);
        _grid.AddRows(13);
        Width = 400;

        var row = 0;

        #region CheckBoxes

        _countPendingOrdersCheckBox = MakeCheckBox("Count Pending Orders");
        
        _countPendingOrdersCheckBox.Checked += _ => CountPendingOrdersCheckBoxChecked?.Invoke(this, EventArgs.Empty);
        _countPendingOrdersCheckBox.Unchecked += _ => CountPendingOrdersCheckBoxUnchecked?.Invoke(this, EventArgs.Empty);
        
        _grid.AddChild(_countPendingOrdersCheckBox, row++, 0, 1, 4);
        
        _ignoreOrdersWithoutStopLossCheckBox = MakeCheckBox("Ignore Orders Without Stop Loss");
        
        _ignoreOrdersWithoutStopLossCheckBox.Checked += _ => IgnoreOrdersWithoutStopLossCheckBoxChecked?.Invoke(this, EventArgs.Empty);
        _ignoreOrdersWithoutStopLossCheckBox.Unchecked += _ => IgnoreOrdersWithoutStopLossCheckBoxUnchecked?.Invoke(this, EventArgs.Empty);
        
        _grid.AddChild(_ignoreOrdersWithoutStopLossCheckBox, row++, 0, 1, 4);
        
        _ignoreOrdersWithoutTakeProfitCheckBox = MakeCheckBox("Ignore Orders Without Take Profit");
        
        _ignoreOrdersWithoutTakeProfitCheckBox.Checked += _ => IgnoreOrdersWithoutTakeProfitCheckBoxChecked?.Invoke(this, EventArgs.Empty);
        _ignoreOrdersWithoutTakeProfitCheckBox.Unchecked += _ => IgnoreOrdersWithoutTakeProfitCheckBoxUnchecked?.Invoke(this, EventArgs.Empty);
        
        _grid.AddChild(_ignoreOrdersWithoutTakeProfitCheckBox, row++, 0, 1, 4);
        
        _ignoreOrdersInOtherSymbolsCheckBox = MakeCheckBox("Ignore Orders In Other Symbols");
        
        _ignoreOrdersInOtherSymbolsCheckBox.Checked += _ => IgnoreOrdersInOtherSymbolsCheckBoxChecked?.Invoke(this, EventArgs.Empty);
        _ignoreOrdersInOtherSymbolsCheckBox.Unchecked += _ => IgnoreOrdersInOtherSymbolsCheckBoxUnchecked?.Invoke(this, EventArgs.Empty);
        
        _grid.AddChild(_ignoreOrdersInOtherSymbolsCheckBox, row++, 0, 1, 4);

        #endregion

        #region CurrentPortfolio

        var currentPortfolioRiskCurrencyTextBlock = MakeTextBlock($"Risk {Account.Asset.Name}");
        
        _grid.AddChild(currentPortfolioRiskCurrencyTextBlock, row, 1);
        
        var currentPortfolioRiskPercentageTextBlock = MakeTextBlock("Risk %");
        
        _grid.AddChild(currentPortfolioRiskPercentageTextBlock, row, 2);
        
        var currentPortfolioLotsTextBlock = MakeTextBlock("Lots");
        
        _grid.AddChild(currentPortfolioLotsTextBlock, row++, 3);
        
        var currentPortfolioLabelTextBlock = MakeTextBlock("Current Portfolio:");
        
        _grid.AddChild(currentPortfolioLabelTextBlock, row, 0);
        
        _currentPortfolioRiskCurrencyTextBox = MakeTextBox(0);
        
        _grid.AddChild(_currentPortfolioRiskCurrencyTextBox, row, 1);
        
        _currentPortfolioRiskPercentageTextBox = MakeTextBox(0);
        
        _grid.AddChild(_currentPortfolioRiskPercentageTextBox, row, 2);
        
        _currentPortfolioLotsTextBox = MakeTextBox(0);
        
        _grid.AddChild(_currentPortfolioLotsTextBox, row++, 3);
        
        var currentPortfolioRewardCurrencyTextBlock = MakeTextBlock($"Reward {Account.Asset.Name}");
        
        _grid.AddChild(currentPortfolioRewardCurrencyTextBlock, row, 1);
        
        var currentPortfolioRewardPercentageTextBlock = MakeTextBlock($"Reward %");
        
        _grid.AddChild(currentPortfolioRewardPercentageTextBlock, row, 2);
        
        var currentPortfolioRewardRiskRatioTextBlock = MakeTextBlock("RRR");
        
        _grid.AddChild(currentPortfolioRewardRiskRatioTextBlock, row++, 3);
        
        _currentPortfolioRewardCurrencyTextBox = MakeTextBox(0);
        
        _grid.AddChild(_currentPortfolioRewardCurrencyTextBox, row, 1);
        
        _currentPortfolioRewardPercentageTextBox = MakeTextBox(0);
        
        _grid.AddChild(_currentPortfolioRewardPercentageTextBox, row, 2);
        
        _currentPortfolioRewardRiskRatioTextBox = MakeTextBox("0");
        
        _grid.AddChild(_currentPortfolioRewardRiskRatioTextBox, row++, 3);

        #endregion

        #region PotentialPortfolio

        var potentialPortfolioRiskCurrencyTextBlock = MakeTextBlock($"Risk {Account.Asset.Name}");
        
        _grid.AddChild(potentialPortfolioRiskCurrencyTextBlock, row, 1);
        
        var potentialPortfolioRiskPercentageTextBlock = MakeTextBlock("Risk %");
        
        _grid.AddChild(potentialPortfolioRiskPercentageTextBlock, row, 2);
        
        var potentialPortfolioLotsTextBlock = MakeTextBlock("Lots");
        
        _grid.AddChild(potentialPortfolioLotsTextBlock, row++, 3);
        
        var potentialPortfolioLabelTextBlock = MakeTextBlock("Potential Portfolio:");
        
        _grid.AddChild(potentialPortfolioLabelTextBlock, row, 0);
        
        _potentialPortfolioRiskCurrencyTextBox = MakeTextBox(0);
        
        _grid.AddChild(_potentialPortfolioRiskCurrencyTextBox, row, 1);
        
        _potentialPortfolioRiskPercentageTextBox = MakeTextBox(0);
        
        _grid.AddChild(_potentialPortfolioRiskPercentageTextBox, row, 2);
        
        _potentialPortfolioLotsTextBox = MakeTextBoxDouble(0, digits: InputCalculateUnadjustedPositionSize ? 8 : 2, valueUpdatedHandler: (_,_) => {});
        _potentialPortfolioLotsTextBox.SetCustomStyle(_resources.CustomStyle);
        _potentialPortfolioLotsTextBox.Width = 85;
        _potentialPortfolioLotsTextBox.IsReadOnly = true;
        _potentialPortfolioLotsTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        
        _grid.AddChild(_potentialPortfolioLotsTextBox, row++, 3);
        
        var potentialPortfolioRewardCurrencyTextBlock = MakeTextBlock($"Reward {Account.Asset.Name}");
        
        _grid.AddChild(potentialPortfolioRewardCurrencyTextBlock, row, 1);
        
        var potentialPortfolioRewardPercentageTextBlock = MakeTextBlock("Reward %");
        
        _grid.AddChild(potentialPortfolioRewardPercentageTextBlock, row, 2);
        
        var potentialPortfolioRewardRiskRatioTextBlock = MakeTextBlock("RRR");
        
        _grid.AddChild(potentialPortfolioRewardRiskRatioTextBlock, row++, 3);
        
        _potentialPortfolioRewardCurrencyTextBox = MakeTextBox(0);
        
        _grid.AddChild(_potentialPortfolioRewardCurrencyTextBox, row, 1);
        
        _potentialPortfolioRewardPercentageTextBox = MakeTextBox(0);
        
        _grid.AddChild(_potentialPortfolioRewardPercentageTextBox, row, 2);
        
        _potentialPortfolioRewardRiskRatioTextBox = MakeTextBox("0");
        
        _grid.AddChild(_potentialPortfolioRewardRiskRatioTextBox, row++, 3);

        #endregion
        
        //--ROW 13
        //www.earnforex.com TextBlock (Column 0)
        
        var earnForexTextBlock = MakeTextBlock("www.earnforex.com");
        earnForexTextBlock.FontSize = 10;
        earnForexTextBlock.ForegroundColor = InputDarkMode ? Color.LightGreen : Color.Green;
        
        _grid.AddChild(earnForexTextBlock, row, 0);
        
        _grid.Width = 400;
        _grid.Margin = 0;
        _grid.Columns[0].SetWidthInPixels(100);
        _grid.Columns[1].SetWidthInPixels(90);
        _grid.Columns[2].SetWidthInPixels(90);
        _grid.Columns[3].SetWidthInPixels(90);
        //
        // for (int i = 0; i < 13; i++)
        // {
        //     _grid.Rows[i].SetHeightToAuto();
        // }
        //
        // for (int i = 0; i < 4; i++)
        // {
        //     _grid.Columns[i].SetWidthToAuto();
        // }
    }

    public void Update(IModel model)
    {
        _countPendingOrdersCheckBox.IsChecked = model.CountPendingOrders;
        _ignoreOrdersWithoutStopLossCheckBox.IsChecked = model.IgnoreOrdersWithoutStopLoss;
        _ignoreOrdersWithoutTakeProfitCheckBox.IsChecked = model.IgnoreOrdersWithoutTakeProfit;
        _ignoreOrdersInOtherSymbolsCheckBox.IsChecked = model.IgnoreOrdersInOtherSymbols;

        _currentPortfolioRiskCurrencyTextBox.Value = model.CurrentPortfolio.RiskCurrency;
        _currentPortfolioRiskPercentageTextBox.Value = model.CurrentPortfolio.RiskPercentage;
        _currentPortfolioLotsTextBox.Value = model.CurrentPortfolio.Lots;

        _currentPortfolioRewardCurrencyTextBox.Value = model.CurrentPortfolio.RewardCurrency;
        _currentPortfolioRewardPercentageTextBox.Value = model.CurrentPortfolio.RewardPercentage;

        // On MQL5 it is set as "-"
        if ((model.CurrentPortfolio.RiskCurrency == 0 && model.CurrentPortfolio.RewardCurrency == 0) ||
            (double.IsPositiveInfinity(model.CurrentPortfolio.RiskCurrency) && double.IsPositiveInfinity(model.CurrentPortfolio.RewardCurrency)))
        {
            _currentPortfolioRewardRiskRatioTextBox.Value = "-";
        }
        else
        {
            _currentPortfolioRewardRiskRatioTextBox.Value = model.CurrentPortfolio.RewardRiskRatio.ToString();
        }
        
        _potentialPortfolioRiskCurrencyTextBox.Value = model.PotentialPortfolio.RiskCurrency;
        _potentialPortfolioRiskPercentageTextBox.Value = model.PotentialPortfolio.RiskPercentage;
        _potentialPortfolioLotsTextBox.Value = model.PotentialPortfolio.Lots;

        _potentialPortfolioRewardCurrencyTextBox.Value = model.PotentialPortfolio.RewardCurrency;
        _potentialPortfolioRewardPercentageTextBox.Value = model.PotentialPortfolio.RewardPercentage;
        
        if ((model.PotentialPortfolio.RiskCurrency == 0 && model.PotentialPortfolio.RewardCurrency == 0) ||
            (double.IsPositiveInfinity(model.PotentialPortfolio.RiskCurrency) && double.IsPositiveInfinity(model.PotentialPortfolio.RewardCurrency)))
        {
            _potentialPortfolioRewardRiskRatioTextBox.Value = "-";
        }
        else
        {
            _potentialPortfolioRewardRiskRatioTextBox.Value = model.PotentialPortfolio.RewardRiskRatio.ToString();
        }
    }

    private CheckBox MakeCheckBox(string text) =>
        new()
        {
            Text = text,
            ForegroundColor = Color.Black,
            BackgroundColor = Color.Gray,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(1)
        };

    private TextBlock MakeTextBlock(string text) =>
        new()
        {
            Text = text,
            ForegroundColor = Color.Black,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(1)
        };

    private XTextBoxDouble MakeTextBox(double defaultValue)
    {
        var tb = MakeTextBoxDouble(defaultValue, digits: 2, valueUpdatedHandler: (_,_) => {});
        tb.SetCustomStyle(_resources.CustomStyle);
        tb.Width = 85;
        tb.IsReadOnly = true;
        tb.HorizontalAlignment = HorizontalAlignment.Left;
    
        return tb;
    
        // return BestTextBoxFactory.Create<double>
        // {
        //     Text = text;
        //     ForegroundColor = Color.Black,
        //     BackgroundColor = Color.Gray,
        //     Width = 100,
        //     HorizontalAlignment = HorizontalAlignment.Left,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(1),
        //     IsReadOnly = true
        // };
    }
    
    private XTextBoxString MakeTextBox(string text)
    {
        var tb = MakeTextBoxString(text, valueUpdatedHandler: (_,_) => {});
        tb.SetCustomStyle(_resources.CustomStyle);
        tb.Width = 85;
        tb.IsReadOnly = true;
        tb.HorizontalAlignment = HorizontalAlignment.Left;
    
        return tb;
    
        // return BestTextBoxFactory.Create<string>
        // {
        //     Text = text;
        //     ForegroundColor = Color.Black,
        //     BackgroundColor = Color.Gray,
        //     Width = 100,
        //     HorizontalAlignment = HorizontalAlignment.Left,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Thickness(1),
        //     IsReadOnly = true
        // };
    }

    public IAccount Account => _resources.Account;
    public CustomStyle CustomStyle => _resources.CustomStyle;
    public void Print(object obj)
    {
        _resources.Print(obj);
    }

    public bool InputCalculateUnadjustedPositionSize => _resources.InputCalculateUnadjustedPositionSize;

    public bool InputDarkMode => _resources.InputDarkMode;
    public XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxDouble(defaultValue, digits, valueUpdatedHandler);
    }

    public XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler)
    {
        return _resources.MakeTextBoxString(defaultValue, valueUpdatedHandler);
    }
}