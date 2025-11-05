// ChatCommandDefOf.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Defines static references to chat command definitions for easy access throughout the mod.
using RimWorld;

namespace CAP_ChatInteractive
{
    [DefOf]
    public static class ChatCommandDefOf
    {
        //public static ChatCommandDef Help;
        //public static ChatCommandDef Points;
        //public static ChatCommandDef Buy;
        //public static ChatCommandDef Use;
        //public static ChatCommandDef Equip;
        //public static ChatCommandDef Wear;
        //public static ChatCommandDef Backpack;
        //public static ChatCommandDef Event;
        //public static ChatCommandDef Balance;

        static ChatCommandDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ChatCommandDefOf));
        }
    }
}