#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Detects and resolves overlapping outdoor props inside <c>Level1_OutdoorArea</c>.
    /// </summary>
    public class SceneOverlapCleaner : EditorWindow
    {
        private const string OutdoorRootName = "Level1_OutdoorArea";
        private const float PathHalfWidth = 2.65f;
        private const float PlayableMinX = Level1OutdoorAreaBuilder.OutdoorMinX;
        private const float PlayableMaxX = Level1OutdoorAreaBuilder.OutdoorMaxX;
        private const float PlayableMinZ = Level1OutdoorAreaBuilder.OutdoorMinZ;
        private const float PlayableMaxZ = Level1OutdoorAreaBuilder.OutdoorMaxZ;
        private const float VisualMargin = 1.25f;

        private static readonly List<OverlapPair> RemainingOverlaps = new List<OverlapPair>();

        private bool dryRun;
        private float overlapThreshold = 0.16f;
        private float duplicateDistance = 0.24f;
        private int maxPasses = 4;
        private Vector2 scroll;

        public static void OpenWindow()
        {
            GetWindow<SceneOverlapCleaner>("Outdoor Overlap Cleaner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Level 1 Outdoor Overlap Cleaner", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Scans props under Level1_OutdoorArea only. Indoor room, player, camera, UI, lights, " +
                "spawn points, triggers, and ground slabs are ignored.",
                MessageType.Info);

            dryRun = EditorGUILayout.Toggle("Dry Run (log only, no changes)", dryRun);
            overlapThreshold = EditorGUILayout.Slider("Overlap Threshold", overlapThreshold, 0.05f, 0.45f);
            duplicateDistance = EditorGUILayout.Slider("Duplicate Distance", duplicateDistance, 0.08f, 0.5f);
            maxPasses = EditorGUILayout.IntSlider("Max Separation Passes", maxPasses, 1, 8);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button(dryRun ? "Run Dry Scan" : "Clean Outdoor Overlaps", GUILayout.Height(32f)))
            {
                CleanSummary summary = RunClean(dryRun, overlapThreshold, duplicateDistance, maxPasses);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.TextArea(summary.ToReport(), GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            if (RemainingOverlaps.Count > 0)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox(
                    RemainingOverlaps.Count + " overlap(s) still flagged — see Console and Scene view gizmos (yellow).",
                    MessageType.Warning);
            }
        }

        [InitializeOnLoadMethod]
        private static void RegisterGizmos()
        {
            SceneView.duringSceneGui -= DrawRemainingOverlapGizmos;
            SceneView.duringSceneGui += DrawRemainingOverlapGizmos;
        }

        private static void DrawRemainingOverlapGizmos(SceneView view)
        {
            if (RemainingOverlaps.Count == 0)
            {
                return;
            }

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            for (int i = 0; i < RemainingOverlaps.Count; i++)
            {
                OverlapPair pair = RemainingOverlaps[i];
                if (pair.A == null || pair.B == null)
                {
                    continue;
                }

                Bounds intersection = IntersectBounds(pair.BoundsA, pair.BoundsB);
                Handles.color = new Color(1f, 0.85f, 0.1f, 0.35f);
                Handles.DrawWireCube(intersection.center, intersection.size);
                Handles.color = new Color(1f, 0.45f, 0.1f, 0.9f);
                Handles.DrawLine(pair.BoundsA.center, pair.BoundsB.center);
            }
        }

        public static CleanSummary RunClean(bool dryRunMode, float threshold, float dupDistance, int passes)
        {
            RemainingOverlaps.Clear();
            CleanSummary summary = new CleanSummary { DryRun = dryRunMode };

            if (EditorApplication.isPlaying)
            {
                summary.ErrorMessage = "Exit Play mode before running the outdoor overlap cleaner.";
                Debug.LogWarning("[SceneOverlapCleaner] " + summary.ErrorMessage);
                return summary;
            }

            Transform outdoorRoot = FindOutdoorRoot();
            if (outdoorRoot == null)
            {
                summary.ErrorMessage = OutdoorRootName + " not found in open scene(s).";
                Debug.LogWarning("[SceneOverlapCleaner] " + summary.ErrorMessage);
                return summary;
            }

            List<ScannableProp> props = CollectScannableProps(outdoorRoot, summary);
            summary.TotalScanned = props.Count;

            RemoveExactDuplicates(props, dupDistance, dryRunMode, summary);

            for (int pass = 0; pass < passes; pass++)
            {
                int movedThisPass = SeparateOverlaps(props, threshold, dryRunMode, summary, pass + 1);
                summary.PassesRun = pass + 1;
                if (movedThisPass == 0)
                {
                    break;
                }

                RefreshAllBounds(props);
            }

            FlagRemainingOverlaps(props, threshold, summary);
            MarkSceneDirty(outdoorRoot);

            Debug.Log(summary.ToReport());
            return summary;
        }

        private static Transform FindOutdoorRoot()
        {
            GameObject found = GameObject.Find(OutdoorRootName);
            if (found != null)
            {
                return found.transform;
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    Transform nested = FindDeep(roots[r].transform, OutdoorRootName);
                    if (nested != null)
                    {
                        return nested;
                    }
                }
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeep(root.GetChild(i), name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static List<ScannableProp> CollectScannableProps(Transform outdoorRoot, CleanSummary summary)
        {
            List<ScannableProp> props = new List<ScannableProp>(256);
            Transform[] all = outdoorRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t == outdoorRoot || ShouldIgnore(t.gameObject) || IsUnderIgnoredGroup(t))
                {
                    continue;
                }

                if (!TryGetBounds(t.gameObject, out Bounds bounds))
                {
                    continue;
                }

                if (bounds.size.sqrMagnitude < 0.0001f)
                {
                    continue;
                }

                props.Add(new ScannableProp
                {
                    Transform = t,
                    GameObject = t.gameObject,
                    Bounds = bounds,
                    Volume = bounds.size.x * bounds.size.y * bounds.size.z,
                    IsFixed = IsFixedProp(t.gameObject, bounds)
                });
            }

            return props;
        }

        private static bool IsUnderIgnoredGroup(Transform t)
        {
            while (t != null)
            {
                string parentName = t.name;
                if (parentName == "Level1_OutdoorPathBoundaries" || parentName == "Ground"
                    || parentName == "OutdoorLighting")
                {
                    return true;
                }

                t = t.parent;
            }

            return false;
        }

        private static bool ShouldIgnore(GameObject go)
        {
            string name = go.name;
            if (name.StartsWith("Outdoor") && (name.Contains("Ground") || name.Contains("Grass") || name.Contains("Garden")
                || name.Contains("Roadside") || name.Contains("TerrainLip") || name.Contains("WalkCollider")))
            {
                return true;
            }

            if (name.Contains("PathBoundaries") || name.Contains("PathPiece") || name.Contains("WalkPath"))
            {
                return name.Contains("Wall") || name.Contains("Blocker") || name.Contains("Collider");
            }

            string lower = name.ToLowerInvariant();
            if (lower.Contains("spawn") || lower.Contains("light") || lower.Contains("sun")
                || lower.Contains("camera") || lower.Contains("canvas") || lower.Contains("trigger")
                || lower.Contains("collider") && !go.GetComponent<Renderer>())
            {
                return true;
            }

            if (go.CompareTag("MainCamera") || go.CompareTag("Player"))
            {
                return true;
            }

            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                string typeName = components[i].GetType().Name;
                if (typeName.Contains("Light") || typeName.Contains("Canvas") || typeName.Contains("Camera"))
                {
                    return true;
                }
            }

            return go.GetComponent<Renderer>() == null && go.GetComponent<Collider>() == null;
        }

        private static bool IsFixedProp(GameObject go, Bounds bounds)
        {
            string name = go.name;
            if (name.StartsWith("BorderTree") || name.StartsWith("PerimeterTree") || name.StartsWith("EdgeCliff")
                || name.StartsWith("BackdropHill") || name.StartsWith("HouseBack") || name.StartsWith("HouseSide")
                || name.StartsWith("HouseRoof") || name.StartsWith("HouseDoor") || name.StartsWith("BackRock")
                || name.StartsWith("BackFence") || name.StartsWith("PathPiece") || name.StartsWith("ExitPathStone"))
            {
                return true;
            }

            float footprint = bounds.size.x * bounds.size.z;
            return footprint > 14f || bounds.size.y > 2.6f;
        }

        private static bool TryGetBounds(GameObject go, out Bounds bounds)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && renderer.enabled)
            {
                bounds = renderer.bounds;
                return true;
            }

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].enabled)
                    {
                        bounds.Encapsulate(renderers[i].bounds);
                    }
                }

                return true;
            }

            Collider collider = go.GetComponent<Collider>();
            if (collider != null && collider.enabled)
            {
                bounds = collider.bounds;
                return true;
            }

            bounds = default;
            return false;
        }

        private static void RemoveExactDuplicates(List<ScannableProp> props, float distance, bool dryRun,
            CleanSummary summary)
        {
            for (int i = 0; i < props.Count; i++)
            {
                for (int j = props.Count - 1; j > i; j--)
                {
                    ScannableProp a = props[i];
                    ScannableProp b = props[j];
                    if (a.GameObject == null || b.GameObject == null)
                    {
                        continue;
                    }

                    if (!AreDuplicateCandidates(a, b, distance))
                    {
                        continue;
                    }

                    summary.DuplicatesFound++;
                    ScannableProp remove = a.Volume <= b.Volume ? a : b;
                    summary.DuplicateLogs.Add(remove.GameObject.name + " @ " + remove.Position);

                    if (!dryRun)
                    {
                        Undo.DestroyObjectImmediate(remove.GameObject);
                    }

                    props.Remove(remove);
                    if (remove == a)
                    {
                        i--;
                        break;
                    }
                }
            }
        }

        private static bool AreDuplicateCandidates(ScannableProp a, ScannableProp b, float distance)
        {
            if (a.GameObject.name != b.GameObject.name)
            {
                return false;
            }

            Vector3 delta = a.Position - b.Position;
            delta.y *= 0.35f;
            if (delta.sqrMagnitude > distance * distance)
            {
                return false;
            }

            return Mathf.Abs(a.Volume - b.Volume) < Mathf.Max(0.15f, a.Volume * 0.2f);
        }

        private static int SeparateOverlaps(List<ScannableProp> props, float threshold, bool dryRun,
            CleanSummary summary, int passIndex)
        {
            int movedCount = 0;
            RefreshAllBounds(props);

            for (int i = 0; i < props.Count; i++)
            {
                for (int j = i + 1; j < props.Count; j++)
                {
                    ScannableProp a = props[i];
                    ScannableProp b = props[j];
                    if (!TryGetOverlapRatio(a.Bounds, b.Bounds, out float ratio))
                    {
                        continue;
                    }

                    if (ratio < threshold)
                    {
                        continue;
                    }

                    summary.OverlapsFound++;
                    ScannableProp mover = ChooseMover(a, b);
                    ScannableProp anchor = mover == a ? b : a;
                    if (mover.IsFixed && anchor.IsFixed)
                    {
                        summary.FixedOverlapLogs.Add(a.GameObject.name + " ↔ " + b.GameObject.name);
                        continue;
                    }

                    Vector3 newPos = ComputeSeparatedPosition(mover, anchor);
                    if (Vector3.SqrMagnitude(newPos - mover.Position) < 0.0004f)
                    {
                        continue;
                    }

                    summary.MoveLogs.Add(
                        "[pass " + passIndex + "] " + mover.GameObject.name + ": " + mover.Position + " → " +
                        newPos);

                    if (!dryRun)
                    {
                        Undo.RecordObject(mover.Transform, "Clean Outdoor Overlaps");
                        mover.Transform.position = newPos;
                        mover.SimulatedPosition = null;
                    }
                    else
                    {
                        mover.SimulatedPosition = newPos;
                    }

                    RefreshBounds(mover);

                    movedCount++;
                    summary.ObjectsMoved++;
                }
            }

            return movedCount;
        }

        private static ScannableProp ChooseMover(ScannableProp a, ScannableProp b)
        {
            if (a.IsFixed != b.IsFixed)
            {
                return a.IsFixed ? b : a;
            }

            return a.Volume <= b.Volume ? a : b;
        }

        private static Vector3 ComputeSeparatedPosition(ScannableProp mover, ScannableProp anchor)
        {
            Vector3 moverCenter = Flatten(mover.Bounds.center);
            Vector3 anchorCenter = Flatten(anchor.Bounds.center);
            Vector3 pushDir = moverCenter - anchorCenter;
            if (pushDir.sqrMagnitude < 0.001f)
            {
                pushDir = new Vector3(mover.Position.x - anchor.Position.x, 0f,
                    mover.Position.z - anchor.Position.z);
            }

            if (pushDir.sqrMagnitude < 0.001f)
            {
                pushDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }

            pushDir.Normalize();

            float requiredSeparation = HorizontalSeparationNeeded(mover.Bounds, anchor.Bounds) + 0.18f;
            Vector3 target = mover.Position + pushDir * requiredSeparation;
            target = ClampToOutdoorBounds(target, mover.Bounds.extents);
            target = PushOutOfPlayerPath(target, mover.Bounds.extents);
            target.y = SnapToGroundY(target, mover);
            return target;
        }

        private static float HorizontalSeparationNeeded(Bounds a, Bounds b)
        {
            Vector2 delta = new Vector2(b.center.x - a.center.x, b.center.z - a.center.z);
            float neededX = (a.extents.x + b.extents.x) - Mathf.Abs(delta.x);
            float neededZ = (a.extents.z + b.extents.z) - Mathf.Abs(delta.y);
            return Mathf.Max(neededX, neededZ, 0.05f);
        }

        private static Vector3 ClampToOutdoorBounds(Vector3 position, Vector3 halfExtents)
        {
            float minX = PlayableMinX - VisualMargin + halfExtents.x;
            float maxX = PlayableMaxX + VisualMargin - halfExtents.x;
            float minZ = PlayableMinZ - VisualMargin + halfExtents.z;
            float maxZ = PlayableMaxZ + VisualMargin - halfExtents.z;
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.z = Mathf.Clamp(position.z, minZ, maxZ);
            return position;
        }

        private static Vector3 PushOutOfPlayerPath(Vector3 position, Vector3 halfExtents)
        {
            float minX = -PathHalfWidth - halfExtents.x;
            float maxX = PathHalfWidth + halfExtents.x;
            if (position.x <= maxX && position.x >= minX)
            {
                float pushLeft = Mathf.Abs(position.x - minX);
                float pushRight = Mathf.Abs(maxX - position.x);
                position.x = pushLeft <= pushRight
                    ? minX - 0.12f - halfExtents.x * 0.25f
                    : maxX + 0.12f + halfExtents.x * 0.25f;
            }

            return position;
        }

        private static float SnapToGroundY(Vector3 position, ScannableProp mover)
        {
            Vector3 rayOrigin = new Vector3(position.x, 8f, position.z);
            float groundY = 0f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                groundY = hit.point.y;
            }

            float pivotToBottom = mover.Position.y - mover.Bounds.min.y;
            return groundY + pivotToBottom;
        }

        private static void FlagRemainingOverlaps(List<ScannableProp> props, float threshold, CleanSummary summary)
        {
            RemainingOverlaps.Clear();
            RefreshAllBounds(props);

            for (int i = 0; i < props.Count; i++)
            {
                for (int j = i + 1; j < props.Count; j++)
                {
                    if (!TryGetOverlapRatio(props[i].Bounds, props[j].Bounds, out float ratio) || ratio < threshold)
                    {
                        continue;
                    }

                    OverlapPair pair = new OverlapPair
                    {
                        A = props[i].GameObject,
                        B = props[j].GameObject,
                        BoundsA = props[i].Bounds,
                        BoundsB = props[j].Bounds,
                        Ratio = ratio
                    };
                    RemainingOverlaps.Add(pair);
                    summary.RemainingOverlapLogs.Add(
                        props[i].GameObject.name + " ↔ " + props[j].GameObject.name + " (" + ratio.ToString("P0") + ")");
                }
            }

            summary.RemainingOverlaps = RemainingOverlaps.Count;
        }

        private static bool TryGetOverlapRatio(Bounds a, Bounds b, out float ratio)
        {
            ratio = 0f;
            if (!BoundsIntersectXZ(a, b))
            {
                return false;
            }

            float overlapX = Mathf.Max(0f,
                Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
            float overlapZ = Mathf.Max(0f,
                Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z));
            float overlapArea = overlapX * overlapZ;
            if (overlapArea <= 0.0001f)
            {
                return false;
            }

            float areaA = Mathf.Max(0.001f, a.size.x * a.size.z);
            float areaB = Mathf.Max(0.001f, b.size.x * b.size.z);
            ratio = overlapArea / Mathf.Min(areaA, areaB);
            return ratio > 0f;
        }

        private static bool BoundsIntersectXZ(Bounds a, Bounds b)
        {
            return a.min.x <= b.max.x && a.max.x >= b.min.x && a.min.z <= b.max.z && a.max.z >= b.min.z;
        }

        private static Bounds IntersectBounds(Bounds a, Bounds b)
        {
            Vector3 min = Vector3.Max(a.min, b.min);
            Vector3 max = Vector3.Min(a.max, b.max);
            Bounds result = new Bounds();
            result.SetMinMax(min, max);
            return result;
        }

        private static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        private static void RefreshAllBounds(List<ScannableProp> props)
        {
            for (int i = 0; i < props.Count; i++)
            {
                if (props[i].GameObject == null)
                {
                    continue;
                }

                RefreshBounds(props[i]);
            }
        }

        private static void RefreshBounds(ScannableProp prop)
        {
            if (TryGetBounds(prop.GameObject, out Bounds bounds))
            {
                if (prop.SimulatedPosition.HasValue)
                {
                    Vector3 delta = prop.SimulatedPosition.Value - prop.Transform.position;
                    bounds.center += delta;
                }

                prop.Bounds = bounds;
                prop.Volume = bounds.size.x * bounds.size.y * bounds.size.z;
            }
        }

        private static void MarkSceneDirty(Transform outdoorRoot)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(outdoorRoot.gameObject.scene);
        }

        private class ScannableProp
        {
            public Transform Transform;
            public GameObject GameObject;
            public Bounds Bounds;
            public float Volume;
            public bool IsFixed;
            public Vector3? SimulatedPosition;

            public Vector3 Position => SimulatedPosition ?? Transform.position;
        }

        private struct OverlapPair
        {
            public GameObject A;
            public GameObject B;
            public Bounds BoundsA;
            public Bounds BoundsB;
            public float Ratio;
        }

        public class CleanSummary
        {
            public bool DryRun;
            public string ErrorMessage;
            public int TotalScanned;
            public int OverlapsFound;
            public int ObjectsMoved;
            public int DuplicatesFound;
            public int DuplicatesRemoved => DuplicateLogs.Count;
            public int RemainingOverlaps;
            public int PassesRun;
            public List<string> MoveLogs = new List<string>();
            public List<string> DuplicateLogs = new List<string>();
            public List<string> FixedOverlapLogs = new List<string>();
            public List<string> RemainingOverlapLogs = new List<string>();

            public string ToReport()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[SceneOverlapCleaner] " + (DryRun ? "DRY RUN — no changes written." : "Clean complete."));
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    sb.AppendLine("Error: " + ErrorMessage);
                    return sb.ToString();
                }

                sb.AppendLine("Total scanned: " + TotalScanned);
                sb.AppendLine("Overlap pairs found: " + OverlapsFound);
                sb.AppendLine("Objects moved: " + ObjectsMoved + " (" + PassesRun + " pass(es))");
                sb.AppendLine("Duplicates found: " + DuplicatesFound);
                sb.AppendLine("Duplicates removed: " + (DryRun ? "0 (dry run)" : DuplicatesRemoved.ToString()));
                sb.AppendLine("Remaining overlaps: " + RemainingOverlaps);

                if (MoveLogs.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Moved:");
                    for (int i = 0; i < MoveLogs.Count; i++)
                    {
                        sb.AppendLine("  • " + MoveLogs[i]);
                    }
                }

                if (DuplicateLogs.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine(DryRun ? "Duplicates (would remove):" : "Duplicates removed:");
                    for (int i = 0; i < DuplicateLogs.Count; i++)
                    {
                        sb.AppendLine("  • " + DuplicateLogs[i]);
                    }
                }

                if (FixedOverlapLogs.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Fixed pairs (manual review):");
                    for (int i = 0; i < FixedOverlapLogs.Count; i++)
                    {
                        sb.AppendLine("  • " + FixedOverlapLogs[i]);
                    }
                }

                if (RemainingOverlapLogs.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Remaining overlaps:");
                    for (int i = 0; i < RemainingOverlapLogs.Count; i++)
                    {
                        sb.AppendLine("  • " + RemainingOverlapLogs[i]);
                    }
                }

                return sb.ToString();
            }
        }
    }
}
#endif
