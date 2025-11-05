// Windows/Window_Viewers.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Obosolete window to manage viewers
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive.Windows
{
    public class Window_Viewers : Window
    {
        public Window_Viewers()
        {
            doCloseButton = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Viewers Management");
            listing.GapLine();

            var activeViewers = Viewers.GetActiveViewers();
            listing.Label($"Active Viewers: {activeViewers.Count}");
            listing.Label($"Total Viewers: {Viewers.All.Count}");

            listing.Gap();

            if (listing.ButtonText("Award Coins to Active Viewers"))
            {
                Viewers.AwardActiveViewersCoins();
                Messages.Message("Coins awarded to active viewers", MessageTypeDefOf.NeutralEvent);
            }

            if (listing.ButtonText("Reset All Coins"))
            {
                Viewers.ResetAllCoins();
                Messages.Message("All viewer coins reset", MessageTypeDefOf.NeutralEvent);
            }

            listing.End();
        }
    }
}