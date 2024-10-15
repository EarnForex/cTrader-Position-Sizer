using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo.Robots;

public enum SizeDistributionMode
{
    Ascending,
    Descending,
    EquallyDistributed
}

public class TakeProfits
{
    public int Decimals { get; set; }
    public bool LockedOnStopLoss { get; set; }
    public double LockedMultiplier { get; set; }
    public bool Blocked { get; set; }
    public TargetMode Mode { get; set; }
    public SizeDistributionMode SizeDistributionMode { get; set; } = SizeDistributionMode.EquallyDistributed;

    /// <summary>
    /// Distribution must be either equal to 99 or 100
    /// </summary>
    public bool DistributionAddsUp
    {
        get
        {
            var distributionSum = (int) List.Sum(x => x.Distribution);
            return distributionSum is 99 or 100;
        }
    }
    public double CommissionPipsExtra { get; set; }
    public List<TakeProfit> List { get; set; } = new();

    public TakeProfits()
    {
        
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"TP Settings: Decimals: {Decimals}, LockedOnStopLoss: {LockedOnStopLoss}, LockedMultiplier: {LockedMultiplier}, Blocked: {Blocked}, Mode: {Mode}, SizeDistributionMode: {SizeDistributionMode}, CommissionPipsExtra: {CommissionPipsExtra}");
        
        for (var index = 0; index < List.Count; index++)
        {
            var tp = List[index];
            sb.AppendLine($"==== TP#{index} ====");
            sb.AppendLine($"{tp}");
        }

        return sb.ToString();
    }
}

public class TakeProfit
{
    public double Price { get; set; }
    public double Pips { get; set; }
    public double Distribution { get; set; }

    public override string ToString()
    {
        return $"(Price: {Price}, Pips: {Pips}, Distribution: {Distribution})";
    }
}