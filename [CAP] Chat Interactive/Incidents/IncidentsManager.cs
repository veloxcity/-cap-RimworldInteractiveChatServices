// IncidentsManager.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Manages the loading, saving, and updating of buyable incidents for the chat interactive mod.
using LudeonTK;
using Newtonsoft.Json;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CAP_ChatInteractive.Incidents
{
    public static class IncidentsManager
    {
        public static Dictionary<string, BuyableIncident> AllBuyableIncidents { get; private set; } = new Dictionary<string, BuyableIncident>();
        private static bool isInitialized = false;
        private static readonly object lockObject = new object();

        public static void InitializeIncidents()
        {
            if (isInitialized) return;

            lock (lockObject)
            {
                if (isInitialized) return;

                Logger.Debug("Initializing Incidents System...");

                if (!LoadIncidentsFromJson())
                {
                    Logger.Debug("No incidents JSON found, creating default incidents...");
                    CreateDefaultIncidents();
                    SaveIncidentsToJson();
                }
                else
                {
                    ValidateAndUpdateIncidents();
                }

                isInitialized = true;
                Logger.Message($"[CAP] Incidents System initialized with {AllBuyableIncidents.Count} incidents");
            }
        }

        private static bool LoadIncidentsFromJson()
        {
            string jsonContent = JsonFileManager.LoadFile("Incidents.json");
            if (string.IsNullOrEmpty(jsonContent))
                return false;

            try
            {
                var loadedIncidents = JsonFileManager.DeserializeIncidents(jsonContent);
                AllBuyableIncidents.Clear();

                foreach (var kvp in loadedIncidents)
                {
                    AllBuyableIncidents[kvp.Key] = kvp.Value;
                }

                Logger.Debug($"Loaded {AllBuyableIncidents.Count} incidents from JSON");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.Error($"Error loading incidents JSON: {e.Message}");
                return false;
            }
        }

        private static void CreateDefaultIncidents()
        {
            AllBuyableIncidents.Clear();
            LogIncidentCategories();

            var allIncidentDefs = DefDatabase<IncidentDef>.AllDefs.ToList();
            Logger.Debug($"Processing {allIncidentDefs.Count} incident definitions");

            int incidentsCreated = 0;
            foreach (var incidentDef in allIncidentDefs)
            {
                try
                {
                    // Just create the incident - let it self-filter
                    var buyableIncident = new BuyableIncident(incidentDef);

                    // Only add if it should be in store
                    if (buyableIncident.ShouldBeInStore)
                    {
                        string key = GetIncidentKey(incidentDef);
                        if (!AllBuyableIncidents.ContainsKey(key))
                        {
                            AllBuyableIncidents[key] = buyableIncident;
                            incidentsCreated++;
                        }
                    }
                    else
                    {
                        Logger.Debug($"Skipping store-unsuitable incident: {incidentDef.defName}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error creating buyable incident for {incidentDef.defName}: {ex.Message}");
                }
            }

            Logger.Debug($"Created {AllBuyableIncidents.Count} store-suitable incidents");
        }

        private static void LogImplementationSummary()
        {
            var incidentsByMod = IncidentsManager.AllBuyableIncidents.Values
                .GroupBy(i => i.ModSource)
                .OrderByDescending(g => g.Count());

            Logger.Message("=== INCIDENT IMPLEMENTATION ROADMAP ===");
            foreach (var modGroup in incidentsByMod)
            {
                Logger.Message($"{modGroup.Key}: {modGroup.Count()} incidents");
            }

            Logger.Message("=== START WITH THESE CORE INCIDENTS ===");
            var easyCore = IncidentsManager.AllBuyableIncidents.Values
                .Where(i => i.ModSource == "Core")
                .Where(i => !i.DefName.Contains("Anomaly") && !i.PointsScaleable && !i.IsQuestIncident)
                .Take(5);

            foreach (var incident in easyCore)
            {
                Logger.Message($"  - {incident.DefName}: {incident.Label} (Cost: {incident.BaseCost})");
            }
        }

        private static bool IsIncidentSuitableForStore(IncidentDef incidentDef)
        {
            // Delegate all logic to BuyableIncident constructor
            // Just do basic null checks here
            if (incidentDef == null) return false;
            if (incidentDef.Worker == null) return false;

            return true; // Let BuyableIncident handle the real filtering
        }

        private static string GetIncidentKey(IncidentDef incidentDef)
        {
            return incidentDef.defName;
        }

        private static void ValidateAndUpdateIncidents()
        {
            var allIncidentDefs = DefDatabase<IncidentDef>.AllDefs;
            int addedIncidents = 0;
            int removedIncidents = 0;
            int updatedCommandAvailability = 0;
            int updatedPricing = 0;
            int updatedKarmaTypes = 0;
            int updatedStoreSuitability = 0;
            int autoDisabledModEvents = 0;

            // Add any new incidents that aren't in our system
            foreach (var incidentDef in allIncidentDefs)
            {
                if (!IsIncidentSuitableForStore(incidentDef))
                    continue;

                string key = GetIncidentKey(incidentDef);
                if (!AllBuyableIncidents.ContainsKey(key))
                {
                    var buyableIncident = new BuyableIncident(incidentDef);
                    AllBuyableIncidents[key] = buyableIncident;
                    addedIncidents++;

                    // Count newly auto-disabled mod events
                    if (!buyableIncident.Enabled && buyableIncident.DisabledReason?.Contains("Auto-disabled") == true)
                    {
                        autoDisabledModEvents++;
                    }
                }
                else
                {
                    // Validate and update existing incidents
                    var existingIncident = AllBuyableIncidents[key];
                    var tempIncident = new BuyableIncident(incidentDef); // Create temp to get current values

                    // Store the original enabled state to check if we should preserve it
                    bool wasOriginallyEnabled = existingIncident.Enabled;

                    // Check if store suitability needs updating
                    bool currentStoreSuitability = existingIncident.ShouldBeInStore;
                    if (existingIncident.ShouldBeInStore != tempIncident.ShouldBeInStore)
                    {
                        existingIncident.ShouldBeInStore = tempIncident.ShouldBeInStore;
                        updatedStoreSuitability++;

                        // Auto-disable if no longer suitable for store
                        if (!existingIncident.ShouldBeInStore)
                        {
                            existingIncident.Enabled = false;
                            existingIncident.DisabledReason = "No longer suitable for store system";
                        }
                    }

                    // Check if command availability needs updating
                    bool currentAvailability = existingIncident.IsAvailableForCommands;
                    if (existingIncident.IsAvailableForCommands != tempIncident.IsAvailableForCommands)
                    {
                        existingIncident.IsAvailableForCommands = tempIncident.IsAvailableForCommands;
                        updatedCommandAvailability++;
                    }

                    // Check if pricing needs updating (if default pricing changed significantly)
                    int priceDifference = Math.Abs(existingIncident.BaseCost - tempIncident.BaseCost);
                    if (priceDifference > existingIncident.BaseCost * 0.2f) // More than 20% difference
                    {
                        // Only update if user hasn't customized the price (check against original)
                        if (IsPriceCloseToDefault(existingIncident, tempIncident.BaseCost))
                        {
                            existingIncident.BaseCost = tempIncident.BaseCost;
                            updatedPricing++;
                        }
                    }

                    // Check if karma type needs updating (if logic changed)
                    if (existingIncident.KarmaType != tempIncident.KarmaType)
                    {
                        // Use the new similarity check from BuyableIncident
                        if (existingIncident.IsKarmaTypeSimilar(existingIncident.KarmaType, tempIncident.KarmaType))
                        {
                            existingIncident.KarmaType = tempIncident.KarmaType;
                            updatedKarmaTypes++;
                        }
                    }

                    // NEW: For existing incidents, don't auto-disable if user already enabled them
                    // This preserves user choice while still applying auto-disable to new incidents
                    if (ShouldAutoDisableModEvent(incidentDef) && wasOriginallyEnabled)
                    {
                        // User already enabled this mod event, so don't auto-disable it
                        existingIncident.Enabled = true;
                        existingIncident.DisabledReason = "";
                    }
                }
            }

            // Remove incidents that no longer exist in the game or are no longer suitable
            var keysToRemove = new List<string>();
            foreach (var kvp in AllBuyableIncidents)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(kvp.Key);
                if (incidentDef == null || !IsIncidentSuitableForStore(incidentDef))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                AllBuyableIncidents.Remove(key);
                removedIncidents++;
            }

            // Log all changes
            if (addedIncidents > 0 || removedIncidents > 0 || updatedCommandAvailability > 0 ||
                updatedPricing > 0 || updatedKarmaTypes > 0 || updatedStoreSuitability > 0 || autoDisabledModEvents > 0)
            {
                StringBuilder changes = new StringBuilder("Incidents updated:");
                if (addedIncidents > 0) changes.Append($" +{addedIncidents} incidents");
                if (removedIncidents > 0) changes.Append($" -{removedIncidents} incidents");
                if (autoDisabledModEvents > 0) changes.Append($" {autoDisabledModEvents} mod events auto-disabled");
                if (updatedStoreSuitability > 0) changes.Append($" {updatedStoreSuitability} store suitability flags updated");
                if (updatedCommandAvailability > 0) changes.Append($" {updatedCommandAvailability} availability flags updated");
                if (updatedPricing > 0) changes.Append($" {updatedPricing} prices updated");
                if (updatedKarmaTypes > 0) changes.Append($" {updatedKarmaTypes} karma types updated");

                Logger.Message(changes.ToString());

                // Add a helpful message about auto-disabled mod events
                if (autoDisabledModEvents > 0)
                {
                    Logger.Message($"Safety feature: {autoDisabledModEvents} mod events were auto-disabled. Enable them manually in the Events Editor if desired.");
                }

                SaveIncidentsToJson(); // Save changes
            }
        }

        public static void SaveIncidentsToJson()
        {
            LongEventHandler.QueueLongEvent(() =>
            {
                lock (lockObject)
                {
                    try
                    {
                        string jsonContent = JsonFileManager.SerializeIncidents(AllBuyableIncidents);
                        JsonFileManager.SaveFile("Incidents.json", jsonContent);
                        Logger.Debug("Incidents JSON saved successfully");
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error($"Error saving incidents JSON: {e.Message}");
                    }
                }
            }, null, false, null, showExtraUIInfo: false, forceHideUI: true);
        }

        private static void LogIncidentCategories()
        {
            var categories = DefDatabase<IncidentCategoryDef>.AllDefs;
            Logger.Debug($"Found {categories.Count()} incident categories:");
            foreach (var category in categories)
            {
                Logger.Debug($"  - {category.defName}: {category.LabelCap}");
            }

            var allIncidents = DefDatabase<IncidentDef>.AllDefs.ToList();
            Logger.Debug($"Total incidents found: {allIncidents.Count}");

            var suitableIncidents = allIncidents.Where(IsIncidentSuitableForStore).ToList();
            Logger.Debug($"Suitable incidents for store: {suitableIncidents.Count}");

            // Log first 10 incidents as sample
            foreach (var incident in suitableIncidents.Take(10))
            {
                Logger.Debug($"Sample: {incident.defName} - {incident.label} - Worker: {incident.Worker?.GetType().Name}");
            }
        }

        private static bool ShouldAutoDisableModEvent(IncidentDef incidentDef)
        {
            string modSource = incidentDef.modContentPack?.Name ?? "RimWorld";

            // Always enable Core RimWorld incidents
            if (modSource == "RimWorld" || modSource == "Core")
                return false;

            // Enable official DLCs
            string[] officialDLCs = {
        "Royalty", "Ideology", "Biotech", "Anomaly"
    };

            if (officialDLCs.Any(dlc => modSource.Contains(dlc)))
                return false;

            // Auto-disable all other mod events for safety
            return true;
        }

        private static bool IsPriceCloseToDefault(BuyableIncident incident, int defaultPrice)
        {
            // Consider price "close to default" if within 30% of default
            float ratio = (float)incident.BaseCost / defaultPrice;
            return ratio >= 0.7f && ratio <= 1.3f;
        }


        [DebugAction("CAP", "Reload Incidents", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugReloadIncidents()
        {
            isInitialized = false;
            InitializeIncidents();
        }
    }
}