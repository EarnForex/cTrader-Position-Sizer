using cAlgo.API;
using cAlgo.API.Internals;

namespace PositionSizer.Tools;

public static class VolumeTools
{
    public static double VolumeAtRisk(Symbol symbol, double balance, double riskPct, double sl, RoundingMode roundingMode, bool normalize = true)
    {
        //Print($"Amount to Risk {Account.Balance * (riskPct / 100.0)} with a SL {sl} * PipValue of {sl * symbol.PipValue}");

        return normalize
            ? symbol.NormalizeVolumeInUnits(balance * (riskPct / 100.0) / (sl * symbol.PipValue), roundingMode)
            : balance * (riskPct / 100.0) / (sl * symbol.PipValue);
    }
}