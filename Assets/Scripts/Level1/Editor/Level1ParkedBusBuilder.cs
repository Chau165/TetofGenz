#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a low-poly placeholder bus for the Level 1 bus pickup area.</summary>
    public static class Level1ParkedBusBuilder
    {
        public static GameObject CreateVisualTemplate(Transform parent)
        {
            Vector3 position = new Vector3(
                Level1BusStopRoadSettings.BusParkX,
                Level1BusStopRoadSettings.BusRootY,
                Level1BusStopRoadSettings.PickupStopZ());
            Quaternion rotation = Quaternion.Euler(0f, Level1BusStopRoadSettings.BusFacingYaw, 0f);
            GameObject bus = Create(parent, position, rotation);
            bus.name = "BusVisualTemplate";

            BoxCollider blocker = bus.GetComponent<BoxCollider>();
            if (blocker != null)
            {
                Object.DestroyImmediate(blocker);
            }

            bus.SetActive(false);
            return bus;
        }

        public static GameObject Create(Transform parent, Vector3 position, Quaternion rotation)
        {
            Material body = LoadMaterial("Accent_Red.mat");
            Material roof = LoadMaterial("Roof_Dark.mat");
            Material glass = LoadMaterial("Glass_Blue.mat");
            Material trim = LoadMaterial("Wood_Warm.mat");
            Material wheel = LoadMaterial("Dark_Brown.mat");
            Material headlight = LoadMaterial("Lantern_Gold.mat");

            GameObject root = new GameObject("Level1_ParkedBus");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = rotation;

            // Body aligned along local +Z; use Yaw=90 so the bus runs left↔right on the horizontal road.
            // Door stays on the south (+Z) side toward the waiting area.
            CreatePart(root.transform, "BusBody", new Vector3(0f, 1.05f, 0f),
                new Vector3(2.15f, 1.35f, 4.85f), body);
            CreatePart(root.transform, "BusRoof", new Vector3(0f, 1.92f, -0.08f),
                new Vector3(2.05f, 0.28f, 4.55f), roof);
            CreatePart(root.transform, "BusWindshield", new Vector3(0f, 1.28f, 2.38f),
                new Vector3(1.85f, 0.72f, 0.12f), glass);
            CreatePart(root.transform, "BusRearWindow", new Vector3(0f, 1.32f, -2.28f),
                new Vector3(1.75f, 0.62f, 0.1f), glass);

            for (int i = 0; i < 3; i++)
            {
                float z = 1.35f - i * 1.35f;
                CreatePart(root.transform, "BusWindowL_" + i, new Vector3(-1.08f, 1.28f, z),
                    new Vector3(0.08f, 0.58f, 0.72f), glass);
                CreatePart(root.transform, "BusWindowR_" + i, new Vector3(1.08f, 1.28f, z),
                    new Vector3(0.08f, 0.58f, 0.72f), glass);
            }

            CreatePart(root.transform, "BusDoorFrame", new Vector3(-1.1f, 0.92f, 0.55f),
                new Vector3(0.12f, 1.05f, 0.82f), trim);
            CreatePart(root.transform, "BusDoorGlass", new Vector3(-1.12f, 1.18f, 0.55f),
                new Vector3(0.06f, 0.72f, 0.68f), glass);
            CreatePart(root.transform, "BusHeadlightL", new Vector3(-0.62f, 0.62f, 2.48f),
                new Vector3(0.18f, 0.16f, 0.1f), headlight);
            CreatePart(root.transform, "BusHeadlightR", new Vector3(0.62f, 0.62f, 2.48f),
                new Vector3(0.18f, 0.16f, 0.1f), headlight);
            CreatePart(root.transform, "BusBumper", new Vector3(0f, 0.42f, 2.52f),
                new Vector3(2.05f, 0.18f, 0.14f), trim);

            Vector3[] wheelOffsets =
            {
                new Vector3(-0.95f, 0.28f, 1.55f), new Vector3(0.95f, 0.28f, 1.55f),
                new Vector3(-0.95f, 0.28f, -1.55f), new Vector3(0.95f, 0.28f, -1.55f)
            };
            for (int i = 0; i < wheelOffsets.Length; i++)
            {
                CreateWheel(root.transform, "BusWheel_" + i, wheelOffsets[i], wheel);
            }

            BoxCollider blocker = root.AddComponent<BoxCollider>();
            blocker.center = new Vector3(0f, 1.05f, 0f);
            blocker.size = new Vector3(2.25f, 2.15f, 5.05f);

            return root;
        }

        private static void CreateWheel(Transform parent, string name, Vector3 localPos, Material material)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = localPos;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.52f, 0.14f, 0.52f);
            ApplyMaterial(wheel, material);
        }

        private static GameObject CreatePart(Transform parent, string name, Vector3 localPos, Vector3 scale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = scale;
            ApplyMaterial(part, material);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static void ApplyMaterial(GameObject target, Material material)
        {
            if (target == null || material == null)
            {
                return;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material LoadMaterial(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                ChoTet.Runtime.ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }
    }
}
#endif
