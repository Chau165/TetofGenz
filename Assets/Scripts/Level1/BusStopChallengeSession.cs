using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1
{
    public enum VehicleBoardingOutcome
    {
        None = 0,
        HomeRide = 1,
        Kidnap = 2,
        WrongVehicle = 3
    }

    public struct BusStopCandidateSlot
    {
        public VehicleBoardingOutcome Outcome;
        public int BodyColorIndex;
        public int PrefabIndex;
    }

    /// <summary>
    /// Five bus-stop candidates per outdoor phase: one correct ride home, four risky rides.
    /// </summary>
    public static class BusStopChallengeSession
    {
        public const int TotalCandidates = 5;

        private static readonly List<BusStopCandidateSlot> remainingSlots = new List<BusStopCandidateSlot>();
        private static BusStopCandidateSlot homeRideSlot;
        private static BusStopCandidateSlot rememberedHomeRideSlot;
        private static bool homeRideAppearanceRemembered;
        private static int dispatchedCount;
        private static int rejectionCount;
        private static bool active;
        private static bool playerSucceeded;
        private static bool encorePending;
        private static bool encoreSpawned;
        private static bool encoreDialoguePlayed;

        public static bool IsActive => active;
        public static int DispatchedCount => dispatchedCount;
        public static int RejectionCount => rejectionCount;
        public static int RemainingCount => remainingSlots.Count;
        public static bool HasRemainingCandidates => active && remainingSlots.Count > 0;
        public static bool EncorePending => encorePending && !encoreSpawned && !playerSucceeded;
        public static bool EncoreDialoguePlayed => encoreDialoguePlayed;

        public static bool TryGetHomeRideSlot(out BusStopCandidateSlot slot)
        {
            if (!active)
            {
                slot = default;
                return false;
            }

            slot = homeRideAppearanceRemembered ? rememberedHomeRideSlot : homeRideSlot;
            slot.Outcome = VehicleBoardingOutcome.HomeRide;
            return true;
        }

        public static void BeginOutdoorChallenge(int bodyColorCount, int candidatePrefabCount)
        {
            remainingSlots.Clear();
            rejectionCount = 0;
            dispatchedCount = 0;
            playerSucceeded = false;
            encorePending = false;
            encoreSpawned = false;
            encoreDialoguePlayed = false;
            homeRideAppearanceRemembered = false;
            rememberedHomeRideSlot = default;

            var outcomes = new List<VehicleBoardingOutcome>
            {
                VehicleBoardingOutcome.HomeRide,
                VehicleBoardingOutcome.Kidnap,
                VehicleBoardingOutcome.Kidnap,
                VehicleBoardingOutcome.WrongVehicle,
                VehicleBoardingOutcome.WrongVehicle
            };
            Shuffle(outcomes);

            int colorCount = Mathf.Max(1, bodyColorCount);
            int prefabCount = Mathf.Max(1, candidatePrefabCount);
            var colorIndices = BuildDistinctIndices(colorCount, TotalCandidates);
            var prefabIndices = BuildDistinctIndices(prefabCount, TotalCandidates);

            for (int i = 0; i < TotalCandidates; i++)
            {
                var slot = new BusStopCandidateSlot
                {
                    Outcome = outcomes[i],
                    BodyColorIndex = colorIndices[i],
                    PrefabIndex = prefabIndices[i]
                };
                remainingSlots.Add(slot);
                if (slot.Outcome == VehicleBoardingOutcome.HomeRide)
                {
                    homeRideSlot = slot;
                }
            }

            active = true;
        }

        public static bool TryTakeNextCandidate(out BusStopCandidateSlot slot)
        {
            if (!HasRemainingCandidates)
            {
                slot = default;
                return false;
            }

            slot = remainingSlots[0];
            remainingSlots.RemoveAt(0);
            dispatchedCount++;
            return true;
        }

        public static void ReturnCandidate(BusStopCandidateSlot slot)
        {
            if (!active)
            {
                return;
            }

            remainingSlots.Insert(0, slot);
            dispatchedCount = Mathf.Max(0, dispatchedCount - 1);
        }

        public static void RememberHomeRideAppearance(BusStopCandidateSlot slot)
        {
            if (!active || slot.Outcome != VehicleBoardingOutcome.HomeRide)
            {
                return;
            }

            rememberedHomeRideSlot = slot;
            homeRideSlot = slot;
            homeRideAppearanceRemembered = true;
        }

        public static bool TryGetEncoreHomeSlot(out BusStopCandidateSlot slot)
        {
            if (!EncorePending)
            {
                slot = default;
                return false;
            }

            slot = homeRideAppearanceRemembered ? rememberedHomeRideSlot : homeRideSlot;
            slot.Outcome = VehicleBoardingOutcome.HomeRide;
            return true;
        }

        public static void MarkEncoreSpawned()
        {
            encoreSpawned = true;
            encorePending = false;
        }

        public static void RecordRejection()
        {
            if (!active || playerSucceeded)
            {
                return;
            }

            rejectionCount++;
            if (rejectionCount >= TotalCandidates && !encoreSpawned && !playerSucceeded)
            {
                encorePending = true;
            }
        }

        public static void MarkPlayerSucceeded()
        {
            playerSucceeded = true;
            encorePending = false;
        }

        public static void MarkEncoreDialoguePlayed()
        {
            encoreDialoguePlayed = true;
        }

        private static List<int> BuildDistinctIndices(int poolSize, int count)
        {
            var pool = new List<int>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                pool.Add(i);
            }

            Shuffle(pool);

            var result = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(pool[i % pool.Count]);
            }

            return result;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewSession()
        {
            remainingSlots.Clear();
            dispatchedCount = 0;
            rejectionCount = 0;
            active = false;
            playerSucceeded = false;
            encorePending = false;
            encoreSpawned = false;
            encoreDialoguePlayed = false;
            homeRideAppearanceRemembered = false;
            rememberedHomeRideSlot = default;
        }
    }
}
