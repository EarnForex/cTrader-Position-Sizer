using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private void MarginViewOnLeverageDisplayChanged(object sender, LeverageDisplayChangedEventArgs e)
    {
        Model.CustomLeverage = e.Leverage;

        Model.UpdateMarginValues(AssetConverter, InputRoundingPositionSizeAndPotentialReward);
        SetupWindowView.MarginView.Update(Model);
    }

    private void MarginViewAreaClicked(ButtonClickEventArgs obj)
    {
        Print($"Margin View Area Clicked");
    }
}