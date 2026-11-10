namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(Button))]
    public class ButtonSelectedColor : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [Header("Target Image")]
        [SerializeField] private Image targetImage;

        [Header("Colors Setup")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage != null)
            {
                targetImage.color = normalColor;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (targetImage != null)
            {
                targetImage.color = selectedColor;
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (targetImage != null)
            {
                targetImage.color = normalColor;
            }
        }

        private void OnDisable()
        {
            if (targetImage != null)
            {
                targetImage.color = normalColor;
            }
        }
    }
}