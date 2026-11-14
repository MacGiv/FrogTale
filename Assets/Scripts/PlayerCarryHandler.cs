namespace FrogGame.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Handles carrying and dropping objects with obstacle and wall collision checks to prevent clipping.
    /// </summary>
    public class PlayerCarryHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform carryPoint;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private Vector2 dropBoxSize = new Vector2(0.6f, 0.6f);
        [SerializeField] private float dropDistance = 0.7f;

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
        /// Validates that the target position in facing direction is completely free of obstacles.
        /// Returns false if blocked, preventing clipping or pushing player.
        /// </summary>
        public bool TryDropBox()
        {
            if (currentCarriedBox == null) return false;

            LayerMask combinedMask = obstacleLayer | wallLayer;
            Vector2 facingDir = movementHandler != null ? movementHandler.FacingDirection : Vector2.down;

            if (facingDir == Vector2.zero)
            {
                facingDir = Vector2.down;
            }

            Vector2 targetDropPos = (Vector2)transform.position + (facingDir.normalized * dropDistance);

            // Check if the drop position in front of the player overlaps any obstacles or walls
            Collider2D obstacleHit = Physics2D.OverlapBox(targetDropPos, dropBoxSize, 0f, combinedMask);

            if (obstacleHit != null)
            {
                // Target spot is blocked by an obstacle or wall: cancel drop to prevent clipping or pushing the player
                return false;
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

        private void OnDrawGizmosSelected()
        {
            Vector2 facingDir = Vector2.down;
            if (movementHandler != null && movementHandler.FacingDirection != Vector2.zero)
            {
                facingDir = movementHandler.FacingDirection;
            }

            Vector2 targetDropPos = (Vector2)transform.position + (facingDir.normalized * dropDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(targetDropPos, dropBoxSize);
        }
    }
}