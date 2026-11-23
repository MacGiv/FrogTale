namespace FrogGame.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using FrogGame.Gameplay;

    public class GameController : MonoBehaviour
    {
        [Header("Input References")]
        [SerializeField] private PlayerInputHandler playerInputHandler;

        [Header("Game Over Settings")]
        [SerializeField] private float gameOverDelay = 1f;

        public bool IsPaused { get; private set; }
        public bool IsPauseLocked { get; set; }
        public bool IsGameOver { get; private set; }

        public event Action<bool> OnPauseToggled;
        public event Action OnGameOver;

        private void Start()
        {
            if (playerInputHandler != null)
            {
                playerInputHandler.EnableInput();
                playerInputHandler.DisableUIInput();
            }
        }

        private void OnEnable()
        {
            PlayerHealthSystem.OnPlayerDied += HandlePlayerDied;

            if (playerInputHandler != null)
            {
                playerInputHandler.OnPausePressed += TogglePause;
            }
        }

        private void OnDisable()
        {
            PlayerHealthSystem.OnPlayerDied -= HandlePlayerDied;

            if (playerInputHandler != null)
            {
                playerInputHandler.OnPausePressed -= TogglePause;
            }
        }

        public void TogglePause()
        {
            if (IsPauseLocked || IsGameOver) return;

            SetPauseState(!IsPaused);
        }

        public void SetPauseState(bool paused)
        {
            if (IsGameOver) return;

            IsPaused = paused;
            Time.timeScale = IsPaused ? 0f : 1f;

            if (IsPaused)
            {
                playerInputHandler.EnableUIInput();
                playerInputHandler.DisableInput();
            }
            else
            {
                playerInputHandler.DisableUIInput();
                playerInputHandler.EnableInput();
            }

            OnPauseToggled?.Invoke(IsPaused);
        }

        private void HandlePlayerDied()
        {
            if (IsGameOver) return;

            IsGameOver = true;
            IsPauseLocked = true;

            if (playerInputHandler != null)
            {
                playerInputHandler.DisableInput();
                playerInputHandler.EnableUIInput();
            }

            Invoke(nameof(TriggerGameOver), gameOverDelay);
        }

        private void TriggerGameOver()
        {
            Time.timeScale = 0f;
            OnGameOver?.Invoke();
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}