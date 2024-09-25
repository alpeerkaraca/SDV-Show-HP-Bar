using Microsoft.Xna.Framework;

namespace ShowMobHealthBars.Core;

/// <summary>
/// Holds the background and text color for a lifebar stage
/// </summary>
internal sealed class StageColors
{
    /// <summary>
    /// Background color of the lifebar
    /// </summary>
    public Color BarColor { get; }

    /// <summary>
    /// Text color of the lifebar
    /// </summary>
    public Color TextColor { get; }

    public StageColors(Color barColor, Color textColor)
    {
        BarColor = barColor;
        TextColor = textColor;
    }
}

/// <summary>
/// Holds the multiple stages of the lifebars color schemes
/// </summary>
internal sealed class ColorScheme
{
    /// <summary>
    /// Represents the name used in the config menu
    /// </summary>
    public string Name { get; }

    private readonly StageColors _stage1;
    private readonly StageColors _stage2;
    private readonly StageColors _stage3;
    private readonly StageColors _stage4;
    private readonly StageColors _stage5;

    public ColorScheme(string name, StageColors stage1, StageColors stage2, StageColors stage3, StageColors stage4, StageColors stage5)
    {
        Name = name;

        _stage1 = stage1;
        _stage2 = stage2;
        _stage3 = stage3;
        _stage4 = stage4;
        _stage5 = stage5;
    }

    /// <summary>
    /// Method to retrieve the lifebar <see cref="StageColors"/> based on <paramref name="monsterHealthPercent"/>
    /// </summary>
    public StageColors GetBarColors(float monsterHealthPercent) => monsterHealthPercent switch
    {
        > 0.9f => _stage1,
        > 0.65f => _stage2,
        > 0.35f => _stage3,
        > 0.15f => _stage4,
        _ => _stage5
    };
}