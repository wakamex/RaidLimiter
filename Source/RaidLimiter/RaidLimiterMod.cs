using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace RaidLimiter;

[StaticConstructorOnStartup]
internal class RaidLimiterMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static RaidLimiterMod instance;

    private static string currentVersion;

    /// <summary>
    ///     The private settings
    /// </summary>
    private RaidLimiterSettings settings;

    /// <summary>
    ///     Cunstructor
    /// </summary>
    /// <param name="content"></param>
    public RaidLimiterMod(ModContentPack content) : base(content)
    {
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.RaidLimiter"));
        instance = this;
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal RaidLimiterSettings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<RaidLimiterSettings>();
            }

            return settings;
        }
        set => settings = value;
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "RaidLimiter";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        listing_Standard.ColumnWidth = (rect.width / 2) - 10f;
        listing_Standard.CheckboxLabeled("RL.CapByDifficultySettings.label".Translate(),
            ref Settings.CapByDifficultySettings,
            "RL.CapByDifficultySettings.description".Translate());
        listing_Standard.GapLine();
        listing_Standard.Gap();
        listing_Standard.Label("RL.CapByDifficultySettingsMultiplier.label".Translate(), -1,
            "RL.CapByDifficultySettingsMultiplier.description".Translate());
        Settings.CapByDifficultySettingsMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.CapByDifficultySettingsMultiplier, 100f, 10000f, false,
            Settings.CapByDifficultySettingsMultiplier.ToString(), null, null, 1);
        listing_Standard.Gap();
        listing_Standard.Label("RL.RaidPointsMultiplier.label".Translate(), -1,
            "RL.RaidPointsMultiplier.description".Translate());
        Settings.RaidPointsMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.RaidPointsMultiplier, 0.01f, 5f, false, Settings.RaidPointsMultiplier.ToString(), null, null,
            0.01f);
        listing_Standard.Gap();
        listing_Standard.Label("RL.RaidCap.label".Translate(), -1,
            "RL.RaidCap.description".Translate());
        Settings.RaidCap = Widgets.HorizontalSlider(listing_Standard.GetRect(20), Settings.RaidCap, -1f, 10000f,
            false, Settings.RaidCap.ToString(), null, null, 1f);
        listing_Standard.Gap();
        listing_Standard.Label("RL.RaidCapPointsPerColonist.label".Translate(), -1,
            "RL.RaidCapPointsPerColonist.description".Translate());
        Settings.RaidCapPointsPerColonist = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.RaidCapPointsPerColonist, -1f, 200f, false, Settings.RaidCapPointsPerColonist.ToString(), null,
            null, 1);
        listing_Standard.Gap();
        listing_Standard.Label("RL.ColonistMultiplier.label".Translate(), -1,
            "RL.ColonistMultiplier.description".Translate());
        Settings.ColonistMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.ColonistMultiplier, 0, 5f, false, Settings.ColonistMultiplier.ToString(), null, null, 0.1f);
        listing_Standard.Gap();
        listing_Standard.Label("RL.CombatAnimalMultiplier.label".Translate(), -1,
            "RL.CombatAnimalMultiplier.description".Translate());
        Settings.CombatAnimalMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.CombatAnimalMultiplier, 0, 5f, false, Settings.CombatAnimalMultiplier.ToString(), null, null,
            0.1f);
        listing_Standard.Gap();
        listing_Standard.Label("RL.WealthMultiplier.label".Translate(), -1,
            "RL.WealthMultiplier.description".Translate());
        Settings.WealthMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.WealthMultiplier, 0, 5f, false, Settings.WealthMultiplier.ToString(), null, null, 0.1f);
        listing_Standard.NewColumn();
        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("RL.Debug.label".Translate(), ref Settings.Debug,
            "RL.Debug.description".Translate());
        listing_Standard.GapLine();
        Text.Font = GameFont.Tiny;
        listing_Standard.Label("RL.SoftCaps.label".Translate());
        Text.Font = GameFont.Small;
        listing_Standard.Gap();
        listing_Standard.Label("RL.SoftCapBeginTapering.label".Translate(), -1,
            "RL.SoftCapBeginTapering.description".Translate());
        Settings.SoftCapBeginTapering = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.SoftCapBeginTapering, -1f, 10000f, false, Settings.SoftCapBeginTapering.ToString(), null, null,
            1);
        listing_Standard.Gap();
        listing_Standard.Label("RL.SoftCapExponent.label".Translate(), -1,
            "RL.SoftCapExponent.description".Translate());
        Settings.SoftCapExponent = Widgets.HorizontalSlider(listing_Standard.GetRect(20), Settings.SoftCapExponent,
            0.01f, 5f, false, Settings.SoftCapExponent.ToString(), null, null, 0.01f);
        listing_Standard.Gap();
        listing_Standard.Label("RL.AdaptationCap.label".Translate(), -1,
            "RL.AdaptationCap.description".Translate());
        Settings.AdaptationCap = Widgets.HorizontalSlider(listing_Standard.GetRect(20), Settings.AdaptationCap, -1f,
            5f, false, Settings.AdaptationCap.ToString(), null, null, 0.1f);
        listing_Standard.Gap();
        listing_Standard.Label("RL.AdaptationTapering.label".Translate(), -1,
            "RL.AdaptationTapering.description".Translate());
        Settings.AdaptationTapering = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.AdaptationTapering, -1f, 10000f, false, Settings.AdaptationTapering.ToString(), null, null, 1);
        listing_Standard.Gap();
        listing_Standard.Label("RL.AdaptationExponent.label".Translate(), -1,
            "RL.AdaptationExponent.description".Translate());
        Settings.AdaptationExponent = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
            Settings.AdaptationExponent, 0.01f, 5f, false, Settings.AdaptationExponent.ToString(), null, null,
            0.01f);
        listing_Standard.Gap();
        if (listing_Standard.ButtonText("RL.Reset.label".Translate()))
        {
            Main.ResetSettings(ref settings);
        }

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("RL.version.label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        if (Find.CurrentMap != null)
        {
            StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap);
        }
    }
}