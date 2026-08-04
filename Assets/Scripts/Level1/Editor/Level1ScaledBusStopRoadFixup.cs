#if UNITY_EDITOR
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// After manually scaling/duplicating bus-stop asphalt and walls, retile materials and
    /// stretch supporting slabs so traffic can drive the full measured road length.
    /// </summary>
    public static class Level1ScaledBusStopRoadFixup
    {
        private const float AsphaltMetersPerTile = 6f;
        private const float ShoulderMetersPerTile = 8f;
        private const float WallMetersPerTileX = 2.4f;
        private const float WallMetersPerTileY = 1.2f;

        public static void FixCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1ScaledBusStopRoadFixup] Exit Play mode before fixing the road.");
                return;
            }

            Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
            Transform layout = FindDeep(outdoor, "BusStopRoadLayout");
            if (layout == null)
            {
                Debug.LogError("[Level1ScaledBusStopRoadFixup] BusStopRoadLayout not found.");
                return;
            }

            if (!TryMeasureAsphalt(layout, out float minX, out float maxX, out float roadDepth, out float roadY))
            {
                Debug.LogError("[Level1ScaledBusStopRoadFixup] No AsphaltRoadSurface renderers found.");
                return;
            }

            float roadLength = maxX - minX;
            float roadCenterX = (minX + maxX) * 0.5f;

            int retiled = 0;
            retiled += RetileNamedPrefix(layout, "AsphaltRoadSurface", AsphaltMetersPerTile);
            retiled += RetileNamedPrefix(layout, "RoadShoulder", ShoulderMetersPerTile);
            retiled += RetileNamedPrefix(layout, "CenterLaneDivider", AsphaltMetersPerTile);

            StretchSupportingSlab(layout, "RoadShoulderNorth", roadCenterX, roadLength);
            StretchSupportingSlab(layout, "RoadShoulderSouth", roadCenterX, roadLength);
            StretchSupportingSlab(layout, "CenterLaneDivider", roadCenterX, roadLength + 0.15f);

            Transform eco = FindDeep(outdoor, Level1AcrossRoadEcoWallBuilder.RootName);
            if (eco != null)
            {
                retiled += RetileEcoWall(eco, roadCenterX, roadLength);
            }

            SyncLaneMarkings(layout, roadCenterX, roadLength);

            Level1BusStopRoadExtents.RefreshFromScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log(
                "[Level1ScaledBusStopRoadFixup] Road span "
                + minX.ToString("F1") + " → " + maxX.ToString("F1")
                + " (" + roadLength.ToString("F1") + "m). Retiled "
                + retiled + " materials. Traffic will despawn past the asphalt ends.");
        }

        private static bool TryMeasureAsphalt(
            Transform layout,
            out float minX,
            out float maxX,
            out float roadDepth,
            out float roadY)
        {
            minX = float.PositiveInfinity;
            maxX = float.NegativeInfinity;
            roadDepth = Level1BusStopRoadSettings.LaneSouthEdgeZ - Level1BusStopRoadSettings.LaneNorthEdgeZ;
            roadY = Level1BusStopRoadSettings.AsphaltSurfaceY;
            bool found = false;

            for (int i = 0; i < layout.childCount; i++)
            {
                Transform child = layout.GetChild(i);
                if (!child.name.StartsWith("AsphaltRoadSurface"))
                {
                    continue;
                }

                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;
                minX = Mathf.Min(minX, bounds.min.x);
                maxX = Mathf.Max(maxX, bounds.max.x);
                roadDepth = bounds.size.z;
                roadY = bounds.center.y;
                found = true;
            }

            return found && maxX > minX + 1f;
        }

        private static int RetileNamedPrefix(Transform layout, string namePrefix, float metersPerTile)
        {
            int count = 0;
            for (int i = 0; i < layout.childCount; i++)
            {
                Transform child = layout.GetChild(i);
                if (!child.name.StartsWith(namePrefix))
                {
                    continue;
                }

                if (ApplyWorldTiling(child.gameObject, metersPerTile, metersPerTile))
                {
                    count++;
                }
            }

            return count;
        }

        private static int RetileEcoWall(Transform ecoRoot, float roadCenterX, float roadLength)
        {
            int count = 0;
            Renderer[] renderers = ecoRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.transform.name.Contains("Banner"))
                {
                    continue;
                }

                if (ApplyWorldTiling(renderer.gameObject, WallMetersPerTileX, WallMetersPerTileY))
                {
                    count++;
                }
            }

            return count;
        }

        private static void StretchSupportingSlab(Transform layout, string exactName, float centerX, float lengthX)
        {
            Transform slab = layout.Find(exactName);
            if (slab == null)
            {
                // Prefer the primary slab when duplicates exist.
                for (int i = 0; i < layout.childCount; i++)
                {
                    if (layout.GetChild(i).name == exactName)
                    {
                        slab = layout.GetChild(i);
                        break;
                    }
                }
            }

            if (slab == null)
            {
                return;
            }

            Vector3 scale = slab.localScale;
            slab.position = new Vector3(centerX, slab.position.y, slab.position.z);
            slab.localScale = new Vector3(lengthX, scale.y, scale.z);
            float meters = exactName.Contains("Shoulder") ? ShoulderMetersPerTile : AsphaltMetersPerTile;
            ApplyWorldTiling(slab.gameObject, meters, meters);
        }

        private static void SyncLaneMarkings(Transform layout, float roadCenterX, float roadLength)
        {
            // Reuse the existing sync helper by temporarily aligning the primary asphalt scale/center.
            Transform primaryAsphalt = layout.Find("AsphaltRoadSurface");
            if (primaryAsphalt == null)
            {
                return;
            }

            Vector3 previousScale = primaryAsphalt.localScale;
            Vector3 previousPos = primaryAsphalt.position;
            primaryAsphalt.position = new Vector3(roadCenterX, previousPos.y, previousPos.z);
            primaryAsphalt.localScale = new Vector3(roadLength, previousScale.y, previousScale.z);

            Level1BusStopRoadMarkingSync.SyncOnCurrentScene();

            // Restore the author's primary asphalt piece; markings already span the full length.
            primaryAsphalt.position = previousPos;
            primaryAsphalt.localScale = previousScale;
            ApplyWorldTiling(primaryAsphalt.gameObject, AsphaltMetersPerTile, AsphaltMetersPerTile);
        }

        private static bool ApplyWorldTiling(GameObject target, float metersPerTileX, float metersPerTileY)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return false;
            }

            Material source = renderer.sharedMaterial;
            Material tiled = new Material(source);
            Vector3 size = renderer.bounds.size;
            // Prefer transform scale for axis-aligned slabs (more stable than bounds while editing).
            Vector3 lossy = target.transform.lossyScale;
            float spanX = Mathf.Abs(lossy.x) > 0.01f ? Mathf.Abs(lossy.x) : size.x;
            float spanY = Mathf.Abs(lossy.y) > 0.01f ? Mathf.Abs(lossy.y) : size.y;
            // Road slabs use X/Z; walls use X/Y.
            float tileU = Mathf.Max(1f, spanX / metersPerTileX);
            float tileV = target.transform.name.Contains("EcoWall") || target.transform.name.Contains("Wall")
                ? Mathf.Max(1f, spanY / metersPerTileY)
                : Mathf.Max(1f, Mathf.Abs(lossy.z) / metersPerTileY);

            if (tiled.HasProperty("_BaseMap"))
            {
                tiled.SetTextureScale("_BaseMap", new Vector2(tileU, tileV));
            }

            if (tiled.HasProperty("_BumpMap"))
            {
                tiled.SetTextureScale("_BumpMap", new Vector2(tileU, tileV));
            }

            if (tiled.HasProperty("_MainTex"))
            {
                tiled.SetTextureScale("_MainTex", new Vector2(tileU, tileV));
            }

            renderer.sharedMaterial = tiled;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return true;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
