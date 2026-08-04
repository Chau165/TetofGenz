using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Pooled highway traffic unit — lane-locked, no body rotation.</summary>
    public class HighwayTrafficVehicle : MonoBehaviour
    {
        public enum TrafficKind
        {
            Sedan,
            Truck,
            Bus,
            Van
        }

        private Transform visualRoot;
        private GameObject prefabSource;
        private WheelRotator wheelRotator;
        private HighwayVisualPool pool;
        private BoxCollider hitBox;
        private AudioSource engineAudio;

        public int LaneIndex { get; private set; }
        public float SpeedZ { get; private set; }
        public float Length { get; private set; }
        public bool IsActive { get; private set; }
        public TrafficKind Kind { get; private set; }

        public void BindPool(HighwayVisualPool visualPool)
        {
            pool = visualPool;
            EnsureCollider();
        }

        public void Activate(
            GameObject prefab,
            int laneIndex,
            float speedZ,
            float scale,
            int bodyColorIndex,
            TrafficKind kind,
            float length)
        {
            prefabSource = prefab;
            LaneIndex = laneIndex;
            SpeedZ = speedZ;
            Length = length;
            Kind = kind;
            IsActive = true;

            ReleaseVisual();
            AttachVisual(prefab, scale, bodyColorIndex);
            transform.rotation = Quaternion.identity;
            UpdatePositionX();
            gameObject.SetActive(true);
            StartPassByAudio();
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            Vector3 pos = transform.position;
            pos.z += SpeedZ * deltaTime;
            pos.y = HighwayLaneSettings.RoadSurfaceY;
            transform.position = pos;

            wheelRotator?.SetSpeed(Mathf.Abs(SpeedZ));
            UpdatePassByAudio();
        }

        public void SetSpeedZ(float speedZ)
        {
            SpeedZ = speedZ;
        }

        public void SmoothNudgeWorldZ(float targetZ, float deltaTime, float smoothSpeed)
        {
            Vector3 pos = transform.position;
            pos.z = Mathf.Lerp(pos.z, targetZ, Mathf.Clamp01(deltaTime * smoothSpeed));
            pos.y = HighwayLaneSettings.RoadSurfaceY;
            transform.position = pos;
        }

        public void Deactivate()
        {
            IsActive = false;
            StopPassByAudio();
            ReleaseVisual();
            gameObject.SetActive(false);
        }

        public bool OverlapsPlayer(Vector3 playerPos, float playerLength)
        {
            if (!IsActive)
            {
                return false;
            }

            float halfSumZ = (Length + playerLength) * 0.5f;
            float halfSumX = HighwayLaneSettings.LaneWidth * 0.42f;
            Vector3 delta = transform.position - playerPos;
            return Mathf.Abs(delta.x) < halfSumX && Mathf.Abs(delta.z) < halfSumZ;
        }

        private void AttachVisual(GameObject prefab, float scale, int bodyColorIndex)
        {
            if (prefab == null || pool == null)
            {
                return;
            }

            visualRoot = pool.Acquire(prefab, transform);
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one * scale;

            StripPhysics(visualRoot.gameObject);
            Level1TrafficVisualUtility.ApplyCarLook(visualRoot.gameObject, bodyColorIndex);

            wheelRotator = visualRoot.GetComponent<WheelRotator>();
            if (wheelRotator == null)
            {
                wheelRotator = visualRoot.gameObject.AddComponent<WheelRotator>();
            }

            wheelRotator.Configure(0.32f, Vector3.right);
            wheelRotator.ResetCache();
            wheelRotator.SetSpeed(Mathf.Abs(SpeedZ));

            EnsureCollider();
            hitBox.size = new Vector3(HighwayLaneSettings.LaneWidth * 0.75f, 1.4f, Length);
            hitBox.center = new Vector3(0f, 0.7f, 0f);
        }

        private void ReleaseVisual()
        {
            if (visualRoot != null && pool != null && prefabSource != null)
            {
                pool.Release(visualRoot, prefabSource);
            }
            else if (visualRoot != null)
            {
                HighwayPropUtility.SafeDestroy(visualRoot.gameObject);
            }

            visualRoot = null;
            prefabSource = null;
            wheelRotator = null;
        }

        private void UpdatePositionX()
        {
            Vector3 pos = transform.position;
            pos.x = HighwayLaneSettings.LaneCenterX(LaneIndex);
            pos.y = HighwayLaneSettings.RoadSurfaceY;
            transform.position = pos;
        }

        private void EnsureCollider()
        {
            if (hitBox != null)
            {
                return;
            }

            hitBox = gameObject.GetComponent<BoxCollider>();
            if (hitBox == null)
            {
                hitBox = gameObject.AddComponent<BoxCollider>();
            }

            hitBox.isTrigger = true;
        }

        private void StartPassByAudio()
        {
            if (engineAudio == null)
            {
                engineAudio = gameObject.GetComponent<AudioSource>();
                if (engineAudio == null)
                {
                    engineAudio = gameObject.AddComponent<AudioSource>();
                }
            }

            engineAudio.playOnAwake = false;
            engineAudio.loop = true;
            engineAudio.spatialBlend = 1f;
            engineAudio.minDistance = 8f;
            engineAudio.maxDistance = 55f;
            engineAudio.rolloffMode = AudioRolloffMode.Linear;
            engineAudio.priority = 160;
            engineAudio.mute = false;
            engineAudio.dopplerLevel = 0.35f;
            engineAudio.clip = ProceduralAudio.GetTrafficPassByClip();
            engineAudio.pitch = Random.Range(0.88f, 1.12f);
            engineAudio.volume = Kind == TrafficKind.Truck || Kind == TrafficKind.Bus ? 0.38f : 0.28f;
            if (!engineAudio.isPlaying)
            {
                engineAudio.Play();
            }
        }

        private void UpdatePassByAudio()
        {
            if (engineAudio == null || !engineAudio.isPlaying)
            {
                return;
            }

            float speedFactor = Mathf.Clamp01(Mathf.Abs(SpeedZ) / 35f);
            engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, 0.9f + speedFactor * 0.35f, Time.deltaTime * 2f);
        }

        private void StopPassByAudio()
        {
            if (engineAudio != null && engineAudio.isPlaying)
            {
                engineAudio.Stop();
            }
        }

        private static void StripPhysics(GameObject root)
        {
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                HighwayPropUtility.SafeDestroy(bodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                HighwayPropUtility.SafeDestroy(colliders[i]);
            }
        }
    }
}
