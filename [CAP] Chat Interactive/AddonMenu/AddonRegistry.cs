// AddonRegistry.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// This class manages the registry of Chat Interactive addons, loading enabled addons and providing access to their menus.
using CAP_ChatInteractive.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive
{
    [StaticConstructorOnStartup]
    public static class AddonRegistry
    {
        public static List<ChatInteractiveAddonDef> AddonDefs { get; private set; }

        static AddonRegistry()
        {
            AddonDefs = DefDatabase<ChatInteractiveAddonDef>.AllDefs
                .Where(def => def.enabled)
                .OrderBy(def => def.displayOrder)
                .ToList();

            // Logger.Debug($"Loaded {AddonDefs.Count} addon defs");
        }

        public static IAddonMenu GetMainMenu()
        {
            var mainDef = AddonDefs.FirstOrDefault();
            return mainDef?.GetAddonMenu();
        }
    }
}