namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using FrogGame.Core;

    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels References")]
        [SerializeField] private MainPanelController mainPanel;
        [SerializeField] private OptionsPanelController optionsPanel;
        [SerializeField] private CreditsPanelController creditsPanel;

        private bool IsAnyPanelTransitioning =>
            (mainPanel != null && mainPanel.IsTransitioning) ||
            (optionsPanel != null && optionsPanel.IsTransitioning) ||
            (creditsPanel != null && creditsPanel.IsTransitioning);

        private void Awake() => SubscribeEvents();

        private void Start()
        {
            optionsPanel.HideImmediate();
            creditsPanel.HideImmediate();
            mainPanel.Show();
        }

        private void OnDestroy() => UnsubscribeEvents();

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            mainPanel.OnPlayPressed += HandlePlayPressed;
            mainPanel.OnOptionsPressed += ShowOptionsPanel;
            mainPanel.OnCreditsPressed += ShowCreditsPanel;
            mainPanel.OnExitPressed += HandleExitPressed;

            optionsPanel.OnBackPressed += ShowMainPanel;

            creditsPanel.OnBackPressed += ShowMainPanel;
        }

        private void UnsubscribeEvents()
        {
            mainPanel.OnPlayPressed -= HandlePlayPressed;
            mainPanel.OnOptionsPressed -= ShowOptionsPanel;
            mainPanel.OnCreditsPressed -= ShowCreditsPanel;
            mainPanel.OnExitPressed -= HandleExitPressed;

            optionsPanel.OnBackPressed -= ShowMainPanel;

            creditsPanel.OnBackPressed -= ShowMainPanel;
        }

        #endregion

        #region Panel Navigation

        private void ShowMainPanel()
        {
            if (IsAnyPanelTransitioning) return;

            optionsPanel.Hide();
            creditsPanel.Hide();
            mainPanel.Show();
        }

        private void ShowOptionsPanel()
        {
            if (IsAnyPanelTransitioning) return;

            mainPanel.Hide();
            creditsPanel.Hide();
            optionsPanel.Show();
        }

        private void ShowCreditsPanel()
        {
            if (IsAnyPanelTransitioning) return;

            mainPanel.Hide();
            optionsPanel.Hide();
            creditsPanel.Show();
        }

        #endregion

        #region Event Handlers

        private void HandlePlayPressed()
        {
            if (IsAnyPanelTransitioning) return;

            SceneManager.LoadScene(SceneNames.PLATEAU);
        }

        private void HandleExitPressed()
        {
            if (IsAnyPanelTransitioning) return;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}