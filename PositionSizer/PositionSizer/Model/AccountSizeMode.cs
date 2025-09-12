using System.ComponentModel;

namespace cAlgo.Robots;

public enum AccountSizeMode
{
    //Uses the Equity automatically
    [Description("Equity")]
    Equity,
    //Uses a Custom Balance
    [Description("Balance")]
    Balance,
    //Account balance less the current portfolio risk as calculated on the Risk tab.
    [Description("Balance - CPR")]
    BalanceCpr
}