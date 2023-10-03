using System;
using System.Text;
using System.Collections.Generic;
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

    private static ValueTuple<float, string> CalculatePawnWealth(Pawn pawn, SimpleCurve colByWealthCurve, SimpleCurve mechFactorCurve, SimpleCurve ageCurve, IIncidentTarget target, float playerWealth)
    {
        float wealth = 0f;
        string category = "Other";
        if (pawn.IsFreeColonist) { wealth = colByWealthCurve.Evaluate(playerWealth); category = "Adult"; }
        else if (pawn.IsSlaveOfColony) { wealth = colByWealthCurve.Evaluate(playerWealth) * 0.75f; category = "Slave"; }
        else if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && !pawn.Downed && pawn.training.CanAssignToTrain(TrainableDefOf.Release).Accepted) {
            wealth = 0.08f * pawn.kindDef.combatPower * ((target is Caravan) ? 0.7f : 1f);
            category = "Animal";
        }
        else if (pawn.IsColonyMech && !pawn.Downed) { wealth = pawn.kindDef.combatPower * mechFactorCurve.Evaluate(playerWealth); category = "Mech"; }

        if (wealth > 0f) {
            wealth *= (pawn.ParentHolder is Building_CryptosleepCasket) ? 0.3f : 1f;
            wealth = Mathf.Lerp(wealth, wealth * pawn.health.summaryHealth.SummaryHealthPercent, 0.65f);
            if (ModsConfig.BiotechActive && pawn.RaceProps.Humanlike) wealth *= ageCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
        }
        if (pawn.ageTracker.AgeBiologicalYearsFloat < 18f) category = "Teen";
        else if (pawn.ageTracker.AgeBiologicalYearsFloat < 13f) category = "Child";
        else if (pawn.ageTracker.AgeBiologicalYearsFloat < 3f) category = "Baby";
        return (wealth, category);
    }

    private static bool Prefix(IIncidentTarget target, ref float __result)
    {
        var PointsPerWealthCurve = new SimpleCurve { {0f, 0f}, {14000f, 0f}, {400000f, 2400f}, {700000f, 3600f}, {1000000f, 4200f} };
        var PointsPerColonistByWealthCurve = new SimpleCurve { {0f, 15f}, {10000f, 15f}, {400000f, 140f}, {1000000f, 200f} };
        var PointsFactorForColonyMechsCurve = new SimpleCurve { {0f, 0.2f}, {10000f, 0.2f}, {400000f, 0.3f}, {1000000f, 0.4f} };
        var PointsFactorForPawnAgeYearsCurve = new SimpleCurve { {3f, 0f}, {13f, 0.5f}, {18f, 1f} };

		StringBuilder sb = new StringBuilder();
        var settings = LoadedModManager.GetMod<RaidLimiterMod>().GetSettings<RaidLimiterSettings>();
        float playerWealth = PointsPerWealthCurve.Evaluate(target.PlayerWealthForStoryteller) * settings.WealthMultiplier;
        sb.AppendLine($"=== RAID ANALYSIS ===\nPlayer Wealth: {playerWealth}");
        
        float pawnWealthTotal = 0f;
        Dictionary<string, float> categorySubtotals = new Dictionary<string, float> { {"Adult", 0}, {"Slave", 0}, {"Animal", 0}, {"Mech", 0}, {"Teen", 0}, {"Child", 0}, {"Baby", 0}, {"Other", 0} };
        foreach (var pawn in target.PlayerPawnsForStoryteller)
        {
            if (pawn.IsQuestLodger()) continue;
            (float pawnWealth, string category) = CalculatePawnWealth(pawn, PointsPerColonistByWealthCurve, PointsFactorForColonyMechsCurve, PointsFactorForPawnAgeYearsCurve, target, playerWealth);
            categorySubtotals[category] += pawnWealth;
            sb.AppendLine($" PAWN {pawn}: {pawnWealth}");
            pawnWealthTotal += pawnWealth;
        }
        sb.AppendLine("raid points subtotal: " + playerWealth + pawnWealthTotal + "(" + pawnWealthTotal/(playerWealth + pawnWealthTotal)*100 + "% pawns, "+playerWealth+" wealth"+pawnWealthTotal+" pawns)");
        foreach (var kvp in categorySubtotals) sb.AppendLine($" {kvp.Key.PadRight(20)} {kvp.Value}");
        
        float adaptationFactor = Mathf.Lerp(1f, Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor);
        float raidPoints = Mathf.Clamp((playerWealth + pawnWealthTotal) * adaptationFactor * Find.Storyteller.difficulty.threatScale * Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate(GenDate.DaysPassedSinceSettle), Rand.RangeSeeded(35f, Find.Storyteller.difficulty.MinThreatPointsCeiling, Find.TickManager.TicksGame / 2500), 10000f);
        sb.AppendLine("Final raid points: " + raidPoints + "(Adaptation: " + adaptationFactor + ")");
        MyLog.Log(sb.ToString());

        __result = raidPoints;
        return false;
    }
}