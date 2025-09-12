using System;
using cAlgo.API;
using PositionSizer.XTextBoxControl.ByTypes;
using PositionSizer.XTextBoxControl.ControlValue;

namespace cAlgo.Robots;

public class TakeProfitRowView
{
    public int Id { get; set; }
    public Button RemoveButton { get; set; }
    public ControlBase TakeProfitNTextBlock { get; set; }
    public XTextBoxDoubleNumeric TakeProfitTextBox { get; set; }
    public event EventHandler<TakeProfitValueChangedEventArgs> TakeProfitValueChanged;
    public event EventHandler<TakeProfitControlClickedEventArgs> TakeProfitControlClicked;

    public TakeProfitRowView(
        int id, 
        Button removeButton, 
        ControlBase takeProfitNTextBlock, 
        XTextBoxDoubleNumeric takeProfitTextBox)
    {
        Id = id;
        RemoveButton = removeButton;
        TakeProfitNTextBlock = takeProfitNTextBlock;
        TakeProfitTextBox = takeProfitTextBox;
        
        TakeProfitTextBox.ValueUpdated += OnTakeProfitTextBoxValueUpdated;
        TakeProfitTextBox.ControlClicked += TakeProfitTextBoxOnControlClicked;
    }

    private void TakeProfitTextBoxOnControlClicked(object sender, EventArgs e)
    {
        TakeProfitControlClicked?.Invoke(this, new TakeProfitControlClickedEventArgs(Id));
    }

    private void OnTakeProfitTextBoxValueUpdated(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        TakeProfitValueChanged?.Invoke(this, new TakeProfitValueChangedEventArgs(e.Value, Id));
    }
    
    //create destructor to remove event handler
    ~TakeProfitRowView()
    {
        TakeProfitTextBox.ValueUpdated -= OnTakeProfitTextBoxValueUpdated;
        TakeProfitTextBox.ControlClicked -= TakeProfitTextBoxOnControlClicked;
    }
}

public class TakeProfitControlClickedEventArgs
{
    public int Id { get; set; }

    public TakeProfitControlClickedEventArgs(int id)
    {
        Id = id;
    }
}