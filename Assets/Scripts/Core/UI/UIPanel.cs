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

        protected Button LastSelected { get; private set; }
        protected CanvasGroup CanvasGroup { get; private set; }

        protected virtual void Awake() => CanvasGroup = GetComponent<CanvasGroup>();

        public virtual void Show()
        {
            CanvasGroup.alpha = 1.0f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;

            FocusTarget(LastSelected != null ? LastSelected : defaultSelected);
        }

        public virtual void Hide()
        {
            SaveCurrentSelection();

            CanvasGroup.alpha = 0.0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        public virtual void Focus() => FocusTarget(defaultSelected);

        public virtual void FocusTarget(Button target)
        {
            if (target == null) return;

            StopAllCoroutines();
            StartCoroutine(SetFocusRoutine(target));
        }

        public void SaveCurrentSelection()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                if (EventSystem.current.currentSelectedGameObject.TryGetComponent(out Button selectable))
                {
                    LastSelected = selectable;
                }
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
        }
    }
}