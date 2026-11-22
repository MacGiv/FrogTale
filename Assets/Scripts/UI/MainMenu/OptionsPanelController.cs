namespace FrogGame.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using FrogGame.Core;

    public class OptionsPanelController : UIPanel
    {
        [Header("Wwise RTPC Parameters")]
        [SerializeField] private AK.Wwise.RTPC musicVolumeRTPC;
        [SerializeField] private AK.Wwise.RTPC sfxVolumeRTPC;

        [Header("Button References")]
        [SerializeField] private Button decreaseMusicVolumeButton;
        [SerializeField] private Button increaseMusicVolumeButton;
        [SerializeField] private Button decreaseSfxVolumeButton;
        [SerializeField] private Button increaseSfxVolumeButton;
        [SerializeField] private Button backButton;

        [Header("Volumen Indicators References")]
        [SerializeField] private Button[] musicIndicators;
        [SerializeField] private Button[] sfxIndicators;

        public event Action OnBackPressed;

        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        private int currentMusicLevel;
        private int currentSfxLevel;

        protected override void Awake()
        {
            base.Awake();

            decreaseMusicVolumeButton.onClick.AddListener(DecreaseMusicVolume);
            increaseMusicVolumeButton.onClick.AddListener(IncreaseMusicVolume);
            decreaseSfxVolumeButton.onClick.AddListener(DecreaseSfxVolume);
            increaseSfxVolumeButton.onClick.AddListener(IncreaseSfxVolume);

            backButton.onClick.AddListener(HandleBackClicked);
        }

        private void Start()
        {
            float savedMusic = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 1f);

            int maxMusicSteps = musicIndicators != null ? musicIndicators.Length : 0;
            int maxSfxSteps = sfxIndicators != null ? sfxIndicators.Length : 0;

            currentMusicLevel = Mathf.RoundToInt(savedMusic * maxMusicSteps);
            currentSfxLevel = Mathf.RoundToInt(savedSFX * maxSfxSteps);

            ApplyMusicLevel();
            ApplySfxLevel();
        }

        private void OnDestroy()
        {
            decreaseMusicVolumeButton.onClick.RemoveListener(DecreaseMusicVolume);
            increaseMusicVolumeButton.onClick.RemoveListener(IncreaseMusicVolume);
            decreaseSfxVolumeButton.onClick.RemoveListener(DecreaseSfxVolume);
            increaseSfxVolumeButton.onClick.RemoveListener(IncreaseSfxVolume);

            backButton.onClick.RemoveListener(HandleBackClicked);
        }

        #region Volume Navigation Logic
        private void DecreaseMusicVolume()
        {
            if (musicIndicators == null || musicIndicators.Length == 0) return;

            currentMusicLevel = Mathf.Clamp(currentMusicLevel - 1, 0, musicIndicators.Length);
            ApplyMusicLevel();
        }

        private void IncreaseMusicVolume()
        {
            if (musicIndicators == null || musicIndicators.Length == 0) return;

            currentMusicLevel = Mathf.Clamp(currentMusicLevel + 1, 0, musicIndicators.Length);
            ApplyMusicLevel();
        }

        private void DecreaseSfxVolume()
        {
            if (sfxIndicators == null || sfxIndicators.Length == 0) return;

            currentSfxLevel = Mathf.Clamp(currentSfxLevel - 1, 0, sfxIndicators.Length);
            ApplySfxLevel();
        }

        private void IncreaseSfxVolume()
        {
            if (sfxIndicators == null || sfxIndicators.Length == 0) return;

            currentSfxLevel = Mathf.Clamp(currentSfxLevel + 1, 0, sfxIndicators.Length);
            ApplySfxLevel();
        }

        private void ApplyMusicLevel()
        {
            float normalizedVolume = musicIndicators.Length > 0 ? (float)currentMusicLevel / musicIndicators.Length : 0f;

            SetMusicVolume(normalizedVolume);
            UpdateIndicators(musicIndicators, currentMusicLevel);
        }

        private void ApplySfxLevel()
        {
            float normalizedVolume = sfxIndicators.Length > 0 ? (float)currentSfxLevel / sfxIndicators.Length : 0f;

            SetSFXVolume(normalizedVolume);
            UpdateIndicators(sfxIndicators, currentSfxLevel);
        }

        private void UpdateIndicators(Button[] indicators, int activeCount)
        {
            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i] != null)
                {
                    indicators[i].interactable = (i < activeCount);
                }
            }
        }
        #endregion

        private void SetMusicVolume(float normalizedValue)
        {
            float clamped = Mathf.Clamp01(normalizedValue);

            if (musicVolumeRTPC != null && musicVolumeRTPC.IsValid())
            {
                musicVolumeRTPC.SetGlobalValue(clamped * 100f);
            }

            PlayerPrefs.SetFloat(MUSIC_KEY, clamped);
        }

        private void SetSFXVolume(float normalizedValue)
        {
            float clamped = Mathf.Clamp01(normalizedValue);

            if (sfxVolumeRTPC != null && sfxVolumeRTPC.IsValid())
            {
                sfxVolumeRTPC.SetGlobalValue(clamped * 100f);
            }

            PlayerPrefs.SetFloat(SFX_KEY, clamped);
        }

        private void HandleBackClicked() => OnBackPressed?.Invoke();
    }
}