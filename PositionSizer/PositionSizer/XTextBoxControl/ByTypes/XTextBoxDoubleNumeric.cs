using System;
using cAlgo.API;
using PositionSizer.XTextBoxControl.ControlValue;

namespace PositionSizer.XTextBoxControl.ByTypes;

public class XTextBoxDoubleNumeric : XTextBoxNumeric<double>
{
    private int _digits;

    public int Digits
    {
        get => _digits;
        set
        {
            if (_digits == value)
                return;

            _digits = value;
            DigitsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler DigitsChanged;
    public event EventHandler OnAfterClick;

    public XTextBoxDoubleNumeric(double defaultValue, int digits, double changeByFactor) : base(defaultValue, changeByFactor)
    {
        Digits = digits;

        if (digits <= 0)
            // ReSharper disable once LocalizableElement
            throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be greater than zero");

        UpdateTextOfControls(Value);

        DigitsChanged += (sender, args) => UpdateTextOfControls(Value);
        ChangeByFactorChanged += (sender, args) => UpdateTextOfControls(Value);
    }

    public sealed override double Value
    {
        get => ControlValue.Value;
        set
        {
            if (Math.Abs(ControlValue.Value - value) < double.Epsilon)
                return;

            ControlValue.Value = value;
        }
    }

    public override void TryValidateText()
    {
        if (!IsBeingEdited)
            return;

        if (double.TryParse(TextBox.Text, out var value) &&
            (value >= 0 && ValidationAllowZero ||
             value > 0 && !ValidationAllowZero))
        {
            BackgroundColor = DefaultBackgroundColor;
            //ControlValue.SetValueWithoutTriggeringEvent(value);
            Value = value;
            OnTextUpdatedAndValid(new TextUpdatedEventArgs<double>(value));
        }
        else
        {
            BackgroundColor = InvalidColor;
        }
    }

    protected override void ControlValueOnValueUpdated(object sender, ControlValueUpdatedEventArgs<double> e)
    {
        UpdateTextOfControls(e.Value);
    }

    protected override void OnTextUpdatedAndValid(object sender, TextUpdatedEventArgs<double> e)
    {
        TextBox.Text = e.Value.ToString($"F{Digits}");
        Button.Text = TextBox.Text;
        Button.IsVisible = true;
        if (UseEditButton)
            if (EditButton != null)
                EditButton.IsVisible = false;
        TextBox.IsVisible = false;
    }

    protected sealed override void UpdateTextOfControls(double value)
    {
        var text = value.ToString($"F{Digits}");

        Button.Text = text;

        if (!TextBox.IsVisible)
            TextBox.Text = text;
    }

    protected override void OnIncrementButtonOnClick(ButtonClickEventArgs args)
    {
        if (IsBeingEdited)
            return;

        OnIncrementButtonClicked();

        Value += ChangeByFactor;

        OnAfterClick?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnDecrementButtonOnClick(ButtonClickEventArgs obj)
    {
        if (IsBeingEdited)
            return;

        OnDecrementButtonClicked();

        Value -= ChangeByFactor;

        OnAfterClick?.Invoke(this, EventArgs.Empty);
    }
}