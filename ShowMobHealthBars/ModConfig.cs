namespace ShowMobHealthBars;

/// <summary>
/// Mod Configration class
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    /// If true, the life counter needs XP + killcount of mobs to show the life level
    /// If false, always show the life level
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool EnableXPNeeded { get; set; }

    /// <summary>
    /// If true, the life text info is hidden
    /// If false, the text info appear
    /// </summary>
    public bool HideTextInfo { get; set; }

    /// <summary>
    /// If true, the life is hidden if mob has full life
    /// If false, the life is shown at all time
    /// </summary>
    public bool HideFullLifeBar { get; set; }

    /// <summary>
    /// If true, the life is will be padded with leading zero to always be three digits long
    /// If false, the life is shown without leading zeros
    /// </summary>
    public bool PadHealth { get; set; }

    /// <summary>
    /// Allow selecting the color scheme of the life bar
    /// </summary>
    public int ColorScheme { get; set; }

    /// <summary>
    /// Initialization of default values
    /// </summary>
    public ModConfig()
    {
        EnableXPNeeded = true;
        ColorScheme = 0;
        HideTextInfo = false;
        HideFullLifeBar = false;
        PadHealth = false;
    }
}
