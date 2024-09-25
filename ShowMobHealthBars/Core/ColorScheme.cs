using Microsoft.Xna.Framework;

namespace ShowMobHealthBars.Core;

internal sealed class StageColors
{
    public Color BarColor { get; }
    public Color TextColor { get; }

    public StageColors(Color barColor, Color textColor)
    {
        BarColor = barColor;
        TextColor = textColor;
    }
}

internal sealed class ColorScheme
{
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

    public StageColors GetBarColors(float monsterHealthPercent) => monsterHealthPercent switch
    {
        > 0.9f => _stage1,
        > 0.65f => _stage2,
        > 0.35f => _stage3,
        > 0.15f => _stage4,
        _ => _stage5
    };
}