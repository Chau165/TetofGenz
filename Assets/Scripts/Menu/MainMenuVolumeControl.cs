using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Top-right volume button that toggles a horizontal master-volume slider.
    /// </summary>
    public class MainMenuVolumeControl : MonoBehaviour
    {
        private Button toggleButton;
        private GameObject sliderPanel;
        private Slider slider;
        private Text percentLabel;

        public void Initialize(Button button, GameObject panel, Slider volumeSlider, Text volumePercentLabel)
        {
            toggleButton = button;
            sliderPanel = panel;
            slider = volumeSlider;
            percentLabel = volumePercentLabel;

            float saved = PlayerPrefs.GetFloat(MainMenuAudioSettings.MasterVolumeKey,
                MainMenuAudioSettings.DefaultVolume);
            saved = Mathf.Clamp01(saved);
            slider.SetValueWithoutNotify(saved);
            UpdatePercentLabel(saved);
            MainMenuAudioSettings.SetVolume(saved);

            slider.onValueChanged.AddListener(OnSliderValueChanged);
            toggleButton.onClick.AddListener(ToggleSliderPanel);
            sliderPanel.SetActive(false);
        }

        private void ToggleSliderPanel()
        {
            sliderPanel.SetActive(!sliderPanel.activeSelf);
        }

        private void OnSliderValueChanged(float value)
        {
            MainMenuAudioSettings.SetVolume(value);
            UpdatePercentLabel(value);
        }

        private void UpdatePercentLabel(float value)
        {
            if (percentLabel == null)
            {
                return;
            }

            percentLabel.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }
}
