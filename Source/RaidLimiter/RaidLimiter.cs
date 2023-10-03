using System;
using System.Text;
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
        var PointsPerWealthCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(14000f, 0f),
            new CurvePoint(400000f, 2400f),
            new CurvePoint(700000f, 3600f),
            new CurvePoint(1000000f, 4200f)
        };
        var PointsPerColonistByWealthCurve = new SimpleCurve
        {
            new CurvePoint(0f, 15f),
            new CurvePoint(10000f, 15f),
            new CurvePoint(400000f, 140f),
            new CurvePoint(1000000f, 200f)
        };
        var PointsFactorForColonyMechsCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.2f),
            new CurvePoint(10000f, 0.2f),
            new CurvePoint(400000f, 0.3f),
            new CurvePoint(1000000f, 0.4f)
        };
        var PointsFactorForPawnAgeYearsCurve = new SimpleCurve
        {
            new CurvePoint(3f, 0f),
            new CurvePoint(13f, 0.5f),
            new CurvePoint(18f, 1f)
        };

		StringBuilder stringBuilder = new StringBuilder();

        var playerWealthForStoryteller = target.PlayerWealthForStoryteller;
        float player_wealth_source = PointsPerWealthCurve.Evaluate(playerWealthForStoryteller);
        var player_wealth = simpleCurve.Evaluate(playerWealthForStoryteller);
        var dynamicSettings = LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>();
        stringBuilder.AppendLine("=== RAID ANALYSIS ===");
        stringBuilder.AppendLine("Player Wealth Contribution: " + player_wealth);
        // MyLog.Log($"Player Wealth Contribution: {player_wealth}");
        player_wealth *= dynamicSettings.WealthMultiplier;
        stringBuilder.AppendLine("Player Wealth After Multiplier: " + player_wealth);
        // MyLog.Log($"Player Wealth After Multiplier: {player_wealth}");
        stringBuilder.AppendLine("Player Wealth (Source): " + player_wealth_source);
        // MyLog.Log($"Player Wealth (Source): {player_wealth_source}");
        var pawn_wealth_total = 0f;
        var pawn_wealth_total_source = 0f;
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

            // BEGIN source code
            if (pawn.IsQuestLodger())
            {
                continue;
            }
            float pawn_wealth_source = 0f;
            if (pawn.IsFreeColonist)
            {
                pawn_wealth_source = PointsPerColonistByWealthCurve.Evaluate(playerWealthForStoryteller);
            }
            else if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && !pawn.Downed && pawn.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
            {
                pawn_wealth_source = 0.08f * pawn.kindDef.combatPower;
                if (target is Caravan)
                {
                    pawn_wealth_source *= 0.7f;
                }
            }
            else if (pawn.IsColonyMech && !pawn.Downed)
            {
                pawn_wealth_source = pawn.kindDef.combatPower * PointsFactorForColonyMechsCurve.Evaluate(playerWealthForStoryteller);
            }
            if (pawn_wealth_source > 0f)
            {
                if (pawn.ParentHolder != null && pawn.ParentHolder is Building_CryptosleepCasket)
                {
                    pawn_wealth_source *= 0.3f;
                } 
                pawn_wealth_source = Mathf.Lerp(pawn_wealth_source, pawn_wealth_source * pawn.health.summaryHealth.SummaryHealthPercent, 0.65f);
                if (pawn.IsSlaveOfColony)
                {
                    pawn_wealth_source *= 0.75f;
                }
                if (ModsConfig.BiotechActive && pawn.RaceProps.Humanlike)
                {
                    pawn_wealth_source *= PointsFactorForPawnAgeYearsCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
                }
                pawn_wealth_total_source += pawn_wealth_source;
            }
            stringBuilder.AppendLine(pawn + " original: " + pawn_wealth + " source: " + pawn_wealth_source);
            // MyLog.Log($"{pawn} original: {pawn_wealth} source: {pawn_wealth_source}");
        }

        stringBuilder.AppendLine("total pawn points (original): " + pawn_wealth_total);
        stringBuilder.AppendLine("total pawn points   (source): " + pawn_wealth_total_source);
        // MyLog.Log($"total pawn points (original): {pawn_wealth_total}");
        // MyLog.Log($"total pawn points   (source): {pawn_wealth_total_source}");
        var raid_points = player_wealth + pawn_wealth_total;
        var raid_points_source = player_wealth_source + pawn_wealth_total_source;

        stringBuilder.AppendLine("raid points subtotal (original): " + raid_points + "(" + pawn_wealth_total/raid_points*100 + "% pawns)");
        stringBuilder.AppendLine("raid points subtotal   (source): " + raid_points_source + "(" + pawn_wealth_total_source/raid_points_source*100 + "% pawns)");
        // MyLog.Log($"raid points subtotal (original): {raid_points} ({pawn_wealth_total/raid_points}% pawns)");
        // MyLog.Log($"raid points subtotal   (source): {raid_points_source} ({pawn_wealth_total_source/raid_points_source}% pawns)");
        raid_points *= target.IncidentPointsRandomFactorRange.RandomInRange;
        raid_points = simpleCurve3.Evaluate(raid_points);
        var adaptation_factor = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
        if (dynamicSettings.AdaptationTapering > 0f && adaptation_factor > dynamicSettings.AdaptationTapering)
        {
            stringBuilder.AppendLine("adaptation before AdaptationTapering: " + adaptation_factor);
            // MyLog.Log($"adaptation Before AdaptationTapering: {adaptation_factor}");
            adaptation_factor = dynamicSettings.AdaptationTapering =
                (float)Math.Pow(adaptation_factor - dynamicSettings.AdaptationTapering, dynamicSettings.AdaptationExponent);
            stringBuilder.AppendLine("adaptation  after AdaptationTapering: " + adaptation_factor);
            // MyLog.Log($"adaptation after AdaptationTapering: {adaptation_factor}");
        }

        if (dynamicSettings.AdaptationCap > 0f && adaptation_factor > dynamicSettings.AdaptationCap)
        {
            stringBuilder.AppendLine("adaptation before AdaptationCap: " + adaptation_factor);
            // MyLog.Log($"adaptation Before AdaptationCap: {adaptation_factor}");
            adaptation_factor = dynamicSettings.AdaptationCap;
            stringBuilder.AppendLine("adaptation  after AdaptationCap: " + adaptation_factor);
            // MyLog.Log($"adaptation Before AdaptationCap: {adaptation_factor}");
        }

        float totalThreatPointsFactor = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
        var adaptation_factor_source = Mathf.Lerp(1f, totalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor);
        stringBuilder.AppendLine("Adaptation factor (original): " + adaptation_factor);
        stringBuilder.AppendLine("Adaptation factor   (source): " + adaptation_factor_source);

        raid_points *= adaptation_factor;
        stringBuilder.AppendLine("with Adaptation Factor (original): " + raid_points);
        stringBuilder.AppendLine("with Adaptation Factor   (source): " + raid_points_source * adaptation_factor_source);
        raid_points *= Find.Storyteller.difficulty.threatScale;
        stringBuilder.AppendLine("Before RaidPointsMultiplier: " + raid_points);
        // MyLog.Log($"Before RaidPointsMultiplier: {raid_points}");
        raid_points *= dynamicSettings.RaidPointsMultiplier;
        stringBuilder.AppendLine(" After RaidPointsMultiplier: " + raid_points);
        // MyLog.Log($"After RaidPointsMultiplier: {raid_points}");
        if (dynamicSettings.SoftCapBeginTapering > 0f && raid_points > dynamicSettings.SoftCapBeginTapering)
        {
            stringBuilder.AppendLine("Before SoftCapBeginTapering: " + raid_points);
            // MyLog.Log($"Before SoftCapBeginTapering: {raid_points}");
            raid_points = dynamicSettings.SoftCapBeginTapering =
                (float)Math.Pow(raid_points - dynamicSettings.SoftCapBeginTapering, dynamicSettings.SoftCapExponent);
            stringBuilder.AppendLine(" After SoftCapBeginTapering: " + raid_points);
            // MyLog.Log($"After SoftCapBeginTapering: {raid_points}");
        }

        if (dynamicSettings.RaidCapPointsPerColonist > 0f)
        {
            stringBuilder.AppendLine("Before RaidCapPointsPerColonist: " + raid_points);
            // MyLog.Log($"Before RaidCapPointsPerColonist: {raid_points}");
            raid_points = Math.Min(raid_points, dynamicSettings.RaidCapPointsPerColonist * num_colonists);
            stringBuilder.AppendLine(" After RaidCapPointsPerColonist: " + raid_points);
            // MyLog.Log($"After RaidCapPointsPerColonist: {raid_points}");
        }

        if (dynamicSettings.RaidCap > 0f)
        {
            stringBuilder.AppendLine("Before RaidCap: " + raid_points);
            // MyLog.Log($"Before RaidCap: {raid_points}");
            raid_points = Math.Min(dynamicSettings.RaidCap, raid_points);
            stringBuilder.AppendLine(" After RaidCap: " + raid_points);
            // MyLog.Log($"After RaidCap: {raid_points}");
        }

        if (dynamicSettings.CapByDifficultySettings)
        {
            stringBuilder.AppendLine("Before CapByDifficultySettings: " + raid_points);
            // MyLog.Log($"Before CapByDifficultySettings: {raid_points}");
            raid_points = Math.Min(
                raid_points * dynamicSettings.CapByDifficultySettingsMultiplier * Find.Storyteller.difficulty.threatScale,
                raid_points);
            stringBuilder.AppendLine(" After CapByDifficultySettings: " + raid_points);
            // MyLog.Log($"After CapByDifficultySettings: {raid_points}");
        }

        // MyLog.Log($"Adaptation factor (original): {adaptation_factor}");
        // MyLog.Log($"Adaptation factor   (source): {adaptation_factor_source}");
        var GlobalPointsMin = Rand.RangeSeeded(35f, Find.Storyteller.difficulty.MinThreatPointsCeiling, Find.TickManager.TicksGame / 2500);
        raid_points_source = Mathf.Clamp(raid_points_source * adaptation_factor * Find.Storyteller.difficulty.threatScale * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate(GenDate.DaysPassedSinceSettle), GlobalPointsMin, 10000f);

        stringBuilder.AppendLine("Final raid points (original): " + raid_points);
        stringBuilder.AppendLine("Final raid points   (source): " + raid_points_source);
        MyLog.Log(stringBuilder.ToString());
        // MyLog.Log($"Final raid points (original): {raid_points}");
        // MyLog.Log($"Final raid points   (source): {raid_points_source}");

        __result = raid_points;
        return false;
    }
}