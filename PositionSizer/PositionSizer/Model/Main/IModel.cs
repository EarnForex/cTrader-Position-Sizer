using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public interface IModel
{
    public string Version { get; set; }
    void SetResources(IModelResources resources);
    public void UpdateMarginValues(IAssetConverter assetConverter, RoundingMode roundingMode);
    public LastKnownState LastKnownState { get; set; }
    
    public event EventHandler<EntryPriceUpdatedEventArgs> EntryPriceUpdated;

    /// <summary>
    /// Used for when the settings are restored
    /// It is needed to compare this value to the previous one to know if the symbol has changed
    /// If the value is the same, the entry price, stop limit price, stop loss, take profit, trade size and account size will be restored
    /// if the value is different, new values from entry price, stop limit price, stop loss, take profit, trade size and account size will be used
    /// </summary>
    string SymbolName { get; set; }

    TradeType TradeType { get; set; }
    double EntryPrice { get; set; }

    /// <summary>
    /// For StopLimit Orders
    /// </summary>
    double StopLimitPrice { get; set; }

    OrderType OrderType { get; set; }
    StopLoss StopLoss { get; set; }
    TradeSize TradeSize { get; set; }
    bool HideLines { get; set; }
    AccountSize AccountSize { get; set; }
    bool IsAtrModeActive { get; set; }
    int Period { get; set; }
    double StopLossMultiplier { get; set; }
    double TakeProfitMultiplier { get; set; }
    bool StopLossSpreadAdjusted { get; set; }
    bool TakeProfitSpreadAdjusted { get; set; }
    SerializableTimeFrame AtrTimeFrame { get; set; }
    AtrCandle AtrCandle { get; set; }
    TakeProfits TakeProfits { get; set; }
    double TrailingStopPips { get; set; }
    double BreakEvenPips { get; set; }
    string Label { get; set; }
    int ExpirationSeconds { get; set; }
    string Comment { get; set; }
    bool AutoSuffix { get; set; }
    int MaxNumberOfTradesTotal { get; set; }
    int MaxNumberOfTradesPerSymbol { get; set; }
    double MaxLotsTotal { get; set; }
    double MaxLotsPerSymbol { get; set; }
    double MaxRiskPctTotal { get; set; }
    double MaxRiskPctPerSymbol { get; set; }
    bool DisableTradingWhenLinesAreHidden { get; set; }
    double MaxSlippagePips { get; set; }
    double MaxSpreadPips { get; set; }
    double MaxEntryStopLossDistancePips { get; set; }
    double MinEntryStopLossDistancePips { get; set; }
    double MaxRiskPercentage { get; set; }
    bool SubtractOpenPositionsVolume { get; set; }
    bool SubtractPendingOrdersVolume { get; set; }
    bool DoNotApplyStopLoss { get; set; }
    bool DoNotApplyTakeProfit { get; set; }
    bool AskForConfirmation { get; set; }
    double PositionMargin { get; set; }
    double FutureUsedMargin { get; set; }
    double FutureFreeMargin { get; set; }
    double MaxPositionSizeByMargin { get; set; }
    double CustomLeverage { get; set; }
    IncludeOrdersMode IncludeOrdersMode { get; set; }
    IncludeSymbolsMode IncludeSymbolsMode { get; set; }
    IncludeDirectionsMode IncludeDirectionsMode { get; set; }
    bool IgnoreOrdersWithoutStopLoss { get; set; }
    bool IgnoreOrdersWithoutTakeProfit { get; set; }
    Portfolio CurrentPortfolio { get; set; }
    Portfolio PotentialPortfolio { get; set; }
    void UpdateAccountSizeMode(AccountSizeMode newMode, double currentPortfolioRisk, RoundingMode roundingMode);
    void UpdateAccountSizeValue(double newValue, RoundingMode roundingMode);
    void UpdateAccountSizeValue(RoundingMode roundingMode);
    double CommissionFromVolume();
    double StandardCommission();
    string Trade(Symbol symbol);
    void UpdateWithTradeSizeLots(double lots);
    void UpdateWithRiskInCurrency(double moneyRisk, RoundingMode roundingMode);
    void UpdateWithRiskPercentage(double riskPercentage, RoundingMode roundingMode);
    void SetRiskDefaults(RoundingMode roundingMode);
    public void SetAtrIndicator(AverageTrueRange averageTrueRange);
    double GetAtrPips();
    double GetAtr();

    /// <summary>
    /// This method selects the next timeframe in order
    /// The timeframe list must be picked and ordered in a loop
    /// if the current timeframe is Minute, the next timeframe will be Minute2
    /// if the current timeframe is Minute30, the next timeframe will be Hour
    /// and so on, until the last timeframe is reached then it will start over
    /// </summary>
    // ReSharper disable once CognitiveComplexity
    void GetNextAtr();

    string GetTimeFrameShortName();
    void UpdateStopLossFromTradeTypeChange();
    void UpdateStopLossFromEntryPriceChange();
    void UpdateStopLossSpreadAdjustment();
    void TryAddStopLossSpreadAdjustment(bool stopLossSpreadAdjusted);
    void UpdateStopLossFromEntryLineMoved();
    void UpdateStopLossFromAtr();
    void ChangeStopLossPrice(double price);
    void ChangeStopLossPips(double pips);
    void UpdateStopLossPriceFromPips();
    void UpdateTakeProfitsFromEntryPriceChanged();
    void UpdateTakeProfitsFromTradeTypeChange();
    void UpdateTakeProfitsFromSpreadAdjustment();
    void UpdateTakeProfitPipsLockedOnStopLoss();
    void UpdateTakeProfitPipsLockedOnStopLoss(int id);
    void ChangeTakeProfitPips(int id, double pips);
    void UpdateTakeProfitFromAtr();
    void UpdateTakeProfitPrice(int id, double price);
    void AddNewTakeProfit(bool prefillAdditionalTpsBasedOnMain);
    void UpdateTakeProfitPriceFromPips();
    void NormalizeTakeProfitPips();
    void UpdateReadOnlyValues();
    double GetUpdatedRiskLots();
    double GetUpdatedRiskCurrency();
    public double GetCustomRiskCurrency(Position[] positions, PendingOrder[] pendingOrders);
    public double GetCustomRiskPercentage(Position[] positions, PendingOrder[] pendingOrders);
    double GetUpdatedRewardCurrency();
    void UpdateEntryPrice(double price, EntryPriceUpdateReason reason);
    public void UpdateTradeSizeValues(RoundingMode roundingMode);
    public bool IsAnyTakeProfitInvalid();
}