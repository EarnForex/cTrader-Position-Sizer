using System;

namespace PositionSizer.XTextBoxControl.ByTypes;

public sealed class XTextBoxString : XTextBox<string>
{
    public XTextBoxString(string defaultValue) : base(defaultValue)
    {
        UpdateTextOfControls(Value);
    }

    public override void TryValidateText()
    {
        if (!IsBeingEdited)
            return;

        BackgroundColor = DefaultBackgroundColor;
        Value = TextBox.Text;
        OnTextUpdatedAndValid(new TextUpdatedEventArgs<string>(Value));
    }

    public override string Value
    {
        get => ControlValue.Value;
        set
        {
            if (string.Equals(ControlValue.Value, value, StringComparison.InvariantCulture))
                return;

            ControlValue.Value = value;
        }
    }
}