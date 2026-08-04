using UnityEngine;

namespace ChoTet.Menu
{
    public static class MainMenuAudioSettings
    {
        public const string MasterVolumeKey = "MasterVolume";
        public const float DefaultVolume = 0.7f;

        public static void LoadSavedVolume()
        {
            AudioListener.volume = Mathf.Clamp01(
                PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume));
        }

        public static void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MasterVolumeKey, AudioListener.volume);
            PlayerPrefs.Save();
        }
    }
}
