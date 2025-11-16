// HARPatch.cs uses features available in C# 10.0, such as file-scoped namespaces.
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Provides compatibility with the Human and Alien Races (HAR) mod for trait and xenotype restrictions.
using _CAP__Chat_Interactive.Interfaces;
using AlienRace;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Patch.HAR
{
    [UsedImplicitly]
    // [StaticConstructorOnStartup]
    public class HARPatch: IAlienCompatibilityProvider
    {
        public string ModId => "erdelf.HumanoidAlienRaces";

        static HARPatch()
        {
            Logger.Message("[CAP] HAR Patch Assembly Loaded!");
            Logger.Debug("[CAP] HAR Patch static constructor executed");
        }

        public HARPatch()
        {
            Logger.Message("[CAP] HAR Patch Instance Created!");
        }

        public static class HARPatchVerifier
        {
            public static void VerifyLoaded()
            {
                Logger.Message("[CAP] HAR Patch Verification Called - Assembly is LOADED!");
            }

            public static bool IsAvailable()
            {
                Logger.Debug("[CAP] HAR Patch Availability Check - YES");
                return true;
            }
        }

        public bool IsTraitForced(Pawn pawn, string defName, int degree)
        {
            if (pawn.def is not ThingDef_AlienRace alienRace ||
                alienRace.alienRace.generalSettings.forcedRaceTraitEntries.NullOrEmpty())
            {
                return false;
            }

            foreach (AlienChanceEntry<TraitWithDegree> entry in alienRace.alienRace.generalSettings.forcedRaceTraitEntries)
            {
                if (string.Equals(entry.entry.def.defName, defName) && entry.entry.degree == degree)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsTraitDisallowed(Pawn pawn, string defName, int degree)
        {
            if (pawn.def is not ThingDef_AlienRace alienRace ||
                alienRace.alienRace.generalSettings.disallowedTraits.NullOrEmpty())
            {
                return false;
            }

            foreach (AlienChanceEntry<TraitWithDegree> entry in alienRace.alienRace.generalSettings.disallowedTraits)
            {
                if (string.Equals(entry.entry.def.defName, defName) && entry.entry.degree == degree)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsTraitAllowed(Pawn pawn, TraitDef traitDef, int degree = -10)
        {
            return !IsTraitDisallowed(pawn, traitDef.defName, degree) &&
                   !IsTraitForced(pawn, traitDef.defName, degree);
        }

        public List<string> GetAllowedXenotypes(ThingDef raceDef)
        {
            Logger.Debug($"=== HAR PROVIDER: GetAllowedXenotypes for {raceDef.defName} ===");

            if (!ModsConfig.BiotechActive || raceDef == ThingDefOf.Human)
            {
                Logger.Debug($"Returning empty - Biotech: {ModsConfig.BiotechActive}, IsHuman: {raceDef == ThingDefOf.Human}");
                return new List<string>();
            }

            // Get the race restriction settings
            var restriction = GetRaceRestriction(raceDef);

            if (restriction == null)
            {
                Logger.Debug("No race restrictions found, returning empty list");
                return new List<string>();
            }

            Logger.Debug($"Race restriction found:");
            Logger.Debug($"  onlyUseRaceRestrictedXenotypes: {restriction.onlyUseRaceRestrictedXenotypes}");
            Logger.Debug($"  xenotypeList count: {restriction.xenotypeList?.Count ?? 0}");
            Logger.Debug($"  whiteXenotypeList count: {restriction.whiteXenotypeList?.Count ?? 0}");
            Logger.Debug($"  blackXenotypeList count: {restriction.blackXenotypeList?.Count ?? 0}");

            // For our purposes, we only care about whiteXenotypeList - this is what we want to enable
            if (restriction.whiteXenotypeList != null && restriction.whiteXenotypeList.Count > 0)
            {
                var result = restriction.whiteXenotypeList.Select(x => x.defName).ToList();
                Logger.Debug($"Returning whiteXenotypeList: {result.Count} xenotypes");
                return result;
            }

            Logger.Debug("No whiteXenotypeList found, returning empty list");
            return new List<string>();
        }

        public bool IsXenotypeAllowed(ThingDef raceDef, XenotypeDef xenotype)
        {
            if (raceDef == ThingDefOf.Human)
                return true;

            var restriction = GetRaceRestriction(raceDef);
            if (restriction == null)
            {
                return true; // No restrictions = all allowed
            }

            // Check blacklist first - if it's in blacklist, it's not allowed
            if (restriction.blackXenotypeList != null &&
                restriction.blackXenotypeList.Any(x => x.defName == xenotype.defName))
            {
                return false;
            }

            // If whitelist exists, check if it's in the whitelist
            if (restriction.whiteXenotypeList != null && restriction.whiteXenotypeList.Count > 0)
            {
                return restriction.whiteXenotypeList.Any(x => x.defName == xenotype.defName);
            }

            // If only race-restricted xenotypes are allowed, check the exclusive list
            if (restriction.onlyUseRaceRestrictedXenotypes)
            {
                if (restriction.xenotypeList != null)
                {
                    return restriction.xenotypeList.Any(x => x.defName == xenotype.defName);
                }
                return false; // No exclusive list = no xenotypes allowed
            }

            // If whitelist exists, check if it's in the whitelist
            if (restriction.whiteXenotypeList != null && restriction.whiteXenotypeList.Count > 0)
            {
                return restriction.whiteXenotypeList.Any(x => x.defName == xenotype.defName);
            }

            // If no whitelist but exclusive list exists, check that too
            if (restriction.xenotypeList != null && restriction.xenotypeList.Count > 0)
            {
                return restriction.xenotypeList.Any(x => x.defName == xenotype.defName);
            }

            // No restrictions = all allowed
            return true;
        }

        /// <summary>
        /// Helper method to get the RaceRestrictionSettings from a race def
        /// </summary>
        private RaceRestrictionSettings GetRaceRestriction(ThingDef raceDef)
        {
            if (raceDef is ThingDef_AlienRace alienRace)
            {
                // Race restrictions are directly on the alienRace field
                return alienRace.alienRace?.raceRestriction;
            }

            // If we get here, it's not a HAR race - no need for reflection fallback
            // Logger.Warn($"Race {raceDef.defName} is not a ThingDef_AlienRace - skipping HAR xenotype restrictions");
            return null;
        }

        //  Gender

        public bool IsGenderAllowed(ThingDef raceDef, Gender gender)
        {
            if (raceDef is not ThingDef_AlienRace alienRace)
            {
                return true; // Non-HAR races allow all genders by default
            }

            var raceSettings = alienRace.alienRace.generalSettings;
            if (raceSettings == null)
            {
                return true;
            }

            // Check gender probability to determine allowed genders
            return gender switch
            {
                Gender.Male => raceSettings.maleGenderProbability > 0f,
                Gender.Female => raceSettings.maleGenderProbability < 1f,
                Gender.None => true, // Usually "None" is allowed
                _ => true
            };
        }

        public GenderPossibility GetAllowedGenders(ThingDef raceDef)
        {
            if (raceDef is not ThingDef_AlienRace alienRace)
            {
                return GenderPossibility.Either; // Non-HAR races allow both by default
            }

            var raceSettings = alienRace.alienRace.generalSettings;
            if (raceSettings == null)
            {
                return GenderPossibility.Either;
            }

            // Round to 2 decimal places to match HAR's precision
            float roundedProbability = (float)Math.Round(raceSettings.maleGenderProbability, 2);

            // Convert maleGenderProbability to GenderPossibility
            if (roundedProbability <= 0f)
                return GenderPossibility.Female; // Only female allowed
            else if (roundedProbability >= 1f)
                return GenderPossibility.Male; // Only male allowed
            else
                return GenderPossibility.Either; // Both allowed
        }

        public (float maleProbability, float femaleProbability) GetGenderProbabilities(ThingDef raceDef)
        {
            if (raceDef is not ThingDef_AlienRace alienRace)
            {
                return (0.5f, 0.5f); // Default 50/50 for non-HAR races
            }

            var raceSettings = alienRace.alienRace.generalSettings;
            if (raceSettings == null)
            {
                return (0.5f, 0.5f);
            }

            // Round to 2 decimal places to match HAR's precision
            float maleProb = (float)Math.Round(raceSettings.maleGenderProbability, 2);
            float femaleProb = 1f - maleProb;

            return (maleProb, femaleProb);
        }
    }
}