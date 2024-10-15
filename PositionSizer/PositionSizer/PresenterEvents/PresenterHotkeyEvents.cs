using System;

namespace cAlgo.Robots;

public partial class PositionSizer
{
    private void SetStopLossWhereMouseIs()
    {
        Print($"Setting SL to {_lastKnownMouseYPosition}");
        //the pips for the sl change
        Model.ChangeStopLossPrice(_lastKnownMouseYPosition);
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
                
        SetupWindowView.Update(Model);
    }
    
    private void SetTakeProfitWhereMouseIs()
    {
        Print($"Setting TP to {_lastKnownMouseYPosition}");
        
        Model.UpdateTakeProfitPrice(0, _lastKnownMouseYPosition);
        
        Model.UpdateTradeSizeValues(InputRoundingPositionSizeAndPotentialReward);
                
        SetupWindowView.Update(Model);
    }
    
    private void SetEntryWhereMouseIs()
    {
        if (Model.OrderType == OrderType.Instant)
            return;
        
        Model.UpdateEntryPrice(_lastKnownMouseYPosition, EntryPriceUpdateReason.SetEntryWhereMouseIs);
    }
    
    private void SwitchStopLossBetweenPipsAndLevel()
    {
        Model.StopLoss.Mode = Model.StopLoss.Mode == TargetMode.Pips
            ? TargetMode.Price
            : TargetMode.Pips;
        
        SetupWindowView.Update(Model);
    }
    
    private void SwitchTakeProfitBetweenPipsAndLevel()
    {
        Model.TakeProfits.Mode = Model.TakeProfits.Mode == TargetMode.Pips
            ? TargetMode.Price
            : TargetMode.Pips;
        
        SetupWindowView.Update(Model);
    }
}