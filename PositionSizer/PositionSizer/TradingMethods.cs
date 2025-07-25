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
            Print("Cannot Place Trade Because TP Distribution Does Not Add Up");
            MessageBox.Show("Cannot Place Trade Because TP Distribution Does Not Add Up", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (Model.AskForConfirmation)
        {
            var question = new StringBuilder();
            //--
            question.AppendLine("Do you want to send the trade?");
            question.AppendLine($"{Model.Trade(Symbol)}");

            var result = MessageBox.Show(
                question.ToString(),
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;
        }

        if (Model.DisableTradingWhenLinesAreHidden && Model.HideLines)
        {
            Print("Trading is disabled because the lines are hidden");
            return;
        }

        if (Model.MaxSpreadPips != 0 && Model.OrderType == OrderType.Instant)
        {
            if (Symbol.Spread / Symbol.PipSize >= Model.MaxSpreadPips)
            {
                Print($"Spread is too high {(Symbol.Spread / Symbol.PipSize)}");
                return;
            }

            Print($"Spread is ok {(Symbol.Spread / Symbol.PipSize)}");
        }

        for (var index = 0; index < Model.TakeProfits.List.Count; index++)
        {
            if (Model.TakeProfits.List[index].Distribution == 0)
            {
                Print($"Cannot Place Trade Because Distribution is 0 for TP {index + 1}");
                continue;
            }

            var vol = Model.TradeSize.Volume * Model.TakeProfits.List[index].Distribution / 100.0;
            var quantity = Symbol.VolumeInUnitsToQuantity(vol);
            
            Print($"Vol to trade: {vol} - Quantity: {quantity}");
                
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
                Print($"Not taking a trade - current Entry to SL distance ({slPips}) > Max Entry SL Distance ({Model.MaxEntryStopLossDistancePips})");
                MessageBox.Show($"Not taking a trade - current Entry to SL distance ({slPips}) > Max Entry SL Distance ({Model.MaxEntryStopLossDistancePips})", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (sl != null && Model.MinEntryStopLossDistancePips != 0 && slPips < Model.MinEntryStopLossDistancePips)
            {
                Print($"Not taking a trade - current Entry to SL distance ({slPips}) < Min Entry SL Distance ({Model.MinEntryStopLossDistancePips})");
                MessageBox.Show($"Not taking a trade - current Entry to SL distance ({slPips}) < Min Entry SL Distance ({Model.MinEntryStopLossDistancePips})", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (Model.MaxNumberOfTradesTotal != 0)
            {
                var totalTrades = PositionsByLabelAndComment.Count();
        
                if (totalTrades >= Model.MaxNumberOfTradesTotal)
                {
                    Print($"Cannot Place Trade Because Max Trades Reached: {totalTrades}");
                    MessageBox.Show($"Cannot Place Trade Because Max Trades Reached: {totalTrades}", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            if (Model.MaxNumberOfTradesPerSymbol != 0)
            {
                var totalTradesPerCurrentSymbol = PositionsByLabelAndComment.Count(x => x.SymbolName == SymbolName);
        
                if (totalTradesPerCurrentSymbol >= Model.MaxNumberOfTradesPerSymbol)
                {
                    Print($"Cannot Place Trade Because Max Trades Per Symbol Reached: {totalTradesPerCurrentSymbol}");
                    MessageBox.Show($"Cannot Place Trade Because Max Trades Per Symbol Reached: {totalTradesPerCurrentSymbol}", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            if (Model.MaxLotsTotal != 0)
            {
                var totalLots = PositionsByLabelAndComment.Sum(x => x.Quantity) + quantity;
        
                if (totalLots > Model.MaxLotsTotal)
                {
                    Print($"Cannot Place Trade Because Max Lots Reached: {totalLots}");
                    MessageBox.Show($"Cannot Place Trade Because Max Lots Reached: {totalLots}", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            if (Model.MaxLotsPerSymbol != 0)
            {
                var totalLotsPerCurrentSymbol = PositionsByLabelAndComment.Where(x => x.SymbolName == SymbolName).Sum(x => x.Quantity) + quantity;
        
                if (totalLotsPerCurrentSymbol > Model.MaxLotsPerSymbol)
                {
                    Print($"Cannot Place Trade Because Max Lots Per Symbol Reached: {totalLotsPerCurrentSymbol}");
                    MessageBox.Show($"Cannot Place Trade Because Max Lots Per Symbol Reached: {totalLotsPerCurrentSymbol}", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            var potentialRisk = sl.HasValue ? Symbol.AmountRisked(vol, sl.Value) : 0;
            var potentialRiskPct = potentialRisk / Model.AccountSize.Value * 100;
            
            Print($"Potential Risk: {potentialRisk} Potential Risk Pct: {potentialRiskPct}");

            if (Model.MaxRiskPctTotal != 0)
            {
                var totalRisk = PositionsByLabelAndComment.Sum(x => x.PctRisk(Model.AccountSize.Value)) + potentialRiskPct;
        
                if (totalRisk >= Model.MaxRiskPctTotal)
                {
                    Print($"Cannot Place Trade Because Max Risk Reached: {totalRisk}");
                    MessageBox.Show($"Cannot Place Trade Because Max Risk Reached: {totalRisk}", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            if (Model.MaxRiskPctPerSymbol != 0)
            {
                var totalRiskPerCurrentSymbol = PositionsByLabelAndComment.Where(x => x.SymbolName == SymbolName).Sum(x => x.PctRisk(Model.AccountSize.Value)) + potentialRiskPct;
        
                if (totalRiskPerCurrentSymbol >= Model.MaxRiskPctPerSymbol)
                {
                    Print($"Cannot Place Trade Because Max Risk Per Symbol Reached: {totalRiskPerCurrentSymbol}");
                    MessageBox.Show($"Cannot Place Trade Because Max Risk Per Symbol Reached: {totalRiskPerCurrentSymbol}", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }   
            }

            var expiry = Model.OrderType != OrderType.Instant &&
                         Model.ExpirationSeconds != 0
                ? Server.Time.AddSeconds(Model.ExpirationSeconds)
                : (DateTime?)null;
            
            var comment = Model.AutoSuffix ? Model.Comment + InstanceId : Model.Comment;

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