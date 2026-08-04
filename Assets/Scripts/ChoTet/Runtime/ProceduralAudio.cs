using UnityEngine;

namespace ChoTet.Runtime
{
    public static class ProceduralAudio
    {
        private static AudioClip footstepClip;
        private static AudioClip trafficHumClip;
        private static AudioClip hornClip;

        public static AudioClip GetFootstepClip()
        {
            if (footstepClip == null)
            {
                footstepClip = CreateFootstepClip();
            }
            return footstepClip;
        }

        public static AudioClip GetTrafficHumClip()
        {
            if (trafficHumClip == null)
            {
                trafficHumClip = CreateTrafficHumClip();
            }
            return trafficHumClip;
        }

        public static AudioClip GetHornClip()
        {
            if (hornClip == null)
            {
                hornClip = CreateHornClip();
            }
            return hornClip;
        }

        private static AudioClip CreateFootstepClip()
        {
            int sampleRate = 22050;
            float duration = 0.12f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            System.Random rand = new System.Random();

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float decay = Mathf.Exp(-28f * t);

                // Gravel/dirt scrunch noise
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.12f;
                // Add a low frequency impact thud (body of step)
                float thud = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.18f;

                samples[i] = (noise + thud) * decay;
            }

            AudioClip clip = AudioClip.Create("ProceduralFootstep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateTrafficHumClip()
        {
            int sampleRate = 22050;
            float duration = 2.0f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;

                // Stronger cabin engine: idle rumble + road noise.
                float engineVal = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.34f;
                engineVal += Mathf.Sin(2f * Mathf.PI * 96f * t) * 0.18f;
                engineVal += Mathf.Sin(2f * Mathf.PI * 144f * t) * 0.08f;
                engineVal += Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.04f;

                float roadNoise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.035f;
                // Soft tire hiss band
                float hiss = Mathf.Sin(2f * Mathf.PI * 1200f * t) * ((float)rand.NextDouble() * 0.02f);

                samples[i] = Mathf.Clamp(engineVal + roadNoise + hiss, -1f, 1f);
            }

            // Crossfade ends for seamless loop.
            int fade = Mathf.Min(sampleRate / 10, sampleCount / 8);
            for (int i = 0; i < fade; i++)
            {
                float w = i / (float)fade;
                samples[i] *= w;
                samples[sampleCount - 1 - i] *= w;
            }

            AudioClip clip = AudioClip.Create("ProceduralTrafficHum", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>Quieter 3D pass-by hum for AI traffic on the highway.</summary>
        public static AudioClip GetTrafficPassByClip()
        {
            return GetTrafficHumClip();
        }

        private static AudioClip CreateHornClip()
        {
            int sampleRate = 22050;
            float duration = 0.38f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                
                // Typical dual-tone horn frequencies
                float horn1 = Mathf.Sin(2f * Mathf.PI * 415f * t);
                float horn2 = Mathf.Sin(2f * Mathf.PI * 495f * t);
                
                // Fade-in envelope (first 20ms) and fade-out envelope (last 40ms)
                float env = 1f;
                if (t < 0.02f)
                {
                    env = t / 0.02f;
                }
                else if (t > duration - 0.04f)
                {
                    env = (duration - t) / 0.04f;
                }

                samples[i] = (horn1 + horn2) * 0.5f * 0.16f * env;
            }

            AudioClip clip = AudioClip.Create("ProceduralHorn", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
