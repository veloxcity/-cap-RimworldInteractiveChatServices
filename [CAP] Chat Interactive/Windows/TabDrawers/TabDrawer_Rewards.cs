// TabDrawer_Rewards.cs
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
// Draws the Rewards Settings tab (Channel Points & Lootboxes)
using CAP_ChatInteractive;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace _CAP__Chat_Interactive
{
    public static class TabDrawer_Rewards
    {
        private static Vector2 _scrollPosition = Vector2.zero;
        private static float _lineHeight = 30f;
        // private static float _iconWidth = 24f;

        public static void Draw(Rect region)
        {
            var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
            var view = new Rect(0f, 0f, region.width - 16f, 1200f);

            Widgets.BeginScrollView(region, ref _scrollPosition, view);
            var listing = new Listing_Standard();
            listing.Begin(view);

            // Channel Points Section
            Text.Font = GameFont.Medium;
            // OLD: listing.Label("Twitch Channel Points");
            listing.Label("RICS.Rewards.ChannelPointsSettingsHeader".Translate());
            Text.Font = GameFont.Small;
            listing.GapLine(6f);

            // Important note about channel points
            Text.Font = GameFont.Tiny;
            GUI.color = Color.yellow;
            //listing.Label("Important: Twitch rewards must be set to 'Require Viewer to Enter Text' or they won't be detected!");
            listing.Label("RICS.Rewards.ChannelPointsImportantNote".Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            listing.Gap(8f);

            // OLD: listing.CheckboxLabeled("Enable Channel Points Rewards", ref settings.ChannelPointsEnabled);
            listing.CheckboxLabeled("RICS.Rewards.EnableChannelPointsRewardsLabel".Translate(), ref settings.ChannelPointsEnabled);
            // OLD: listing.CheckboxLabeled("Show Channel Points Debug Messages", ref settings.ShowChannelPointsDebugMessages);
            listing.CheckboxLabeled("RICS.Rewards.ChannelPointsDebugMessagesLabel".Translate(), ref settings.ShowChannelPointsDebugMessages);

            listing.Gap(12f);

            // Channel Points Rewards Table
            DrawChannelPointsTable(listing, settings);

            listing.Gap(24f);

            // Lootbox Section
            Text.Font = GameFont.Medium;
            // OLD: listing.Label("Loot Box Settings");
            listing.Label("RICS.Rewards.LootBoxSettingsHeader".Translate());
            Text.Font = GameFont.Small;
            listing.GapLine(6f);

            // Coin range
            // OLD: listing.Label("Coin Range (1-10000):");
            listing.Label("RICS.Rewards.LootBoxCoinRangeDescription".Translate());
            listing.Gap(4f);

            // Create a horizontal layout for min/max inputs
            var coinRangeRect = listing.GetRect(Text.LineHeight);
            var coinMinRect = new Rect(coinRangeRect.x, coinRangeRect.y, 80f, Text.LineHeight);
            var coinMinInputRect = new Rect(coinMinRect.xMax + 4f, coinRangeRect.y, 60f, Text.LineHeight);
            var coinMaxLabelRect = new Rect(coinMinInputRect.xMax + 8f, coinRangeRect.y, 80f, Text.LineHeight);
            var coinMaxInputRect = new Rect(coinMaxLabelRect.xMax + 4f, coinRangeRect.y, 60f, Text.LineHeight);

            // OLD: Widgets.Label(coinMinRect, "Min:");
            Widgets.Label(coinMinRect, "RICS.Rewards.LootBoxCoinRangeMinLabel".Translate());
            string coinMinBuffer = settings.LootBoxRandomCoinRange.min.ToString();
            UIUtilities.TextFieldNumericFlexible(coinMinInputRect, ref settings.LootBoxRandomCoinRange.min, ref coinMinBuffer, 1, 10000);

            // OLD: Widgets.Label(coinMaxLabelRect, "Max:");
            Widgets.Label(coinMaxLabelRect, "RICS.Rewards.LootBoxCoinRangeMaxLabel".Translate());
            string coinMaxBuffer = settings.LootBoxRandomCoinRange.max.ToString();
            UIUtilities.TextFieldNumericFlexible(coinMaxInputRect, ref settings.LootBoxRandomCoinRange.max, ref coinMaxBuffer, 1, 10000);

            listing.Gap(8f);

            // Lootboxes per day
            var perDayRect = listing.GetRect(Text.LineHeight);
            var perDayLabelRect = new Rect(perDayRect.x, perDayRect.y, 150f, Text.LineHeight);
            var perDayInputRect = new Rect(perDayLabelRect.xMax + 8f, perDayRect.y, 80f, Text.LineHeight);

            // OLD: Widgets.Label(perDayLabelRect, "Lootboxes Per Day:");
            Widgets.Label(perDayLabelRect, "RICS.Rewards.LootBoxesPerDayLabel".Translate());
            string perDayBuffer = settings.LootBoxesPerDay.ToString();
            UIUtilities.TextFieldNumericFlexible(perDayInputRect, ref settings.LootBoxesPerDay, ref perDayBuffer, 1, 20);

            listing.Gap(8f);

            // Show welcome message
            // OLD: listing.CheckboxLabeled("Show Welcome Message", ref settings.LootBoxShowWelcomeMessage);
            listing.CheckboxLabeled("RICS.Rewards.ShowWelcomeMessageLabel".Translate(), ref settings.LootBoxShowWelcomeMessage);
            listing.Gap(4f);

            // Force open all at once
            // OLD: listing.CheckboxLabeled("Force Open All At Once", ref settings.LootBoxForceOpenAllAtOnce);
            listing.CheckboxLabeled("RICS.Rewards.ForceOpenAllAtOnceLabel".Translate(), ref settings.LootBoxForceOpenAllAtOnce);    

            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawChannelPointsTable(Listing_Standard listing, CAPGlobalChatSettings settings)
        {
            // Safety check - ensure RewardSettings is not null
            if (settings.RewardSettings == null)
            {
                settings.RewardSettings = new List<ChannelPoints_RewardSettings>();
            }

            // Table headers
            var headerRect = listing.GetRect(_lineHeight);
            var nameWidth = headerRect.width * 0.25f;
            var uuidWidth = headerRect.width * 0.35f;
            var coinsWidth = headerRect.width * 0.15f;
            var autoWidth = headerRect.width * 0.1f;
            var enabledWidth = headerRect.width * 0.1f;
            var deleteWidth = headerRect.width * 0.05f;

            var headerRow = new WidgetRow(headerRect.x, headerRect.y, UIDirection.RightThenDown);
            // OLD: headerRow.Label("Reward Name", nameWidth);
            headerRow.Label("RICS.Rewards.RewardNameHeader".Translate(), nameWidth);    
            // OLD: headerRow.Label("Reward UUID", uuidWidth);
            headerRow.Label("RICS.Rewards.RewardUUIDHeader".Translate(), uuidWidth);
            // OLD: headerRow.Label("Coins", coinsWidth);
            headerRow.Label("RICS.Rewards.CoinsHeader".Translate(), coinsWidth);
            // OLD: headerRow.Label("Auto", autoWidth);
            headerRow.Label("RICS.Rewards.AutoCaptureHeader".Translate(), autoWidth);
            // OLD: headerRow.Label("Enabled", enabledWidth);
            headerRow.Label("RICS.Rewards.EnabledHeader".Translate(), enabledWidth);
            headerRow.Label("", deleteWidth);



            listing.Gap(4f);

            // Safety check for empty list
            if (settings.RewardSettings.Count == 0)
            {
                var emptyRect = listing.GetRect(_lineHeight);
                // OLD: Widgets.Label(emptyRect, "No rewards configured. Add one below.");
                Widgets.Label(emptyRect, "RICS.Rewards.NoRewardsConfigured".Translate());
                listing.Gap(8f);
            }
            else
            {
                // Reward rows
                for (int i = 0; i < settings.RewardSettings.Count; i++)
                {
                    var reward = settings.RewardSettings[i];
                    // Safety check for null reward
                    if (reward == null)
                    {
                        settings.RewardSettings[i] = new ChannelPoints_RewardSettings();
                        reward = settings.RewardSettings[i];
                    }

                    var rowRect = listing.GetRect(_lineHeight);

                    // Name field
                    var nameRect = new Rect(rowRect.x, rowRect.y, nameWidth, _lineHeight);
                    reward.RewardName = Widgets.TextField(nameRect, reward.RewardName ?? "");

                    // UUID field
                    var uuidRect = new Rect(nameRect.xMax, rowRect.y, uuidWidth, _lineHeight);
                    reward.RewardUUID = Widgets.TextField(uuidRect, reward.RewardUUID ?? "");

                    // Coins field
                    var coinsRect = new Rect(uuidRect.xMax, rowRect.y, coinsWidth, _lineHeight);
                    reward.CoinsToAward = Widgets.TextField(coinsRect, reward.CoinsToAward ?? "300");

                    // Auto capture toggle
                    var autoRect = new Rect(coinsRect.xMax, rowRect.y, autoWidth, _lineHeight);
                    Widgets.Checkbox(autoRect.position, ref reward.AutomaticallyCaptureUUID);

                    // Enabled toggle
                    var enabledRect = new Rect(autoRect.xMax, rowRect.y, enabledWidth, _lineHeight);
                    Widgets.Checkbox(enabledRect.position, ref reward.Enabled);

                    // Delete button
                    var deleteRect = new Rect(enabledRect.xMax, rowRect.y, deleteWidth, _lineHeight);
                    if (Widgets.ButtonText(deleteRect, "RICS.Rewards.DeleteRewardButton".Translate()))
                    {
                        settings.RewardSettings.RemoveAt(i);
                        i--; // Adjust index after removal
                    }

                    listing.Gap(4f);
                }
            }

            // Add new reward button
            var addButtonRect = listing.GetRect(30f);
            // OLD: if (Widgets.ButtonText(addButtonRect, "Add New Reward"))
            if (Widgets.ButtonText(addButtonRect, "RICS.Rewards.AddNewRewardButton".Translate()))
            {
                // Ensure list is initialized
                if (settings.RewardSettings == null)
                {
                    settings.RewardSettings = new List<ChannelPoints_RewardSettings>();
                }

                settings.RewardSettings.Add(new ChannelPoints_RewardSettings(
                    // OLD: "New Reward",
                    "RICS.Rewards.NewRewardDefaultName".Translate(),
                    "",
                    "300",
                    false,
                    true
                ));
            }

            listing.Gap(12f);

            // Auto-capture explanation
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.7f, 0.7f, 0.7f);
            // OLD: listing.Label("Auto Capture: When enabled, this reward will automatically capture the next UUID redeemed on Twitch");
            listing.Label("RICS.Rewards.AutoCaptureExplanation".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }
    }
}