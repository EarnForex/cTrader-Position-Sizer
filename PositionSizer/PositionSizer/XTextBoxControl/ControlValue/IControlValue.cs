using System;

namespace PositionSizer.XTextBoxControl.ControlValue;

public interface IControlValue<T>
{
    event EventHandler<ControlValueUpdatedEventArgs<T>> ValueUpdated;
    void SetValueWithoutTriggeringEvent(T value);
    T Value { get; set; }
    bool IsInteger { get; }
    bool IsDouble { get; }
    bool IsString { get; }
}