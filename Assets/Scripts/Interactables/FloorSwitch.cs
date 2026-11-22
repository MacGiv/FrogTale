namespace FrogGame.World
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using FrogGame.Gameplay;

    /// <summary>
    /// Pressure plate that triggers events when stepped on by player or pushable blocks.
    /// Locks at most one grabbable item/stone to its center so a second stone cannot soft-lock multi-switch doors.
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

        private readonly HashSet<Collider2D> activeOccupants = new HashSet<Collider2D>();
        private GrabbableItem lockedItem;
        private bool isPressedState = false;

        public bool HasLockedItem => lockedItem != null;
        public bool IsPressed => isPressedState;

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
            if ((colLayer & triggerLayers) == 0) return;

            GrabbableItem grabbable = collision.GetComponent<GrabbableItem>();
            if (grabbable == null)
            {
                grabbable = collision.GetComponentInParent<GrabbableItem>();
            }

            if (grabbable != null)
            {
                // Ignore stones currently flying back on the player's tongue
                if (grabbable.IsBeingPulled) return;

                // Lock ONLY if this switch has no locked stone yet and this stone isn't locked elsewhere
                if (lockStonesOnCenter && !grabbable.IsLockedOnSwitch && lockedItem == null)
                {
                    lockedItem = grabbable;
                    grabbable.LockToSwitch(transform.position);
                }

                // If another stone is already locked on this switch, do NOT lock this second stone
                if (lockedItem != null && lockedItem != grabbable)
                {
                    return;
                }
            }

            activeOccupants.Add(collision);
            EvaluateSwitchState();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            int colLayer = 1 << collision.gameObject.layer;
            if ((colLayer & triggerLayers) == 0) return;

            activeOccupants.Remove(collision);

            if (stayPressed) return;

            EvaluateSwitchState();
        }

        private void EvaluateSwitchState()
        {
            activeOccupants.RemoveWhere(col => col == null || !col.enabled || !col.gameObject.activeInHierarchy);

            bool shouldBePressed = lockedItem != null || activeOccupants.Count > 0;

            if (shouldBePressed && !isPressedState)
            {
                isPressedState = true;
                OnSwitchPressed();
            }
            else if (!shouldBePressed && isPressedState && !stayPressed)
            {
                isPressedState = false;
                OnSwitchReleased();
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