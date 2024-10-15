using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots.RiskManagers;

public interface ITrailingStopResources
{
    public IEnumerable<Position> PositionsByLabelAndComment { get; }
    Symbol Symbol { get; }
    double Bid { get; }
    double Ask { get; }
    TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit);
    IModel Model { get; }
}

public class TrailingStop : IRiskManager, ITrailingStopResources
{
    private readonly ITrailingStopResources _resources;

    private const double _inputTrigger = 0;

    public TrailingStop(ITrailingStopResources resources)
    {
        _resources = resources;
    }
    
    public void Check()
    {
        foreach (var pos in PositionsByLabelAndComment)
        {
            if (Model.TrailingStopPips == 0)
                continue;
            
            if (pos.TradeType == TradeType.Buy)
            {
                var newStopLossPrice = Math.Round(Symbol.Bid - Model.TrailingStopPips * Symbol.PipSize, Symbol.Digits);
                if (!pos.StopLoss.HasValue || newStopLossPrice > pos.StopLoss) 
                    ModifyPosition(pos, newStopLossPrice, pos.TakeProfit);
            }
            else
            {
                var newStopLossPrice = Math.Round(Symbol.Ask + Model.TrailingStopPips * Symbol.PipSize, Symbol.Digits);
                if (!pos.StopLoss.HasValue || newStopLossPrice < pos.StopLoss) 
                    ModifyPosition(pos, newStopLossPrice, pos.TakeProfit);
            }
        }
    }
    
    public IEnumerable<Position> PositionsByLabelAndComment => _resources.PositionsByLabelAndComment;
    public Symbol Symbol => _resources.Symbol;
    public double Bid => _resources.Bid;
    public double Ask => _resources.Ask;
    public TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit)
    {
        return _resources.ModifyPosition(position, stopLoss, takeProfit);
    }
    public IModel Model => _resources.Model;
}