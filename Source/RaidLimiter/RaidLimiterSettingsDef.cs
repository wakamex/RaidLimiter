using Verse;

namespace RaidLimiter
{
    // Token: 0x02000003 RID: 3
    internal class RaidLimiterSettingsDef : Def
	{
		// Token: 0x04000001 RID: 1
		public float RaidCap;

		// Token: 0x04000002 RID: 2
		public string CapByDifficultySettings;

		// Token: 0x04000003 RID: 3
		public float CapByDifficultySettingsMultiplier;

		// Token: 0x04000004 RID: 4
		public float RaidPointsMultiplier;

		// Token: 0x04000005 RID: 5
		public float RaidCapPointsPerColonist;

		// Token: 0x04000006 RID: 6
		public float ColonistMultiplier;

		// Token: 0x04000007 RID: 7
		public float CombatAnimalMultiplier;

		// Token: 0x04000008 RID: 8
		public float WealthMultiplier;

		// Token: 0x04000009 RID: 9
		public float SoftCapBeginTapering;

		// Token: 0x0400000A RID: 10
		public float SoftCapExponent;

		// Token: 0x0400000B RID: 11
		public float AdaptationCap;

		// Token: 0x0400000C RID: 12
		public float AdaptationTapering;

		// Token: 0x0400000D RID: 13
		public float AdaptationExponent;

		// Token: 0x0400000E RID: 14
		public string Debug;
	}
}
