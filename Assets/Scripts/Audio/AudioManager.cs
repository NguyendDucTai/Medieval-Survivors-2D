using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivors.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;

        [Header("Audio Clips Catalog")]
        public AudioClip bgmMedievalTheme;
        public AudioClip sfxGemPickup;
        public AudioClip sfxHit;
        public AudioClip sfxLevelUp;
        public AudioClip sfxPlayerDeath;
        public AudioClip sfxShoot;
        public AudioClip sfxButtonClick;

        public float MusicVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 1.0f;
        public bool IsMuted { get; private set; } = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupAudioSources();
            LoadAudioSettings();
            LoadClipsIfNull();
        }

        private void Start()
        {
            PlayBGM();
        }

        private void SetupAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void LoadAudioSettings()
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            SfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            IsMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

            ApplyVolumes();
        }

        private void LoadClipsIfNull()
        {
#if UNITY_EDITOR
            if (sfxGemPickup == null)
            {
                sfxGemPickup = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_gem_pickup.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_gem_pickup.wav");
            }
            if (sfxHit == null)
            {
                sfxHit = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_hit.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit.wav");
            }
            if (sfxLevelUp == null)
            {
                sfxLevelUp = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_levelup.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_levelup.wav");
            }
            if (sfxPlayerDeath == null)
            {
                sfxPlayerDeath = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_player_death.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_player_death.wav");
            }
            if (sfxShoot == null)
            {
                sfxShoot = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_shoot.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_shoot.wav");
            }
#endif
        }

        public void SetMusicVolume(float vol)
        {
            MusicVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetSfxVolume(float vol)
        {
            SfxVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("SFXVolume", SfxVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetMute(bool mute)
        {
            IsMuted = mute;
            PlayerPrefs.SetInt("IsMuted", IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = IsMuted ? 0f : MusicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = IsMuted ? 0f : SfxVolume;
            }
        }

        public void PlayBGM()
        {
            if (bgmSource != null && bgmMedievalTheme != null && !bgmSource.isPlaying)
            {
                bgmSource.clip = bgmMedievalTheme;
                bgmSource.Play();
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (IsMuted || sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, SfxVolume);
        }

        public void PlaySfxByName(string soundName)
        {
            switch (soundName.ToLower())
            {
                case "gem":
                case "pickup":
                    PlaySFX(sfxGemPickup);
                    break;
                case "hit":
                    PlaySFX(sfxHit);
                    break;
                case "levelup":
                    PlaySFX(sfxLevelUp);
                    break;
                case "death":
                    PlaySFX(sfxPlayerDeath);
                    break;
                case "shoot":
                case "attack":
                    PlaySFX(sfxShoot);
                    break;
                case "click":
                case "button":
                    PlaySFX(sfxButtonClick ?? sfxGemPickup);
                    break;
            }
        }
    }
}
