using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Light elevated deck fascia and support piers — road stays at gameplay height.</summary>
    public static class HighwayElevatedStructureBuilder
    {
        private const float PierSpacing = 14f;
        private const float PierSize = 0.7f;
        private const float DeckFasciaHeight = 0.38f;
        private const float DeckFasciaThickness = 0.22f;

        public static void Build(
            Transform segmentRoot,
            float segmentLength,
            Material bridgeConcrete,
            Material underside)
        {
            if (bridgeConcrete == null)
            {
                return;
            }

            Transform structure = segmentRoot.Find("ElevatedStructure");
            if (structure == null)
            {
                GameObject go = new GameObject("ElevatedStructure");
                go.transform.SetParent(segmentRoot, false);
                go.transform.localRotation = Quaternion.identity;
                structure = go.transform;
            }
            else
            {
                HighwayPropUtility.ClearChildren(structure);
            }

            float outer = HighwayRoadLayout.ShoulderOuterX;
            float shoulderY = HighwayLaneSettings.ShoulderSurfaceY;
            float deckY = HighwayLaneSettings.RoadDeckY;
            float length = segmentLength + HighwayRoadLayout.SegmentOverlapZ;
            float centerZ = length * 0.5f;
            Material pierMat = underside ?? bridgeConcrete;

            BuildDeckFascia(structure, "Fascia_L", -outer, deckY, shoulderY, centerZ, length, bridgeConcrete);
            BuildDeckFascia(structure, "Fascia_R", outer, deckY, shoulderY, centerZ, length, bridgeConcrete);

            for (float z = PierSpacing * 0.5f; z <= length + 0.01f; z += PierSpacing)
            {
                BuildPier(structure, "Pier_L_" + Mathf.RoundToInt(z * 10f), -outer - 0.25f, shoulderY, z, pierMat);
                BuildPier(structure, "Pier_R_" + Mathf.RoundToInt(z * 10f), outer + 0.25f, shoulderY, z, pierMat);
            }
        }

        private static void BuildDeckFascia(
            Transform parent,
            string name,
            float outerX,
            float deckY,
            float shoulderY,
            float centerZ,
            float length,
            Material mat)
        {
            float inward = outerX < 0f ? 1f : -1f;
            float fasciaX = outerX + inward * DeckFasciaThickness * 0.5f;
            float fasciaY = (deckY + shoulderY) * 0.5f + DeckFasciaHeight * 0.2f;
            BuildSlab(parent, name, mat,
                new Vector3(fasciaX, fasciaY, centerZ),
                new Vector3(DeckFasciaThickness, DeckFasciaHeight, length));
        }

        private static void BuildPier(
            Transform parent,
            string name,
            float x,
            float shoulderY,
            float z,
            Material mat)
        {
            float pierHeight = 4.6f;
            float pierCenterY = shoulderY - pierHeight * 0.5f - 0.1f;
            BuildSlab(parent, name, mat,
                new Vector3(x, pierCenterY, z),
                new Vector3(PierSize, pierHeight, PierSize));
        }

        private static void BuildSlab(Transform parent, string name, Material mat, Vector3 localPos, Vector3 size)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = localPos;
            slab.transform.localRotation = Quaternion.identity;
            slab.transform.localScale = size;
            HighwayPropUtility.SafeDestroy(slab.GetComponent<Collider>());
            if (mat != null)
            {
                var renderer = slab.GetComponent<Renderer>();
                renderer.sharedMaterial = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
}
