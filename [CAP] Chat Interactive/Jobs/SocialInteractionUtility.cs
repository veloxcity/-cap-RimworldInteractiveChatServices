// SocialInteractionUtility.cs
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;

namespace CAP_ChatInteractive.Utilities
{
    public static class SocialInteractionUtility
    {
        public static bool TryStartSocialVisit(Pawn initiator, Pawn target, InteractionDef interaction, out string failReason)
        {
            failReason = null;

            if (!CanPawnsInteract(initiator, target))
            {
                failReason = $"{initiator.Name} cannot interact with {target.Name}";
                return false;
            }

            if (!CanTargetInteractNow(target))
            {
                failReason = GetUnavailableReason(target);
                return false;
            }

            Job socialJob = JobMaker.MakeJob(JobDefOf_CAP.CAP_SocialVisit, target);
            socialJob.interaction = interaction;
            initiator.jobs.StartJob(socialJob, JobCondition.InterruptForced);

            return true;
        }

        private static bool CanPawnsInteract(Pawn initiator, Pawn target)
        {
            return initiator != null && target != null &&
                   !initiator.Dead && !target.Dead &&
                   initiator.Spawned && target.Spawned &&
                   !initiator.Downed && !target.Downed;
        }

        private static bool CanTargetInteractNow(Pawn target)
        {
            if (target.jobs.curDriver != null && target.jobs.curDriver.asleep) return false;
            if (target.CurJob != null && target.CurJob.def == JobDefOf.LayDown) return false;
            if (target.InMentalState) return false;
            if (target.Drafted) return false; // Maybe allow interactions with drafted pawns?

            return true;
        }

        private static string GetUnavailableReason(Pawn target)
        {
            if (target.jobs.curDriver != null && target.jobs.curDriver.asleep)
                return $"{target.Name} is sleeping";
            if (target.CurJob != null && target.CurJob.def == JobDefOf.LayDown)
                return $"{target.Name} is resting";
            if (target.InMentalState)
                return $"{target.Name} is in a mental break";
            if (target.Drafted)
                return $"{target.Name} is drafted";

            return $"{target.Name} is unavailable";
        }
    }
}