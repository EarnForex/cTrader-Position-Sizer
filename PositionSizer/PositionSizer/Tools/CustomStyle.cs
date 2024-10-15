using cAlgo.API;

namespace cAlgo.Robots.Tools;

public class CustomStyle
{
    public Style BackgroundStyle { get; set; } = new();
    public Style ButtonStyle { get; set; } = new();
    public Style LockedButtonStyle { get; set; } = new();
    public Style TextBoxStyle { get; set; } = new();
    public Style ReadOnlyTextBoxStyle { get; set; } = new();
}