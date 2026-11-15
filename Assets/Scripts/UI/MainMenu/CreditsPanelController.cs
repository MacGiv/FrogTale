namespace FrogGame.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using FrogGame.Core;

    public class CreditsPanelController : UIPanel
    {
        [Header("Button References")]
        [SerializeField] private Button backButton;

        [Header("Pages References")]
        [SerializeField] private GameObject[] pages;

        [Header("Input Navigation")]
        [SerializeField] private InputActionReference navigateAction;

        public event Action OnBackPressed;

        private int currentPageIndex = 0;
        private bool isNavigating = false;

        protected override void Awake()
        {
            base.Awake();

            backButton.onClick.AddListener(HandleBackClicked);
        }

        private void OnDestroy() => backButton.onClick.RemoveListener(HandleBackClicked);

        private void OnEnable()
        {
            if (navigateAction != null)
            {
                navigateAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (navigateAction != null)
            {
                navigateAction.action.Disable();
            }
        }

        private void Update()
        {
            if (CanvasGroup == null || CanvasGroup.alpha < 0.9f) return;

            if (navigateAction != null)
            {
                Vector2 navInput = navigateAction.action.ReadValue<Vector2>();

                if (navInput.x > 0.5f)
                {
                    if (!isNavigating)
                    {
                        NextPage();

                        isNavigating = true;
                    }
                }
                else if (navInput.x < -0.5f)
                {
                    if (!isNavigating)
                    {
                        PreviousPage();

                        isNavigating = true;
                    }
                }
                else
                {
                    isNavigating = false;
                }
            }
        }

        public override void Show()
        {
            base.Show();

            currentPageIndex = 0;
            UpdatePageVisibility();
        }

        #region Navigation Logic

        public void NextPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex = (currentPageIndex + 1) % pages.Length;

            UpdatePageVisibility();
        }

        public void PreviousPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex--;

            if (currentPageIndex < 0)
            {
                currentPageIndex = pages.Length - 1;
            }

            UpdatePageVisibility();
        }

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

        #endregion

        private void HandleBackClicked() => OnBackPressed?.Invoke();
    }
}