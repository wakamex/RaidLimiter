using HarmonyLib;
using RimWorld;
using Verse;

namespace RaidLimiter
{
    // Token: 0x02000005 RID: 5
    [HarmonyPatch(typeof(StorytellerUtility))]
    [HarmonyPatch("DefaultParmsNow")]
    [HarmonyPatch(new[]
    {
        typeof(IncidentCategoryDef),
        typeof(IIncidentTarget)
    })]
    internal class RaidLimiterDebug
    {
        // Token: 0x06000005 RID: 5 RVA: 0x000020CC File Offset: 0x000002CC
        private static bool Prefix(IncidentCategoryDef incCat, IIncidentTarget target, ref IncidentParms __result)
        {
            bool result;
            if (!LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>().Debug)
            {
                result = true;
            }
            else
            {
                MyLog.Log("Using Raid Limiter");
                var flag2 = incCat == null;
                if (flag2)
                {
                    Log.Warning("Trying to get default parms for null incident category.");
                }

                var incidentParms = new IncidentParms
                {
                    target = target
                };
                var needsParmsPoints = incCat != null && incCat.needsParmsPoints;
                if (needsParmsPoints)
                {
                    var typeFromHandle = typeof(StorytellerUtility);
                    var method = typeFromHandle.GetMethod("DefaultThreatPointsNow");
                    if (!(method is null))
                    {
                        incidentParms.points = (float) method.Invoke(null, new object[]
                        {
                            target
                        });
                    }

                    MyLog.Log("Final points: " + incidentParms.points);
                }

                __result = incidentParms;
                result = false;
            }

            return result;
        }
    }
}