using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class SerializableTimeFrame : IEquatable<SerializableTimeFrame>
{
    public string Name { get; set; }

    public SerializableTimeFrame FromTimeFrame(TimeFrame timeFrame)
    {
        Name = timeFrame.Name;
        
        return this;
    }

    public TimeFrame ToTimeFrame()
    {
        return TimeFrame.Parse(Name);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as SerializableTimeFrame);
    }

    public bool Equals(SerializableTimeFrame other)
    {
        if (other == null)
            return false;

        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Name!.GetHashCode();
    }

    public override string ToString()
    {
        return $"Name: {Name}";
    }
}