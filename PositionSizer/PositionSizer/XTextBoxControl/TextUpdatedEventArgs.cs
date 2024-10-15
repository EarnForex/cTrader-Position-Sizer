using System;

namespace PositionSizer.XTextBoxControl;

public class TextUpdatedEventArgs<T> : EventArgs
{
    public T Value { get; }

    public TextUpdatedEventArgs(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Value cannot be null");

        Value = value;
    }
}