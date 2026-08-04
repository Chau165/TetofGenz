using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Spawns lightweight procedural particles for banner ambience (dust, sparkles, petals).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SimpleParticleSpawner : MonoBehaviour
    {
        public enum ParticleMode
        {
            Dust,
            Sparkle,
            Petal
        }

        [SerializeField] private ParticleMode mode = ParticleMode.Dust;
        [SerializeField] private float emissionRate = 6f;

        private ParticleSystem ps;

        public void Configure(ParticleMode particleMode, float rate)
        {
            mode = particleMode;
            emissionRate = rate;
            if (ps != null)
            {
                ApplySettings();
            }
        }

        private void Awake()
        {
            ps = gameObject.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
            {
                renderer.material = new Material(shader);
            }

            ApplySettings();
        }

        private void ApplySettings()
        {
            if (ps == null)
            {
                return;
            }

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = mode == ParticleMode.Sparkle ? 24 : 40;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = emissionRate;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            RectTransform rect = (RectTransform)transform;
            shape.scale = new Vector3(Mathf.Max(rect.rect.width, 40f), Mathf.Max(rect.rect.height, 20f), 0.1f);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = mode == ParticleMode.Sparkle;

            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = mode == ParticleMode.Petal;

            // VelocityOverLifetime requires x/y/z in the same curve mode — leave disabled.
            ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
            velocity.enabled = false;

            switch (mode)
            {
                case ParticleMode.Dust:
                    ConfigureDust(main, colorOverLifetime, shape);
                    break;
                case ParticleMode.Sparkle:
                    ConfigureSparkle(main, colorOverLifetime, sizeOverLifetime);
                    break;
                case ParticleMode.Petal:
                    ConfigurePetal(main, colorOverLifetime, rotationOverLifetime, shape);
                    break;
            }
        }

        private static void ResetShapeForUpwardDrift(ParticleSystem.ShapeModule shape)
        {
            shape.randomDirectionAmount = 0.15f;
            shape.sphericalDirectionAmount = 0f;
            shape.randomPositionAmount = 0.05f;
        }

        private void ConfigureDust(ParticleSystem.MainModule main,
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime,
            ParticleSystem.ShapeModule shape)
        {
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 4.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.gravityModifier = -0.02f;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.92f, 0.55f, 0.35f),
                new Color(1f, 0.78f, 0.42f, 0.15f));

            ResetShapeForUpwardDrift(shape);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.9f, 0.6f), 0f),
                    new GradientColorKey(new Color(1f, 0.8f, 0.45f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.45f, 0.25f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;
        }

        private void ConfigureSparkle(ParticleSystem.MainModule main,
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime,
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime)
        {
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.02f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new Color(1f, 0.95f, 0.65f, 0.85f);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.85f, 0.3f), 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.45f, 1f),
                new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }

        private void ConfigurePetal(ParticleSystem.MainModule main,
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime,
            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime,
            ParticleSystem.ShapeModule shape)
        {
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.55f, 0.45f, 0.7f),
                new Color(1f, 0.82f, 0.35f, 0.55f));
            main.gravityModifier = 0.08f;

            shape.randomDirectionAmount = 0.35f;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.7f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.85f, 0.4f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.65f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            rotationOverLifetime.enabled = true;
            rotationOverLifetime.separateAxes = false;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-90f * Mathf.Deg2Rad, 90f * Mathf.Deg2Rad);
        }
    }
}
