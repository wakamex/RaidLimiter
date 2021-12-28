using Verse;

namespace RaidLimiter;

internal class RaidLimiterSettingsDef : Def
{
    public float AdaptationCap;

    public float AdaptationExponent;

    public float AdaptationTapering;

    public string CapByDifficultySettings;

    public float CapByDifficultySettingsMultiplier;

    public float ColonistMultiplier;

    public float CombatAnimalMultiplier;

    public string Debug;

    public float RaidCap;

    public float RaidCapPointsPerColonist;

    public float RaidPointsMultiplier;

    public float SoftCapBeginTapering;

    public float SoftCapExponent;

    public float WealthMultiplier;
}