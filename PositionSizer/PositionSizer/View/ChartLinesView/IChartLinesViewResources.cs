using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public interface IChartLinesViewResources
{
    IAccount Account { get; }
    Chart Chart { get; }
    Symbol Symbol { get; }
    IServer Server { get; }
    bool InputHideEntryLineForInstantOrders { get; }
    Color InputEntryLineColor { get; }
    LineStyle InputEntryLineStyle { get; }
    int InputEntryLineWidth { get; }
    bool InputShowMainLineLabels { get; }
    int IndexForLabelReference { get; }

    Color InputStopLossLineColor { get; }
    LineStyle InputStopLossLineStyle { get; }
    int InputStopLossLineWidth { get; }

    Color InputTakeProfitLineColor { get; }
    LineStyle InputTakeProfitLineStyle { get; }
    int InputTakeProfitLineWidth { get; }

    bool InputShowAdditionalStopLossLabel { get; }
    bool InputShowAdditionalTpLabel { get; }
    bool InputShowAdditionalEntryLabel { get; }
    
    Color InputStopLossLabelColor { get; }
    Color InputTpLabelColor { get; }
    Color InputStopPriceLabelColor { get; }
    Color InputEntryLabelColor { get; }
    int InputLabelsFontSize { get; }
    
    Color InputStopPriceLineColor { get; }
    LineStyle InputStopPriceLineStyle { get; }
    int InputStopPriceLineWidth { get; }
    
    AdditionalTradeButtons InputAdditionalTradeButtons { get; }
    
    event EventHandler TimerEvent;
    Button MakeButton(string text);
    void Print(object obj);
    
    CustomStyle CustomStyle { get; }
    Color InputTradeButtonColor { get; }
}