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
            UpdatePageVisibility();
        }

        #region Pages Navigation Logic

        private void UpdatePageVisibility()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == currentPageIndex);
                }
            }
        }

        private void PreviousPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex--;

            if (currentPageIndex < 0)
            {
                currentPageIndex = pages.Length - 1;
            }

            UpdatePageVisibility();
        }

        private void NextPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex = (currentPageIndex + 1) % pages.Length;

            UpdatePageVisibility();
        }

        #endregion

        private void HandleBackClicked() => OnBackPressed?.Invoke();
    }
}