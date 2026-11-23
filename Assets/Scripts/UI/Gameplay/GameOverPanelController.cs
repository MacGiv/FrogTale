namespace FrogGame.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using FrogGame.Core;

    public class GameOverPanelController : UIPanel
    {
        [Header("Button References")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        public event Action OnPlayAgainPressed;
        public event Action OnMainMenuPressed;

        protected override void Awake()
        {
            base.Awake();

            playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }

        private void OnDestroy()
        {
            playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }

        private void HandlePlayAgainClicked() => OnPlayAgainPressed?.Invoke();

        private void HandleMainMenuClicked() => OnMainMenuPressed?.Invoke();
    }
}