using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Simple burst particles for collision feedback.</summary>
    public class HighwayImpactBurst : MonoBehaviour
    {
        [SerializeField] private int particleCount = 12;
        [SerializeField] private float lifetime = 0.55f;
        [SerializeField] private float speed = 6f;
        [SerializeField] private float size = 0.22f;

        private ParticleSystem particles;
        private bool configured;

        private void Awake()
        {
            EnsureParticles();
        }

        public void PlayAt(Vector3 worldPos)
        {
            EnsureParticles();
            transform.position = worldPos + Vector3.up * 0.8f;
            particles.Play();
        }

        private void EnsureParticles()
        {
            if (configured)
            {
                return;
            }

            particles = gameObject.GetComponent<ParticleSystem>();
            if (particles == null)
            {
                particles = gameObject.AddComponent<ParticleSystem>();
            }

            if (particles.isPlaying)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            var main = particles.main;
            main.loop = false;
            main.duration = lifetime;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.maxParticles = particleCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particleCount) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer.sharedMaterial == null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                renderer.material.color = new Color(1f, 0.72f, 0.35f, 1f);
            }

            configured = true;
        }
    }
}
