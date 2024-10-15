using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class PresenterRiskViewEvents
{
    
}

public partial class PositionSizer
{
    private void CountPendingOrdersCheckBoxChecked(object sender, EventArgs e)
    {
        Model.CountPendingOrders = true;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }
    
    private void CountPendingOrdersCheckBoxUnchecked(object sender, EventArgs e)
    {
        Model.CountPendingOrders = false;
        
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

    private void IgnoreOrdersInOtherSymbolsCheckBoxChecked(object sender, EventArgs e)
    {
        Model.IgnoreOrdersInOtherSymbols = true;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void IgnoreOrdersInOtherSymbolsCheckBoxUnchecked(object sender, EventArgs e)
    {
        Model.IgnoreOrdersInOtherSymbols = false;
        
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void RiskViewAreaClicked(ButtonClickEventArgs obj)
    {
        Print("Risk View Area Clicked");
    }
}