namespace FrogGame.Gameplay
{
    using UnityEngine;
    using FrogGame.World;

    /// <summary>
    /// Handles carrying and dropping objects with obstacle, wall, and occupied-switch checks to prevent clipping or stacking stones.
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
        /// Validates that the target position is free of obstacles, walls, other stones, and already-occupied switches.
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

            // 1. Check if the drop position overlaps any obstacles or walls
            Collider2D obstacleHit = Physics2D.OverlapBox(targetDropPos, dropBoxSize, 0f, combinedMask);
            if (obstacleHit != null)
            {
                return false;
            }

            // 2. Prevent stacking two stones on top of each other or dropping onto an already-occupied FloorSwitch
            Collider2D[] overlapHits = Physics2D.OverlapBoxAll(targetDropPos, dropBoxSize, 0f);
            for (int i = 0; i < overlapHits.Length; i++)
            {
                Collider2D hit = overlapHits[i];
                if (hit == null) continue;

                GrabbableItem existingItem = hit.GetComponent<GrabbableItem>() ?? hit.GetComponentInParent<GrabbableItem>();
                if (existingItem != null && existingItem != currentCarriedBox)
                {
                    return false;
                }

                FloorSwitch floorSwitch = hit.GetComponent<FloorSwitch>() ?? hit.GetComponentInParent<FloorSwitch>();
                if (floorSwitch != null && floorSwitch.HasLockedItem)
                {
                    return false;
                }
            }

            currentCarriedBox.transform.SetParent(null);
            currentCarriedBox.transform.position = targetDropPos;
            currentCarriedBox.SetGroundedPhysics();

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