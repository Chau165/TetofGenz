using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Runtime
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class CharacterOutline : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float fallbackOutlineWidth = 0.02f;
        [SerializeField] private Color fallbackOutlineColor = new Color(0.1647059f, 0.1294118f, 0.1137255f, 1f);
        [SerializeField] private string cloneSuffix = "_Outline";
        [SerializeField] private bool includeInactiveRenderers = true;

        private static Material sharedFallbackMaterial;
        private readonly List<GameObject> createdClones = new List<GameObject>();

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                ClearClones();
                ClearExistingOutlineChildren();
                return;
            }

            Rebuild();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled || !Application.isPlaying)
            {
                ClearClones();
                ClearExistingOutlineChildren();
                return;
            }

            Rebuild();
        }

        private void OnDisable()
        {
            ClearClones();
        }

        private Material ResolveMaterial()
        {
            if (outlineMaterial != null)
            {
                return outlineMaterial;
            }

            if (sharedFallbackMaterial != null)
            {
                return sharedFallbackMaterial;
            }

            Shader shader = Shader.Find("ChoTet/CollectableOutline");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            sharedFallbackMaterial = new Material(shader)
            {
                name = "CharacterOutline (Runtime)"
            };

            if (sharedFallbackMaterial.HasProperty("_OutlineWidth"))
            {
                sharedFallbackMaterial.SetFloat("_OutlineWidth", fallbackOutlineWidth);
            }

            if (sharedFallbackMaterial.HasProperty("_Color"))
            {
                sharedFallbackMaterial.SetColor("_Color", fallbackOutlineColor);
            }

            return sharedFallbackMaterial;
        }

        [ContextMenu("Rebuild Outline")]
        public void Rebuild()
        {
            ClearClones();
            ClearExistingOutlineChildren();

            Material material = ResolveMaterial();
            Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer source = renderers[i];
                if (source == null)
                {
                    continue;
                }

                if (source.name.EndsWith(cloneSuffix))
                {
                    continue;
                }

                if (source is MeshRenderer && source.GetComponent<MeshFilter>() == null)
                {
                    continue;
                }

                CreateClone(source, material);
            }
        }

        private void CreateClone(Renderer source, Material material)
        {
            if (source.transform.GetComponentInParent<Animator>() != null && source is MeshRenderer)
            {
                return;
            }

            GameObject clone = new GameObject(source.name + cloneSuffix);
            clone.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            clone.layer = source.gameObject.layer;
            clone.transform.SetParent(source.transform.parent, false);
            clone.transform.localPosition = source.transform.localPosition;
            clone.transform.localRotation = source.transform.localRotation;
            clone.transform.localScale = source.transform.localScale;

            if (source is SkinnedMeshRenderer sourceSkinned)
            {
                SkinnedMeshRenderer cloneSkinned = clone.AddComponent<SkinnedMeshRenderer>();
                cloneSkinned.sharedMesh = sourceSkinned.sharedMesh;
                cloneSkinned.bones = sourceSkinned.bones;
                cloneSkinned.rootBone = sourceSkinned.rootBone;
                cloneSkinned.updateWhenOffscreen = sourceSkinned.updateWhenOffscreen;
                cloneSkinned.quality = sourceSkinned.quality;
                cloneSkinned.sharedMaterial = material;
                ConfigureRenderer(cloneSkinned);
                createdClones.Add(clone);
                return;
            }

            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            if (sourceFilter == null || sourceFilter.sharedMesh == null)
            {
                DestroyClone(clone);
                return;
            }

            MeshFilter cloneFilter = clone.AddComponent<MeshFilter>();
            cloneFilter.sharedMesh = sourceFilter.sharedMesh;

            MeshRenderer cloneRenderer = clone.AddComponent<MeshRenderer>();
            cloneRenderer.sharedMaterial = material;
            ConfigureRenderer(cloneRenderer);
            createdClones.Add(clone);
        }

        private static void ConfigureRenderer(Renderer renderer)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        private void ClearClones()
        {
            for (int i = createdClones.Count - 1; i >= 0; i--)
            {
                DestroyClone(createdClones[i]);
            }

            createdClones.Clear();
        }

        private void ClearExistingOutlineChildren()
        {
            Transform[] children = GetComponentsInChildren<Transform>(includeInactiveRenderers);
            for (int i = children.Length - 1; i >= 0; i--)
            {
                Transform child = children[i];
                if (child == null || child == transform)
                {
                    continue;
                }

                if (child.name.EndsWith(cloneSuffix))
                {
                    DestroyClone(child.gameObject);
                }
            }
        }

        private static void DestroyClone(GameObject clone)
        {
            if (clone == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(clone);
            }
            else
            {
                DestroyImmediate(clone);
            }
        }
    }
}
