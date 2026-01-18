


using RimWorld;
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
}
