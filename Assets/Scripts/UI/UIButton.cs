namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Header("Target Image")]
        [SerializeField] private Image targetImage;

        [Header("Colors Setup")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage != null)
            {
                targetImage.color = normalColor;
            }

            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy() => button.onClick.RemoveListener(HandleButtonClicked);

        private void OnDisable()
        {
            if (targetImage != null)
            {
                targetImage.color = normalColor;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!button.IsInteractable()) return;

            if (targetImage != null)
            {
                targetImage.color = selectedColor;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIHoverButton();
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (targetImage != null)
            {
                targetImage.color = normalColor;
            }
        }

        private void HandleButtonClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClickButton();
            }
        }
    }
}