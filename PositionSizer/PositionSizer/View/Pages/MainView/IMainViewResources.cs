using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public interface IMainViewResources
{
    public CustomStyle CustomStyle { get; }
    bool InputCalculateUnadjustedPositionSize { get; }
    bool InputShowMaxPositionSizeButton { get; }
    AdditionalTradeButtons InputAdditionalTradeButtons { get; }
    double InputQuickRisk1Pct { get; }
    double InputQuickRisk2Pct { get; }
    int InputTakeProfitsNumber { get; }
    bool InputShowAtrOptions { get; }
    bool InputShowPipValue { get; }
    bool InputHideAccountSize { get; }
    bool InputDisableStopLimit { get; }
    IAccount Account { get; }
    Symbol Symbol { get; }
    double Bid { get; }
    double Ask { get; }
    Chart Chart { get; }
    void Print(object obj);
    XTextBoxDouble MakeTextBoxDouble(double defaultValue, int digits, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxDoubleNumeric MakeTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor, EventHandler<ControlValueUpdatedEventArgs<double>> valueUpdatedHandler);
    XTextBoxInt MakeTextBoxInt(int defaultValue, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxIntNumericUpDown MakeTextBoxIntNumeric(int defaultValue, int changeByFactor, EventHandler<ControlValueUpdatedEventArgs<int>> valueUpdatedHandler);
    XTextBoxString MakeTextBoxString(string defaultValue, EventHandler<ControlValueUpdatedEventArgs<string>> valueUpdatedHandler);
    Button MakeButton(string text);
    //KeyMultiplierFeature KeyMultiplierFeature { get; }
    bool InputDarkMode { get; }
    Color InputLongButtonColor { get; }
    Color InputShortButtonColor { get; }
    Color InputTradeButtonColor { get; }
}