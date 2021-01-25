using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RaidLimiter
{
	// Token: 0x02000005 RID: 5
	[HarmonyPatch(typeof(StorytellerUtility))]
	[HarmonyPatch("DefaultParmsNow")]
	[HarmonyPatch(new Type[]
	{
		typeof(IncidentCategoryDef),
		typeof(IIncidentTarget)
	})]
	internal class RaidLimiterDebug
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020CC File Offset: 0x000002CC
		private static bool Prefix(IncidentCategoryDef incCat, IIncidentTarget target, ref IncidentParms __result)
		{
			RaidLimiterSettingsDef named = DefDatabase<RaidLimiterSettingsDef>.GetNamed("Settings", true);
			var debug = named.Debug;
			var flag = debug != "YES";
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				MyLog.Log("Using Raid Limiter");
				var flag2 = incCat == null;
				if (flag2)
				{
					Log.Warning("Trying to get default parms for null incident category.", false);
				}
                var incidentParms = new IncidentParms
                {
                    target = target
                };
                var needsParmsPoints = incCat.needsParmsPoints;
				if (needsParmsPoints)
				{
					Type typeFromHandle = typeof(StorytellerUtility);
					MethodInfo method = typeFromHandle.GetMethod("DefaultThreatPointsNow");
					incidentParms.points = (float)method.Invoke(null, new object[]
					{
						target
					});
					MyLog.Log("Final points: " + incidentParms.points);
				}
				__result = incidentParms;
				result = false;
			}
			return result;
		}
	}
}
