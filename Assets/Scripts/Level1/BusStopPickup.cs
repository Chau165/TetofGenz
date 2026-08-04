using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Player interaction with a stopped pickup vehicle at the Level 1 bus stop.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BusStopPickup : Interactable
    {
        private VehicleTraffic hostVehicle;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }

            SetPrompt("Nhấn E để lên xe");
        }

        public void Bind(VehicleTraffic vehicle)
        {
            hostVehicle = vehicle;
            SetPrompt("Nhấn E để lên xe");
            enabled = false;
        }

        public void SetWaiting(bool waiting)
        {
            enabled = waiting;
        }

        public override bool CanInteract()
        {
            if (!base.CanInteract())
            {
                return false;
            }

            if (hostVehicle == null || !hostVehicle.IsWaitingForPickup || hostVehicle.IsEncoreRide)
            {
                return false;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            return phase != null &&
                   phase.CurrentPhase == Level1PhaseController.Level1Phase.OutdoorStreet &&
                   !phase.IsTransitioning;
        }

        public override void Interact(PlayerTopDownController player)
        {
            if (hostVehicle == null || player == null)
            {
                return;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            if (phase != null)
            {
                phase.BeginBoardingTransition(player, hostVehicle);
            }
        }
    }
}
