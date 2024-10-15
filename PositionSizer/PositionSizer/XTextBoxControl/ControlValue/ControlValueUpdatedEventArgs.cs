using System;

namespace PositionSizer.XTextBoxControl.ControlValue;

public class ControlValueUpdatedEventArgs<T> : EventArgs
{
    public T Value { get; private set; }

    public ControlValueUpdatedEventArgs(T value)
    {
        Value = value;
    }
}