using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// UI-layer festive FX for the title banner: glow, ribbons, and particles.
    /// </summary>
    public static class MainMenuBannerUiFxBuilder
    {
        public static void BuildGlow(Transform cluster, KenneyRpgUiLibrary library)
        {
            if (cluster == null || library == null)
            {
                return;
            }

            Sprite sprite = library.panelBeigeLight != null ? library.panelBeigeLight : library.panelInsetBeige;
            GameObject glow = new GameObject("BannerGlow", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(cluster, false);

            Image image = glow.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = new Color(1f, 0.9f, 0.55f, 0.14f);
            image.raycastTarget = false;

            RectTransform rect = glow.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 4f);
            rect.sizeDelta = new Vector2(780f, 300f);
        }

        public static void BuildOverlays(Transform cluster, KenneyRpgUiLibrary library)
        {
            if (cluster == null || library == null)
            {
                return;
            }

            GameObject fxRoot = new GameObject("BannerFxRoot", typeof(RectTransform));
            fxRoot.transform.SetParent(cluster, false);
            Stretch(fxRoot.GetComponent<RectTransform>());

            CreateParticleZones(fxRoot.transform);
        }

        private static void CreateParticleZones(Transform parent)
        {
            CreateParticleZone(parent, "DustZone", new Vector2(0f, 40f), new Vector2(780f, 300f),
                SimpleParticleSpawner.ParticleMode.Dust, 5f);
            CreateParticleZone(parent, "SparkleTop", new Vector2(0f, 128f), new Vector2(760f, 36f),
                SimpleParticleSpawner.ParticleMode.Sparkle, 8f);
            CreateParticleZone(parent, "SparkleBottom", new Vector2(0f, -118f), new Vector2(760f, 28f),
                SimpleParticleSpawner.ParticleMode.Sparkle, 6f);
            CreateParticleZone(parent, "PetalFall", new Vector2(0f, 160f), new Vector2(700f, 60f),
                SimpleParticleSpawner.ParticleMode.Petal, 3f);
        }

        private static void CreateParticleZone(Transform parent, string name, Vector2 pos, Vector2 size,
            SimpleParticleSpawner.ParticleMode mode, float rate)
        {
            GameObject zone = new GameObject(name, typeof(RectTransform));
            zone.transform.SetParent(parent, false);

            RectTransform rect = zone.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            SimpleParticleSpawner spawner = zone.AddComponent<SimpleParticleSpawner>();
            spawner.Configure(mode, rate);
        }

        private static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }
    }
}
