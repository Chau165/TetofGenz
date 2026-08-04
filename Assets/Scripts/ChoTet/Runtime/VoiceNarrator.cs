using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Downloads and plays Vietnamese text-to-speech audio via Google Translate TTS.
    /// Pitch is adjusted depending on the speaker name to keep voices consistent and distinct.
    /// </summary>
    public class VoiceNarrator : MonoBehaviour
    {
        private static VoiceNarrator instance;
        private AudioSource audioSource;
        private Coroutine speakRoutine;

        public static VoiceNarrator Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("VoiceNarrator");
                    instance = go.AddComponent<VoiceNarrator>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D Sound
        }

        public void Speak(string speaker, string text)
        {
            if (speakRoutine != null)
            {
                StopCoroutine(speakRoutine);
            }

            if (audioSource != null)
            {
                audioSource.Stop();
            }

            speakRoutine = StartCoroutine(SpeakRoutine(speaker, text));
        }

        public void Stop()
        {
            if (speakRoutine != null)
            {
                StopCoroutine(speakRoutine);
                speakRoutine = null;
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private IEnumerator SpeakRoutine(string speaker, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }

            // Remove guidelines in dialogue text if any (e.g. "Nhấn Enter để tiếp tục")
            int hintIdx = text.IndexOf("\n\n");
            if (hintIdx >= 0)
            {
                text = text.Substring(0, hintIdx);
            }

            text = text.Replace("\n", " ").Replace("•", "").Trim();
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }

            // Set voice pitch depending on the speaker name to ensure consistency
            float pitch = GetSpeakerPitch(speaker);
            if (audioSource != null)
            {
                audioSource.pitch = pitch;
            }

            string url = "https://translate.google.com/translate_tts?ie=UTF-8&tl=vi&client=tw-ob&q=" + UnityWebRequest.EscapeURL(text);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                // Set common browser User-Agent to avoid rate-limiting or security checks
                www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null && clip.loadState == AudioDataLoadState.Loaded && audioSource != null)
                    {
                        audioSource.clip = clip;
                        audioSource.Play();
                    }
                }
                else
                {
                    Debug.LogWarning("Failed to download voice clip: " + www.error + " URL: " + url);
                }
            }
        }

        private string NormalizeSpeakerName(string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return "";
            string s = speaker.ToLowerInvariant().Trim();
            
            // Remove common shop/prefix texts to isolate the character name
            s = s.Replace("quầy hoa quả ", "")
                 .Replace("quầy hoa mai ", "")
                 .Replace("quầy bánh kẹo ", "")
                 .Replace("quầy thịt ", "")
                 .Replace("quầy quà tết ", "")
                 .Replace("quầy ", "")
                 .Replace("sạp ", "");

            s = RemoveDiacritics(s);
            return s;
        }

        private string RemoveDiacritics(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
                "í", "ì", "ỉ", "ĩ", "ị",
                "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ",
                "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự",
                "ý", "ỳ", "ỷ", "ỹ", "ỵ" };
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
                "i", "i", "i", "i", "i",
                "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
                "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u",
                "y", "y", "y", "y", "y" };
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
            }
            return text;
        }

        private float GetSpeakerPitch(string speaker)
        {
            string name = NormalizeSpeakerName(speaker);
            if (string.IsNullOrEmpty(name)) return 1.15f;

            // baseSpeed: 1.20f scales up the tempo/speed of speech to make it flow naturally
            float baseSpeed = 1.20f; 
            float relativePitch = 1.0f;

            if (name == "con" || name == "em" || name == "tui" || name == "nguoi choi")
            {
                relativePitch = 1.06f; // Player Character (unified)
            }
            else if (name == "me")
            {
                relativePitch = 0.98f; // Mother
            }
            else if (name == "bo" || name == "ba")
            {
                relativePitch = 0.86f; // Father
            }
            else if (name == "bac tai")
            {
                relativePitch = 0.90f; // Driver
            }
            else if (name == "chu ba")
            {
                relativePitch = 0.84f; // Uncle Ba
            }
            else if (name == "co lan")
            {
                relativePitch = 1.04f; // Aunt Lan
            }
            else if (name == "co huong")
            {
                relativePitch = 1.00f; // Aunt Huong
            }
            else if (name == "chu tu")
            {
                relativePitch = 0.88f; // Uncle Tu
            }
            else if (name == "chi sau")
            {
                relativePitch = 1.02f; // Sister Sau
            }
            else if (name == "be na")
            {
                relativePitch = 1.15f; // Child
            }
            else if (name == "minh")
            {
                relativePitch = 0.92f;
            }
            else if (name == "lan")
            {
                relativePitch = 1.03f;
            }
            else if (name == "anh tuan")
            {
                relativePitch = 0.94f;
            }
            else if (name == "ban hung")
            {
                relativePitch = 0.96f;
            }
            else if (name == "chi mai")
            {
                relativePitch = 1.05f;
            }
            else if (name == "su thay")
            {
                relativePitch = 0.82f;
            }
            else
            {
                // Generalized keyword matching
                if (name.Contains("chu") || name.Contains("ong") || name.Contains("bac") || name.Contains("thay") || name.Contains("nam"))
                {
                    relativePitch = 0.88f;
                }
                else if (name.Contains("co") || name.Contains("ba ") || name.Contains("chi") || name.Contains("nu"))
                {
                    relativePitch = 1.02f;
                }
            }

            return relativePitch * baseSpeed;
        }
    }
}
