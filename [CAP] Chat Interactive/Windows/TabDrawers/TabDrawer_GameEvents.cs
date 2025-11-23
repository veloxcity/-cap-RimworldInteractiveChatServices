// TabDrawer_GameEvents.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Draws the Game Events & Cooldowns tab in the mod settings window
using CAP_ChatInteractive.Incidents;
using CAP_ChatInteractive.Incidents.Weather;
using CAP_ChatInteractive.Store;
using CAP_ChatInteractive.Traits;
using System.Linq;
using UnityEngine;
using Verse;
using ColorLibrary = CAP_ChatInteractive.ColorLibrary;

namespace CAP_ChatInteractive
{
    public static class TabDrawer_GameEvents
    {
        private static Vector2 _scrollPosition = Vector2.zero;

        public static void Draw(Rect region)
        {
            var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
            var view = new Rect(0f, 0f, region.width - 16f, 800f);

            Widgets.BeginScrollView(region, ref _scrollPosition, view);
            var listing = new Listing_Standard();
            listing.Begin(view);

            // Header
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            listing.Label("Game Events & Cooldowns");
            Text.Font = GameFont.Small;
            GUI.color = ColorLibrary.White;
            listing.GapLine(6f);

            // Description
            listing.Label("Configure cooldowns for events, traits, and store purchases. Manage all game interactions in one place.");
            listing.Gap(12f);

            // COOLDOWN SETTINGS SECTION (Always available - no game required)
            DrawCooldownSettings(listing, settings);

            listing.Gap(24f);

            // OTHER SETTINGS SECTION
            DrawOtherSettings(listing, settings);

            listing.Gap(24f);

            // STATISTICS AND EDITORS SECTION
            DrawStatisticsAndEditors(listing);

            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawCooldownSettings(Listing_Standard listing, CAPGlobalChatSettings settings)
        {
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            listing.Label("Global Cooldown Settings");
            Text.Font = GameFont.Small;
            GUI.color = ColorLibrary.White;
            listing.GapLine(6f);

            // Event cooldown toggle
            listing.CheckboxLabeled("Enable event cooldowns", ref settings.EventCooldownsEnabled,
                "When enabled, events will go on cooldown after being purchased");

            // Cooldown days
            NumericField(listing, "Event cooldown duration in game days:", ref settings.EventCooldownDays, 1, 90);
            Text.Font = GameFont.Tiny;
            listing.Label($"All events will be unavailable for {settings.EventCooldownDays} in-game days.");
            Text.Font = GameFont.Small;

            // Events per cooldown period
            NumericField(listing, "Events per cooldown period:", ref settings.EventsperCooldown, 1, 1000);
            Text.Font = GameFont.Tiny;
            listing.Label($"Limit of {settings.EventsperCooldown} event purchases per cooldown period");
            Text.Font = GameFont.Small;

            listing.Gap(12f);

            // Karma type limits toggle
            listing.CheckboxLabeled("Limit events by karma type", ref settings.KarmaTypeLimitsEnabled,
                "Restrict how many events of each karma type can be purchased within the ");

            if (settings.KarmaTypeLimitsEnabled)
            {
                listing.Gap(4f);
                NumericField(listing, "Maximum bad event purchases:", ref settings.MaxBadEvents, 1, 100);
                NumericField(listing, "Maximum good event purchases:", ref settings.MaxGoodEvents, 1, 100);
                NumericField(listing, "Maximum neutral event purchases:", ref settings.MaxNeutralEvents, 1, 100);
            }

            listing.Gap(12f);

            // Store purchase limits
            NumericField(listing, "Maximum item purchases per period:", ref settings.MaxItemPurchases, 1, 1000);
            Text.Font = GameFont.Tiny;
            listing.Label($"Viewers can purchase up to {settings.MaxItemPurchases} items before cooldown");
            Text.Font = GameFont.Small;
        }

        private static void DrawOtherSettings(Listing_Standard listing, CAPGlobalChatSettings settings)
        {
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            listing.Label("Other Settings");
            Text.Font = GameFont.Small;
            GUI.color = ColorLibrary.White;
            listing.GapLine(6f);

            // Max Traits setting
            NumericField(listing, "Max traits for a pawn:", ref settings.MaxTraits, 1, 20);
            Text.Font = GameFont.Tiny;
            listing.Label($"Maximum number of traits a single pawn can have");
            Text.Font = GameFont.Small;
        }

        private static void DrawStatisticsAndEditors(Listing_Standard listing)
        {
            bool gameLoaded = Current.ProgramState == ProgramState.Playing;

            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            listing.Label("Event Management");
            Text.Font = GameFont.Small;
            GUI.color = ColorLibrary.White;
            listing.GapLine(6f);

            if (!gameLoaded)
            {
                listing.Label("Load a game to access event editors and statistics");
                listing.Gap(12f);
                return;
            }

            // Statistics row
            DrawStatisticsRow(listing);

            listing.Gap(12f);

            // Editor buttons row
            DrawEditorButtons(listing);

            listing.Gap(12f); // Add extra space after buttons
        }

        private static void DrawStatisticsRow(Listing_Standard listing)
        {
            // Calculate statistics
            int totalStoreItems = StoreInventory.AllStoreItems.Count;
            int enabledStoreItems = StoreInventory.GetEnabledItems().Count();

            int totalTraits = TraitsManager.AllBuyableTraits.Count;
            int enabledTraits = TraitsManager.GetEnabledTraits().Count();

            int totalWeather = BuyableWeatherManager.AllBuyableWeather.Count;
            int enabledWeather = BuyableWeatherManager.AllBuyableWeather.Values.Count(w => w.Enabled);

            // ADD EVENTS STATISTICS
            int totalEvents = IncidentsManager.AllBuyableIncidents?.Count ?? 0;
            int enabledEvents = IncidentsManager.AllBuyableIncidents?.Values.Count(e => e.Enabled) ?? 0;

            Text.Font = GameFont.Small;
            GUI.color = ColorLibrary.SkyBlue;
            listing.Label("Current Statistics:");
            Text.Font = GameFont.Tiny;

            listing.Label($"  • Store: {enabledStoreItems}/{totalStoreItems} items enabled");
            listing.Label($"  • Traits: {enabledTraits}/{totalTraits} traits enabled");
            listing.Label($"  • Weather: {enabledWeather}/{totalWeather} types enabled");
            listing.Label($"  • Events: {enabledEvents}/{totalEvents} events enabled"); // ADD THIS LINE

            Text.Font = GameFont.Small;
        }

        private static void DrawEditorButtons(Listing_Standard listing)
        {
            // Create a rect for the button row
            Rect buttonRow = listing.GetRect(30f);
            float buttonWidth = (buttonRow.width - 30f) / 4f; // Changed from 20f/3f to 30f/4f for 4 buttons

            // Store Editor Button
            Rect storeRect = new Rect(buttonRow.x, buttonRow.y, buttonWidth, 30f);
            if (Widgets.ButtonText(storeRect, "Store Editor"))
            {
                Find.WindowStack.Add(new Dialog_StoreEditor());
            }

            // Traits Editor Button  
            Rect traitsRect = new Rect(buttonRow.x + buttonWidth + 10f, buttonRow.y, buttonWidth, 30f);
            if (Widgets.ButtonText(traitsRect, "Traits Editor"))
            {
                Find.WindowStack.Add(new Dialog_TraitsEditor());
            }

            // Weather Editor Button
            Rect weatherRect = new Rect(buttonRow.x + (buttonWidth + 10f) * 2, buttonRow.y, buttonWidth, 30f);
            if (Widgets.ButtonText(weatherRect, "Weather Editor"))
            {
                Find.WindowStack.Add(new Dialog_WeatherEditor());
            }

            // Events Editor Button - NEW
            Rect eventsRect = new Rect(buttonRow.x + (buttonWidth + 10f) * 3, buttonRow.y, buttonWidth, 30f);
            if (Widgets.ButtonText(eventsRect, "Events Editor"))
            {
                Find.WindowStack.Add(new Dialog_EventsEditor());
            }
        }

        private static void NumericField(Listing_Standard listing, string label, ref int value, int min, int max)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect leftRect = rect.LeftPart(0.6f).Rounded();
            Rect rightRect = rect.RightPart(0.4f).Rounded();

            Widgets.Label(leftRect, label);
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(rightRect, ref value, ref buffer, min, max);
        }
    }
}