using CAP_ChatInteractive.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
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
//
// Command handler for buying items from Rimazon store
namespace CAP_ChatInteractive.Commands.CommandHandlers
{
    internal static class SurgeryBuyItemCommandHandler
    {

        public static string HandleSurgery(ChatMessageWrapper messageWrapper, string[] args)
        {
            try
            {
                Logger.Debug($"HandleSurgery called for user: {messageWrapper.Username}, args: {string.Join(", ", args)}");

                if (args.Length == 0)
                {
                    return "Usage: !surgery <implant> [left/right] [quantity] - Example: !surgery bionic arm left 1";
                }

                var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
                var currencySymbol = settings.CurrencyName?.Trim() ?? "¢";
                var viewer = Viewers.GetViewer(messageWrapper);

                // REPLACE the parsing code (about 40 lines) with:
                var parsed = CommandParserUtility.ParseCommandArguments(args, allowQuality: false, allowMaterial: false, allowSide: true, allowQuantity: true);
                if (parsed.HasError)
                    return parsed.Error;

                string itemName = parsed.ItemName;
                string sideStr = parsed.Side;
                string quantityStr = parsed.Quantity.ToString();

                // Get store item
                var storeItem = StoreCommandHelper.GetStoreItemByName(itemName);
                if (storeItem == null)
                {
                    return $"Implant '{itemName}' not found in Rimazon.";
                }

                if (!storeItem.Enabled)
                {
                    return $"Implant '{itemName}' is not available for purchase.";
                }

                // Check if this is actually an implant/surgery item
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(storeItem.DefName);
                if (thingDef == null)
                {
                    Logger.Error($"ThingDef not found: {storeItem.DefName}");
                    return $"Error: Implant definition not found.";
                }

                // Check if this is a valid surgery item (bionic, implant, etc.)
                if (!IsValidSurgeryItem(thingDef))
                {
                    return $"{itemName} is not a valid implant or surgery item. Use !buy instead for regular items.";
                }

                // Check research requirements
                if (!StoreCommandHelper.HasRequiredResearch(storeItem))
                {
                    return $"{itemName} requires research that hasn't been completed yet.";
                }

                // Get viewer's pawn
                Verse.Pawn viewerPawn = StoreCommandHelper.GetViewerPawn(messageWrapper);
                if (viewerPawn == null)
                {
                    return "You need to have a pawn in the colony to perform surgery. Use !buy pawn first.";
                }

                if (viewerPawn.Dead)
                {
                    return "Your pawn is dead. You cannot perform surgery.";
                }

                // Parse quantity
                if (!int.TryParse(quantityStr, out int quantity) || quantity < 1)
                {
                    quantity = 1;
                }

                // SPECIAL HANDLING FOR SURGERY ITEMS: Allow up to 2 for body parts
                int surgeryQuantityLimit = Math.Max(storeItem.QuantityLimit, 2);
                if (quantity > surgeryQuantityLimit)
                {
                    Logger.Debug($"Quantity {quantity} exceeds surgery limit of {surgeryQuantityLimit} for {itemName}, clamping");
                    quantity = surgeryQuantityLimit;
                }

                // Calculate final price (no quality/material for surgery items)
                int finalPrice = storeItem.BasePrice * quantity;

                // Check if user can afford
                if (!StoreCommandHelper.CanUserAfford(messageWrapper, finalPrice))
                {
                    return $"You need {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)} for {quantity}x {itemName} surgery! You have {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";
                }

                // Find appropriate recipe for this implant
                var recipe = FindSurgeryRecipeForImplant(thingDef, viewerPawn);
                if (recipe == null)
                {
                    return $"No surgical procedure found for {itemName} on your pawn.";
                }

                // Find body parts for the surgery - let RimWorld decide which parts, we just filter by side
                var bodyParts = FindBodyPartsForSurgery(recipe, viewerPawn, sideStr, quantity);
                if (bodyParts.Count == 0)
                {
                    string availableParts = GetAvailableBodyPartsDescription(recipe, viewerPawn);
                    return $"No suitable body parts found for {itemName} surgery. Available: {availableParts}. Try specifying left/right.";
                }

                // Limit quantity to available body parts
                quantity = Math.Min(quantity, bodyParts.Count);

                // Adjust final price for actual quantity
                finalPrice = storeItem.BasePrice * quantity;

                // Deduct coins
                viewer.TakeCoins(finalPrice);

                int karmaEarned = finalPrice / 100;
                if (karmaEarned > 0)
                {
                    viewer.GiveKarma(karmaEarned);
                    Logger.Debug($"Awarded {karmaEarned} karma for {finalPrice} coin surgery");
                }

                for (int i = 0; i < quantity; i++)
                {
                    StoreCommandHelper.SpawnItemForPawn(thingDef, 1, null, null, viewerPawn, false); // Add to inventory
                    Logger.Debug($"Spawned surgery item {i + 1} of {quantity}: {thingDef.defName}");
                }



                // Schedule the surgeries
                ScheduleSurgeries(viewerPawn, recipe, bodyParts.Take(quantity).ToList());

                // In HandleSurgery method, after scheduling surgeries:


                LookTargets surgeryLookTargets = new LookTargets(viewerPawn);
                string invoiceLabel = $"🏥 Rimazon Surgery - {messageWrapper.Username}";
                string invoiceMessage = CreateRimazonSurgeryInvoice(messageWrapper.Username, itemName, quantity, finalPrice, currencySymbol, bodyParts.Take(quantity).ToList());
                MessageHandler.SendBlueLetter(invoiceLabel, invoiceMessage, surgeryLookTargets);

                Logger.Debug($"Surgery scheduled: {messageWrapper.Username} scheduled {quantity}x {itemName} for {finalPrice}{currencySymbol}");

                return $"Scheduled {quantity}x {itemName} surgery for {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)}! Implant delivered to pawn's inventory please give them to the doctor. Remaining: {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";

            }
            catch (Exception ex)
            {
                Logger.Error($"Error in HandleSurgery: {ex}");
                return "Error scheduling surgery. Please try again.";
            }
        }

        // ===== INVOICE CREATION METHODS =====
        private static string CreateRimazonSurgeryInvoice(string username, string itemName, int quantity, int price, string currencySymbol, List<BodyPartRecord> bodyParts)
        {
            string invoice = $"RIMAZON SURGERY SERVICE\n";
            invoice += $"====================\n";
            invoice += $"Customer: {username}\n";
            invoice += $"Procedure: {itemName} x{quantity}\n";

            if (bodyParts.Count > 0)
            {
                invoice += $"Body Parts: {string.Join(", ", bodyParts.Select(bp => bp.Label))}\n";
            }

            invoice += $"Service: Surgical Implantation\n";
            invoice += $"====================\n";
            invoice += $"Total: {price:N0}{currencySymbol}\n";
            invoice += $"====================\n";
            invoice += $"Thank you for using Rimazon Surgery!\n";
            invoice += $"Implant delivered to pawn's inventory.\n";
            invoice += $"Surgery scheduled with colony doctors.";

            return invoice;
        }

        private static List<BodyPartRecord> FindBodyPartsForSurgery(RecipeDef recipe, Verse.Pawn pawn, string sideFilter, int maxQuantity)
        {
            Logger.Debug($"FindBodyPartsForSurgery - Recipe: {recipe.defName}, SideFilter: {sideFilter}, MaxQuantity: {maxQuantity}");

            // Let RimWorld tell us which parts this surgery applies to
            var availableParts = recipe.Worker.GetPartsToApplyOn(pawn, recipe).ToList();
            Logger.Debug($"Initial available parts from recipe: {availableParts.Count}");

            // Filter by side if specified
            if (!string.IsNullOrEmpty(sideFilter))
            {
                var beforeFilterCount = availableParts.Count;
                availableParts = availableParts
                    .Where(part => GetBodyPartSide(part).ToLower().Contains(sideFilter.ToLower()))
                    .ToList();
                Logger.Debug($"After side filter '{sideFilter}': {beforeFilterCount} -> {availableParts.Count}");
            }

            // Remove parts that already have this surgery scheduled or the implant already installed
            var beforeDedupeCount = availableParts.Count;
            availableParts = availableParts
                .Where(part => !HasSurgeryScheduled(pawn, recipe, part) && !HasImplantAlready(pawn, part, recipe))
                .ToList();
            Logger.Debug($"After deduplication: {beforeDedupeCount} -> {availableParts.Count}");

            // Log available parts for debugging
            if (availableParts.Count > 0)
            {
                Logger.Debug($"Available body parts: {string.Join(", ", availableParts.Select(p => $"{GetBodyPartDisplayName(p)}"))}");
            }
            else
            {
                Logger.Debug("No available body parts found after all filters");
            }

            // Limit to requested quantity
            return availableParts.Take(maxQuantity).ToList();
        }

        private static RecipeDef FindSurgeryRecipeForImplant(ThingDef implantDef, Verse.Pawn pawn)
        {
            return DefDatabase<RecipeDef>.AllDefs
                .Where(r => r.IsSurgery && r.AvailableOnNow(pawn))
                .FirstOrDefault(r => r.ingredients.Any(i => i.filter.AllowedThingDefs.Contains(implantDef)));
        }

        private static string GetAvailableBodyPartsDescription(RecipeDef recipe, Verse.Pawn pawn)
        {
            var availableParts = recipe.Worker.GetPartsToApplyOn(pawn, recipe).ToList();
            if (availableParts.Count == 0) return "none";

            // Group by side and get unique part types
            var partGroups = availableParts
                .GroupBy(p => GetBodyPartSide(p))
                .Select(g => $"{g.Count()} {g.Key} parts")
                .ToList();

            return string.Join(", ", partGroups);
        }

        // ===== BODY PART METHODS =====
        private static string GetBodyPartDisplayName(BodyPartRecord part)
        {
            return !string.IsNullOrEmpty(part.customLabel) ? part.customLabel : part.Label;
        }

        private static string GetBodyPartSide(BodyPartRecord part)
        {
            // Use customLabel if available, otherwise use label
            var label = (!string.IsNullOrEmpty(part.customLabel) ? part.customLabel : part.Label).ToLower();

            if (label.Contains("left")) return "left";
            if (label.Contains("right")) return "right";
            return "center";
        }

        private static bool HasImplantAlready(Verse.Pawn pawn, BodyPartRecord part, RecipeDef recipe)
        {
            // Check if the pawn already has the hediff that this surgery would add
            if (recipe.addsHediff != null)
            {
                return pawn.health.hediffSet.hediffs.Any(h =>
                    h.def == recipe.addsHediff && h.Part == part);
            }
            return false;
        }

        private static bool HasSurgeryScheduled(Verse.Pawn pawn, RecipeDef recipe, BodyPartRecord part)
        {
            return pawn.health.surgeryBills.Bills.Any(bill =>
                bill is Bill_Medical medicalBill &&
                medicalBill.recipe == recipe &&
                medicalBill.Part == part);
        }

        // ===== VALIDATION METHODS =====

        private static bool IsValidSurgeryItem(ThingDef thingDef)
        {
            // Check if this is an implant, bionic part, or other surgical item
            if (thingDef.isTechHediff) return true;
            if (thingDef.defName.Contains("Bionic") || thingDef.defName.Contains("Prosthetic")) return true;
            if (thingDef.defName.Contains("Implant")) return true;

            // Check if there are any recipes that use this item as an ingredient for surgery
            var surgeryRecipes = DefDatabase<RecipeDef>.AllDefs
                .Where(r => r.IsSurgery && r.ingredients.Any(i => i.filter.AllowedThingDefs.Contains(thingDef)))
                .ToList();

            return surgeryRecipes.Count > 0;
        }

        private static void ScheduleSurgeries(Verse.Pawn pawn, RecipeDef recipe, List<BodyPartRecord> bodyParts)
        {
            foreach (var bodyPart in bodyParts)
            {
                var bill = new Bill_Medical(recipe, null) { Part = bodyPart };
                pawn.health.surgeryBills.AddBill(bill);
                Logger.Debug($"Scheduled {recipe.defName} on {bodyPart.Label} for pawn {pawn.Name}");
            }
        }
    }
}