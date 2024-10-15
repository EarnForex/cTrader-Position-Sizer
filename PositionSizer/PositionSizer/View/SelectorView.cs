using System;
using cAlgo.API;

namespace cAlgo.Robots;

public interface ISelectorViewResources
{
    
}

public class SelectorView : Grid, ISelectorViewResources
{
    private readonly ISelectorViewResources _resources;
    
    private readonly Button _mainButton;
    private readonly Button _riskButton;
    private readonly Button _marginButton;
    private readonly Button _swapsButton;
    private readonly Button _tradingButton;

    public event Action<ButtonClickEventArgs> MainButtonClick;
    public event Action<ButtonClickEventArgs> RiskButtonClick;
    public event Action<ButtonClickEventArgs> MarginButtonClick;
    public event Action<ButtonClickEventArgs> SwapsButtonClick;
    public event Action<ButtonClickEventArgs> TradingButtonClick;

    public SelectorView(ISelectorViewResources resources)
    {
        _resources = resources;

        AddRow();
        AddColumns(5);
        
        //ShowGridLines = true;
        
        _mainButton = CreateButton("Main");
        _mainButton.Click += args => MainButtonClick?.Invoke(args);
        
        _riskButton = CreateButton("Risk");
        _riskButton.Click += args => RiskButtonClick?.Invoke(args);
        
        _marginButton = CreateButton("Margin");
        _marginButton.Click += args => MarginButtonClick?.Invoke(args);
        
        _swapsButton = CreateButton("Swaps");
        _swapsButton.Click += args => SwapsButtonClick?.Invoke(args);
        
        _tradingButton = CreateButton("Trading");
        _tradingButton.Click += args => TradingButtonClick?.Invoke(args);
        
        AddChild(_mainButton, 0, 0);
        Columns[0].SetWidthToAuto();
        AddChild(_riskButton, 0, 1);
        Columns[1].SetWidthToAuto();
        AddChild(_marginButton, 0, 2);
        Columns[2].SetWidthToAuto();
        AddChild(_swapsButton, 0, 3);
        Columns[3].SetWidthToAuto();
        AddChild(_tradingButton, 0, 4);
        Columns[4].SetWidthToAuto();
    }
    
    private Button CreateButton(string text)
    {
        return new Button
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = 5
        };
    }
}