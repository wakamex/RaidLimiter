using Verse;

namespace RaidLimiter
{
    // Token: 0x02000004 RID: 4
    internal static class MyLog
    {
        // Token: 0x06000004 RID: 4 RVA: 0x00002088 File Offset: 0x00000288
        public static void Log(string message)
        {
            if (LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>().Debug)
            {
                Verse.Log.Warning("[RaidLimiter]: " + message);
            }
        }
    }
}