using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Sparse Vietnamese expressway props — km markers, signs, reflective poles.</summary>
    public class HighwayCountrysidePropsSpawner : MonoBehaviour
    {
        private enum PropKind
        {
            KilometerMarker,
            EmergencyPhone,
            DirectionSign,
            ReflectivePole,
            SpeedLimit,
            ExitSign
        }

        private struct PropSlot
        {
            public Transform Root;
            public float BaseZ;
            public PropKind Kind;
            public bool RightSide;
        }

        [SerializeField] private int propCount = 10;
        [SerializeField] private float minSpacing = 165f;
        [SerializeField] private float spawnClearDistance = 80f;
        [SerializeField] private Material signMaterial;
        [SerializeField] private Material poleMaterial;

        private readonly List<PropSlot> props = new List<PropSlot>();
        private float scrollOffset;

        private static readonly PropKind[] Cycle =
        {
            PropKind.KilometerMarker,
            PropKind.ReflectivePole,
            PropKind.DirectionSign,
            PropKind.ReflectivePole,
            PropKind.SpeedLimit,
            PropKind.EmergencyPhone,
            PropKind.ReflectivePole,
            PropKind.ExitSign
        };

        private void Awake()
        {
            EnsureMaterials();
        }

        public void Initialize(Material sign, Material pole)
        {
            signMaterial = sign;
            poleMaterial = pole;
            EnsureMaterials();
            RebuildProps();
        }

        public void RebuildProps()
        {
            ClearProps();
            BuildProps();
            RepositionProps();
        }

        public void SetScrollOffset(float distance)
        {
            if (props.Count == 0)
            {
                BuildProps();
            }

            scrollOffset = distance;
            RepositionProps();
        }

        private void EnsureMaterials()
        {
#if UNITY_EDITOR
            if (signMaterial == null)
            {
                signMaterial = HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.LaneMarkFile);
            }

            if (poleMaterial == null)
            {
                poleMaterial = HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.GuardRailFile);
            }
#endif
        }

        private void BuildProps()
        {
            ClearProps();
            float z = spawnClearDistance + minSpacing * 0.4f;

            for (int i = 0; i < propCount; i++)
            {
                bool rightSide = i % 2 == 0;
                PropKind kind = Cycle[i % Cycle.Length];
                if (kind == PropKind.ExitSign && i % 4 != 0)
                {
                    kind = PropKind.ReflectivePole;
                }

                GameObject root = new GameObject("HighwayProp_" + i);
                root.transform.SetParent(transform, false);
                BuildPropVisual(root.transform, kind, i);

                props.Add(new PropSlot
                {
                    Root = root.transform,
                    BaseZ = z,
                    Kind = kind,
                    RightSide = rightSide
                });

                z += minSpacing * (0.88f + (i % 5) * 0.04f);
            }

            RepositionProps();
        }

        private void BuildPropVisual(Transform root, PropKind kind, int index)
        {
            HighwayPropUtility.ClearChildren(root);
            float signW = 0.9f;
            float signH = 0.55f;
            float poleH = 2.1f;

            switch (kind)
            {
                case PropKind.KilometerMarker:
                    BuildSignBoard(root, signMaterial, signW * 0.7f, signH * 0.5f, poleH * 0.85f, new Color(0.22f, 0.48f, 0.28f));
                    break;
                case PropKind.EmergencyPhone:
                    BuildSignBoard(root, signMaterial, signW, signH, poleH, new Color(0.15f, 0.42f, 0.78f));
                    break;
                case PropKind.DirectionSign:
                    BuildSignBoard(root, signMaterial, signW * 1.4f, signH, poleH * 1.05f, new Color(0.2f, 0.35f, 0.72f));
                    break;
                case PropKind.SpeedLimit:
                    BuildSignBoard(root, signMaterial, signW * 0.65f, signH * 0.9f, poleH, Color.white);
                    break;
                case PropKind.ExitSign:
                    BuildSignBoard(root, signMaterial, signW * 1.2f, signH * 0.85f, poleH, new Color(0.18f, 0.55f, 0.28f));
                    break;
                default:
                    BuildReflectivePole(root, poleMaterial, poleH * 0.75f);
                    break;
            }
        }

        private static void BuildSignBoard(
            Transform root,
            Material signMat,
            float width,
            float height,
            float poleHeight,
            Color boardTint)
        {
            BuildPole(root, poleHeight);
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "SignBoard";
            board.transform.SetParent(root, false);
            board.transform.localPosition = new Vector3(0f, poleHeight + height * 0.5f, 0f);
            board.transform.localScale = new Vector3(width, height, 0.06f);
            HighwayPropUtility.SafeDestroy(board.GetComponent<Collider>());
            ApplyMaterial(board, signMat, boardTint);
        }

        private static void BuildReflectivePole(Transform root, Material poleMat, float height)
        {
            BuildPole(root, height);
            GameObject reflector = GameObject.CreatePrimitive(PrimitiveType.Cube);
            reflector.name = "Reflector";
            reflector.transform.SetParent(root, false);
            reflector.transform.localPosition = new Vector3(0f, height, 0f);
            reflector.transform.localScale = new Vector3(0.14f, 0.14f, 0.04f);
            HighwayPropUtility.SafeDestroy(reflector.GetComponent<Collider>());
            ApplyMaterial(reflector, poleMat, new Color(0.95f, 0.92f, 0.55f));
        }

        private static void BuildPole(Transform root, float height)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(root, false);
            pole.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            pole.transform.localScale = new Vector3(0.08f, height * 0.5f, 0.08f);
            HighwayPropUtility.SafeDestroy(pole.GetComponent<Collider>());
            ApplyMaterial(pole, null, new Color(0.62f, 0.64f, 0.66f));
        }

        private static void ApplyMaterial(GameObject go, Material source, Color tint)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (source != null)
            {
                renderer.sharedMaterial = source;
                if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                {
                    renderer.sharedMaterial = new Material(renderer.sharedMaterial);
                    renderer.sharedMaterial.SetColor("_BaseColor", tint);
                }
            }
            else
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = tint };
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void RepositionProps()
        {
            float recycleBehind = minSpacing * 0.5f;
            float maxBaseZ = 0f;
            for (int i = 0; i < props.Count; i++)
            {
                maxBaseZ = Mathf.Max(maxBaseZ, props[i].BaseZ);
            }

            for (int i = 0; i < props.Count; i++)
            {
                PropSlot slot = props[i];
                float z = slot.BaseZ - scrollOffset;

                while (z < -recycleBehind)
                {
                    slot.BaseZ += maxBaseZ + minSpacing;
                    slot.Kind = Cycle[Mathf.Abs(Mathf.RoundToInt(slot.BaseZ)) % Cycle.Length];
                    slot.RightSide = !slot.RightSide;
                    BuildPropVisual(slot.Root, slot.Kind, Mathf.RoundToInt(slot.BaseZ));
                    z = slot.BaseZ - scrollOffset;
                }

                float x = (slot.RightSide ? 1f : -1f) * (HighwayRoadLayout.ShoulderOuterX + 1.1f);
                slot.Root.localPosition = new Vector3(x, HighwayLaneSettings.ShoulderSurfaceY, z);
                props[i] = slot;
            }
        }

        private void ClearProps()
        {
            for (int i = 0; i < props.Count; i++)
            {
                if (props[i].Root != null)
                {
                    HighwayPropUtility.SafeDestroy(props[i].Root.gameObject);
                }
            }

            props.Clear();
        }
    }
}
