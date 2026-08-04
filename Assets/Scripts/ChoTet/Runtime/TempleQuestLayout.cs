using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Places temple quest props, large interact triggers, worshippers, and guidance labels.
    /// </summary>
    public class TempleQuestLayout : MonoBehaviour
    {
        private const float HallFloorY = 1.65f;
        private const string ResourcesRoot = "ChoTet/TempleProps/";
        private static Material incenseSmokeMaterial;

        // In front of the Buddha pedestal (pedestal occupies ~z 151–159).
        private static readonly Vector3 GatePos = new Vector3(55f, HallFloorY, 98f);
        private static readonly Vector3 OfferingPos = new Vector3(51.5f, HallFloorY, 146.2f);
        private static readonly Vector3 IncensePos = new Vector3(55f, HallFloorY, 147.4f);
        private static readonly Vector3 FortunePos = new Vector3(58.8f, HallFloorY, 146.2f);

        private void Awake()
        {
            Apply();
        }

        [ContextMenu("Apply Temple Quest Layout")]
        public void Apply()
        {
            SetupGateStation();

            SetupStation(
                "TempleOfferingAltar",
                "TempleOfferingPoint",
                "TempleOfferingAltar",
                OfferingPos,
                visualScale: 2.1f,
                triggerSize: new Vector3(4.2f, 2.8f, 3.6f),
                prompt: "Nhấn E để dâng lễ phẩm Tết",
                label: "Dâng lễ phẩm Tết",
                kind: TempleInteractionPoint.TempleInteractionKind.Offering,
                isFortune: false,
                addSmoke: false);

            SetupStation(
                "TempleIncenseSpot",
                "TempleIncensePoint",
                "TempleIncenseBurner",
                IncensePos,
                visualScale: 2.0f,
                triggerSize: new Vector3(3.8f, 2.8f, 3.6f),
                prompt: "Nhấn E để thắp hương",
                label: "Thắp hương",
                kind: TempleInteractionPoint.TempleInteractionKind.Incense,
                isFortune: false,
                addSmoke: true);

            SetupStation(
                "TempleFortuneStand",
                "TempleFortunePoint",
                "TempleFortuneTube",
                FortunePos,
                visualScale: 2.2f,
                triggerSize: new Vector3(3.8f, 2.8f, 3.6f),
                prompt: "Nhấn E để xin quẻ",
                label: "Xin quẻ",
                kind: TempleInteractionPoint.TempleInteractionKind.Offering,
                isFortune: true,
                addSmoke: false);

            // Creative Character worshippers are baked by ChoTetMarketCrowdBuilder (ChoTetTempleCrowd).
            RemoveBlockingBarriers();
            EnsureSafetyDeck();
        }

        private static void SetupGateStation()
        {
            GameObject gate = GameObject.Find("TempleGateInteractPoint");
            if (gate == null)
            {
                gate = new GameObject("TempleGateInteractPoint");
            }

            gate.transform.position = GatePos + Vector3.up;
            BoxCollider box = gate.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = gate.AddComponent<BoxCollider>();
            }

            box.isTrigger = true;
            box.center = Vector3.zero;
            box.size = new Vector3(10f, 3f, 6f);

            TempleGateInteractable interactable = gate.GetComponent<TempleGateInteractable>();
            if (interactable == null)
            {
                interactable = gate.AddComponent<TempleGateInteractable>();
            }

            interactable.SetPrompt("Nhấn E để vào chùa đầu năm");
            EnsureLabel(gate.transform, "Qua cổng chùa", new Color(1f, 0.95f, 0.55f));

            // Visible gate marker mats.
            EnsureFloorMarker(gate.transform, new Color(1f, 0.85f, 0.2f, 0.55f), 3.2f);
        }

        private static void SetupStation(
            string visualName,
            string interactableName,
            string resourceName,
            Vector3 floorPosition,
            float visualScale,
            Vector3 triggerSize,
            string prompt,
            string label,
            TempleInteractionPoint.TempleInteractionKind kind,
            bool isFortune,
            bool addSmoke)
        {
            GameObject visual = EnsureBlenderVisual(visualName, resourceName, floorPosition, visualScale);
            GameObject point = EnsureTriggerPoint(interactableName, floorPosition + Vector3.up * 1.1f, triggerSize);

            if (isFortune)
            {
                TempleFortunePoint fortune = point.GetComponent<TempleFortunePoint>();
                if (fortune == null)
                {
                    fortune = point.AddComponent<TempleFortunePoint>();
                }

                // Remove wrong component if layout previously attached offering to fortune root.
                TempleInteractionPoint wrong = point.GetComponent<TempleInteractionPoint>();
                if (wrong != null)
                {
                    SafeDestroy(wrong);
                }

                fortune.SetPrompt(prompt);
            }
            else
            {
                TempleInteractionPoint interaction = point.GetComponent<TempleInteractionPoint>();
                if (interaction == null)
                {
                    interaction = point.AddComponent<TempleInteractionPoint>();
                }

                TempleFortunePoint wrong = point.GetComponent<TempleFortunePoint>();
                if (wrong != null)
                {
                    SafeDestroy(wrong);
                }

                interaction.interactionKind = kind;
                interaction.SetPrompt(prompt);
            }

            if (addSmoke)
            {
                EnsureIncenseEffects(visual.transform);
            }

            EnsureLabel(point.transform, label, new Color(1f, 0.92f, 0.45f));
            EnsureFloorMarker(visual.transform, new Color(1f, 0.8f, 0.2f, 0.4f), 1.8f);
        }

        private static GameObject EnsureBlenderVisual(
            string visualName,
            string resourceName,
            Vector3 floorPosition,
            float scale)
        {
            GameObject visual = GameObject.Find(visualName);
            if (visual == null)
            {
                visual = new GameObject(visualName);
            }

            // Always rebuild prop mesh so scale/position stay correct.
            ClearChildren(visual.transform);
            visual.transform.position = floorPosition;
            visual.transform.rotation = Quaternion.identity;

            GameObject prefab = Resources.Load<GameObject>(ResourcesRoot + resourceName);
            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab, visual.transform);
                instance.name = "BlenderPropRoot";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one * scale;
                StripColliders(instance);
            }

            return visual;
        }

        private static GameObject EnsureTriggerPoint(string name, Vector3 position, Vector3 size)
        {
            GameObject point = GameObject.Find(name);
            if (point == null)
            {
                point = new GameObject(name);
            }

            point.transform.position = position;

            // Dedicated trigger child so mesh/label never steal the collider.
            Transform triggerTransform = point.transform.Find("InteractTrigger");
            GameObject triggerObject = triggerTransform != null ? triggerTransform.gameObject : new GameObject("InteractTrigger");
            triggerObject.transform.SetParent(point.transform, false);
            triggerObject.transform.localPosition = Vector3.zero;
            triggerObject.transform.localRotation = Quaternion.identity;
            triggerObject.transform.localScale = Vector3.one;

            BoxCollider box = triggerObject.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = triggerObject.AddComponent<BoxCollider>();
            }

            box.isTrigger = true;
            box.center = Vector3.zero;
            box.size = size;

            // Interactable lives on parent; OverlapSphere uses GetComponentInParent.
            BoxCollider parentBox = point.GetComponent<BoxCollider>();
            if (parentBox == null)
            {
                parentBox = point.AddComponent<BoxCollider>();
            }

            parentBox.isTrigger = true;
            parentBox.center = Vector3.zero;
            parentBox.size = size;

            return point;
        }

        private static void RemoveBlockingBarriers()
        {
            // Old invisible walls near the temple blocked free movement where no fence exists.
            string[] barrierRoots = { "TempleFallBarriers", "TempleWorshippers" };
            for (int i = 0; i < barrierRoots.Length; i++)
            {
                GameObject root = GameObject.Find(barrierRoots[i]);
                if (root != null)
                {
                    SafeDestroy(root);
                }
            }
        }

        private static void EnsureIncenseEffects(Transform parent)
        {
            Transform smoke = parent.Find("TempleIncenseSmoke");
            if (smoke == null)
            {
                GameObject smokeObject = new GameObject("TempleIncenseSmoke");
                smokeObject.transform.SetParent(parent, false);
                smokeObject.transform.localPosition = new Vector3(0f, 1.8f, 0f);
                ParticleSystem particles = smokeObject.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particles.main;
                main.loop = true;
                main.playOnAwake = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.4f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
                main.startColor = new Color(1f, 1f, 1f, 0.55f);
                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = 14f;
                ParticleSystem.ColorOverLifetimeModule colorOver = particles.colorOverLifetime;
                colorOver.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new[]
                    {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(new Color(0.92f, 0.92f, 0.92f), 1f)
                    },
                    new[]
                    {
                        new GradientAlphaKey(0.55f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    });
                colorOver.color = gradient;
                ApplyIncenseSmokeMaterial(particles);
                particles.Play(true);
            }
            else
            {
                ParticleSystem existingSmoke = smoke.GetComponent<ParticleSystem>();
                if (existingSmoke != null)
                {
                    ApplyIncenseSmokeMaterial(existingSmoke);
                    if (!existingSmoke.isPlaying)
                    {
                        existingSmoke.Play(true);
                    }
                }
            }

            Transform light = parent.Find("TempleIncenseWarmLight");
            if (light == null)
            {
                GameObject lightObject = new GameObject("TempleIncenseWarmLight");
                lightObject.transform.SetParent(parent, false);
                lightObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                Light warm = lightObject.AddComponent<Light>();
                warm.type = LightType.Point;
                warm.color = new Color(1f, 0.77f, 0.48f, 1f);
                warm.range = 5f;
                warm.intensity = 2.1f;
                warm.enabled = false;
            }
        }

        private static void ApplyIncenseSmokeMaterial(ParticleSystem particles)
        {
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                return;
            }

            if (incenseSmokeMaterial == null)
            {
                incenseSmokeMaterial = Resources.Load<Material>("Temple/TempleIncenseSmokeWhite");
                if (incenseSmokeMaterial == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Particles/Standard Unlit")
                        ?? Shader.Find("Standard");
                    incenseSmokeMaterial = new Material(shader);
                    if (incenseSmokeMaterial.HasProperty("_BaseColor"))
                    {
                        incenseSmokeMaterial.SetColor("_BaseColor", Color.white);
                    }

                    if (incenseSmokeMaterial.HasProperty("_Color"))
                    {
                        incenseSmokeMaterial.SetColor("_Color", Color.white);
                    }
                }
            }

            renderer.sharedMaterials = new[] { incenseSmokeMaterial };
        }

        private static void EnsureLabel(Transform parent, string text, Color color)
        {
            const string labelName = "QuestLabel";
            Transform existing = parent.Find(labelName);
            if (existing != null)
            {
                SafeDestroy(existing.gameObject);
            }

            GameObject labelObject = new GameObject(labelName);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
            textMesh.fontStyle = FontStyle.Bold;
            labelObject.AddComponent<QuestLabelBillboard>();
        }

        private static void EnsureFloorMarker(Transform parent, Color color, float diameter)
        {
            const string markerName = "FloorMarker";
            Transform existing = parent.Find(markerName);
            if (existing != null)
            {
                SafeDestroy(existing.gameObject);
            }

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = markerName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            marker.transform.localScale = new Vector3(diameter, 0.03f, diameter);
            StripCollider(marker);
            ApplyColor(marker, color);
        }

        private static void ApplyColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard");
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(parent.GetChild(i).gameObject);
            }
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                SafeDestroy(colliders[i]);
            }
        }

        private static void StripCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                SafeDestroy(collider);
            }
        }

        private static void EnsureSafetyDeck()
        {
            const string name = "TempleSafetyDeck";
            if (GameObject.Find(name) != null)
            {
                return;
            }

            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = name;
            deck.transform.position = new Vector3(55f, -0.35f, 130f);
            deck.transform.localScale = new Vector3(72f, 0.8f, 92f);
            Renderer renderer = deck.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private static void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    public class QuestLabelBillboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up);
        }
    }
}
