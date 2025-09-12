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
    }
}