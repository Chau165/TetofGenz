using UnityEngine;

namespace ChoTet.Level1
{
    public enum VehicleTurnSignalSide
    {
        None,
        Left,
        Right
    }

    /// <summary>Procedural amber blinkers for pickup pull-over / merge.</summary>
    public class VehicleTurnSignals : MonoBehaviour
    {
        private static readonly Color SignalOn = new Color(1f, 0.72f, 0.12f, 1f);
        private static readonly Color SignalOff = new Color(0.45f, 0.28f, 0.05f, 0.35f);

        [SerializeField] private float blinkInterval = 0.42f;

        private Renderer leftRenderer;
        private Renderer rightRenderer;
        private VehicleTurnSignalSide activeSide = VehicleTurnSignalSide.None;
        private float blinkTimer;
        private bool blinkOn;

        public void Build(Transform parent, float vehicleScale)
        {
            Clear();

            float sx = 0.34f * vehicleScale;
            float sy = 0.08f * vehicleScale;
            float sz = 0.14f * vehicleScale;
            float forward = 0.92f * vehicleScale;
            float lateral = 0.38f * vehicleScale;
            float height = 0.52f * vehicleScale;

            leftRenderer = CreateLamp(parent, "TurnSignalL", new Vector3(lateral, height, forward), new Vector3(sx, sy, sz));
            rightRenderer = CreateLamp(parent, "TurnSignalR", new Vector3(-lateral, height, forward), new Vector3(sx, sy, sz));
            SetSide(VehicleTurnSignalSide.None);
        }

        public void SetSide(VehicleTurnSignalSide side)
        {
            activeSide = side;
            blinkTimer = 0f;
            blinkOn = true;
            ApplyColors();
        }

        private void Update()
        {
            if (activeSide == VehicleTurnSignalSide.None)
            {
                return;
            }

            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                blinkOn = !blinkOn;
                ApplyColors();
            }
        }

        public void Clear()
        {
            activeSide = VehicleTurnSignalSide.None;
            if (leftRenderer != null)
            {
                Destroy(leftRenderer.gameObject);
            }

            if (rightRenderer != null)
            {
                Destroy(rightRenderer.gameObject);
            }

            leftRenderer = null;
            rightRenderer = null;
        }

        private void ApplyColors()
        {
            ApplyLamp(leftRenderer, activeSide == VehicleTurnSignalSide.Left && blinkOn);
            ApplyLamp(rightRenderer, activeSide == VehicleTurnSignalSide.Right && blinkOn);
        }

        private static Renderer CreateLamp(Transform parent, string name, Vector3 localPos, Vector3 localScale)
        {
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lamp.name = name;
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = localPos;
            lamp.transform.localRotation = Quaternion.identity;
            lamp.transform.localScale = localScale;

            Collider collider = lamp.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = lamp.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            material.SetColor("_BaseColor", SignalOff);
            material.SetColor("_EmissionColor", Color.black);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
            return renderer;
        }

        private static void ApplyLamp(Renderer renderer, bool on)
        {
            if (renderer == null)
            {
                return;
            }

            Material material = renderer.material;
            material.SetColor("_BaseColor", on ? SignalOn : SignalOff);
            material.SetColor("_EmissionColor", on ? SignalOn * 1.35f : Color.black);
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
