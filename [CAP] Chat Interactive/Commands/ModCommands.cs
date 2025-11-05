// ModCommands.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Defines moderator commands for giving coins, setting karma, and toggling coin earning.
using System;

namespace CAP_ChatInteractive.Commands.ModCommands
{
    public class GiveCoins : ChatCommand
    {
        public override string Name => "givecoins";
        public override string Description => "Give coins to a viewer";
        public override string PermissionLevel => "moderator";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {

            // TODO: Implement coin giving logic
            return "Coin giving functionality coming soon!";
        }
    }

    public class SetKarma : ChatCommand
    {
        public override string Name => "setkarma";
        public override string Description => "Set karma for a viewer";
        public override string PermissionLevel => "moderator";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // TODO: Implement karma setting logic
            return "Karma setting functionality coming soon!";
        }
    }

    public class ToggleCoins : ChatCommand
    {
        public override string Name => "togglecoins";
        public override string Description => "Toggle coin earning on/off";
        public override string PermissionLevel => "moderator";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // TODO: Implement coin toggling logic
            return "Coin toggling functionality coming soon!";
        }
    }
}