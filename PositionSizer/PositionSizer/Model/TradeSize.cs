using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public enum LastRiskValueChanged
{
    RiskPercentage,
    RiskCurrency,
    LotSize
}

public class TradeSize
{
    public bool IsLotsValueInvalid { get; set; }
    public double Lots { get; set; }
    public LastRiskValueChanged LastRiskValueChanged { get; set; }
    public double Volume => Symbol.QuantityToVolumeInUnits(Lots);
    public double RiskPercentage { get; set; }
    public double RiskPercentageResult { get; set; }
    public double RewardPercentageResult => RiskPercentageResult * RewardRiskRatioResult;
    public double RiskInCurrency { get; set; }
    public double RewardRiskRatio { get; set; }
    public double RiskInCurrencyResult { get; set; }
    public double RewardRiskRatioResult { get; set; }
    public double RewardInCurrency { get; set; }
    public double RewardCurrencyResult { get; set; }
    private Symbol Symbol { get; set; }

    public TradeSize(Symbol symbol)
    {
        Symbol = symbol;
    }

    public TradeSize()
    {
        
    }
    
    public void SetSymbol(Symbol symbol)
    {
        Symbol = symbol;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Size: {Lots} lots")
            .AppendLine($"Volume: {Volume}")
            .AppendLine($"Risk: {RiskInCurrency}")
            .AppendLine($"Reward: {RewardInCurrency}")
            .AppendLine($"Risk %: {RiskPercentage}")
            .AppendLine($"Reward/Risk: {RewardRiskRatioResult}");
        
        return sb.ToString();
    }
}