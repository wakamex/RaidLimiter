using RimWorld;
using UnityEngine;
using Verse;

namespace RaidLimiter
{
    [StaticConstructorOnStartup]
    internal class RaidLimiterMod : Mod
    {
        /// <summary>
        ///     The instance of the settings to be read by the mod
        /// </summary>
        public static RaidLimiterMod instance;

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
            listing_Standard.CheckboxLabeled("Cap By Difficulty Settings", ref Settings.CapByDifficultySettings,
                "Leave this on if you want my raid system to pick a value based off your storyteller's difficulty");
            listing_Standard.GapLine();
            listing_Standard.Gap();
            listing_Standard.Label("Cap By Difficulty Settings Multiplier", -1,
                "Change this number to the base value multiplied by your difficulty. At 4000, the starting multiplier, this means on rough you will end up with 4000 raid point cap, on medium a cap of 2500 or so. 1700 is a good default for medium difficulty.");
            Settings.CapByDifficultySettingsMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.CapByDifficultySettingsMultiplier, 100f, 10000f, false,
                Settings.CapByDifficultySettingsMultiplier.ToString(), null, null, 1);
            listing_Standard.Gap();
            listing_Standard.Label("Raid Points Multiplier", -1,
                "This is not a cap, but just a straight multiplier on top of your difficulty. Maybe you want Rough, but slightly less rough? In that case you can choose rough with your storyteller and choose 0.9 here for example. Or maybe you want extreme's other effects but not it's raid sizes? Choose extreme and 0.5 here.");
            Settings.RaidPointsMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.RaidPointsMultiplier, 0.01f, 5f, false, Settings.RaidPointsMultiplier.ToString(), null, null,
                0.01f);
            listing_Standard.Gap();
            listing_Standard.Label("Raid Cap", -1,
                "If you just want to set it manually, change -1 to a number. 1600 is what v2 uses.");
            Settings.RaidCap = Widgets.HorizontalSlider(listing_Standard.GetRect(20), Settings.RaidCap, -1f, 10000f,
                false, Settings.RaidCap.ToString(), null, null, 1f);
            listing_Standard.Gap();
            listing_Standard.Label("Raid Cap Points Per Colonist", -1,
                "Does what it says. Change from -1 to a number you want if you want to use this and your raid cap will be based off your number of colonists. If you have 25 colonists and you set this to 10, your raids will be capped at 2500");
            Settings.RaidCapPointsPerColonist = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.RaidCapPointsPerColonist, -1f, 200f, false, Settings.RaidCapPointsPerColonist.ToString(), null,
                null, 1);
            listing_Standard.Gap();
            listing_Standard.Label("Colonist Multiplier", -1,
                "The base game adds points for every colonist you recruit. Maybe you don't want that! Change from 1 to 0 and colonists have no impact on raid points. Or choose some positive multiplier for it to work the way you want.");
            Settings.ColonistMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.ColonistMultiplier, 0, 5f, false, Settings.ColonistMultiplier.ToString(), null, null, 0.1f);
            listing_Standard.Gap();
            listing_Standard.Label("Combat Animal Multiplier", -1,
                "Same as Colonist Multiplier but for combat animals.");
            Settings.CombatAnimalMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.CombatAnimalMultiplier, 0, 5f, false, Settings.CombatAnimalMultiplier.ToString(), null, null,
                0.1f);
            listing_Standard.Gap();
            listing_Standard.Label("Wealth Multiplier", -1,
                "Same as Colonist/Animal Multiplier but for wealth. Be careful of tweaking this too much as most of your points come from wealth.");
            Settings.WealthMultiplier = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.WealthMultiplier, 0, 5f, false, Settings.WealthMultiplier.ToString(), null, null, 0.1f);
            listing_Standard.NewColumn();
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Debug mode", ref Settings.Debug,
                "You probably want this when changing values. The Logger should give warning logs for every active setting except some of the per item multipliers using a before and after so you can see how your tags effect the overall raid points.");
            listing_Standard.GapLine();
            Text.Font = GameFont.Tiny;
            listing_Standard.Label("SoftCaps and adaptation probably shouldn't be touched unless you like math ;)");
            Text.Font = GameFont.Small;
            listing_Standard.Gap();
            listing_Standard.Label("Soft Cap Begin Tapering", -1,
                "Soft caps basically have a soft cap at a certain raid value rather than completely cutting it off. Change this from -1 to a raid point amount to start considerably slowing raid points rather than capping them at this value");
            Settings.SoftCapBeginTapering = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.SoftCapBeginTapering, -1f, 10000f, false, Settings.SoftCapBeginTapering.ToString(), null, null,
                1);
            listing_Standard.Gap();
            listing_Standard.Label("Soft Cap Exponent", -1,
                "This is the exponent that will be used if you use this. Example: with 2000 and 0.9, if you had 3000 raid points, it would instead give 2000 + 1000^.9 points");
            Settings.SoftCapExponent = Widgets.HorizontalSlider(listing_Standard.GetRect(20), Settings.SoftCapExponent,
                0.01f, 5f, false, Settings.SoftCapExponent.ToString(), null, null, 0.01f);
            listing_Standard.Gap();
            listing_Standard.Label("Adaptation Cap", -1,
                "I default this to being on. This caps the adaptation so you're never getting raids 50% higher than you otherwise would have. Change it to -1 to let the game do what it does normally, change it to 1 to completely get rid of adaptation altogether.");
            Settings.AdaptationCap = Widgets.HorizontalSlider(listing_Standard.GetRect(20), Settings.AdaptationCap, 0,
                5f, false, Settings.AdaptationCap.ToString(), null, null, 0.1f);
            listing_Standard.Gap();
            listing_Standard.Label("Adaptation Tapering", -1,
                "These work just like the raidsoftcaps only for adaptation.");
            Settings.AdaptationTapering = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.AdaptationTapering, -1f, 10000f, false, Settings.AdaptationTapering.ToString(), null, null, 1);
            listing_Standard.Gap();
            listing_Standard.Label("Adaptation Exponent", -1,
                "These really aren't useful. I don't know why I added them lol, since the game already massively slows down adaptation after a point. If you want to use it knock yourself out.");
            Settings.AdaptationExponent = Widgets.HorizontalSlider(listing_Standard.GetRect(20),
                Settings.AdaptationExponent, 0.01f, 5f, false, Settings.AdaptationExponent.ToString(), null, null,
                0.01f);
            listing_Standard.Gap();
            if (listing_Standard.ButtonText("Reset values"))
            {
                Main.ResetSettings(ref settings);
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
}