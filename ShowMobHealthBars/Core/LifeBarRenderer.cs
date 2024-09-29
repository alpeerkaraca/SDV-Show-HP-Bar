using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace ShowMobHealthBars.Core;

internal static class LifeBarRenderer
{
    private static ModConfig _config;

    /// <summary>
    /// Border texture of the lifebar
    /// </summary>
    private static Texture2D _lifebarBorder;

    /// <summary>
    /// Texture that is used to draw lifebar
    /// </summary>
    private static Texture2D _whitePixel;

    private static readonly Dictionary<Monster, int> _maxHealthCache = new();

    public static void Initialize(IModHelper helper, ModConfig config)
    {
        _config = config;
        _lifebarBorder = helper.ModContent.Load<Texture2D>("assets/SDV_lifebar.png");

        helper.Events.Display.RenderedWorld += RenderLifeBars;
        helper.Events.World.NpcListChanged += UpdateMaxHealthCache;
    }

    /// <summary>
    /// Handle the rendering of mobs life bars
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event parameters</param>
    private static void RenderLifeBars(object sender, RenderedWorldEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.activeClickableMenu != null)
            return;

        if (_whitePixel == null)
        {
            _whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }

        IEnumerable<Monster> monsters = Game1.currentLocation.characters.OfType<Monster>();

        foreach (Monster monster in monsters)
            RenderLifeBar(monster);
    }

    private static void RenderLifeBar(Monster monster)
    {
        if (!CanShowLifeBar(monster))
            return;

        int maxHealth = ResolveMaxHealth(monster);

        if (_config.HideFullLifeBar && maxHealth == monster.Health)
            return;

        int monsterKilledAmount = Game1.stats.specificMonstersKilled.GetValueOrDefault(monster.Name, 0);
        string healthText = "???";

        Color barColor;
        float barLengthPercent = 1f;

        TextProps textProps = new()
        {
            Font = Game1.smallFont,
            Color = Color.Ivory,
            Scale = Globals.TEXT_SPEC_CHAR_SCALE_LEVEL,
            BottomOffset = Globals.TEXT_SPEC_CHAR_OFFSET
        };

        bool useAlternateSprite = true;

        // If level system is deactivated or the basic level is OK, we display the colours
        if (!_config.EnableXPNeeded || monsterKilledAmount + Game1.player.combatLevel.Value > Globals.EXPERIENCE_BASIC_STATS_LEVEL)
        {
            useAlternateSprite = false;

            float monsterHealthPercent = (float)monster.Health / (float)maxHealth;

            (Color BarColor, Color TextColor) colorSchemeColors = ColorSchemes.GetSchemeOrDefault(_config.ColorScheme).GetBarColors(monsterHealthPercent);
            barColor = colorSchemeColors.BarColor;

            // If level system is deactivated or the full level is OK, we display the stats
            if (!_config.EnableXPNeeded || monsterKilledAmount + Game1.player.combatLevel.Value * 4 > Globals.EXPERIENCE_FULL_STATS_LEVEL)
            {
                barLengthPercent = monsterHealthPercent;

                // If it's a very strong monster, we hide the life counter
                if (!_config.EnableXPNeeded || monster.Health <= 999)
                {
                    healthText = _config.PadHealth ? $"{monster.Health:000}" : monster.Health.ToString();
                    textProps.Font = Game1.tinyFont;
                    textProps.Color = colorSchemeColors.TextColor;
                    textProps.Scale = Globals.TEXT_DEFAUT_SCALE_LEVEL;
                    textProps.BottomOffset = Globals.TEXT_DEFAUT_OFFSET;
                }
                else
                    healthText = "!!!";
            }
        }
        else
            barColor = Color.DarkSlateGray;

        // Display the life bar
        Vector2 monsterLocalPosition = monster.getLocalPosition(Game1.viewport);
        Vector2 lifebarCenterPos = new(monsterLocalPosition.X + (float)monster.Sprite.SpriteWidth * Game1.pixelZoom / 2, (float)monsterLocalPosition.Y - ((float)monster.Sprite.SpriteHeight + 5) * Game1.pixelZoom / 2);

        // If we use alternate sprite (do not show life level)
        if (useAlternateSprite)
        {
            //Display background of the bar
            Game1.spriteBatch.Draw(
                _lifebarBorder,
                lifebarCenterPos,
                new Rectangle(0, Globals.SPRITE_HEIGHT * Globals.SPRITE_INDEX_DEACTIVATED, _lifebarBorder.Width, Globals.SPRITE_HEIGHT),
                Color.White * 1f,
                0f,
                new Vector2(_lifebarBorder.Width / 2f, Globals.SPRITE_HEIGHT / 2f),
                1f,
                SpriteEffects.None,
                0f
            );
        }
        else
        {
            //Display background of the bar
            Game1.spriteBatch.Draw(
                _lifebarBorder,
                lifebarCenterPos,
                new Rectangle(0, Globals.SPRITE_HEIGHT * Globals.SPRITE_INDEX_BACK, _lifebarBorder.Width, Globals.SPRITE_HEIGHT),
                Color.White * 1f,
                0f,
                new Vector2(_lifebarBorder.Width / 2f, Globals.SPRITE_HEIGHT / 2f),
                1f,
                SpriteEffects.None,
                0f
            );

            //Calculate size of the lifebox
            Rectangle lifeBox = new(0, 0, (int)((_lifebarBorder.Width - Globals.LIFEBAR_MARGINS * 2) * barLengthPercent), Globals.SPRITE_HEIGHT - Globals.LIFEBAR_MARGINS * 2);
            Vector2 internalLifebarPos = new(lifebarCenterPos.X - _lifebarBorder.Width / 2f + Globals.LIFEBAR_MARGINS, lifebarCenterPos.Y);
            //Display life bar
            Game1.spriteBatch.Draw(
                _whitePixel,
                internalLifebarPos,
                lifeBox,
                barColor,
                0f,
                new Vector2(0, lifeBox.Height / 2f),
                1f,
                SpriteEffects.None,
                0f
            );
        }

        // Draw text
        if (!_config.HideTextInfo)
        {
            Vector2 textsize = textProps.Font.MeasureString(healthText);
            Game1.spriteBatch.DrawString(
                textProps.Font,
                healthText,
                lifebarCenterPos,
                textProps.Color,
                0f,
                new Vector2(textsize.X / 2, textsize.Y / 2 + textProps.BottomOffset),
                textProps.Scale,
                SpriteEffects.None,
                0f
            );
        }

        // If we display alternate sprite, there is no foreground
        if (!useAlternateSprite)
        {
            //Display foreground of the bar
            Game1.spriteBatch.Draw(
                _lifebarBorder,
                lifebarCenterPos,
                new Rectangle(0, Globals.SPRITE_HEIGHT * Globals.SPRITE_INDEX_FRONT, _lifebarBorder.Width, Globals.SPRITE_HEIGHT),
                Color.White * 1.0f,
                0f,
                new Vector2(_lifebarBorder.Width / 2f, Globals.SPRITE_HEIGHT / 2f),
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }

    private static int ResolveMaxHealth(Monster monster)
    {
        if (!_maxHealthCache.TryGetValue(monster, out int maxHealth))
            _maxHealthCache[monster] = maxHealth = Math.Max(monster.Health, monster.MaxHealth);

        return maxHealth;
    }

    private static bool CanShowLifeBar(Monster monster)
    {
        if (monster.IsInvisible || !Utility.isOnScreen(monster.position.Value, Game1.tileSize * 3))
            return false;

        switch (monster)
        {
            case Spiker:
            case RockCrab when monster.Sprite.CurrentFrame % 4 == 0:
            case RockGolem when monster.Health == monster.MaxHealth:
            case Bug bug when bug.isArmoredBug.Value:
            case Grub when monster.Sprite.CurrentFrame == 19:
                return false;
        }

        return true;
    }

    private static void UpdateMaxHealthCache(object sender, NpcListChangedEventArgs e)
    {
        IEnumerable<Monster> monsters = e.Removed.OfType<Monster>();

        foreach (Monster monster in monsters)
            _maxHealthCache.Remove(monster);
    }
}
