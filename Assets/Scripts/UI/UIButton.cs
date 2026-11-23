namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour, ISelectHandler
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!button.IsInteractable()) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIHoverButton();
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