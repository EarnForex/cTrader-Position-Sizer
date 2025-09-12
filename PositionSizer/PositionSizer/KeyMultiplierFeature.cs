using cAlgo.API;
using cAlgo.Robots.Tools;
using PositionSizer.XTextBoxControl.ByTypes;

namespace cAlgo.Robots;

public interface IKeyMultiplierFeature
{
    Chart Chart { get; }
    void Print(object obj);
}

public class KeyMultiplierFeature
{
    private readonly IKeyMultiplierFeature _resources;
    public double KeyMultiplier { get; set; }

    public KeyMultiplierFeature(IKeyMultiplierFeature resources)
    {
        _resources = resources;

        Chart.KeyDown += args =>
        {
            KeyMultiplier = 1;
            
            /*
             Add keyboard modifiers for the increase and decrease buttons to add/subtract
             in multiples of the tick size: Ctrl (×10), Shift (×100), and Ctrl+Shift (×1000).
             For the +/- buttons - e.g., near the Stop-loss, Take-profit, and so on.
             */

            if (args.CtrlKey && !args.ShiftKey)
            {
                KeyMultiplier = 10;
                Print($"Key Multiplier: {KeyMultiplier}");
                return;
            }

            if (!args.CtrlKey && args.ShiftKey)
            {
                KeyMultiplier = 100;
                Print($"Key Multiplier: {KeyMultiplier}");
                return;
            }
            
            if (args.CtrlKey && args.ShiftKey)
            {
                KeyMultiplier = 1000;
                Print($"Key Multiplier: {KeyMultiplier}");
                return;
            }
        };
        
        Chart.MouseDown += args =>
        {
            Print($"Setting Key Multiplier to 1");
            KeyMultiplier = 1;
        };
    }
    
    public void SetFeatureOnButton(XTextBoxDoubleNumeric button)
    {
        button.DecrementButtonClicked += (sender, args) => button.ChangeByFactor *= KeyMultiplier;
        button.IncrementButtonClicked += (sender, args) => button.ChangeByFactor *= KeyMultiplier;
        button.OnAfterClick += (sender, args) =>
        {
            if (KeyMultiplier.IsNot(1))
                button.ChangeByFactor /= KeyMultiplier;
            
            ResetKeyMultiplier();
        };
    }
    
    public void ResetKeyMultiplier()
    {
        KeyMultiplier = 1;
    }

    private Chart Chart => _resources.Chart;
    private void Print(object obj)
    {
        _resources.Print(obj);
    }
}