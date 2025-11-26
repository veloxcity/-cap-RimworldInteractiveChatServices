// GameComponent_CommandsInitializer.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Initializes chat commands when a game is loaded or started.
using Newtonsoft.Json;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace CAP_ChatInteractive
{
    public class GameComponent_CommandsInitializer : GameComponent
    {
        public bool commandsInitialized = false;

        public GameComponent_CommandsInitializer(Game game) { }

        public override void LoadedGame()
        {
            InitializeCommands();
        }

        public override void StartedNewGame()
        {
            InitializeCommands();
        }

        public override void GameComponentTick()
        {
            // Initialize on first tick to ensure all defs are loaded
            if (!commandsInitialized && Current.ProgramState == ProgramState.Playing)
            {
                InitializeCommands();
            }
        }
        public void InitializeCommands()
        {
            if (!commandsInitialized)
            {
                Logger.Debug("Initializing commands via GameComponent...");

                // Add debug logging to see defs
                var totalDefs = DefDatabase<Def>.AllDefsListForReading.Count;
                var commandDefs = DefDatabase<ChatCommandDef>.AllDefsListForReading;
                Logger.Debug($"Total defs: {totalDefs}, ChatCommandDefs: {commandDefs.Count}");

                // Initialize settings first
                CAP_InitializeCommandSettings();

                // Then register commands
                RegisterDefCommands();

                // Ensure raid settings are properly initialized
                EnsureRaidSettingsInitialized();

                commandsInitialized = true;
                Logger.Message("[CAP] Commands initialized successfully");
            }
        }

        public void ResetCommands()
        {
            commandsInitialized = false;
            InitializeCommands();
        }

        private void CAP_InitializeCommandSettings()
        {
            Logger.Message("=== CAP_InitializeCommandSettings called ===");

            // FORCE check for any missing commands and add them
            ForceAddMissingCommands();

            Logger.Message($"=== [CAP] Command settings initialized ===");
        }

        private void ForceAddMissingCommands()
        {
            try
            {
                // Load current settings
                string jsonContent = JsonFileManager.LoadFile("CommandSettings.json");
                var currentSettings = new Dictionary<string, CommandSettings>();

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    currentSettings = JsonConvert.DeserializeObject<Dictionary<string, CommandSettings>>(jsonContent) ?? new Dictionary<string, CommandSettings>();
                }

                bool settingsChanged = false;
                var commandDefs = DefDatabase<ChatCommandDef>.AllDefsListForReading;

                // Check every command def and ensure it exists in settings
                foreach (var def in commandDefs)
                {
                    if (!string.IsNullOrEmpty(def.commandText))
                    {
                        // FIX: Use lowercase consistently
                        string commandName = def.commandText.ToLowerInvariant();
                        if (!currentSettings.ContainsKey(commandName))
                        {
                            currentSettings[commandName] = new CommandSettings
                            {
                                Enabled = def.enabled,
                                CooldownSeconds = def.cooldownSeconds,
                                PermissionLevel = def.permissionLevel,
                                useCommandCooldown = def.useCommandCooldown
                            };
                            settingsChanged = true;
                            Logger.Debug($"FORCE ADDED missing command: '{commandName}'");
                        }
                    }
                }

                // Save if changes were made
                if (settingsChanged)
                {
                    string newJson = JsonConvert.SerializeObject(currentSettings, Formatting.Indented);
                    JsonFileManager.SaveFile("CommandSettings.json", newJson);
                    Logger.Message("[CAP] Added missing commands to settings");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in ForceAddMissingCommands: {ex}");
            }
        }

        private void RegisterDefCommands()
        {
            var defs = DefDatabase<ChatCommandDef>.AllDefsListForReading;
            Logger.Debug($"Registering {defs.Count} commands from Defs...");

            foreach (var commandDef in defs)
            {
                commandDef.RegisterCommand();
            }
        }

        private void EnsureRaidSettingsInitialized()
        {
            try
            {
                var raidSettings = CommandSettingsManager.GetSettings("raid"); // CORRECT

                // Initialize raid-specific lists if they're null or empty
                if (raidSettings.AllowedRaidTypes == null || raidSettings.AllowedRaidTypes.Count == 0)
                {
                    raidSettings.AllowedRaidTypes = new List<string> {
                "standard", "drop", "dropcenter", "dropedge", "dropchaos",
                "dropgroups", "mech", "mechcluster", "manhunter", "infestation",
                "water", "wateredge"
            };
                }

                if (raidSettings.AllowedRaidStrategies == null || raidSettings.AllowedRaidStrategies.Count == 0)
                {
                    raidSettings.AllowedRaidStrategies = new List<string> {
                "default", "immediate", "smart", "sappers", "breach",
                "breachsmart", "stage", "siege"
            };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error ensuring raid settings are initialized: {ex}");
            }
        }
    }
}