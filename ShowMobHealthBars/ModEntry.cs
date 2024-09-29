using ShowMobHealthBars.Core;
using ShowMobHealthBars.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ShowMobHealthBars;

/// <summary>
/// Main class of the mod
/// </summary>
public sealed class ModEntry : Mod
{
    /// <summary>
    /// Contains the configuration of the mod
    /// </summary>
    private ModConfig _config;

    /// <summary>
    /// Mod initialization method
    /// </summary>
    /// <param name="helper">helper provided by SMAPI</param>
    public override void Entry(IModHelper helper)
    {
        _config = helper.ReadConfig<ModConfig>();
        EnsureCorrectConfig();

        LifeBarRenderer.Initialize(helper, _config);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        IGenericModConfigMenuApi configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

        if (configMenu is not null)
            ConfigMenu.Initialize(configMenu, ModManifest, Helper, _config);
    }

    /// <summary>
    /// Method that ensure the configuration provided by user is correct and will not break the game
    /// </summary>
    private void EnsureCorrectConfig()
    {
        if (ColorSchemes.Schemes.Length > _config.ColorScheme && _config.ColorScheme >= 0)
            return;

        _config.ColorScheme = 0;
        Helper.WriteConfig(_config);
    }
}
