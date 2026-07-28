using System;
using cAlgo.API;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;

namespace cAlgo.Robots;

/// <summary>
/// Owns the "SA" (Spread Adjustment) toggle button and drives the unified dual-row display on an
/// <see cref="XTextBoxDoubleNumeric"/> (editable row + read-only spread-adjusted row with aligned +/- buttons).
/// Independent of ATR: the same component is used in both ATR and non-ATR modes.
/// </summary>
public class SpreadAdjustFieldView
{
    private readonly CustomStyle _customStyle;
    private readonly XTextBoxDoubleNumeric _editable;
    private bool _isEnabled;

    public Button SaButton { get; }

    /// <summary>Raised only on user interaction (SA button click), carrying the new enabled state.</summary>
    public event EventHandler<bool> EnabledChanged;

    public SpreadAdjustFieldView(Button saButton, XTextBoxDoubleNumeric editable, CustomStyle customStyle)
    {
        SaButton = saButton;
        _editable = editable;
        _customStyle = customStyle;

        _editable.SpreadAdjustDisplayForegroundColor = Color.Green;

        SaButton.Click += _ => SetEnabled(!_isEnabled, raiseEvent: true);
        ApplyButtonStyle();
    }

    public bool IsEnabled => _isEnabled;

    /// <summary>Reflects model SA flag on the button without raising <see cref="EnabledChanged"/> (used on refresh).</summary>
    public void SyncEnabled(bool enabled)
    {
        _isEnabled = enabled;
        ApplyButtonStyle();
    }

    /// <summary>Shows or hides the read-only spread-adjusted row (may differ from <see cref="SyncEnabled"/>).</summary>
    public void SyncAdjustedRowVisible(bool visible) => _editable.SpreadAdjustDisplayVisible = visible;

    /// <summary>Sets the read-only spread-adjusted row, matching the editable field's precision.</summary>
    public void SetAdjustedValue(double value, int digits, double changeByFactor)
    {
        _editable.Digits = digits;
        _editable.ChangeByFactor = changeByFactor;
        _editable.SetSpreadAdjustDisplayValue(value);
    }

    private void SetEnabled(bool enabled, bool raiseEvent)
    {
        _isEnabled = enabled;
        ApplyButtonStyle();
        //Adjusted row visibility is set by MainView.UpdateSpreadAdjustFields after each model refresh.

        if (raiseEvent)
            EnabledChanged?.Invoke(this, enabled);
    }

    private void ApplyButtonStyle()
    {
        SaButton.Style = _isEnabled ? _customStyle.SpreadAdjustOnButtonStyle : _customStyle.ButtonStyle;
    }
}
