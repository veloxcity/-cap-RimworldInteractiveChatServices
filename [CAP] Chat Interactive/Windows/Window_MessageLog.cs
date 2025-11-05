// Windows/Window_MessageLog.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// A window to display the chat message log
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive.Windows
{
    public class Window_MessageLog : Window
    {
        public Window_MessageLog()
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

            listing.Label("Chat Message Log");
            listing.GapLine();

            // TODO: Implement message log display
            listing.Label("Message log functionality coming soon...");

            listing.End();
        }
    }
}