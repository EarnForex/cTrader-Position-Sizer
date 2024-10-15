using System;

namespace cAlgo.Robots;

public enum EntryPriceUpdateReason
{
    EntryLineMoved,
    SetEntryWhereMouseIs,
    TargetPriceChanged,
    TickUpdate
}

public class EntryPriceUpdatedEventArgs : EventArgs
{
    public double EntryPrice { get; set; }
    public EntryPriceUpdateReason Reason { get; set; }
    
    public EntryPriceUpdatedEventArgs(double entryPrice, EntryPriceUpdateReason reason)
    {
        EntryPrice = entryPrice;
        Reason = reason;
    }
}