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
        var player_wealth = simpleCurve.Evaluate(playerWealthForStoryteller);
        var dynamicSettings = LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>();
        MyLog.Log($"Player Wealth Contribution: {player_wealth}");
        player_wealth *= dynamicSettings.WealthMultiplier;
        MyLog.Log($"Player Wealth After Multiplier: {player_wealth}");
        var pawn_wealth_total = 0f;
        var num_colonists = 0;
        foreach (var pawn in target.PlayerPawnsForStoryteller)
        {
            var pawn_wealth = 0f;
            var isFreeColonist = pawn.IsFreeColonist;
            if (isFreeColonist)
            {
                pawn_wealth = simpleCurve2.Evaluate(playerWealthForStoryteller) * dynamicSettings.ColonistMultiplier;
                num_colonists++;
            }
            else
            {
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && !pawn.Downed &&
                    pawn.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
                {
                    pawn_wealth = 0.09f * pawn.kindDef.combatPower;
                    if (target is Caravan)
                    {
                        pawn_wealth *= 0.5f;
                    }

                    pawn_wealth *= dynamicSettings.CombatAnimalMultiplier;
                }
            }

            if (!(pawn_wealth > 0f))
            {
                continue;
            }

            if (pawn.ParentHolder is Building_CryptosleepCasket)
            {
                pawn_wealth *= 0.3f;
            }

            pawn_wealth = Mathf.Lerp(pawn_wealth, pawn_wealth * pawn.health.summaryHealth.SummaryHealthPercent, 0.65f);
            pawn_wealth_total += pawn_wealth;
        }

        var raid_points = player_wealth + pawn_wealth_total;
        raid_points *= target.IncidentPointsRandomFactorRange.RandomInRange;
        raid_points = simpleCurve3.Evaluate(raid_points);
        var adaptation_factor = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
        if (dynamicSettings.AdaptationTapering > 0f && adaptation_factor > dynamicSettings.AdaptationTapering)
        {
            MyLog.Log($"adaptation Before AdaptationTapering: {adaptation_factor}");
            adaptation_factor = dynamicSettings.AdaptationTapering =
                (float)Math.Pow(adaptation_factor - dynamicSettings.AdaptationTapering, dynamicSettings.AdaptationExponent);
            MyLog.Log($"adaptation after AdaptationTapering: {adaptation_factor}");
        }

        if (dynamicSettings.AdaptationCap > 0f && adaptation_factor > dynamicSettings.AdaptationCap)
        {
            MyLog.Log($"adaptation Before AdaptationCap: {adaptation_factor}");
            adaptation_factor = dynamicSettings.AdaptationCap;
            MyLog.Log($"adaptation Before AdaptationCap: {adaptation_factor}");
        }

        raid_points *= adaptation_factor;
        raid_points *= Find.Storyteller.difficulty.threatScale;
        MyLog.Log($"Before RaidPointsMultiplier: {raid_points}");
        raid_points *= dynamicSettings.RaidPointsMultiplier;
        MyLog.Log($"After RaidPointsMultiplier: {raid_points}");
        if (dynamicSettings.SoftCapBeginTapering > 0f && raid_points > dynamicSettings.SoftCapBeginTapering)
        {
            MyLog.Log($"Before SoftCapBeginTapering: {raid_points}");
            raid_points = dynamicSettings.SoftCapBeginTapering =
                (float)Math.Pow(raid_points - dynamicSettings.SoftCapBeginTapering, dynamicSettings.SoftCapExponent);
            MyLog.Log($"After SoftCapBeginTapering: {raid_points}");
        }

        if (dynamicSettings.RaidCapPointsPerColonist > 0f)
        {
            MyLog.Log($"Before RaidCapPointsPerColonist: {raid_points}");
            raid_points = Math.Min(raid_points, dynamicSettings.RaidCapPointsPerColonist * num_colonists);
            MyLog.Log($"After RaidCapPointsPerColonist: {raid_points}");
        }

        if (dynamicSettings.RaidCap > 0f)
        {
            MyLog.Log($"Before RaidCap: {raid_points}");
            raid_points = Math.Min(dynamicSettings.RaidCap, raid_points);
            MyLog.Log($"After RaidCap: {raid_points}");
        }

        if (dynamicSettings.CapByDifficultySettings)
        {
            MyLog.Log($"Before CapByDifficultySettings: {raid_points}");
            raid_points = Math.Min(
                raid_points * dynamicSettings.CapByDifficultySettingsMultiplier * Find.Storyteller.difficulty.threatScale,
                raid_points);
            MyLog.Log($"After CapByDifficultySettings: {raid_points}");
        }

        __result = raid_points;
        return false;
    }
}