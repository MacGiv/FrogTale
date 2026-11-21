namespace FrogGame.World
{
    using UnityEngine;

    /// <summary>
    /// Interactive puzzle door supporting 2x2 downward doors (with 1x2 top frame when open)
    /// and 2x1 side doors (2 rows x 1 column).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Door : MonoBehaviour
    {
        public enum DoorOrientation
        {
            Down_2x2,
            Left_2x1
        }

        [Header("Puzzle Settings")]
        [SerializeField] private int requiredSwitches = 1;
        [SerializeField] private bool isOpen = false;

        [Header("Orientation & Grid Layout")]
        [SerializeField] private DoorOrientation orientation = DoorOrientation.Down_2x2;
        [SerializeField] private bool flipHorizontally = false;
        [Tooltip("World units per single grid tile (default 1).")]
        [SerializeField] private float tileUnitSize = 1f;

        [Header("Down Door Sprites (2x2 Closed / 1x2 Top Frame Open)")]
        [SerializeField] private Sprite closedDownSprite;
        [SerializeField] private Sprite openDownFrameSprite;

        [Header("Left Door Sprites (2x1 Closed / Frame Open)")]
        [SerializeField] private Sprite closedLeftSprite;
        [SerializeField] private Sprite openLeftFrameSprite;

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Collider2D doorCollider;
        private int currentActiveSwitches = 0;

        /// <summary>
        /// Returns collider dimensions in world units (Width x Height) based on Row x Column orientation.
        /// Down_2x2 = 2 wide x 2 high | Left_2x1 = 1 wide x 2 high.
        /// </summary>
        public Vector2 ClosedColliderDimensions => orientation == DoorOrientation.Left_2x1
            ? new Vector2(1f * tileUnitSize, 2f * tileUnitSize)
            : new Vector2(2f * tileUnitSize, 2f * tileUnitSize);

        private void Awake()
        {
            doorCollider = GetComponent<Collider2D>();
            ApplyTileLayout();
            UpdateDoorState();
        }

        private void OnValidate()
        {
            tileUnitSize = Mathf.Max(0.1f, tileUnitSize);
            ApplyTileLayout();
            UpdateDoorState();
        }

        /// <summary>
        /// Synchronizes BoxCollider2D bounds and sprite orientation with the selected Row x Column layout.
        /// </summary>
        public void ApplyTileLayout()
        {
            Vector2 dimensions = ClosedColliderDimensions;

            if (doorCollider == null)
            {
                doorCollider = GetComponent<Collider2D>();
            }

            if (doorCollider is BoxCollider2D boxCollider && !boxCollider.autoTiling)
            {
                boxCollider.size = dimensions;
                boxCollider.offset = Vector2.zero;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = flipHorizontally;
            }
        }

        /// <summary>
        /// Call this when any connected switch gets pressed.
        /// </summary>
        public void RegisterSwitchPressed()
        {
            currentActiveSwitches++;
            EvaluateState();
        }

        /// <summary>
        /// Call this when any connected switch gets released.
        /// </summary>
        public void RegisterSwitchReleased()
        {
            currentActiveSwitches = Mathf.Max(0, currentActiveSwitches - 1);
            EvaluateState();
        }

        private void EvaluateState()
        {
            bool shouldBeOpen = currentActiveSwitches >= requiredSwitches;

            if (shouldBeOpen != isOpen)
            {
                isOpen = shouldBeOpen;
                UpdateDoorState();

                if (isOpen && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnvDoorsOpen(gameObject);
                }
            }
        }

        private void UpdateDoorState()
        {
            if (doorCollider == null)
            {
                doorCollider = GetComponent<Collider2D>();
            }

            if (doorCollider != null)
            {
                doorCollider.enabled = !isOpen;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = flipHorizontally;
                spriteRenderer.sprite = GetActiveSprite();

                // If the SpriteRenderer lives on a child object, offset the 1x2 open top frame up by half a tile
                // so it aligns with the upper row of the 2x2 doorway.
                if (spriteRenderer.transform != transform)
                {
                    bool needsTopRowOffset = isOpen && orientation == DoorOrientation.Down_2x2;
                    spriteRenderer.transform.localPosition = needsTopRowOffset
                        ? new Vector3(0f, 0.5f * tileUnitSize, 0f)
                        : Vector3.zero;
                }
            }
        }

        private Sprite GetActiveSprite()
        {
            if (orientation == DoorOrientation.Left_2x1)
            {
                return isOpen ? openLeftFrameSprite : closedLeftSprite;
            }

            return isOpen ? openDownFrameSprite : closedDownSprite;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isOpen ? Color.green : Color.red;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, ClosedColliderDimensions);
            Gizmos.matrix = oldMatrix;
        }
    }
}