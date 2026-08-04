using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Adds a soft blob shadow under tabletop decoration props.
    /// </summary>
    public static class MainMenuPropShadow
    {
        private static Material shadowMaterial;

        public static void Attach(GameObject prop, float scaleHint)
        {
            if (prop == null)
            {
                return;
            }

            GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.name = "ContactShadow";
            Collider shadowCollider = shadow.GetComponent<Collider>();
            if (shadowCollider != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(shadowCollider);
                }
                else
                {
                    Object.DestroyImmediate(shadowCollider);
                }
            }

            Transform shadowTransform = shadow.transform;
            shadowTransform.SetParent(prop.transform, false);
            shadowTransform.localPosition = new Vector3(0f, 0.015f, 0f);
            shadowTransform.localRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);

            float width = Mathf.Lerp(0.55f, 1.05f, scaleHint / 3f);
            shadowTransform.localScale = new Vector3(width, width * 0.72f, 1f);

            Renderer renderer = shadow.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetShadowMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material GetShadowMaterial()
        {
            if (shadowMaterial != null)
            {
                return shadowMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            shadowMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            if (shadowMaterial.HasProperty("_BaseColor"))
            {
                shadowMaterial.SetColor("_BaseColor", new Color(0f, 0f, 0f, 0.34f));
            }
            else
            {
                shadowMaterial.color = new Color(0f, 0f, 0f, 0.34f);
            }

            ConfigureTransparent(shadowMaterial);
            return shadowMaterial;
        }

        private static void ConfigureTransparent(Material material)
        {
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
            }

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }
}
