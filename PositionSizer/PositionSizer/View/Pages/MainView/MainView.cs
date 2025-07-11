using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Loader;
using System.Xml.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;
using static cAlgo.Robots.Tools.BotTools;

namespace cAlgo.Robots;

public class MainView : Button, IMainViewResources
{
    #region PrivateFields

    private readonly Grid _grid;
    private readonly IMainViewResources _resources;
    private readonly Button _longShortButton;
    
    private readonly XTextBoxDoubleNumeric _priceTargetTb;
    private readonly XTextBoxDoubleNumeric _stopLossTb;
    private readonly XTextBoxDoubleNumeric _takeProfitTb;
    private readonly XTextBoxDoubleNumeric _stopLimitPriceTextBox;
    
    private readonly Button _orderTypeButton;
    private readonly Button _hideLinesButton;
    private readonly Button _accountSizeModeButton;
    
    private readonly XTextBoxDouble _commissionTextBox;
    private readonly XTextBoxDouble _accountBalanceTextBox;
    private readonly XTextBoxDouble _riskPercentTextBox;
    private readonly XTextBoxDouble _riskCashTextBox;
    private readonly XTextBoxDouble _rewardCashTextBox;
    private readonly XTextBoxDouble _rewardRiskTextBox;
    private readonly XTextBoxDouble _rewardRiskResultTextBox;
    private readonly XTextBoxString _invalidTpTextBox;
    private readonly XTextBoxDouble _positionSizeTextBox;
    private readonly XTextBoxDouble _riskPercentResultTextBox;
    private readonly XTextBoxDouble _riskCashResultTextBox;
    private readonly XTextBoxDouble _rewardCashResultTextBox;
    
    private int _rowIndex;
    private readonly XTextBoxDouble _showPipValueTextBox;
    private readonly TextBlock _stopPriceTextBlock;
    private readonly TextBlock _wrongStopPriceValueTextBlock;
    private readonly Grid _atrPeriodAndValueGrid;
    private readonly TextBlock _atrPeriodTextBlock;
    private readonly XTextBoxInt _atrPeriodTextBox;
    private readonly TextBlock _atrStopLossMultiplierTextBlock;
    private readonly XTextBoxDouble _atrStopLossMultiplierTextBox;
    private readonly CheckBox _atrStopLossSaCheckBox;
    private readonly TextBlock _atrCurrentValueTextBlock;
    private readonly TextBlock _atrTpMultiplierTextBlock;
    private readonly XTextBoxDouble _atrTakeProfitMultiplierTextBox;
    private readonly CheckBox _atrTakeProfitSaCheckBox;
    private readonly TextBlock _atrTimeFrameTextBlock;
    private readonly Button _atrTimeFrameButton;
    private readonly Grid _takeProfitGrid;
    private readonly List<TakeProfitRowView> _tpViews = new();
    private readonly TextBlock _riskUsdTextBlock;
    private readonly TextBlock _rewardCashTextBlock;
    private readonly TextBlock _rewardRiskTextBlock;
    private readonly Button _takeProfitButton;
    private readonly Button _quickRisk1Button;
    private readonly Button _quickRisk2Button;
    private readonly Grid _quickRiskGrid;
    private readonly TextBlock _accountSizeAsterisk;
    private readonly Grid _rewardRiskResultGrid;
    private readonly Button _tradeButton;

    #endregion

    #region EventHandlers

    public event EventHandler TradeButtonClicked;
    public event EventHandler<TradeTypeChangedEventArgs> TradeTypeChanged;
    public event EventHandler<TargetPriceChangedEventArgs> TargetPriceChanged;
    public event EventHandler<StopLossPriceChangedEventArgs> StopLossFieldValueChanged;
    public event EventHandler StopLossDefaultClick;
    public event EventHandler<StopLimitPriceChangedEventArgs> StopLimitPriceChanged; 
    public event EventHandler<TakeProfitPriceChangedEventArgs> TakeProfitPriceChanged;
    public event EventHandler<TakeProfitLevelAddedEventArgs> TakeProfitLevelAdded;
    public event EventHandler<TakeProfitLevelRemovedEventArgs> TakeProfitLevelRemoved;
    public event EventHandler<OrderTypeChangedEventArgs> OrderTypeChanged;
    public event EventHandler<HideLinesClickedEventArgs> HideLinesClicked;
    public event EventHandler<AccountValueTypeChangedEventArgs> AccountSizeModeChanged;
    public event EventHandler<AccountValueChangedEventArgs> AccountValueChanged; 
    public event EventHandler<RiskPercentageChangedEventArgs> RiskPercentageChanged;
    public event EventHandler<RiskCashValueChangedEventArgs> RiskCashValueChanged;
    public event EventHandler<PositionSizeValueChangedEventArgs> PositionSizeValueChanged;
    public event EventHandler PositionMaxSizeClicked;
    public event EventHandler<AtrPeriodChangedEventArgs> AtrPeriodChanged;
    public event EventHandler<AtrStopLossMultiplierChangedEventArgs> AtrStopLossMultiplierChanged;
    public event EventHandler<AtrTakeProfitMultiplierChangedEventArgs> AtrTakeProfitMultiplierChanged;
    public event EventHandler<AtrTimeFrameChangedEventArgs> AtrTimeFrameChanged;
    public event EventHandler<AtrStopLossSaChangedEventArgs> AtrStopLossSaChanged;
    public event EventHandler<AtrTakeProfitSaChangedEventArgs> AtrTakeProfitSaChanged;
    public event EventHandler TakeProfitButtonClick;

    #endregion

    public MainView(IMainViewResources resources, IModel model)
    {
        _resources = resources;
        _grid = new Grid()
        {
            //ShowGridLines = true
        };
        this.Content = _grid;
        
        _grid.AddColumns(4);
        Width = 400;
        //Height = 300;
        //_grid.ShowGridLines = true;
        
        //--ROW 0
        //Entry TextBlock (Column 0)
        //(Short, Long) Buttons (Column 1)
        //(Price Target) TextBox (Column 2)

        _tradeButton = MakeButton("Trade");
        _tradeButton.Click += OnTradeButtonClicked;
        
        var entryTextBlock = MakeTextBlock("Entry");
        
        _longShortButton = MakeButton("Long");
        _longShortButton.Width = 59;
        _longShortButton.HorizontalAlignment = HorizontalAlignment.Left;
        _longShortButton.Click += LongShortButtonOnClick;
        
        _priceTargetTb = MakeTextBoxDoubleNumeric(Symbol.Ask, Symbol.Digits, Symbol.TickSize, TextBoxOnPriceTargetValueUpdated);
        //KeyMultiplierFeature.SetFeatureOnButton(_priceTargetTb);

        _grid.AddRow();

        switch (InputAdditionalTradeButtons)
        {
            case AdditionalTradeButtons.None:
            case AdditionalTradeButtons.AboveTheEntryLine:
                _grid.AddChild(entryTextBlock, _rowIndex, 0);
                break;
            case AdditionalTradeButtons.MainTab:
            case AdditionalTradeButtons.Both:
                _grid.AddChild(_tradeButton, _rowIndex, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _grid.AddChild(_longShortButton, _rowIndex, 1, 1, 2);
        _grid.AddChild(_priceTargetTb, _rowIndex, 2);
        
        //--NEW ROW
        //Stop Loss TextBlock (Column 0)
        //Stop Loss TextBox(+-) (Column 2)
        
        _stopLossTb = MakeTextBoxDoubleNumeric(Symbol.Ask, Symbol.Digits, Symbol.TickSize, TextBoxOnStopLossValueUpdated);
        _stopLossTb.ValidationAllowZero = false;
        //KeyMultiplierFeature.SetFeatureOnButton(_stopLossTb);

        NewRow();
        
        if (model.StopLoss.HasDefaultSwitch)
        {
            var stopLossDefaultButton = MakeButton("Stop Loss");
            stopLossDefaultButton.Width = 145;
            stopLossDefaultButton.Click += OnStopLossDefaultButtonOnClick;
            _grid.AddChild(stopLossDefaultButton, _rowIndex, 0, 1, 2);
        }
        else
        {
            var stopLossTextBlock = MakeTextBlock("Stop Loss");
            _grid.AddChild(stopLossTextBlock, _rowIndex, 0, 1, 2);
        }
        
        _grid.AddChild(_stopLossTb, _rowIndex, 2);
        
        //--NEW ROW
        //(+)Take Profit Button (Column 0-1)
        //Take Profit (+-) TextBox(+-) (Column 2)

        _takeProfitGrid = new Grid(1, 3)
        {
            //ShowGridLines = true,
            Width = 255,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        
        var addTakeProfitButton = MakeButton("+");
        addTakeProfitButton.Width = 30;
        addTakeProfitButton.Click += AddTakeProfitButtonOnClick;
        
        _takeProfitButton = MakeButton(Math.Abs(model.TakeProfits.LockedMultiplier - 1) < double.Epsilon ? "Take Profit" : $"TP x {model.TakeProfits.LockedMultiplier}");
        _takeProfitButton.Width = 98;
        _takeProfitTb = model.TakeProfits.Mode == TargetMode.Pips
            ? MakeTextBoxDoubleNumeric(model.TakeProfits.List[0].Pips, 1, 0.1, TakeProfitTextBoxOnTextUpdatedAndValid)
            : MakeTextBoxDoubleNumeric(model.TakeProfits.List[0].Price, Symbol.Digits, Symbol.TickSize, TakeProfitTextBoxOnTextUpdatedAndValid);
        //KeyMultiplierFeature.SetFeatureOnButton(_takeProfitTb);
        
        _takeProfitButton.Click += TakeProfitButtonOnClick;
        
        //_takeProfitGrid.BackgroundColor = Color.LightBlue;
        //_takeProfitGrid.ShowGridLines = true;
        _takeProfitGrid.Rows[0].SetHeightInPixels(28);
        _takeProfitGrid.Columns[0].SetWidthInPixels(45);
        _takeProfitGrid.Columns[1].SetWidthInPixels(108);
        _takeProfitGrid.Columns[2].SetWidthInPixels(101);
        _takeProfitGrid.Width = 350;
        
        _takeProfitGrid.AddChild(addTakeProfitButton, 0, 0);
        _takeProfitGrid.AddChild(_takeProfitButton, 0, 1);
        _takeProfitGrid.AddChild(_takeProfitTb, 0, 2);

        NewRow();
        _grid.AddChild(_takeProfitGrid, _rowIndex, 0, 1, 4);
        
        var tpView = new TakeProfitRowView(_tpViews.Count, addTakeProfitButton, _takeProfitButton, _takeProfitTb);
        
        _tpViews.Add(tpView);
        
        // AddChild(addTakeProfitButton, _rowIndex, 0);
        // AddChild(takeProfitButton, _rowIndex, 1);
        // AddChild(_takeProfitTextBox.ButtonGrid, _rowIndex, 2);
        
        //--NEW ROW (Only if Stop Limit mode is active)
        //Stop Price TextBlock (Column 0-1)
        //Stop Price TextBox(+-) (Column 2)
        
        _stopPriceTextBlock = MakeTextBlock("Stop Price");
        _stopLimitPriceTextBox = MakeTextBoxDoubleNumeric(model.StopLimitPrice, Symbol.Digits, Symbol.TickSize, TextBoxOnStopLimitPriceChanged);
        //KeyMultiplierFeature.SetFeatureOnButton(_stopLimitPriceTextBox);
        
        _wrongStopPriceValueTextBlock = MakeTextBlock("(Wrong Value!!!)");
        
        _stopPriceTextBlock.IsVisible = false;
        _stopLimitPriceTextBox.IsVisible = false;
        _wrongStopPriceValueTextBlock.IsVisible = false;
        
        //This row should be invisible at start
        NewRow();
        _grid.AddChild(_stopPriceTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_stopLimitPriceTextBox, _rowIndex, 2);
        _grid.AddChild(_wrongStopPriceValueTextBlock, _rowIndex, 3);

        if (InputShowAtrOptions)
        {
            //THE ATR ROWS Can be disabled if not checked
            //--NEW ROW (ATR Period [ Value ] SL Multiplier [ Value ] SA CheckBox [ Value ])
        
            _atrPeriodTextBlock = MakeTextBlock("ATR Period");
            _atrPeriodTextBlock.Width = 58;
            _atrPeriodTextBox = new XTextBoxInt(model.Period);
            _atrPeriodTextBox = MakeTextBoxInt(model.Period, TextBoxAtrPeriodChanged);
            _atrPeriodTextBox.ChangeWriteAreaWidth(25);

            _atrPeriodAndValueGrid = new Grid(1, 2)
            {
                //ShowGridLines = true,
                //BackgroundColor = Color.Red,
                Width = 85,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
        
            _atrPeriodAndValueGrid.Columns[0].SetWidthInPixels(55);
            _atrPeriodAndValueGrid.Columns[1].SetWidthInPixels(26);
        
            _atrPeriodAndValueGrid.AddChild(_atrPeriodTextBlock, 0, 0);
            _atrPeriodAndValueGrid.AddChild(_atrPeriodTextBox, 0, 1);
        
            _atrStopLossMultiplierTextBlock = MakeTextBlock("SL Multiplier:");
            _atrStopLossMultiplierTextBox = MakeTextBoxDouble(model.StopLossMultiplier, 2, TextBoxAtrStopLossMultiplierChanged);
            _atrStopLossMultiplierTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            _atrStopLossMultiplierTextBox.ChangeWriteAreaWidth(40);
            
            _grid.Columns[2].SetWidthInPixels(20);
            
            
            _atrStopLossSaCheckBox = MakeCheckBox("SA");
            _atrStopLossSaCheckBox.IsChecked = model.StopLossSpreadAdjusted;
            _atrStopLossSaCheckBox.Checked += AtrStopLossSaCheckBoxChecked;
            _atrStopLossSaCheckBox.Unchecked += AtrStopLossSaCheckBoxUnChecked;
        
            NewRow();
            _grid.AddChild(_atrPeriodAndValueGrid, _rowIndex, 0, 1, 2);
            _grid.AddChild(_atrStopLossMultiplierTextBlock, _rowIndex, 1);
            _grid.AddChild(_atrStopLossMultiplierTextBox, _rowIndex, 2);
            _grid.AddChild(_atrStopLossSaCheckBox, _rowIndex, 3);
        
            //--NEW ROW (ATR = Current Value) TP Multiplier [ Value ] TP CheckBox [ Value ])
            _atrCurrentValueTextBlock = MakeTextBlock("ATR = 0.0000");
            _atrTpMultiplierTextBlock = MakeTextBlock("TP Multiplier:");
            _atrTakeProfitMultiplierTextBox = MakeTextBoxDouble(model.TakeProfitMultiplier, 2, TextBoxAtrTakeProfitMultiplierChanged);
            _atrTakeProfitMultiplierTextBox.ChangeWriteAreaWidth(40);
            _atrTakeProfitMultiplierTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            _atrTakeProfitSaCheckBox = MakeCheckBox("SA");
            _atrTakeProfitSaCheckBox.IsChecked = model.TakeProfitSpreadAdjusted;
            _atrTakeProfitSaCheckBox.Checked += AtrTakeProfitSaCheckBoxChecked;
            _atrTakeProfitSaCheckBox.Unchecked += AtrTakeProfitSaCheckBoxUnChecked;
        
            NewRow();
            _grid.AddChild(_atrCurrentValueTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_atrTpMultiplierTextBlock, _rowIndex, 1);
            _grid.AddChild(_atrTakeProfitMultiplierTextBox, _rowIndex, 2);
            _grid.AddChild(_atrTakeProfitSaCheckBox, _rowIndex, 3);
        
            //--NEW ROW (ATR Timeframe [TimeFrame Button])
            _atrTimeFrameTextBlock = MakeTextBlock("ATR Timeframe");
            _atrTimeFrameButton = MakeButton("CURRENT");
            _atrTimeFrameButton.Click += AtrTimeFrameButtonOnClick;
        
            NewRow();
            _grid.AddChild(_atrTimeFrameTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_atrTimeFrameButton, _rowIndex, 2);
        }
            
        //--NEW ROW
        //Order Type TextBlock (Column 0)
        //Order Type Button (Column 2)
        //Hide Lines Button (Column 3)
        
        var orderTypeTextBlock = MakeTextBlock("Order Type");
        _orderTypeButton = MakeButton("Market");
        _orderTypeButton.Click += OrderTypeButtonOnClick;
        
        _hideLinesButton = MakeButton("Hide Lines");
        _hideLinesButton.Click += OnHideLinesClicked;

        NewRow();
        _grid.AddChild(orderTypeTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_orderTypeButton, _rowIndex, 2);
        _grid.AddChild(_hideLinesButton, _rowIndex, 3);
        
        //--NEW ROW
        //Commission TextBlock (Column 0-2)
        //Commission TextBox (read-only) (Column 3)
        
        var commissionTextBlock = MakeTextBlock("Commission");
        _commissionTextBox = new XTextBoxDouble(Symbol.Commission, 2);
        _commissionTextBox.SetCustomStyle(CustomStyle);
        _commissionTextBox.IsReadOnly = true;

        NewRow();
        _grid.AddChild(commissionTextBlock, _rowIndex, 0, 1, 3);
        _grid.AddChild(_commissionTextBox, _rowIndex, 3);
        
        //--NEW ROW
        //Account Balance Button (Column 0-1)
        //Account Balance TextBox (Column 2-3)

        if (!InputHideAccountSize)
        {
            var accSizeWrapPanel = new WrapPanel();
            
            _accountSizeModeButton = MakeButton(string.Empty);
            _accountSizeModeButton.Click += OnAccountSizeModeChanged;

            _accountBalanceTextBox = MakeTextBoxDouble(9000.00, 2, OnAccountValueChanged);
            _accountBalanceTextBox.ValidationAllowZero = false;

            _accountSizeAsterisk = MakeTextBlock("*");
            _accountSizeAsterisk.FontWeight = FontWeight.Bold;
            _accountSizeAsterisk.FontSize = 12;
            _accountSizeAsterisk.IsVisible = GetAsteriskVisibility(model);
            
            accSizeWrapPanel.AddChild(_accountBalanceTextBox);
            accSizeWrapPanel.AddChild(_accountSizeAsterisk);

            NewRow();
            _grid.AddChild(_accountSizeModeButton, _rowIndex, 0, 1, 2);
            _grid.AddChild(accSizeWrapPanel, _rowIndex, 2, 1, 3);   
        }
        
        //--NEW ROW
        //Input TextBlock (Column 2)
        //Result TextBlock (Column 3)
        
        var inputTextBlock = MakeTextBlock("Input");
        var resultTextBlock = MakeTextBlock("Result");

        NewRow();
        _grid.AddChild(inputTextBlock, _rowIndex, 2);
        _grid.AddChild(resultTextBlock, _rowIndex, 3);
        
        //--NEW ROW
        //Risk % TextBlock (Column 0-1)
        //Risk % TextBox (Column 2)
        
        var riskTextBlock = MakeTextBlock("Risk %");

        _quickRiskGrid = new Grid(1, 1);
        _quickRiskGrid.AddChild(riskTextBlock, 0, 0);
        
        if (InputQuickRisk1Pct != 0)
        {
            _quickRiskGrid.AddColumn();
            _quickRisk1Button = MakeButton(InputQuickRisk1Pct.ToString(CultureInfo.InvariantCulture));
            _quickRisk1Button.Click += args =>
            {
                TrySaveTextBoxesContent();
                _riskPercentTextBox.Value = InputQuickRisk1Pct;
                RiskPercentageChanged?.Invoke(this, new RiskPercentageChangedEventArgs(InputQuickRisk1Pct));
            };
            _quickRiskGrid.AddChild(_quickRisk1Button, 0, 1);
        }

        if (InputQuickRisk2Pct != 0)
        {
            _quickRiskGrid.AddColumn();
            _quickRisk2Button = MakeButton(InputQuickRisk2Pct.ToString(CultureInfo.InvariantCulture));
            _quickRisk2Button.Click += args =>
            {
                TrySaveTextBoxesContent();
                _riskPercentTextBox.Value = InputQuickRisk2Pct;
                RiskPercentageChanged?.Invoke(this, new RiskPercentageChangedEventArgs(InputQuickRisk2Pct));
            };
            _quickRiskGrid.AddChild(_quickRisk2Button, 0, _quickRiskGrid.Columns.Count - 1);
        }
        
        _riskPercentTextBox = MakeTextBoxDouble(model.TradeSize.RiskPercentage, 2, RiskPercentTextBoxOnValueChanged);
        
        _riskPercentResultTextBox = MakeTextBoxDouble(model.TradeSize.RiskPercentage, 2, RiskPercentTextBoxOnValueChanged);
        _riskPercentResultTextBox.IsReadOnly = true;

        NewRow();
        _grid.AddChild(_quickRiskGrid, _rowIndex, 0, 1, 2);
        _grid.AddChild(_riskPercentTextBox, _rowIndex, 2);
        _grid.AddChild(_riskPercentResultTextBox, _rowIndex, 3);

        //--NEW ROW
        //Risk (USD) TextBlock (Column 0-1)
        //Risk (USD) TextBox (Column 2)
        
        _riskUsdTextBlock = MakeTextBlock($"Risk ({Account.Asset.Name})");
        _riskCashTextBox = MakeTextBoxDouble(model.TradeSize.RiskInCurrency, 2, RiskCashTextBoxOnTextChanged);
        
        _riskCashResultTextBox = MakeTextBoxDouble(model.TradeSize.RiskInCurrency, 2, RiskCashTextBoxOnTextChanged);
        _riskCashResultTextBox.IsReadOnly = true;

        NewRow();
        _grid.AddChild(_riskUsdTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_riskCashTextBox, _rowIndex, 2);
        _grid.AddChild(_riskCashResultTextBox, _rowIndex, 3);

        //--NEW ROW
        //Reward (USD) TextBlock (Column 0-1)
        //Reward (USD) TextBox (Column 2)
        
        _rewardCashTextBlock = MakeTextBlock($"Reward ({Account.Asset.Name})");
        _rewardCashTextBox = new XTextBoxDouble(model.TradeSize.RewardInCurrency, 2);
        _rewardCashTextBox.SetCustomStyle(CustomStyle);
        _rewardCashTextBox.IsReadOnly = true;

        _rewardCashResultTextBox = new XTextBoxDouble(model.TradeSize.RewardInCurrency, 2);
        _rewardCashResultTextBox.SetCustomStyle(CustomStyle);
        _rewardCashResultTextBox.IsReadOnly = true;

        NewRow();
        _grid.AddChild(_rewardCashTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_rewardCashTextBox, _rowIndex, 2);
        _grid.AddChild(_rewardCashResultTextBox, _rowIndex, 3);

        //--NEW ROW
        //Reward/Risk TextBlock (Column 0-1)
        //Reward/Risk TextBox (Column 3)

        _rewardRiskTextBlock = MakeTextBlock("Reward/Risk");
        
        _rewardRiskTextBox = new XTextBoxDouble(model.TradeSize.RewardRiskRatio, 2);
        _rewardRiskTextBox.SetCustomStyle(CustomStyle);
        _rewardRiskTextBox.IsReadOnly = true;
        _rewardRiskTextBox.IsVisible = false;

        _rewardRiskResultGrid = new Grid(2, 1);
        
        _rewardRiskResultTextBox = new XTextBoxDouble(model.TradeSize.RewardRiskRatioResult, 2);
        _rewardRiskResultTextBox.SetCustomStyle(CustomStyle);
        _rewardRiskResultTextBox.IsReadOnly = true;

        _invalidTpTextBox = MakeTextBoxString("Invalid TP", (_, _) => { });
        _invalidTpTextBox.IsReadOnly = true;
        _invalidTpTextBox.ForegroundColor = Color.Red;
        
        _rewardRiskResultGrid.AddChild(_rewardRiskResultTextBox, 0, 0);
        _rewardRiskResultGrid.AddChild(_invalidTpTextBox, 1, 0);

        NewRow();
        _grid.AddChild(_rewardRiskTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_rewardRiskTextBox, _rowIndex, 2);
        _grid.AddChild(_rewardRiskResultGrid, _rowIndex, 3);
        
        //--NEW ROW
        //Position Size TextBlock (Column 0-1)
        //Position Size TextBox (Column 3)
        NewRow();

        if (InputShowMaxPositionSizeButton)
        {
            var maxPositionSizeButton = MakeButton("Max PS");
            maxPositionSizeButton.Width = 100;

            NewRow();
            _grid.AddChild(maxPositionSizeButton, _rowIndex, 2);
            
            maxPositionSizeButton.Click += args =>
            {
                TrySaveTextBoxesContent();
                PositionMaxSizeClicked?.Invoke(this, EventArgs.Empty);
            };
        }
        
        var positionSizeTextBlock = MakeTextBlock("Position Size");
        _positionSizeTextBox = MakeTextBoxDouble(
            defaultValue: model.TradeSize.Lots,
            digits: InputCalculateUnadjustedPositionSize ? 8 : CountDecimals(Symbol.VolumeInUnitsToQuantity(Symbol.VolumeInUnitsMin)), 
            valueUpdatedHandler: PositionSizeTextBoxOnTextChanged);
        
        _grid.AddChild(positionSizeTextBlock, _rowIndex, 0, 1, 2);
        _grid.AddChild(_positionSizeTextBox, _rowIndex, 3);
        
        //--NEW ROW
        //Show Pip Value

        if (InputShowPipValue)
        {
            var showPipValueTextBlock = MakeTextBlock($"Pip value, {Account.Asset.Name}");
            
            _showPipValueTextBox = new XTextBoxDouble(0.00, 2);
            _showPipValueTextBox.SetCustomStyle(CustomStyle);
            _showPipValueTextBox.IsReadOnly = true;
        
            NewRow();
            _grid.AddChild(showPipValueTextBlock, _rowIndex, 0, 1, 2);
            _grid.AddChild(_showPipValueTextBox, _rowIndex, 3);   
        }
        
        //--NEW ROW
        //www.earnforex.com TextBlock (Column 0)
        
        var earnForexTextBlock = MakeTextBlock("www.earnforex.com");
        earnForexTextBlock.FontSize = 10;
        earnForexTextBlock.ForegroundColor = InputDarkMode ? Color.LightGreen : Color.Green;

        NewRow();
        _grid.AddChild(earnForexTextBlock, _rowIndex, 0, 1, 2);
        
        for (int i = 0; i < _rowIndex; i++)
        {
            _grid.Rows[i].SetHeightToAuto();
        }
        
        _grid.Columns[0].SetWidthInPixels(85);
        _grid.Columns[1].SetWidthInPixels(70);
        _grid.Columns[2].SetWidthInPixels(102);
    }

    private void OnStopLossDefaultButtonOnClick(ButtonClickEventArgs args)
    {
        TrySaveTextBoxesContent();
        StopLossDefaultClick?.Invoke(this, EventArgs.Empty);
    }

    private void OnTradeButtonClicked(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        TradeButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    private void NewRow()
    {
        _rowIndex++;
        _grid.AddRow();
    }

    private void TakeProfitButtonOnClick(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        
        TakeProfitButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private void TakeProfitTextBoxOnTextUpdatedAndValid(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        TakeProfitPriceChanged?.Invoke(this, new TakeProfitPriceChangedEventArgs(0, e.Value));
    }

    private void AddTakeProfitButtonOnClick(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        AddTakeProfitLevel();
    }

    private void AddTakeProfitLevel(bool triggerAlert = true)
    {
        //new row inside _takeProfitGrid
        //X which means remove this row, at column 0
        //"Take Profit (n)" at column 1 (It's a textblock)
        //TextBox at column 2

        var removeButton = MakeButton("X");
        removeButton.Width = 30;
        removeButton.Click += RemoveTakeProfitButtonOnClick;

        var takeProfitBlock = MakeTextBlock($"Take Profit ({_takeProfitGrid.Rows.Count + 1})");

        var takeProfitTextBox = new XTextBoxDoubleNumeric(0, Symbol.Digits, Symbol.TickSize);
        takeProfitTextBox.SetCustomStyle(CustomStyle);

        var row = _takeProfitGrid.Rows.Count;

        var tpView = new TakeProfitRowView(_tpViews.Count, removeButton, takeProfitBlock, takeProfitTextBox);

        tpView.TakeProfitValueChanged += (sender, args) => 
            TakeProfitPriceChanged?.Invoke(this, new TakeProfitPriceChangedEventArgs(args.Id, args.Value));

        tpView.TakeProfitControlClicked += (sender, args) => TrySaveTextBoxesContent();

        _tpViews.Add(tpView);

        if (_tpViews.Count > 1)
        {
            if (_tpViews[^2].RemoveButton.Text == "X")
                _tpViews[^2].RemoveButton.IsVisible = false;
        }

        _takeProfitGrid.AddRow();
        _takeProfitGrid.AddChild(removeButton, row, 0);
        _takeProfitGrid.AddChild(takeProfitBlock, row, 1);
        _takeProfitGrid.AddChild(takeProfitTextBox, row, 2);

        if (triggerAlert)
            TakeProfitLevelAdded?.Invoke(this, new TakeProfitLevelAddedEventArgs());
    }

    private void RemoveTakeProfitButtonOnClick(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        RemoveTakeProfitLevel();
    }

    private void RemoveTakeProfitLevel(bool triggerAlert = true)
    {
        _tpViews[^1].RemoveButton.Click -= RemoveTakeProfitButtonOnClick;
        _takeProfitGrid.RemoveChild(_tpViews[^1].RemoveButton);
        _takeProfitGrid.RemoveChild(_tpViews[^1].TakeProfitNTextBlock);
        _takeProfitGrid.RemoveChild(_tpViews[^1].TakeProfitTextBox);
        _tpViews.RemoveAt(_tpViews.Count - 1);
        _takeProfitGrid.RemoveRowAt(_takeProfitGrid.Rows.Count - 1);

        if (_tpViews.Count > 0)
            _tpViews[^1].RemoveButton.IsVisible = true;

        if (triggerAlert)
            TakeProfitLevelRemoved?.Invoke(this, new TakeProfitLevelRemovedEventArgs());
    }

    private void AtrTimeFrameButtonOnClick(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        AtrTimeFrameChanged?.Invoke(this, new AtrTimeFrameChangedEventArgs(_atrTimeFrameButton.Text));
    }

    private void AtrTakeProfitSaCheckBoxUnChecked(CheckBoxEventArgs obj)
    {
        if (obj.CheckBox.IsChecked.HasValue && !obj.CheckBox.IsChecked.Value)
            AtrTakeProfitSaChanged?.Invoke(this, new AtrTakeProfitSaChangedEventArgs(false));
    }

    private void AtrTakeProfitSaCheckBoxChecked(CheckBoxEventArgs obj)
    {
        if (obj.CheckBox.IsChecked.HasValue && obj.CheckBox.IsChecked.Value)
            AtrTakeProfitSaChanged?.Invoke(this, new AtrTakeProfitSaChangedEventArgs(true));
    }
    
    private void TextBoxAtrTakeProfitMultiplierChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        AtrTakeProfitMultiplierChanged?.Invoke(this, new AtrTakeProfitMultiplierChangedEventArgs(e.Value));
    }
    
    public void UpdateAtrTimeFrame(IModel model)
    {
        _atrTimeFrameButton.Text = model.GetTimeFrameShortName();
    }

    public void UpdateAtrValue(IModel model, Symbol symbol)
    {
        _atrCurrentValueTextBlock.Text = $"ATR = {model.GetAtrPips().ToString($"0.{new string('0', 2)}")}";
    }

    private void AtrStopLossSaCheckBoxUnChecked(CheckBoxEventArgs obj)
    {
        if (obj.CheckBox.IsChecked.HasValue && !obj.CheckBox.IsChecked.Value)
            AtrStopLossSaChanged?.Invoke(this, new AtrStopLossSaChangedEventArgs(false));
    }

    private void AtrStopLossSaCheckBoxChecked(CheckBoxEventArgs obj)
    {
        if (obj.CheckBox.IsChecked.HasValue && obj.CheckBox.IsChecked.Value)
            AtrStopLossSaChanged?.Invoke(this, new AtrStopLossSaChangedEventArgs(true));
    }
    
    private void TextBoxAtrStopLossMultiplierChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        if (e.Value < 0)
            return;
        
        AtrStopLossMultiplierChanged?.Invoke(this, new AtrStopLossMultiplierChangedEventArgs(e.Value));
    }
    
    private void TextBoxAtrPeriodChanged(object sender, ControlValueUpdatedEventArgs<int> e)
    {
        if (e.Value < 1)
            return;
        
        AtrPeriodChanged?.Invoke(this, new AtrPeriodChangedEventArgs(e.Value));
    }
    
    private void PositionSizeTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        PositionSizeValueChanged?.Invoke(this, new PositionSizeValueChangedEventArgs(e.Value));
    }
    
    private void RiskCashTextBoxOnTextChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        RiskCashValueChanged?.Invoke(this, new RiskCashValueChangedEventArgs(e.Value));
    }
    
    private void RiskPercentTextBoxOnValueChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        RiskPercentageChanged?.Invoke(this, new RiskPercentageChangedEventArgs(e.Value));
    }
    
    private void OnAccountValueChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        AccountValueChanged?.Invoke(this, new AccountValueChangedEventArgs(e.Value));
    }

    private void OnAccountSizeModeChanged(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        //options are Equity, Balance, Balance CPR
        if (_accountSizeModeButton.Text == GetDescription(AccountSizeMode.Equity))
            AccountSizeModeChanged?.Invoke(this, new AccountValueTypeChangedEventArgs(AccountSizeMode.Balance));
        else if (_accountSizeModeButton.Text == GetDescription(AccountSizeMode.Balance))
            AccountSizeModeChanged?.Invoke(this, new AccountValueTypeChangedEventArgs(AccountSizeMode.BalanceCpr));
        else
            AccountSizeModeChanged?.Invoke(this, new AccountValueTypeChangedEventArgs(AccountSizeMode.Equity));
    }

    private void OnHideLinesClicked(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        ChangeLinesStatus();
    }

    public void ChangeLinesStatus()
    {
        if (_hideLinesButton.Text == "Hide Lines")
        {
            _hideLinesButton.Text = "Show Lines";
            HideLinesClicked?.Invoke(this, new HideLinesClickedEventArgs(true));
        }
        else
        {
            _hideLinesButton.Text = "Hide Lines";
            HideLinesClicked?.Invoke(this, new HideLinesClickedEventArgs(false));
        }
    }

    private void OrderTypeButtonOnClick(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        ChangeOrderType();
    }

    public void ChangeOrderType()
    {
        //Options are Market, Pending, Stop Limit, in that order
        if (_orderTypeButton.Text == "Instant")
        {
            _orderTypeButton.Text = "Pending";
            OrderTypeChanged?.Invoke(this, new OrderTypeChangedEventArgs(OrderType.Pending));
        }
        else if (_orderTypeButton.Text == "Pending" && !InputDisableStopLimit)
        {
            _orderTypeButton.Text = "Stop Limit";
            OrderTypeChanged?.Invoke(this, new OrderTypeChangedEventArgs(OrderType.StopLimit));
        }
        else
        {
            _orderTypeButton.Text = "Instant";
            OrderTypeChanged?.Invoke(this, new OrderTypeChangedEventArgs(OrderType.Instant));
        }
    }
    
    private void TextBoxOnStopLimitPriceChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        StopLimitPriceChanged?.Invoke(this, new StopLimitPriceChangedEventArgs(e.Value));
    }
    
    private void TextBoxOnStopLossValueUpdated(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        StopLossFieldValueChanged?.Invoke(this, new StopLossPriceChangedEventArgs(e.Value));
    }
    
    private void TextBoxOnPriceTargetValueUpdated(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        TargetPriceChanged?.Invoke(this, new TargetPriceChangedEventArgs(e.Value));
    }

    private void LongShortButtonOnClick(ButtonClickEventArgs obj)
    {
        TrySaveTextBoxesContent();
        ChangeDirection();
    }

    public void ChangeDirection()
    {
        if (_longShortButton.Text == "Long")
        {
            _longShortButton.Text = "Short";
            TradeTypeChanged?.Invoke(this, new TradeTypeChangedEventArgs(TradeType.Sell));
        }
        else
        {
            _longShortButton.Text = "Long";
            TradeTypeChanged?.Invoke(this, new TradeTypeChangedEventArgs(TradeType.Buy));
        }
    } 
    
    public void UpdatePriceTarget(IModel model)
    {
        if (model.OrderType == OrderType.Instant)
        {
            _priceTargetTb.ResetProperty(ControlProperty.BackgroundColor);
            _priceTargetTb.Style = CustomStyle.ReadOnlyTextBoxStyle;
            _priceTargetTb.IsReadOnly = true;
        }
        else
        {
            _priceTargetTb.Style = CustomStyle.TextBoxStyle;
            _priceTargetTb.IsReadOnly = false;
        }

        _priceTargetTb.SetValueWithoutTriggeringEvent(model.EntryPrice);
    }

    public void UpdateStopLossFields(IModel model)
    {
        UpdateStopLossPrecision(model);

        _stopLossTb.Value = model.StopLoss.Mode == TargetMode.Price 
            ? model.StopLoss.Price
            : model.StopLoss.Pips;
    }

    private static bool IsAnyTakeProfitInvalid(IModel model)
    {
        foreach (var tp in model.TakeProfits.List)
        {
            if (model.TradeType == TradeType.Buy)
            {
                if (tp.Price < model.EntryPrice)
                    return true;
            }
            else
            {
                if (tp.Price > model.EntryPrice)
                    return true;
            }
        }

        return false;
    }

    public void Update(IModel model)
    {
        UpdateLongShortButton(model);
        UpdatePriceTarget(model);
        UpdateTakeProfitLocketStatus(model);
        UpdateStopLossPrecision(model);
        UpdateTakeProfitPrecision(model);
        UpdateStopLossText(model);
        UpdateTakeProfitView(model);
        UpdateRiskRewardSection(model);
        UpdateStopPriceIfNeeded(model);
        UpdateOrderTypeButton(model);
        UpdateHideShowLines(model);
        UpdateAccountValue(model);
        UpdateRiskPercent(model);
        UpdateRiskCash(model);
        UpdateRewardCash(model);
        UpdateRewardRisk(model);
        UpdatePositionSize(model);
        UpdatePipValue(model);
        UpdateAtrControls(model);
        UpdateCommission(model);
    }

    private void UpdateCommission(IModel model)
    {
        _commissionTextBox.SetValueWithoutTriggeringEvent(model.StandardCommission());
    }

    private void UpdateAtrControls(IModel model)
    {
        if (!model.IsAtrModeActive) 
            return;
        
        _atrPeriodTextBox.SetValueWithoutTriggeringEvent(model.Period);
        _atrStopLossMultiplierTextBox.SetValueWithoutTriggeringEvent(model.StopLossMultiplier);
        _atrTakeProfitMultiplierTextBox.IsReadOnly = model.TakeProfits.LockedOnStopLoss;
        _atrTakeProfitMultiplierTextBox.SetValueWithoutTriggeringEvent(model.TakeProfitMultiplier);
        _atrTimeFrameButton.Text = model.GetTimeFrameShortName();
        _atrCurrentValueTextBlock.Text = $"ATR = {model.GetAtrPips().ToString($"0.{new string('0', 2)}")}";
    }

    private void UpdatePipValue(IModel model)
    {
        if (InputShowPipValue)
            _showPipValueTextBox.SetValueWithoutTriggeringEvent(Symbol.PipValue * model.TradeSize.Volume);
    }

    private void UpdatePositionSize(IModel model)
    {
        _positionSizeTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.Lots);
        _positionSizeTextBox.ForegroundColor = model.TradeSize.IsLotsValueInvalid
            ? Color.Red
            : Color.Black;
    }

    private void UpdateRewardRisk(IModel model)
    {
        if (_invalidTpTextBox.IsVisible)
            return;
        
        if (model.TradeSize.RewardRiskRatio.IsNot(model.TradeSize.RewardRiskRatioResult, 0.01) && model.TakeProfits.List[0].Pips != 0)
        {
            _rewardRiskTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RewardRiskRatio);
            _rewardRiskTextBox.IsVisible = true;
        }
        else
        {
            _rewardRiskTextBox.IsVisible = false;
        }
        
        _rewardRiskResultTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RewardRiskRatioResult);
    }

    private void UpdateRewardCash(IModel model)
    {
        _rewardCashTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RewardInCurrency);
        _rewardCashResultTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RewardCurrencyResult);
    }

    private void UpdateRiskCash(IModel model)
    {
        _riskCashTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RiskInCurrency);
        _riskCashResultTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RiskInCurrencyResult);
    }

    private void UpdateRiskPercent(IModel model)
    {
        _riskPercentTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RiskPercentage);
        _riskPercentResultTextBox.SetValueWithoutTriggeringEvent(model.TradeSize.RiskPercentageResult);
    }

    private void UpdateHideShowLines(IModel model)
    {
        _hideLinesButton.Text = model.HideLines ? "Show Lines" : "Hide Lines";
    }

    private void UpdateAccountValue(IModel model)
    {
        if (InputHideAccountSize) 
            return;
        
        _accountSizeModeButton.Text = GetDescription(model.AccountSize.Mode);
        _accountBalanceTextBox.SetValueWithoutTriggeringEvent(model.AccountSize.Value);

        if (model.AccountSize.Mode == AccountSizeMode.Balance)
        {
            _accountBalanceTextBox.IsReadOnly = false;
            _accountBalanceTextBox.ResetProperty(ControlProperty.BackgroundColor);
            _accountBalanceTextBox.Style = CustomStyle.TextBoxStyle;
        }
        else
        {
            _accountBalanceTextBox.IsReadOnly = true;
            _accountBalanceTextBox.ResetProperty(ControlProperty.BackgroundColor);
            _accountBalanceTextBox.Style = CustomStyle.ReadOnlyTextBoxStyle;
        }

        _accountSizeAsterisk.IsVisible = GetAsteriskVisibility(model);
    }

    private bool GetAsteriskVisibility(IModel model)
    {
        var isVisible = false;

        switch (model.AccountSize.Mode)
        {
            case AccountSizeMode.Balance:
            case AccountSizeMode.BalanceCpr:
                isVisible = model.AccountSize.IsCustomBalance;
                break;
            case AccountSizeMode.Equity:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (!model.AccountSize.IsCustomBalance)
        {
            if (model.AccountSize.HasAdditionalFunds) 
                isVisible = true;
        }

        return isVisible;

        /*
         FROM MQL5
         
        switch(sets.AccountButton)
        {
        default:
        case Balance:
        case Balance_minus_Risk:
            if (sets.CustomBalance > 0)
            {
                if (!m_minimized) m_LblAdditionalFundsAsterisk.Show();
                ObjectSetString(ChartID(), m_name + "m_LblAdditionalFundsAsterisk", OBJPROP_TOOLTIP, TRANSLATION_TOOLTIP_ACCOUNT_SIZE_ASTERISK_CUSTOM);
            }
            else
            {
                m_LblAdditionalFundsAsterisk.Hide();
            }
            break;
        case Equity:
            m_LblAdditionalFundsAsterisk.Hide();
            break;
        }
        if (sets.CustomBalance <= 0)
        {
            if (AdditionalFunds != 0)
            {
                if (!m_minimized) m_LblAdditionalFundsAsterisk.Show();
                string tooltip = "";
                if (AdditionalFunds > 0) tooltip = "+" + DoubleToString(AdditionalFunds, 2) + " " + TRANSLATION_TOOLTIP_ACCOUNT_SIZE_ASTERISK_ADD;
                else if (AdditionalFunds < 0) tooltip = DoubleToString(-AdditionalFunds, 2) + " " + TRANSLATION_TOOLTIP_ACCOUNT_SIZE_ASTERISK_SUB;

                ObjectSetString(ChartID(), m_name + "m_LblAdditionalFundsAsterisk", OBJPROP_TOOLTIP, tooltip);
            }
        }
         */
    }

    private void UpdateOrderTypeButton(IModel model)
    {
        _orderTypeButton.Text = model.OrderType.ToString();
    }

    private void UpdateStopPriceIfNeeded(IModel model)
    {
        if (model.OrderType == OrderType.StopLimit)
        {
            _stopLimitPriceTextBox.SetValueWithoutTriggeringEvent(model.StopLimitPrice);

            _stopPriceTextBlock.IsVisible = true;
            _stopLimitPriceTextBox.IsVisible = true;

            if (model.StopLimitPrice < model.EntryPrice && model.TradeType == TradeType.Buy)
                _wrongStopPriceValueTextBlock.IsVisible = true;
            else if (model.StopLimitPrice > model.EntryPrice && model.TradeType == TradeType.Sell)
                _wrongStopPriceValueTextBlock.IsVisible = true;
            else
                _wrongStopPriceValueTextBlock.IsVisible = false;
        }
        else
        {
            _stopPriceTextBlock.IsVisible = false;
            _stopLimitPriceTextBox.IsVisible = false;
            _wrongStopPriceValueTextBlock.IsVisible = false;
        }
    }

    private void UpdateRiskRewardSection(IModel model)
    {
        if (model.TakeProfits.List[0].Pips == 0)
        {
            _rewardCashTextBlock.IsVisible = false;
            _rewardRiskTextBlock.IsVisible = false;
            
            _rewardCashTextBox.IsVisible = false;
            _rewardCashResultTextBox.IsVisible = false;

            _rewardRiskTextBox.IsVisible = false;
            _rewardRiskResultTextBox.IsVisible = false;

            _invalidTpTextBox.IsVisible = false;
        }
        else if (IsAnyTakeProfitInvalid(model))
        {
            _rewardCashTextBlock.IsVisible = false;
            
            _rewardCashTextBox.IsVisible = false;
            _rewardCashResultTextBox.IsVisible = false;
            
            _rewardRiskTextBox.IsVisible = false;
            _rewardRiskResultTextBox.IsVisible = false;
            
            _invalidTpTextBox.IsVisible = true;
        }
        else
        {
            _rewardCashTextBlock.IsVisible = true;
            _rewardRiskTextBlock.IsVisible = true;
            
            _rewardCashTextBox.IsVisible = true;
            _rewardCashResultTextBox.IsVisible = true;

            _rewardRiskTextBlock.IsVisible = true;
            _rewardRiskResultTextBox.IsVisible = true;
            
            _invalidTpTextBox.IsVisible = false;
        }
    }

    private void UpdateTakeProfitView(IModel model)
    {
        if (model.TakeProfits.List.Count != _tpViews.Count)
        {
            //From the model, we need to add or remove rows for the views
            if (model.TakeProfits.List.Count > _tpViews.Count)
            {
                for (var index = _tpViews.Count; index < model.TakeProfits.List.Count; index++)
                {
                    AddTakeProfitLevel(false);
                }
            }
            //from the views, we need to remove rows
            else
            {
                for (var index = model.TakeProfits.List.Count; index < _tpViews.Count; index++)
                {
                    RemoveTakeProfitLevel(false);
                }
            }
        }

        for (var index = 0; index < model.TakeProfits.List.Count; index++)
        {
            var takeProfit = model.TakeProfits.List[index];

            var tpView = _tpViews[index];

            tpView.TakeProfitTextBox.SetValueWithoutTriggeringEvent(model.TakeProfits.Mode == TargetMode.Price
                ? takeProfit.Price
                : takeProfit.Pips);
        }
    }

    private void UpdateStopLossText(IModel model)
    {
        _stopLossTb.SetValueWithoutTriggeringEvent(model.StopLoss.Mode == TargetMode.Price
            ? model.StopLoss.Price
            : model.StopLoss.Pips);
    }

    private void UpdateTakeProfitPrecision(IModel model)
    {
        if (model.TakeProfits.Mode == TargetMode.Pips)
        {
            var changeByFactor = Symbol.TickSize / Symbol.PipSize;
            var digits = CountDecimals(changeByFactor);
            
            _takeProfitTb.Digits = digits;
            _takeProfitTb.ChangeByFactor = changeByFactor;
            
            _tpViews.ForEach(view =>
            {
                view.TakeProfitTextBox.Digits = digits;
                view.TakeProfitTextBox.ChangeByFactor = changeByFactor;
            });
        }
        else
        {
            _takeProfitTb.Digits = Symbol.Digits;
            _takeProfitTb.ChangeByFactor = Symbol.TickSize;
            _tpViews.ForEach(view =>
            {
                view.TakeProfitTextBox.Digits = Symbol.Digits;
                view.TakeProfitTextBox.ChangeByFactor = Symbol.TickSize;
            });
        }
    }

    private void UpdateStopLossPrecision(IModel model)
    {
        if (model.StopLoss.Mode == TargetMode.Pips)
        {
            var changeByFactor = Symbol.TickSize / Symbol.PipSize;
            
            _stopLossTb.Digits = CountDecimals(changeByFactor);
            _stopLossTb.ChangeByFactor = changeByFactor;
        }
        else
        {
            _stopLossTb.Digits = Symbol.Digits;
            _stopLossTb.ChangeByFactor = Symbol.TickSize;
        }
    }

    private void UpdateTakeProfitLocketStatus(IModel model)
    {
        if (model.TakeProfits.LockedOnStopLoss)
        {
            _takeProfitButton.Style = CustomStyle.LockedButtonStyle;
            _tpViews[0].TakeProfitTextBox.IsReadOnly = true;
        }
        else
        {
            _takeProfitButton.Style = CustomStyle.ButtonStyle;
            _tpViews[0].TakeProfitTextBox.IsReadOnly = false;
        }
    }

    private void UpdateLongShortButton(IModel model)
    {
        _longShortButton.Text = model.TradeType == TradeType.Buy ? "Long" : "Short";
    }

    public void TrySaveTextBoxesContent()
    {
        _takeProfitTb.TryValidateText();
        _tpViews.ForEach(x => x.TakeProfitTextBox.TryValidateText());
        _stopLossTb.TryValidateText();
        _priceTargetTb.TryValidateText();
        _stopLimitPriceTextBox.TryValidateText();

        if (InputShowAtrOptions)
        {
            _atrPeriodTextBox.TryValidateText();
            _atrStopLossMultiplierTextBox.TryValidateText();
            _atrTakeProfitMultiplierTextBox.TryValidateText();
        }
        
        if (!InputHideAccountSize)
            _accountBalanceTextBox.TryValidateText();
        
        _riskPercentTextBox.TryValidateText();
        _riskCashTextBox.TryValidateText();
        _positionSizeTextBox.TryValidateText();
    }

    private TextBlock MakeTextBlock(string text)
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
            Margin = new Thickness(1),
            ForegroundColor = Color.Black,
            BackgroundColor = Color.LightGray,
        };
    }
    
    private TextBox MakeTextBox(string text)
    {
        return new TextBox
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 100,
            Height = 30,
            Margin = new Thickness(1)
        };
    }

    #region Resources

    public CustomStyle CustomStyle => _resources.CustomStyle;
    public bool InputCalculateUnadjustedPositionSize => _resources.InputCalculateUnadjustedPositionSize;
    public bool InputShowMaxPositionSizeButton => _resources.InputShowMaxPositionSizeButton;
    public AdditionalTradeButtons InputAdditionalTradeButtons => _resources.InputAdditionalTradeButtons;
    public double InputQuickRisk1Pct => _resources.InputQuickRisk1Pct;
    public double InputQuickRisk2Pct => _resources.InputQuickRisk2Pct;
    public int InputTakeProfitsNumber => _resources.InputTakeProfitsNumber;
    public bool InputShowAtrOptions => _resources.InputShowAtrOptions;
    public bool InputShowPipValue => _resources.InputShowPipValue;
    public bool InputHideAccountSize => _resources.InputHideAccountSize;
    public bool InputDisableStopLimit => _resources.InputDisableStopLimit;

    public IAccount Account => _resources.Account;
    public Symbol Symbol => _resources.Symbol;
    public double Bid => _resources.Bid;
    public double Ask => _resources.Ask;
    public Chart Chart => _resources.Chart;
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

    public Button MakeButton(string text)
    {
        return _resources.MakeButton(text);
    }

    //public KeyMultiplierFeature KeyMultiplierFeature => _resources.KeyMultiplierFeature;
    public bool InputDarkMode => _resources.InputDarkMode;

    #endregion
}