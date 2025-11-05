// IAddonMenu.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Interface for addon menus to provide their menu options
using System.Collections.Generic;
using Verse;

namespace CAP_ChatInteractive.Interfaces
{
    public interface IAddonMenu
    {
        List<FloatMenuOption> MenuOptions();
    }
}