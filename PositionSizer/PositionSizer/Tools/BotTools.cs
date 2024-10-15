using System;
using System.ComponentModel;
using System.Globalization;
using cAlgo.API;

namespace cAlgo.Robots.Tools;

public static class BotTools
{
    public static double Round(this double value, int digits = 2)
    {
        return Math.Round(value, digits);
    }

    public static bool Is(this double value, double otherValue, double tolerance = double.Epsilon)
    {
        return Math.Abs(value - otherValue) < tolerance;
    }
    
    public static bool IsNot(this double value, double otherValue, double tolerance = double.Epsilon)
    {
        return !Is(value, otherValue, tolerance);
    }

    public static double StopLossPips(this Position position)
    {
        if (!position.StopLoss.HasValue)
            return 0;
        
        return position.TradeType == TradeType.Buy 
            ? ((position.EntryPrice - position.StopLoss.Value) / position.Symbol.PipSize).Round(1) 
            : ((position.StopLoss.Value - position.EntryPrice) / position.Symbol.PipSize).Round(1);
    }
    
    public static double TakeProfitPips(this Position position)
    {
        if (!position.TakeProfit.HasValue)
            return 0;
        
        return position.TradeType == TradeType.Buy 
            ? ((position.TakeProfit.Value - position.EntryPrice) / position.Symbol.PipSize).Round(1) 
            : ((position.EntryPrice - position.TakeProfit.Value) / position.Symbol.PipSize).Round(1);
    }

    public static double Ticks(this Position position)
    {
        return position.Pips * (position.Symbol.PipSize / position.Symbol.TickSize);
    }
    
    public static double PercentageIncrease(double oldValue, double newValue)
    {
        return ((newValue - oldValue) / oldValue) * 100.0;
    }

    public static double PctRisk(this Position position, double equity)
    {
        var symbol = position.Symbol;

        if (position.StopLossPips() <= 0)
            return 0;

        return (symbol.AmountRisked(position.VolumeInUnits, position.StopLossPips()) / equity) * 100.0;
    }
    
    public static int CountDecimals(double value)
    {
        if (value == 0)
        {
            return 0;
        }

        // Convert to string with high precision
        var strValue = value.ToString("0.#############################", CultureInfo.InvariantCulture);
        var decimalIndex = strValue.IndexOf('.');
        if (decimalIndex == -1)
        {
            return 0;
        }

        // Subtract 1 to exclude the decimal point itself
        var decimalCount = strValue.Length - decimalIndex - 1;

        // Remove trailing zeros
        while (strValue[^1] == '0')
        {
            strValue = strValue.Remove(strValue.Length - 1);
            decimalCount--;
        }

        return decimalCount;
    }
    
    public static string GetDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo == null)
            return value.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes is { Length: > 0 } ? attributes[0].Description : value.ToString();
    }
}