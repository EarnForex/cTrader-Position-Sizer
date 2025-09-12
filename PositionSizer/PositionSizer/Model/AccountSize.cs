namespace cAlgo.Robots;

public class AccountSize
{
    public AccountSizeMode Mode { get; set; }
    public double Value { get; set; }
    public bool HasAdditionalFunds { get; set; }
    public double AdditionalFunds { get; set; }
    public bool IsCustomBalance { get; set; }
    public double CustomBalance => Value;

    public override string ToString()
    {
        return $"Mode: {Mode}, Value: {Value}, HasAdditionalFunds: {HasAdditionalFunds}, AdditionalFunds: {AdditionalFunds}, IsCustomBalance: {IsCustomBalance}, CustomBalance: {CustomBalance}";
    }
}