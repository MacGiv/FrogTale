namespace FrogGame.UI
{
    using System;
    using System.Collections;
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
        [SerializeField] private UIIndicator[] indicators;

        [Header("Carousel Settings")]
        [SerializeField] private float slideDuration = 0.25f;

        public event Action OnBackPressed;

        private RectTransform canvasRect;
        private RectTransform[] pagesRects;
        private int currentPageIndex = 0;
        private bool isNavigating = false;
        private Coroutine slideCoroutine;

        protected override void Awake()
        {
            base.Awake();

            Canvas parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                canvasRect = parentCanvas.GetComponent<RectTransform>();
            }

            if (pages != null)
            {
                pagesRects = new RectTransform[pages.Length];

                for (int i = 0; i < pages.Length; i++)
                {
                    if (pages[i] != null)
                    {
                        pagesRects[i] = pages[i].GetComponent<RectTransform>();
                    }
                }
            }

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

            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }

            isNavigating = false;
            currentPageIndex = 0;

            UpdateUIPage();
        }

        public override void Hide()
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }

            isNavigating = false;
            UpdateUIPage();

            base.Hide();
        }

        #region Pages Navigation Logic

        private void PreviousPage()
        {
            if (isNavigating || pages == null || pages.Length == 0) return;

            int previousIndex = currentPageIndex;
            currentPageIndex = GetWrappedIndex(currentPageIndex - 1);

            slideCoroutine = StartCoroutine(SlidePagesRoutine(previousIndex, currentPageIndex, -1));
        }

        private void NextPage()
        {
            if (isNavigating || pages == null || pages.Length == 0) return;

            int previousIndex = currentPageIndex;
            currentPageIndex = GetWrappedIndex(currentPageIndex + 1);

            slideCoroutine = StartCoroutine(SlidePagesRoutine(previousIndex, currentPageIndex, 1));
        }

        private void UpdateUIPage()
        {
            if (pages == null || pages.Length == 0) return;

            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == null)
                {
                    continue;
                }

                bool isCurrent = (i == currentPageIndex);

                pages[i].SetActive(isCurrent);

                if (isCurrent)
                {
                    SetPagePositionX(i, 0f);
                }
            }

            UpdateIndicators();
        }

        private IEnumerator SlidePagesRoutine(int fromIndex, int toIndex, int direction)
        {
            isNavigating = true;
            UpdateIndicators();

            float spacing = GetCanvasWidth();
            int incomingEdgeIndex = GetWrappedIndex(toIndex + direction);

            if (pages.Length >= 3 && incomingEdgeIndex != fromIndex)
            {
                pages[incomingEdgeIndex].SetActive(true);
                SetPagePositionX(incomingEdgeIndex, direction * spacing * 2f);
            }

            pages[fromIndex].SetActive(true);
            pages[toIndex].SetActive(true);

            float fromStartX = 0f;
            float fromEndX = -direction * spacing;

            float toStartX = direction * spacing;
            float toEndX = 0f;

            float edgeStartX = direction * spacing * 2f;
            float edgeEndX = direction * spacing;

            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;

                float time = Mathf.Clamp01(elapsed / slideDuration);
                float smoothT = time * time * (3f - 2f * time);

                SetPagePositionX(fromIndex, Mathf.Lerp(fromStartX, fromEndX, smoothT));
                SetPagePositionX(toIndex, Mathf.Lerp(toStartX, toEndX, smoothT));

                if (pages.Length >= 3 && incomingEdgeIndex != fromIndex)
                {
                    SetPagePositionX(incomingEdgeIndex, Mathf.Lerp(edgeStartX, edgeEndX, smoothT));
                }

                yield return null;
            }

            UpdateUIPage();
            isNavigating = false;
        }

        private void UpdateIndicators()
        {
            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i] != null)
                {
                    indicators[i].SetActiveState(i == currentPageIndex);
                }
            }
        }

        private float GetCanvasWidth() => canvasRect != null ? canvasRect.rect.width : Screen.width;

        private void SetPagePositionX(int index, float posX)
        {
            if (pagesRects != null && pagesRects[index] != null)
            {
                Vector2 pos = pagesRects[index].anchoredPosition;
                pos.x = posX;

                pagesRects[index].anchoredPosition = pos;
            }
        }

        private int GetWrappedIndex(int index)
        {
            int total = pages.Length;

            if (index < 0)
            {
                return index + total;
            }

            if (index >= total)
            {
                return index - total;
            }

            return index;
        }

        #endregion

        private void HandleBackClicked() => OnBackPressed?.Invoke();
    }
}