using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShowMobHealthBars.Core;
using ShowMobHealthBars.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace ShowMobHealthBars;

/// <summary>
/// Main class of the mod
/// </summary>
public sealed class ModEntry : Mod
{
    /// <summary>
    /// Texture that is used to draw lifebar
    /// </summary>
    private Texture2D _whitePixel;
    /// <summary>
    /// Contains the configuration of the mod
    /// </summary>
    private ModConfig _config;


    /// <summary>
    /// Border texture of the lifebar
    /// </summary>
    private static Texture2D lifebarBorder;

    /// <summary>
    /// Available colour schemes of the life bar
    /// </summary>
    private static readonly ColorScheme[] ColorSchemes =
    {
        new("Classic",
            new StageColors(Color.LawnGreen, Color.DarkSlateGray),
            new StageColors(Color.YellowGreen, Color.DarkSlateGray),
            new StageColors(Color.Gold, Color.DarkSlateGray),
            new StageColors(Color.DarkOrange, Color.DarkSlateGray),
            new StageColors(Color.Crimson, Color.DarkSlateGray)
        ),
        new("Classic (inverted)",
            new StageColors(Color.Crimson, Color.Ivory),
            new StageColors(Color.DarkOrange, Color.Ivory),
            new StageColors(Color.Gold, Color.DarkSlateGray),
            new StageColors(Color.YellowGreen, Color.DarkSlateGray),
            new StageColors(Color.LawnGreen, Color.DarkSlateGray)
        ),
        new("Midnight",
            new StageColors(Color.CornflowerBlue, Color.DarkSlateGray),
            new StageColors(Color.RoyalBlue, Color.Ivory),
            new StageColors(Color.Blue, Color.Ivory),
            new StageColors(Color.DarkBlue, Color.DarkSlateGray),
            new StageColors(Color.MidnightBlue, Color.DarkSlateGray)
        ),
        new("Midnight (inverted)",
            new StageColors(Color.MidnightBlue, Color.Ivory),
            new StageColors(Color.DarkBlue, Color.Ivory),
            new StageColors(Color.Blue, Color.Ivory),
            new StageColors(Color.RoyalBlue, Color.DarkSlateGray),
            new StageColors(Color.CornflowerBlue, Color.DarkSlateGray)
        ),
        new("Rasmodius",
            new StageColors(Color.DarkViolet, Color.Ivory),
            new StageColors(Color.MediumOrchid, Color.DarkSlateGray),
            new StageColors(Color.Orchid, Color.DarkSlateGray),
            new StageColors(Color.MediumPurple, Color.DarkSlateGray),
            new StageColors(Color.BlueViolet, Color.DarkSlateGray)
        ),
        new("Rasmodius (inverted)",
            new StageColors(Color.BlueViolet, Color.Ivory),
            new StageColors(Color.MediumPurple, Color.Ivory),
            new StageColors(Color.Orchid, Color.DarkSlateGray),
            new StageColors(Color.MediumOrchid, Color.DarkSlateGray),
            new StageColors(Color.DarkViolet, Color.DarkSlateGray)
        ),
    };

    private static ColorScheme GetColorSchemeOrDefault(int colorScheme)
        => ColorSchemes.ElementAtOrDefault(colorScheme) ?? ColorSchemes.First();

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        // get Generic Mod Config Menu's API (if it's installed)
        IGenericModConfigMenuApi configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            mod: ModManifest,
            reset: () => _config = new ModConfig(),
            save: () => Helper.WriteConfig(_config)
        );

        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ehb.config.enableXPNeeded.title"),
            tooltip: () => Helper.Translation.Get("ehb.config.enableXPNeeded.desc"),
            getValue: () => _config.EnableXPNeeded,
            setValue: value => _config.EnableXPNeeded = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ehb.config.hideTextInfo.title"),
            tooltip: () => Helper.Translation.Get("ehb.config.hideTextInfo.desc"),
            getValue: () => _config.HideTextInfo,
            setValue: value => _config.HideTextInfo = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ehb.config.hideFullLifeBar.title"),
            tooltip: () => Helper.Translation.Get("ehb.config.hideFullLifeBar.desc"),
            getValue: () => _config.HideFullLifeBar,
            setValue: value => _config.HideFullLifeBar = value
        );
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ehb.config.colorScheme.title"),
            tooltip: () => Helper.Translation.Get("ehb.config.colorScheme.desc"),
            allowedValues: ColorSchemes.Select(colorScheme => colorScheme.Name).ToArray(),
            getValue: () => GetColorSchemeOrDefault(_config.ColorScheme).Name,
            setValue: value =>
            {
                int index = Array.FindIndex(ColorSchemes, colorScheme => colorScheme.Name == value);
                _config.ColorScheme = Math.Max(index, 0);
            }
        );
    }

    /// <summary>
    /// Mod initialization method
    /// </summary>
    /// <param name="helper">helper provided by SMAPI</param>
    public override void Entry(IModHelper helper)
    {
        _config = Helper.ReadConfig<ModConfig>();
        EnsureCorrectConfig();
        //lifebarBorder = helper.ContentPacks.Load<Texture2D>(@"assets/SDV_lifebar.png", ContentSource.ModFolder);
        lifebarBorder = helper.ModContent.Load<Texture2D>(@"assets/SDV_lifebar.png");
        helper.Events.Display.RenderedWorld += RenderLifeBars;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    /// <summary>
    /// Method that ensure the configuration provided by user is correct and will not break the game
    /// </summary>
    private void EnsureCorrectConfig()
    {
        if (ColorSchemes.Length > _config.ColorScheme && _config.ColorScheme >= 0)
            return;

        _config.ColorScheme = 0;
        Helper.WriteConfig(_config);
    }

    /// <summary>
    /// Handle the rendering of mobs life bars
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="e">Event parameters</param>
    private void RenderLifeBars(object sender, RenderedWorldEventArgs e)
    {

        if (!Context.IsWorldReady || Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.activeClickableMenu != null) return;

        if (_whitePixel == null)
        {
            _whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }

        // Iterate through all NPC
        foreach (NPC character in Game1.currentLocation.characters)
        {
            // We only care about monsters
            if (character is not Monster monster)
            {
                continue;
            }

            // If monster is not visible, next
            if (monster.IsInvisible || !Utility.isOnScreen(monster.position.Value, 3 * Game1.tileSize))
            {
                continue;
            }

            switch (monster)
            {
                // Check if the current monster should not display life bar
                case RockCrab when monster.Sprite.CurrentFrame % 4 == 0:
                case RockGolem when monster.Health == monster.MaxHealth:
                case Bug bug when bug.isArmoredBug.Value:
                case Grub when monster.Sprite.CurrentFrame == 19:
                    continue;
            }


            // Get all infos about the monster
            int health = monster.Health;
            int maxHealth = monster.MaxHealth;
            if (health > maxHealth) maxHealth = health;
            if (_config.HideFullLifeBar && maxHealth == health) continue;


            // If monster has already been killed once by player, we get the number of kills, else it's 0
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

                float monsterHealthPercent = (float)health / (float)maxHealth;

                StageColors stageColors = GetColorSchemeOrDefault(_config.ColorScheme).GetBarColors(monsterHealthPercent);
                barColor = stageColors.BarColor;

                // If level system is deactivated or the full level is OK, we display the stats
                if (!_config.EnableXPNeeded || monsterKilledAmount + Game1.player.combatLevel.Value * 4 > Globals.EXPERIENCE_FULL_STATS_LEVEL)
                {
                    barLengthPercent = monsterHealthPercent;
                    // If it's a very strong monster, we hide the life counter
                    if (_config.EnableXPNeeded && monster.Health > 999)
                    {
                        healthText = "!!!";
                    }
                    else
                    {
                        healthText = $"{health:000}";
                        textProps.Font = Game1.tinyFont;
                        textProps.Color = stageColors.TextColor;
                        textProps.Scale = Globals.TEXT_DEFAUT_SCALE_LEVEL;
                        textProps.BottomOffset = Globals.TEXT_DEFAUT_OFFSET;
                    }
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
                    lifebarBorder,
                    lifebarCenterPos,
                    new Rectangle(0, Globals.SPRITE_HEIGHT * Globals.SPRITE_INDEX_DEACTIVATED, lifebarBorder.Width, Globals.SPRITE_HEIGHT),
                    Color.White * 1f,
                    0f,
                    new Vector2(lifebarBorder.Width / 2f, Globals.SPRITE_HEIGHT / 2f),
                    1f,
                    SpriteEffects.None,
                    0f);
            }
            else
            {
                //Display background of the bar
                Game1.spriteBatch.Draw(
                    lifebarBorder,
                    lifebarCenterPos,
                    new Rectangle(0, Globals.SPRITE_HEIGHT * Globals.SPRITE_INDEX_BACK, lifebarBorder.Width, Globals.SPRITE_HEIGHT),
                    Color.White * 1f,
                    0f,
                    new Vector2(lifebarBorder.Width / 2f, Globals.SPRITE_HEIGHT / 2f),
                    1f,
                    SpriteEffects.None,
                    0f);

                //Calculate size of the lifebox
                Rectangle lifeBox = new(0, 0, (int)((lifebarBorder.Width - Globals.LIFEBAR_MARGINS * 2) * barLengthPercent), Globals.SPRITE_HEIGHT - Globals.LIFEBAR_MARGINS * 2);
                Vector2 internalLifebarPos = new(lifebarCenterPos.X - lifebarBorder.Width / 2f + Globals.LIFEBAR_MARGINS, lifebarCenterPos.Y);
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
                    0f);
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
                    0f);
            }

            // If we display alternate sprite, there is no foreground
            if (!useAlternateSprite)
            {
                //Display foreground of the bar
                Game1.spriteBatch.Draw(
                    lifebarBorder,
                    lifebarCenterPos,
                    new Rectangle(0, Globals.SPRITE_HEIGHT * Globals.SPRITE_INDEX_FRONT, lifebarBorder.Width, Globals.SPRITE_HEIGHT),
                    Color.White * 1.0f,
                    0f,
                    new Vector2(lifebarBorder.Width / 2f, Globals.SPRITE_HEIGHT / 2f),
                    1f,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
}