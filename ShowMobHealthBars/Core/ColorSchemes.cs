using System.Linq;
using Microsoft.Xna.Framework;

namespace ShowMobHealthBars.Core;

internal static class ColorSchemes
{
    /// <summary>
    /// Available colour schemes of the life bar
    /// </summary>
    public static readonly ColorScheme[] Schemes =
    {
        new("Classic",
            (Color.LawnGreen, Color.DarkSlateGray),
            (Color.YellowGreen, Color.DarkSlateGray),
            (Color.Gold, Color.DarkSlateGray),
            (Color.DarkOrange, Color.DarkSlateGray),
            (Color.Crimson, Color.DarkSlateGray)
        ),
        new("Classic (inverted)",
            (Color.Crimson, Color.Ivory),
            (Color.DarkOrange, Color.Ivory),
            (Color.Gold, Color.DarkSlateGray),
            (Color.YellowGreen, Color.DarkSlateGray),
            (Color.LawnGreen, Color.DarkSlateGray)
        ),
        new("Midnight",
            (Color.CornflowerBlue, Color.DarkSlateGray),
            (Color.RoyalBlue, Color.Ivory),
            (Color.Blue, Color.Ivory),
            (Color.DarkBlue, Color.DarkSlateGray),
            (Color.MidnightBlue, Color.DarkSlateGray)
        ),
        new("Midnight (inverted)",
            (Color.MidnightBlue, Color.Ivory),
            (Color.DarkBlue, Color.Ivory),
            (Color.Blue, Color.Ivory),
            (Color.RoyalBlue, Color.DarkSlateGray),
            (Color.CornflowerBlue, Color.DarkSlateGray)
        ),
        new("Rasmodius",
            (Color.DarkViolet, Color.Ivory),
            (Color.MediumOrchid, Color.DarkSlateGray),
            (Color.Orchid, Color.DarkSlateGray),
            (Color.MediumPurple, Color.DarkSlateGray),
            (Color.BlueViolet, Color.DarkSlateGray)
        ),
        new("Rasmodius (inverted)",
            (Color.BlueViolet, Color.Ivory),
            (Color.MediumPurple, Color.Ivory),
            (Color.Orchid, Color.DarkSlateGray),
            (Color.MediumOrchid, Color.DarkSlateGray),
            (Color.DarkViolet, Color.DarkSlateGray)
        ),
    };

    public static ColorScheme GetSchemeOrDefault(int colorScheme)
        => Schemes.ElementAtOrDefault(colorScheme) ?? Schemes.First();
}
