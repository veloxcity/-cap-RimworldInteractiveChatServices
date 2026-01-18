// File: GenderSwapRecipeWorker.cs
// Copyright (c) Captolamia
// This file is part of CAP Chat Interactive.
// 
// CAP Chat Interactive is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CAP Chat Interactive is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with CAP Chat Interactive. If not, see <https://www.gnu.org/licenses/>.
//
// Gender swap surgery worker for RimWorld mod CAP Chat Interactive
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CAP_ChatInteractive.Commands.CommandHandlers
{
    public class GenderSwapRecipeWorker : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, System.Collections.Generic.List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            if (pawn == null || pawn.Dead) return;

            // Perform the gender swap
            Gender originalGender = pawn.gender;
            pawn.gender = originalGender == Gender.Male ? Gender.Female : Gender.Male;

            // Update body type to match new gender (vanilla body types are gendered)
            if (pawn.gender == Gender.Male)
            {
                pawn.story.bodyType = BodyTypeDefOf.Male;
            }
            else if (pawn.gender == Gender.Female)
            {
                pawn.story.bodyType = BodyTypeDefOf.Female;
            }
            // Note: For other body types like Thin/Hulk/Fat, you might need custom mapping or randomization

            // Remove beard if switching to female
            if (pawn.gender == Gender.Female && pawn.style.beardDef != BeardDefOf.NoBeard)
            {
                pawn.style.beardDef = BeardDefOf.NoBeard;
            }

            // Optionally, regenerate apparel to fit new body type
            if (pawn.apparel != null)
            {
                pawn.apparel.UnlockAll(); // Or regenerate if needed
            }

            // Force redraw and update
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);

            // Add a tale or log entry
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn); // Or custom tale if success

            Logger.Debug($"Gender swapped {pawn.Name} from {originalGender} to {pawn.gender}");
        }
    }

    public class FatBodyRecipeWorker : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            if (pawn == null || pawn.Dead) return;

            // Set body type
            pawn.story.bodyType = BodyTypeDefOf.Fat;

            // Force graphics update
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);

            // Record tale
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

            Logger.Message($"Body type changed for {pawn.Name} to Fat");
        }
    }

    public class ThinBodyRecipeWorker : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            if (pawn == null || pawn.Dead) return;

            // Set body type
            pawn.story.bodyType = BodyTypeDefOf.Thin;

            // Force graphics update
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);

            // Record tale
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            Logger.Message($"Body type changed for {pawn.Name} to Thin");
        }
    }

    public class HulkBodyRecipeWorker : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            if (pawn == null || pawn.Dead) return;

            // Set body type
            pawn.story.bodyType = BodyTypeDefOf.Hulk;

            // Force graphics update
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);

            // Record tale
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            Logger.Message($"Body type changed for {pawn.Name} to Hulk");
        }
    }

    public class MasculineBodyRecipeWorker : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            if (pawn == null || pawn.Dead) return;

            // Set body type
            pawn.story.bodyType = BodyTypeDefOf.Male;

            // Force graphics update
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);

            // Record tale
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            Logger.Message($"Body type changed for {pawn.Name} to Masculine");
        }
    }

    public class FeminineBodyRecipeWorker : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            if (pawn == null || pawn.Dead) return;
            
            // Set body type
            pawn.story.bodyType = BodyTypeDefOf.Female;
            
            // Force graphics update
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);
            
            // Record tale
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            Logger.Message($"Body type changed for {pawn.Name} to Feminine");
        }
    }
}