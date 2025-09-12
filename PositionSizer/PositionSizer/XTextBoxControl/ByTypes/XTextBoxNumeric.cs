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

        AddChild(IncrementButton);
        AddChild(DecrementButton);

        IncrementButton.Click += OnIncrementButtonOnClick;
        DecrementButton.Click += OnDecrementButtonOnClick;
        ReadOnlyPropertyChanged += OnReadOnlyPropertyChanged;
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

        base.SetCustomStyle(style);
    }

    private void OnReadOnlyPropertyChanged(object sender, EventArgs e)
    {
        if (CustomStyle == null)
            throw new NullReferenceException("CustomStyle is null");

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

    protected abstract void OnIncrementButtonOnClick(ButtonClickEventArgs args);
    protected abstract void OnDecrementButtonOnClick(ButtonClickEventArgs obj);

    public override void ChangeWriteAreaWidth(double width)
    {
        Width = width;
        Button.Width = width - IncrementButton.Width;
        TextBox.Width = width - IncrementButton.Width;
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