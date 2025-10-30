using CAP_ChatInteractive.Commands.CommandHandlers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    public class Pawn : ChatCommand
    {
        public override string Name => "pawn";
        public override string Description => "Purchase a pawn to join the colony";
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
                return "The Pawn command is currently disabled.";
            }

            Logger.Debug($"Pawn command executed by {user.Username} with args: [{string.Join(", ", args)}]");

            // Handle different argument patterns
            if (args.Length == 0)
            {
                return ShowPawnHelp();
            }

            string firstArg = args[0].ToLower();

            // Handle list commands
            if (firstArg == "list" || firstArg == "races" || firstArg == "xenotypes")
            {
                return HandleListCommand(user, args);
            }

            // Handle purchase
            return HandlePawnPurchase(user, args);
        }

        private string ShowPawnHelp()
        {
            return "Usage: !pawn <race> [xenotype] [gender] [age] OR !pawn list <races|xenotypes> OR !pawn mypawn\n" +
                   "Examples:\n" +
                   "!pawn human\n" +
                   "!pawn human hussar\n" +
                   "!pawn human baseliner female\n" +
                   "!pawn human genie male 25\n" +
                   "!pawn list races\n" +
                   "!pawn mypawn";
        }

        private string HandleListCommand(ChatMessageWrapper user, string[] args)
        {
            if (args.Length > 1)
            {
                string listType = args[1].ToLower();
                switch (listType)
                {
                    case "races":
                        return ListAvailableRaces();
                    case "xenotypes":
                        return ListAvailableXenotypes();
                    default:
                        return $"Unknown list type: {listType}. Use: races, xenotypes";
                }
            }

            return "List types: races, xenotypes. Example: !pawn list races";
        }

        private string ListAvailableRaces()
        {
            var availableRaces = GetEnabledRaces();
            if (availableRaces.Count == 0)
            {
                return "No races available for purchase.";
            }

            var raceList = availableRaces.Select(r => r.LabelCap.RawText);
            return $"Available races: {string.Join(", ", raceList)}";
        }

        private string ListAvailableXenotypes()
        {
            if (!ModsConfig.BiotechActive)
            {
                return "Biotech DLC not active - only baseliners available.";
            }

            var xenotypes = DefDatabase<XenotypeDef>.AllDefs.Where(x => x != XenotypeDefOf.Baseliner);
            var xenotypeList = xenotypes.Select(x => x.defName);
            return $"Available xenotypes: {string.Join(", ", xenotypeList)}";
        }

        private string HandlePawnPurchase(ChatMessageWrapper user, string[] args)
        {
            // Handle "mypawn" subcommand
            if (args.Length > 0 && args[0].ToLower() == "mypawn")
            {
                return HandleMyPawnCommand(user);
            }

            // Parse arguments with better logic
            string raceName = "";
            string xenotypeName = "Baseliner";
            string genderName = "Random";
            string ageString = "Random";

            if (args.Length > 0)
            {
                raceName = args[0];

                // Process remaining arguments
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i].ToLower();

                    // Check if argument is a gender
                    if (arg == "male" || arg == "female")
                    {
                        genderName = arg;
                    }
                    // Check if argument is an age (numeric)
                    else if (int.TryParse(arg, out int age))
                    {
                        ageString = age.ToString();
                    }
                    // Check if argument is "random" for age
                    else if (arg == "random")
                    {
                        ageString = "Random";
                    }
                    // Otherwise, assume it's a xenotype
                    else
                    {
                        xenotypeName = args[i]; // Keep original casing for xenotype lookup
                    }
                }
            }
            else
            {
                return ShowPawnHelp();
            }

            Logger.Debug($"Parsed - Race: {raceName}, Xenotype: {xenotypeName}, Gender: {genderName}, Age: {ageString}");

            // Validate that we have at least a race name
            if (string.IsNullOrEmpty(raceName))
            {
                return "You must specify a race. Usage: !pawn <race> [xenotype] [gender] [age]";
            }

            // Call the command handler
            return BuyPawnCommandHandler.HandleBuyPawnCommand(user, raceName, xenotypeName, genderName, ageString);
        }

        private string HandleMyPawnCommand(ChatMessageWrapper user)
        {
            var assignmentManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
            var pawn = assignmentManager.GetAssignedPawn(user.Username);

            if (pawn != null && !pawn.Dead)
            {
                string status = pawn.Spawned ? "alive and in colony" : "alive but not in colony";
                string health = pawn.health.summaryHealth.SummaryHealthPercent.ToStringPercent();

                return $"Your pawn {pawn.Name} is {status}. Health: {health}, Age: {pawn.ageTracker.AgeBiologicalYears}";
            }
            else
            {
                // Clear the assignment if pawn is dead
                assignmentManager.UnassignPawn(user.Username);
                return "You don't have an active pawn in the colony. Use !pawn to purchase one!";
            }
        }

        private List<ThingDef> GetEnabledRaces()
        {
            // Get races from your race settings that are enabled
            var pawnSettings = Find.WindowStack.WindowOfType<Dialog_PawnSettings>();
            if (pawnSettings != null)
            {
                // Access the race settings from your dialog
                // This will need to be implemented based on your data structure
            }

            // Fallback: all humanlike races
            return DefDatabase<ThingDef>.AllDefs
                .Where(d => d.race?.Humanlike ?? false)
                .ToList();
        }
    }

    public class MyPawn : ChatCommand
    {
        public override string Name => "mypawn";
        public override string Description => "Show information about your pawn and manage it";
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
                return "The MyPawn command is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            if (args.Length == 0)
            {
                return ShowMyPawnHelp();
            }

            string subCommand = args[0].ToLower();
            string[] subArgs = args.Length > 1 ? args.Skip(1).ToArray() : Array.Empty<string>();

            return MyPawnCommandHandler.HandleMyPawnCommand(user, subCommand, subArgs);
        }

        private string ShowMyPawnHelp()
        {
            return "!mypawn <type>: body, gear, kills, needs, relations, skills, stats, story, traits, work\n" +
                "Ex: !mypawn health, !mypawn skills, !mypawn stats shooting melee";
        }
    }

    public class JoinQueue : ChatCommand
    {
        public override string Name => "join";
        public override string Description => "Join the pawn queue";
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
                return "The JoinQueue is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            // TODO: Implement pawn queue logic
            return "Pawn queue functionality coming soon!";
        }
    }
}