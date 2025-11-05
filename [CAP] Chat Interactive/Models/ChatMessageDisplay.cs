// Models/ChatMessageDisplay.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// A model representing a chat message for display purposes
using System;

namespace CAP_ChatInteractive
{
    public class ChatMessageDisplay
    {
        public string Username { get; set; }
        public string Text { get; set; }
        public string Platform { get; set; }
        public bool IsSystem { get; set; }
        public DateTime Timestamp { get; set; }
    }
}