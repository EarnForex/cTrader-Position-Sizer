using System.Text;

namespace cAlgo.Robots;

public class Portfolio
{
    /// <summary>
    /// It's the Risk in Currency Units from the Positions and Pending Orders Open and maybe even new potential
    /// orders being set, depending on the settings. 
    /// From EarnForex:
    /// If it's Current Portfolio: Shows the risk in currency units without the position that is
    /// currently being calculated by this expert advisor.
    /// If it's Potential Portfolio: shows the risk in currency units as if you have
    /// already opened a position that is currently calculated by this expert advisor.
    /// </summary>
    public double RiskCurrency { get; set; }
    public double RiskPercentage { get; set; }
    public double Lots { get; set; }
    
    public double RewardCurrency { get; set; }
    public double RewardPercentage { get; set; }
    public double RewardRiskRatio { get; set; }
    
    //use tostring with stringBuilder, new line for each property
    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Risk Currency: {RiskCurrency}");
        sb.AppendLine($"Risk Percentage: {RiskPercentage}");
        sb.AppendLine($"Lots: {Lots}");
        sb.AppendLine($"Reward Currency: {RewardCurrency}");
        sb.AppendLine($"Reward Percentage: {RewardPercentage}");
        sb.AppendLine($"Reward Risk Ratio: {RewardRiskRatio}");
        
        return sb.ToString();
    }
}