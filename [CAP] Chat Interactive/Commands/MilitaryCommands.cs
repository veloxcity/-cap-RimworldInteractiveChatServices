using CAP_ChatInteractive.Commands.CommandHandlers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    public class MilitaryAid : ChatCommand
    {
        public override string Name => "militaryaid";
        public override string Description => "Call for military reinforcements from friendly factions";
        public override string PermissionLevel => "everyone";
        public override int CooldownSeconds
        {
            get
            {
                var settings = GetCommandSettings();
                return settings?.CooldownSeconds ?? 0;
            }
        }

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            Logger.Debug($"MilitaryAid command executed by {user.Username} with args: [{string.Join(", ", args)}]");

            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The raid command is currently disabled.";
            }

            // Get command settings
            var settings = GetCommandSettings();

            // Parse wager amount if provided, otherwise use default from settings
            int wager = settings.DefaultMilitaryAidWager;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedWager))
            {
                // Clamp between min and max from settings
                wager = Math.Max(settings.MinMilitaryAidWager, Math.Min(settings.MaxMilitaryAidWager, parsedWager));
            }

            return MilitaryAidCommandHandler.HandleMilitaryAid(user, wager);
        }
    }

    public class Raid : ChatCommand
    {
        public override string Name => "raid";
        public override string Description => "Call a hostile raid on the colony. Usage: !raid [type] [strategy] [wager]";
        public override string PermissionLevel => "everyone";
        public override int CooldownSeconds
        {
            get
            {
                var settings = GetCommandSettings();
                return settings?.CooldownSeconds ?? 0; // Fallback to 0 if settings not available
            }
        }

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            Logger.Debug($"Raid command executed by {user.Username} with args: [{string.Join(", ", args)}]");

            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The raid command is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            // Use settings defaults
            string raidType = "standard";
            string strategy = "default";
            int wager = settingsCommand.DefaultRaidWager;

            // Use allowed types from settings, fallback to all if empty
            var validRaidTypes = settingsCommand.AllowedRaidTypes.Count > 0
                ? settingsCommand.AllowedRaidTypes
                : new List<string> { "standard", "drop", "dropcenter", "dropedge", "dropchaos", "dropgroups", "mech", "mechcluster", "manhunter", "infestation", "water", "wateredge" };

            var validStrategies = settingsCommand.AllowedRaidStrategies.Count > 0
                ? settingsCommand.AllowedRaidStrategies
                : new List<string> { "default", "immediate", "smart", "sappers", "breach", "breachsmart", "stage", "siege" };

            // Parse arguments
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg)) continue;
                string lowerArg = arg.ToLower();

                if (validRaidTypes.Contains(lowerArg))
                {
                    raidType = lowerArg;
                }
                else if (validStrategies.Contains(lowerArg))
                {
                    strategy = lowerArg;
                }
                else if (int.TryParse(arg, out int parsedWager))
                {
                    wager = Math.Max(settingsCommand.MinRaidWager, Math.Min(settingsCommand.MaxRaidWager, parsedWager));
                }
                else
                {
                    return $"Unknown argument: {arg}. Use !raidinfo for available options.";
                }
            }

            // Check if this specific raid type is allowed
            if (!validRaidTypes.Contains(raidType))
            {
                return $"Raid type '{raidType}' is not allowed. Available types: {string.Join(", ", validRaidTypes)}";
            }

            // Check if this strategy is allowed
            if (!validStrategies.Contains(strategy))
            {
                return $"Strategy '{strategy}' is not allowed. Available strategies: {string.Join(", ", validStrategies)}";
            }

            return RaidCommandHandler.HandleRaidCommand(user, raidType, strategy, wager);
        }
    }

    public class RaidInfo : ChatCommand
    {
        public override string Name => "raidinfo";
        public override string Description => "Show information about available raid types and strategies";
        public override string PermissionLevel => "everyone";
        public override int CooldownSeconds
        {
            get
            {
                var settings = GetCommandSettings();
                return settings?.CooldownSeconds ?? 0;
            }
        }
        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            if (!IsEnabled())
            {
                return "The RaidInfo is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            try
            {
                var info = new System.Text.StringBuilder();
                info.AppendLine("=== AVAILABLE RAID TYPES ===");

                info.AppendLine("!raid standard [strategy] [wager] - Edge walk-in raid");
                info.AppendLine("  Examples: !raid standard smart 5000, !raid standard siege 6000");
                info.AppendLine("!raid drop [strategy] [wager] - Random drop (varies in danger)");
                info.AppendLine("!raid dropcenter [strategy] [wager] - Center drop (-15% points)");
                info.AppendLine("!raid dropedge [strategy] [wager] - Edge drop");
                info.AppendLine("!raid dropchaos [strategy] [wager] - Random chaotic drop");
                info.AppendLine("!raid dropgroups [strategy] [wager] - Edge drop groups (+15% points)");
                info.AppendLine("!raid mech [wager] - Mechanoid raid");

                if (RaidCommandHandler.HasRoyaltyDLC)
                {
                    info.AppendLine("!raid mechcluster [wager] - Mech Cluster (Royalty DLC)");
                }

                info.AppendLine("!raid manhunter [wager] - Manhunter animal pack");
                info.AppendLine("!raid infestation [wager] - Insect infestation");

                if (RaidCommandHandler.HasBiotechDLC)
                {
                    info.AppendLine("!raid water [wager] - Water edge raid (Biotech DLC)");
                }

                info.AppendLine("\n=== STRATEGIES ===");
                info.AppendLine("immediate - Direct assault");
                info.AppendLine("smart - Avoids turrets and traps");
                info.AppendLine("sappers - Uses explosives to breach walls");
                info.AppendLine("breach - Focuses on breaking through defenses");
                info.AppendLine("breachsmart - Smart breaching tactics");
                info.AppendLine("stage - Waits then attacks");
                info.AppendLine("siege - Builds mortars and defenses");

                info.AppendLine("\nDefault wager: 5000, Min: 1000, Max: 20000");
                info.AppendLine("Higher wager = stronger raid, more negative karma");

                // Send as green letter for in-game reference
                MessageHandler.SendGreenLetter("Raid Commands", info.ToString());

                return "Raid commands sent to in-game letter. Use !raid [type] [strategy] [wager]";
            }
            catch (Exception ex)
            {
                Logger.Error($"Raid info error: {ex}");
                return "Error getting raid info.";
            }
        }
    }
}