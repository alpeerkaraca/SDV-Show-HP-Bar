﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShowMobHealthBars.Core;

/// <summary>
/// Properties of the life text display
/// </summary>
internal sealed class TextProps
{
    /// <summary>
    /// Font to use on the text
    /// </summary>
    public SpriteFont Font { get; set; }

    /// <summary>
    /// Color of the text to display
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Scale transformation of the text
    /// </summary>
    public float Scale { get; set; }

    /// <summary>
    /// Bottom offset to add to correct display
    /// </summary>
    public float BottomOffset { get; set; }
}