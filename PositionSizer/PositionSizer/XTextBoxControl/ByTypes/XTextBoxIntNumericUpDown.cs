using cAlgo.API;

namespace PositionSizer.XTextBoxControl.ByTypes;

public sealed class XTextBoxIntNumericUpDown : XTextBoxNumeric<int>
{

    public XTextBoxIntNumericUpDown(int defaultValue, int changeByFactor) : base(defaultValue, changeByFactor)
    {
        UpdateTextOfControls(Value);
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

    protected override void OnIncrementButtonOnClick(ButtonClickEventArgs args)
    {
        if (IsBeingEdited)
            return;

        OnIncrementButtonClicked();

        Value += ChangeByFactor;
    }

    protected override void OnDecrementButtonOnClick(ButtonClickEventArgs obj)
    {
        if (IsBeingEdited)
            return;

        OnDecrementButtonClicked();

        Value -= ChangeByFactor;
    }
}