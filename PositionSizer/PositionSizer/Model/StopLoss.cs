using System;
using System.Diagnostics;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class StopLoss
{
    public double Price { get; set; }
    
    private double _pips;
    public double Pips
    {
        get => _pips;
        set
        {
            if (value <= 0)
            {
                Debug.WriteLine("Exception Caught");
                throw new ArgumentException("Pips value cannot be less than or equal to zero");
            }

            _pips = value;
        }
    }
    public bool Blocked { get; set; }
    public TargetMode Mode { get; set; }
    public bool HasDefaultSwitch { get; set; }
    public double InitialDefaultValuePips { get; set; }

    public StopLoss()
    {
        
    }

    public override string ToString()
    {
        return $"Price: {Price}, Pips: {Pips}, Blocked: {Blocked}, Mode: {Mode}";
    }
}