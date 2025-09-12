using System.ComponentModel;

namespace cAlgo.Robots;

public enum IncludeOrdersMode
{
    [Description("All")]
    All,
    [Description("Pending Only")]
    PendingOnly,
    [Description("Pos. Only")]
    PositionsOnly
}