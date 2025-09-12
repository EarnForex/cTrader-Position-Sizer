using System.ComponentModel;

namespace cAlgo.Robots;

public enum IncludeSymbolsMode
{
    [Description("All")]
    All,
    [Description("Current Only")]
    CurrentOnly,
    [Description("Others Only")]
    OthersOnly
}