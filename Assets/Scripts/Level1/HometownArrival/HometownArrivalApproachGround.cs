using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Grass floor and invisible walk colliders under the arrival road corridor.</summary>
    internal static class HometownArrivalApproachGround
    {
        private const float FillCenterZ = 58f;
        private const float FillLengthZ = 260f;
        private const float FillWidthX = 130f;
        private const float FillTopY = -0.28f;
        private const float FillThickness = 0.32f;

        public static void EnsureBaseFill(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            DestroyChild(parent, "ApproachBaseFill");
            DestroyChild(parent, "ApproachWalkSurface");

            Material grass = HometownArrivalResourceCache.LoadMaterial("Highway_Grass_NSK2.mat")
                ?? HometownArrivalResourceCache.LoadMaterial("Grass_Ground.mat");
            if (grass == null)
            {
                return;
            }

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "ApproachBaseFill";
            fill.transform.SetParent(parent, false);
            fill.transform.position = new Vector3(0f, FillTopY - FillThickness * 0.5f, FillCenterZ);
            fill.transform.localScale = new Vector3(FillWidthX, FillThickness, FillLengthZ);

            MeshRenderer renderer = fill.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = grass;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }

            Collider baseCollider = fill.GetComponent<Collider>();
            if (baseCollider != null)
            {
                Object.Destroy(baseCollider);
            }

            EnsureWalkSurface(parent);
        }

        private static void EnsureWalkSurface(Transform parent)
        {
            GameObject walk = new GameObject("ApproachWalkSurface");
            walk.transform.SetParent(parent, false);
            walk.transform.position = new Vector3(0f, HighwayLaneSettings.RoadSurfaceY - 0.03f, FillCenterZ);

            BoxCollider collider = walk.AddComponent<BoxCollider>();
            collider.size = new Vector3(24f, 0.06f, FillLengthZ);
            collider.isTrigger = false;
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }
        }
    }
}
