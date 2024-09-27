using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace ShowMobHealthBars.Core;

/// <summary>
/// Holds the multiple stages of the lifebars color schemes
/// </summary>
internal sealed class ColorScheme
{
    private readonly (Color BarColor, Color TextColor)[] _colors;

    /// <summary>
    /// Represents the name used in the config menu
    /// </summary>
    public string Name { get; }

    public ColorScheme(string name, (Color BarColor, Color TextColor) colors1,
        (Color BarColor, Color TextColor) colors2, params (Color BarColor, Color TextColor)[] moreColors)
    {
        // Inverse colors to account the lerp direction and avoid "inverting" the monsterHealthPercent range
        _colors = new[] { colors1, colors2, }.Concat(moreColors).Reverse().ToArray();
        Name = name;
    }

    /// <summary>
    /// Method to retrieve the lifebar colors based on <paramref name="monsterHealthPercent"/>
    /// </summary>
    public (Color BarColor, Color TextColor) GetBarColors(float monsterHealthPercent)
    {
        if (_colors.Length == 2)
            return Lerp(_colors[0], _colors[1], monsterHealthPercent);

        switch (monsterHealthPercent)
        {
            case <= 0:
                return _colors[0];
            case >= 1:
                return _colors[^1];
        }

        float scaledAmount = monsterHealthPercent * (_colors.Length - 1);
        int index = (int)Math.Floor(scaledAmount);
        float localAmount = scaledAmount - index;

        return Lerp(_colors[index], _colors[index + 1], localAmount);
    }

    private static (Color BarColor, Color TextColor) Lerp((Color BarColor, Color TextColor) colors1,
        (Color BarColor, Color TextColor) colors2, float amount)
        => (
            Color.Lerp(colors1.BarColor, colors2.BarColor, amount),
            Color.Lerp(colors1.TextColor, colors2.TextColor, amount)
        );
}