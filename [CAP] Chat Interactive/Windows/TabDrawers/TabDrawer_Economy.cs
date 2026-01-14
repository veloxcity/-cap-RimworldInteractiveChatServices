// TabDrawer_Economy.cs
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
// Draws the Economy settings tab in the mod settings window
using CAP_ChatInteractive;
using UnityEngine;
using Verse;
using ColorLibrary = CAP_ChatInteractive.ColorLibrary;

namespace _CAP__Chat_Interactive
{
    public static class TabDrawer_Economy
    {
        public static void Draw(Rect rect)
        {
            var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
            var listing = new Listing_Standard();
            listing.Begin(rect);

            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.HeaderAccent;
            listing.Label("RICS.Economy.EconomySettingsHeader".Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            listing.GapLine(6f);

            // Coin Settings
            GUI.color = ColorLibrary.SubHeader;
            listing.Label("RICS.Economy.CoinEconomyHeader".Translate());
            GUI.color = Color.white;
            UIUtilities.NumericField(listing, "RICS.Economy.StartingCoins".Translate(), "RICS.Economy.StartingCoinsDesc".Translate(), ref settings.StartingCoins, 0, 10000);
            UIUtilities.NumericField(listing, "RICS.Economy.BaseCoinReward".Translate(), "RICS.Economy.BaseCoinRewardDesc".Translate(), ref settings.BaseCoinReward, 1, 100);
            UIUtilities.NumericField(listing, "RICS.Economy.SubscriberExtraCoins".Translate(), "RICS.Economy.SubscriberExtraCoinsDesc".Translate(), ref settings.SubscriberExtraCoins, 0, 50);
            UIUtilities.NumericField(listing, "RICS.Economy.VIPExtraCoins".Translate(), "RICS.Economy.VIPExtraCoinsDesc".Translate(), ref settings.VipExtraCoins, 0, 50);
            UIUtilities.NumericField(listing, "RICS.Economy.ModExtraCoins".Translate(), "RICS.Economy.ModExtraCoinsDesc".Translate(), ref settings.ModExtraCoins, 0, 50);

            listing.Gap(12f);

            // Karma Settings
            GUI.color = ColorLibrary.SubHeader;
            listing.Label("RICS.Economy.KarmaSystemHeader".Translate());
            GUI.color = Color.white;
            UIUtilities.NumericField(listing, "RICS.Economy.StartingKarma".Translate(), "RICS.Economy.StartingKarmaDesc".Translate(), ref settings.StartingKarma, 0, 200);

            // Min Karma with validation
            int originalMinKarma = settings.MinKarma;
            UIUtilities.NumericField(listing, "RICS.Economy.MinimumKarma".Translate(), "RICS.Economy.MinimumKarmaDesc".Translate(), ref settings.MinKarma, 0, 200);
            if (settings.MinKarma != originalMinKarma && settings.MinKarma > settings.MaxKarma)
            {
                settings.MinKarma = settings.MaxKarma;
            }

            // Max Karma with validation  
            int originalMaxKarma = settings.MaxKarma;
            UIUtilities.NumericField(listing, "RICS.Economy.MaximumKarma".Translate(), "RICS.Economy.MaximumKarmaDesc".Translate(), ref settings.MaxKarma, 0, 1000);
            if (settings.MaxKarma != originalMaxKarma && settings.MaxKarma < settings.MinKarma)
            {
                settings.MaxKarma = settings.MinKarma;
            }

            listing.Gap(12f);

            UIUtilities.NumericField(listing, "RICS.Economy.ActiveViewerMinutes".Translate(), "RICS.Economy.ActiveViewerMinutesDesc".Translate(), ref settings.MinutesForActive, 1, 1440);
            listing.Gap(12f);

            // Currency
            GUI.color = ColorLibrary.SubHeader;
            listing.Label("RICS.Economy.CurrencyNameHeader".Translate());
            GUI.color = Color.white;
            listing.Gap(6f);

            Rect currencyLabelRect = listing.GetRect(Text.LineHeight);
            UIUtilities.LabelWithDescription(currencyLabelRect, "RICS.Economy.CurrencyNameDesc".Translate(), "RICS.Economy.CurrencyNameExample".Translate());
            // Current value 
            listing.Gap(6f);
            listing.Label(string.Format("RICS.Economy.CurrentCurrencyDisplay".Translate(), settings.CurrencyName));


            // Text entry field
            settings.CurrencyName = listing.TextEntry(settings.CurrencyName).Trim();
            listing.Gap(6f);

            listing.End();
        }
    }
}