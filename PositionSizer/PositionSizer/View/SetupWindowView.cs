using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public interface ISetupWindowResources
{
    IModel Model { get; }
    public CustomStyle CustomStyle { get; }
    IAccount Account { get; }
    IServer Server { get; }
    Chart Chart { get; }
    void Print(object obj);
    event EventHandler TimerEvent;
    Symbol Symbol { get; }
    double Ask { get; }
    double Bid { get; }
    void Stop();
    int IndexForLabelReference { get; }
    bool InputCalculateUnadjustedPositionSize { get; } 
    bool InputShowAdditionalStopLossLabel { get; }
    bool InputShowAdditionalTpLabel { get; }
    bool InputShowAdditionalEntryLabel { get; }
    bool InputShowPipValue { get; }
    //--
    bool InputShowMaxPositionSizeButton { get; }
    bool InputStartPanelMinimized { get; }
    bool InputShowMainLineLabels { get; }
    bool InputHideEntryLineForInstantOrders { get; }
    Color InputEntryLineColor { get; }
    LineStyle InputEntryLineStyle { get; }
    int InputEntryLineWidth { get; }
    double InputQuickRisk1Pct { get; }
    double InputQuickRisk2Pct { get; }
    bool InputDisableStopLimit { get; }
    Color InputStopLossLineColor { get; }
    LineStyle InputStopLossLineStyle { get; }
    int InputStopLossLineWidth { get; }
    
    Color InputTakeProfitLineColor { get; }
    LineStyle InputTakeProfitLineStyle { get; }
    int InputTakeProfitLineWidth { get; }
    
    Color InputStopLossLabelColor { get; }
    Color InputTpLabelColor { get; }
    Color InputStopPriceLabelColor { get; }
    Color InputEntryLabelColor { get; }
    int InputTakeProfitsNumber { get; }
    int InputLabelsFontSize { get; }
    ShowSpreadMode InputShowSpread { get; }
    int InputPanelPositionX { get;  }
    int InputPanelPositionY { get; }
    bool InputHideAccountSize { get; }
    LineStyle InputStopPriceLineStyle { get; }
    int InputStopPriceLineWidth { get; }
    Color InputStopPriceLineColor { get; }
    bool InputShowAtrOptions { get; }
    bool InputShowMaxParametersOnTradingTab { get; }
    bool InputShowTradingFusesOnTradingTab { get; }
    AdditionalTradeButtons InputAdditionalTradeButtons { get; }
    bool InputShowCheckBoxesOnTradingTab { get; }
    IAssetConverter AssetConverter { get; }
    bool InputRestoreWindowLocationOnChartSizeChange { get; }
    bool InputAskForConfirmationBeforeClosingThePanel { get; }
    XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxDoubleNumeric MakeTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxInt MakeTextBoxInt(int defaultValue, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxIntNumericUpDown MakeTextBoxIntNumeric(int defaultValue, int changeByFactor, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler);
    Button MakeButton(string text);
    //KeyMultiplierFeature KeyMultiplierFeature { get; }
    bool InputDarkMode { get; }
    LocalStorage LocalStorage { get; }
    string CleanBrokerName { get; }
    Color InputLongButtonColor { get; }
    Color InputShortButtonColor { get; }
    Color InputTradeButtonColor { get; }
    double InputFallbackLotSize { get; }
}

public enum WindowActive
{
    Main,
    Risk,
    Margin,
    Swaps,
    Trading
}

public class WindowActiveChangedEventArgs : EventArgs
{
    public WindowActive WindowActive { get; }

    public WindowActiveChangedEventArgs(WindowActive windowActive)
    {
        WindowActive = windowActive;
    }
}

public class LastKnownState
{
    public WindowActive WindowActive { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public sealed class SetupWindowView : Grid, 
    ISetupWindowResources,
    ISelectorViewResources,
    IMainViewResources,
    IRiskViewResources,
    IChartLinesViewResources,
    ITradingViewResources,
    IMarginViewResources,
    ISwapsViewResources
{
    private readonly ISetupWindowResources _resources;
    private readonly Button _hideShow;
    private readonly Button _move;
    private readonly Button _close;
    private bool _dragWindow;
    private bool _refreshLag;
    private readonly StackPanel _stackPanel;
    private readonly SelectorView _selectorView;
    private readonly TextBlock _title;
    public MainView MainView { get; }
    public RiskView RiskView { get; }
    public MarginView MarginView { get; }
    public SwapsView SwapsView { get; }
    public TradingView TradingView { get; set; }
    public ChartLinesView ChartLinesView { get; }

    public ControlBase ViewUsed { get; private set; }
    private bool _selectorVisible = true;

    private const int WindowWidth = 400;

    public event EventHandler HideShowClickEvent;
    
    public event EventHandler<WindowActiveChangedEventArgs> WindowActiveChanged; 
    
    public string TitleAndVersion { get; set; }
    
    private DateTime _lastTimeCheckedForWindow = DateTime.MinValue;
    private double _lastKnownChartWidth, _lastKnownChartHeight;
    
    //triangle up icon is \ud83d\uddd5
    public string DirectionArrow => Model.TradeType == TradeType.Buy ? "\u25B2" : "\u25BC";

    public SetupWindowView(ISetupWindowResources resources)
    {
        _resources = resources;
        
        AddRows(3);
        AddColumn();

        TitleAndVersion = $"{DirectionArrow} Position Sizer ({Model.Version})";
        
        //ShowGridLines = true;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Width = WindowWidth;
        Margin = new Thickness(InputPanelPositionX, InputPanelPositionY, 0, 0);
        _lastKnownChartWidth = Chart.Width;
        _lastKnownChartHeight = Chart.Height;

        //first row is for a stack panel which contains buttons to hide/show, move and close the window
        var topGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            BackgroundColor = Color.Gray,
            Width = WindowWidth
        };

        topGrid.AddRow();
        topGrid.AddColumns(4);
        
        _title = new TextBlock
        {
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Text = TitleAndVersion
        };
        
        var wasHidden = LocalStorage.GetString($"{CleanBrokerName}-wasHidden");

        _hideShow = new Button
        {
            //Text = InputStartPanelMinimized ? "Show" : "Hide",
            Text = InputStartPanelMinimized || wasHidden == "Y" ? "🗗" : "\ud83d\uddd5",
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };
        
        _hideShow.Click += HideShow_Click;
        
        _move = new Button
        {
            // Text = $"Move ({InputPanelPositionX},{InputPanelPositionY})",
            Text = $"Move",
            Margin = new Thickness(5, 5, 5, 5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        };
        
        _move.Click += Move_Click;
        
        _close = new Button
        {
            Text = "\ud83d\uddd9",
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        
        _close.Click += Close_Click;
        
        topGrid.AddChild(_title, 0, 0);
        topGrid.Columns[0].SetWidthToAuto();
        
        topGrid.AddChild(_move, 0, 1);
        
        topGrid.AddChild(_hideShow, 0, 2);
        topGrid.Columns[2].SetWidthToAuto();
        
        topGrid.AddChild(_close, 0, 3);
        topGrid.Columns[3].SetWidthToAuto();
        
        AddChild(topGrid, 0, 0);
        Rows[0].SetHeightToAuto();
        
        _stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            BackgroundColor = Color.Gray,
            //Height = 800,
            Width = WindowWidth
        };
        
        _selectorView = new SelectorView(this)
        {
            //Width = WindowWidth,
            HorizontalAlignment = HorizontalAlignment.Center,
            //BackgroundColor = Color.White,
            ShowGridLines = false
        };

        _stackPanel.AddChild(_selectorView);
        
        AddChild(_stackPanel, 1, 0);
        Rows[1].SetHeightToAuto();
        
        _selectorView.MainButtonClick += SelectorViewOnMainButtonClick;
        _selectorView.RiskButtonClick += SelectorViewOnRiskButtonClick;
        _selectorView.MarginButtonClick += SelectorViewOnMarginButtonClick;
        _selectorView.SwapsButtonClick += SelectorViewOnSwapsButtonClick;
        _selectorView.TradingButtonClick += SelectorViewOnTradingButtonClick;

        MainView = new MainView(this, Model)
        {
            Style = CustomStyle.BackgroundStyle,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        
        ViewUsed = MainView;
        _stackPanel.AddChild(ViewUsed);

        RiskView = new RiskView(this, Model)
        {
            Style = CustomStyle.BackgroundStyle,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        
        MarginView = new MarginView(this)
        {
            Style = CustomStyle.BackgroundStyle,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        
        SwapsView = new SwapsView(this)
        {
            Style = CustomStyle.BackgroundStyle,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        
        TradingView = new TradingView(this, Model)
        {
            Style = CustomStyle.BackgroundStyle,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        
        ChartLinesView = new ChartLinesView(this, Model);
        
        Chart.MouseDown += Chart_MouseDown;
        Chart.MouseMove += Chart_MouseMove;
        TimerEvent += OnTimerEvent;
        
        Chart.AddControl(this);
        
        if (InputStartPanelMinimized || wasHidden == "Y")
            HideShow_Click(null);
    }

    public void Update(IModel model)
    {
        ChartLinesView.UpdateLines(model);
        UpdateSpread(model);
        MainView.Update(model);
        RiskView.Update(model);
        TradingView.UpdateValues(model);
        MarginView.Update(model);
        SwapsView.Update(model);
        
        //Chart.DrawStaticText("PositionSizer", model.MainModel.ToString(), VerticalAlignment.Bottom, HorizontalAlignment.Right, Color.Red);
    }
    
    public void UpdateState(LastKnownState lastKnownState)
    {
        switch (lastKnownState.WindowActive)
        {
            case WindowActive.Main:
                SelectorViewOnMainButtonClick(null);
                break;
            case WindowActive.Risk:
                SelectorViewOnRiskButtonClick(null);
                break;
            case WindowActive.Margin:
                SelectorViewOnMarginButtonClick(null);
                break;
            case WindowActive.Swaps:
                SelectorViewOnSwapsButtonClick(null);
                break;
            case WindowActive.Trading:
                SelectorViewOnTradingButtonClick(null);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Margin = new Thickness(lastKnownState.X, lastKnownState.Y, 0, 0);
        // _move.Text = $"Move ({lastKnownState.X},{lastKnownState.Y})";
    }

    private void SelectorViewOnTradingButtonClick(ButtonClickEventArgs obj)
    {
        if (ViewUsed == TradingView)
            return;
        
        TradingView.Dispose();
        TradingView = new TradingView(this, Model)
        {
            Style = CustomStyle.BackgroundStyle,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        
        OnWindowActiveChanged(new WindowActiveChangedEventArgs(WindowActive.Trading));
        
        _stackPanel.RemoveChild(ViewUsed);
        ViewUsed = TradingView;
        _stackPanel.AddChild(ViewUsed);
    }

    private void SelectorViewOnSwapsButtonClick(ButtonClickEventArgs obj)
    {
        if (ViewUsed == SwapsView)
            return;
        
        OnWindowActiveChanged(new WindowActiveChangedEventArgs(WindowActive.Swaps));
        
        _stackPanel.RemoveChild(ViewUsed);
        ViewUsed = SwapsView;
        _stackPanel.AddChild(ViewUsed);
    }

    private void SelectorViewOnMarginButtonClick(ButtonClickEventArgs obj)
    {
        if (ViewUsed == MarginView)
            return;
        
        OnWindowActiveChanged(new WindowActiveChangedEventArgs(WindowActive.Margin));
        
        _stackPanel.RemoveChild(ViewUsed);
        ViewUsed = MarginView;
        _stackPanel.AddChild(ViewUsed);
    }

    private void SelectorViewOnRiskButtonClick(ButtonClickEventArgs obj)
    {
        if (ViewUsed == RiskView)
            return;
        
        OnWindowActiveChanged(new WindowActiveChangedEventArgs(WindowActive.Risk));

        _stackPanel.RemoveChild(ViewUsed);
        ViewUsed = RiskView;
        _stackPanel.AddChild(ViewUsed);
    }

    private void SelectorViewOnMainButtonClick(ButtonClickEventArgs obj)
    {
        if (ViewUsed == MainView)
            return;
        
        OnWindowActiveChanged(new WindowActiveChangedEventArgs(WindowActive.Main));
        
        _stackPanel.RemoveChild(ViewUsed);
        ViewUsed = MainView;
        _stackPanel.AddChild(ViewUsed);
    }

    private void OnTimerEvent(object sender, EventArgs args)
    {
        _refreshLag = !_refreshLag;

        if (_lastTimeCheckedForWindow == DateTime.MinValue)
        {
            _lastTimeCheckedForWindow = Server.Time;
            return;
        }
        
        if (Server.Time.Subtract(_lastTimeCheckedForWindow).TotalSeconds > 1)
        {
            _lastTimeCheckedForWindow = Server.Time;
            CheckOrFixBorders();
        }
    }

    private void CheckOrFixBorders()
    {
        if (!InputRestoreWindowLocationOnChartSizeChange)
            return;
        
        if (Chart.Width.Is(_lastKnownChartWidth) && Chart.Height.Is(_lastKnownChartHeight))
            return;

        var left = Margin.Left;

        if (Chart.Width.IsNot(_lastKnownChartWidth))
        {
            if (Chart.Width < _lastKnownChartWidth)
                if (Chart.Width < Margin.Left + WindowWidth)
                    left = 10;

            _lastKnownChartWidth = Chart.Width;
        }
        
        var top = Margin.Top;
        
        if (Chart.Height.IsNot(_lastKnownChartHeight))
        {
            if (Chart.Height < _lastKnownChartHeight)
                if (Chart.Height < Margin.Top + 350)
                    top = 10;

            _lastKnownChartHeight = Chart.Height;
        }
        
        Margin = new Thickness(left, top, 0, 0);
        
        Model.LastKnownState.X = (int) left;
        Model.LastKnownState.Y = (int) top;
        
        // _move.Text = $"Move ({left},{top})";
    }

    private void Close_Click(ButtonClickEventArgs obj)
    {
        if (InputAskForConfirmationBeforeClosingThePanel)
        {
            var result = MessageBox.Show("Are you sure you want to close the Position Sizer?"
                , "Close Position Sizer"
                , MessageBoxButton.YesNo, MessageBoxImage.Question);
        
            if (result != MessageBoxResult.Yes)
                return;
        }

        Stop();
    }

    private void Move_Click(ButtonClickEventArgs obj)
    {
        _dragWindow = !_dragWindow;
    }

    private void Chart_MouseMove(ChartMouseEventArgs obj)
    {
        if (_refreshLag)
            return;
        
        //Print("Chart_MouseMove Event");
        
        if (!_dragWindow)
            return;
        
        var x = (int)(Math.Min(Chart.Width - 400, obj.MouseX * 1.05));
        var y = IsMinimized 
                ? (int)(Math.Min(Chart.Height - 30, obj.MouseY * 1.05)) 
            : (int)(Math.Min(Chart.Height - 350, obj.MouseY * 1.05));

        Model.LastKnownState.X = x;
        Model.LastKnownState.Y = y;
        
        Margin = new Thickness(x, y, 0, 0);
        //_move.Text = $"Move ({x},{y})";
    }

    private void Chart_MouseDown(ChartMouseEventArgs obj)
    {
        //Print("Chart_MouseDown Event");
        
        if (_dragWindow)
            _dragWindow = false;
    }

    private void HideShow_Click(ButtonClickEventArgs obj)
    {
        HideOrShow();
    }
    
    public void UpdateSpread(IModel model)
    {
        TitleAndVersion = $"{DirectionArrow} Position Sizer ({model.Version})";
        
        _title.Text = InputShowSpread switch
        {
            ShowSpreadMode.None => TitleAndVersion,
            ShowSpreadMode.Pips => $"{TitleAndVersion} - Spread: {Symbol.Spread / Symbol.PipSize:F1}",
            ShowSpreadMode.Spread_SL_Ratio =>
                $"{TitleAndVersion} - Spread: {((Symbol.Spread / Symbol.PipSize) / model.StopLoss.Pips):P2}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public bool IsMinimized => !_selectorVisible;

    public void HideOrShow()
    {
        //selector will be the reference to see if the window is visible or not
        _selectorVisible = !_selectorVisible;

        if (_selectorVisible)
        {
            _hideShow.Text = "\ud83d\uddd5";

            _stackPanel.AddChild(_selectorView);
            _stackPanel.RemoveChild(ViewUsed);
            
            LocalStorage.SetString($"{CleanBrokerName}-wasHidden", "N", LocalStorageScope.Instance);
            LocalStorage.Flush(LocalStorageScope.Instance);

            ViewUsed = Model.LastKnownState.WindowActive switch
            {
                WindowActive.Main => MainView,
                WindowActive.Trading => TradingView,
                WindowActive.Risk => RiskView,
                WindowActive.Margin => MarginView,
                WindowActive.Swaps => SwapsView,
                _ => throw new ArgumentOutOfRangeException()
            };

            _stackPanel.AddChild(ViewUsed);
        }
        else
        {
            _hideShow.Text = "🗗";

            LocalStorage.SetString($"{CleanBrokerName}-wasHidden", "Y", LocalStorageScope.Instance);
            LocalStorage.Flush(LocalStorageScope.Instance);
            
            _stackPanel.RemoveChild(_selectorView);
            _stackPanel.RemoveChild(ViewUsed);
        }

        OnHideShowClickEvent();
    }

    public IModel Model => _resources.Model;

    public CustomStyle CustomStyle => _resources.CustomStyle;

    public IAccount Account => _resources.Account;
    public IServer Server => _resources.Server;

    public Chart Chart => _resources.Chart;

    public void Print(object obj)
    {
        _resources.Print(obj);
    }

    public bool InputAskForConfirmationBeforeClosingThePanel => _resources.InputAskForConfirmationBeforeClosingThePanel;

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

    public event EventHandler TimerEvent
    {
        add => _resources.TimerEvent += value;
        remove => _resources.TimerEvent -= value;
    }

    public Symbol Symbol => _resources.Symbol;

    public double Ask => _resources.Ask;

    public double Bid => _resources.Bid;

    public void Stop()
    {
        _resources.Stop();
    }

    public int IndexForLabelReference => _resources.IndexForLabelReference;
    public bool InputCalculateUnadjustedPositionSize => _resources.InputCalculateUnadjustedPositionSize;
    public bool InputHideAccountSize => _resources.InputHideAccountSize;
    public bool InputShowAdditionalStopLossLabel => _resources.InputShowAdditionalStopLossLabel;
    public bool InputShowAdditionalTpLabel => _resources.InputShowAdditionalTpLabel;
    public bool InputShowAdditionalEntryLabel => _resources.InputShowAdditionalEntryLabel;
    public bool InputShowAtrOptions => _resources.InputShowAtrOptions;
    public bool InputShowMaxParametersOnTradingTab => _resources.InputShowMaxParametersOnTradingTab;
    public bool InputShowTradingFusesOnTradingTab => _resources.InputShowTradingFusesOnTradingTab;
    public AdditionalTradeButtons InputAdditionalTradeButtons => _resources.InputAdditionalTradeButtons;
    public bool InputShowCheckBoxesOnTradingTab => _resources.InputShowCheckBoxesOnTradingTab;
    public IAssetConverter AssetConverter => _resources.AssetConverter;
    public bool InputRestoreWindowLocationOnChartSizeChange => _resources.InputRestoreWindowLocationOnChartSizeChange;

    public bool InputShowPipValue => _resources.InputShowPipValue;
    public bool InputShowMaxPositionSizeButton => _resources.InputShowMaxPositionSizeButton;
    public bool InputStartPanelMinimized => _resources.InputStartPanelMinimized;
    public bool InputShowMainLineLabels => _resources.InputShowMainLineLabels;
    public bool InputHideEntryLineForInstantOrders => _resources.InputHideEntryLineForInstantOrders;

    public Color InputEntryLineColor => _resources.InputEntryLineColor;
    public LineStyle InputEntryLineStyle => _resources.InputEntryLineStyle;
    public int InputEntryLineWidth => _resources.InputEntryLineWidth;
    public double InputQuickRisk1Pct => _resources.InputQuickRisk1Pct;
    public double InputQuickRisk2Pct => _resources.InputQuickRisk2Pct;
    public bool InputDisableStopLimit => _resources.InputDisableStopLimit;

    public Color InputStopLossLineColor => _resources.InputStopLossLineColor;
    public LineStyle InputStopLossLineStyle => _resources.InputStopLossLineStyle;
    public int InputStopLossLineWidth => _resources.InputStopLossLineWidth;
    public Color InputTakeProfitLineColor => _resources.InputTakeProfitLineColor;
    public LineStyle InputTakeProfitLineStyle => _resources.InputTakeProfitLineStyle;
    public int InputTakeProfitLineWidth => _resources.InputTakeProfitLineWidth;
    public Color InputStopLossLabelColor => _resources.InputStopLossLabelColor;
    public Color InputTpLabelColor => _resources.InputTpLabelColor;
    public Color InputStopPriceLabelColor => _resources.InputStopPriceLabelColor;
    public Color InputEntryLabelColor => _resources.InputEntryLabelColor;
    public int InputTakeProfitsNumber => _resources.InputTakeProfitsNumber;

    public int InputLabelsFontSize => _resources.InputLabelsFontSize;
    public ShowSpreadMode InputShowSpread => _resources.InputShowSpread;
    public int InputPanelPositionX => _resources.InputPanelPositionX;
    public int InputPanelPositionY => _resources.InputPanelPositionY;
    public LineStyle InputStopPriceLineStyle => _resources.InputStopPriceLineStyle;
    public int InputStopPriceLineWidth => _resources.InputStopPriceLineWidth;
    public Color InputStopPriceLineColor => _resources.InputStopPriceLineColor;
    public Button MakeButton(string text) => _resources.MakeButton(text);
    //public KeyMultiplierFeature KeyMultiplierFeature => _resources.KeyMultiplierFeature;
    public bool InputDarkMode => _resources.InputDarkMode;
    public Color InputLongButtonColor => _resources.InputLongButtonColor;
    public Color InputShortButtonColor => _resources.InputShortButtonColor;
    public Color InputTradeButtonColor => _resources.InputTradeButtonColor;

    public LocalStorage LocalStorage => _resources.LocalStorage;
    public string CleanBrokerName => _resources.CleanBrokerName;
    public double InputFallbackLotSize => _resources.InputFallbackLotSize;

    private void OnHideShowClickEvent()
    {
        HideShowClickEvent?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowActiveChanged(WindowActiveChangedEventArgs e)
    {
        WindowActiveChanged?.Invoke(this, e);
    }
}