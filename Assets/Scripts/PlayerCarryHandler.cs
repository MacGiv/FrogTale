namespace FrogGame.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Handles carrying and dropping objects with obstacle collision checks to prevent wall clipping.
    /// </summary>
    public class PlayerCarryHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform carryPoint;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Vector2 dropBoxSize = new Vector2(0.5f, 0.5f);
        [SerializeField] private float dropDistance = 0.6f;

        private GrabbableItem currentCarriedBox;
        private PlayerMovementHandler movementHandler;

        public bool IsCarrying => currentCarriedBox != null;

        private void Awake()
        {
            movementHandler = GetComponent<PlayerMovementHandler>();
        }

        public void AttachBox(GrabbableItem box)
        {
            if (box == null || box.IsLockedOnSwitch) return;

            currentCarriedBox = box;

            if (carryPoint != null)
            {
                currentCarriedBox.transform.SetParent(carryPoint);
                currentCarriedBox.transform.localPosition = Vector3.zero;
            }
            else
            {
                currentCarriedBox.transform.SetParent(transform);
                currentCarriedBox.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            }
        }

        /// <summary>
        /// Attempts to drop the carried box in front of the player. 
        /// If blocked by an obstacle/wall, drops it safely at the player's current feet position.
        /// </summary>
        public bool TryDropBox()
        {
            if (currentCarriedBox == null) return false;

            Vector2 facingDir = movementHandler != null ? movementHandler.FacingDirection : Vector2.down;
            Vector2 targetDropPos = (Vector2)transform.position + (facingDir * dropDistance);

            Collider2D obstacleHit = Physics2D.OverlapBox(targetDropPos, dropBoxSize, 0f, obstacleLayer);

            if (obstacleHit != null)
            {
                targetDropPos = transform.position;
            }

            currentCarriedBox.transform.SetParent(null);
            currentCarriedBox.transform.position = targetDropPos;

            if (currentCarriedBox.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero;
            }

            if (currentCarriedBox.ItemCollider != null)
            {
                currentCarriedBox.ItemCollider.enabled = true;
            }

            currentCarriedBox = null;
            return true;
        }
    }
}