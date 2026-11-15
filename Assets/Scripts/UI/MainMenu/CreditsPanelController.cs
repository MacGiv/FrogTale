namespace FrogGame.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using FrogGame.Core;

    public class CreditsPanelController : UIPanel
    {
        [Header("Button References")]
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button backButton;

        [Header("Pages References")]
        [SerializeField] private GameObject[] pages;

        [Header("Indicators References")]
        [SerializeField] private Button[] indicators;

        public event Action OnBackPressed;

        private int currentPageIndex = 0;
        private bool isNavigating = false;

        protected override void Awake()
        {
            base.Awake();

            previousPageButton.onClick.AddListener(PreviousPage);
            nextPageButton.onClick.AddListener(NextPage);
            backButton.onClick.AddListener(HandleBackClicked);
        }

        private void OnDestroy()
        {
            previousPageButton.onClick.RemoveListener(PreviousPage);
            nextPageButton.onClick.RemoveListener(NextPage);
            backButton.onClick.RemoveListener(HandleBackClicked);
        }

        public override void Show()
        {
            base.Show();

            currentPageIndex = 0;

            UpdateUIPage();
        }

        #region Pages Navigation Logic

        private void PreviousPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex--;

            if (currentPageIndex < 0)
            {
                currentPageIndex = pages.Length - 1;
            }

            UpdateUIPage();
        }

        private void NextPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex++;

            if (currentPageIndex > pages.Length - 1)
            {
                currentPageIndex = 0;
            }

            UpdateUIPage();
        }

        private void UpdateUIPage()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == currentPageIndex);
                }
            }

            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i] != null)
                {
                    indicators[i].interactable = (i == currentPageIndex);
                }
            }
        }

        #endregion

        private void HandleBackClicked() => OnBackPressed?.Invoke();
    }
}