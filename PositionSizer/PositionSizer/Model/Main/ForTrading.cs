namespace cAlgo.Robots;

public partial class Model
{
    #region ForTradingView
    
    public TakeProfits TakeProfits { get; set; }
    public double TrailingStopPips { get; set; }
    public double BreakEvenPips { get; set; }
    public string Label { get; set; }
    public int ExpirationSeconds { get; set; }
    public string Comment { get; set; }
    public bool AutoSuffix { get; set; }
    public int MaxNumberOfTradesTotal { get; set; }
    public int MaxNumberOfTradesPerSymbol { get; set; }
    public double MaxLotsTotal { get; set; }
    public double MaxLotsPerSymbol { get; set; }
    public double MaxRiskPctTotal { get; set; }
    public double MaxRiskPctPerSymbol { get; set; }
    public bool DisableTradingWhenLinesAreHidden { get; set; }
    public double MaxSlippagePips { get; set; }
    public double MaxSpreadPips { get; set; }
    public double MaxEntryStopLossDistancePips { get; set; }
    public double MinEntryStopLossDistancePips { get; set; }
    public double MaxRiskPercentage { get; set; }
    public bool SubtractOpenPositionsVolume { get; set; }
    public bool SubtractPendingOrdersVolume { get; set; }
    public bool DoNotApplyStopLoss { get; set; }
    public bool DoNotApplyTakeProfit { get; set; }
    public bool AskForConfirmation { get; set; }
    
    #endregion
}