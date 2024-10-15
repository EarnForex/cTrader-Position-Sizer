using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class TakeProfitValueChangedEventArgs : EventArgs
{
    public double Value { get; set; }
    public int Id { get; set; }
    
    public TakeProfitValueChangedEventArgs(double value, int id)
    {
        Value = value;
        Id = id;
    }
}

public class StopLimitPriceChangedEventArgs : EventArgs
{
    public double StopLimitPrice { get; }

    public StopLimitPriceChangedEventArgs(double stopLimitPrice)
    {
        StopLimitPrice = stopLimitPrice;
    }
}

public class TakeProfitLevelRemovedEventArgs : EventArgs
{
    public TakeProfitLevelRemovedEventArgs()
    {
        
    }
}

public class TakeProfitLevelAddedEventArgs : EventArgs
{
    public TakeProfitLevelAddedEventArgs()
    {
        
    }
}

public class AtrTakeProfitSaChangedEventArgs : EventArgs
{
    public bool IsChecked { get; }
    
    public AtrTakeProfitSaChangedEventArgs(bool isChecked)
    {
        IsChecked = isChecked;
    }
}

public class AtrStopLossSaChangedEventArgs : EventArgs
{
    public bool IsChecked { get; }

    public AtrStopLossSaChangedEventArgs(bool isChecked)
    {
        IsChecked = isChecked;
    }
}

public class AtrTimeFrameChangedEventArgs : EventArgs
{
    public string OldTimeFrameName { get; }
    
    public AtrTimeFrameChangedEventArgs(string oldTimeFrameName)
    {
        OldTimeFrameName = oldTimeFrameName;
    }
}

public class AtrTakeProfitMultiplierChangedEventArgs : EventArgs
{
    public double TakeProfitMultiplier { get; }

    public AtrTakeProfitMultiplierChangedEventArgs(double takeProfitMultiplier)
    {
        TakeProfitMultiplier = takeProfitMultiplier;
    }
}

public class AtrStopLossMultiplierChangedEventArgs : EventArgs
{
    public double StopLossMultiplier { get; }

    public AtrStopLossMultiplierChangedEventArgs(double stopLossMultiplier)
    {
        StopLossMultiplier = stopLossMultiplier;
    }
}

public class AtrPeriodChangedEventArgs : EventArgs
{
    public int Period { get; }

    public AtrPeriodChangedEventArgs(int period)
    {
        Period = period;
    }
}

public class AccountValueChangedEventArgs : EventArgs
{
    public double AccountValue { get; }

    public AccountValueChangedEventArgs(double accountValue)
    {
        AccountValue = accountValue;
    }
}

public class PositionSizeValueChangedEventArgs : EventArgs
{
    public double PositionSize { get; }

    public PositionSizeValueChangedEventArgs(double positionSize)
    {
        PositionSize = positionSize;
    }
}

public class RewardRiskValueChangedEventArgs : EventArgs
{
    public double RewardRisk { get; }

    public RewardRiskValueChangedEventArgs(double rewardRisk)
    {
        RewardRisk = rewardRisk;
    }
}

public class RewardCashValueChangedEventArgs : EventArgs
{
    public double RewardCash { get; }

    public RewardCashValueChangedEventArgs(double rewardCash)
    {
        RewardCash = rewardCash;
    }
}

public class RiskCashValueChangedEventArgs : EventArgs
{
    public double RiskCash { get; }

    public RiskCashValueChangedEventArgs(double riskCash)
    {
        RiskCash = riskCash;
    }
}

public class RiskPercentageChangedEventArgs : EventArgs
{
    public double RiskPercentage { get; }

    public RiskPercentageChangedEventArgs(double riskPercentage)
    {
        RiskPercentage = riskPercentage;
    }
}

public class AccountValueTypeChangedEventArgs : EventArgs
{
    public AccountSizeMode AccountSizeMode { get; }
    
    public AccountValueTypeChangedEventArgs(AccountSizeMode accountSizeMode)
    {
        AccountSizeMode = accountSizeMode;
    }
}

public class HideLinesClickedEventArgs : EventArgs
{
    public bool IsHidden { get; }
    
    public HideLinesClickedEventArgs(bool isHidden)
    {
        IsHidden = isHidden;
    }
}

public class OrderTypeChangedEventArgs : EventArgs
{
    public OrderType OrderType { get; }

    public OrderTypeChangedEventArgs(OrderType orderType)
    {
        OrderType = orderType;
    }
}

public class TakeProfitPriceChangedEventArgs : EventArgs
{
    public int TakeProfitId { get; }
    public double TakeProfitValue { get; }
    
    public TakeProfitPriceChangedEventArgs(int takeProfitId, double takeProfitValue)
    {
        TakeProfitId = takeProfitId;
        TakeProfitValue = takeProfitValue;
    }
}

public class StopLossPriceChangedEventArgs : EventArgs
{
    public double NewValue { get; }

    public StopLossPriceChangedEventArgs(double newValue)
    {
        NewValue = newValue;
    }
}

public class TargetPriceChangedEventArgs : EventArgs
{
    public double TargetPrice { get; }

    public TargetPriceChangedEventArgs(double targetPrice)
    {
        TargetPrice = targetPrice;
    }
}

public class TradeTypeChangedEventArgs : EventArgs
{
    public TradeType TradeType { get; }

    public TradeTypeChangedEventArgs(TradeType tradeType)
    {
        TradeType = tradeType;
    }
}