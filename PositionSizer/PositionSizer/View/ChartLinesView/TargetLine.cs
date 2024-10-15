using cAlgo.API;

namespace cAlgo.Robots;

public class TargetLine
{
    public static string TargetLineTag = "TargetLine";
    public static string TargetTextTag = "TargetText";
    public static string TargetAdditionalTextTag = "TargetAdditionalText";
    
    public ChartHorizontalLine TargetLineObj { get; set; }
    public double LastKnownYPrice { get; set; }
    public ChartText TargetTextObj { get; set; }
    public ChartText TargetAdditionalTextObj { get; set; }
}