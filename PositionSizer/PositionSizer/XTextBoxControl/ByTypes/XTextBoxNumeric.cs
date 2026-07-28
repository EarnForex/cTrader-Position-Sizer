using System;
using cAlgo.API;
using cAlgo.Robots.Tools;

namespace PositionSizer.XTextBoxControl.ByTypes;

public abstract class XTextBoxNumeric<T> : XTextBox<T>
{
    private T _changeByFactor;

    public T ChangeByFactor
    {
        get => _changeByFactor;
        set
        {
            if (Equals(_changeByFactor, value))
                return;

            _changeByFactor = value;
            ChangeByFactorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Button IncrementButton { get; set; }
    public Button DecrementButton { get; set; }

    /// <summary>Read-only bottom row shown when <see cref="SpreadAdjustDisplayVisible"/> is true.</summary>
    public Button SpreadAdjustDisplayButton { get; }

    private bool _spreadAdjustDisplayVisible;
    private Color _spreadAdjustDisplayForegroundColor = Color.Green;

    /// <summary>When true, shows a second read-only row below the editable value; + aligns to the top row, - to the bottom.</summary>
    public bool SpreadAdjustDisplayVisible
    {
        get => _spreadAdjustDisplayVisible;
        set
        {
            if (_spreadAdjustDisplayVisible == value)
                return;

            _spreadAdjustDisplayVisible = value;
            ApplySpreadAdjustLayout();
        }
    }

    public Color SpreadAdjustDisplayForegroundColor
    {
        get => _spreadAdjustDisplayForegroundColor;
        set
        {
            if (_spreadAdjustDisplayForegroundColor == value)
                return;

            _spreadAdjustDisplayForegroundColor = value;
            SpreadAdjustDisplayButton.ForegroundColor = value;
        }
    }

    protected event EventHandler ChangeByFactorChanged;

    public event EventHandler IncrementButtonClicked;
    public event EventHandler DecrementButtonClicked;

    protected XTextBoxNumeric(T defaultValue, T changeByFactor) : base(defaultValue)
    {
        ChangeByFactor = changeByFactor;

        Button.Width = 87;
        Button.HorizontalAlignment = HorizontalAlignment.Left;
        Button.BorderThickness = new Thickness(1, 1, 0, 1);
        Button.Margin = new Thickness(1, 1, 0, 1);

        TextBox.Width = 87;
        TextBox.HorizontalAlignment = HorizontalAlignment.Left;
        TextBox.BorderThickness = new Thickness(1, 1, 0, 1);
        TextBox.Margin = new Thickness(1, 1, 0, 1);

        var width = 13;
        var height = 13;
        var fontSize = 10;

        IncrementButton = new Button
        {
            Text = "+",
            BackgroundColor = BackgroundColor,
            ForegroundColor = Color.Black,
            BorderColor = Color.FromHex("FFB2C3CF"),
            //BorderColor = Color.Red,
            BorderThickness = new Thickness(1, 1, 1, 1),
            FontSize = fontSize,
            Width = width,
            Height = height,
            Margin = new Thickness(0, 1, 1, 0),
            Padding = 0,
            CornerRadius = 0,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };

        DecrementButton = new Button
        {
            Text = "-",
            BackgroundColor = BackgroundColor,
            ForegroundColor = Color.Black,
            BorderColor = Color.FromHex("FFB2C3CF"),
            //BorderColor = Color.Red,
            BorderThickness = new Thickness(1, 0, 1, 1),
            FontSize = fontSize,
            Width = width,
            Height = height,
            Margin = new Thickness(0, 0, 1, 1),
            Padding = 0,
            CornerRadius = 0,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        SpreadAdjustDisplayButton = new Button
        {
            IsVisible = false,
            IsEnabled = false,
            FontSize = 9,
            HorizontalContentAlignment = HorizontalAlignment.Right,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            ForegroundColor = _spreadAdjustDisplayForegroundColor,
            BorderThickness = new Thickness(1, 0, 0, 1),
            CornerRadius = 0,
            Margin = new Thickness(1, 0, 0, 1),
            Padding = 5
        };

        AddChild(IncrementButton);
        AddChild(DecrementButton);
        AddChild(SpreadAdjustDisplayButton);

        IncrementButton.Click += OnIncrementButtonOnClick;
        DecrementButton.Click += OnDecrementButtonOnClick;
        ReadOnlyPropertyChanged += OnReadOnlyPropertyChanged;
    }

    public void SetSpreadAdjustDisplayText(string text)
    {
        SpreadAdjustDisplayButton.Text = text;
    }

    public override void SetCustomStyle(CustomStyle style)
    {
        IncrementButton.ResetProperty(ControlProperty.BackgroundColor);
        IncrementButton.ResetProperty(ControlProperty.ForegroundColor);
        IncrementButton.ResetProperty(ControlProperty.BorderColor);

        DecrementButton.ResetProperty(ControlProperty.BackgroundColor);
        DecrementButton.ResetProperty(ControlProperty.ForegroundColor);
        DecrementButton.ResetProperty(ControlProperty.BorderColor);

        IncrementButton.Style = style?.TextBoxStyle;
        DecrementButton.Style = style?.TextBoxStyle;

        SpreadAdjustDisplayButton.ResetProperty(ControlProperty.BackgroundColor);
        SpreadAdjustDisplayButton.ResetProperty(ControlProperty.ForegroundColor);
        SpreadAdjustDisplayButton.ResetProperty(ControlProperty.BorderColor);
        SpreadAdjustDisplayButton.Style = style?.ReadOnlyTextBoxStyle;
        SpreadAdjustDisplayButton.ForegroundColor = _spreadAdjustDisplayForegroundColor;

        base.SetCustomStyle(style);
        ApplySpreadAdjustLayout();
    }

    private void OnReadOnlyPropertyChanged(object sender, EventArgs e)
    {
        if (CustomStyle == null)
            throw new NullReferenceException("CustomStyle is null");

        if (_spreadAdjustDisplayVisible)
        {
            ApplySpreadAdjustLayout();
            return;
        }

        if (IsReadOnly)
        {
            Button.BorderThickness = new Thickness(1, 1, 1, 1);
            Button.Margin = new Thickness(1, 1, 1, 1);

            TextBox.BorderThickness = new Thickness(1, 1, 1, 1);
            TextBox.Margin = new Thickness(1, 1, 1, 1);
        }
        else
        {
            IncrementButton.Style = CustomStyle.TextBoxStyle;
            DecrementButton.Style = CustomStyle.TextBoxStyle;

            Button.BorderThickness = new Thickness(1, 1, 0, 1);
            Button.Margin = new Thickness(1, 1, 0, 1);

            TextBox.BorderThickness = new Thickness(1, 1, 0, 1);
            TextBox.Margin = new Thickness(1, 1, 0, 1);
        }

        IncrementButton.IsVisible = !IsReadOnly;
        DecrementButton.IsVisible = !IsReadOnly;
    }

    private void ApplySpreadAdjustLayout()
    {
        const int singleModeHeight = 26;
        const int spreadAdjustRowHeight = singleModeHeight / 2;
        const int buttonColWidth = 13;
        const int textWidth = 87;
        const int editableFontSize = 9;
        const int adjustedFontSize = 9;

        if (!_spreadAdjustDisplayVisible)
        {
            SpreadAdjustDisplayButton.IsVisible = false;
            Button.VerticalAlignment = VerticalAlignment.Center;
            TextBox.VerticalAlignment = VerticalAlignment.Center;
            Button.ResetProperty(ControlProperty.FontSize);
            TextBox.ResetProperty(ControlProperty.FontSize);
            Button.Padding = 5;
            TextBox.Padding = 5;
            //Let the control auto-size: a fixed 26px height clips the 1px vertical margins and hides top/bottom borders.
            ResetProperty(ControlProperty.Height);

            if (IsReadOnly)
            {
                Button.BorderThickness = new Thickness(1, 1, 1, 1);
                Button.Margin = new Thickness(1, 1, 1, 1);
                TextBox.BorderThickness = new Thickness(1, 1, 1, 1);
                TextBox.Margin = new Thickness(1, 1, 1, 1);
            }
            else
            {
                Button.Height = singleModeHeight;
                TextBox.Height = singleModeHeight;
                Button.Width = textWidth;
                TextBox.Width = textWidth;
                Button.BorderThickness = new Thickness(1, 1, 0, 1);
                Button.Margin = new Thickness(1, 1, 0, 1);
                TextBox.BorderThickness = new Thickness(1, 1, 0, 1);
                TextBox.Margin = new Thickness(1, 1, 0, 1);
                IncrementButton.Height = 13;
                DecrementButton.Height = 13;
                IncrementButton.ResetProperty(ControlProperty.FontSize);
                DecrementButton.ResetProperty(ControlProperty.FontSize);
                IncrementButton.IsVisible = true;
                DecrementButton.IsVisible = true;
            }

            return;
        }

        //Unified dual-row layout: editable on top (+), spread-adjusted read-only below (-).
        //Same total height as single-row mode: two 13px rows inside the standard 26px control envelope.
        SpreadAdjustDisplayButton.IsVisible = true;
        ResetProperty(ControlProperty.Height);

        Button.FontSize = editableFontSize;
        TextBox.FontSize = editableFontSize;
        SpreadAdjustDisplayButton.FontSize = adjustedFontSize;
        Button.Padding = new Thickness(3, 0, 3, 0);
        TextBox.Padding = new Thickness(3, 0, 3, 0);
        SpreadAdjustDisplayButton.Padding = new Thickness(3, 0, 3, 0);

        Button.Height = spreadAdjustRowHeight;
        TextBox.Height = spreadAdjustRowHeight;
        Button.Width = textWidth;
        TextBox.Width = textWidth;
        Button.VerticalAlignment = VerticalAlignment.Top;
        TextBox.VerticalAlignment = VerticalAlignment.Top;
        Button.HorizontalAlignment = HorizontalAlignment.Left;
        TextBox.HorizontalAlignment = HorizontalAlignment.Left;
        Button.BorderThickness = new Thickness(1, 1, 0, 0);
        Button.Margin = new Thickness(1, 1, 0, 0);
        TextBox.BorderThickness = new Thickness(1, 1, 0, 0);
        TextBox.Margin = new Thickness(1, 1, 0, 0);

        SpreadAdjustDisplayButton.Width = textWidth;
        SpreadAdjustDisplayButton.Height = spreadAdjustRowHeight;
        SpreadAdjustDisplayButton.HorizontalAlignment = HorizontalAlignment.Left;
        SpreadAdjustDisplayButton.VerticalAlignment = VerticalAlignment.Bottom;
        SpreadAdjustDisplayButton.Margin = new Thickness(1, 0, 0, 1);
        SpreadAdjustDisplayButton.BorderThickness = new Thickness(1, 0, 0, 1);

        IncrementButton.Width = buttonColWidth;
        IncrementButton.Height = spreadAdjustRowHeight;
        IncrementButton.FontSize = editableFontSize;
        DecrementButton.Width = buttonColWidth;
        DecrementButton.Height = spreadAdjustRowHeight;
        DecrementButton.FontSize = editableFontSize;
        IncrementButton.Margin = new Thickness(0, 1, 1, 0);
        DecrementButton.Margin = new Thickness(0, 0, 1, 1);

        IncrementButton.IsVisible = !IsReadOnly;
        DecrementButton.IsVisible = !IsReadOnly;
    }

    protected abstract void OnIncrementButtonOnClick(ButtonClickEventArgs args);
    protected abstract void OnDecrementButtonOnClick(ButtonClickEventArgs obj);

    public override void ChangeWriteAreaWidth(double width)
    {
        Width = width;
        var textWidth = width - IncrementButton.Width;
        Button.Width = textWidth;
        TextBox.Width = textWidth;

        if (_spreadAdjustDisplayVisible)
            SpreadAdjustDisplayButton.Width = textWidth;
    }

    protected virtual void OnIncrementButtonClicked()
    {
        IncrementButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnDecrementButtonClicked()
    {
        DecrementButtonClicked?.Invoke(this, EventArgs.Empty);
    }
}