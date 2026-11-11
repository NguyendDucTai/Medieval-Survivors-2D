using System;
using System.Collections;
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
        public AudioSource hitSfxSource;

        [Header("Audio Clips Catalog")]
        public AudioClip bgmMedievalTheme;
        public AudioClip sfxGemPickup;
        public AudioClip sfxHit;
        public AudioClip sfxLevelUp;
        public AudioClip sfxPlayerDeath;
        public AudioClip sfxShoot;
        public AudioClip sfxButtonClick;

        [Header("Combat Hit Audio Throttle")]
        [Tooltip("Minimum interval between enemy hit sounds to prevent audio clipping during large AoE swarms")]
        public float minEnemyHitInterval = 0.055f;
        [Tooltip("Minimum interval between player hit sounds")]
        public float minPlayerHitInterval = 0.12f;

        private float lastEnemyHitTime = -10f;
        private float lastPlayerHitTime = -10f;

        [Header("BGM Ducking (Level Up / Chest Modal)")]
        private float currentDuckMultiplier = 1.0f;
        private Coroutine duckCoroutine;

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
            LoadClipsIfNull();
            LoadAudioSettings();
        }

        private void Start()
        {
            // BGM is only started when the player enters battle via MainMenuManager.StartBattle()
        }

        private void SetupAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.spatialBlend = 0f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }

            if (hitSfxSource == null)
            {
                hitSfxSource = gameObject.AddComponent<AudioSource>();
                hitSfxSource.loop = false;
                hitSfxSource.playOnAwake = false;
                hitSfxSource.spatialBlend = 0f;
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
            if (bgmMedievalTheme == null)
            {
                bgmMedievalTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/bgm_medieval_theme.wav")
                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_medieval_theme.wav");
            }
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

            // Runtime procedural fallback if BGM clip was not yet serialized or imported
            if (bgmMedievalTheme == null)
            {
                bgmMedievalTheme = GenerateProceduralMedievalBgmClip();
            }

            // Runtime procedural fallback if Hit clip is missing in build
            if (sfxHit == null)
            {
                sfxHit = GenerateProceduralHitClip();
            }
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
                bgmSource.volume = IsMuted ? 0f : (MusicVolume * currentDuckMultiplier);
            }

            if (sfxSource != null)
            {
                sfxSource.volume = IsMuted ? 0f : SfxVolume;
            }

            if (hitSfxSource != null)
            {
                hitSfxSource.volume = IsMuted ? 0f : SfxVolume;
            }
        }

        /// <summary>
        /// Smoothly lowers (ducks) or restores background music volume using unscaled time
        /// so it works seamlessly even when Time.timeScale == 0 during Level Up or Boss Chest modals.
        /// </summary>
        public void SetBgmDucking(bool isDucked, float duckRatio = 0.35f, float duration = 0.25f)
        {
            float target = isDucked ? Mathf.Clamp(duckRatio, 0.05f, 1.0f) : 1.0f;

            if (!gameObject.activeInHierarchy)
            {
                currentDuckMultiplier = target;
                ApplyVolumes();
                return;
            }

            if (duckCoroutine != null)
            {
                StopCoroutine(duckCoroutine);
            }
            duckCoroutine = StartCoroutine(FadeBgmDuckRoutine(target, duration));
        }

        private IEnumerator FadeBgmDuckRoutine(float targetMultiplier, float duration)
        {
            float startMultiplier = currentDuckMultiplier;
            if (duration <= 0.001f)
            {
                currentDuckMultiplier = targetMultiplier;
                ApplyVolumes();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth step curve for natural ear perception
                float smoothT = t * t * (3f - 2f * t);
                currentDuckMultiplier = Mathf.Lerp(startMultiplier, targetMultiplier, smoothT);
                ApplyVolumes();
                yield return null;
            }

            currentDuckMultiplier = targetMultiplier;
            ApplyVolumes();
            duckCoroutine = null;
        }

        public void PlayBGM()
        {
            if (bgmSource == null) SetupAudioSources();
            if (bgmMedievalTheme == null) LoadClipsIfNull();

            if (bgmSource != null && bgmMedievalTheme != null)
            {
                bgmSource.loop = true;
                if (bgmSource.clip != bgmMedievalTheme)
                {
                    bgmSource.clip = bgmMedievalTheme;
                }
                ApplyVolumes();
                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (IsMuted || sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, SfxVolume);
        }

        /// <summary>
        /// Intelligent combat hit sound dispatcher with throttle cooldown and pitch variation
        /// to prevent speaker clipping during heavy AoE swarms while keeping hits punchy.
        /// </summary>
        public void PlayHitSound(bool isPlayer, bool isCrit = false)
        {
            if (IsMuted || SfxVolume <= 0.001f) return;
            if (hitSfxSource == null) SetupAudioSources();
            if (sfxHit == null) LoadClipsIfNull();
            if (sfxHit == null || hitSfxSource == null) return;

            float now = Time.unscaledTime;

            if (isPlayer)
            {
                if (now - lastPlayerHitTime < minPlayerHitInterval) return;
                lastPlayerHitTime = now;

                // Lower, heavier armor thud so player immediately feels incoming damage
                hitSfxSource.pitch = UnityEngine.Random.Range(0.80f, 0.88f);
                hitSfxSource.PlayOneShot(sfxHit, SfxVolume);
            }
            else
            {
                // Allow critical hits a slightly tighter window so crits always pop through
                float requiredInterval = isCrit ? (minEnemyHitInterval * 0.6f) : minEnemyHitInterval;
                if (now - lastEnemyHitTime < requiredInterval) return;
                lastEnemyHitTime = now;

                if (isCrit)
                {
                    // Sharp, high-energy impact for critical strikes
                    hitSfxSource.pitch = UnityEngine.Random.Range(1.20f, 1.28f);
                    hitSfxSource.PlayOneShot(sfxHit, Mathf.Min(1.0f, SfxVolume * 0.95f));
                }
                else
                {
                    // Organic pitch variation across normal hits
                    hitSfxSource.pitch = UnityEngine.Random.Range(0.92f, 1.12f);
                    hitSfxSource.PlayOneShot(sfxHit, SfxVolume * 0.75f);
                }
            }
        }

        public void PlaySfxByName(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;

            switch (soundName.ToLower())
            {
                case "gem":
                case "pickup":
                    PlaySFX(sfxGemPickup);
                    break;
                case "hit":
                    PlayHitSound(false, false);
                    break;
                case "player_hit":
                    PlayHitSound(true, false);
                    break;
                case "crit":
                    PlayHitSound(false, true);
                    break;
                case "levelup":
                    PlaySFX(sfxLevelUp);
                    break;
                case "death":
                case "gameover":
                    PlaySFX(sfxPlayerDeath);
                    break;
                case "shoot":
                case "attack":
                    PlaySFX(sfxShoot);
                    break;
                case "explosion":
                    PlayHitSound(false, true);
                    break;
                case "click":
                case "button":
                    PlaySFX(sfxButtonClick ?? sfxGemPickup);
                    break;
            }
        }

        private AudioClip GenerateProceduralHitClip()
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * 0.085f);
            float[] data = new float[samples];
            System.Random rng = new System.Random(42);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 42f);
                float freq = 180f * Mathf.Exp(-t * 28f) + 65f;
                float body = Mathf.Sin(2f * Mathf.PI * freq * t);
                float crunch = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 65f) * 0.45f;
                data[i] = Mathf.Clamp((body * 0.75f + crunch) * env, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Hit", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateProceduralMedievalBgmClip()
        {
            int sampleRate = 22050;
            float bpm = 112f;
            float beatSec = 60f / bpm;
            int totalBeats = 32; // 8-bar loop fallback
            int totalSamples = (int)(totalBeats * beatSec * sampleRate);
            float[] buffer = new float[totalSamples];

            int[][] chords = new int[][]
            {
                new int[] { 50, 53, 57, 62 }, // Dm
                new int[] { 48, 52, 55, 60 }, // C
                new int[] { 43, 47, 50, 55 }, // G
                new int[] { 45, 48, 52, 57 }, // Am
                new int[] { 46, 50, 53, 58 }, // Bb
                new int[] { 50, 53, 57, 62 }, // Dm
                new int[] { 48, 52, 55, 60 }, // C
                new int[] { 45, 49, 52, 57 }  // A7
            };

            int[] arpPattern = new int[] { 0, 1, 2, 3, 2, 1, 2, 3 };
            float sixteenthSec = beatSec * 0.25f;
            int total16ths = totalBeats * 4;

            for (int s16 = 0; s16 < total16ths; s16++)
            {
                int bar = s16 / 16;
                int step = s16 % 16;
                int[] chord = chords[bar % chords.Length];
                int midi = chord[arpPattern[step % 8]] + 12;
                float freq = 440f * Mathf.Pow(2f, (midi - 69f) / 12f);

                int start = (int)(s16 * sixteenthSec * sampleRate);
                int len = (int)(sixteenthSec * 1.6f * sampleRate);
                for (int i = 0; i < len && (start + i) < totalSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float env = Mathf.Exp(-t * 9.5f);
                    float wave = 0.7f * Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
                    buffer[start + i] += wave * env * 0.20f;
                }
            }

            AudioClip clip = AudioClip.Create("Procedural_Medieval_BGM", totalSamples, 1, sampleRate, false);
            clip.SetData(buffer, 0);
            return clip;
        }
    }
}
