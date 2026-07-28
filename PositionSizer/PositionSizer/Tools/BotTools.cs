using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using cAlgo.API;
using cAlgo.API.Internals;

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
            return double.PositiveInfinity;

        return (symbol.AmountRisked(position.VolumeInUnits, position.StopLossPips()) / equity) * 100.0;
    }
    
    /// <summary>
    /// Pip value in account currency for the given position size.
    /// Uses a linear rate from min-lot <see cref="Symbol.AmountRisked"/> so sub-cent values scale consistently.
    /// </summary>
    public static double GetPositionPipValue(this Symbol symbol, double lots, double volumeInUnits)
    {
        if (lots <= 0 && volumeInUnits <= 0)
            return 0;

        var volume = volumeInUnits > 0
            ? volumeInUnits
            : symbol.QuantityToVolumeInUnits(lots);

        if (volume <= 0)
            return 0;

        var normalizedVolume = symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);

        var refVolume = symbol.VolumeInUnitsMin;
        if (refVolume > 0)
        {
            var pipAtRef = symbol.AmountRisked(refVolume, 1);
            if (pipAtRef > 0)
                return pipAtRef * normalizedVolume / refVolume;
        }

        if (symbol.PipValue > 0)
        {
            var lotsNormalized = symbol.VolumeInUnitsToQuantity(normalizedVolume);
            return symbol.PipValue * lotsNormalized;
        }

        return 0;
    }

    /// <summary>
    /// Rounds to N significant figures with a minimum decimal-place floor (MT5 Position Sizer parity).
    /// </summary>
    public static double RoundToSignificant(double value, int digits = 2, int minDecimals = 2)
    {
        if (value == 0.0 || digits <= 0)
            return 0;

        var absValue = Math.Abs(value);
        var power = (int)Math.Floor(Math.Log10(absValue));

        // Guard against Log10 floating-point error near exact powers of ten.
        if (absValue >= Math.Pow(10.0, power + 1))
            power++;
        else if (absValue < Math.Pow(10.0, power))
            power--;

        var decimals = digits - 1 - power;
        if (decimals < minDecimals)
            decimals = minDecimals;

        var scale = Math.Pow(10.0, decimals);
        return Math.Round(value * scale) / scale;
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
    
    public static T NextEnumValue<T>(T value) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, value);
        return values[(index + 1) % values.Length];
    }
}