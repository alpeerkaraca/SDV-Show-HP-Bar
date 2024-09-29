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
    private static Texture2D _border;

    /// <summary>
    /// Texture that is used to draw lifebar
    /// </summary>
    private static Texture2D _lifeMeter;

    /// <summary>
    /// Stores max health because some monsters start with higher health than <see cref="Monster.MaxHealth"/> states
    /// </summary>
    private static readonly Dictionary<Monster, int> _maxHealthCache = new();

    public static void Initialize(IModHelper helper, ModConfig config)
    {
        _config = config;

        _border = helper.ModContent.Load<Texture2D>("assets/SDV_lifebar.png");

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

        if (_lifeMeter == null)
        {
            _lifeMeter = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            _lifeMeter.SetData(new[] { Color.White });
        }

        IEnumerable<Monster> monsters = Game1.currentLocation.characters.OfType<Monster>().Where(CanShowLifeBar);

        foreach (Monster monster in monsters)
            RenderLifeBar(monster);
    }

    private static void RenderLifeBar(Monster monster)
    {
        int maxHealth = ResolveMaxHealth(monster);

        if (_config.HideFullLifeBar && monster.Health == maxHealth)
            return;

        Vector2 monsterLocalPosition = monster.getLocalPosition(Game1.viewport);
        Vector2 lifebarCenterPos = new(monsterLocalPosition.X + (float)monster.Sprite.SpriteWidth * Game1.pixelZoom / 2, monsterLocalPosition.Y - ((float)monster.Sprite.SpriteHeight + 5) * Game1.pixelZoom / 2);

        int monsterKilledAmount = Game1.stats.specificMonstersKilled.GetValueOrDefault(monster.Name, 0);

        if (!_config.EnableXPNeeded || monsterKilledAmount + Game1.player.combatLevel.Value >= Globals.ExperienceBasicStatsLevel)
        {
            float monsterHealthPercent = (float)monster.Health / (float)maxHealth;

            (Color barColor, Color textColor) = ColorSchemes.GetSchemeOrDefault(_config.ColorScheme).GetBarColors(monsterHealthPercent);

            DrawBarSprite(lifebarCenterPos, Globals.SpriteIndexBack);
            DrawLifeMeter(lifebarCenterPos, monsterHealthPercent, barColor);
            DrawBarSprite(lifebarCenterPos, Globals.SpriteIndexFront);

            (string text, SpriteFont spriteFont, float bottomOffset, float scale) = GetHealthText(monster.Health, monsterKilledAmount);
            DrawText(lifebarCenterPos, text, textColor, spriteFont, bottomOffset, scale);
        }
        else
        {
            DrawBarSprite(lifebarCenterPos, Globals.SpriteIndexUnavailable);
            DrawText(lifebarCenterPos, Globals.TextHealthUnavailable, Color.Ivory, Game1.smallFont, Globals.TextSpecCharOffset, Globals.TextSpecCharScaleLevel);
        }
    }

    private static (string text, SpriteFont spriteFont, float bottomOffset, float scale) GetHealthText(int health, int monsterKilledAmount)
    {
        if (!_config.EnableXPNeeded || Globals.ExperienceHighHealth > health || monsterKilledAmount + Game1.player.combatLevel.Value * 4 >= Globals.ExperienceFullStatsLevel)
        {
            string healthText = _config.PadHealth ? $"{health:000}" : health.ToString();
            return (healthText, Game1.tinyFont, Globals.TextDefaultOffset, Globals.TextDefaultScaleLevel);
        }

        return (Globals.TextHealthHigh, Game1.smallFont, Globals.TextSpecCharOffset, Globals.TextSpecCharScaleLevel);
    }

    private static void DrawLifeMeter(Vector2 center, float barLengthPercent, Color barColor)
    {
        Vector2 internalLifebarPos = new(center.X - _border.Width / 2f + Globals.LifebarMargins, center.Y);
        Rectangle rect = new(0, 0, (int)((_border.Width - Globals.LifebarMargins * 2) * barLengthPercent),
            Globals.SpriteHeight - Globals.LifebarMargins * 2);
        Vector2 origin = new(0, rect.Height / 2f);

        Game1.spriteBatch.Draw(_lifeMeter, internalLifebarPos, rect, barColor, 0f, origin, 1f, SpriteEffects.None, 0f);
    }

    private static void DrawBarSprite(Vector2 center, int spriteIndex)
    {
        Rectangle rect = new(0, Globals.SpriteHeight * spriteIndex, _border.Width, Globals.SpriteHeight);
        Vector2 origin = new(_border.Width / 2f, Globals.SpriteHeight / 2f);

        Game1.spriteBatch.Draw(_border, center, rect, Color.White * 1.0f, 0f, origin, 1f, SpriteEffects.None, 0f);
    }

    private static void DrawText(Vector2 center, string text, Color textColor, SpriteFont spriteFont, float bottomOffset, float scale)
    {
        if (_config.HideTextInfo)
            return;

        Vector2 textSize = spriteFont.MeasureString(text);
        Vector2 origin = new(textSize.X / 2, textSize.Y / 2 + bottomOffset);

        Game1.spriteBatch.DrawString(spriteFont, text, center, textColor, 0f, origin, scale, SpriteEffects.None, 0f);
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
