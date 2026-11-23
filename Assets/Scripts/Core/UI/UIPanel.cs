namespace FrogGame.Core
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        [Header("Focus Button Setup")]
        [SerializeField] protected Button defaultSelected;

        [Header("Transition General Settings")]
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Fade Settings")]
        [SerializeField] private bool useFade = false;
        [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0f;

        [Header("Slide Settings")]
        [SerializeField] private bool useSlide = false;
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, 0f);

        [Header("Scale Settings")]
        [SerializeField] private bool useScale = false;
        [SerializeField] private Vector3 hiddenScale = new Vector3(1f, 1f, 1f);

        public bool IsTransitioning => transitionCoroutine != null;

        protected Button LastSelected { get; private set; }
        protected CanvasGroup CanvasGroup { get; private set; }

        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private Vector3 originalScale;
        private Coroutine transitionCoroutine;
        private Coroutine focusCoroutine;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();

            originalPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
        }

        public virtual void Show()
        {
            StartTransition(true);

            if (transitionDuration <= 0f)
            {
                FocusTarget(LastSelected != null ? LastSelected : defaultSelected);
            }
        }

        public virtual void ShowImmediate()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            ApplyVisualState(true);
            FocusTarget(LastSelected != null ? LastSelected : defaultSelected);
        }

        public virtual void Hide()
        {
            SaveCurrentSelection();

            StartTransition(false);
        }

        public virtual void HideImmediate()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            ApplyVisualState(false);
        }

        public virtual void Focus() => FocusTarget(defaultSelected);

        public virtual void FocusTarget(Button target)
        {
            if (target == null) return;

            if (focusCoroutine != null)
            {
                StopCoroutine(focusCoroutine);
            }

            focusCoroutine = StartCoroutine(SetFocusRoutine(target));
        }

        private void SaveCurrentSelection()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                Transform currentSelectedTransform = EventSystem.current.currentSelectedGameObject.transform;

                if (currentSelectedTransform.IsChildOf(transform))
                {
                    if (EventSystem.current.currentSelectedGameObject.TryGetComponent(out Button selectable))
                    {
                        LastSelected = selectable;
                    }
                }
            }
        }

        private void StartTransition(bool showing)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            if (transitionDuration <= 0f)
            {
                ApplyVisualState(showing);

                return;
            }

            transitionCoroutine = StartCoroutine(TransitionRoutine(showing));
        }

        private void ApplyVisualState(bool showing)
        {
            CanvasGroup.interactable = showing;
            CanvasGroup.blocksRaycasts = showing;

            CanvasGroup.alpha = useFade ? (showing ? 1f : hiddenAlpha) : 1f;
            rectTransform.anchoredPosition = useSlide ? (showing ? originalPosition : originalPosition + hiddenOffset) : originalPosition;
            rectTransform.localScale = useScale ? (showing ? originalScale : hiddenScale) : originalScale;
        }

        private IEnumerator TransitionRoutine(bool showing)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            float startAlpha = CanvasGroup.alpha;
            float endAlpha = useFade ? (showing ? 1f : hiddenAlpha) : 1f;

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = useSlide ? (showing ? originalPosition : originalPosition + hiddenOffset) : originalPosition;

            Vector3 startScale = rectTransform.localScale;
            Vector3 endScale = useScale ? (showing ? originalScale : hiddenScale) : originalScale;

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float time = Mathf.Clamp01(elapsed / transitionDuration);
                float curvedTime = easeCurve.Evaluate(time);

                if (useFade)
                {
                    CanvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, curvedTime);
                }

                if (useSlide)
                {
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curvedTime);
                }

                if (useScale)
                {
                    rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curvedTime);
                }

                yield return null;
            }

            ApplyVisualState(showing);
            transitionCoroutine = null;

            if (showing)
            {
                FocusTarget(LastSelected != null ? LastSelected : defaultSelected);
            }
        }

        private IEnumerator SetFocusRoutine(Button target)
        {
            EventSystem.current?.SetSelectedGameObject(null);

            yield return new WaitForEndOfFrame();

            if (target != null && target.gameObject.activeInHierarchy)
            {
                EventSystem.current?.SetSelectedGameObject(target.gameObject);
            }

            focusCoroutine = null;
        }
    }
}