// BuyItemCommandHandler.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Command handler for buying items from Rimazon store
using CAP_ChatInteractive.Commands.ViewerCommands;
using CAP_ChatInteractive.Store;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using Pawn = CAP_ChatInteractive.Commands.ViewerCommands.Pawn;

namespace CAP_ChatInteractive.Commands.CommandHandlers
{
    public static class BuyItemCommandHandler
    {
        
        public static string HandleBuyItem(ChatMessageWrapper user, string[] args, bool requireEquippable = false, bool requireWearable = false, bool addToInventory = false)
        {
            try
            {
                Logger.Debug($"HandleBuyItem called for user: {user.Username}, args: {string.Join(", ", args)}, requireEquippable: {requireEquippable}, requireWearable: {requireWearable}, addToInventory: {addToInventory}");

                // Parse arguments - handle multi-word item names
                string itemName;
                string qualityStr = "random";
                string materialStr = "random";
                string quantityStr = "1";

                if (args.Length >= 1)
                {
                    // Try to find the item name by combining arguments until we hit quality/material/quantity keywords
                    var itemNameParts = new List<string>();

                    for (int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i];

                        // The FIRST argument must always be part of the item name
                        // Only check for keywords after we have at least one item name part
                        if (itemNameParts.Count > 0)
                        {
                            // Check if this argument is a quality, material, or quantity indicator
                            if (IsQualityKeyword(arg.ToLower()) || IsMaterialKeyword(arg) || int.TryParse(arg, out _))
                            {
                                // We've hit a non-item-name argument, stop collecting
                                break;
                            }
                        }

                        itemNameParts.Add(args[i]);
                    }

                    itemName = string.Join(" ", itemNameParts);

                    // Parse remaining arguments
                    int currentIndex = itemNameParts.Count;

                    // Parse quality (if next arg is a quality keyword)
                    if (args.Length > currentIndex && IsQualityKeyword(args[currentIndex].ToLower()))
                    {
                        qualityStr = args[currentIndex];
                        currentIndex++;
                    }

                    // Parse material (if next arg is a material keyword)  
                    if (args.Length > currentIndex && IsMaterialKeyword(args[currentIndex]))
                    {
                        materialStr = args[currentIndex];
                        currentIndex++;
                    }

                    // Parse quantity (if next arg is a number) - lowest priority
                    if (args.Length > currentIndex && int.TryParse(args[currentIndex], out _))
                    {
                        quantityStr = args[currentIndex];
                    }

                    Logger.Debug($"Parsed - Item: '{itemName}', Quality: '{qualityStr}', Material: '{materialStr}', Quantity: '{quantityStr}'");
                    Logger.Debug($"Original args: {string.Join("|", args)}, ItemNameParts: [{string.Join(", ", itemNameParts)}]");
                }
                else
                {
                    return "Usage: !buy/equip/wear <item> [quality] [material] [quantity]";
                }

                var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
                var currencySymbol = settings.CurrencyName?.Trim() ?? "¢";
                var viewer = Viewers.GetViewer(user.Username);

                // Get store item
                var storeItem = StoreCommandHelper.GetStoreItemByName(itemName);
                Logger.Debug($"Store item lookup for '{itemName}': {(storeItem != null ? $"Found: {storeItem.DefName}, Enabled: {storeItem.Enabled}" : "Not Found")}");

                if (storeItem == null)
                {
                    Logger.Debug($"Item not found: {itemName}");
                    return $"Item '{itemName}' not found in Rimazon.";
                }

                if (!storeItem.Enabled)
                {
                    Logger.Debug($"Item disabled: {itemName}");
                    return $"Item '{itemName}' is not available for purchase.";
                }

                // Check item type requirements
                if (!StoreCommandHelper.IsItemTypeValid(storeItem, requireEquippable, requireWearable, false))
                {
                    string itemType = StoreCommandHelper.GetItemTypeDescription(storeItem);
                    string expectedType = requireEquippable ? "equippable" : requireWearable ? "wearable" : "purchasable";
                    return $"{itemName} is a {itemType}, not an {expectedType} item. Use !buy instead.";
                }

                // Check research requirements
                if (!StoreCommandHelper.HasRequiredResearch(storeItem))
                {
                    return $"{itemName} requires research that hasn't been completed yet.";
                }

                // Parse quality
                var quality = StoreCommandHelper.ParseQuality(qualityStr);
                if (!StoreCommandHelper.IsQualityAllowed(quality))
                {
                    return $"Quality '{qualityStr}' is not allowed for purchases.";
                }

                // Get thing def
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(storeItem.DefName);
                if (thingDef == null)
                {
                    Logger.Error($"ThingDef not found: {storeItem.DefName}");
                    return $"Error: Item definition not found.";
                }

                // Parse material
                ThingDef material = null;
                if (thingDef.MadeFromStuff)
                {
                    material = StoreCommandHelper.ParseMaterial(materialStr, thingDef);
                    if (materialStr != "random" && material == null)
                    {
                        return $"Material '{materialStr}' is not valid for {itemName}.";
                    }
                }

                // Parse quantity
                if (!int.TryParse(quantityStr, out int quantity) || quantity < 1)
                {
                    quantity = 1;
                }

                Logger.Debug($"Parsed quantity: {quantity}");

                // Check quantity limits and clamp to maximum allowed
                if (storeItem.HasQuantityLimit && quantity > storeItem.QuantityLimit)
                {
                    Logger.Debug($"Quantity {quantity} exceeds limit of {storeItem.QuantityLimit} for {itemName}, clamping to maximum");
                    quantity = storeItem.QuantityLimit;
                }

                Logger.Debug($"Final quantity after limits: {quantity}");

                // Check quantity limits and clamp to maximum allowed
                if (storeItem.HasQuantityLimit && quantity > storeItem.QuantityLimit)
                {
                    Logger.Debug($"Quantity {quantity} exceeds limit of {storeItem.QuantityLimit} for {itemName}, clamping to maximum");
                    quantity = storeItem.QuantityLimit;
                }

                // Calculate final price
                int finalPrice = StoreCommandHelper.CalculateFinalPrice(storeItem, quantity, quality, material);

                // Check if user can afford
                if (!StoreCommandHelper.CanUserAfford(user, finalPrice))
                {
                    return $"You need {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)} to purchase {quantity}x {itemName}! You have {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";
                }

                // Get viewer's pawn for equip/wear/backpack commands
                Verse.Pawn viewerPawn = null;

                if (requireEquippable || requireWearable || addToInventory)
                {
                    viewerPawn = StoreCommandHelper.GetViewerPawn(user.Username);
                    if (viewerPawn == null)
                    {
                        return "You need to have a pawn in the colony. Use !buy pawn first.";
                    }

                    if (viewerPawn.Dead)
                    {
                        return "Your pawn is dead. You cannot equip/wear items.";
                    }
                }
                else
                {
                    // For regular buy commands, try to get the pawn but don't require it
                    viewerPawn = StoreCommandHelper.GetViewerPawn(user.Username);
                    // If no pawn, items will be delivered to a random colony location
                }

                // Deduct coins and process purchase
                viewer.TakeCoins(finalPrice);

                int karmaEarned = finalPrice / 100;
                if (karmaEarned > 0)
                {
                    viewer.GiveKarma(karmaEarned);
                    Logger.Debug($"Awarded {karmaEarned} karma for {finalPrice} coin purchase");
                }

                // Spawn the item
                if (requireEquippable || requireWearable)
                {
                    StoreCommandHelper.SpawnItemForPawn(thingDef, quantity, quality, material, viewerPawn, false, requireEquippable, requireWearable);
                }
                else
                {
                    StoreCommandHelper.SpawnItemForPawn(thingDef, quantity, quality, material, viewerPawn, addToInventory);
                }



                // Log success
                Logger.Debug($"Purchase successful: {user.Username} bought {quantity}x {itemName} for {finalPrice}{currencySymbol}");

                // Send appropriate letter notification
                string itemLabel = thingDef?.LabelCap ?? itemName;
                string invoiceLabel = "";
                string invoiceMessage = "";

                if (addToInventory || requireEquippable || requireWearable)
                {
                    // Backpack, Equip, and Wear all involve direct delivery to pawn
                    string serviceType = requireEquippable ? "Equip" : requireWearable ? "Wear" : "Backpack";
                    string emoji = requireEquippable ? "⚔️" : requireWearable ? "👕" : "🎒";

                    invoiceLabel = $"{emoji} Rimazon {serviceType} - {user.Username}";
                    invoiceMessage = CreateRimazonDirectInvoice(user.Username, itemLabel, quantity, finalPrice, currencySymbol, serviceType);
                }
                else
                {
                    // Regular drop pod delivery
                    invoiceLabel = $"🟡 Rimazon Delivery - {user.Username}";
                    invoiceMessage = CreateRimazonInvoice(user.Username, itemLabel, quantity, finalPrice, currencySymbol, quality, material);
                }

                if (IsMajorPurchase(finalPrice, quality))
                {
                    MessageHandler.SendGoldLetter(invoiceLabel, invoiceMessage);
                }
                else
                {
                    MessageHandler.SendGreenLetter(invoiceLabel, invoiceMessage);
                }


                // Return success message
                string action = addToInventory ? "added to your pawn's inventory" :
                              requireEquippable ? "equipped to your pawn" :
                              requireWearable ? "worn by your pawn" : "delivered via Rimazon";

                return $"Purchased {quantity}x {itemName} for {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)} and {action}! Remaining: {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";

            }
            catch (Exception ex)
            {
                Logger.Error($"Error in HandleBuyItem: {ex}");
                return "Error processing purchase. Please try again.";
            }
        }

        public static string HandleUseItem(ChatMessageWrapper user, string[] args)
        {
            try
            {
                Logger.Debug($"HandleUseItem called for user: {user.Username}, args: {string.Join(", ", args)}");

                if (args.Length == 0)
                {
                    return "Usage: !use <item> [quantity]";
                }

                var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
                var currencySymbol = settings.CurrencyName?.Trim() ?? "¢";
                var viewer = Viewers.GetViewer(user.Username);

                // Parse arguments
                string itemName = args[0];
                string quantityStr = args.Length > 1 ? args[1] : "1";

                // Get store item
                var storeItem = StoreCommandHelper.GetStoreItemByName(itemName);
                if (storeItem == null)
                {
                    return $"Item '{itemName}' not found in Rimazon.";
                }

                if (!storeItem.Enabled)
                {
                    return $"Item '{itemName}' is not available for purchase.";
                }

                if (!storeItem.IsUsable)
                {
                    return $"{itemName} is not a usable item.";
                }

                // Check research requirements
                if (!StoreCommandHelper.HasRequiredResearch(storeItem))
                {
                    return $"{itemName} requires research that hasn't been completed yet.";
                }

                // Parse quantity
                if (!int.TryParse(quantityStr, out int quantity) || quantity < 1)
                {
                    quantity = 1;
                }

                // Check quantity limits and clamp to maximum allowed
                if (storeItem.HasQuantityLimit && quantity > storeItem.QuantityLimit)
                {
                    Logger.Debug($"Quantity {quantity} exceeds limit of {storeItem.QuantityLimit} for {itemName}, clamping to maximum");
                    quantity = storeItem.QuantityLimit;
                }

                // Get viewer's pawn
                var viewerPawn = StoreCommandHelper.GetViewerPawn(user.Username);
                Verse.Pawn rimworldPawn = viewerPawn; // This is already a Verse.Pawn
                Logger.Debug($"Viewer pawn for {user.Username}: {(viewerPawn != null ? viewerPawn.Name.ToString() : "null")}");
                Logger.Debug($"Viewer pawn dead status: {(viewerPawn != null ? viewerPawn.Dead.ToString() : "N/A")}");
                Logger.Debug($"Rimworld pawn for {user.Username}: {(rimworldPawn != null ? rimworldPawn.Name.ToString() : "null")}");
                Logger.Debug($"Rimworld pawn dead status: {(rimworldPawn != null ? rimworldPawn.Dead.ToString() : "N/A")}");

                if (viewerPawn == null)
                {
                    return "You need to have a pawn in the colony to use items. Use !buy pawn first.";
                }

                // SPECIAL RESURRECTION LOGIC: Allow Resurrector Mech Serum on dead pawns
                bool isResurrectorSerum = storeItem.DefName == "MechSerumResurrector";

                if (viewerPawn.Dead && !isResurrectorSerum)
                {
                    return "Your pawn is dead. You cannot use items.";
                }

                // For resurrector serum on dead pawns, force quantity to 1
                if (isResurrectorSerum && viewerPawn.Dead)
                {
                    quantity = 1;
                    Logger.Debug($"Using Resurrector Mech Serum on dead pawn, quantity forced to 1");
                }

                // Calculate final price (no quality/material multipliers for usable items)
                int finalPrice = storeItem.BasePrice * quantity;

                // Check if user can afford
                if (!StoreCommandHelper.CanUserAfford(user, finalPrice))
                {
                    return $"You need {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)} to use {quantity}x {itemName}! You have {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";
                }

                // Get thing def
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(storeItem.DefName);
                if (thingDef == null)
                {
                    Logger.Error($"ThingDef not found: {storeItem.DefName}");
                    return $"Error: Item definition not found.";
                }

                // Deduct coins
                viewer.TakeCoins(finalPrice);

                int karmaEarned = finalPrice / 100;
                if (karmaEarned > 0)
                {
                    viewer.GiveKarma(karmaEarned);
                    Logger.Debug($"Awarded {karmaEarned} karma for {finalPrice} coin purchase");
                }

                // SPECIAL RESURRECTION: Handle resurrector serum differently
                if (isResurrectorSerum && viewerPawn.Dead)
                {
                    ResurrectPawn(viewerPawn);
                }
                else
                {
                    // Normal item usage
                    UseItemImmediately(thingDef, quantity, rimworldPawn);
                }

                // Send appropriate letter notification
                string itemLabel = thingDef?.LabelCap ?? itemName;
                string invoiceLabel;
                string invoiceMessage;

                if (isResurrectorSerum && viewerPawn.Dead)
                {
                    // Pink letter for resurrection
                    invoiceLabel = $"💖 Rimazon Resurrection - {user.Username}";
                    invoiceMessage = CreateRimazonResurrectionInvoice(user.Username, itemLabel, finalPrice, currencySymbol);
                    MessageHandler.SendPinkLetter(invoiceLabel, invoiceMessage);
                }
                else if (IsMajorPurchase(finalPrice, null)) // Don't check quality for use commands
                {
                    invoiceLabel = $"🔵 Rimazon Instant - {user.Username}";
                    invoiceMessage = CreateRimazonInstantInvoice(user.Username, itemLabel, quantity, finalPrice, currencySymbol);
                    MessageHandler.SendGoldLetter(invoiceLabel, invoiceMessage);
                }
                else
                {
                    invoiceLabel = $"🔵 Rimazon Instant - {user.Username}";
                    invoiceMessage = CreateRimazonInstantInvoice(user.Username, itemLabel, quantity, finalPrice, currencySymbol);
                    MessageHandler.SendBlueLetter(invoiceLabel, invoiceMessage); // Blue for instant/medical items
                }

                Logger.Debug($"Use item successful: {user.Username} used {quantity}x {itemName} for {finalPrice}{currencySymbol}");

                // Return appropriate success message
                if (isResurrectorSerum && viewerPawn.Dead)
                {
                    return $"💖 RESURRECTION! Used {itemName} to bring your pawn back to life for {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)}! Remaining: {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";
                }
                else
                {
                    return $"Used {quantity}x {itemName} for {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)}! Remaining: {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";
                }

            }
            catch (Exception ex)
            {
                Logger.Error($"Error in HandleUseItem: {ex}");
                return "Error using item. Please try again.";
            }
        }

        public static string HandleSurgery(ChatMessageWrapper user, string[] args)
        {
            try
            {
                Logger.Debug($"HandleSurgery called for user: {user.Username}, args: {string.Join(", ", args)}");

                if (args.Length == 0)
                {
                    return "Usage: !surgery <implant> [left/right] [quantity] - Example: !surgery bionic arm left 1";
                }

                var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
                var currencySymbol = settings.CurrencyName?.Trim() ?? "¢";
                var viewer = Viewers.GetViewer(user.Username);

                // Parse arguments - handle multi-word item names like "bionic arm"
                string itemName;
                string sideStr = null;
                string quantityStr = "1";

                if (args.Length >= 1)
                {
                    // Try to find the item name by combining arguments until we hit side/quantity keywords
                    var itemNameParts = new List<string>();

                    for (int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i].ToLower();

                        // Check if this argument could be a side or quantity
                        if (itemNameParts.Count > 0 && (IsSideKeyword(arg) || int.TryParse(arg, out _)))
                        {
                            // We've hit a non-item-name argument, stop collecting
                            break;
                        }

                        itemNameParts.Add(args[i]);
                    }

                    itemName = string.Join(" ", itemNameParts);

                    // Parse remaining arguments
                    int currentIndex = itemNameParts.Count;

                    // Parse side (if next arg is a side keyword)
                    if (args.Length > currentIndex && IsSideKeyword(args[currentIndex].ToLower()))
                    {
                        sideStr = args[currentIndex];
                        currentIndex++;
                    }

                    // Parse quantity (if next arg is a number) - lowest priority
                    if (args.Length > currentIndex && int.TryParse(args[currentIndex], out _))
                    {
                        quantityStr = args[currentIndex];
                    }

                    Logger.Debug($"Parsed - Item: '{itemName}', Side: '{sideStr}', Quantity: '{quantityStr}'");
                }
                else
                {
                    return "Usage: !surgery <implant> [left/right] [quantity] - Example: !surgery bionic arm left 1";
                }

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
                Verse.Pawn viewerPawn = StoreCommandHelper.GetViewerPawn(user.Username);
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

                // Check quantity limits
                if (storeItem.HasQuantityLimit && quantity > storeItem.QuantityLimit)
                {
                    Logger.Debug($"Quantity {quantity} exceeds limit of {storeItem.QuantityLimit} for {itemName}, clamping to maximum");
                    quantity = storeItem.QuantityLimit;
                }

                // Calculate final price (no quality/material for surgery items)
                int finalPrice = storeItem.BasePrice * quantity;

                // Check if user can afford
                if (!StoreCommandHelper.CanUserAfford(user, finalPrice))
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

                // Spawn the implant items in pawn's inventory
                StoreCommandHelper.SpawnItemForPawn(thingDef, quantity, null, null, viewerPawn, true); // Add to inventory

                // Schedule the surgeries
                ScheduleSurgeries(viewerPawn, recipe, bodyParts.Take(quantity).ToList());

                // Send notification
                string invoiceLabel = $"🏥 Rimazon Surgery - {user.Username}";
                string invoiceMessage = CreateRimazonSurgeryInvoice(user.Username, itemName, quantity, finalPrice, currencySymbol, bodyParts.Take(quantity).ToList());
                MessageHandler.SendBlueLetter(invoiceLabel, invoiceMessage);

                Logger.Debug($"Surgery scheduled: {user.Username} scheduled {quantity}x {itemName} for {finalPrice}{currencySymbol}");

                return $"Scheduled {quantity}x {itemName} surgery for {StoreCommandHelper.FormatCurrencyMessage(finalPrice, currencySymbol)}! Implant delivered to pawn's inventory. Remaining: {StoreCommandHelper.FormatCurrencyMessage(viewer.Coins, currencySymbol)}.";

            }
            catch (Exception ex)
            {
                Logger.Error($"Error in HandleSurgery: {ex}");
                return "Error scheduling surgery. Please try again.";
            }
        }

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

        private static RecipeDef FindSurgeryRecipeForImplant(ThingDef implantDef, Verse.Pawn pawn)
        {
            return DefDatabase<RecipeDef>.AllDefs
                .Where(r => r.IsSurgery && r.AvailableOnNow(pawn))
                .FirstOrDefault(r => r.ingredients.Any(i => i.filter.AllowedThingDefs.Contains(implantDef)));
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

        private static bool IsSideKeyword(string arg)
        {
            return arg.ToLower() switch
            {
                "left" or "right" or "l" or "r" => true,
                _ => false
            };
        }

        private static bool HasSurgeryScheduled(Verse.Pawn pawn, RecipeDef recipe, BodyPartRecord part)
        {
            return pawn.health.surgeryBills.Bills.Any(bill =>
                bill is Bill_Medical medicalBill &&
                medicalBill.recipe == recipe &&
                medicalBill.Part == part);
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

        private static void ScheduleSurgeries(Verse.Pawn pawn, RecipeDef recipe, List<BodyPartRecord> bodyParts)
        {
            foreach (var bodyPart in bodyParts)
            {
                var bill = new Bill_Medical(recipe, null) { Part = bodyPart };
                pawn.health.surgeryBills.AddBill(bill);
                Logger.Debug($"Scheduled {recipe.defName} on {bodyPart.Label} for pawn {pawn.Name}");
            }
        }

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
            invoice += $"Total: {price}{currencySymbol}\n";
            invoice += $"====================\n";
            invoice += $"Thank you for using Rimazon Surgery!\n";
            invoice += $"Implant delivered to pawn's inventory.\n";
            invoice += $"Surgery scheduled with colony doctors.";

            return invoice;
        }

        private static void UseItemImmediately(ThingDef thingDef, int quantity, Verse.Pawn pawn)
        {
            for (int i = 0; i < quantity; i++)
            {
                Thing thing = ThingMaker.MakeThing(thingDef);

                // Handle different types of usable items with appropriate sounds
                if (thingDef.IsIngestible && thingDef.ingestible != null)
                {
                    float nutritionWanted = pawn.needs.food?.NutritionWanted ?? 0f;
                    thing.Ingested(pawn, nutritionWanted);

                    // Use the ingest sound defined in the ingestible properties if available
                    if (thingDef.ingestible.ingestSound != null)
                    {
                        thingDef.ingestible.ingestSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                    }
                    else
                    {
                        // Fallback to our custom logic if no ingestSound is defined
                        PlayFallbackIngestSound(thingDef, pawn);
                    }
                }
                else if (thingDef.IsMedicine)
                {
                    // Medicine - add to inventory since immediate use is complex
                    if (!pawn.inventory.innerContainer.TryAdd(thing))
                    {
                        GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    }
                    SoundDefOf.Interact_Tend.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }
                else if (thingDef.defName.Contains("Psytrainer") || thingDef.defName.Contains("Neuroformer"))
                {
                    // Psy trainers and neuroformers - add to inventory
                    if (!pawn.inventory.innerContainer.TryAdd(thing))
                    {
                        GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    }
                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }
                else if (thingDef.defName.Contains("MechSerum"))
                {
                    // Mech serums - add to inventory
                    if (!pawn.inventory.innerContainer.TryAdd(thing))
                    {
                        GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    }
                    SoundDefOf.MechSerumUsed.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }
                else
                {
                    // Fallback for other usable items - add to inventory
                    if (!pawn.inventory.innerContainer.TryAdd(thing))
                    {
                        GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    }
                    SoundDefOf.Standard_Pickup.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }

                Logger.Debug($"Used item {thingDef.defName}, played sound effect");
            }
        }

        private static void PlayFallbackIngestSound(ThingDef thingDef, Verse.Pawn pawn)
        {
            if (thingDef.IsDrug)
            {
                // Use specific drug sounds based on drug type
                if (thingDef.ingestible.drugCategory == DrugCategory.Social || thingDef.defName.Contains("Smoke"))
                {
                    DefDatabase<SoundDef>.GetNamed("Ingest_Smoke").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }
                else if (thingDef.ingestible.drugCategory == DrugCategory.Hard)
                {
                    DefDatabase<SoundDef>.GetNamed("Ingest_Snort").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }
                else
                {
                    DefDatabase<SoundDef>.GetNamed("Ingest_Pill").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                }
            }
            else if (thingDef.ingestible.IsMeal)
            {
                DefDatabase<SoundDef>.GetNamed("Meal_Eat").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
            else if (thingDef.IsCorpse || thingDef.defName.Contains("Meat"))
            {
                SoundDefOf.RawMeat_Eat.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
            else if (thingDef.IsIngestible && thingDef.ingestible != null &&
                     (thingDef.ingestible.foodType & FoodTypeFlags.Liquor) != 0)
            {
                DefDatabase<SoundDef>.GetNamed("Ingest_Beer").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
            else
            {
                // Default for vegetables, fruits, etc.
                DefDatabase<SoundDef>.GetNamed("RawVegetable_Eat").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
        }

        private static bool IsMajorPurchase(int price, QualityCategory? quality)
        {
            // Legendary quality items
            if (quality.HasValue && quality.Value == QualityCategory.Legendary)
                return true;

            // Very expensive items (adjust threshold as needed)
            if (price >= 5000)
                return true;

            return false;
        }

        private static string CreateRimazonInvoice(string username, string itemName, int quantity, int price, string currencySymbol, QualityCategory? quality, ThingDef material)
        {
            string invoice = $"RIMAZON INVOICE\n";
            invoice += $"====================\n";
            invoice += $"Customer: {username}\n";
            invoice += $"Item: {itemName} x{quantity}\n";

            // Add quality info if specified
            if (quality.HasValue)
            {
                invoice += $"Quality: {quality.Value}\n";
            }

            // Add material info if specified and different from default
            if (material != null)
            {
                invoice += $"Material: {material.LabelCap}\n";
            }

            invoice += $"====================\n";
            invoice += $"Total: {price}{currencySymbol}\n";
            invoice += $"====================\n";
            invoice += $"Thank you for shopping with Rimazon!\n";
            invoice += $"Delivery: Standard Drop Pod\n";
            invoice += $"Satisfaction guaranteed or your coins back!";

            return invoice;
        }

        private static string CreateRimazonInstantInvoice(string username, string itemName, int quantity, int price, string currencySymbol)
        {
            string invoice = $"RIMAZON INSTANT\n";
            invoice += $"====================\n";
            invoice += $"Customer: {username}\n";
            invoice += $"Item: {itemName} x{quantity}\n";
            invoice += $"Service: Immediate Use\n";
            invoice += $"====================\n";
            invoice += $"Total: {price}{currencySymbol}\n";
            invoice += $"====================\n";
            invoice += $"Thank you for using Rimazon Instant!\n";
            invoice += $"No delivery required - instant satisfaction!";

            return invoice;
        }

        private static string CreateRimazonDirectInvoice(string username, string itemName, int quantity, int price, string currencySymbol, string serviceType)
        {
            string emoji = serviceType switch
            {
                "Equip" => "⚔️",
                "Wear" => "👕",
                "Backpack" => "🎒",
                _ => "📦"
            };

            string invoice = $"RIMAZON {serviceType.ToUpper()}\n";
            invoice += $"====================\n";
            invoice += $"Customer: {username}\n";
            invoice += $"Item: {itemName} x{quantity}\n";
            invoice += $"Service: Direct {serviceType}\n";
            invoice += $"====================\n";
            invoice += $"Total: {price}{currencySymbol}\n";
            invoice += $"====================\n";
            invoice += $"Thank you for using Rimazon {serviceType}!\n";

            // Custom message based on service type
            switch (serviceType)
            {
                case "Equip":
                    invoice += $"Weapon equipped and ready for action!";
                    break;
                case "Wear":
                    invoice += $"Apparel worn and looking stylish!";
                    break;
                case "Backpack":
                    invoice += $"Items delivered to your pawn's inventory.";
                    break;
            }

            return invoice;
        }

        public static string CreateRimazonResurrectionInvoice(string username, string itemName, int price, string currencySymbol)
        {
            string invoice = $"RIMAZON RESURRECTION SERVICE\n";
            invoice += $"====================\n";
            invoice += $"Customer: {username}\n";
            invoice += $"Service: Pawn Resurrection\n";
            invoice += $"Item: {itemName}\n";
            invoice += $"====================\n";
            invoice += $"Total: {price}{currencySymbol}\n";
            invoice += $"====================\n";
            invoice += $"Thank you for using Rimazon Resurrection!\n";
            invoice += $"Your pawn has been restored to life!\n";
            invoice += $"Life is precious - cherish every moment! 💖";

            return invoice;
        }

        public static void ResurrectPawn(Verse.Pawn pawn)
        {
            try
            {
                Logger.Debug($"Attempting to resurrect pawn: {pawn?.Name}");

                // Safety check - ensure pawn exists and is actually dead
                if (pawn == null)
                {
                    Logger.Error("Cannot resurrect - pawn is null");
                    return;
                }

                if (!pawn.Dead)
                {
                    Logger.Warning($"Pawn {pawn.Name} is not dead, cannot resurrect");
                    return;
                }

                // Check if pawn is completely destroyed (no corpse exists)
                if (IsPawnCompletelyDestroyed(pawn))
                {
                    Logger.Error($"Cannot resurrect {pawn.Name} - pawn is completely destroyed (no corpse exists)");
                    return;
                }

                Logger.Debug($"Resurrecting pawn: {pawn.Name}");

                // Use RimWorld's built-in resurrection method with side effects
                try
                {
                    ResurrectionUtility.TryResurrectWithSideEffects(pawn);
                }
                catch (NullReferenceException)
                {
                    Logger.Warning("Failed to revive with side effects -- falling back to regular revive");
                    ResurrectionUtility.TryResurrect(pawn);
                }

                Logger.Debug($"Successfully resurrected pawn: {pawn.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error resurrecting pawn: {ex}");
                throw;
            }
        }

        public static bool IsPawnCompletelyDestroyed(Verse.Pawn pawn)
        {
            try
            {
                // Check if the pawn exists as a corpse in any map
                foreach (var map in Find.Maps)
                {
                    foreach (var thing in map.listerThings.AllThings)
                    {
                        if (thing is Corpse corpse && corpse.InnerPawn == pawn)
                        {
                            return false; // Corpse exists, not completely destroyed
                        }
                    }
                }

                // Check if pawn exists in world pawns (dead)
                if (Find.WorldPawns.AllPawnsDead.Contains(pawn))
                {
                    return false; // Pawn exists in world pawns
                }

                // If we get here, the pawn is completely gone
                Logger.Debug($"Pawn {pawn.Name} is completely destroyed - no corpse found in any map or world pawns");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking if pawn is destroyed: {ex}");
                return true; // Assume destroyed if we can't check
            }
        }

        private static bool IsQualityKeyword(string arg)
        {
            return arg.ToLower() switch
            {
                "awful" or "poor" or "normal" or "good" or "excellent" or "masterwork" or "legendary" => true,
                _ => false
            };
        }

        private static HashSet<string> _materialKeywords = null;

        private static void InitializeMaterialKeywords()
        {
            if (_materialKeywords != null) return;

            _materialKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var allStuffDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.IsStuff);
                foreach (var stuffDef in allStuffDefs)
                {
                    // Add def name
                    _materialKeywords.Add(stuffDef.defName);

                    // Add label without spaces
                    if (!string.IsNullOrEmpty(stuffDef.label))
                    {
                        _materialKeywords.Add(stuffDef.label.Replace(" ", ""));
                    }

                    // Add raw label
                    _materialKeywords.Add(stuffDef.label);
                }

                Logger.Debug($"Initialized material keywords with {_materialKeywords.Count} entries");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initializing material keywords: {ex}");
                // Fallback to common materials
                _materialKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "wood", "steel", "plasteel", "cloth", "leather", "synthread", "hyperweave",
            "gold", "silver", "uranium", "jade", "component", "components"
        };
            }
        }

        public static bool IsMaterialKeyword(string arg)
        {
            InitializeMaterialKeywords();
            return _materialKeywords.Contains(arg);
        }
    }
}