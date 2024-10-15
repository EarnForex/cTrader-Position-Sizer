namespace PositionSizer.XTextBoxControl.ByTypes;

public sealed class XTextBoxInt : XTextBox<int>
{
    public XTextBoxInt(int defaultValue) : base(defaultValue)
    {
        UpdateTextOfControls(Value);
    }

    public override int Value
    {
        get => ControlValue.Value;
        set
        {
            if (ControlValue.Value.Equals(value))
                return;

            ControlValue.Value = value;
        }
    }

    public override void TryValidateText()
    {
        if (!IsBeingEdited)
            return;

        if (int.TryParse(TextBox.Text, out var value) &&
            (value >= 0 && ValidationAllowZero ||
             value > 0 && !ValidationAllowZero))
        {
            BackgroundColor = DefaultBackgroundColor;
            Value = value;
            OnTextUpdatedAndValid(new TextUpdatedEventArgs<int>(value));
        }
        else
        {
            BackgroundColor = InvalidColor;
        }
    }
}