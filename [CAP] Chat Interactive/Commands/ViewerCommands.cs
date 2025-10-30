using CAP_ChatInteractive.Commands.CommandHandlers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    public class CheckBalance : ChatCommand
    {
        public override string Name => "bal";
        public override string Description => "Check your coin & Karma balance";
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
            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The CheckBalance command is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            var viewer = Viewers.GetViewer(user.Username);
            if (viewer != null)
            {
                var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
                var currencySymbol = settings.CurrencyName?.Trim() ?? "¢";

                // Use the shared karma emoji method
                string karmaEmoji = GetKarmaEmoji(viewer.Karma);

                return $"You have {viewer.Coins}{currencySymbol} and {viewer.Karma} karma! {karmaEmoji}";
            }
            return "Could not find your viewer data.";
        }
    }

    public class WhatIsKarma : ChatCommand
    {
        public override string Name => "whatiskarma";
        public override string Description => "Explain what karma is";
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
            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The WhatIsKarma command is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            return "Karma affects your coin rewards! Higher karma = more coins per message. Be active and positive to increase your karma!";
        }
    }

    // Event command
    public class Event : ChatCommand
    {
        public override string Name => "event";
        public override string Description => "Trigger a game event (weather, raid, etc.)";
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
            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The Event command is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            if (args.Length == 0)
            {
                return "Usage: !event <type> [options]. Types: weather, raid, animal, trader. Example: !event weather rain";
            }

            string eventType = args[0].ToLower();

            switch (eventType)
            {
                case "weather":
                    return HandleWeatherEvent(user, args.Skip(1).ToArray());
                //case "raid":
                //    return HandleRaidEvent(user, args.Skip(1).ToArray());
                case "animal":
                    return HandleAnimalEvent(user, args.Skip(1).ToArray());
                case "trader":
                    return HandleTraderEvent(user, args.Skip(1).ToArray());
                default:
                    return $"Unknown event type: {eventType}. Available: weather, raid, animal, trader";
            }
        }

        private string HandleWeatherEvent(ChatMessageWrapper user, string[] args)
        {
            if (args.Length == 0)
            {
                return "Usage: !event weather <type>. Types: rain, snow, fog, thunderstorm, clear, list, etc.";
            }

            string weatherType = args[0].ToLower();

            // Handle list for event command
            if (weatherType == "list" || weatherType.StartsWith("list"))
            {
                // For list commands, let WeatherCommandHandler handle the response directly
                return WeatherCommandHandler.HandleWeatherCommand(user, weatherType);
            }

            return WeatherCommandHandler.HandleWeatherCommand(user, weatherType);
        }

        // Placeholder methods for other event types
        // private string HandleRaidEvent(ChatMessageWrapper user, string[] args)
        // {
        //    return "Raid events coming soon!";
        // }

        private string HandleAnimalEvent(ChatMessageWrapper user, string[] args)
        {
            return "Animal events coming soon!";
        }

        private string HandleTraderEvent(ChatMessageWrapper user, string[] args)
        {
            return "Trader events coming soon!";
        }
    }

    public class Weather : ChatCommand
    {
        public override string Name => "weather";
        public override string Description => "Change the weather";
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
            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The Weather command is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            if (args.Length == 0)
            {
                return "Usage: !weather <type>. Types: rain, snow, fog, thunderstorm, clear, etc.";
            }

            string weatherType = args[0].ToLower();
            return WeatherCommandHandler.HandleWeatherCommand(user, weatherType);
        }
    }

    public class ModInfo : ChatCommand
    {
        public override string Name => "modinfo";
        public override string Description => "Show information about this mod";
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
            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The ModInfo is currently disabled.";
            }

            // Get command settings
            // var settingsCommand = GetCommandSettings();
            return "[CAP] Chat Interactive v1.0 - Twitch & YouTube integration for RimWorld!";
        }
    }

    public class Instructions : ChatCommand
    {
        public override string Name => "help";
        public override string Description => "Show how to use the mod";
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
            // Check if command is enabled
            if (!IsEnabled())
            {
                return "The Instructions is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            return "Available commands: !bal (check coins), !items (see store), !whatiskarma (learn about karma). More commands coming soon!";
        }
    }

    public class AvailableCommands : ChatCommand
    {
        public override string Name => "commands";
        public override string Description => "List all available commands";
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
                return "The Instructions is currently disabled.";
            }

            // Get command settings
            var settingsCommand = GetCommandSettings();

            var availableCommands = ChatCommandProcessor.GetAvailableCommands(user);
            var commandList = string.Join(", ", availableCommands.Select(cmd => $"!{cmd.Name}"));
            return $"Available commands: {commandList}";
        }
    }
}