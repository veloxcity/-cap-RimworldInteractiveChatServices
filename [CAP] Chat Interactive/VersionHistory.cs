using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Add this class to a new file or within your existing files

namespace CAP_ChatInteractive
{
    public static class VersionHistory
    {
        public static Dictionary<string, string> UpdateNotes = new Dictionary<string, string>
        {
            {
                "1.0.14",
                @"Xenotype Pricing System Update 

CRITICAL MIGRATION REQUIRED
If you were using the previous version (before 2026.01.11), you MUST reset all xenotype prices!
The old system used arbitrary multipliers (0.5x-8x), but the new system uses actual silver prices based on Rimworld's gene market values.

Immediate Action Required:
1. Open Pawn Race Settings (RICS -> RICS Button -> Pawn Races)
2. Select any race
3. Click 'Reset All Prices' button in the header
4. OR click 'Reset' next to each xenotype individually
5. Repeat for all races you use

What Changed?

OLD SYSTEM (Broken)
Total Price = Race Base Price × Xenotype Multiplier
Example: Human (1000) × Sanguophage (2.2x) = 2200
Multipliers were arbitrary guesses (0.5x to 8x)

NEW SYSTEM (Correct)
Total Price = Race Base Price + Xenotype Price
Xenotype Price = Sum[(gene.marketValueFactor - 1) × Race Base Price]
Uses Rimworld's actual gene marketValueFactor

Key Benefits of New System:
- Accurate: Matches Rimworld's caravan/trade values exactly
- Transparent: Shows actual silver, not confusing multipliers
- Consistent: 1 silver = 1 unit of value (Rimworld standard)
- Mod-Compatible: Works with all gene mods using marketValueFactor
- Future-Proof: Based on Rimworld's official valuation system

New Features Added:
1. Help System
   - Click '?' Help button in Race Settings for complete documentation
   - Includes migration instructions and price calculation examples
   - Explains all settings and features

2. Bulk Reset Options
   - 'Reset All Prices' button resets all xenotypes for selected race
   - Individual 'Reset' buttons next to each xenotype
   - Tooltips show calculated price before resetting

3. Improved UI
   - 'Price (silver)' column instead of confusing 'Multiplier'
   - Clear separation: Race Base Price + Xenotype Price
   - Better input validation (0-1,000,000 silver range)

4. Updated Debug Tools
   - Debug actions now show actual silver values
   - 'Recalculate All Xenotype Prices' updates to new system
   - Gene details show marketValueFactor contributions"
            },
            // Add more versions here as they're released
        };

        public static string GetUpdateNotes(string version)
        {
            if (UpdateNotes.TryGetValue(version, out string notes))
            {
                return notes;
            }

            // Default/fallback message
            return $"RICS has been updated to version {version}.\n\n" +
                   "Please check the mod's documentation or release notes for detailed changelog.\n\n" +
                   "Thank you for using RICS!";
        }

        public static string GetMigrationNotes(string fromVersion, string toVersion)
        {
            // Special handling for migrations from older versions
            if (string.IsNullOrEmpty(fromVersion) || fromVersion == "0")
            {
                return GetUpdateNotes(toVersion) + "\n\n" +
                       "NOTE: This appears to be your first time using RICS with this save file, " +
                       "or you're migrating from a very old version. " +
                       "Please review the changes above carefully.";
            }

            return GetUpdateNotes(toVersion);
        }
    }
}
