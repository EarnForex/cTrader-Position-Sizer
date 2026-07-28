using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ControlValue;

namespace PositionSizer.XTextBoxControl.ByTypes;

public class XTextBoxDouble : XTextBox<double>
{
    private readonly int _digits;
    private readonly double _tolerance;

    public bool UseAdaptiveDecimalPlaces { get; set; }

    public XTextBoxDouble(double defaultValue, int digits) : base(defaultValue)
    {
        _digits = digits;
        _tolerance = Math.Pow(10, -digits);

        if (digits < 0)
            throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be greater or equal zero");

        UpdateTextOfControls(Value);
    }

    public override double Value
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

    public void SyncDisplayFromValue()
    {
        UpdateTextOfControls(ControlValue.Value);
    }

    public new void SetValueWithoutTriggeringEvent(double value)
    {
        ControlValue.SetValueWithoutTriggeringEvent(value);
        UpdateTextOfControls(value);
    }

    private int GetDisplayDecimals(double value)
    {
        if (!UseAdaptiveDecimalPlaces)
            return _digits;

        if (value == 0)
            return 0;

        var decimals = BotTools.CountDecimals(value);
        return Math.Max(decimals, _digits);
    }

    protected override void OnTextUpdatedAndValid(object sender, TextUpdatedEventArgs<double> e)
    {
        TextBox.Text = e.Value.ToString($"F{GetDisplayDecimals(e.Value)}");
        Button.Text = TextBox.Text;
        Button.IsVisible = true;
        if (UseEditButton)
            if (EditButton != null)
                EditButton.IsVisible = false;
        TextBox.IsVisible = false;
    }

    protected sealed override void UpdateTextOfControls(double value)
    {
        var text = value.ToString($"F{GetDisplayDecimals(value)}");

        Button.Text = text;

        if (!TextBox.IsVisible)
            TextBox.Text = text;
    }
}