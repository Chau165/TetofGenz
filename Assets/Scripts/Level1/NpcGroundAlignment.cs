using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Aligns a static NPC's visual mesh so both feet rest on the sampled walk surface.</summary>
    [DisallowMultipleComponent]
    public class NpcGroundAlignment : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float probeUp = 2.5f;
        [SerializeField] private float probeDown = 4f;
        [SerializeField] private bool alignOnStart = true;
        [SerializeField] private float visualFeetLiftLocal;

        private bool aligned;

        public Transform VisualRoot => visualRoot;

        public void Configure(Transform root, bool runOnStart = true)
        {
            visualRoot = root;
            alignOnStart = runOnStart;
        }

        private void Start()
        {
            if (alignOnStart)
            {
                AlignToGround();
            }
            else if (visualRoot != null)
            {
                visualRoot.localPosition = new Vector3(
                    visualRoot.localPosition.x,
                    visualFeetLiftLocal,
                    visualRoot.localPosition.z);
            }
        }

        public void AlignToGround()
        {
            if (aligned)
            {
                return;
            }

            if (!TrySampleGround(transform.position, out Vector3 feetPosition))
            {
                feetPosition = new Vector3(transform.position.x, Level1OutdoorSettings.WalkSurfaceY, transform.position.z);
            }

            transform.position = new Vector3(transform.position.x, feetPosition.y, transform.position.z);
            EnsureVisualRoot();

            if (visualRoot != null)
            {
                visualRoot.localPosition = new Vector3(visualRoot.localPosition.x, 0f, visualRoot.localPosition.z);
            }

            visualFeetLiftLocal = ComputeVisualFeetLiftLocal();
            if (visualRoot != null)
            {
                visualRoot.localPosition = new Vector3(
                    visualRoot.localPosition.x,
                    visualFeetLiftLocal,
                    visualRoot.localPosition.z);
            }

            aligned = true;
        }

        private void EnsureVisualRoot()
        {
            if (visualRoot != null)
            {
                return;
            }

            Transform existing = transform.Find("VisualRoot");
            if (existing != null)
            {
                visualRoot = existing;
                return;
            }

            if (transform.childCount == 0)
            {
                return;
            }

            GameObject rootObject = new GameObject("VisualRoot");
            visualRoot = rootObject.transform;
            visualRoot.SetParent(transform, false);

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name == "NpcShadow" || child.GetComponent<NPCDialogue>() != null)
                {
                    continue;
                }

                child.SetParent(visualRoot, true);
            }
        }

        private float ComputeVisualFeetLiftLocal()
        {
            if (visualRoot == null)
            {
                return 0f;
            }

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return 0f;
            }

            Bounds combined = default;
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!IsFeetRenderer(renderer))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    combined = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds || combined.size.y < 0.08f)
            {
                return 0f;
            }

            return transform.position.y - combined.min.y;
        }

        private static bool IsFeetRenderer(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled)
            {
                return false;
            }

            if (renderer is SkinnedMeshRenderer skinned && skinned.sharedMesh == null)
            {
                return false;
            }

            string name = renderer.gameObject.name;
            return name != "NpcShadow" && !name.StartsWith("Shadow");
        }

        private static bool TrySampleGround(Vector3 worldPosition, out Vector3 feetPosition)
        {
            return NpcPlacementValidator.TrySampleGround(worldPosition, out feetPosition);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 feet = transform.position;
            NpcPlacementValidator.DrawCapsuleGizmo(feet, Mathf.Max(0.1f, transform.lossyScale.x), new Color(0.2f, 0.85f, 1f, 0.85f));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(feet, feet + Vector3.up * probeUp);
            Gizmos.DrawLine(feet, feet + Vector3.down * probeDown);
        }
#endif
    }
}
