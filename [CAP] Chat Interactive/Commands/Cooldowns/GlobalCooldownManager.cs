// GlobalCooldownManager.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Manages global cooldowns for chat events and commands in RimWorld.
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CAP_ChatInteractive.Commands.Cooldowns
{
    public class GlobalCooldownManager : GameComponent
    {
        public GlobalCooldownData data = new GlobalCooldownData();
        private int lastCleanupDay = 0;

        // REQUIRED: GameComponent constructor
        public GlobalCooldownManager(Game game) { }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref data, "globalCooldownData");
            Scribe_Values.Look(ref lastCleanupDay, "lastCleanupDay");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                CleanupOldRecords();
        }

        public bool CanUseEvent(string eventType, CAPGlobalChatSettings settings)
        {
            // 0 = infinite
            if (settings.MaxGoodEvents == 0 && eventType == "good") return true;
            if (settings.MaxBadEvents == 0 && eventType == "bad") return true;
            if (settings.MaxNeutralEvents == 0 && eventType == "neutral") return true;

            var record = GetOrCreateEventRecord(eventType);
            CleanupOldEvents(record, settings.EventCooldownDays);

            int maxUses = eventType switch
            {
                "good" => settings.MaxGoodEvents,
                "bad" => settings.MaxBadEvents,
                "neutral" => settings.MaxNeutralEvents,
                "doom" => 1, // Special case
                _ => 10
            };

            return record.CurrentPeriodUses < maxUses;
        }

        public bool CanUseCommand(string commandName, CommandSettings settings, CAPGlobalChatSettings globalSettings)
        {
            // Check per-command game days cooldown first (applies in both modes)
            if (settings.UseEventCooldown && settings.MaxUsesPerCooldownPeriod > 0)
            {
                var cmdRecord = GetOrCreateCommandRecord(commandName);
                CleanupOldCommandUses(cmdRecord, globalSettings.EventCooldownDays);

                if (cmdRecord.CurrentPeriodUses >= settings.MaxUsesPerCooldownPeriod)
                    return false;
            }

            // If per-command limit is 0 (unlimited), we break out here
            if (settings.UseEventCooldown && settings.MaxUsesPerCooldownPeriod == 0)
            {
                return true; // Unlimited uses for this command
            }

            // Main event cooldown logic
            if (globalSettings.EventCooldownsEnabled)
            {
                if (settings.UseGameDaysCooldown)
                {
                    // Game-day based individual command cooldowns
                    // We already handled MaxUsesPerCooldownPeriod above, so just return true
                    return true;
                }
                else
                {
                    // Traditional event cooldown system
                    // 1. Check total event limit
                    if (!CanUseGlobalEvents(globalSettings))
                        return false;

                    // 2. Check type-specific limits if enabled
                    if (globalSettings.KarmaTypeLimitsEnabled)
                    {
                        string eventType = GetEventTypeForCommand(commandName);
                        if (!CanUseEvent(eventType, globalSettings))
                            return false;
                    }

                    return true;
                }
            }
            else
            {
                // Event cooldowns disabled - only use per-command game day limits
                // (which we already checked above)
                return true;
            }
        }

        // NEW: Check global event count limit
        private bool CanUseGlobalEvents(CAPGlobalChatSettings settings)
        {
            if (settings.EventsperCooldown == 0) return true; // Unlimited

            int totalEvents = data.EventUsage.Values.Sum(record => record.CurrentPeriodUses);
            return totalEvents < settings.EventsperCooldown;
        }

        public void RecordEventUse(string eventType)
        {
            var record = GetOrCreateEventRecord(eventType);
            record.UsageDays.Add(CurrentGameDay);
        }

        public void RecordCommandUse(string commandName)
        {
            var record = GetOrCreateCommandRecord(commandName);
            record.UsageDays.Add(CurrentGameDay);
        }

        private void CleanupOldRecords()
        {
            int currentDay = CurrentGameDay;
            if (currentDay == lastCleanupDay) return;

            var globalSettings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings as CAPGlobalChatSettings;

            foreach (var record in data.EventUsage.Values)
                CleanupOldEvents(record, globalSettings.EventCooldownDays);

            foreach (var record in data.CommandUsage.Values)
                CleanupOldCommandUses(record, globalSettings.EventCooldownDays);

            lastCleanupDay = currentDay;
        }

        private void CleanupOldEvents(EventUsageRecord record, int cooldownDays)
        {
            if (cooldownDays == 0) return; // Never expire
            record.UsageDays.RemoveAll(day => (CurrentGameDay - day) > cooldownDays);
        }

        private void CleanupOldCommandUses(CommandUsageRecord record, int cooldownDays)
        {
            if (cooldownDays == 0) return;
            record.UsageDays.RemoveAll(day => (CurrentGameDay - day) > cooldownDays);
        }

        private int CurrentGameDay => GenDate.DaysPassed;

        // Helper methods
        private EventUsageRecord GetOrCreateEventRecord(string eventType)
        {
            if (!data.EventUsage.ContainsKey(eventType))
                data.EventUsage[eventType] = new EventUsageRecord { EventType = eventType };
            return data.EventUsage[eventType];
        }

        private CommandUsageRecord GetOrCreateCommandRecord(string commandName)
        {
            if (!data.CommandUsage.ContainsKey(commandName))
                data.CommandUsage[commandName] = new CommandUsageRecord { CommandName = commandName };
            return data.CommandUsage[commandName];
        }

        public string GetEventTypeForCommand(string commandName)
        {
            // Map commands to event types
            return commandName.ToLower() switch
            {
                "raid" => "bad",
                "militaryaid" => "good",
                "weather" => "neutral",
                _ => "neutral"
            };
        }
        public bool CanPurchaseItem(CAPGlobalChatSettings settings)
        {
            if (!settings.EventCooldownsEnabled) return true;

            // Count all purchases across all item types
            int totalPurchases = data.BuyUsage.Values.Sum(record => record.CurrentPeriodPurchases);

            return totalPurchases < settings.MaxItemPurchases;
        }

        public void RecordItemPurchase(string itemType = "general")
        {
            var record = GetOrCreateBuyRecord(itemType);
            record.PurchaseDays.Add(GenDate.DaysPassed);

            // Also cleanup old records
            CleanupOldPurchases(record, CAPChatInteractiveMod.Instance.Settings.GlobalSettings.EventCooldownDays);
        }

        private BuyUsageRecord GetOrCreateBuyRecord(string itemType)
        {
            if (!data.BuyUsage.ContainsKey(itemType))
                data.BuyUsage[itemType] = new BuyUsageRecord { ItemType = itemType };
            return data.BuyUsage[itemType];
        }

        private void CleanupOldPurchases(BuyUsageRecord record, int cooldownDays)
        {
            if (cooldownDays == 0) return;
            record.PurchaseDays.RemoveAll(day => (GenDate.DaysPassed - day) > cooldownDays);
        }
    }

    // Supporting data classes
    public class GlobalCooldownData : IExposable
    {
        public Dictionary<string, EventUsageRecord> EventUsage = new Dictionary<string, EventUsageRecord>();
        public Dictionary<string, CommandUsageRecord> CommandUsage = new Dictionary<string, CommandUsageRecord>();
        public Dictionary<string, BuyUsageRecord> BuyUsage = new Dictionary<string, BuyUsageRecord>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref EventUsage, "eventUsage", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref CommandUsage, "commandUsage", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref BuyUsage, "buyUsage", LookMode.Value, LookMode.Deep);
        }
    }

    public class BuyUsageRecord : IExposable
    {
        public string ItemType; // "weapon", "apparel", "item", "surgery", etc.
        public List<int> PurchaseDays = new List<int>(); // Game days when items were purchased
        public int CurrentPeriodPurchases => PurchaseDays.Count;

        public void ExposeData()
        {
            Scribe_Values.Look(ref ItemType, "itemType");
            Scribe_Collections.Look(ref PurchaseDays, "purchaseDays", LookMode.Value);
        }
    }

    public class EventUsageRecord : IExposable
    {
        public string EventType; // "good", "bad", "neutral", "doom"
        public List<int> UsageDays = new List<int>(); // Game days when events were used
        public int CurrentPeriodUses => UsageDays.Count;

        public void ExposeData()
        {
            Scribe_Values.Look(ref EventType, "eventType");
            Scribe_Collections.Look(ref UsageDays, "usageDays", LookMode.Value);
        }
    }

    public class CommandUsageRecord : IExposable
    {
        public string CommandName;
        public List<int> UsageDays = new List<int>();
        public int CurrentPeriodUses => UsageDays.Count;

        public void ExposeData()
        {
            Scribe_Values.Look(ref CommandName, "commandName");
            Scribe_Collections.Look(ref UsageDays, "usageDays", LookMode.Value);
        }
    }
  }