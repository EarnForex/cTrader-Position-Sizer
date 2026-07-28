using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public partial class Model
{
    #region ForMarginView

    public double PositionMargin { get; set; }
    public double FutureUsedMargin { get; set; }
    public double FutureFreeMargin { get; set; }
    public double MaxPositionSizeByMargin { get; set; }
    
    public double CustomLeverage { get; set; }

    public MarginUtilizationBase MarginUtilizationBase { get; set; }
    public double MubStartingBalance { get; set; }
    public double MarginUtilizedCurrent { get; set; }
    public double MarginUtilizedPosition { get; set; }
    public double MarginUtilizedFuture { get; set; }
    public double MarginUtilizedCurrentSymbol { get; set; }
    public double MarginUtilizationBaseValue { get; set; }

    #endregion
    
    public void UpdateMarginValues(IAssetConverter assetConverter, RoundingMode roundingMode)
    {
        var multiplier = CustomLeverage == 0 ? 1 : Account.PreciseLeverage / CustomLeverage;

        PositionMargin = Symbol.GetEstimatedMargin(TradeType, TradeSize.Volume) * multiplier;
        FutureUsedMargin = Symbol.GetEstimatedMargin(TradeType, TradeSize.Volume) * multiplier + Account.Margin;
        FutureFreeMargin = Account.FreeMargin - Symbol.GetEstimatedMargin(TradeType, TradeSize.Volume) * multiplier;

        var convert = assetConverter.Convert(Account.FreeMargin, Account.Asset, Symbol.BaseAsset);

        MaxPositionSizeByMargin = Symbol.VolumeInUnitsToQuantity(convert * Account.PreciseLeverage * multiplier);
        
        TradeSize.IsLotsValueInvalid = TradeSize.Lots > MaxPositionSizeByMargin;

        UpdateMarginUtilization();
    }

    public void UpdateMarginUtilization()
    {
        var muBase = MarginUtilizationBase switch
        {
            MarginUtilizationBase.StartingBalance => MubStartingBalance,
            MarginUtilizationBase.FreeMargin => Account.FreeMargin,
            _ => Account.Balance
        };

        MarginUtilizationBaseValue = muBase;

        if (muBase == 0)
        {
            MarginUtilizedCurrent = 0;
            MarginUtilizedPosition = 0;
            MarginUtilizedFuture = 0;
            MarginUtilizedCurrentSymbol = 0;
            return;
        }

        var currentSymbolMargin = Positions.Where(position => position.SymbolName == Symbol.Name).Sum(position => position.Margin);

        // Difference vs MT5: in the MT5 Position Sizer the prospective order's margin is always
        // counted in utilization, even for pending/stop-limit orders. That is an upstream bug:
        // pending orders reserve no margin until they trigger, so they have no immediate effect
        // on the account. Here only Instant orders contribute to "position" margin utilization;
        // pending/stop-limit orders contribute 0 until they become market positions.
        var prospectiveMarginUtilized = OrderType == OrderType.Instant
            ? PositionMargin
            : 0;

        MarginUtilizedCurrent = Account.Margin / muBase * 100;
        MarginUtilizedPosition = prospectiveMarginUtilized / muBase * 100;
        MarginUtilizedFuture = MarginUtilizedCurrent + MarginUtilizedPosition;
        MarginUtilizedCurrentSymbol = currentSymbolMargin / muBase * 100;
    }
}
