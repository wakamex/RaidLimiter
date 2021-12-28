using HarmonyLib;
using RimWorld;
using Verse;

namespace RaidLimiter;

[HarmonyPatch(typeof(StorytellerUtility))]
[HarmonyPatch("DefaultParmsNow")]
[HarmonyPatch(new[]
{
    typeof(IncidentCategoryDef),
    typeof(IIncidentTarget)
})]
internal class RaidLimiterDebug
{
    private static bool Prefix(IncidentCategoryDef incCat, IIncidentTarget target, ref IncidentParms __result)
    {
        bool result;
        if (!LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>().Debug)
        {
            result = true;
        }
        else
        {
            MyLog.Log("RL.LogTitle.label".Translate());
            if (incCat == null)
            {
                Log.Warning("Trying to get default parms for null incident category.");
            }

            var incidentParms = new IncidentParms
            {
                target = target
            };
            var needsParmsPoints = incCat is { needsParmsPoints: true };
            if (needsParmsPoints)
            {
                var typeFromHandle = typeof(StorytellerUtility);
                var method = typeFromHandle.GetMethod("DefaultThreatPointsNow");
                if (!(method is null))
                {
                    incidentParms.points = (float)method.Invoke(null, new object[]
                    {
                        target
                    });
                }

                MyLog.Log("RL.LogPoints.label".Translate(incidentParms.points));
            }

            __result = incidentParms;
            result = false;
        }

        return result;
    }
}