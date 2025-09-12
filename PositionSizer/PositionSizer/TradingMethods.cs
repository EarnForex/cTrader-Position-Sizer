using System;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public partial class PositionSizer 
{
    private void TrySendTrade()
    {
        if (!Model.TakeProfits.DistributionAddsUp)
        {
            var msg = "Cannot place trade: take profit distribution does not add up to 100%.";
            Print(msg);
            MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (Model.AskForConfirmation)
        {
            var question = new StringBuilder();
            //--
            question.AppendLine("Do you want to send this trade?");
            question.AppendLine($"{Model.Trade(Symbol)}");

            var result = MessageBox.Show(
                question.ToString(),
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;
        }

        if (Model.DisableTradingWhenLinesAreHidden && Model.HideLines)
        {
            Print("Trading is disabled because chart lines are hidden.");
            return;
        }

        if (Model.MaxSpreadPips != 0 && Model.OrderType == OrderType.Instant)
        {
            if (Symbol.Spread / Symbol.PipSize >= Model.MaxSpreadPips)
            {
                Print($"Cannot place trade: spread is too high ({(Symbol.Spread / Symbol.PipSize):F2} pips).");
                return;
            }

            Print($"Spread is acceptable: {(Symbol.Spread / Symbol.PipSize):F2} pips.");
        }

        if (Model.MaxRiskPercentage > 0)
        {
            var sl = Model.DoNotApplyStopLoss
                ? null
                : (double?)Model.StopLoss.Pips;
            
            var potentialRisk = sl.HasValue 
                ? Symbol.AmountRisked(Model.TradeSize.Volume, sl.Value) 
                : Symbol.AmountRisked(Model.TradeSize.Volume, Symbol.Bid / Symbol.PipSize);
            var potentialRiskPct = potentialRisk / Model.AccountSize.Value * 100;
                
            if (potentialRiskPct > Model.MaxRiskPercentage)
            {
                var msg = $"Cannot place trade: potential risk ({potentialRiskPct:F2}%) exceeds the maximum allowed risk percentage ({Model.MaxRiskPercentage:F2}%).";
                Print(msg);
                MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        for (var index = 0; index < Model.TakeProfits.List.Count; index++)
        {
            if (Model.TakeProfits.List[index].Distribution == 0)
            {
                Print($"Cannot place trade: distribution is 0% for take profit {index + 1}.");
                continue;
            }

            var vol = Model.TradeSize.Volume * Model.TakeProfits.List[index].Distribution / 100.0;
            var quantity = Symbol.VolumeInUnitsToQuantity(vol);
            var expiry = Model.OrderType != OrderType.Instant &&
                         Model.ExpirationSeconds != 0
                ? Server.Time.AddSeconds(Model.ExpirationSeconds)
                : (DateTime?)null;
            
            var comment = Model.AutoSuffix ? Model.Comment + InstanceId : Model.Comment;
            
            Print($"Volume to trade: {vol:F2} | Quantity: {quantity:F2}");
                
            var takeProfit = Model.TakeProfits.List[index];
            var tp = Model.DoNotApplyTakeProfit
                ? null
                : (double?)takeProfit.Pips;

            var sl = Model.DoNotApplyStopLoss
                ? null
                : (double?)Model.StopLoss.Pips;

            var slPips = Model.StopLoss.Pips;

            if (sl != null && Model.MaxEntryStopLossDistancePips != 0 && slPips > Model.MaxEntryStopLossDistancePips)
            {
                var msg = $"Cannot place trade: entry to stop loss distance ({slPips}) exceeds the maximum allowed ({Model.MaxEntryStopLossDistancePips}).";
                Print(msg);
                MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (sl != null && Model.MinEntryStopLossDistancePips != 0 && slPips < Model.MinEntryStopLossDistancePips)
            {
                var msg = $"Cannot place trade: entry to stop loss distance ({slPips}) is less than the minimum allowed ({Model.MinEntryStopLossDistancePips}).";
                Print(msg);
                MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (Model.MaxNumberOfTradesTotal != 0)
            {
                var totalTrades = PositionsByLabel.Count();
        
                if (totalTrades >= Model.MaxNumberOfTradesTotal)
                {
                    var msg = $"Cannot place trade: maximum number of trades reached ({totalTrades}).";
                    Print(msg);
                    MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            if (Model.MaxNumberOfTradesPerSymbol != 0)
            {
                var totalTradesPerCurrentSymbol = PositionsByLabel.Count(x => x.SymbolName == SymbolName);
        
                if (totalTradesPerCurrentSymbol >= Model.MaxNumberOfTradesPerSymbol)
                {
                    var msg = $"Cannot place trade: maximum number of trades per symbol reached ({totalTradesPerCurrentSymbol}).";
                    Print(msg);
                    MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            if (Model.MaxLotsTotal != 0)
            {
                var existingLots = PositionsByLabel.Sum(x => x.Quantity);
                var totalLots = existingLots + quantity;

                if (existingLots >= Model.MaxLotsTotal)
                {
                    var msg = $"Cannot place trade: existing lots ({existingLots:F2}) are greater than or equal to the maximum allowed total lots ({Model.MaxLotsTotal:F2}).";
                    Print(msg);
                    MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
        
                if (totalLots > Model.MaxLotsTotal)
                {
                    if (InputAllowSmallerTradesWhenTradingLimitsAreExceeded)
                    {
                        quantity = Model.MaxLotsTotal - existingLots;
                        vol = Symbol.QuantityToVolumeInUnits(quantity);

                        if (vol < Symbol.VolumeInUnitsMin)
                        {
                            var msg =
                                $"Cannot place trade: adjusted trade volume ({vol:F2}) to fit within the maximum lots limit, but it is below the broker's minimum allowed ({Symbol.VolumeInUnitsMin}). Trade not executed.";
                            MessageBox.Show(messageBoxText: msg,
                                caption: "Error",
                                button: MessageBoxButton.OK);
                            return;
                        }
                        
                        var msgBoxResult = MessageBox.Show(messageBoxText: $"Take a smaller trade? Total lots ({totalLots:F2}) exceed the maximum allowed ({Model.MaxLotsTotal}). New position quantity: {quantity:F2}, volume: {vol:F2}.",
                            caption: "Confirmation",
                            button: MessageBoxButton.OKCancel);

                        if (msgBoxResult != MessageBoxResult.OK)
                            return;
                    }
                    else
                    {
                        Print($"Cannot place trade: total lots after this trade ({totalLots:F2}) would exceed the maximum allowed ({Model.MaxLotsTotal}).");
                        MessageBox.Show($"Cannot place trade: total lots after this trade ({totalLots:F2}) would exceed the maximum allowed ({Model.MaxLotsTotal}).", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;   
                    }
                }   
            }

            if (Model.MaxLotsPerSymbol != 0)
            {
                var existingLotsPerCurrentSymbol = PositionsByLabel.Where(x => x.SymbolName == SymbolName).Sum(x => x.Quantity);
                var totalLotsPerCurrentSymbol = existingLotsPerCurrentSymbol + quantity;
                
                if (existingLotsPerCurrentSymbol >= Model.MaxLotsPerSymbol)
                {
                    var msg = $"Cannot place trade: existing lots for this symbol ({existingLotsPerCurrentSymbol:F2}) are greater than or equal to the maximum allowed per symbol ({Model.MaxLotsPerSymbol}).";
                    Print(msg);
                    MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
        
                if (totalLotsPerCurrentSymbol > Model.MaxLotsPerSymbol)
                {
                    if (InputAllowSmallerTradesWhenTradingLimitsAreExceeded)
                    {
                        quantity = Model.MaxLotsPerSymbol - existingLotsPerCurrentSymbol;
                        vol = Symbol.QuantityToVolumeInUnits(quantity);
                        
                        if (vol < Symbol.VolumeInUnitsMin)
                        {
                            var msg = $"Cannot place trade: adjusted trade volume ({vol:F2}) to fit within the maximum lots per symbol, but it is below the broker's minimum allowed ({Symbol.VolumeInUnitsMin}). Trade not executed.";
                            MessageBox.Show(messageBoxText: msg,
                                caption: "Error",
                                button: MessageBoxButton.OK);
                            return;
                        }

                        var msgBoxResult = MessageBox.Show(
                            messageBoxText: $"Take a smaller trade? Total lots for this symbol ({totalLotsPerCurrentSymbol:F2}) exceed the maximum allowed ({Model.MaxLotsPerSymbol}). New position quantity: {quantity:F2}, volume: {vol:F2}.",
                            caption: "Confirmation",
                            button: MessageBoxButton.OKCancel);

                        if (msgBoxResult != MessageBoxResult.OK)
                            return;
                    }
                    else
                    {
                        var msg = $"Cannot place trade: total lots for this symbol after this trade ({totalLotsPerCurrentSymbol:F2}) would exceed the maximum allowed ({Model.MaxLotsPerSymbol}).";
                        Print(msg);
                        MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;   
                    }
                }   
            }

            double potentialRiskInCurrency;

            if (Model.StopLoss.Pips == 0)
            {
                potentialRiskInCurrency = Model.TradeType == TradeType.Buy 
                    ? Symbol.AmountRisked(Model.TradeSize.Volume, Symbol.Ask / Symbol.PipSize) 
                    : double.PositiveInfinity;
            }
            else
            {
                potentialRiskInCurrency = Symbol.AmountRisked(Model.TradeSize.Volume, Model.StopLoss.Pips);
            }
            
            var potentialRiskPct = potentialRiskInCurrency / Model.AccountSize.Value * 100;

            if (Model.MaxRiskPctTotal != 0)
            {
                var existingRiskPct = Model.GetCustomRiskPercentage(
                    positions: PositionsByLabel.ToArray(),
                    pendingOrders: PendingOrdersByLabel.ToArray());
                var totalRiskPct = existingRiskPct + potentialRiskPct;
                
                if (existingRiskPct >= Model.MaxRiskPctTotal)
                {
                    var msg = $"Cannot place trade: existing total risk ({existingRiskPct:F2}%) is greater than or equal to the maximum allowed ({Model.MaxRiskPctTotal}%).";
                    Print(msg);
                    MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (totalRiskPct >= Model.MaxRiskPctTotal)
                {
                    if (InputAllowSmallerTradesWhenTradingLimitsAreExceeded)
                    {
                        //1 Calculate max risk amount
                        var maxRiskAmount = Model.MaxRiskPctTotal / 100 * Model.AccountSize.Value;
                        //2 Calculate the remaining risk budget
                        var remainingRiskBudget = maxRiskAmount - Model.GetCustomRiskCurrency(PositionsByLabel.ToArray(), PendingOrdersByLabel.ToArray());

                        vol = Symbol.VolumeForFixedRisk(remainingRiskBudget, sl.Value);

                        if (vol < Symbol.VolumeInUnitsMin)
                        {
                            var msg = $"Cannot place trade: adjusted trade volume ({vol:F2}) to fit within the maximum total risk, but it is below the broker's minimum allowed ({Symbol.VolumeInUnitsMin}). Trade not executed.";
                            MessageBox.Show(messageBoxText: msg,
                                caption: "Error",
                                button: MessageBoxButton.OK);
                            return;
                        }
                        
                        vol = Symbol.NormalizeVolumeInUnits(vol, InputRoundingPositionSizeAndPotentialReward);
                        
                        var result = MessageBox.Show(
                            $"Take a smaller trade? Total risk ({totalRiskPct:F2}%) exceeds the maximum allowed ({Model.MaxRiskPctTotal}%). New position volume: {vol:F2}.",
                        "Confirmation", 
                        MessageBoxButton.OKCancel);
                        
                        if (result != MessageBoxResult.OK)
                            return;
                    }
                    else
                    {
                        var msg = $"Cannot place trade: total risk after this trade ({totalRiskPct:F2}%) would exceed the maximum allowed ({Model.MaxRiskPctTotal}%).";
                        Print(msg);
                        MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;   
                    }
                }
            }

            if (Model.MaxRiskPctPerSymbol != 0)
            {
                var existingRiskPerCurrentSymbol = Model.GetCustomRiskPercentage(
                    positions: PositionsByLabel.Where(x => x.SymbolName == SymbolName).ToArray(),
                    pendingOrders: PendingOrdersByLabel.Where(x => x.SymbolName == SymbolName).ToArray());
                var totalRiskPerCurrentSymbol = existingRiskPerCurrentSymbol + potentialRiskPct;
                
                if (existingRiskPerCurrentSymbol >= Model.MaxRiskPctPerSymbol)
                {
                    var msg = $"Cannot place trade: existing risk for this symbol ({existingRiskPerCurrentSymbol:F2}%) is greater than or equal to the maximum allowed per symbol ({Model.MaxRiskPctPerSymbol}%).";
                    Print(msg);
                    MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (totalRiskPerCurrentSymbol >= Model.MaxRiskPctPerSymbol)
                {
                    if (InputAllowSmallerTradesWhenTradingLimitsAreExceeded)
                    {
                        //1 Calculate max risk amount
                        var maxRiskAmount = Model.MaxRiskPctPerSymbol / 100 * Model.AccountSize.Value;
                        //2 Calculate the remaining risk budget
                        var remainingRiskBudget = maxRiskAmount - Model.GetCustomRiskCurrency(
                            PositionsByLabel.Where(x => x.SymbolName == SymbolName).ToArray(), 
                            PendingOrdersByLabel.Where(x => x.SymbolName == SymbolName).ToArray());
                        
                        vol = Symbol.VolumeForFixedRisk(remainingRiskBudget, sl.Value);

                        if (vol < Symbol.VolumeInUnitsMin)
                        {
                            var msg = $"Cannot place trade: adjusted trade volume ({vol:F2}) to fit within the maximum risk per symbol, but it is below the broker's minimum allowed ({Symbol.VolumeInUnitsMin}). Trade not executed.";
                            MessageBox.Show(messageBoxText: msg,
                                caption: "Error",
                                button: MessageBoxButton.OK);
                            return;
                        }
                        
                        vol = Symbol.NormalizeVolumeInUnits(vol, InputRoundingPositionSizeAndPotentialReward);
                        
                        var result = MessageBox.Show(
                            $"Take a smaller trade? Total risk for this symbol ({totalRiskPerCurrentSymbol:F2}%) exceeds the maximum allowed ({Model.MaxRiskPctPerSymbol}%). New position volume: {vol:F2}.",
                        "Confirmation", 
                        MessageBoxButton.OKCancel);
                        
                        if (result != MessageBoxResult.OK)
                            return;
                    }
                    else
                    {
                        var msg = $"Cannot place trade: total risk for this symbol after this trade ({totalRiskPerCurrentSymbol:F2}%) would exceed the maximum allowed ({Model.MaxRiskPctPerSymbol}).";
                        Print(msg);
                        MessageBox.Show(msg, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;   
                    }
                }   
            }
            
            if (InputSurpassBrokerMaxPositionSizeWithMultipleTrades && vol > Symbol.VolumeInUnitsMax)
            {
                //step 1: Check how many times we can trade the max volume + the remainder
                var tradeHowManyTimes = (int)Math.Floor(vol / Symbol.VolumeInUnitsMax);
                var remainder = vol % Symbol.VolumeInUnitsMax;
                //step 2: Trade the max volume
                for (int i = 0; i < tradeHowManyTimes; i++)
                    SendTrades(Symbol.VolumeInUnitsMax, sl, tp, comment, expiry);
                //step 3: Trade the remainder
                SendTrades(remainder, sl, tp, comment, expiry);
            }
            else
            {
                SendTrades(vol, sl, tp, comment, expiry);
            }
        }
    }

    private void SendTrades(double volume, double? sl, double? tp, string comment, DateTime? expiry)
    {
        volume = Symbol.NormalizeVolumeInUnits(volume, InputRoundingPositionSizeAndPotentialReward);
        
        switch (Model.OrderType)
        {
            case OrderType.Instant:
                if (InputApplySlTpAfterAllTradesExecuted)
                    PlaceMarketOrderWithDelayedSlTpTargets(volume, sl, tp, comment);
                else
                {
                    if (Model.MaxSlippagePips == 0)
                    {
                        if (InputUseAsyncOrders)
                            ExecuteMarketOrderAsync(Model.TradeType, SymbolName, volume, Model.Label, sl, tp, comment);
                        else
                            ExecuteMarketOrder(Model.TradeType, SymbolName, volume, Model.Label, sl, tp, comment);
                    }
                    else
                    {
                        var marketRangePips = Model.MaxSlippagePips;
                        var basePrice = Model.TradeType == TradeType.Buy ? Symbol.Ask : Symbol.Bid;

                        if (InputUseAsyncOrders)
                            ExecuteMarketRangeOrderAsync(Model.TradeType, SymbolName, volume, marketRangePips, basePrice, Model.Label, sl, tp, comment);
                        else
                            ExecuteMarketRangeOrder(Model.TradeType, SymbolName, volume, marketRangePips, basePrice, Model.Label, sl, tp,comment);
                    }
                }
                break;
            case OrderType.Pending:
                if (Model.TradeType == TradeType.Buy)
                {
                    if (Model.EntryPrice >= Symbol.Ask)
                    {
                        if (InputUseAsyncOrders)
                        {
                            PlaceStopOrderAsync(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative, //pips
                                expiration: expiry,
                                comment: comment,
                                hasTrailingStop: false,
                                stopLossTriggerMethod: StopTriggerMethod.Trade, //default 
                                stopOrderTriggerMethod: StopTriggerMethod.Trade,
                                callback: null);
                        }
                        else
                        {
                            PlaceStopOrder(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                    }
                    else
                    {
                        if (InputUseAsyncOrders)
                        {
                            PlaceLimitOrderAsync(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                        else
                        {
                            PlaceLimitOrder(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                    }
                }
                else
                {
                    if (Model.EntryPrice >= Symbol.Bid)
                    {
                        if (InputUseAsyncOrders)
                        {
                            PlaceLimitOrderAsync(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                        else
                        {
                            PlaceLimitOrder(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                    }
                    else
                    {
                        if (InputUseAsyncOrders)
                        {
                            PlaceStopOrderAsync(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                        else
                        {
                            PlaceStopOrder(
                                tradeType: Model.TradeType,
                                symbolName: SymbolName,
                                volume: volume,
                                targetPrice: Model.EntryPrice,
                                label: Model.Label,
                                stopLoss: sl,
                                takeProfit: tp,
                                protectionType: ProtectionType.Relative,
                                expiration: expiry,
                                comment: comment);
                        }
                    }
                }

                break;
            case OrderType.StopLimit:
                var stopLimitRangePips = Math.Round(Math.Abs(Model.StopLimitPrice - Model.EntryPrice), 1);
                
                if (InputUseAsyncOrders)
                {
                    PlaceStopLimitOrderAsync(
                        tradeType: Model.TradeType,
                        symbolName: SymbolName,
                        volume: volume,
                        targetPrice: Model.EntryPrice,
                        stopLimitRangePips: stopLimitRangePips,
                        label: Model.Label,
                        stopLoss: sl,
                        takeProfit: tp,
                        protectionType: ProtectionType.Relative,
                        expiration: expiry,
                        comment: comment);
                }
                else
                {
                    PlaceStopLimitOrder(
                        tradeType: Model.TradeType,
                        symbolName: SymbolName,
                        volume: volume,
                        targetPrice: Model.EntryPrice,
                        stopLimitRangePips: stopLimitRangePips,
                        label: Model.Label,
                        stopLoss: sl,
                        takeProfit: tp,
                        protectionType: ProtectionType.Relative,
                        expiration: expiry,
                        comment: comment);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PlaceMarketOrderWithDelayedSlTpTargets(double volume, double? sl, double? tp, string comment)
    {
        TradeResult result;
        
        if (Model.MaxSlippagePips == 0)
        {
            if (InputUseAsyncOrders)
            {
                ExecuteMarketOrderAsync(Model.TradeType, SymbolName, volume, Model.Label, null, null, comment, r =>
                {
                    if (r.IsSuccessful)
                    {
                        double? slPrice = null;

                        if (sl != 0)
                        {
                            slPrice = Model.TradeType == TradeType.Buy
                                ? r.Position.EntryPrice - sl * Symbol.PipSize
                                : r.Position.EntryPrice + sl * Symbol.PipSize;
                        }

                        double? tpPrice = null;

                        if (tp != 0)
                        {
                            tpPrice = Model.TradeType == TradeType.Buy
                                ? r.Position.EntryPrice + tp * Symbol.PipSize
                                : r.Position.EntryPrice - tp * Symbol.PipSize; 
                        }
                        
                        ModifyPositionAsync(
                            position: r.Position,
                            stopLoss: slPrice,
                            takeProfit: tpPrice,
                            protectionType: ProtectionType.Absolute,
                            callback: null);
                    }
                });

                return;
            }

            result = ExecuteMarketOrder(Model.TradeType, SymbolName, volume,Model.Label, null, null, comment);
        }
        else
        {
            var marketRangePips = Model.MaxSlippagePips / Symbol.PipSize;
            var basePrice = Model.TradeType == TradeType.Buy ? Symbol.Ask : Symbol.Bid;

            if (InputUseAsyncOrders)
            {
                ExecuteMarketRangeOrderAsync(Model.TradeType, SymbolName, volume, marketRangePips, basePrice, Model.Label, null, null, comment, r =>
                {
                    if (r.IsSuccessful)
                    {
                        double? slPrice = null;

                        if (sl != 0)
                        {
                            slPrice = Model.TradeType == TradeType.Buy
                                ? r.Position.EntryPrice - sl * Symbol.PipSize
                                : r.Position.EntryPrice + sl * Symbol.PipSize;
                        }

                        double? tpPrice = null;

                        if (tp != 0)
                        {
                            tpPrice = Model.TradeType == TradeType.Buy
                                ? r.Position.EntryPrice + tp * Symbol.PipSize
                                : r.Position.EntryPrice - tp * Symbol.PipSize; 
                        }
                        
                        ModifyPositionAsync(
                            position: r.Position,
                            stopLoss: slPrice,
                            takeProfit: tpPrice,
                            protectionType: ProtectionType.Absolute,
                            callback: null);
                    }
                });
                return;
            }

            result = ExecuteMarketRangeOrder(Model.TradeType, SymbolName, volume, marketRangePips, basePrice, Model.Label, null, null, comment);
        }

        if (result.IsSuccessful)
        {
            double? slPrice = null;

            if (sl != 0)
            {
                slPrice = Model.TradeType == TradeType.Buy
                    ? result.Position.EntryPrice - sl * Symbol.PipSize
                    : result.Position.EntryPrice + sl * Symbol.PipSize;
            }

            double? tpPrice = null;

            if (tp != 0)
            {
                tpPrice = Model.TradeType == TradeType.Buy
                    ? result.Position.EntryPrice + tp * Symbol.PipSize
                    : result.Position.EntryPrice - tp * Symbol.PipSize; 
            }
            
            ModifyPosition(
                position: result.Position,
                stopLoss: slPrice,
                takeProfit: tpPrice,
                protectionType: ProtectionType.Absolute);
        }
    }
}