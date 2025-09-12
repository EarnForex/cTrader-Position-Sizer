using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class SerializableColor : IEquatable<SerializableColor>
{
    public int A { get; set; }
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public void FromColor(Color color)
    {
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public Color ToColor()
    {
        return Color.FromArgb(A, R, G, B);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as SerializableColor ?? throw new InvalidOperationException());
    }

    public bool Equals(SerializableColor other)
    {
        if (other == null)
        {
            return false;
        }

        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    public override int GetHashCode()
    {
        return A ^ R ^ G ^ B;
    }

    public override string ToString()
    {
        return $"A: {A}, R: {R}, G: {G}, B: {B}";
    }
}