using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RaidLimiter
{
	// Token: 0x02000006 RID: 6
	[HarmonyPatch(typeof(StorytellerUtility))]
	[HarmonyPatch("DefaultThreatPointsNow")]
	[HarmonyPatch(new Type[]
	{
		typeof(IIncidentTarget)
	})]
	internal class RaidLimiter
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000021A0 File Offset: 0x000003A0
		private static bool Prefix(IIncidentTarget target, ref float __result)
		{
			var simpleCurve = new SimpleCurve
			{
				{
					new CurvePoint(0f, 0f),
					true
				},
				{
					new CurvePoint(300000f, 1800f),
					true
				},
				{
					new CurvePoint(600000f, 3000f),
					true
				},
				{
					new CurvePoint(900000f, 3600f),
					true
				}
			};
			var simpleCurve2 = new SimpleCurve
			{
				{
					new CurvePoint(0f, 40f),
					true
				},
				{
					new CurvePoint(300000f, 110f),
					true
				}
			};
			var simpleCurve3 = new SimpleCurve
			{
				{
					new CurvePoint(0f, 35f),
					true
				},
				{
					new CurvePoint(100f, 35f),
					true
				},
				{
					new CurvePoint(1000f, 700f),
					true
				},
				{
					new CurvePoint(2000f, 1400f),
					true
				},
				{
					new CurvePoint(3000f, 2100f),
					true
				},
				{
					new CurvePoint(4000f, 2800f),
					true
				},
				{
					new CurvePoint(5000f, 3500f),
					true
				},
				{
					new CurvePoint(6000f, 4000f),
					true
				}
			};
			RaidLimiterSettingsDef named = DefDatabase<RaidLimiterSettingsDef>.GetNamed("Settings", true);
			var playerWealthForStoryteller = target.PlayerWealthForStoryteller;
			var num = simpleCurve.Evaluate(playerWealthForStoryteller);
			MyLog.Log("Player Wealth Contribution: " + num.ToString());
			num *= named.WealthMultiplier;
			MyLog.Log("Player Wealth After Multiplier: " + num.ToString());
			var num2 = 0f;
			var num3 = 0;
			foreach (Pawn pawn in target.PlayerPawnsForStoryteller)
			{
				var num4 = 0f;
				var isFreeColonist = pawn.IsFreeColonist;
				if (isFreeColonist)
				{
					num4 = simpleCurve2.Evaluate(playerWealthForStoryteller) * named.ColonistMultiplier;
					num3++;
				}
				else
				{
					var flag = pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && !pawn.Downed && pawn.training.CanAssignToTrain(TrainableDefOf.Release).Accepted;
					if (flag)
					{
						num4 = 0.09f * pawn.kindDef.combatPower;
						var flag2 = target is Caravan;
						if (flag2)
						{
							num4 *= 0.5f;
						}
						num4 *= named.CombatAnimalMultiplier;
					}
				}
				var flag3 = num4 > 0f;
				if (flag3)
				{
					var flag4 = pawn.ParentHolder != null && pawn.ParentHolder is Building_CryptosleepCasket;
					if (flag4)
					{
						num4 *= 0.3f;
					}
					num4 = Mathf.Lerp(num4, num4 * pawn.health.summaryHealth.SummaryHealthPercent, 0.65f);
					num2 += num4;
				}
			}
			var num5 = num + num2;
			num5 *= target.IncidentPointsRandomFactorRange.RandomInRange;
			num5 = simpleCurve3.Evaluate(num5);
			var num6 = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
			var flag5 = named.AdaptationTapering > 0f && num6 > named.AdaptationTapering;
			if (flag5)
			{
				MyLog.Log("adaptation Before AdaptationTapering: " + num6.ToString());
				num6 = named.AdaptationTapering = (float)Math.Pow((double)(num6 - named.AdaptationTapering), (double)named.AdaptationExponent);
				MyLog.Log("adaptation after AdaptationTapering: " + num6.ToString());
			}
			var flag6 = named.AdaptationCap > 0f && num6 > named.AdaptationCap;
			if (flag6)
			{
				MyLog.Log("adaptation Before AdaptationCap: " + num6.ToString());
				num6 = named.AdaptationCap;
				MyLog.Log("adaptation Before AdaptationCap: " + num6.ToString());
			}
			num5 *= num6;
			num5 *= Find.Storyteller.difficulty.threatScale;
			MyLog.Log("Before RaidPointsMultiplier: " + num5.ToString());
			num5 *= named.RaidPointsMultiplier;
			MyLog.Log("After RaidPointsMultiplier: " + num5.ToString());
			var flag7 = named.SoftCapBeginTapering > 0f && num5 > named.SoftCapBeginTapering;
			if (flag7)
			{
				MyLog.Log("Before SoftCapBeginTapering: " + num5.ToString());
				num5 = named.SoftCapBeginTapering = (float)Math.Pow((double)(num5 - named.SoftCapBeginTapering), (double)named.SoftCapExponent);
				MyLog.Log("After SoftCapBeginTapering: " + num5.ToString());
			}
			var flag8 = named.RaidCapPointsPerColonist > 0f;
			if (flag8)
			{
				MyLog.Log("Before RaidCapPointsPerColonist: " + num5.ToString());
				num5 = Math.Min(num5, named.RaidCapPointsPerColonist * (float)num3);
				MyLog.Log("After RaidCapPointsPerColonist: " + num5.ToString());
			}
			var flag9 = named.RaidCap > 0f;
			if (flag9)
			{
				MyLog.Log("Before RaidCap: " + num5.ToString());
				num5 = Math.Min(named.RaidCap, num5);
				MyLog.Log("After RaidCap: " + num5.ToString());
			}
			var flag10 = named.CapByDifficultySettings == "YES";
			if (flag10)
			{
				MyLog.Log("Before CapByDifficultySettings: " + num5.ToString());
				num5 = Math.Min(num5 * named.CapByDifficultySettingsMultiplier * Find.Storyteller.difficulty.threatScale, num5);
				MyLog.Log("After CapByDifficultySettings: " + num5.ToString());
			}
			__result = num5;
			return false;
		}
	}
}
