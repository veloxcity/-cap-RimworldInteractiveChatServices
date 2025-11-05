// InventoryCommands.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Commands for purchasing and using items from the in-game store.
using CAP_ChatInteractive.Commands.CommandHandlers;
using CAP_ChatInteractive.Store;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    public class Buy : ChatCommand
    {
        public override string Name => "buy";
        public override string Description => "Purchase an item from the store";
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
                return "The Buy command is currently disabled.";
            }

            if (args.Length == 0)
            {
                return "Usage: !buy <item> [quality] [material] [quantity] OR !buy pawn <race> <xenotype> <gender> <age>";
            }

            // Check if this is a pawn purchase
            if (args[0].ToLower() == "pawn")
            {
                var pawnArgs = args.Skip(1).ToArray();
                var pawnCommand = new Pawn();
                return pawnCommand.Execute(user, pawnArgs);
            }

            return BuyItemCommandHandler.HandleBuyItem(user, args, false, false);
        }
    }

    public class Use : ChatCommand
    {
        public override string Name => "use";
        public override string Description => "Use an item immediately";
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
                return "The Use command is currently disabled.";
            }

            return BuyItemCommandHandler.HandleUseItem(user, args);
        }
    }

    public class Equip : ChatCommand
    {
        public override string Name => "equip";
        public override string Description => "Equip an item to your pawn";
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
                return "The Equip command is currently disabled.";
            }

            return BuyItemCommandHandler.HandleBuyItem(user, args, true, false);
        }
    }

    public class Wear : ChatCommand
    {
        public override string Name => "wear";
        public override string Description => "Wear apparel on your pawn";
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
                return "The Wear command is currently disabled.";
            }

            return BuyItemCommandHandler.HandleBuyItem(user, args, false, true);
        }
    }

    public class Backpack : ChatCommand
    {
        public override string Name => "backpack";
        public override string Description => "Add item to your pawn's inventory";
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
                return "The Backpack command is currently disabled.";
            }

            return BuyItemCommandHandler.HandleBuyItem(user, args, false, false, true);
        }
    }

    public class PurchaseList : ChatCommand
    {
        public override string Name => "items";
        public override string Description => "List available items for purchase";
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
                return "The items command is currently disabled.";
            }

            // Show available categories or a simple message
            var enabledItems = StoreInventory.GetEnabledItems().Take(5); // Show first 5 as example
            var itemList = string.Join(", ", enabledItems.Select(item =>
            {
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(item.DefName);
                return thingDef?.LabelCap ?? item.DefName;
            }));

            return $"Available items (sample): {itemList}. Use !buy <itemname> to purchase.";
        }
    }
}