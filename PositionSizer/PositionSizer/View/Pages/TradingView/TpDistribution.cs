using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;

namespace cAlgo.Robots;

public class TpDistributionPriceChangedEventArgs : EventArgs
{
    public int Id { get; }
    public double Price { get; }
    
    public TpDistributionPriceChangedEventArgs(int id, double price)
    {
        Id = id;
        Price = price;
    }
}

public class TpDistributionPercentageChangedEventArgs : EventArgs
{
    public int Id { get; }
    public double Percentage { get; }
    
    public TpDistributionPercentageChangedEventArgs(int id, double percentage)
    {
        Id = id;
        Percentage = percentage;
    }
}

public interface ITpDistributionResources
{
    Button MakeButton(string text);
    TextBlock MakeTextBlock(string text);
    CustomStyle CustomStyle { get; }
    void TrySaveTextBoxesContent();
}

public class TpDistribution : CustomControl, ITpDistributionResources
{
    public TextBlock NameTpTextBlock { get; init;}
    public Button FillEquidistantButton { get; init; }
    public Button PlaceBeyondMainTpButton { get; init; }
    public Button ShareOrPercentageButton { get; init; }
    
    public event EventHandler FillEquidistantButtonClick;
    public event EventHandler PlaceBeyondMainTpButtonClick;
    public event EventHandler ShareOrPercentageButtonClick;
    public event EventHandler<TpDistributionPriceChangedEventArgs> PriceChanged;
    public event EventHandler<TpDistributionPercentageChangedEventArgs> PercentageChanged;
    
    private readonly ITpDistributionResources _resources;
    private readonly IModel _model;
    private readonly Grid _grid;
    public readonly List<TpDistributionRow> TpRows = new();

    public TpDistribution(ITpDistributionResources resources, IModel model)
    {
        _resources = resources;
        _model = model;
        _grid = new Grid(1, 5)
        {
            Width = 370
        };

        NameTpTextBlock = MakeTextBlock("TP");
        FillEquidistantButton = MakeButton("<<");
        FillEquidistantButton.Width = 35;
        FillEquidistantButton.HorizontalAlignment = HorizontalAlignment.Right;
        PlaceBeyondMainTpButton = MakeButton(">>");
        PlaceBeyondMainTpButton.Width = 35;
        PlaceBeyondMainTpButton.HorizontalAlignment = HorizontalAlignment.Right;
        ShareOrPercentageButton = MakeButton("Share, %");
        ShareOrPercentageButton.Margin = new Thickness(10, 0, 0, 0);
        ShareOrPercentageButton.Width = 90;
        ShareOrPercentageButton.HorizontalAlignment = HorizontalAlignment.Left;
        
        FillEquidistantButton.Click += OnFillEquidistantButtonOnClick;
        PlaceBeyondMainTpButton.Click += OnPlaceBeyondMainTpButtonOnClick;
        ShareOrPercentageButton.Click += OnShareOrPercentageButtonOnClick;
        IsVisible = model.TakeProfits.List.Count > 1;

        _grid.Columns[0].SetWidthInPixels(150);
        _grid.AddChild(NameTpTextBlock, 0, 1);
        _grid.Columns[1].SetWidthInPixels(40);
        _grid.AddChild(FillEquidistantButton, 0, 2);
        _grid.Columns[2].SetWidthInPixels(40);
        _grid.AddChild(PlaceBeyondMainTpButton, 0, 3);
        _grid.Columns[3].SetWidthInPixels(40);
        _grid.AddChild(ShareOrPercentageButton, 0, 4);
        
        AddChild(_grid);
        
        AddTps(model);
    }
    
    public void AddTps(IModel model)
    {
        if (model.TakeProfits.List.Count <= 1)
            return;
        
        for (var i = 0; i < model.TakeProfits.List.Count; i++)
        {
            var takeProfit = model.TakeProfits.List[i];
            
            var nameTextBlock = MakeTextBlock($"Take Profit {i + 1}");
            
            var priceTextBox = new XTextBoxDouble(takeProfit.Price, model.TakeProfits.Decimals);
            priceTextBox.SetCustomStyle(CustomStyle);
            priceTextBox.HorizontalAlignment = HorizontalAlignment.Right;
            priceTextBox.Width = 118;
            
            var percentageTextBox = new XTextBoxDouble(takeProfit.Distribution, 0);
            percentageTextBox.Width = 35;
            percentageTextBox.SetCustomStyle(CustomStyle);
            percentageTextBox.ForegroundColor = model.TakeProfits.DistributionAddsUp ? Color.Black : Color.Red;
            
            var tpRow = new TpDistributionRow
            {
                Id = i,
                NameTextBlock = nameTextBlock,
                PriceTextBox = priceTextBox,
                PercentageTextBox = percentageTextBox,
                //ShowGridLines = true
            };

            //tpRow.BackgroundColor = Color.LightSlateGray;
            tpRow.AddRow();
            tpRow.AddChild(tpRow.NameTextBlock, 0, 0);
            tpRow.Columns[0].SetWidthInPixels(150);
            
            tpRow.AddChild(tpRow.PriceTextBox, 0, 1);
            tpRow.Columns[1].SetWidthInPixels(120);
            tpRow.AddChild(tpRow.PercentageTextBox, 0, 2);
            tpRow.Columns[2].SetWidthInPixels(50);

            tpRow.PriceTextBox.ValueUpdated += tpRow.PriceTextBoxOnTextUpdatedAndValid;
            tpRow.PriceTextBox.ControlClicked += (_, _) => TrySaveTextBoxesContent(); 
            tpRow.PercentageTextBox.ValueUpdated += tpRow.PercentageTextBoxOnTextUpdatedAndValid;
            tpRow.PercentageTextBox.ControlClicked += (_, _) => TrySaveTextBoxesContent();
            tpRow.PriceChanged += TpRowOnPriceChanged;
            tpRow.PercentageChanged += TpRowOnPercentageChanged;

            _grid.AddRow();
            TpRows.Add(tpRow);
            _grid.AddChild(tpRow, i + 1, 0, 1, 5);
        }
    }
    
    public void UpdateTpRowValues(IModel tradingModel)
    {
        if (tradingModel.LastKnownState.WindowActive != WindowActive.Trading)
            return;
    
        if (tradingModel.TakeProfits.List.Count != TpRows.Count)
            return;
        
        for (var index = 0; index < TpRows.Count; index++)
        {
            var tpDistRow = TpRows[index];
            tpDistRow.PriceTextBox.Value = tradingModel.TakeProfits.List[index].Price;
            tpDistRow.PercentageTextBox.Value = tradingModel.TakeProfits.List[index].Distribution;
            tpDistRow.PercentageTextBox.ForegroundColor = tradingModel.TakeProfits.DistributionAddsUp ? Color.Black : Color.Red;
        }
    }

    private void OnShareOrPercentageButtonOnClick(ButtonClickEventArgs _)
    {
        ShareOrPercentageButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlaceBeyondMainTpButtonOnClick(ButtonClickEventArgs _)
    {
        PlaceBeyondMainTpButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private void OnFillEquidistantButtonOnClick(ButtonClickEventArgs _)
    {
        FillEquidistantButtonClick?.Invoke(this, EventArgs.Empty);
    }

    public void ClearEvents()
    {
        FillEquidistantButton.Click -= OnFillEquidistantButtonOnClick;
        PlaceBeyondMainTpButton.Click -= OnPlaceBeyondMainTpButtonOnClick;
        ShareOrPercentageButton.Click -= OnShareOrPercentageButtonOnClick;
    }

    public Button MakeButton(string text) => _resources.MakeButton(text);

    public TextBlock MakeTextBlock(string text) => _resources.MakeTextBlock(text);
    public CustomStyle CustomStyle => _resources.CustomStyle;
    public void TrySaveTextBoxesContent()
    {
        _resources.TrySaveTextBoxesContent();
    }

    private void TpRowOnPriceChanged(object sender, TpDistributionPriceChangedEventArgs e)
    {
        PriceChanged?.Invoke(this, new TpDistributionPriceChangedEventArgs(e.Id, e.Price));
    }
    
    private void TpRowOnPercentageChanged(object sender, TpDistributionPercentageChangedEventArgs e)
    {
        PercentageChanged?.Invoke(this, new TpDistributionPercentageChangedEventArgs(e.Id, e.Percentage));
    }
}

