using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Low-density layered grass details over continuous roadside grass ground.</summary>
    public static class HighwayNatureGroundCover
    {
        private static readonly float[] RowAnchors =
        {
            0.22f, 0.5f, 0.78f
        };

        public struct GrassPalette
        {
            public Material BladeA;
            public Material BladeB;
        }

        public static void Build(
            Transform parent,
            bool leftSide,
            int segmentIndex,
            HighwayNaturePrefabLibrary library,
            GrassPalette palette,
            int seed)
        {
            HighwayPropUtility.ClearChildren(parent);
            if (palette.BladeA == null)
            {
                return;
            }

            float segmentLength = HighwayRoadLayout.SegmentLength;
            int clusterIndex = 0;

            for (int row = 0; row < RowAnchors.Length; row++)
            {
                // Three broad bands (near / mid / far) with deterministic spacing.
                float rowZ = segmentLength * RowAnchors[row];
                int clustersInRow = row == 1 ? 3 : 2;

                for (int c = 0; c < clustersInRow; c++)
                {
                    int clusterSeed = seed + segmentIndex * 113 + clusterIndex * 47;
                    clusterIndex++;

                    float lateralT = (c + 0.5f) / clustersInRow;
                    float near = HighwayRoadsideLayout.GrassClusterNearOutward;
                    float mid = HighwayRoadsideLayout.GrassClusterMidOutward;
                    float far = HighwayRoadsideLayout.GrassClusterFarOutward;
                    float outward = row switch
                    {
                        0 => Mathf.Lerp(near, near + 2.5f, lateralT),
                        1 => Mathf.Lerp(near + 2f, mid, lateralT),
                        _ => Mathf.Lerp(mid - 1f, far, lateralT)
                    };
                    outward = HighwayRoadsideLayout.ClampOutwardOffset(outward);

                    float zJitter = ((clusterSeed % 7) - 3) * 0.8f;
                    float centerZ = Mathf.Clamp(rowZ + zJitter, 2f, segmentLength - 2f);

                    float signedX = HighwayRoadsideLayout.SignedWorldX(leftSide, outward);
                    if (!HighwayRoadsideLayout.IsValidVegetationAbsX(Mathf.Abs(signedX)))
                    {
                        continue;
                    }

                    GameObject cluster = new GameObject("GrassCluster_" + clusterIndex);
                    cluster.transform.SetParent(parent, false);
                    cluster.transform.localPosition = new Vector3(signedX, 0.02f, centerZ);

                    HighwayNatureWindSway sway = cluster.AddComponent<HighwayNatureWindSway>();
                    sway.ConfigureForCluster(clusterSeed);

                    int tuftsInCluster = 2 + clusterSeed % 2;
                    for (int t = 0; t < tuftsInCluster; t++)
                    {
                        int tuftSeed = clusterSeed + t * 29;
                        float tuftOutward = ((tuftSeed * 7) % 6) * 0.08f;
                        float along = ((tuftSeed * 11) % 10 - 5) * 0.1f;
                        float scale = Mathf.Lerp(0.75f, 1.05f, (tuftSeed % 100) / 100f);

                        GameObject tuft = HighwayNatureGrassTuftMesh.CreateTuft(
                            cluster.transform,
                            palette.BladeA,
                            palette.BladeB,
                            tuftSeed,
                            scale,
                            addWindSway: false);

                        float tuftX = HighwayRoadsideLayout.SignedWorldX(leftSide, outward + tuftOutward);
                        tuft.transform.localPosition = new Vector3(
                            tuftX - signedX,
                            0f,
                            along);
                    }

                    if (library != null && row == 2 && (clusterSeed % 4) == 0)
                    {
                        SpawnGroundBush(cluster.transform, library, clusterSeed, leftSide, outward);
                    }
                }
            }
        }

        private static void SpawnGroundBush(
            Transform parent,
            HighwayNaturePrefabLibrary library,
            int seed,
            bool leftSide,
            float clusterOutward)
        {
            GameObject prefab = library.PickBush(seed);
            if (prefab == null)
            {
                return;
            }

            float bushOutward = HighwayRoadsideLayout.ClampOutwardOffset(clusterOutward + 2.2f);
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            float scale = Mathf.Lerp(0.42f, 0.62f, (seed % 100) / 100f);
            instance.transform.localPosition = new Vector3(
                HighwayRoadsideLayout.SignedWorldX(leftSide, bushOutward) - parent.localPosition.x,
                0f,
                ((seed * 3) % 5) * 0.3f);
            instance.transform.localRotation = Quaternion.Euler(0f, (seed * 23) % 360, 0f);
            instance.transform.localScale = Vector3.one * scale;
            HighwayPropUtility.StripColliders(instance);
        }
    }
}
