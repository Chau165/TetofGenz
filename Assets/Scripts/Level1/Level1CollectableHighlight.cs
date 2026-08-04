using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1
{
    /// <summary>White pulsing outline so packable items are easy to spot before collection.</summary>
    [DisallowMultipleComponent]
    public class Level1CollectableHighlight : MonoBehaviour
    {
        [SerializeField] private Transform visualTarget;
        [SerializeField] private float shellScale = 1.1f;
        [SerializeField] private float outlineWidth = 0.028f;
        [SerializeField] private float pulseSpeed = 2.4f;
        [SerializeField] private float minAlpha = 0.55f;
        [SerializeField] private float maxAlpha = 1f;

        private static Material sharedOutlineMaterial;

        private GameObject outlineRoot;
        private MaterialPropertyBlock propertyBlock;
        private Renderer outlineRenderer;
        private bool visible = true;

        private static Material SharedOutlineMaterial
        {
            get
            {
                if (sharedOutlineMaterial != null)
                {
                    return sharedOutlineMaterial;
                }

                Shader shader = Shader.Find("ChoTet/CollectableOutline");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                sharedOutlineMaterial = new Material(shader);
                sharedOutlineMaterial.name = "CollectableOutline (Runtime)";
                if (sharedOutlineMaterial.HasProperty("_OutlineWidth"))
                {
                    sharedOutlineMaterial.SetFloat("_OutlineWidth", 0.028f);
                }

                sharedOutlineMaterial.SetColor("_Color", Color.white);
                return sharedOutlineMaterial;
            }
        }

        private void Start()
        {
            if (visualTarget == null)
            {
                Transform visual = transform.Find("Visual");
                visualTarget = visual != null ? visual : transform;
            }

            RebuildOutline();
        }

        private void Update()
        {
            if (!visible || outlineRenderer == null)
            {
                return;
            }

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            Color color = new Color(1f, 1f, 1f, alpha);

            propertyBlock ??= new MaterialPropertyBlock();
            outlineRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", color);
            if (sharedOutlineMaterial != null && sharedOutlineMaterial.HasProperty("_OutlineWidth"))
            {
                propertyBlock.SetFloat("_OutlineWidth", outlineWidth);
            }

            outlineRenderer.SetPropertyBlock(propertyBlock);
        }

        public void Bind(Transform target)
        {
            visualTarget = target;
            RebuildOutline();
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            if (outlineRoot != null)
            {
                outlineRoot.SetActive(isVisible);
            }
        }

        private void RebuildOutline()
        {
            ClearOutline();
            if (visualTarget == null)
            {
                return;
            }

            MeshFilter sourceFilter = visualTarget.GetComponentInChildren<MeshFilter>();
            if (sourceFilter == null || sourceFilter.sharedMesh == null)
            {
                return;
            }

            outlineRoot = new GameObject("CollectOutline");
            outlineRoot.transform.SetParent(visualTarget, false);
            outlineRoot.transform.localPosition = Vector3.zero;
            outlineRoot.transform.localRotation = Quaternion.identity;
            outlineRoot.transform.localScale = Vector3.one * shellScale;

            MeshFilter outlineFilter = outlineRoot.AddComponent<MeshFilter>();
            outlineFilter.sharedMesh = sourceFilter.sharedMesh;

            outlineRenderer = outlineRoot.AddComponent<MeshRenderer>();
            outlineRenderer.sharedMaterial = SharedOutlineMaterial;
            outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.lightProbeUsage = LightProbeUsage.Off;
            outlineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void ClearOutline()
        {
            if (outlineRoot != null)
            {
                Destroy(outlineRoot);
                outlineRoot = null;
                outlineRenderer = null;
            }
        }

        private void OnDestroy()
        {
            ClearOutline();
        }
    }
}
