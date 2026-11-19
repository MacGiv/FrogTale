namespace FrogGame.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using FrogGame.Gameplay;

    /// <summary>
    /// Debug & scene controller for restarting, quitting, and pausing using Unity's New Input System.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Input References")]
        [SerializeField] private PlayerInputHandler playerInputHandler;

        [Header("Restart Settings")]
        [SerializeField] private float deathRestartDelay = 1f;

        public bool IsPaused { get; private set; }
        public bool IsPauseLocked { get; set; }

        public event Action<bool> OnPauseToggled;

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
            if (IsPauseLocked) return;

            SetPauseState(!IsPaused);
        }

        public void SetPauseState(bool paused)
        {
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
            Invoke(nameof(RestartScene), deathRestartDelay);
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}