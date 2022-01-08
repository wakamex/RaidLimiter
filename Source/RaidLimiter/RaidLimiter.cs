using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RaidLimiter;

[HarmonyPatch(typeof(StorytellerUtility))]
[HarmonyPatch("DefaultThreatPointsNow")]
[HarmonyPatch(new[]
{
    typeof(IIncidentTarget)
})]
internal class RaidLimiter
{
    private static bool Prefix(IIncidentTarget target, ref float __result)
    {
        var simpleCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(300000f, 1800f),
            new CurvePoint(600000f, 3000f),
            new CurvePoint(900000f, 3600f)
        };
        var simpleCurve2 = new SimpleCurve
        {
            new CurvePoint(0f, 40f),
            new CurvePoint(300000f, 110f)
        };
        var simpleCurve3 = new SimpleCurve
        {
            new CurvePoint(0f, 35f),
            new CurvePoint(100f, 35f),
            new CurvePoint(1000f, 700f),
            new CurvePoint(2000f, 1400f),
            new CurvePoint(3000f, 2100f),
            new CurvePoint(4000f, 2800f),
            new CurvePoint(5000f, 3500f),
            new CurvePoint(6000f, 4000f)
        };
        var playerWealthForStoryteller = target.PlayerWealthForStoryteller;
        var num = simpleCurve.Evaluate(playerWealthForStoryteller);
        var dynamicSettings = LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>();
        MyLog.Log("Player Wealth Contribution: " + num);
        num *= dynamicSettings.WealthMultiplier;
        MyLog.Log("Player Wealth After Multiplier: " + num);
        var num2 = 0f;
        var num3 = 0;
        foreach (var pawn in target.PlayerPawnsForStoryteller)
        {
            var num4 = 0f;
            var isFreeColonist = pawn.IsFreeColonist;
            if (isFreeColonist)
            {
                num4 = simpleCurve2.Evaluate(playerWealthForStoryteller) * dynamicSettings.ColonistMultiplier;
                num3++;
            }
            else
            {
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && !pawn.Downed &&
                    pawn.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
                {
                    num4 = 0.09f * pawn.kindDef.combatPower;
                    if (target is Caravan)
                    {
                        num4 *= 0.5f;
                    }

                    num4 *= dynamicSettings.CombatAnimalMultiplier;
                }
            }

            if (!(num4 > 0f))
            {
                continue;
            }

            if (pawn.ParentHolder is Building_CryptosleepCasket)
            {
                num4 *= 0.3f;
            }

            num4 = Mathf.Lerp(num4, num4 * pawn.health.summaryHealth.SummaryHealthPercent, 0.65f);
            num2 += num4;
        }

        var num5 = num + num2;
        num5 *= target.IncidentPointsRandomFactorRange.RandomInRange;
        num5 = simpleCurve3.Evaluate(num5);
        var num6 = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
        if (dynamicSettings.AdaptationTapering > 0f && num6 > dynamicSettings.AdaptationTapering)
        {
            MyLog.Log("adaptation Before AdaptationTapering: " + num6);
            num6 = dynamicSettings.AdaptationTapering =
                (float)Math.Pow(num6 - dynamicSettings.AdaptationTapering, dynamicSettings.AdaptationExponent);
            MyLog.Log("adaptation after AdaptationTapering: " + num6);
        }

        if (dynamicSettings.AdaptationCap > 0f && num6 > dynamicSettings.AdaptationCap)
        {
            MyLog.Log("adaptation Before AdaptationCap: " + num6);
            num6 = dynamicSettings.AdaptationCap;
            MyLog.Log("adaptation Before AdaptationCap: " + num6);
        }

        num5 *= num6;
        num5 *= Find.Storyteller.difficulty.threatScale;
        MyLog.Log("Before RaidPointsMultiplier: " + num5);
        num5 *= dynamicSettings.RaidPointsMultiplier;
        MyLog.Log("After RaidPointsMultiplier: " + num5);
        if (dynamicSettings.SoftCapBeginTapering > 0f && num5 > dynamicSettings.SoftCapBeginTapering)
        {
            MyLog.Log("Before SoftCapBeginTapering: " + num5);
            num5 = dynamicSettings.SoftCapBeginTapering =
                (float)Math.Pow(num5 - dynamicSettings.SoftCapBeginTapering, dynamicSettings.SoftCapExponent);
            MyLog.Log("After SoftCapBeginTapering: " + num5);
        }

        if (dynamicSettings.RaidCapPointsPerColonist > 0f)
        {
            MyLog.Log("Before RaidCapPointsPerColonist: " + num5);
            num5 = Math.Min(num5, dynamicSettings.RaidCapPointsPerColonist * num3);
            MyLog.Log("After RaidCapPointsPerColonist: " + num5);
        }

        if (dynamicSettings.RaidCap > 0f)
        {
            MyLog.Log("Before RaidCap: " + num5);
            num5 = Math.Min(dynamicSettings.RaidCap, num5);
            MyLog.Log("After RaidCap: " + num5);
        }

        if (dynamicSettings.CapByDifficultySettings)
        {
            MyLog.Log("Before CapByDifficultySettings: " + num5);
            num5 = Math.Min(
                num5 * dynamicSettings.CapByDifficultySettingsMultiplier * Find.Storyteller.difficulty.threatScale,
                num5);
            MyLog.Log("After CapByDifficultySettings: " + num5);
        }

        __result = num5;
        return false;
    }
}