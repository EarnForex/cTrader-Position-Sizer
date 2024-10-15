using cAlgo.API;
using cAlgo.Robots.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PositionSizer.XTextBoxControl.ControlValue;

namespace PositionSizer.XTextBoxControl;

public abstract class XTextBox<T> : CustomControl, IControlValue<T>
{
    protected readonly ControlValue<T> ControlValue;
    protected Button Button { get; set; }
    protected TextBox TextBox { get; set; }
    protected Button EditButton { get; set; }
    protected bool UseEditButton { get; private set; } = false;
    protected CustomStyle CustomStyle { get; private set; }

    private bool _isReadOnly;

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly == value)
                return;

            _isReadOnly = value;
            ReadOnlyPropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #region TextAlignmentProperty

    private TextAlignment _textAlignment;
    public TextAlignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            // if (_textAlignment == value)
            //     return;

            _textAlignment = value;
            TextAlignmentChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region ForeGroundColorProperty

    private Color _foregroundColor;


    public Color ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            if (_foregroundColor == value)
                return;

            _foregroundColor = value;
            ForegroundColorUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region BackgroundColorProperty

    protected Color EditingColor { get; }
    protected Color InvalidColor { get; }
    protected Color DefaultBackgroundColor { get; }

    private Color _backgroundColor;


    protected Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor == value)
                return;

            _backgroundColor = value;
            BackgroundColorUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion


    private double _width;
    public new double Width
    {
        get => _width;
        set
        {
            Button.Width = value;
            TextBox.Width = value;
            _width = value;
        }
    }

    public event EventHandler TextUpdatedByEnter;
    public event EventHandler BackgroundColorUpdated;
    public event EventHandler<TextUpdatedEventArgs<T>> TextUpdatedAndValid;
    public event EventHandler ReadOnlyPropertyChanged;
    public event EventHandler ForegroundColorUpdated;
    public event EventHandler TextAlignmentChanged;
    public event EventHandler ControlClicked;

    private const int ControlHeight = 26;

    protected XTextBox(T defaultValue)
    {
        ControlValue = new ControlValue<T>(defaultValue);

        EditingColor = Color.DarkOrange;
        InvalidColor = Color.Red;
        DefaultBackgroundColor = Color.White;
        BackgroundColor = Color.White;

        _width = 100;

        Button = new Button
        {
            Width = _width,
            Height = ControlHeight,
            BackgroundColor = Color.White,
            ForegroundColor = Color.Black,
            BorderColor = Color.FromHex("FFB2C3CF"),
            // BorderColor = Color.Red,
            BorderThickness = new Thickness(1, 1, 1, 1),
            CornerRadius = 0,
            IsVisible = true,
            HorizontalContentAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(1, 1, 1, 1),
            Padding = 5
        };

        TextBox = new TextBox
        {
            Width = _width,
            Height = ControlHeight,
            BackgroundColor = Color.White,
            ForegroundColor = Color.Black,
            BorderColor = Color.FromHex("FFB2C3CF"),
            BorderThickness = new Thickness(1, 1, 1, 1),
            IsVisible = false,
            AcceptsReturn = true,
            MaxLines = 1,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(1, 1, 1, 1),
            Padding = 5
        };

        //https://github.com/Waxavi/PositionSizer/issues/1
        //This will be removed because it causes errors when using compilation with embedded Resources
        //it's no longer in use and client was the installation and use to be as simple as possible
        // var img = new Image
        // {
        //     Source = Resources.checkMark,
        //     Width = 20,
        //     Height = 20,
        //     Margin = 0,
        // };

        if (UseEditButton)
            EditButton = new Button
            {
                BackgroundColor = Color.CornflowerBlue,
                Text = "✓",
                // Content = img,
                Width = 20,
                Height = 20,
                BorderColor = Color.Black,
                BorderThickness = 1,
                // CornerRadius = 50,
                CornerRadius = 0,
                IsVisible = false,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 20, 0),
                Padding = 0
            };

        AddChild(Button);
        AddChild(TextBox);

        if (UseEditButton)
            AddChild(EditButton);

        if (UseEditButton && EditButton != null)
            EditButton.Click += EditButtonOnClick;

        Button.Click += OnButtonOnClick;
        TextBox.TextChanged += TextBoxOnTextChanged;
        ControlValue.ValueUpdated += ControlValueOnValueUpdated;
        TextUpdatedByEnter += OnTextUpdatedByEnter;
        TextUpdatedAndValid += OnTextUpdatedAndValid;
        BackgroundColorUpdated += OnBackgroundColorUpdated;
        ForegroundColorUpdated += OnForegroundColorUpdated;
        ReadOnlyPropertyChanged += OnReadOnlyPropertyChanged;
        TextAlignmentChanged += OnTextAlignmentChanged;
    }

    private void OnTextAlignmentChanged(object sender, EventArgs e)
    {
        TextBox.TextAlignment = TextAlignment;
        switch (TextAlignment)
        {
            case TextAlignment.Left:
                Button.HorizontalContentAlignment = HorizontalAlignment.Left;
                break;
            case TextAlignment.Right:
                Button.HorizontalContentAlignment = HorizontalAlignment.Right;
                break;
            case TextAlignment.Center:
                Button.HorizontalContentAlignment = HorizontalAlignment.Center;
                break;
            case TextAlignment.Justify:
                Button.HorizontalAlignment = HorizontalAlignment.Stretch;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnReadOnlyPropertyChanged(object sender, EventArgs e)
    {
        TextBox.IsReadOnly = IsReadOnly;
        TextBox.IsReadOnlyCaretVisible = IsReadOnly;

        if (CustomStyle == null)
            throw new NullReferenceException("CustomStyle is not set");

        if (IsReadOnly)
        {
            TextBox.Style = CustomStyle.ReadOnlyTextBoxStyle;
            Button.Style = CustomStyle.ReadOnlyTextBoxStyle;
        }
        else
        {
            TextBox.Style = CustomStyle.TextBoxStyle;
            Button.Style = CustomStyle.TextBoxStyle;
        }
    }

    private void EditButtonOnClick(ButtonClickEventArgs obj)
    {
        TryValidateText();
    }

    private void OnBackgroundColorUpdated(object sender, EventArgs e)
    {
        Button.BackgroundColor = BackgroundColor;
        TextBox.BackgroundColor = BackgroundColor;
    }

    private void OnForegroundColorUpdated(object sender, EventArgs e)
    {
        Button.ForegroundColor = ForegroundColor;
        TextBox.ForegroundColor = ForegroundColor;
    }

    protected virtual void OnTextUpdatedAndValid(object sender, TextUpdatedEventArgs<T> e)
    {
        Button.Text = TextBox.Text;
        Button.IsVisible = true;
        TextBox.IsVisible = false;

        if (!UseEditButton)
            return;

        if (EditButton != null)
            EditButton.IsVisible = false;
    }

    public bool ValidationAllowZero { get; set; } = true;

    public abstract void TryValidateText();

    public virtual void SetCustomStyle(CustomStyle style)
    {
        CustomStyle = style;

        Button.ResetProperty(ControlProperty.BackgroundColor);
        Button.ResetProperty(ControlProperty.ForegroundColor);
        Button.ResetProperty(ControlProperty.BorderColor);

        TextBox.ResetProperty(ControlProperty.BackgroundColor);
        TextBox.ResetProperty(ControlProperty.ForegroundColor);
        TextBox.ResetProperty(ControlProperty.BorderColor);

        if (IsReadOnly)
        {
            Button.Style = style?.ReadOnlyTextBoxStyle;
            TextBox.Style = style?.ReadOnlyTextBoxStyle;
        }
        else
        {
            Button.Style = style?.TextBoxStyle;
            TextBox.Style = style?.TextBoxStyle;
        }
    }

    public virtual void ChangeWriteAreaWidth(double width)
    {
        Width = width;
        Button.Width = width - 2;
        TextBox.Width = width - 2;
    }

    private void OnTextUpdatedByEnter(object sender, EventArgs e)
        => TryValidateText();

    protected virtual void ControlValueOnValueUpdated(object sender, ControlValueUpdatedEventArgs<T> e)
    {
        var str = e.Value?.ToString();
        Button.Text = str;

        if (!TextBox.IsVisible)
            TextBox.Text = str;
    }

    protected virtual void OnTextUpdatedAndValid(TextUpdatedEventArgs<T> e)
    {
        TextUpdatedAndValid?.Invoke(this, e);
    }

    protected virtual void UpdateTextOfControls(T value)
    {
        Button.Text = value?.ToString();

        if (!TextBox.IsVisible)
            TextBox.Text = value?.ToString();
    }

    private void TextBoxOnTextChanged(TextChangedEventArgs obj)
    {
        if (TextBox.BackgroundColor != EditingColor /*&& _lastKnownText != RemoveAllWhiteSpaces(TextBox.Text)*/)
            BackgroundColor = EditingColor;

        if (!EnterKeyPressed)
            return;

        TextBox.Text = RemoveAllWhiteSpaces(TextBox.Text);
        TextUpdatedByEnter?.Invoke(this, EventArgs.Empty);
    }

    private void OnButtonOnClick(ButtonClickEventArgs obj)
    {
        if (IsReadOnly)
            return;

        if (!IsBeingEdited)
            ControlClicked?.Invoke(this, EventArgs.Empty);

        Button.IsVisible = false;
        TextBox.IsVisible = true;

        if (!UseEditButton)
            return;

        if (EditButton != null)
            EditButton.IsVisible = true;
    }

    private bool EnterKeyPressed => TextBox.Text.Contains('\n');
    protected bool IsBeingEdited => TextBox.IsVisible;

    private string RemoveAllWhiteSpaces(string text) =>
        new(text.Where(c => !char.IsWhiteSpace(c)).ToArray());

    #region ControlValue

    public event EventHandler<ControlValueUpdatedEventArgs<T>> ValueUpdated
    {
        add => ControlValue.ValueUpdated += value;
        remove => ControlValue.ValueUpdated -= value;
    }

    public void SetValueWithoutTriggeringEvent(T value)
    {
        ControlValue.SetValueWithoutTriggeringEvent(value);
        UpdateTextOfControls(value);
    }

    public virtual T Value
    {
        get => ControlValue.Value;
        set => ControlValue.Value = value;
    }

    public bool IsInteger => ControlValue.IsInteger;
    public bool IsDouble => ControlValue.IsDouble;
    public bool IsString => ControlValue.IsString;

    #endregion
}