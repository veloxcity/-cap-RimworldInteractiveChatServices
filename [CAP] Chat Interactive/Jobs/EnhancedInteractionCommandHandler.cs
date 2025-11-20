// EnhancedInteractionCommandHandler.cs
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace CAP_ChatInteractive.Commands.CommandHandlers
{
    public static class EnhancedInteractionCommandHandler
    {
        private static readonly Dictionary<InteractionDef, InteractionInfo> InteractionData =
            new Dictionary<InteractionDef, InteractionInfo>
        {
            { InteractionDefOf.Chitchat, new InteractionInfo { IsNegative = false, Cost = 10, KarmaCost = 0, JobDef = JobDefOf_CAP.CAP_SocialVisit } },
            // Add other interactions...
        };

        public static string HandleInteractionCommand(ChatMessageWrapper user, InteractionDef interaction, string[] args)
        {
            try
            {
                // Get viewer data
                var viewer = Viewers.GetViewer(user);
                if (viewer == null) return "Could not find your viewer data.";

                // Check interaction validity and cost (your existing logic)
                if (!InteractionData.TryGetValue(interaction, out var interactionInfo))
                    return "This interaction is not available.";

                if (viewer.GetCoins() < interactionInfo.Cost)
                    return $"You need {interactionInfo.Cost} coins for this interaction.";

                // Get pawns
                var assignmentManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
                var initiatorPawn = assignmentManager.GetAssignedPawn(user);
                if (initiatorPawn == null) return "You don't have an active pawn.";

                // Find target pawn
                Pawn targetPawn = FindInteractionTarget(initiatorPawn, args);
                if (targetPawn == null) return "No valid target found.";

                // Check basic pawn conditions
                if (!CanPawnsInteract(initiatorPawn, targetPawn))
                    return $"{initiatorPawn.Name} cannot interact with {targetPawn.Name} right now.";

                // Create and assign the social visit job
                Job socialJob = JobMaker.MakeJob(interactionInfo.JobDef, targetPawn);
                socialJob.interaction = interaction; // Store which interaction to use

                initiatorPawn.jobs.StartJob(socialJob, JobCondition.InterruptForced);

                // Deduct cost immediately (or wait for completion?)
                viewer.TakeCoins(interactionInfo.Cost);
                Viewers.SaveViewers();

                return $"{initiatorPawn.Name} is going to visit {targetPawn.Name} for a {interaction.label}...";
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in enhanced interaction command: {ex}");
                return "An error occurred while processing the interaction.";
            }
        }

        private static Pawn FindInteractionTarget(Pawn initiator, string[] args)
        {
            // Your existing logic from FindInteractionTarget
            if (args.Length > 0)
            {
                string targetQuery = args[0].TrimStart('@');

                var assignmentManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
                if (assignmentManager != null && assignmentManager.HasAssignedPawn(targetQuery))
                {
                    var targetPawn = assignmentManager.GetAssignedPawn(targetQuery);
                    if (targetPawn != null && targetPawn != initiator) return targetPawn;
                }

                var namedPawn = FindPawnByName(targetQuery);
                if (namedPawn != null && namedPawn != initiator) return namedPawn;

                return null;
            }

            return FindRandomColonist(initiator);
        }

        private static bool CanPawnsInteract(Pawn initiator, Pawn target)
        {
            if (initiator == null || target == null) return false;
            if (initiator.Dead || target.Dead) return false;
            if (!initiator.Spawned || !target.Spawned) return false;
            if (initiator.Downed || target.Downed) return false;
            if (initiator.InMentalState || target.InMentalState) return false;

            return true;
        }

        private static Pawn FindPawnByName(string name)
        {
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
                .FirstOrDefault(p => !p.Dead && p.Name != null &&
                    p.Name.ToString().ToLower().Contains(name.ToLower()));
        }

        private static Pawn FindRandomColonist(Pawn excludePawn)
        {
            var colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
                .Where(p => !p.Dead && p != excludePawn)
                .ToList();
            return colonists.Count > 0 ? colonists.RandomElement() : null;
        }

        private class InteractionInfo
        {
            public bool IsNegative { get; set; }
            public int Cost { get; set; }
            public int KarmaCost { get; set; }
            public JobDef JobDef { get; set; }
        }
    }
}