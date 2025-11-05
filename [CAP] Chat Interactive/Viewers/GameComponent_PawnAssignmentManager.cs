// GameComponent_PawnAssignmentManager.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Manages the assignment of pawns to chat viewers, including queueing and pending offers.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using CAP_ChatInteractive.Commands;

namespace CAP_ChatInteractive
{
    public class GameComponent_PawnAssignmentManager : GameComponent
    {
        private Dictionary<string, string> viewerPawnAssignments; // Username -> ThingID
        private List<string> pawnQueue; // Usernames in queue order
        private Dictionary<string, float> queueJoinTimes; // Username -> join time (ticks)
        private Dictionary<string, PendingPawnOffer> pendingOffers; // Username -> offer data
        private List<string> expiredOffers; // Offers that timed out

        public GameComponent_PawnAssignmentManager(Game game)
        {
            viewerPawnAssignments = new Dictionary<string, string>();
            pawnQueue = new List<string>();
            queueJoinTimes = new Dictionary<string, float>();
            pendingOffers = new Dictionary<string, PendingPawnOffer>();
            expiredOffers = new List<string>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref viewerPawnAssignments, "viewerPawnAssignments", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref pawnQueue, "pawnQueue", LookMode.Value);
            Scribe_Collections.Look(ref queueJoinTimes, "queueJoinTimes", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref pendingOffers, "pendingOffers", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref expiredOffers, "expiredOffers", LookMode.Value);

            // Initialize if null after loading
            if (viewerPawnAssignments == null)
                viewerPawnAssignments = new Dictionary<string, string>();
            if (pawnQueue == null)
                pawnQueue = new List<string>();
            if (queueJoinTimes == null)
                queueJoinTimes = new Dictionary<string, float>();
            if (pendingOffers == null)
                pendingOffers = new Dictionary<string, PendingPawnOffer>();
            if (expiredOffers == null)
                expiredOffers = new List<string>();
        }


        public override void GameComponentTick()
        {
            base.GameComponentTick();

            // Check for expired offers every 60 ticks (about 1 second)
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                CheckExpiredOffers();
            }
        }

        public void AssignPawnToViewer(string username, Pawn pawn)
        {
            viewerPawnAssignments[username.ToLowerInvariant()] = pawn.ThingID;
        }

        public Pawn GetAssignedPawn(string username)
        {
            if (viewerPawnAssignments.TryGetValue(username.ToLowerInvariant(), out string thingId))
            {
                Logger.Debug($"Retrieving assigned pawn for {username}, ThingID: {thingId}");
                Pawn pawn = FindPawnByThingId(thingId);
                if (pawn != null)
                {
                    Logger.Debug($"Found pawn: {pawn.Name}, Dead: {pawn.Dead}, Destroyed: {pawn.Destroyed}");
                }
                else
                {
                    Logger.Debug($"No pawn found with ThingID: {thingId}");
                }
                return pawn;
            }
            Logger.Debug($"No assignment found for username: {username}");
            return null;
        }

        public bool HasAssignedPawn(string username)
        {
            if (viewerPawnAssignments.TryGetValue(username.ToLowerInvariant(), out string thingId))
            {
                Logger.Debug($"Checking assigned pawn for {username}, ThingID: {thingId}");
                Pawn pawn = FindPawnByThingId(thingId);
                // Return true even if pawn is dead - we still want to allow resurrection
                return pawn != null;
            }
            return false;
        }

        public void UnassignPawn(string username)
        {
            viewerPawnAssignments.Remove(username.ToLowerInvariant());
        }

        public IEnumerable<string> GetAllAssignedUsernames()
        {
            return viewerPawnAssignments.Keys.ToList();
        }

        private static Pawn FindPawnByThingId(string thingId)
        {
            if (string.IsNullOrEmpty(thingId))
                return null;

            // Search all maps for the pawn (alive)
            foreach (var map in Find.Maps)
            {
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    if (pawn.ThingID == thingId)
                        return pawn;
                }

                // NEW: Also search for dead pawns/corpses
                foreach (var thing in map.listerThings.AllThings)
                {
                    if (thing.ThingID == thingId && thing is Corpse corpse)
                    {
                        return corpse.InnerPawn;
                    }
                }
            }

            // Also check world pawns (alive and dead)
            var worldPawn = Find.WorldPawns.AllPawnsAlive.FirstOrDefault(p => p.ThingID == thingId);
            if (worldPawn != null) return worldPawn;

            // NEW: Check dead world pawns
            var deadWorldPawn = Find.WorldPawns.AllPawnsDead.FirstOrDefault(p => p.ThingID == thingId);
            return deadWorldPawn;
        }
        public List<Pawn> GetAllViewerPawns()
        {
            var viewerPawns = new List<Pawn>();
            foreach (var thingId in viewerPawnAssignments.Values)
            {
                var pawn = FindPawnByThingId(thingId);
                if (pawn != null && !pawn.Dead)
                {
                    viewerPawns.Add(pawn);
                }
            }
            return viewerPawns;
        }

        public bool IsViewerPawn(Pawn pawn)
        {
            return viewerPawnAssignments.Values.Contains(pawn.ThingID);
        }

        public string GetUsernameForPawn(Pawn pawn)
        {
            var entry = viewerPawnAssignments.FirstOrDefault(x => x.Value == pawn.ThingID);
            return entry.Key ?? null;
        }

        // Queue management methods
        public bool AddToQueue(string username)
        {
            string lowerUsername = username.ToLowerInvariant();

            // Check if already in queue
            if (pawnQueue.Contains(lowerUsername))
            {
                return false;
            }

            // Check if already has a pawn
            if (HasAssignedPawn(lowerUsername))
            {
                return false;
            }

            pawnQueue.Add(lowerUsername);
            queueJoinTimes[lowerUsername] = Find.TickManager.TicksGame;
            return true;
        }

        public bool RemoveFromQueue(string username)
        {
            string lowerUsername = username.ToLowerInvariant();
            bool removed = pawnQueue.Remove(lowerUsername);
            if (removed)
            {
                queueJoinTimes.Remove(lowerUsername);
            }
            return removed;
        }

        public bool IsInQueue(string username)
        {
            return pawnQueue.Contains(username.ToLowerInvariant());
        }

        public string GetNextInQueue()
        {
            if (pawnQueue.Count == 0)
                return null;

            return pawnQueue[0];
        }

        public string PopNextInQueue()
        {
            if (pawnQueue.Count == 0)
                return null;

            string nextUser = pawnQueue[0];
            pawnQueue.RemoveAt(0);
            queueJoinTimes.Remove(nextUser);
            return nextUser;
        }

        public List<string> GetQueueList()
        {
            return new List<string>(pawnQueue);
        }

        public int GetQueuePosition(string username)
        {
            int position = pawnQueue.IndexOf(username.ToLowerInvariant());
            return position >= 0 ? position + 1 : -1;
        }

        public int GetQueueSize()
        {
            return pawnQueue.Count;
        }

        public void ClearQueue()
        {
            pawnQueue.Clear();
            queueJoinTimes.Clear();
        }
        public void AddPendingOffer(string username, Pawn pawn, int timeoutSeconds = -1)
        {
            // Use global setting if not specified, default to 300 seconds (5 minutes)
            if (timeoutSeconds == -1)
            {
                var settings = CAPChatInteractiveMod.Instance?.Settings?.GlobalSettings;
                timeoutSeconds = settings?.PawnOfferTimeoutSeconds ?? 300;
            }

            pendingOffers[username.ToLowerInvariant()] = new PendingPawnOffer
            {
                Username = username,
                OfferTime = Find.TickManager.TicksGame,
                TimeoutTicks = timeoutSeconds * 60,
                PawnThingId = pawn?.ThingID // NEW: Store the pawn's ID
            };
        }

        public bool HasPendingOffer(string username)
        {
            return pendingOffers.ContainsKey(username.ToLowerInvariant());
        }

        public Pawn AcceptPendingOffer(string username)
        {
            string lowerUsername = username.ToLowerInvariant();
            if (pendingOffers.TryGetValue(lowerUsername, out PendingPawnOffer offer))
            {
                // Find the pawn by its stored ThingID
                Pawn pawn = FindPawnByThingId(offer.PawnThingId);
                pendingOffers.Remove(lowerUsername);

                // Only assign if pawn is still valid
                if (pawn != null && !pawn.Dead)
                {
                    // Set the pawn's nickname to the username
                    if (pawn.Name is NameTriple nameTriple)
                    {
                        pawn.Name = new NameTriple(nameTriple.First, username, nameTriple.Last);
                    }
                    else
                    {
                        pawn.Name = new NameSingle(username);
                    }

                    // Assign the pawn to the viewer
                    AssignPawnToViewer(username, pawn);

                    // Debug logging
                    Logger.Debug($"Successfully assigned pawn {pawn.Name} (ThingID: {pawn.ThingID}) to viewer {username}");

                    return pawn;
                }
                else
                {
                    Logger.Debug($"Pawn offer for {username} failed - pawn null: {pawn == null}, pawn dead: {(pawn != null && pawn.Dead)}");
                    return null;
                }
            }

            Logger.Debug($"No pending offer found for {username}");
            return null;
        }

        public void RemovePendingOffer(string username)
        {
            pendingOffers.Remove(username.ToLowerInvariant());
        }

        private void CheckExpiredOffers()
        {
            var currentTicks = Find.TickManager.TicksGame;
            var expired = new List<string>();

            foreach (var offer in pendingOffers)
            {
                if (currentTicks - offer.Value.OfferTime > offer.Value.TimeoutTicks)
                {
                    expired.Add(offer.Key);
                    expiredOffers.Add(offer.Key);

                    // Send timeout message to chat using the new broadcast function
                    string timeoutMessage = $"⏰ Your pawn offer has expired! Join the queue again with !join";
                    ChatCommandProcessor.SendMessageToUsername(offer.Value.Username, timeoutMessage);
                }
            }

            // Remove expired offers
            foreach (string username in expired)
            {
                pendingOffers.Remove(username);
            }
        }

        public List<PendingPawnOffer> GetPendingOffers()
        {
            return pendingOffers.Values.ToList();
        }

        public List<string> GetExpiredOffers()
        {
            return new List<string>(expiredOffers);
        }

        public void ClearExpiredOffers()
        {
            expiredOffers.Clear();
        }
    }

    // NEW: Pending offer data structure
    public class PendingPawnOffer : IExposable
    {
        public string Username;
        public float OfferTime;
        public int TimeoutTicks;
        public string PawnThingId;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Username, "username");
            Scribe_Values.Look(ref OfferTime, "offerTime");
            Scribe_Values.Look(ref TimeoutTicks, "timeoutTicks");
            Scribe_Values.Look(ref PawnThingId, "pawnThingId");
        }

        public float TimeRemaining
        {
            get
            {
                float elapsed = Find.TickManager.TicksGame - OfferTime;
                return Mathf.Max(0, (TimeoutTicks - elapsed) / 60f); // Return seconds remaining
            }
        }

        public bool IsExpired
        {
            get
            {
                return TimeRemaining <= 0;
            }
        }
    }
}