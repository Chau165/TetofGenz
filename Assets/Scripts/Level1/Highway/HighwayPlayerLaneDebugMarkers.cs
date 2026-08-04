using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Runtime markers and editor gizmos for the 3 playable lane centers.</summary>
    public class HighwayPlayerLaneDebugMarkers : MonoBehaviour
    {
        [SerializeField] private bool showMarkers = false;
        [SerializeField] private float markerY = 0.35f;
        [SerializeField] private float markerScale = 0.55f;

        private Transform[] markers;

        private void Awake()
        {
            BuildMarkers();
        }

        private void LateUpdate()
        {
            if (!showMarkers || markers == null)
            {
                return;
            }

            float z = transform.position.z;
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i] == null)
                {
                    continue;
                }

                markers[i].position = new Vector3(
                    HighwayPlayerLaneLayout.LaneCenterX(i),
                    markerY,
                    z);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showMarkers)
            {
                return;
            }

            float z = Application.isPlaying ? transform.position.z : 0f;
            for (int i = 0; i < HighwayPlayerLaneLayout.LaneCount; i++)
            {
                Gizmos.color = i switch
                {
                    0 => Color.cyan,
                    1 => Color.green,
                    _ => Color.yellow
                };

                Vector3 center = new Vector3(HighwayPlayerLaneLayout.LaneCenterX(i), markerY, z);
                Gizmos.DrawSphere(center, markerScale * 0.45f);
                Gizmos.DrawLine(center + Vector3.left * markerScale, center + Vector3.right * markerScale);
            }
        }

        private void BuildMarkers()
        {
            if (!showMarkers)
            {
                return;
            }

            var root = new GameObject("LaneCenterMarkers");
            root.transform.SetParent(transform, false);
            markers = new Transform[HighwayPlayerLaneLayout.LaneCount];

            for (int i = 0; i < markers.Length; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "LaneCenter_" + i;
                marker.transform.SetParent(root.transform, false);
                marker.transform.localScale = Vector3.one * markerScale;
                HighwayPropUtility.SafeDestroy(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().sharedMaterial = CreateMarkerMaterial(i);
                markers[i] = marker.transform;
            }
        }

        private static Material CreateMarkerMaterial(int laneIndex)
        {
            Color color = laneIndex switch
            {
                0 => Color.cyan,
                1 => Color.green,
                _ => Color.yellow
            };

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            return mat;
        }
    }
}
