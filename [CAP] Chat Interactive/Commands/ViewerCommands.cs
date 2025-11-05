// ViewerCommands.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Commands that viewers can use to interact with the game
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
        public override string Description => "Trigger various game events";
        public override string PermissionLevel => "everyone";
        public override int CooldownSeconds => 0; // Using global cooldown system instead

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // Check if command is enabled globally
            if (!IsEnabled())
            {
                return "The Event command is currently disabled.";
            }

            if (args.Length == 0)
            {
                return "Usage: !event <event_name> or !event list. Examples: !event resourcepod, !event heatwave, !event psychicsoothe";
            }

            string incidentType = string.Join(" ", args).Trim();
            return IncidentCommandHandler.HandleIncidentCommand(user, incidentType);
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