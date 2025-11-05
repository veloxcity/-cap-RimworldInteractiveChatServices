// ChatInterfaceBase.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Abstract base class for chat interfaces to handle message parsing and system events
using System;
using Verse;

namespace CAP_ChatInteractive
{
    public abstract class ChatInterfaceBase : GameComponent
    {
        public ChatInterfaceBase(Game game) { }
        public ChatInterfaceBase() { } // Required for GameComponent

        public abstract void ParseMessage(ChatMessageWrapper message);

        // Optional: Method for handling system events
        public virtual void OnServiceConnected(string platform) { }
        public virtual void OnServiceDisconnected(string platform) { }
    }
}