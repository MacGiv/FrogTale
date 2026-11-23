namespace FrogGame.UI
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using FrogGame.Core;

    public class GameplayUIManager : MonoBehaviour
    {
        [Header("Controller References")]
        [SerializeField] private GameController gameController;

        [Header("Pause Background References")]
        [SerializeField] private GameObject pauseBackground;
        [SerializeField] private Animator backgroundAnimator;

        [Header("Panels References")]
        [SerializeField] private PausePanelController pausePanel;
        [SerializeField] private OptionsPanelController optionsPanel;

        private Coroutine pauseIntroCoroutine;

        private void Awake() => SubscribeEvents();

        private void Start()
        {
            if (pauseBackground != null)
            {
                pauseBackground.SetActive(false);
            }

            HideAllPanels();
        }

        private void OnDestroy() => UnsubscribeEvents();

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            if (gameController != null)
            {
                gameController.OnPauseToggled += HandlePauseToggled;
            }

            pausePanel.OnContinuePressed += HandleContinuePressed;
            pausePanel.OnOptionsPressed += ShowOptionsPanel;
            pausePanel.OnQuitToMenuPressed += HandleQuitToMenuPressed;

            optionsPanel.OnBackPressed += ShowPausePanel;
        }

        private void UnsubscribeEvents()
        {
            if (gameController != null)
            {
                gameController.OnPauseToggled -= HandlePauseToggled;
            }

            pausePanel.OnContinuePressed -= HandleContinuePressed;
            pausePanel.OnOptionsPressed -= ShowOptionsPanel;
            pausePanel.OnQuitToMenuPressed -= HandleQuitToMenuPressed;

            optionsPanel.OnBackPressed -= ShowPausePanel;
        }

        #endregion

        #region Panel Navigation

        private void ShowPausePanel()
        {
            pausePanel.Show();
            optionsPanel.Hide();
        }

        private void ShowPausePanelImmediate()
        {
            pausePanel.ShowImmediate();
            optionsPanel.Hide();
        }

        private void ShowOptionsPanel()
        {
            pausePanel.Hide();
            optionsPanel.Show();
        }

        private void HideAllPanels()
        {
            pausePanel.HideImmediate();
            optionsPanel.HideImmediate();
        }

        #endregion

        #region Event Handlers

        private void HandlePauseToggled(bool isPaused)
        {
            if (pauseIntroCoroutine != null)
            {
                StopCoroutine(pauseIntroCoroutine);

                pauseIntroCoroutine = null;
            }

            if (isPaused)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPlayerBoogerShot();
                    MusicManager.Instance.PauseMusic();
                }

                pauseIntroCoroutine = StartCoroutine(PlayPauseIntroRoutine());
            }
            else
            {
                if (AudioManager.Instance != null)
                {
                    MusicManager.Instance.ResumeMusic();
                }

                if (gameController != null)
                {
                    gameController.IsPauseLocked = false;
                }

                if (pauseBackground != null)
                {
                    pauseBackground.SetActive(false);
                }

                HideAllPanels();
            }
        }

        private IEnumerator PlayPauseIntroRoutine()
        {
            if (gameController != null)
            {
                gameController.IsPauseLocked = true;
            }

            HideAllPanels();

            if (pauseBackground != null)
            {
                pauseBackground.SetActive(true);
            }

            if (backgroundAnimator != null)
            {
                backgroundAnimator.Play("PauseBackground", 0, 0f);
                yield return null;

                float animDuration = backgroundAnimator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSecondsRealtime(animDuration);
            }

            ShowPausePanelImmediate();

            if (gameController != null)
            {
                gameController.IsPauseLocked = false;
            }

            pauseIntroCoroutine = null;
        }

        private void HandleContinuePressed()
        {
            if (gameController != null)
            {
                gameController.SetPauseState(false);
            }
        }

        private void HandleQuitToMenuPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.MAIN_MENU);
        }

        #endregion
    }
}