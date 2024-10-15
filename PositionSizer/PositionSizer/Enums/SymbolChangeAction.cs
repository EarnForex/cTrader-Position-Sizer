namespace cAlgo.Robots;

public enum SymbolChangeAction
{
    /// <summary>
    /// Each chart symbol has it's own model file and parameter settings file
    /// </summary>
    EachSymbolOwnSettings,
    
    /// <summary>
    /// Everytime the symbol changes, the robot will reset to default settings
    /// If there's an old settings file, it will be kept and used
    /// </summary>
    ResetToDefaultsOnSymbolChange,
    
    /// <summary>
    /// There are some global settings that are shared between all symbols
    /// Those that are not shared will be reset to default everytime the symbol changes
    /// </summary>
    KeepPanelAsIs
}