namespace FrogGame.World
{
    using UnityEngine;
    using UnityEngine.Events;
    using FrogGame.Gameplay;

    /// <summary>
    /// Pressure plate that triggers events when stepped on by player or pushable blocks.
    /// Snaps and locks grabbable items/stones to its center upon contact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FloorSwitch : MonoBehaviour
    {
        [Header("Switch Logic")]
        [SerializeField] private bool stayPressed = false;
        [SerializeField] private bool lockStonesOnCenter = true;
        [SerializeField] private LayerMask triggerLayers;

        [Header("Events")]
        [SerializeField] private UnityEvent onPressed;
        [SerializeField] private UnityEvent onReleased;

        [Header("Visual Feedback (Optional)")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] private Sprite unpressedSprite;

        private int occupantsCount = 0;
        public bool IsPressed => occupantsCount > 0;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            HandleCollision(collision);
        }

        private void HandleCollision(Collider2D collision)
        {
            int colLayer = 1 << collision.gameObject.layer;

            if ((colLayer & triggerLayers) != 0)
            {
                GrabbableItem grabbable = collision.GetComponent<GrabbableItem>();
                if (grabbable == null)
                {
                    grabbable = collision.GetComponentInParent<GrabbableItem>();
                }

                if (grabbable != null && lockStonesOnCenter && !grabbable.IsLockedOnSwitch && !grabbable.IsBeingPulled)
                {
                    grabbable.LockToSwitch(transform.position);
                }

                if (occupantsCount == 0)
                {
                    occupantsCount = 1;
                    OnSwitchPressed();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (stayPressed) return;

            int colLayer = 1 << collision.gameObject.layer;

            if ((colLayer & triggerLayers) != 0)
            {
                occupantsCount--;
                occupantsCount = Mathf.Max(0, occupantsCount);

                if (occupantsCount == 0)
                {
                    OnSwitchReleased();
                }
            }
        }

        private void OnSwitchPressed()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnvRockPushButton(gameObject);
            }

            if (spriteRenderer != null && pressedSprite != null)
            {
                spriteRenderer.sprite = pressedSprite;
            }

            onPressed?.Invoke();
        }

        private void OnSwitchReleased()
        {
            if (spriteRenderer != null && unpressedSprite != null)
            {
                spriteRenderer.sprite = unpressedSprite;
            }

            onReleased?.Invoke();
        }
    }
}