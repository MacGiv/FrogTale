namespace FrogGame.UI
{
    using System;
    using FrogGame.Core;
    using UnityEngine;
    using UnityEngine.UI;

    public class OptionsPanelController : UIPanel
    {
        [Header("Wwise RTPC Parameters")]
        [SerializeField] private AK.Wwise.RTPC musicVolumeRTPC;
        [SerializeField] private AK.Wwise.RTPC sfxVolumeRTPC;

        [Header("UI Sliders (Opcional)")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Button References")]
        [SerializeField] private Button backButton;

        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        public event Action OnBackPressed;

        protected override void Awake()
        {
            base.Awake();

            backButton.onClick.AddListener(HandleBackClicked);
        }

        private void Start()
        {
            float savedMusic = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 1f);

            if (musicSlider != null)
            {
                musicSlider.value = savedMusic;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = savedSFX;
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            SetMusicVolume(savedMusic);
            SetSFXVolume(savedSFX);
        }

        private void OnDestroy()
        {
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);

            backButton.onClick.RemoveListener(HandleBackClicked);
        }

        public void SetMusicVolume(float normalizedValue)
        {
            float clamped = Mathf.Clamp01(normalizedValue);

            if (musicVolumeRTPC != null && musicVolumeRTPC.IsValid())
            {
                musicVolumeRTPC.SetGlobalValue(clamped * 100f);
            }

            PlayerPrefs.SetFloat(MUSIC_KEY, clamped);
        }

        public void SetSFXVolume(float normalizedValue)
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