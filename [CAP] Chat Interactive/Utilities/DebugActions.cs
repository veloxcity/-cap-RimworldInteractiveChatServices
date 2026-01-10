// Copyright (c) Captolamia
// This file is part of CAP Chat Interactive.
// 
// CAP Chat Interactive is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CAP Chat Interactive is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with CAP Chat Interactive. If not, see <https://www.gnu.org/licenses/>.


using CAP_ChatInteractive;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace _CAP__Chat_Interactive.Utilities
{
    // Add this to GeneUtils.cs or your main mod class
    [StaticConstructorOnStartup]
    public static class GeneDebugActions
    {
        static GeneDebugActions()
        {
            // Auto-register debug actions on game start
        }

        [DebugAction("CAP", "Analyze Xenotype Pricing", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugAnalyzeXenotypePricing()
        {
            var options = new List<DebugMenuOption>();

            // First, get a sample race for context (use Human as default)
            var humanRace = ThingDefOf.Human;

            foreach (var xenotype in DefDatabase<XenotypeDef>.AllDefs)
            {
                options.Add(new DebugMenuOption(xenotype.defName, DebugMenuOptionMode.Action, () =>
                {
                    AnalyzeAndLogXenotypePricing(humanRace, xenotype);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("CAP", "Recalculate All Xenotype Prices", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugRecalculateXenotypePrices()
        {
            // Recalculate and update all xenotype prices in race settings
            var raceSettings = JsonFileManager.LoadRaceSettings();
            bool updated = false;

            foreach (var raceSetting in raceSettings.Values)
            {
                // Find the race def for this setting
                var raceDef = RaceUtils.FindRaceByName(raceSetting.DisplayName ?? raceSetting.GetType().Name);
                if (raceDef == null) continue;

                foreach (var xenotypeKey in raceSetting.XenotypePrices.Keys.ToList())
                {
                    // Calculate new price using actual market value
                    float newPrice = GeneUtils.CalculateXenotypeMarketValue(raceDef, xenotypeKey);
                    if (Math.Abs(newPrice - raceSetting.XenotypePrices[xenotypeKey]) > 0.1f)
                    {
                        raceSetting.XenotypePrices[xenotypeKey] = newPrice;
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                JsonFileManager.SaveFile("RaceSettings.json", JsonFileManager.SerializeRaceSettings(raceSettings));
                Messages.Message("Xenotype prices updated with gene-based pricing", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("No xenotype price changes needed", MessageTypeDefOf.NeutralEvent);
            }
        }

        [DebugAction("CAP", "Show Xenotype Gene Details", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugShowXenotypeGeneDetails()
        {
            var options = new List<DebugMenuOption>();

            // Use Human as the sample race for gene details
            var humanRace = ThingDefOf.Human;

            foreach (var xenotype in DefDatabase<XenotypeDef>.AllDefs)
            {
                options.Add(new DebugMenuOption(xenotype.defName, DebugMenuOptionMode.Action, () =>
                {
                    ShowXenotypeGeneDetails(humanRace, xenotype);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        private static void AnalyzeAndLogXenotypePricing(ThingDef race, XenotypeDef xenotype)
        {
            // Calculate actual market value
            float marketValue = GeneUtils.CalculateXenotypeMarketValue(race, xenotype.defName);
            float geneValueOnly = GeneUtils.GetXenotypeGeneValueOnly(xenotype.defName, race.BaseMarketValue);
            var geneDetails = GeneUtils.GetXenotypeGeneDetails(race, xenotype.defName);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== {xenotype.defName} Pricing Analysis (for {race.defName}) ===");
            sb.AppendLine($"Race Base Value: {race.BaseMarketValue:F0} silver");
            sb.AppendLine($"Total Gene Value: {geneValueOnly:F0} silver");
            sb.AppendLine($"Final Market Value: {marketValue:F0} silver");
            sb.AppendLine($"Total Genes: {GeneUtils.GetXenotypeGenes(xenotype.defName).Count}");
            sb.AppendLine($"Gene Summary: {GeneUtils.GetXenotypeGeneSummary(xenotype.defName)}");
            sb.AppendLine();
            sb.AppendLine("Gene Details:");
            foreach (var detail in geneDetails)
            {
                sb.AppendLine($"  {detail}");
            }

            Log.Message(sb.ToString());
            Messages.Message($"Check log for {xenotype.defName} pricing details", MessageTypeDefOf.NeutralEvent);
        }

        private static void ShowXenotypeGeneDetails(ThingDef race, XenotypeDef xenotype)
        {
            var geneDetails = GeneUtils.GetXenotypeGeneDetails(race, xenotype.defName);
            float marketValue = GeneUtils.CalculateXenotypeMarketValue(race, xenotype.defName);
            float baseRaceValue = race.BaseMarketValue;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{xenotype.defName} - Race: {race.defName}");
            sb.AppendLine($"Race Base Value: {baseRaceValue:F0} silver");
            sb.AppendLine($"Final Market Value: {marketValue:F0} silver");
            sb.AppendLine();
            sb.AppendLine("Gene Details:");
            sb.AppendLine(string.Join("\n", geneDetails));

            Find.WindowStack.Add(new Dialog_MessageBox(sb.ToString(), "OK", null, null, null, "Xenotype Gene Details"));
        }
    }
}