using cAlgo.API;
using cAlgo.API.Internals;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private void MarginViewOnLeverageDisplayChanged(object sender, LeverageDisplayChangedEventArgs e)
    {
        Model.CustomLeverage = e.Leverage;

        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView.MarginView.Update(Model);
    }

    private void MarginViewOnMarginUtilizationBaseChanged(object sender, MarginUtilizationBaseChangedEventArgs e)
    {
        Model.MarginUtilizationBase = e.Base;

        Model.UpdateMarginUtilization();
        SetupWindowView.Update(Model);
    }

    private void MarginViewOnMubStartingBalanceChanged(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        Model.MubStartingBalance = e.Value;

        Model.UpdateMarginUtilization();
        SetupWindowView.Update(Model);
    }

    private void MarginViewAreaClicked(ButtonClickEventArgs obj)
    {
        Print($"Margin View Area Clicked");
    }
}
