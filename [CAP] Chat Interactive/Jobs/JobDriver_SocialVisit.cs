// JobDriver_SocialVisit.cs
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CAP_ChatInteractive
{
    public class JobDriver_SocialVisit : JobDriver
    {
        private const int WaitTimeoutTicks = 300; // 5 seconds real-time
        private const int CheckInterval = 60; // Check every second

        private Pawn TargetPawn => job.targetA.Thing as Pawn;
        private int waitTicks;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Fail conditions
            this.AddFailCondition(() => TargetPawn == null || TargetPawn.Dead || TargetPawn.Destroyed);
            this.AddFailCondition(() => pawn == null || pawn.Dead || pawn.Destroyed);

            // Stage 1: Go to target pawn
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOn(() => !CanInteractWithTarget(TargetPawn));

            // Stage 2: Wait for target to be available
            Toil waitToil = new Toil();
            waitToil.initAction = () => waitTicks = 0;
            waitToil.tickAction = () =>
            {
                waitTicks++;
                if (waitTicks % CheckInterval == 0)
                {
                    if (CanInteractNow(TargetPawn))
                    {
                        ReadyForNextToil();
                    }
                    else if (waitTicks >= WaitTimeoutTicks)
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                }
            };
            waitToil.defaultCompleteMode = ToilCompleteMode.Never;
            waitToil.WithProgressBar(TargetIndex.A, () => waitTicks / (float)WaitTimeoutTicks);
            yield return waitToil;

            // Stage 3: Execute interaction
            yield return new Toil
            {
                initAction = () =>
                {
                    if (CanInteractNow(TargetPawn))
                    {
                        pawn.interactions.TryInteractWith(TargetPawn, InteractionDefOf.Chitchat);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private bool CanInteractWithTarget(Pawn target)
        {
            return target != null &&
                   !target.Dead &&
                   !target.Destroyed &&
                   target.Spawned &&
                   !target.Downed;
        }

        private bool CanInteractNow(Pawn target)
        {
            if (!CanInteractWithTarget(target)) return false;

            // Check if target is sleeping, in mental break, etc.
            if (target.CurJob != null && target.CurJob.def == JobDefOf.LayDown) return false;
            if (target.InMentalState) return false;
            if (target.jobs.curDriver != null && target.jobs.curDriver.asleep) return false;

            return true;
        }
    }
}