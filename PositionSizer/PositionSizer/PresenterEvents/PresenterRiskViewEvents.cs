using System;
using cAlgo.API;
using cAlgo.Robots.Tools;

namespace cAlgo.Robots;

public class PresenterRiskViewEvents
{
    
}

public partial class PositionSizer
{
    private void RiskViewOnIncludeOrdersModeChanged(object sender, EventArgs e)
    {
        Model.IncludeOrdersMode = BotTools.NextEnumValue(Model.IncludeOrdersMode);
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }
    
    private void RiskViewOnIncludeSymbolsModeChanged(object sender, EventArgs e)
    {
        Model.IncludeSymbolsMode = BotTools.NextEnumValue(Model.IncludeSymbolsMode);
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }
    
    private void RiskViewOnIncludeDirectionsModeChanged(object sender, EventArgs e)
    {
        Model.IncludeDirectionsMode = BotTools.NextEnumValue(Model.IncludeDirectionsMode);
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }
    
    private void IgnoreOrdersWithoutStopLossCheckBoxChecked(object sender, EventArgs e)
    {
        Model.IgnoreOrdersWithoutStopLoss = true;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }
    
    private void IgnoreOrdersWithoutStopLossCheckBoxUnchecked(object sender, EventArgs e)
    {
        Model.IgnoreOrdersWithoutStopLoss = false;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void IgnoreOrdersWithoutTakeProfitCheckBoxChecked(object sender, EventArgs e)
    {
        Model.IgnoreOrdersWithoutTakeProfit = true;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void IgnoreOrdersWithoutTakeProfitCheckBoxUnchecked(object sender, EventArgs e)
    {
        Model.IgnoreOrdersWithoutTakeProfit = false;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void RiskViewAreaClicked(ButtonClickEventArgs obj)
    {
        Print("Risk View Area Clicked");
    }
}