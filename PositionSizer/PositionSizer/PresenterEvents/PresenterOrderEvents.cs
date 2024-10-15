using System;
using cAlgo.API;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private void PositionsOnOpened(PositionOpenedEventArgs obj)
    {
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
        
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView.MarginView.Update(Model);
    }

    private void PositionsOnClosed(PositionClosedEventArgs obj)
    {
        switch (Model.AccountSize.Mode)
        {
            case AccountSizeMode.Equity:
                Model.AccountSize.Value = Account.Equity;
                break;
            case AccountSizeMode.Balance:
                Model.AccountSize.Value = Account.Balance;
                break;
            case AccountSizeMode.BalanceCpr:
                var riskCurrency = Model.GetUpdatedRiskCurrency();
                
                if (double.IsNaN(riskCurrency))
                {
                    Model.AccountSize.Value = Account.Balance;
                    break;
                }

                Model.AccountSize.Value = Account.Balance - riskCurrency;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView.MarginView.Update(Model);
    }

    private void PositionsOnModified(PositionModifiedEventArgs obj)
    {
        switch (Model.AccountSize.Mode)
        {
            case AccountSizeMode.Equity:
                Model.AccountSize.Value = Account.Equity;
                break;
            case AccountSizeMode.Balance:
                Model.AccountSize.Value = Account.Balance;
                break;
            case AccountSizeMode.BalanceCpr:
                var riskCurrency = Model.GetUpdatedRiskCurrency();

                if (double.IsNaN(riskCurrency))
                {
                    Model.AccountSize.Value = Account.Balance;
                    break;
                }
                
                Model.AccountSize.Value = Account.Balance - riskCurrency;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
        
        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView.MarginView.Update(Model);
    }

    private void PendingOrdersOnCreated(PendingOrderCreatedEventArgs obj)
    {
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void PendingOrdersOnModified(PendingOrderModifiedEventArgs obj)
    {
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void PendingOrdersOnDeleted(PendingOrderCancelledEventArgs obj)
    {
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }

    private void PendingOrdersOnFilled(PendingOrderFilledEventArgs obj)
    {
        Model.UpdateReadOnlyValues();
        SetupWindowView.RiskView.Update(Model);
    }
}