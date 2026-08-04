using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Persistent background music for menu + gameplay scenes.
    /// Ducks only while dialogue TTS is speaking (not for engine / ambient loops).
    /// </summary>
    public sealed class SceneBackgroundMusic : MonoBehaviour
    {
        private const string ObjectName = "BackgroundMusic";
        private const string MusicResourcePath = "Audio/Background/272. Chinese New Year";
        private const float NormalVolume = 0.28f;
        private const float DuckVolume = 0.1f;
        private const float VolumeFadeSpeed = 0.55f;

        private static SceneBackgroundMusic instance;
        private AudioSource musicSource;
        private bool missingClipWarningShown;
        private bool recreatedAudioSource;

        public static SceneBackgroundMusic Instance
        {
            get
            {
                EnsureExists();
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBeforeSceneLoad()
        {
            EnsureExists();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAfterSceneLoad()
        {
            EnsureExists();
            if (instance != null)
            {
                instance.ForcePlay();
            }
        }

        public static void EnsurePlaying()
        {
            SceneBackgroundMusic music = EnsureExists();
            music?.ForcePlay();
        }

        private static SceneBackgroundMusic EnsureExists()
        {
            if (instance != null)
            {
                return instance;
            }

            SceneBackgroundMusic existing = Object.FindAnyObjectByType<SceneBackgroundMusic>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            GameObject musicObject = new GameObject(ObjectName);
            instance = musicObject.AddComponent<SceneBackgroundMusic>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            gameObject.name = ObjectName;
            DontDestroyOnLoad(gameObject);
            ConfigureAudioSource();
            EnsureClipLoaded();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            StartCoroutine(StartMusicWhenReady());
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        private void Update()
        {
            if (!EnsureClipLoaded())
            {
                return;
            }

            TryPlayMusic();

            float targetVolume = IsDialogueSpeaking() ? DuckVolume : NormalVolume;
            musicSource.volume = Mathf.MoveTowards(
                musicSource.volume,
                targetVolume,
                VolumeFadeSpeed * Time.unscaledDeltaTime);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ForcePlay();
        }

        public void ForcePlay()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            StartCoroutine(StartMusicWhenReady());
        }

        private void ConfigureAudioSource()
        {
            if (musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
                if (musicSource == null)
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                }
            }

            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.priority = 64;
            musicSource.volume = NormalVolume;
            musicSource.mute = false;
            musicSource.ignoreListenerPause = true;
            musicSource.dopplerLevel = 0f;
        }

        private IEnumerator StartMusicWhenReady()
        {
            yield return null;

            if (!EnsureClipLoaded())
            {
                yield break;
            }

            int guard = 0;
            while (musicSource.clip.loadState == AudioDataLoadState.Loading && guard < 120)
            {
                guard++;
                yield return null;
            }

            TryPlayMusic(restart: !musicSource.isPlaying);
        }

        private bool EnsureClipLoaded()
        {
            ConfigureAudioSource();

            if (musicSource.clip == null)
            {
                musicSource.clip = Resources.Load<AudioClip>(MusicResourcePath);
            }

            if (musicSource.clip == null)
            {
                if (!missingClipWarningShown)
                {
                    Debug.LogWarning("[SceneBackgroundMusic] Clip not found in Resources: " + MusicResourcePath);
                    missingClipWarningShown = true;
                }

                return false;
            }

            if (musicSource.clip.loadState == AudioDataLoadState.Unloaded)
            {
                musicSource.clip.LoadAudioData();
            }

            return true;
        }

        private void TryPlayMusic(bool restart = false)
        {
            if (musicSource == null || musicSource.clip == null)
            {
                return;
            }

            if (musicSource.clip.loadState == AudioDataLoadState.Loading)
            {
                return;
            }

            if (musicSource.isPlaying && !restart)
            {
                return;
            }

            if (restart)
            {
                musicSource.timeSamples = 0;
            }

            musicSource.mute = false;
            musicSource.Play();

            if (musicSource.isPlaying || recreatedAudioSource)
            {
                return;
            }

            musicSource.enabled = false;
            musicSource = gameObject.AddComponent<AudioSource>();
            recreatedAudioSource = true;
            ConfigureAudioSource();
            musicSource.clip = Resources.Load<AudioClip>(MusicResourcePath);

            if (musicSource.clip != null)
            {
                musicSource.Play();
            }
        }

        private static bool IsDialogueSpeaking()
        {
            VoiceNarrator narrator = Object.FindAnyObjectByType<VoiceNarrator>();
            if (narrator == null)
            {
                return false;
            }

            AudioSource source = narrator.GetComponent<AudioSource>();
            return source != null && source.isPlaying;
        }
    }
}
