using Verse;

namespace RaidLimiter;

internal static class MyLog
{
    public static void Log(string message)
    {
        if (LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>().Debug)
        {
            Verse.Log.Warning("[RaidLimiter]: " + message);
        }
    }
}