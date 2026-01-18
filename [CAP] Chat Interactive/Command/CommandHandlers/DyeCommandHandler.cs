// DyeCommandHandler.cs
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

using CAP_ChatInteractive.Commands.CommandHandlers;
using CAP_ChatInteractive.Helpers;
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    internal static class DyeCommandHandler
    {
        // In DyeCommandHandler.cs, update the HandleDyeCommand method
        internal static string HandleDyeCommand(ChatMessageWrapper messageWrapper, string[] args)
        {
            // Get the viewer's pawn
            Verse.Pawn viewerPawn = StoreCommandHelper.GetViewerPawn(messageWrapper);
            if (viewerPawn == null)
            {
                return "You need to have a pawn in the colony to dye thier clothing. Use !buy pawn first.";
            }

            // Parse color from arguments
            Color? color = null;
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                color = ColorHelper.ParseColor(args[0]);
                if (!color.HasValue)
                {
                    return $"'{args[0]}' is not a valid color. Use color names or hex codes like #FF0000.";
                }
            }

            // Use favorite color if no color specified, but check for Ideology DLC
            if (!color.HasValue)
            {
                if (!ModsConfig.IdeologyActive)
                {
                    return "Please specify a color. The favorite color system requires the Ideology DLC.";
                }

                color = viewerPawn.story?.favoriteColor?.color ?? new Color(0.6f, 0.6f, 0.6f);
            }

            // Apply dye to appropriate apparel
            int dyedCount = ApplyDyeToApparel(viewerPawn, color.Value);

            if (dyedCount == 0)
            {
                return "No dyeable clothing found on your pawn.";
            }

            return $"Successfully dyed {dyedCount} piece(s) of clothing.";
        }

        private static int ApplyDyeToApparel(Verse.Pawn pawn, Color color)
        {
            int count = 0;
            var apparel = pawn.apparel?.WornApparel;

            if (apparel == null) return 0;

            foreach (var item in apparel)
            {
                if (IsDyeableApparel(item))
                {
                    var comp = item.TryGetComp<CompColorable>();
                    if (comp != null)
                    {
                        comp.SetColor(color);
                        count++;
                    }
                }
            }

            return count;
        }

        private static bool IsDyeableApparel(Apparel apparel)
        {
            // Exclude jewelry, accessories, and utility items
            if (apparel.def == null) return false;

            // Check for jewelry by defName or label
            string defName = apparel.def.defName?.ToLower() ?? "";
            string label = apparel.def.label?.ToLower() ?? "";

            // Exclude common jewelry and accessory types
            if (defName.Contains("jewelry") || label.Contains("jewelry") ||
                defName.Contains("earring") || label.Contains("earring") ||
                defName.Contains("necklace") || label.Contains("necklace") ||
                defName.Contains("ring") || label.Contains("ring") ||
                defName.Contains("bracelet") || label.Contains("bracelet") ||
                defName.Contains("crown") || label.Contains("crown") ||
                defName.Contains("tiara") || label.Contains("tiara"))
            {
                return false;
            }

            // Check apparel tags for exclusion
            if (apparel.def.apparel?.tags != null)
            {
                var tags = apparel.def.apparel.tags;
                if (tags.Contains("Jewelry") || tags.Contains("Accessory") || tags.Contains("Utility"))
                {
                    return false;
                }
            }

            // Check for utility slot items
            if (IsUtilitySlotItem(apparel))
            {
                return false;
            }

            return true;
        }

        private static bool IsUtilitySlotItem(Apparel apparel)
        {
            // Check if this apparel goes in utility slots
            var layers = apparel.def.apparel?.layers;
            if (layers == null) return false;

            // Utility items are typically in the Belt layer
            if (layers.Contains(ApparelLayerDefOf.Belt))
            {
                return true;
            }

            // Check apparel tags for utility items
            if (apparel.def.apparel?.tags != null)
            {
                var tags = apparel.def.apparel.tags;
                if (tags.Contains("Utility") || tags.Contains("Belt") || tags.Contains("Holster"))
                {
                    return true;
                }
            }

            // Additional check for utility-related defNames
            string defName = apparel.def.defName?.ToLower() ?? "";
            return defName.Contains("utility") || defName.Contains("belt") || defName.Contains("holster") ||
                   defName.Contains("tool") || defName.Contains("pouch");
        }
    }
}