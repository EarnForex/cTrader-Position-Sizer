using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public partial class Model
{
    /// <summary>
    /// Round-trip commission for the current <see cref="TradeSize"/> in account currency.
    /// </summary>
    public double CommissionFromVolume()
    {
        if (Symbol.Commission == 0)
            return 0;

        // One-way per-lot commission × lots × 2 (entry + exit).
        // StandardCommission() already returns the account-currency value for every commission type
        // (including the base→account conversion for UsdPerMillionUsdVolume), so routing all types
        // through it keeps CommissionFromVolume consistent with the displayed commission and with
        // CommissionPerUnitVolume(). The previous UsdPerMillionUsdVolume special-case used the raw
        // base-currency volume without converting, over-counting commission by 1/(base→account rate).
        return 2 * StandardCommission() * TradeSize.Lots;
    }

    /// <summary>
    /// One-way commission per 1 lot in account currency (MQL5 <c>CalculateCommission</c> equivalent).
    /// </summary>
    public double StandardCommission()
    {
        if (Symbol.Commission == 0)
            return 0;

        var accountAsset = Account.Asset;

        if (Symbol.CommissionType == SymbolCommissionType.UsdPerMillionUsdVolume)
        {
            var lotSize = Symbol.LotSize == 0 ? InputFallbackLotSize : Symbol.LotSize;
            var lotSizeOfBaseAssetInAccountCurrency = Symbol.BaseAsset.Convert(accountAsset, lotSize);
            return Symbol.Commission / 1_000_000.0 * lotSizeOfBaseAssetInAccountCurrency;
        }

        if (Symbol.CommissionType == SymbolCommissionType.UsdPerOneLot)
            return Assets.GetAsset("USD").Convert(accountAsset, Symbol.Commission);

        if (Symbol.CommissionType == SymbolCommissionType.QuoteCurrencyPerOneLot)
            return Symbol.QuoteAsset.Convert(accountAsset, Symbol.Commission);

        if (Symbol.CommissionType == SymbolCommissionType.PercentageOfTradingVolume)
            return OneLotTradingVolumeInAccountCurrency() * CommissionPercentageRate();

        throw new Exception("Unknown commission type");
    }

    /// <summary>
    /// cTrader stores percentage-of-volume rates scaled by 10^5 (e.g. 17500 → 0.175%).
    /// Values below 1 are treated as already being in percent points (e.g. 0.175 → 0.175%).
    /// </summary>
    private double CommissionPercentageRate()
    {
        var percentPoints = Symbol.Commission >= 1 ? Symbol.Commission / 100_000.0 : Symbol.Commission;
        return percentPoints / 100.0;
    }

    private double CommissionReferencePrice()
    {
        if (EntryPrice > 0)
            return EntryPrice;

        return TradeType == TradeType.Buy ? Symbol.Ask : Symbol.Bid;
    }

    /// <summary>
    /// Notional trading volume of 1 lot in account currency (MQL5 contract value logic).
    /// </summary>
    private double OneLotTradingVolumeInAccountCurrency()
    {
        var lotSize = Symbol.LotSize == 0 ? InputFallbackLotSize : Symbol.LotSize;
        var accountAsset = Account.Asset;

        if (Symbol.BaseAsset == accountAsset)
            return lotSize;

        if (Symbol.QuoteAsset == accountAsset)
            return lotSize * CommissionReferencePrice();

        return Symbol.BaseAsset.Convert(accountAsset, lotSize);
    }
}
