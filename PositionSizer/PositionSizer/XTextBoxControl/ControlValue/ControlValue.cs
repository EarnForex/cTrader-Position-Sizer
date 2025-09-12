#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PositionSizer.XTextBoxControl.ControlValue;

public sealed class ControlValue<T> : IControlValue<T>
{
    public event EventHandler<ControlValueUpdatedEventArgs<T>>? ValueUpdated;

    public ControlValue(T defaultValue)
    {
        _value = defaultValue;
        Value = defaultValue;
    }

    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            ValueUpdated?.Invoke(this, new ControlValueUpdatedEventArgs<T>(_value));
        }
    }

    public void SetValueWithoutTriggeringEvent(T value)
    {
        _value = value;
    }

    public bool IsInteger => typeof(T) == typeof(int);
    public bool IsDouble => typeof(T) == typeof(double);
    public bool IsString => typeof(T) == typeof(string);
}