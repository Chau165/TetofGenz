using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Father and mother waiting on the shoulder at the bus stop before the shuttle arrives.</summary>
    internal static class HometownArrivalParentsWaiting
    {
        public const string RootName = "ParentsWaiting";
        private const string FatherName = "Father";
        private const string MotherName = "Mother";
        private const string VisualChildName = "CharacterVisual";
        private const float FatherScale = 0.96f;
        private const float MotherScale = 0.94f;

        private const string PackColorMaterialPath = "Assets/ithappy/Creative_Characters_FREE/Materials/Color.mat";

        /// <summary>Lateral offset from stopped bus center onto the left shoulder/sidewalk.</summary>
        private const float ShoulderOffsetFromBusCenter = 2.05f;
        private const float AlongStopSpacing = 0.85f;
        private const float FootClearance = 0.08f;
        private const float GreetingStandoffBehindParents = 1.1f;
        /// <summary>Further into the shoulder than the parents row (away from the road).</summary>
        private const float GreetingDeepIntoSidewalk = 0.95f;

        public static float ShoulderSidewalkX =>
            HometownArrivalLayout.ParkedRoadsideX - ShoulderOffsetFromBusCenter;

        public static Transform EnsureWaiting(Transform worldRoot)
        {
            Transform root = GameObject.Find(RootName)?.transform;
            if (root == null)
            {
                GameObject host = new GameObject(RootName);
                if (worldRoot != null)
                {
                    host.transform.SetParent(worldRoot, false);
                }

                root = host.transform;
            }

            DetachFromVehicles(root);
            EnsureParent(root, FatherName, parentIndex: 0);
            EnsureParent(root, MotherName, parentIndex: 1);
            HometownArrivalShoulderBarrier.Remove();
            return root;
        }

        /// <summary>Fixed sidewalk pose at the bus stop — never tracks the moving shuttle.</summary>
        public static bool TryGetShuttleBusWaitingPose(int parentIndex, out Vector3 position, out Quaternion rotation)
        {
            Vector3 busAtStop = new Vector3(
                HometownArrivalLayout.ParkedRoadsideX,
                HometownArrivalLayout.BusGroundY,
                HometownArrivalLayout.BusStopZ);

            float shoulderX = busAtStop.x - ShoulderOffsetFromBusCenter;
            float alongZ = parentIndex == 0 ? -AlongStopSpacing : AlongStopSpacing;
            position = new Vector3(
                shoulderX,
                HometownArrivalLayout.BusGroundY + FootClearance,
                busAtStop.z + alongZ);

            Vector3 faceBus = busAtStop - position;
            faceBus.y = 0f;
            rotation = faceBus.sqrMagnitude > 0.02f
                ? Quaternion.LookRotation(faceBus.normalized, Vector3.up)
                : Quaternion.LookRotation(Vector3.forward, Vector3.up);
            return true;
        }

        /// <summary>Point behind parents on the sidewalk where the child stops for the reunion beat.</summary>
        public static bool TryGetFamilyGreetingPoint(out Vector3 position)
        {
            position = default;
            Transform root = GameObject.Find(RootName)?.transform;
            if (root == null)
            {
                return false;
            }

            Transform father = root.Find(FatherName);
            Transform mother = root.Find(MotherName);
            if (father == null && mother == null)
            {
                return false;
            }

            Vector3 center = Vector3.zero;
            int count = 0;
            if (father != null)
            {
                center += father.position;
                count++;
            }

            if (mother != null)
            {
                center += mother.position;
                count++;
            }

            center /= Mathf.Max(1, count);

            Vector3 busAtStop = new Vector3(
                HometownArrivalLayout.ParkedRoadsideX,
                HometownArrivalLayout.BusGroundY,
                HometownArrivalLayout.BusStopZ);
            Vector3 towardBus = busAtStop - center;
            towardBus.y = 0f;
            if (towardBus.sqrMagnitude < 0.02f)
            {
                towardBus = Vector3.right;
            }

            Vector3 behindParents = center - towardBus.normalized * GreetingStandoffBehindParents;
            behindParents.x = ShoulderSidewalkX - GreetingDeepIntoSidewalk;
            behindParents.y = HometownArrivalLayout.BusGroundY + FootClearance;
            position = behindParents;
            return true;
        }

        /// <summary>Route: clear the bus, step onto the shoulder, then walk behind mom and dad.</summary>
        public static bool TryBuildWalkPathToGreeting(Transform bus, Vector3 start, out Vector3[] waypoints)
        {
            waypoints = null;
            if (!TryGetFamilyGreetingPoint(out Vector3 greetingPoint)
                || !TryGetParentsCenter(out Vector3 parentsCenter))
            {
                return false;
            }

            Vector3 busPos = bus != null
                ? bus.position
                : new Vector3(
                    HometownArrivalLayout.ParkedRoadsideX,
                    HometownArrivalLayout.BusGroundY,
                    HometownArrivalLayout.BusStopZ);
            Vector3 busForward = bus != null ? bus.forward : Vector3.back;
            if (busForward.sqrMagnitude < 0.01f)
            {
                busForward = Vector3.back;
            }
            else
            {
                busForward.y = 0f;
                busForward.Normalize();
            }

            float y = greetingPoint.y;

            // Past the rear of the bus along the road (avoid cutting through the vehicle).
            Vector3 pastRear = busPos - busForward * 3.6f;
            pastRear.y = y;
            pastRear.x = Mathf.Lerp(start.x, busPos.x + 2.4f, 0.7f);

            // Step onto the shoulder behind the bus.
            Vector3 onShoulder = new Vector3(ShoulderSidewalkX + 0.2f, y, pastRear.z);

            // Walk along the parents' row on the sidewalk, then step behind them (deeper on the grass).
            Vector3 alongParentsRow = new Vector3(ShoulderSidewalkX + 0.1f, y, parentsCenter.z);
            Vector3 behindParents = greetingPoint;

            waypoints = new[] { pastRear, onShoulder, alongParentsRow, behindParents };
            return true;
        }

        public static bool TryGetParentsCenter(out Vector3 center)
        {
            center = default;
            Transform root = GameObject.Find(RootName)?.transform;
            if (root == null)
            {
                return false;
            }

            Transform father = root.Find(FatherName);
            Transform mother = root.Find(MotherName);
            if (father == null && mother == null)
            {
                return false;
            }

            center = Vector3.zero;
            int count = 0;
            if (father != null)
            {
                center += father.position;
                count++;
            }

            if (mother != null)
            {
                center += mother.position;
                count++;
            }

            center /= Mathf.Max(1, count);
            return true;
        }

        private static void DetachFromVehicles(Transform waitingRoot)
        {
            string[] vehicleNames = { HometownArrivalFamilyCar.RootName, "PickupBus" };
            for (int v = 0; v < vehicleNames.Length; v++)
            {
                Transform vehicle = GameObject.Find(vehicleNames[v])?.transform;
                if (vehicle == null)
                {
                    continue;
                }

                DetachNamedChild(vehicle, FatherName, waitingRoot);
                DetachNamedChild(vehicle, MotherName, waitingRoot);
            }
        }

        private static void DetachNamedChild(Transform vehicle, string childName, Transform waitingRoot)
        {
            Transform child = vehicle.Find(childName);
            if (child != null && waitingRoot != null)
            {
                child.SetParent(waitingRoot, true);
            }
        }

        private static void EnsureParent(Transform waitingRoot, string parentName, int parentIndex)
        {
            Transform existing = waitingRoot.Find(parentName);
            if (existing == null)
            {
                GameObject slot = new GameObject(parentName);
                slot.transform.SetParent(waitingRoot, false);
                existing = slot.transform;
            }
            else if (existing.parent != waitingRoot)
            {
                existing.SetParent(waitingRoot, true);
            }

            if (!TryGetShuttleBusWaitingPose(parentIndex, out Vector3 position, out Quaternion rotation))
            {
                return;
            }

            existing.SetPositionAndRotation(position, rotation);

            if (existing.Find(VisualChildName) == null)
            {
                ApplyVisual(existing, parentIndex);
            }
        }

        private static void ApplyVisual(Transform parentRoot, int parentIndex)
        {
            GameObject prefab = ResolveCharacterPrefab(parentIndex);
            if (prefab == null)
            {
                Debug.LogWarning(
                    "[HometownArrival] Parent character prefab unavailable for index "
                    + parentIndex
                    + ". Run Tools/ChoTet/Setup Parent Characters (Father + Mother).");
                return;
            }

            GameObject instance = Object.Instantiate(prefab, parentRoot);
            instance.name = VisualChildName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(
                0f,
                PlayerCharacterVisualPaths.CreativeCharacterModelYaw,
                0f);
            instance.transform.localScale = Vector3.one * (parentIndex == 0 ? FatherScale : MotherScale);

            EnsurePackColorMaterials(instance, parentIndex);

            RuntimeAnimatorController controller = ResolveAnimatorController();
            Animator animator = instance.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                }

                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.SetFloat("MoveAmount", 0f);
                animator.SetBool("IsRunning", false);
            }

            StripPhysics(instance);
        }

        private static void EnsurePackColorMaterials(GameObject root, int parentIndex)
        {
            Material colorMat = HometownArrivalResourceCache.LoadMaterialAtPath(PackColorMaterialPath);
            Material pinkShirt = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.MotherPinkShirtMaterial);
            Material jeanPants = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.MotherJeanPantsMaterial);

            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (!IsBrokenMaterial(materials[materialIndex]))
                    {
                        continue;
                    }

                    Material replacement = ResolveReplacementMaterial(
                        renderer.name,
                        parentIndex,
                        colorMat,
                        pinkShirt,
                        jeanPants);
                    if (replacement == null)
                    {
                        continue;
                    }

                    materials[materialIndex] = replacement;
                    changed = true;
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private static Material ResolveReplacementMaterial(
            string slotName,
            int parentIndex,
            Material colorMat,
            Material pinkShirt,
            Material jeanPants)
        {
            if (parentIndex == 1
                && slotName.StartsWith("Outerwear", System.StringComparison.Ordinal)
                && pinkShirt != null)
            {
                return pinkShirt;
            }

            if (parentIndex == 1
                && slotName.StartsWith("Pants", System.StringComparison.Ordinal)
                && jeanPants != null)
            {
                return jeanPants;
            }

            return colorMat;
        }

        private static bool IsBrokenMaterial(Material material)
        {
            if (material == null)
            {
                return true;
            }

            if (material.shader == null
                || !material.shader.isSupported
                || material.shader.name.Contains("Hidden/InternalErrorShader"))
            {
                return true;
            }

            string shaderName = material.shader.name;
            return shaderName == "Hidden/InternalErrorShader";
        }

        private static GameObject ResolveCharacterPrefab(int parentIndex)
        {
            string dedicatedPath = parentIndex == 0
                ? PlayerCharacterVisualPaths.FatherVisualPrefab
                : PlayerCharacterVisualPaths.MotherVisualPrefab;

            GameObject dedicated = HometownArrivalResourceCache.LoadPrefab(dedicatedPath);
            if (dedicated != null)
            {
                return dedicated;
            }

            Debug.LogWarning(
                "[HometownArrival] Missing "
                + dedicatedPath
                + " — run Tools/ChoTet/Setup Parent Characters (Father + Mother).");

            return null;
        }

        private static RuntimeAnimatorController ResolveAnimatorController()
        {
            RuntimeAnimatorController creative = HometownArrivalResourceCache.LoadAnimatorController(
                PlayerCharacterVisualPaths.CreativeCharacterTopDownController);
            if (creative != null)
            {
                return creative;
            }

            return HometownArrivalResourceCache.LoadAnimatorController(
                PlayerCharacterVisualPaths.LegacyAnimatorController);
        }

        private static void StripPhysics(GameObject root)
        {
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Object.Destroy(bodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.Destroy(colliders[i]);
            }
        }
    }
}
