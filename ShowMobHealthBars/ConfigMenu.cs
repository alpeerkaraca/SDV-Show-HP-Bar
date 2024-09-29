using System;
using System.Linq;
using ShowMobHealthBars.Core;
using ShowMobHealthBars.GenericModConfigMenu;
using StardewModdingAPI;

namespace ShowMobHealthBars;

internal static class ConfigMenu
{
    public static void Initialize(IGenericModConfigMenuApi configMenu, IManifest modManifest, IModHelper helper, ModConfig config)
    {
        configMenu.Register(
            mod: modManifest,
            reset: () => config = new ModConfig(),
            save: () => helper.WriteConfig(config)
        );

        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => helper.Translation.Get("ehb.config.enableXPNeeded.title"),
            tooltip: () => helper.Translation.Get("ehb.config.enableXPNeeded.desc"),
            getValue: () => config.EnableXPNeeded,
            setValue: value => config.EnableXPNeeded = value
        );
        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => helper.Translation.Get("ehb.config.hideTextInfo.title"),
            tooltip: () => helper.Translation.Get("ehb.config.hideTextInfo.desc"),
            getValue: () => config.HideTextInfo,
            setValue: value => config.HideTextInfo = value
        );
        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => helper.Translation.Get("ehb.config.hideFullLifeBar.title"),
            tooltip: () => helper.Translation.Get("ehb.config.hideFullLifeBar.desc"),
            getValue: () => config.HideFullLifeBar,
            setValue: value => config.HideFullLifeBar = value
        );
        configMenu.AddBoolOption(
            mod: modManifest,
            name: () => helper.Translation.Get("ehb.config.padHealth.title"),
            tooltip: () => helper.Translation.Get("ehb.config.padHealth.desc"),
            getValue: () => config.PadHealth,
            setValue: value => config.PadHealth = value
        );
        configMenu.AddTextOption(
            mod: modManifest,
            name: () => helper.Translation.Get("ehb.config.colorScheme.title"),
            tooltip: () => helper.Translation.Get("ehb.config.colorScheme.desc"),
            allowedValues: ColorSchemes.Schemes.Select(colorScheme => colorScheme.Name).ToArray(),
            getValue: () => ColorSchemes.GetSchemeOrDefault(config.ColorScheme).Name,
            setValue: value =>
            {
                int index = Array.FindIndex(ColorSchemes.Schemes, colorScheme => colorScheme.Name == value);
                config.ColorScheme = Math.Max(index, 0);
            }
        );
    }
}
