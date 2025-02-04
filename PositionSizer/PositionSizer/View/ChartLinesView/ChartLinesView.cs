using System;
using System.Collections.Generic;
using System.Net.Security;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public class ChartLinesView : IChartLinesViewResources
{
    private readonly IChartLinesViewResources _resources;
    private readonly IModel _model;

    public event EventHandler<ChartLineMovedEventArgs> EntryLineMoved;
    public event EventHandler<TargetLineMovedEventArgs> TargetLineMoved;
    public event EventHandler<ChartLineMovedEventArgs> StopLossLineMoved;
    public event EventHandler<ChartLineMovedEventArgs> StopPriceLineMoved;
    
    public event EventHandler EntryLineRemoved;
    public event EventHandler<TargetLineRemovedEventArgs> TargetLineRemoved;
    public event EventHandler StopLossLineRemoved;
    public event EventHandler StopPriceLineRemoved;
    
    public event EventHandler TradeButtonClicked;
    
    private double _lastKnownEntryPrice, _lastKnownStopLossPrice, _lastKnownStopPrice;
    private DateTime _lastTimeChecked;
    private double _furtherXCoordinate;
    private Button _tradeButton;

    private int IndexFromXCoordinate
    {
        get
        {
            try
            {
                return checked((int)Chart.XToBarIndex(_furtherXCoordinate));
            }
            catch (OverflowException)
            {
                return Chart.Bars.Count - 1;
            }
        }
    }

    public ChartText EntryText { get; set; }
    public ChartText EntryAdditionalText { get; set; }
    public ChartHorizontalLine StopLossLine { get; set; }
    public ChartHorizontalLine StopPriceLine { get; set; }
    public List<TargetLine> TargetLines { get; set; } = new();
    public ChartHorizontalLine EntryLine { get; private set; }
    public ChartText StopLossText { get; set; }
    public ChartText StopAdditionalText { get; set; }
    public ChartText StopPriceText { get; set; }
    
    private const string EntryLineTag = "EntryLine";
    private const string EntryTextTag = "EntryText";
    private const string EntryAdditionalTextTag = "EntryAdditionalText";
    
    private const string StopLossLineTag = "StopLossLine";
    private const string StopLossTextTag = "StopLossText";
    private const string StopAdditionalTextTag = "StopAdditionalText";
    
    private const string StopPriceLineTag = "StopPriceLine";
    private const string StopPriceTextTag = "StopPriceText";

    public ChartLinesView(IChartLinesViewResources resources, IModel model)
    {
        _resources = resources;
        _model = model;

        Chart.ObjectsRemoved += OnChartObjectsRemoved;
        Chart.ObjectsUpdated += OnChartObjectsUpdated;
        Chart.ScrollChanged += ChartOnScrollChanged;
        TimerEvent += OnTimerEvent;

        _furtherXCoordinate = Chart.Width - IndexForLabelReference;
    }

    private void OnTimerEvent(object sender, EventArgs e)
    {
        if (Server.Time < _lastTimeChecked.AddSeconds(1))
            return;
        
        _furtherXCoordinate = Chart.Width - IndexForLabelReference;
        
        _lastTimeChecked = Server.Time;
        
        DrawEntryText(_model);
        DrawTradeButton(_model);
        DrawStopLine(_model);
        DrawStopText(_model);

        UpdateLines(_model);
        
        if (_model.OrderType == OrderType.StopLimit)
        {
            DrawStopPriceLinesAndText(_model);
        }
    }

    private void ChartOnScrollChanged(ChartScrollEventArgs obj)
    {
        
    }

    private void OnChartObjectsUpdated(ChartObjectsUpdatedEventArgs obj)
    {
        if (EntryLine != null && Chart.FindObject(EntryLine.Name) != null)
        {
            if (EntryLine.Y.IsNot(_lastKnownEntryPrice, Symbol.TickSize))
            {
                OnEntryLineMoved(new ChartLineMovedEventArgs(EntryLine.Y));
                _lastKnownEntryPrice = EntryLine.Y;
                DrawTradeButton(_model);
            }
        }

        for (var index = 0; index < TargetLines.Count; index++)
        {
            var targetLine = TargetLines[index];
            if (targetLine.TargetLineObj != null && Chart.FindObject(targetLine.TargetLineObj.Name) != null)
            {
                if (targetLine.TargetLineObj.Y.IsNot(targetLine.LastKnownYPrice, Symbol.TickSize))
                {
                    OnTargetLineMoved(new TargetLineMovedEventArgs(index, targetLine.TargetLineObj.Y));
                    targetLine.LastKnownYPrice = targetLine.TargetLineObj.Y;
                }
            }
        }
        
        if (StopLossLine != null && Chart.FindObject(StopLossLine.Name) != null)
        {
            if (StopLossLine.Y.IsNot(_lastKnownStopLossPrice, Symbol.TickSize))
            {
                OnStopLineMoved(new ChartLineMovedEventArgs(StopLossLine.Y));
                _lastKnownStopLossPrice = StopLossLine.Y;
            }
        }
        
        if (StopPriceLine != null && Chart.FindObject(StopPriceLine.Name) != null)
        {
            if (StopPriceLine.Y.IsNot(_lastKnownStopPrice, Symbol.TickSize))
            {
                OnStopPriceLineMoved(new ChartLineMovedEventArgs(StopPriceLine.Y));
                _lastKnownStopPrice = StopPriceLine.Y;
            }
        }
    }

    private void OnChartObjectsRemoved(ChartObjectsRemovedEventArgs obj)
    {
        //It should be enough for the interactive lines only
        
        if (EntryLine == null || Chart.FindObject(EntryLineTag) == null) 
            OnEntryLineRemoved();

        for (var index = 0; index < TargetLines.Count; index++)
        {
            var targetLine = TargetLines[index];
            
            if (targetLine.TargetLineObj == null || Chart.FindObject(TargetLine.TargetLineTag + index) == null)
                OnTargetLineRemoved(new TargetLineRemovedEventArgs(index));
        }
        
        if (StopLossLine == null || Chart.FindObject(StopLossLineTag) == null)
            OnStopLineRemoved();
        
        if (StopPriceLine == null || Chart.FindObject(StopPriceLineTag) == null)
            OnStopPriceLineRemoved();
    }

    public void DrawLines(IModel model)
    {
        DrawEntryLine(model);
        DrawEntryText(model);

        DrawTradeButton(model);

        DrawTargetLinesAndText(model);

        DrawStopLine(model);
        DrawStopText(model);
        
        if (model.OrderType == OrderType.StopLimit)
        {
            DrawStopPriceLinesAndText(model);
        }
    }

    private void DrawTradeButton(IModel model)
    {
        if (InputAdditionalTradeButtons is AdditionalTradeButtons.None or AdditionalTradeButtons.MainTab)
            return;

        if (_tradeButton == null)
        {
            // var text = InputShowAdditionalEntryLabel ? $"{model.TradeType} {model.OrderType} - {model.TradeSize.Lots:F}" : $"{model.TradeType} {model.OrderType}";
            _tradeButton = new Button
            {
                Text = "Trade",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Height = PositionSizer.ControlHeight,
                //FontSize = 9,
                Width = 140,
                Margin = new Thickness(-100, -PositionSizer.ControlHeight, 0, 0),
                Style = CustomStyle.ButtonStyle 
            };
            
            // //Style = CustomStyle.ButtonStyle,
            // Text = text,
            // HorizontalAlignment = HorizontalAlignment.Left,
            // VerticalAlignment = VerticalAlignment.Center,
            // // Margin = new Thickness(2, 4, 2, 4),
            // Margin = 1,
            // Height = ControlHeight

            _tradeButton.Click += TradeButtonOnClick;
            //Chart.AddControl(_tradeButton);
            Chart.AddControl(_tradeButton, IndexFromXCoordinate, model.EntryPrice);
        }
        else
        {
            // _tradeButton.Text = InputShowAdditionalEntryLabel ? $"{model.TradeType} {model.OrderType} - {model.TradeSize.Lots:F}" : $"{model.TradeType} {model.OrderType}";
            Chart.RemoveControl(_tradeButton);
            Chart.AddControl(_tradeButton, IndexFromXCoordinate, model.EntryPrice);
            
            //There's an issue in which using this method below, it causes the button to remain
            //on chart after the bot has stopped
            //Chart.MoveControl(_tradeButton, IndexFromXCoordinate, model.EntryPrice);   
        }
    }

    private void TradeButtonOnClick(ButtonClickEventArgs obj)
    {
        TradeButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    public void DrawStopPriceLinesAndText(IModel model)
    {
        StopPriceLine = Chart.DrawHorizontalLine(StopPriceLineTag, model.StopLimitPrice, InputStopPriceLineColor, InputStopPriceLineWidth, InputStopPriceLineStyle);
        StopPriceLine.IsInteractive = true;
        StopPriceLine.IsHidden = model.HideLines;
        
        _lastKnownStopPrice = model.StopLimitPrice;
        
        var distanceFromEntry = Math.Abs(model.EntryPrice - model.StopLimitPrice) / Symbol.PipSize;

        StopPriceText = Chart.DrawText(StopPriceTextTag, $"{distanceFromEntry:F1}", IndexFromXCoordinate, model.StopLimitPrice, InputStopPriceLabelColor);
        StopPriceText.IsHidden = model.HideLines;
        StopPriceText.FontSize = InputLabelsFontSize;
        StopPriceText.VerticalAlignment = VerticalAlignment.Bottom;
        StopPriceText.HorizontalAlignment = HorizontalAlignment.Left;
    }

    public void RemoveStopPriceLinesAndText()
    {
        if (StopPriceLine != null)
            Chart.RemoveObject(StopPriceLineTag);
        
        if (StopPriceText != null)
            Chart.RemoveObject(StopPriceTextTag);
    }
    
    public void UpdateStopPriceLine(IModel model)
    {
        _lastKnownStopPrice = model.StopLimitPrice;
        
        var distanceFromEntry = Math.Abs(model.EntryPrice - model.StopLimitPrice) / Symbol.PipSize;
        
        StopPriceText.Y = model.StopLimitPrice;
        StopPriceText.Text = $"{distanceFromEntry:F1}";
        StopPriceLine.Y = model.StopLimitPrice;
    }

    private void DrawStopText(IModel model)
    {
        if (!InputShowLineLabels)
            return;
        
        StopLossText = Chart.DrawText(StopLossTextTag, $"{GetStopDistancePips(model):F1}", IndexFromXCoordinate, model.StopLoss.Price, InputStopLossLabelColor);
        StopLossText.IsHidden = model.HideLines;
        StopLossText.FontSize = InputLabelsFontSize;
        StopLossText.VerticalAlignment = VerticalAlignment.Bottom;
        StopLossText.HorizontalAlignment = HorizontalAlignment.Left;
    }

    public void DrawStopLine(IModel model)
    {
        StopLossLine = Chart.DrawHorizontalLine(StopLossLineTag, model.StopLoss.Price, InputStopLossLineColor, InputStopLossLineWidth, InputStopLossLineStyle);
        StopLossLine.IsHidden = model.HideLines;
        StopLossLine.IsInteractive = true;
        
        _lastKnownStopLossPrice = model.StopLoss.Price;

        //StopLossAdditionalText shows SL $/%, example:
        //2.37% (212.99 USD)
        if (InputShowAdditionalStopLossLabel)
        {
            StopAdditionalText = Chart.DrawText(StopAdditionalTextTag, GetExtraStopLossText(model), IndexFromXCoordinate, model.StopLoss.Price, InputStopLossLabelColor);
            StopAdditionalText.FontSize = InputLabelsFontSize;
            StopAdditionalText.VerticalAlignment = VerticalAlignment.Top;
            StopAdditionalText.HorizontalAlignment = HorizontalAlignment.Left;
            StopAdditionalText.IsHidden = model.HideLines;
        }
    }

    public void DrawTargetLinesAndText(IModel model)
    {
        for (var index = 0; index < model.TakeProfits.List.Count; index++)
        {
            var takeProfit = model.TakeProfits.List[index];
            
            var targetLine = new TargetLine
            {
                TargetLineObj = Chart.DrawHorizontalLine(TargetLine.TargetLineTag + index, takeProfit.Price, InputTakeProfitLineColor,
                    InputTakeProfitLineWidth, InputTakeProfitLineStyle),
                LastKnownYPrice = takeProfit.Price
            };

            targetLine.TargetLineObj.IsInteractive = true;
            targetLine.TargetLineObj.IsHidden = model.HideLines;
            
            //TargetAdditionalText shows TP $/% + R/R, example:
            //2.37% (212.99 USD) 0.45R
            if (InputShowAdditionalTpLabel)
            {
                targetLine.TargetAdditionalTextObj = Chart.DrawText(TargetLine.TargetAdditionalTextTag + index, 
                    GetExtraTakeProfitText(model, index), IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor);
                targetLine.TargetAdditionalTextObj.FontSize = InputLabelsFontSize;
                targetLine.TargetAdditionalTextObj.VerticalAlignment = VerticalAlignment.Top;
                targetLine.TargetAdditionalTextObj.HorizontalAlignment = HorizontalAlignment.Left;
                targetLine.TargetAdditionalTextObj.IsHidden = model.HideLines;
            }

            if (InputShowLineLabels)
            {
                targetLine.TargetTextObj = Chart.DrawText(TargetLine.TargetTextTag + index, $"{GetTargetDistancePips(model, index):F1}", IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor);
                targetLine.TargetTextObj.FontSize = InputLabelsFontSize;
                targetLine.TargetTextObj.IsHidden = model.HideLines;
                targetLine.TargetTextObj.VerticalAlignment = VerticalAlignment.Bottom;
                targetLine.TargetTextObj.HorizontalAlignment = HorizontalAlignment.Left;
            }
            
            TargetLines.Add(targetLine);
        }
    }

    public void DrawEntryText(IModel model)
    {
        if (model.OrderType != OrderType.Instant)
        {
            EntryText = Chart.DrawText(EntryTextTag, $"{GetEntryDistancePips(model):F1}", IndexFromXCoordinate, model.EntryPrice, InputEntryLabelColor);
            EntryText.FontSize = InputLabelsFontSize;
            EntryText.VerticalAlignment = VerticalAlignment.Bottom;
            EntryText.HorizontalAlignment = HorizontalAlignment.Left;
            EntryText.IsHidden = model.HideLines;
        }

        //Additional Entry Label just shows the Position Size
        if (InputShowAdditionalEntryLabel /*&& InputAdditionalTradeButtons is AdditionalTradeButtons.Both or AdditionalTradeButtons.AboveTheEntryLine*/)
        {
            EntryAdditionalText = Chart.DrawText(EntryAdditionalTextTag, model.TradeSize.Lots.ToString("F"), IndexFromXCoordinate, model.EntryPrice, InputEntryLabelColor);
            EntryAdditionalText.FontSize = InputLabelsFontSize;
            EntryAdditionalText.VerticalAlignment = VerticalAlignment.Top;
            EntryAdditionalText.HorizontalAlignment = HorizontalAlignment.Left;
            EntryAdditionalText.IsHidden = model.HideLines;
        }
    }

    public void DrawEntryLine(IModel model)
    {
        EntryLine = Chart.DrawHorizontalLine(EntryLineTag, model.EntryPrice, InputEntryLineColor, InputEntryLineWidth, InputEntryLineStyle);
        EntryLine.IsHidden = model.HideLines || (InputHideEntryLineForInstantOrders && model.OrderType == OrderType.Instant);
        
        if (model.OrderType == OrderType.Instant) 
            EntryLine.IsInteractive = false;   
        
        _lastKnownEntryPrice = model.EntryPrice;
    }

    private double GetStopDistancePips(IModel model)
    {
        return model.StopLoss.Pips;
    }

    private double GetTargetDistancePips(IModel model, int takeProfitIndex)
    {
        return model.TakeProfits.List[takeProfitIndex].Pips;
    }

    private double GetEntryDistancePips(IModel model)
    {
        return model.TradeType == TradeType.Buy 
            ? (Math.Abs(model.EntryPrice - Symbol.Ask) / Symbol.PipSize).Round(1) : 
            (Math.Abs(model.EntryPrice - Symbol.Bid) / Symbol.PipSize).Round(1);
    }
    
    private string GetExtraStopLossText(IModel model)
    {
        var slUsd = Symbol.AmountRisked(model.TradeSize.Volume, model.StopLoss.Pips);
        var slPct = (slUsd / model.AccountSize.Value) * 100.0;
        
        return $"{slPct:F2}% ({slUsd:F2} USD)";
    }

    private string GetExtraTakeProfitText(IModel model, int tpIndex)
    {
        //if there's just one takeProfit-targetLine, it doesn't write the lot sizes
        //if there are multiple takeProfit-targetLines, it writes the lot sizes first (the lot sizes are split equally)
        if (model.TakeProfits.List.Count == 1)
        {
            var tpUsd = Symbol.AmountRisked(model.TradeSize.Volume, model.TakeProfits.List[0].Pips);
            var slRisked = Symbol.AmountRisked(model.TradeSize.Volume, model.StopLoss.Pips);
            var tpPct = (tpUsd / model.AccountSize.Value) * 100.0;
            var rr = tpUsd / slRisked;
            
            return $"{tpPct:F2}% ({tpUsd:F2} USD) {rr:F2}R";
        }
        else
        {
            var distribution = model.TakeProfits.List[tpIndex].Distribution / 100.0;
            var tpUsd = Symbol.AmountRisked(model.TradeSize.Volume * distribution, model.TakeProfits.List[tpIndex].Pips);
            var slRisked = Symbol.AmountRisked(model.TradeSize.Volume * distribution, model.StopLoss.Pips);
            var tpPct = (tpUsd / model.AccountSize.Value) * 100.0;
            var rr = tpUsd / slRisked;
            var lots = model.TradeSize.Lots; 
            
            return $"{lots * distribution:F2} Lots {tpPct:F2}% ({tpUsd:F2} USD) {rr:F2}R";
        }
    }
    
    // private void UpdateTextOfFirstTakeProfitWithLots(MainModel model)
    // {
    //     var tpUsd = Symbol.AmountRisked(model.TradeSize.Volume, model.TakeProfits[0].Pips);
    //     var slRisked = Symbol.AmountRisked(model.TradeSize.Volume, model.StopLoss.Pips);
    //     var tpPct = (tpUsd / model.AccountSize.Value) * 100.0;
    //     var rr = tpUsd / slRisked;
    //     
    //     TargetLines[0].TargetAdditionalTextObj.Text = $"{model.TradeSize.Lots:F2} Lots {tpPct:F2}% ({tpUsd:F2} USD) {rr:F2}R";
    // }
    
    private void UpdateTextOfFirstTakeProfitWithoutLots(IModel model)
    {
        var tpUsd = Symbol.AmountRisked(model.TradeSize.Volume, model.TakeProfits.List[0].Pips);
        var slRisked = Symbol.AmountRisked(model.TradeSize.Volume, model.StopLoss.Pips);
        var tpPct = (tpUsd / model.AccountSize.Value) * 100.0;
        var rr = tpUsd / slRisked;
        
        if (InputShowAdditionalTpLabel)
            TargetLines[0].TargetAdditionalTextObj.Text = $"{tpPct:F2}% ({tpUsd:F2} USD) {rr:F2}R";
    }

    public void UpdateLines(IModel model)
    {
        _lastKnownEntryPrice = model.EntryPrice;
        
        if (EntryLine != null)
        {
            EntryLine.Y = model.EntryPrice;
            EntryLine.IsInteractive = model.OrderType != OrderType.Instant;
            EntryLine.IsHidden = model.HideLines || (InputHideEntryLineForInstantOrders && model.OrderType == OrderType.Instant);
        }

        if (EntryText != null)
        {
            EntryText.Y = model.EntryPrice;
            EntryText.Text = $"{GetEntryDistancePips(model):F1}";
            EntryText.IsHidden = model.OrderType == OrderType.Instant || model.HideLines;   
        }

        if (EntryAdditionalText != null)
        {
            EntryAdditionalText.Y = model.EntryPrice;
            EntryAdditionalText.Text = model.TradeSize.Lots.ToString("F");
            EntryAdditionalText.IsHidden = model.HideLines;
        }

        if (InputShowAdditionalEntryLabel && InputAdditionalTradeButtons is AdditionalTradeButtons.Both or AdditionalTradeButtons.AboveTheEntryLine)
        {
            if (EntryAdditionalText != null)
            {
                EntryAdditionalText.Y = model.EntryPrice;
                EntryAdditionalText.Text = model.TradeSize.Lots.ToString("F");   
            }
        }
        
        //This is for when there's a takeProfit removed
        if (model.TakeProfits.List.Count < TargetLines.Count)
        {
            Chart.RemoveObject(TargetLine.TargetLineTag + (TargetLines.Count - 1));
            Chart.RemoveObject(TargetLine.TargetTextTag + (TargetLines.Count - 1));
            Chart.RemoveObject(TargetLine.TargetAdditionalTextTag + (TargetLines.Count - 1));
                
            TargetLines.RemoveAt(TargetLines.Count - 1);
            
            if (model.TakeProfits.List.Count == 1)
                UpdateTextOfFirstTakeProfitWithoutLots(model);
        }
        else
        {
            //maybe there's a new takeProfit added, or an existing one updated
            for (var index = 0; index < model.TakeProfits.List.Count; index++)
            {
                var takeProfit = model.TakeProfits.List[index];
            
                //need to tell apart if there's a new takeProfit added, removed, or an existing one updated
                //for removed, the number of takeProfits in the model is less than the number of targetLines
                //Therefore we remove the last targetLine
                //this one below is for added
                if (index >= TargetLines.Count)
                {
                    var targetLine = new TargetLine
                    {
                        TargetLineObj = Chart.DrawHorizontalLine(TargetLine.TargetLineTag + index, takeProfit.Price, InputTakeProfitLineColor,
                            InputTakeProfitLineWidth, InputTakeProfitLineStyle),
                        LastKnownYPrice = takeProfit.Price
                    };

                    targetLine.TargetLineObj.IsInteractive = true;
                    targetLine.TargetLineObj.IsHidden = model.HideLines;
                
                    if (InputShowAdditionalTpLabel)
                    {
                        targetLine.TargetAdditionalTextObj = Chart.DrawText(TargetLine.TargetAdditionalTextTag + index, 
                            GetExtraTakeProfitText(model, index), IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor);
                        targetLine.TargetAdditionalTextObj.FontSize = InputLabelsFontSize;
                        targetLine.TargetAdditionalTextObj.VerticalAlignment = VerticalAlignment.Top;
                        targetLine.TargetAdditionalTextObj.HorizontalAlignment = HorizontalAlignment.Left;
                        targetLine.TargetAdditionalTextObj.IsHidden = model.HideLines;
                    }

                    if (InputShowLineLabels)
                    {
                        targetLine.TargetTextObj = Chart.DrawText(TargetLine.TargetTextTag + index, $"{GetTargetDistancePips(model, index):F1}", IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor);
                        targetLine.TargetTextObj.FontSize = InputLabelsFontSize;
                        targetLine.TargetTextObj.IsHidden = model.HideLines;
                        targetLine.TargetTextObj.VerticalAlignment = VerticalAlignment.Bottom;
                        targetLine.TargetTextObj.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                
                    TargetLines.Add(targetLine);
                    
                    // if (index == model.TakeProfits.Count - 1)
                    //     UpdateTextOfFirstTakeProfitWithLots(model);
                }
                else
                //this one below is for updated
                {
                    var targetLine = TargetLines[index];
            
                    targetLine.LastKnownYPrice = takeProfit.Price;
                    targetLine.TargetLineObj.Y = takeProfit.Price;

                    if (targetLine.TargetTextObj != null)
                    {
                        targetLine.TargetTextObj = Chart.DrawText(TargetLine.TargetTextTag + index, $"{GetTargetDistancePips(model, index):F1}", IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor);
                        targetLine.TargetTextObj.FontSize = InputLabelsFontSize;
                        targetLine.TargetTextObj.IsHidden = model.HideLines;
                        targetLine.TargetTextObj.VerticalAlignment = VerticalAlignment.Bottom;
                        targetLine.TargetTextObj.HorizontalAlignment = HorizontalAlignment.Left;
                    }
            
                    if (InputShowAdditionalTpLabel)
                    {
                        if (model.TradeType == TradeType.Buy && takeProfit.Price < model.EntryPrice ||
                            model.TradeType == TradeType.Sell && takeProfit.Price > model.EntryPrice)
                        {
                            //draw Invalid TP
                            targetLine.TargetAdditionalTextObj = Chart.DrawText(TargetLine.TargetAdditionalTextTag + index, "Invalid TP", IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor); 
                        }
                        else
                        {
                            targetLine.TargetAdditionalTextObj = Chart.DrawText(TargetLine.TargetAdditionalTextTag + index, 
                                GetExtraTakeProfitText(model, index), IndexFromXCoordinate, takeProfit.Price, InputTpLabelColor);    
                        }
                        
                        targetLine.TargetAdditionalTextObj.FontSize = InputLabelsFontSize;
                        targetLine.TargetAdditionalTextObj.VerticalAlignment = VerticalAlignment.Top;
                        targetLine.TargetAdditionalTextObj.HorizontalAlignment = HorizontalAlignment.Left;
                        targetLine.TargetAdditionalTextObj.IsHidden = model.HideLines;
                    }
                    
                    // if (index == model.TakeProfits.Count - 1)
                    //     UpdateTextOfFirstTakeProfitWithLots(model);
                }
            }
        }
        
        if (TargetLines.Count >= 1)
        {
            if (model.TakeProfits.List[0].Pips == 0)
            {
                TargetLines[0].TargetLineObj.IsHidden = true;
            
                if (InputShowLineLabels)
                    TargetLines[0].TargetTextObj.IsHidden = true;
            
                if (InputShowAdditionalTpLabel)
                    TargetLines[0].TargetAdditionalTextObj.IsHidden = true;   
            }
            else
            {
                TargetLines[0].TargetLineObj.IsHidden = model.HideLines;
            
                if (InputShowLineLabels)
                    TargetLines[0].TargetTextObj.IsHidden = model.HideLines;
            
                if (InputShowAdditionalTpLabel)
                    TargetLines[0].TargetAdditionalTextObj.IsHidden = model.HideLines;
            }
        }

        _lastKnownStopLossPrice = model.StopLoss.Price;
        
        if (StopLossLine != null) 
            StopLossLine.Y = model.StopLoss.Price;

        if (StopLossText != null)
        {
            StopLossText.Y = model.StopLoss.Price;
            StopLossText.Text = $"{GetStopDistancePips(model):F1}";   
        }

        if (InputShowAdditionalStopLossLabel)
        {
            if (StopAdditionalText != null)
            {
                StopAdditionalText.Y = model.StopLoss.Price;
                StopAdditionalText.Text = GetExtraStopLossText(model);   
            }
        }
        
        if (model.OrderType == OrderType.StopLimit)
        {
            UpdateStopPriceLine(model);
        }
    }
    
    public void RemoveLines()
    {
        RemoveChartObject(EntryLine, EntryLineTag);
        RemoveChartObject(EntryText, EntryTextTag);
        RemoveChartObject(EntryAdditionalText, EntryAdditionalTextTag);
        
        if (_tradeButton != null)
        {
            Print("Removing trade button");
            Chart.RemoveControl(_tradeButton);
        }

        for (var index = 0; index < TargetLines.Count; index++)
        {
            var targetLine = TargetLines[index];
            
            RemoveChartObject(targetLine.TargetLineObj, TargetLine.TargetLineTag + index);
            RemoveChartObject(targetLine.TargetTextObj, TargetLine.TargetTextTag + index);
            RemoveChartObject(targetLine.TargetAdditionalTextObj, TargetLine.TargetAdditionalTextTag + index);
        }
        
        RemoveChartObject(StopLossLine, StopLossLineTag);
        RemoveChartObject(StopLossText, StopLossTextTag);
        RemoveChartObject(StopAdditionalText, StopAdditionalTextTag);
        RemoveChartObject(StopPriceLine, StopPriceLineTag);
        RemoveChartObject(StopPriceText, StopPriceTextTag);
    }

    public void HideLines()
    {
        EntryLine.IsHidden = true;
        
        if (EntryText != null)
            EntryText.IsHidden = true;
        
        if (InputShowAdditionalEntryLabel /*&& InputAdditionalTradeButtons is AdditionalTradeButtons.Both or AdditionalTradeButtons.AboveTheEntryLine*/)
            EntryAdditionalText.IsHidden = true;
        
        for (var index = 0; index < TargetLines.Count; index++)
        {
            var targetLine = TargetLines[index];
            
            targetLine.TargetLineObj.IsHidden = true;
            targetLine.TargetTextObj.IsHidden = true;
            
            if (InputShowAdditionalTpLabel)
                targetLine.TargetAdditionalTextObj.IsHidden = true;
        }
        
        StopLossLine.IsHidden = true;
        StopLossText.IsHidden = true;
        
        if (InputShowAdditionalStopLossLabel)
            StopAdditionalText.IsHidden = true;
        
        if (StopPriceLine != null)
        {
            StopPriceLine.IsHidden = true;
            StopPriceText.IsHidden = true;
        }
    }

    public void ShowLines()
    {
        EntryLine.IsHidden = false;
        
        if (EntryText != null)
            EntryText.IsHidden = false;
        
        if (InputShowAdditionalEntryLabel /*&& InputAdditionalTradeButtons is AdditionalTradeButtons.Both or AdditionalTradeButtons.AboveTheEntryLine*/)
            EntryAdditionalText.IsHidden = false;

        for (var index = 0; index < TargetLines.Count; index++)
        {
            var targetLine = TargetLines[index];
            
            targetLine.TargetLineObj.IsHidden = false;
            targetLine.TargetTextObj.IsHidden = false;
            
            if (InputShowAdditionalTpLabel)
                targetLine.TargetAdditionalTextObj.IsHidden = false;
        }
        
        StopLossLine.IsHidden = false;
        StopLossText.IsHidden = false;
        
        if (InputShowAdditionalStopLossLabel)
            StopAdditionalText.IsHidden = false;
        
        if (StopPriceLine != null)
        {
            StopPriceLine.IsHidden = false;
            StopPriceText.IsHidden = false;
        }
    }

    protected virtual void OnStopPriceLineRemoved()
    {
        StopPriceLineRemoved?.Invoke(this, EventArgs.Empty);
    }

    public void RedrawLine(IModel model, int eTakeProfitId)
    {
        var takeProfit = model.TakeProfits.List[eTakeProfitId];
        var targetLine = TargetLines[eTakeProfitId];
        
        //check if targetLine is null or isn't found in the chart
        //redraw it in such case
        if (targetLine.TargetLineObj == null || Chart.FindObject(targetLine.TargetLineObj.Name) == null)
        {
            targetLine.TargetLineObj = Chart.DrawHorizontalLine(TargetLine.TargetLineTag + eTakeProfitId, takeProfit.Price, InputTakeProfitLineColor,
                InputTakeProfitLineWidth, InputTakeProfitLineStyle);
            targetLine.TargetLineObj.IsInteractive = true;
            targetLine.TargetLineObj.IsHidden = model.HideLines;
        }
    }

    private void RemoveChartObject(ChartObject chartObject, string tag)
    {
        if (chartObject != null) 
            Chart.RemoveObject(tag);
    }

    #region Resources

    public AdditionalTradeButtons InputAdditionalTradeButtons => _resources.InputAdditionalTradeButtons;

    public event EventHandler TimerEvent
    {
        add => _resources.TimerEvent += value;
        remove => _resources.TimerEvent -= value;
    }

    public Button MakeButton(string text)
    {
        return _resources.MakeButton(text);
    }

    public void Print(object obj)
    {
        _resources.Print(obj);
    }

    public CustomStyle CustomStyle => _resources.CustomStyle;

    public Chart Chart => _resources.Chart;
    public Symbol Symbol => _resources.Symbol;
    public IServer Server => _resources.Server;
    public bool InputHideEntryLineForInstantOrders => _resources.InputHideEntryLineForInstantOrders;

    public Color InputEntryLineColor => _resources.InputEntryLineColor;
    public LineStyle InputEntryLineStyle => _resources.InputEntryLineStyle;
    public int InputEntryLineWidth => _resources.InputEntryLineWidth;
    public bool InputShowLineLabels => _resources.InputShowLineLabels;
    public int IndexForLabelReference => _resources.IndexForLabelReference;

    public Color InputStopLossLineColor => _resources.InputStopLossLineColor;
    public LineStyle InputStopLossLineStyle => _resources.InputStopLossLineStyle;
    public int InputStopLossLineWidth => _resources.InputStopLossLineWidth;
    public Color InputTakeProfitLineColor => _resources.InputTakeProfitLineColor;
    public LineStyle InputTakeProfitLineStyle => _resources.InputTakeProfitLineStyle;
    public int InputTakeProfitLineWidth => _resources.InputTakeProfitLineWidth;
    public bool InputShowAdditionalStopLossLabel => _resources.InputShowAdditionalStopLossLabel;
    public bool InputShowAdditionalTpLabel => _resources.InputShowAdditionalTpLabel;
    public bool InputShowAdditionalEntryLabel => _resources.InputShowAdditionalEntryLabel;
    public Color InputStopLossLabelColor => _resources.InputStopLossLabelColor;

    public Color InputTpLabelColor => _resources.InputTpLabelColor;

    public Color InputStopPriceLabelColor => _resources.InputStopPriceLabelColor;

    public Color InputEntryLabelColor => _resources.InputEntryLabelColor;

    public int InputLabelsFontSize => _resources.InputLabelsFontSize;
    public Color InputStopPriceLineColor => _resources.InputStopPriceLineColor;

    public LineStyle InputStopPriceLineStyle => _resources.InputStopPriceLineStyle;

    public int InputStopPriceLineWidth => _resources.InputStopPriceLineWidth;

    protected virtual void OnEntryLineMoved(ChartLineMovedEventArgs e) => EntryLineMoved?.Invoke(this, e);
    protected virtual void OnTargetLineMoved(TargetLineMovedEventArgs e) => TargetLineMoved?.Invoke(this, e);
    protected virtual void OnStopLineMoved(ChartLineMovedEventArgs e) => StopLossLineMoved?.Invoke(this, e);

    protected virtual void OnEntryLineRemoved()
    {
        EntryLineRemoved?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnTargetLineRemoved(TargetLineRemovedEventArgs e)
    {
        TargetLineRemoved?.Invoke(this, e);
    }

    protected virtual void OnStopLineRemoved()
    {
        StopLossLineRemoved?.Invoke(this, EventArgs.Empty);
    }
    
    protected virtual void OnStopPriceLineMoved(ChartLineMovedEventArgs e)
    {
        StopPriceLineMoved?.Invoke(this, e);
    }

    #endregion
}