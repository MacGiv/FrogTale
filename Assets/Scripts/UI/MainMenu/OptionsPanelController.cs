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

        [Header("Button References")]
        [SerializeField] private Button decreaseMusicVolumeButton;
        [SerializeField] private Button increaseMusicVolumeButton;
        [SerializeField] private Button decreaseSfxVolumeButton;
        [SerializeField] private Button increaseSfxVolumeButton;
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

            SetMusicVolume(savedMusic);
            SetSFXVolume(savedSFX);
        }

        private void OnDestroy()
        {
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