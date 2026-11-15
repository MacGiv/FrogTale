namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.SceneManagement;
    using FrogGame.Core;

    public class GameplayUIManager : MonoBehaviour
    {
        [Header("Controller References")]
        [SerializeField] private GameController gameController;

        [Header("Panels References")]
        [SerializeField] private PausePanelController pausePanel;
        [SerializeField] private OptionsPanelController optionsPanel;

        private void Awake() => SubscribeEvents();

        private void Start() => HideAllPanels();

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

        private void ShowOptionsPanel()
        {
            pausePanel.Hide();
            optionsPanel.Show();
        }

        private void HideAllPanels()
        {
            pausePanel.Hide();
            optionsPanel.Hide();
        }

        #endregion

        #region Event Handlers

        private void HandlePauseToggled(bool isPaused)
        {
            if (isPaused)
            {
                ShowPausePanel();
            }
            else
            {
                HideAllPanels();
            }
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
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}