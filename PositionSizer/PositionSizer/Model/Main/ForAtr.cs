using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots;

public partial class Model
{
    #region ForAtrSection
    
    public bool IsAtrModeActive { get; set; }
    
    public int Period { get; set; }
    public double StopLossMultiplier { get; set; }
    public double TakeProfitMultiplier { get; set; }
    public bool StopLossSpreadAdjusted { get; set; }
    public bool TakeProfitSpreadAdjusted { get; set; }
    public SerializableTimeFrame AtrTimeFrame { get; set; }
    private AverageTrueRange AverageTrueRange { get; set; }
    public AtrCandle AtrCandle { get; set; }

    public void SetAtrIndicator(AverageTrueRange averageTrueRange)
    {
        AverageTrueRange = averageTrueRange;
    }

    public double GetAtrPips() =>
        AtrCandle == AtrCandle.CurrentCandle
            ? AverageTrueRange.Result.LastValue / Symbol.PipSize
            : AverageTrueRange.Result.Last(1) / Symbol.PipSize;
    
    public double GetAtr() => 
        AtrCandle == AtrCandle.CurrentCandle
            ? AverageTrueRange.Result.LastValue
            : AverageTrueRange.Result.Last(1);

    /// <summary>
    /// This method selects the next timeframe in order
    /// The timeframe list must be picked and ordered in a loop
    /// if the current timeframe is Minute, the next timeframe will be Minute2
    /// if the current timeframe is Minute30, the next timeframe will be Hour
    /// and so on, until the last timeframe is reached then it will start over
    /// </summary>
    // ReSharper disable once CognitiveComplexity
    public void GetNextAtr()
    {
        var atrTimeFrame = AtrTimeFrame.ToTimeFrame();
        
        if (atrTimeFrame == TimeFrame.Minute)
            atrTimeFrame = TimeFrame.Minute2;
        else if (atrTimeFrame == TimeFrame.Minute2)
            atrTimeFrame = TimeFrame.Minute3;
        else if (atrTimeFrame == TimeFrame.Minute3)
            atrTimeFrame = TimeFrame.Minute4;
        else if (atrTimeFrame == TimeFrame.Minute4)
            atrTimeFrame = TimeFrame.Minute5;
        else if (atrTimeFrame == TimeFrame.Minute5)
            atrTimeFrame = TimeFrame.Minute6;
        else if (atrTimeFrame == TimeFrame.Minute6)
            atrTimeFrame = TimeFrame.Minute10;
        else if (atrTimeFrame == TimeFrame.Minute10)
            atrTimeFrame = TimeFrame.Minute15;
        else if (atrTimeFrame == TimeFrame.Minute15)
            atrTimeFrame = TimeFrame.Minute20;
        else if (atrTimeFrame == TimeFrame.Minute20)
            atrTimeFrame = TimeFrame.Minute30;
        else if (atrTimeFrame == TimeFrame.Minute30)
            atrTimeFrame = TimeFrame.Hour;
        else if (atrTimeFrame == TimeFrame.Hour)
            atrTimeFrame = TimeFrame.Hour2;
        else if (atrTimeFrame == TimeFrame.Hour2)
            atrTimeFrame = TimeFrame.Hour3;
        else if (atrTimeFrame == TimeFrame.Hour3)
            atrTimeFrame = TimeFrame.Hour4;
        else if (atrTimeFrame == TimeFrame.Hour4)
            atrTimeFrame = TimeFrame.Hour6;
        else if (atrTimeFrame == TimeFrame.Hour6)
            atrTimeFrame = TimeFrame.Hour8;
        else if (atrTimeFrame == TimeFrame.Hour8)
            atrTimeFrame = TimeFrame.Hour12;
        else if (atrTimeFrame == TimeFrame.Hour12)
            atrTimeFrame = TimeFrame.Daily;
        else if (atrTimeFrame == TimeFrame.Daily)
            atrTimeFrame = TimeFrame.Day2;
        else if (atrTimeFrame == TimeFrame.Day2)
            atrTimeFrame = TimeFrame.Day3;
        else if (atrTimeFrame == TimeFrame.Day3)
            atrTimeFrame = TimeFrame.Weekly;
        else if (atrTimeFrame == TimeFrame.Weekly)
            atrTimeFrame = TimeFrame.Monthly;
        else if (atrTimeFrame == TimeFrame.Monthly)
            atrTimeFrame = TimeFrame.Minute;
        
        AtrTimeFrame.FromTimeFrame(atrTimeFrame);
    }

    // ReSharper disable once CognitiveComplexity
    public string GetTimeFrameShortName()
    {
        var atrTimeFrame = AtrTimeFrame.ToTimeFrame();
        
        if (atrTimeFrame == TimeFrame)
            return "CURRENT";

        if (atrTimeFrame == TimeFrame.Minute)
            return "Minute";
        if (atrTimeFrame == TimeFrame.Minute2)
            return "Minute 2";
        if (atrTimeFrame == TimeFrame.Minute3)
            return "Minute 3";
        if (atrTimeFrame == TimeFrame.Minute4)
            return "Minute 4";
        if (atrTimeFrame == TimeFrame.Minute5)
            return "Minute 5";
        if (atrTimeFrame == TimeFrame.Minute6)
            return "Minute 6";
        if (atrTimeFrame == TimeFrame.Minute10)
            return "Minute 10";
        if (atrTimeFrame == TimeFrame.Minute15)
            return "Minute 15";
        if (atrTimeFrame == TimeFrame.Minute20)
            return "Minute 20";
        if (atrTimeFrame == TimeFrame.Minute30)
            return "Minute 30";
        if (atrTimeFrame == TimeFrame.Hour)
            return "Hour";
        if (atrTimeFrame == TimeFrame.Hour2)
            return "Hour 2";
        if (atrTimeFrame == TimeFrame.Hour3)
            return "Hour 3";
        if (atrTimeFrame == TimeFrame.Hour4)
            return "Hour 4";
        if (atrTimeFrame == TimeFrame.Hour6)
            return "Hour 6";
        if (atrTimeFrame == TimeFrame.Hour8)
            return "Hour 8";
        if (atrTimeFrame == TimeFrame.Hour12)
            return "Hour 12";
        if (atrTimeFrame == TimeFrame.Daily)
            return "Daily";
        if (atrTimeFrame == TimeFrame.Day2)
            return "Day 2";
        if (atrTimeFrame == TimeFrame.Day3)
            return "Day 3";
        if (atrTimeFrame == TimeFrame.Weekly)
            return "Weekly";
        return "Monthly";
    }

    #endregion
}