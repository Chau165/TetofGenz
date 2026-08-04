using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Per-play-session vehicle booking details shown on the inventory note.
    /// </summary>
    public static class VehicleBookingSession
    {
        private static readonly int[] CommonProvinceCodes = { 11, 30, 43, 50, 51, 59, 79, 92 };

        private static string licensePlate;
        private static string pickupTime;
        private static bool initialized;

        public static string LicensePlate
        {
            get
            {
                if (!initialized)
                {
                    Initialize();
                }

                return licensePlate;
            }
        }

        public static string PickupTime
        {
            get
            {
                if (!initialized)
                {
                    Initialize();
                }

                return pickupTime;
            }
        }

        public static string GenerateLicensePlate()
        {
            int province = CommonProvinceCodes[Random.Range(0, CommonProvinceCodes.Length)];
            char letter = (char)('A' + Random.Range(0, 26));
            int mid = Random.Range(100, 1000);
            int tail = Random.Range(10, 100);
            return $"{province}{letter}-{mid}.{tail:D2}";
        }

        private static void Initialize()
        {
            licensePlate = GenerateLicensePlate();
            int totalMinutes = Random.Range(87, 94) * 5;
            int hour = totalMinutes / 60;
            int minute = totalMinutes % 60;
            pickupTime = $"{hour:00}:{minute:00}";
            initialized = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewSession()
        {
            licensePlate = null;
            pickupTime = null;
            initialized = false;
        }
    }
}
