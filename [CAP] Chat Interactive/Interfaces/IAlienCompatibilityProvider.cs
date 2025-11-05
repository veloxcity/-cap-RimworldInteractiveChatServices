// IAlienCompatibilityProvider.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Interface for compatibility providers
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace _CAP__Chat_Interactive.Interfaces
{
    public interface ICompatibilityProvider
    {
        string ModId { get;}
    }
    public interface IAlienCompatibilityProvider : ICompatibilityProvider
    {
        bool IsTraitForced(Pawn pawn, string defName, int degree);
        bool IsTraitDisallowed(Pawn pawn, string defName, int degree);
        bool IsTraitAllowed(Pawn pawn, TraitDef traitDef, int degree = -10);
        List<string> GetAllowedXenotypes(ThingDef raceDef);
        bool IsXenotypeAllowed(ThingDef raceDef, XenotypeDef xenotype);
    }
}

