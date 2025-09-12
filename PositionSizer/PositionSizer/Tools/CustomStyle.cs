using cAlgo.API;

namespace cAlgo.Robots.Tools;

public class CustomStyle
{
    public Style BackgroundStyle { get; } = new();
    public Style ButtonStyle { get; } = new();
    public Style LockedButtonStyle { get; } = new();
    public Style TextBoxStyle { get; } = new();
    public Style ReadOnlyTextBoxStyle { get; } = new();
    public Style CheckBoxStyle { get; } = new();
}