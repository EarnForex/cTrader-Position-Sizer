using System;
using cAlgo.API;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public partial class Model
{
    /// <summary>
    /// Distance (in pips) between the Stop Limit price and the entry price.
    /// Always positive for a valid configuration (buy stop-limit above entry, sell stop-limit below entry).
    /// </summary>
    public double StopLimitPips()
    {
        return Math.Round(Math.Abs(StopLimitPrice - EntryPrice) / Symbol.PipSize, 1);
    }

    /// <summary>
    /// Sets the Stop Limit price from a pips distance to the entry price.
    /// Buy stop-limit is placed above entry, sell stop-limit below entry.
    /// </summary>
    public void ChangeStopLimitPips(double pips)
    {
        var rounded = Math.Abs(Math.Round(pips, 1));
        StopLimitPipsDistance = rounded;
        var sign = TradeType == TradeType.Buy ? 1 : -1;
        StopLimitPrice = EntryPrice + sign * rounded * Symbol.PipSize;
    }

    public void SyncStopLimitPipsDistanceFromPrice()
    {
        if (StopLimitMode == TargetMode.Pips && StopLimitPrice != 0)
            StopLimitPipsDistance = StopLimitPips();
    }
}
