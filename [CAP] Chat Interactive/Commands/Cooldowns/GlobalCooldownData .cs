// GlobalCooldownData.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Data structures for tracking global cooldowns for events and commands
using CAP_ChatInteractive.Commands.Cooldowns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CAP_ChatInteractive;

namespace CAP_ChatInteractive.Commands.Cooldowns
{
    public class GlobalCooldownData : IExposable
    {
        public Dictionary<string, EventUsageRecord> EventUsage;
        public Dictionary<string, CommandUsageRecord> CommandUsage;
        public Dictionary<string, BuyUsageRecord> BuyUsage;
        public void ExposeData()
        {
            Scribe_Collections.Look(ref EventUsage, "eventUsage", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref CommandUsage, "commandUsage", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref BuyUsage, "buyUsage", LookMode.Value, LookMode.Deep);

            // Backward compatibility: Initialize any missing dictionaries after loading
            if (EventUsage == null)
            {
                EventUsage = new Dictionary<string, EventUsageRecord>();
                Logger.Debug("EventUsage initialized in GlobalCooldownData.ExposeData");
            }
            if (CommandUsage == null)
            {
                CommandUsage = new Dictionary<string, CommandUsageRecord>();
                Logger.Debug("CommandUsage initialized in GlobalCooldownData.ExposeData");
            }
            if (BuyUsage == null)
            {
                BuyUsage = new Dictionary<string, BuyUsageRecord>();
                Logger.Debug("BuyUsage initialized in GlobalCooldownData.ExposeData");
            }
        }
    
        public GlobalCooldownData()
        {
            // Ensure all dictionaries are initialized
            EventUsage = new Dictionary<string, EventUsageRecord>();
            CommandUsage = new Dictionary<string, CommandUsageRecord>();
            BuyUsage = new Dictionary<string, BuyUsageRecord>();
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


}

