using System;

namespace cAlgo.Robots;

public class TargetLineMovedEventArgs : EventArgs
{
    public int TakeProfitId { get; set; }
    public double Price { get; set; }
    
    public TargetLineMovedEventArgs(int takeProfitId, double price)
    {
        TakeProfitId = takeProfitId;
        Price = price;
    }
}

public class ChartLineMovedEventArgs : EventArgs
{
    public double Price { get; set; }

    public ChartLineMovedEventArgs(double price)
    {
        Price = price;
    }
}

public class TargetLineRemovedEventArgs : EventArgs
{
    //need to know the id, which one was removed?
    public int TakeProfitId { get; set; }

    public TargetLineRemovedEventArgs(int takeProfitId)
    {
        TakeProfitId = takeProfitId;
    }
}