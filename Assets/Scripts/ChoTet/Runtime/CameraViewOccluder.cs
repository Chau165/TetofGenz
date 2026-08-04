using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Marks foliage or props that may block a top-down camera. Paired with <see cref="TopDownCameraOcclusion"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraViewOccluder : MonoBehaviour
    {
        [SerializeField] private float minHeight = 1.15f;
        [SerializeField] private float hideThreshold = 0.45f;

        private Renderer[] renderers;
        private float targetStrength;
        private float currentStrength;

        public bool IsRegistered { get; private set; }

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            EnsureCollider();
            IsRegistered = HasMeaningfulHeight();
            if (!IsRegistered)
            {
                enabled = false;
            }
        }

        public void SetTargetStrength(float strength)
        {
            targetStrength = Mathf.Clamp01(strength);
        }

        private void Update()
        {
            if (!IsRegistered || renderers == null)
            {
                return;
            }

            currentStrength = Mathf.MoveTowards(currentStrength, targetStrength, Time.deltaTime * 10f);
            bool visible = currentStrength < hideThreshold;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }
        }

        private void OnDisable()
        {
            targetStrength = 0f;
            currentStrength = 0f;
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = true;
                }
            }
        }

        private bool HasMeaningfulHeight()
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderers[i].bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return hasBounds && bounds.size.y >= minHeight;
        }

        private void EnsureCollider()
        {
            if (GetComponentInChildren<Collider>() != null)
            {
                return;
            }

            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderers[i].bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            if (!hasBounds)
            {
                return;
            }

            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.center = transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size;
            box.isTrigger = true;
        }
    }
}
