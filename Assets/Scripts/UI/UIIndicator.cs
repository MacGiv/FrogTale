namespace FrogGame.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class UIIndicator : MonoBehaviour
    {
        [Header("Sprites References")]
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;

        private Image targetImage;

        private void Awake() => targetImage = GetComponent<Image>();

        public void SetActiveState(bool isActive)
        {
            if (targetImage != null)
            {
                targetImage.sprite = isActive ? activeSprite : inactiveSprite;
            }
        }
    }
}