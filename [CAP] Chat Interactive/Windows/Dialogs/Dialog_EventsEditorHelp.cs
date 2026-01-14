// Dialog_EventsEditorHelp.cs
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
// A help dialog for the Events Editor
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive
{
    public class Dialog_EventsEditorHelp : Window
    {
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public Dialog_EventsEditorHelp()
        {
            doCloseButton = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            resizeable = true;
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Title
            Rect titleRect = new Rect(0f, 0f, inRect.width, 35f);
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.HeaderAccent;
            Widgets.Label(titleRect, "Events Editor Help");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Content area
            Rect contentRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f - CloseButSize.y);
            DrawHelpContent(contentRect);
        }

        // Update Dialog_EventsEditorHelp.cs - add cooldown information
        private void DrawHelpContent(Rect rect)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<b>Events Editor Overview</b>");
            sb.AppendLine($"The Events Editor allows you to configure which incidents (events) are available for purchase via chat commands, and set their prices and karma types.");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Main Interface Sections:</b>");
            sb.AppendLine($"1. <b>Left Panel</b> - Filter events by Mod Source or Category");
            sb.AppendLine($"   • Click the panel header to toggle between Mod Sources and Categories");
            sb.AppendLine($"   • Click any item to filter events");
            sb.AppendLine($"   • Numbers in parentheses show event counts");
            sb.AppendLine($"");

            sb.AppendLine($"2. <b>Right Panel</b> - Event list with editing controls");
            sb.AppendLine($"   • Events are listed with name, source mod, and category");
            sb.AppendLine($"   • Click an event name to view detailed information");
            sb.AppendLine($"   • Use checkboxes to enable/disable events");
            sb.AppendLine($"   • Set custom prices and karma types for each event");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Header Controls:</b>");
            sb.AppendLine($"• <b>Search Bar</b> - Filter events by name, description, or mod");
            sb.AppendLine($"• <b>Sort Buttons</b> - Sort events by Name, Cost, or Karma type");
            sb.AppendLine($"   - Click once to sort by that field");
            sb.AppendLine($"   - Click again to reverse sort order");
            sb.AppendLine($"   - Current sort direction is shown by arrow/icons");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Action Buttons:</b>");
            sb.AppendLine($"• <b>Reset Prices</b> - Reset all event prices to default values");
            sb.AppendLine($"• <b>Enable →</b> - Bulk enable events by mod source");
            sb.AppendLine($"• <b>Disable →</b> - Bulk disable events by mod source");
            sb.AppendLine($"• <b>Cooldowns →</b> - Bulk manage event cooldowns (see below)");
            sb.AppendLine($"");

            sb.AppendLine($"<b>NEW: Cooldowns System</b>");
            sb.AppendLine($"Events can now have individual cooldowns to prevent spamming:");
            sb.AppendLine($"");

            sb.AppendLine($"<b>How Cooldowns Work:</b>");
            sb.AppendLine($"• <b>Cooldown Days</b> - Number of game days before the same event can be triggered again");
            sb.AppendLine($"• <b>0 Days</b> = No cooldown (infinite) - event can be used repeatedly");
            sb.AppendLine($"• <b>1+ Days</b> = Event goes on cooldown after use");
            sb.AppendLine($"• Cooldowns are global - if anyone triggers an event, it goes on cooldown for everyone");
            sb.AppendLine($"• Only applies if 'Event Cooldowns Enabled' is ON in global settings");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Cooldown Controls:</b>");
            sb.AppendLine($"• <b>CD Input Box</b> - Set cooldown days for individual events");
            sb.AppendLine($"   - Displays ∞ symbol for 0 days (no cooldown)");
            sb.AppendLine($"   - Click ∞ to set a cooldown, or click number to set back to ∞");
            sb.AppendLine($"   - Tooltip shows explanation and current setting");
            sb.AppendLine($"• <b>Cooldowns → Button</b> - Bulk cooldown operations");
            sb.AppendLine($"   - <b>Presets</b>: Quickly set 0, 1, 3, 5, 7, or 14 day cooldowns");
            sb.AppendLine($"   - <b>Reset Filtered</b>: Reset cooldowns to defaults for filtered events");
            sb.AppendLine($"   - <b>Reset ALL</b>: Reset ALL event cooldowns to defaults");
            sb.AppendLine($"   - <b>Custom...</b>: Set any value (0-1000 days) with filtered-only option");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Default Cooldowns:</b>");
            sb.AppendLine($"• <b>Raids</b>: 7 days (long cooldown for major threats)");
            sb.AppendLine($"• <b>Diseases</b>: 5 days (moderate cooldown)");
            sb.AppendLine($"• <b>Weather Events</b>: 3 days (shorter cooldown)");
            sb.AppendLine($"• <b>Quests</b>: 10 days (longest cooldown for rare/special events)");
            sb.AppendLine($"• <b>Other Events</b>: 1 day (minimal cooldown)");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Event Row Controls:</b>");
            sb.AppendLine($"• <b>Enabled Checkbox</b> - Toggle if event is available for purchase");
            sb.AppendLine($"   - Grayed out if event is not available via commands");
            sb.AppendLine($"• <b>Cost Field</b> - Set the price in points");
            sb.AppendLine($"   - Use the Reset button to restore default price");
            sb.AppendLine($"• <b>Karma Dropdown</b> - Set karma type: Good, Bad, or Neutral");
            sb.AppendLine($"   - Affects player karma when event is purchased");
            sb.AppendLine($"• <b>Karma Type</b> - Shows current karma type in color (Good/Bad/Neutral)");
            sb.AppendLine($"• <b>CD Input</b> - Set cooldown days (see above)");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Event Availability:</b>");
            sb.AppendLine($"• Some events may show as 'UNAVAILABLE' if they cannot be triggered via commands");
            sb.AppendLine($"• These events are grayed out and cannot be enabled");
            sb.AppendLine($"• Toggle visibility of unavailable events in Settings");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Settings (Gear Icon):</b>");
            sb.AppendLine($"• Configure global settings for the Events system");
            sb.AppendLine($"• Show/Hide unavailable events");
            sb.AppendLine($"• Configure default pricing multipliers");
            sb.AppendLine($"• Set global event caps and cooldowns");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Event Information Window:</b>");
            sb.AppendLine($"• Click any event name to open detailed information");
            sb.AppendLine($"• Shows all incident properties and analysis");
            sb.AppendLine($"• Useful for debugging or understanding event mechanics");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Label Customization:</b>");
            sb.AppendLine($"• Click any event name to open the detailed information window");
            sb.AppendLine($"• Use the label edit box at the top to customize the display name");
            sb.AppendLine($"• Click 'Save' to save your custom label to JSON");
            sb.AppendLine($"• Click 'Reset' to restore the original name from the game files");
            sb.AppendLine($"• Custom labels will appear in the Events Editor and chat store");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Filtered Operations:</b>");
            sb.AppendLine($"• When using Cooldowns → Custom... option:");
            sb.AppendLine($"   - Check 'Apply to filtered events only' to affect only current filter");
            sb.AppendLine($"   - Useful for setting different cooldowns for different event types");
            sb.AppendLine($"   - Example: Set raids to 7 days, weather events to 3 days");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Tips:</b>");
            sb.AppendLine($"• Changes are saved automatically when you close the window");
            sb.AppendLine($"• Use bulk operations to quickly enable/disable mods");
            sb.AppendLine($"• Sort events by cost to find expensive or cheap events");
            sb.AppendLine($"• Search supports partial matching (case-insensitive)");
            sb.AppendLine($"• Events with 'UNAVAILABLE' tag won't appear in chat store");
            sb.AppendLine($"• Set longer cooldowns for powerful/expensive events");
            sb.AppendLine($"• Set 0 cooldown for minor events you want to be spammable");

            string fullText = sb.ToString();

            // Calculate text height with proper formatting
            float textHeight = Text.CalcHeight(fullText, rect.width - 30f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, textHeight + 20f);

            // Scroll view with background
            Widgets.DrawMenuSection(rect);
            Widgets.BeginScrollView(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f),
                                   ref scrollPosition, viewRect);

            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            Widgets.Label(new Rect(0f, 0f, viewRect.width, textHeight), fullText);
            GUI.color = Color.white;

            Widgets.EndScrollView();
        }
    }
}