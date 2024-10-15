using System;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public interface IModelResources
{
    bool InputCalculateUnadjustedPositionSize { get; }
    bool InputSurpassBrokerMaxPositionSizeWithMultipleTrades { get; }
    //--
    Symbol Symbol { get; }
    Positions Positions { get; }
    TimeFrame TimeFrame { get; }
    PendingOrders PendingOrders { get; }
    IAccount Account { get; }
    void Print(object obj);
}

[Log]
public partial class Model : IModel
{
    #region Resources And Constructor
    
    private IModelResources _resources;

    private bool InputCalculateUnadjustedPositionSize => _resources.InputCalculateUnadjustedPositionSize;
    private bool InputSurpassBrokerMaxPositionSizeWithMultipleTrades => _resources.InputSurpassBrokerMaxPositionSizeWithMultipleTrades;
    private IAccount Account => _resources.Account;
    private Positions Positions => _resources.Positions;
    private TimeFrame TimeFrame => _resources.TimeFrame;
    private PendingOrders PendingOrders => _resources.PendingOrders;
    private Symbol Symbol => _resources.Symbol;
    
    public Model(IModelResources resources)
    {
        _resources = resources;
    }

    public void SetResources(IModelResources resources)
    {
        _resources = resources;
    }

    #endregion
    
    public string Version { get; set; }
    
    public LastKnownState LastKnownState { get; set; }
    public event EventHandler<EntryPriceUpdatedEventArgs> EntryPriceUpdated;

    #region ForMainView

    /// <summary>
    /// Used for when the settings are restored
    /// It is needed to compare this value to the previous one to know if the symbol has changed
    /// If the value is the same, the entry price, stop limit price, stop loss, take profit, trade size and account size will be restored
    /// if the value is different, new values from entry price, stop limit price, stop loss, take profit, trade size and account size will be used
    /// </summary>
    public string SymbolName { get; set; }
    //independents (Don't need to update if any of the other properties change)
    public TradeType TradeType { get; set; }
    public double EntryPrice { get; set; }
    /// <summary>
    /// For StopLimit Orders
    /// </summary>
    public double StopLimitPrice { get; set; }
    public OrderType OrderType { get; set; }
    //--
    //dependents (Need to update if any of the other properties change)
    public StopLoss StopLoss { get; set; }
    public TradeSize TradeSize { get; set; }
    public bool HideLines { get; set; }
    
    public AccountSize AccountSize { get; set; }

    // public void ChangeTradeType()
    // {
    //     TradeType = TradeType == TradeType.Buy ? TradeType.Sell : TradeType.Buy;
    //     
    //     //Since TradeType Changed, we need to update StopLoss and TakeProfit
    //     (StopLoss.Price, TakeProfit.Price) = (TakeProfit.Price, StopLoss.Price);
    //     //The SL.Pips and Tp.Pips also need to be updated
    //     StopLoss.Pips = Math.Round(Math.Abs(EntryPrice - StopLoss.Price) / Symbol.PipSize, 1);
    //     TakeProfit.Pips = Math.Round(Math.Abs(EntryPrice - TakeProfit.Price) / Symbol.PipSize, 1);
    //     
    //     //it kinda seems that the reward also needs to be updated, but it's just a rounding issue because of Bid/Ask
    //     ModelUpdated?.Invoke(this, EventArgs.Empty);
    // }

    // public void UpdatePositionSize(double newLots)
    // {
    //     TradeSize.Lots = newLots;
    //     
    //     //The following must also be updated
    //     TradeSize.RiskInCurrency = Symbol.AmountRisked(TradeSize.Volume, StopLoss.Pips);
    //     TradeSize.RiskPercentage = Math.Round((TradeSize.RiskInCurrency / Account.Balance) * 100.0, 2);
    //     TradeSize.RewardRiskRatio = Math.Round(TakeProfit.Pips / StopLoss.Pips, 2);
    //     TradeSize.RewardInCurrency = TradeSize.RiskInCurrency * TradeSize.RewardRiskRatio;
    //     
    //     ModelUpdated?.Invoke(this, EventArgs.Empty);
    // }

    // public void UpdateRiskPercentage(double newRiskPercentage)
    // {
    //     TradeSize.RiskPercentage = newRiskPercentage;
    //     
    //     //The following must also be updated
    //     var amountRisked = AccountSize.Value * (TradeSize.RiskPercentage / 100.0);
    //     TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeForFixedRisk(amountRisked, StopLoss.Pips));
    //     TradeSize.RiskInCurrency = amountRisked;
    //     TradeSize.RewardInCurrency = TradeSize.RiskInCurrency * TradeSize.RewardRiskRatio;
    //     
    //     ModelUpdated?.Invoke(this, EventArgs.Empty);
    // }
    
    // public void UpdateRiskCurrency(double newRiskCurrency)
    // {
    //     TradeSize.RiskInCurrency = newRiskCurrency;
    //     
    //     //The following must also be updated
    //     TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeForFixedRisk(TradeSize.RiskInCurrency, StopLoss.Pips));
    //     TradeSize.RiskPercentage = Math.Round((TradeSize.RiskInCurrency / Account.Balance) * 100.0, 2);
    //     TradeSize.RewardInCurrency = TradeSize.RiskInCurrency * TradeSize.RewardRiskRatio;
    // }
    
    // public void UpdateEntryPrice(double newEntryPrice)
    // {
    //     EntryPrice = newEntryPrice;
    //     
    //     //The following must also be updated
    //     StopLoss.Pips = Math.Round(Math.Abs(EntryPrice - StopLoss.Price) / Symbol.PipSize, 1);
    //     TakeProfit.Pips = Math.Round(Math.Abs(EntryPrice - TakeProfit.Price) / Symbol.PipSize, 1);
    //     TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeForFixedRisk(TradeSize.RiskInCurrency, StopLoss.Pips));
    //     TradeSize.RewardRiskRatio = TakeProfit.Pips / StopLoss.Pips;
    //     TradeSize.RewardInCurrency = TradeSize.RiskInCurrency * TradeSize.RewardRiskRatio;
    //     
    //     ModelUpdated?.Invoke(this, EventArgs.Empty);
    // }
    
    // public void UpdateTakeProfitPrice(double newTakeProfitPrice)
    // {
    //     TakeProfit.Price = newTakeProfitPrice;
    //     
    //     //The following must also be updated
    //     TakeProfit.Pips = Math.Round(Math.Abs(EntryPrice - TakeProfit.Price) / Symbol.PipSize, 1);
    //     TradeSize.RewardRiskRatio = Math.Round(TakeProfit.Pips / StopLoss.Pips, 2);
    //     TradeSize.RewardInCurrency = TradeSize.RiskInCurrency * TradeSize.RewardRiskRatio;
    //     
    //     ModelUpdated?.Invoke(this, EventArgs.Empty);
    // }
    
    // public void UpdateStopLossPrice(double newStopLossPrice)
    // {
    //     StopLoss.Price = newStopLossPrice;
    //     
    //     //The following must also be updated
    //     StopLoss.Pips = Math.Round(Math.Abs(EntryPrice - StopLoss.Price) / Symbol.PipSize, 1);
    //     TradeSize.RewardRiskRatio = Math.Round(TakeProfit.Pips / StopLoss.Pips, 2);
    //     TradeSize.RewardInCurrency = TradeSize.RiskInCurrency * TradeSize.RewardRiskRatio;
    //     TradeSize.Lots = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeForFixedRisk(TradeSize.RiskInCurrency, StopLoss.Pips));
    //     
    //     ModelUpdated?.Invoke(this, EventArgs.Empty);
    // }
    
    public void UpdateAccountSizeMode(AccountSizeMode newMode, double currentPortfolioRisk, RoundingMode roundingMode)
    {
        AccountSize.Mode = newMode;

        AccountSize.Value = AccountSize.Mode switch
        {
            AccountSizeMode.Equity => Account.Equity,
            AccountSizeMode.Balance => Account.Balance,
            AccountSizeMode.BalanceCpr => Account.Balance - currentPortfolioRisk,
            _ => throw new ArgumentOutOfRangeException()
        };

        UpdateWithRiskInCurrency(AccountSize.Value * (TradeSize.RiskPercentage / 100.0), roundingMode);
    }
    
    public void UpdateAccountSizeValue(double newValue, RoundingMode roundingMode)
    {
        //If it's equity mode, it can only change automatically
        //If it's Balance CPR, it can only change automatically
        
        if (AccountSize.Mode != AccountSizeMode.Balance)
            return;
        
        AccountSize.Value = newValue;

        switch (TradeSize.LastRiskValueChanged)
        {
            case LastRiskValueChanged.RiskPercentage:
                UpdateWithRiskPercentage(TradeSize.RiskPercentage, roundingMode);
                break;
            case LastRiskValueChanged.RiskCurrency:
                UpdateWithRiskInCurrency(TradeSize.RiskInCurrency, roundingMode);
                break;
            case LastRiskValueChanged.LotSize:
                UpdateWithTradeSizeLots(TradeSize.Lots);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateAccountSizeValue(RoundingMode roundingMode)
    {
        if (AccountSize.IsCustomBalance)
            return;
        
        AccountSize.Value = AccountSize.Mode switch
        {
            AccountSizeMode.Equity => Account.Equity,
            AccountSizeMode.Balance => Account.Balance,
            AccountSizeMode.BalanceCpr => Account.Balance - CurrentPortfolio.RiskCurrency,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        switch (TradeSize.LastRiskValueChanged)
        {
            case LastRiskValueChanged.RiskPercentage:
                UpdateWithRiskPercentage(TradeSize.RiskPercentage, roundingMode);
                break;
            case LastRiskValueChanged.RiskCurrency:
                UpdateWithRiskInCurrency(TradeSize.RiskInCurrency, roundingMode);
                break;
            case LastRiskValueChanged.LotSize:
                UpdateWithTradeSizeLots(TradeSize.Lots);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public double CommissionFromVolume()
    {
        //2 because it accounts for both the entry and exit
        //1_000_000 because the commission is per million
        return 2 * TradeSize.Volume * Symbol.Commission / 1_000_000;
    }

    public double StandardCommission()
    {
        return Symbol.CommissionType switch
        {
            SymbolCommissionType.UsdPerMillionUsdVolume => Symbol.Commission / 1_000_000,
            SymbolCommissionType.UsdPerOneLot => Symbol.Commission,
            SymbolCommissionType.PercentageOfTradingVolume => TradeSize.Volume * Symbol.Commission / 100,
            SymbolCommissionType.QuoteCurrencyPerOneLot => Symbol.Commission * Symbol.PipValue,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    //Use stringBuilder, a new line for every property that has a getter and setter
    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"TradeType: {TradeType}");
        sb.AppendLine($"EntryPrice: {EntryPrice}");
        sb.AppendLine($"StopPrice: {StopLimitPrice}");
        sb.AppendLine($"OrderType: {OrderType}");
        sb.AppendLine($"StopLoss: {StopLoss}");
        //Take Profits don't need to be prefixed
        sb.AppendLine($"{TakeProfits}");
        sb.AppendLine($"TradeSize: {TradeSize}");
        sb.AppendLine($"HideLines: {HideLines}");
        sb.AppendLine($"AccountSize: {AccountSize}");
        
        return sb.ToString();
    }

    #endregion

    #region FromAppStateModel

    public string Trade(Symbol symbol)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Order: {TradeType} {OrderType}");
            
        if (TakeProfits.List.Any(x => x.Pips != 0))
        {
            //first lot that is seen is the one which has TP != 0, so it's the tradezie x its distribution %
            var firstTakeProfit = TakeProfits.List.First(x => x.Pips != 0);
            var lotSize = firstTakeProfit.Distribution * TradeSize.Lots / 100.0;
            var numberOfTakeProfits = TakeProfits.List.Count(x => x.Pips != 0);

            if (numberOfTakeProfits == 1)
                sb.AppendLine($"Size: {lotSize:F2} lots");
            else
                sb.AppendLine($"Size: {lotSize:F2} lots (multiple)");
        }
        
        sb.AppendLine($"Account Balance: {AccountSize.Value:F2} {Account.Asset.Name}")
            .AppendLine($"Risk: {TradeSize.RiskInCurrency:F2} {Account.Asset.Name}")
            .AppendLine($"Margin: {PositionMargin:F2} {Account.Asset.Name}")
            .AppendLine($"Entry: {EntryPrice.ToString($"0.{new string('0', symbol.Digits)}")}");

        if (!StopLoss.Blocked)
            sb.AppendLine($"Stop Loss: {StopLoss.Price.ToString($"0.{new string('0', symbol.Digits)}")}");

        if (TakeProfits.List.Any(x => x.Pips != 0))
        {
            var firstTakeProfit = TakeProfits.List.First(x => x.Pips != 0);
            var numberOfTakeProfits = TakeProfits.List.Count(x => x.Pips != 0);

            if (numberOfTakeProfits == 1)
                sb.AppendLine($"Take Profit: {firstTakeProfit.Price.ToString($"0.{new string('0', symbol.Digits)}")}");
            else
                sb.AppendLine($"Take Profits: {firstTakeProfit.Price.ToString($"0.{new string('0', symbol.Digits)}")} (multiple)");
        }
                
        // if (!MainModel.TakeProfit.Blocked)
        //     sb.AppendLine($"Take Profit: {MainModel.TakeProfit.Price.ToString($"0.{new string('0', symbol.Digits)}")}");

        return sb.ToString();
    }

    #endregion

    public void UpdateEntryPrice(double newPrice, EntryPriceUpdateReason reason)
    {
        if (newPrice.Is(EntryPrice, Symbol.TickSize))
            return;
        
        EntryPrice = newPrice;
        EntryPriceUpdated?.Invoke(this, new EntryPriceUpdatedEventArgs(EntryPrice, reason));
    }
}