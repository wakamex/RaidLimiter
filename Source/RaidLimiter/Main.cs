using System.Reflection;
using HarmonyLib;
using Verse;

namespace RaidLimiter
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    internal class Main
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        static Main()
        {
            var dynamicSettings = LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>();
            if (!dynamicSettings.HasLoadedSettingsBefore)
            {
                ResetSettings(ref dynamicSettings);
                dynamicSettings.HasLoadedSettingsBefore = true;
                dynamicSettings.Write();
            }

            var harmonyInstance = new Harmony("com.whocares.whatisthis.rimworld.mod.stuff");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void ResetSettings(ref RaidLimiterSettings dynamicSettings)
        {
            var staticSettings = DefDatabase<RaidLimiterSettingsDef>.GetNamed("Settings");
            dynamicSettings.CapByDifficultySettings = staticSettings.CapByDifficultySettings == "YES";
            dynamicSettings.CapByDifficultySettingsMultiplier = staticSettings.CapByDifficultySettingsMultiplier;
            dynamicSettings.RaidCap = staticSettings.RaidCap;
            dynamicSettings.RaidPointsMultiplier = staticSettings.RaidPointsMultiplier;
            dynamicSettings.RaidCapPointsPerColonist = staticSettings.RaidCapPointsPerColonist;
            dynamicSettings.ColonistMultiplier = staticSettings.ColonistMultiplier;
            dynamicSettings.CombatAnimalMultiplier = staticSettings.CombatAnimalMultiplier;
            dynamicSettings.WealthMultiplier = staticSettings.WealthMultiplier;
            dynamicSettings.SoftCapBeginTapering = staticSettings.SoftCapBeginTapering;
            dynamicSettings.SoftCapExponent = staticSettings.SoftCapExponent;
            dynamicSettings.AdaptationCap = staticSettings.AdaptationCap;
            dynamicSettings.AdaptationTapering = staticSettings.AdaptationTapering;
            dynamicSettings.AdaptationExponent = staticSettings.AdaptationExponent;
            dynamicSettings.Debug = staticSettings.Debug == "YES";
        }
    }
}