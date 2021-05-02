using Verse;

namespace RaidLimiter
{
    /// <summary>
    ///     Definition of the settings for the mod
    /// </summary>
    internal class RaidLimiterSettings : ModSettings
    {
        public float AdaptationCap;
        public float AdaptationExponent;
        public float AdaptationTapering;
        public bool CapByDifficultySettings;
        public float CapByDifficultySettingsMultiplier;
        public float ColonistMultiplier;
        public float CombatAnimalMultiplier;
        public bool Debug;
        public bool HasLoadedSettingsBefore;
        public float RaidCap;
        public float RaidCapPointsPerColonist;
        public float RaidPointsMultiplier;
        public float SoftCapBeginTapering;
        public float SoftCapExponent;
        public float WealthMultiplier;

        /// <summary>
        ///     Saving and loading the values
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref CapByDifficultySettings, "CapByDifficultySettings");
            Scribe_Values.Look(ref CapByDifficultySettingsMultiplier, "CapByDifficultySettingsMultiplier");
            Scribe_Values.Look(ref RaidCap, "RaidCap");
            Scribe_Values.Look(ref RaidPointsMultiplier, "RaidPointsMultiplier");
            Scribe_Values.Look(ref RaidCapPointsPerColonist, "RaidCapPointsPerColonist");
            Scribe_Values.Look(ref ColonistMultiplier, "ColonistMultiplier");
            Scribe_Values.Look(ref CombatAnimalMultiplier, "CombatAnimalMultiplier");
            Scribe_Values.Look(ref WealthMultiplier, "WealthMultiplier");
            Scribe_Values.Look(ref SoftCapBeginTapering, "SoftCapBeginTapering");
            Scribe_Values.Look(ref SoftCapExponent, "SoftCapExponent");
            Scribe_Values.Look(ref AdaptationCap, "AdaptationCap");
            Scribe_Values.Look(ref AdaptationTapering, "AdaptationTapering");
            Scribe_Values.Look(ref AdaptationExponent, "AdaptationExponent");
            Scribe_Values.Look(ref Debug, "Debug");
            Scribe_Values.Look(ref HasLoadedSettingsBefore, "HasLoadedSettingsBefore");
        }
    }
}