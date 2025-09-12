using System.ComponentModel;

namespace cAlgo.Robots;

public enum IncludeDirectionsMode
{
    [Description("All Directions")]
    AllDirections,
    [Description("Long Only")]
    LongOnly,
    [Description("Short Only")]
    ShortOnly
}