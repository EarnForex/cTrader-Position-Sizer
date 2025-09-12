using System;
using cAlgo.API;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public class TpDistributionRow : Grid
{
    public int Id { get; init; }
    public TextBlock NameTextBlock { get; init; }
    public XTextBoxDouble PriceTextBox { get; init; }
    public XTextBoxDouble PercentageTextBox { get; init; }
    
    public event EventHandler<TpDistributionPriceChangedEventArgs> PriceChanged;
    public event EventHandler<TpDistributionPercentageChangedEventArgs> PercentageChanged;

    public TpDistributionRow()
    {
        AddColumns(3);
    }

    public void PriceTextBoxOnTextUpdatedAndValid(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        PriceChanged?.Invoke(this, new TpDistributionPriceChangedEventArgs(Id, e.Value));
    }

    public void PercentageTextBoxOnTextUpdatedAndValid(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        PercentageChanged?.Invoke(this, new TpDistributionPercentageChangedEventArgs(Id, e.Value));
    }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {NameTextBlock.Text}, Price: {PriceTextBox.Value}, Percentage: {PercentageTextBox.Value}";
    }
}