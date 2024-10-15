using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots.RiskManagers;

public interface IBreakEvenResources
{
    IEnumerable<Position> PositionsByLabelAndComment { get; }
    Symbol Symbol { get; }
    double Bid { get; }
    double Ask { get; }
    TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit);
    Positions Positions { get; }
    Color InputBreakevenLineColor { get; }
    LineStyle InputBreakevenLineStyle { get; }
    int InputBreakevenLineWidth { get; }
    Chart Chart { get; }
    IModel Model { get; }
}

public class BreakEven : IRiskManager, IBreakEvenResources
{
    private readonly IBreakEvenResources _resources;

    private readonly List<int> _positionsMovedToBreakeven = new();

    public BreakEven(IBreakEvenResources resources)
    {
        _resources = resources;
        
        Positions.Closed += OnPositionClosed; 
    }

    private void OnPositionClosed(PositionClosedEventArgs obj)
    {
        if (Chart.FindObject($"Breakeven-{obj.Position.Id}") != null)
            Chart.RemoveObject($"Breakeven-{obj.Position.Id}");
    }

    public void Check()
    {
        foreach (var pos in PositionsByLabelAndComment)
        {
            if (_positionsMovedToBreakeven.Contains(pos.Id))
                continue;

            if (Model.BreakEvenPips == 0)
                continue;

            if (pos.Pips < Model.BreakEvenPips)
            {
                var trigger = pos.TradeType == TradeType.Buy ? pos.EntryPrice + Model.BreakEvenPips * pos.Symbol.PipSize : pos.EntryPrice - Model.BreakEvenPips * pos.Symbol.PipSize;
                Chart.DrawHorizontalLine($"Breakeven-{pos.Id}", trigger, InputBreakevenLineColor, InputBreakevenLineWidth, InputBreakevenLineStyle);
                continue;
            }

            var newStopLossPrice = pos.EntryPrice;
            ModifyPosition(pos, newStopLossPrice, pos.TakeProfit);
            _positionsMovedToBreakeven.Add(pos.Id);
        }
    }
    
    public void UpdateTriggers()
    {
        if (Model.BreakEvenPips == 0)
        {
            DeleteTriggerLines();
            return;
        }

        foreach (var pos in PositionsByLabelAndComment)
        {
            if (pos.Pips < Model.BreakEvenPips)
            {
                var trigger = pos.TradeType == TradeType.Buy ? pos.EntryPrice + Model.BreakEvenPips * pos.Symbol.PipSize : pos.EntryPrice - Model.BreakEvenPips * pos.Symbol.PipSize;
                Chart.DrawHorizontalLine($"Breakeven-{pos.Id}", trigger, InputBreakevenLineColor, InputBreakevenLineWidth, InputBreakevenLineStyle);
            }
        }
    }

    private void DeleteTriggerLines()
    {
        var horizontalLines = Chart.FindAllObjects<ChartHorizontalLine>();

        foreach (var hLine in horizontalLines)
        {
            if (hLine.Name.Contains("Breakeven"))
                Chart.RemoveObject(hLine.Name);
        }
    }

    #region Resources
    
    public IEnumerable<Position> PositionsByLabelAndComment => _resources.PositionsByLabelAndComment;
    public Symbol Symbol => _resources.Symbol;
    public double Bid => _resources.Bid;
    public double Ask => _resources.Ask;
    public Positions Positions => _resources.Positions;
    public Color InputBreakevenLineColor => _resources.InputBreakevenLineColor;
    public LineStyle InputBreakevenLineStyle => _resources.InputBreakevenLineStyle;
    public int InputBreakevenLineWidth => _resources.InputBreakevenLineWidth;
    public Chart Chart => _resources.Chart;
    public IModel Model => _resources.Model;

    public TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit)
    {
        return _resources.ModifyPosition(position, stopLoss, takeProfit);
    }

    #endregion
}