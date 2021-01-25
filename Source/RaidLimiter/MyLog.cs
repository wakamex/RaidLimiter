using Verse;

namespace RaidLimiter
{
    // Token: 0x02000004 RID: 4
    internal static class MyLog
	{
		// Token: 0x06000004 RID: 4 RVA: 0x00002088 File Offset: 0x00000288
		public static void Log(string message)
		{
			RaidLimiterSettingsDef named = DefDatabase<RaidLimiterSettingsDef>.GetNamed("Settings", true);
			var flag = named.Debug == "YES";
			if (flag)
			{
				Verse.Log.Warning("[RaidLimiter]: " + message, false);
			}
		}
	}
}
