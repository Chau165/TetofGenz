using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Menu
{
    public class MainMenuController : MonoBehaviour
    {
        private const string DefaultLevelSelectScene = "LevelSelectMenu";

        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private GameObject settingsPanel;

        public void ConfigureHowToPlayPanel(GameObject howToPlay)
        {
            howToPlayPanel = howToPlay;
        }

        public void ConfigureSettingsPanel(GameObject settings)
        {
            settingsPanel = settings;
        }

        private void Start()
        {
            MainMenuAudioSettings.LoadSavedVolume();
            SceneBackgroundMusic.EnsurePlaying();

            if (howToPlayPanel != null)
            {
                howToPlayPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void StartJourney()
        {
            if (!Application.CanStreamedLevelBeLoaded(DefaultLevelSelectScene))
            {
                Debug.LogError("[MainMenuController] Scene '" + DefaultLevelSelectScene +
                               "' is missing or not enabled in Build Settings.");
                return;
            }

            SceneManager.LoadScene(DefaultLevelSelectScene);
        }

        public void OpenHowToPlay()
        {
            CloseSettings();
            if (howToPlayPanel != null)
            {
                howToPlayPanel.SetActive(true);
            }
        }

        public void CloseHowToPlay()
        {
            if (howToPlayPanel != null)
            {
                howToPlayPanel.SetActive(false);
            }
        }

        public void OpenSettings()
        {
            CloseHowToPlay();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
