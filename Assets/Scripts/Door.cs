namespace FrogGame.World
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Interactive puzzle door driven natively by a Unity Animator ("IsOpen" bool),
    /// blocking player passage until the opening animation finishes and elevating open-frame sorting order.
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

        [Header("Animator & Passage Timing")]
        [SerializeField] private Animator doorAnimator;
        [Tooltip("Seconds to wait after triggering the open animation before disabling the collider (match your 6-frame clip duration, e.g. 0.45s).")]
        [Min(0f)]
        [SerializeField] private float openPassageDelay = 0.45f;

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Enable only if the open sprite is a 1x2 top arch on a child object that needs to shift up half a tile.")]
        [SerializeField] private bool offsetOpenDownChildSprite = false;

        [Header("Sorting Order Settings")]
        [Tooltip("Elevates the open door frame sprite above the player sprite once the door is passable.")]
        [SerializeField] private bool elevateOpenFrameSorting = true;
        [Tooltip("Optional Sorting Layer name for the open frame (e.g. Player or Foreground). Leave empty to keep current layer.")]
        [SerializeField] private string openFrameSortingLayer = "";
        [Tooltip("Order in Layer applied to the open door frame so it renders above the player.")]
        [SerializeField] private int openFrameSortingOrder = 20;

        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

        private Collider2D doorCollider;
        private Coroutine passageCoroutine;
        private int currentActiveSwitches = 0;

        private int closedSortingLayerID;
        private int closedSortingOrder;
        private bool hasCachedClosedSorting = false;

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

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (doorAnimator == null)
            {
                doorAnimator = GetComponentInChildren<Animator>();
            }

            CacheClosedSorting();
            ApplyTileLayout();
            ApplyImmediateState(isOpen);
        }

        private void OnValidate()
        {
            tileUnitSize = Mathf.Max(0.1f, tileUnitSize);
            CacheClosedSorting();
            ApplyTileLayout();
        }

        private void CacheClosedSorting()
        {
            if (spriteRenderer != null && !hasCachedClosedSorting)
            {
                closedSortingLayerID = spriteRenderer.sortingLayerID;
                closedSortingOrder = spriteRenderer.sortingOrder;
                hasCachedClosedSorting = true;
            }
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

        /// <summary>
        /// Optional Animation Event hook: place on frame 5 or 6 of the open clip to unlock passage on that exact frame.
        /// </summary>
        public void AnimEvent_OnDoorPassable()
        {
            if (!isOpen) return;

            if (passageCoroutine != null)
            {
                StopCoroutine(passageCoroutine);
                passageCoroutine = null;
            }

            SetPassageEnabled(true);
        }

        private void EvaluateState()
        {
            bool shouldBeOpen = currentActiveSwitches >= requiredSwitches;
            if (shouldBeOpen == isOpen) return;

            isOpen = shouldBeOpen;

            if (passageCoroutine != null)
            {
                StopCoroutine(passageCoroutine);
                passageCoroutine = null;
            }

            if (doorAnimator != null)
            {
                doorAnimator.SetBool(IsOpenHash, isOpen);
            }

            if (isOpen)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnvDoorsOpen(gameObject);
                }

                passageCoroutine = StartCoroutine(EnablePassageAfterDelayRoutine());
            }
            else
            {
                // Lock passage immediately when closing starts
                SetPassageEnabled(false);
            }
        }

        private IEnumerator EnablePassageAfterDelayRoutine()
        {
            if (openPassageDelay > 0f)
            {
                yield return new WaitForSeconds(openPassageDelay);
            }

            SetPassageEnabled(true);
            passageCoroutine = null;
        }

        private void ApplyImmediateState(bool open)
        {
            if (doorAnimator != null)
            {
                doorAnimator.SetBool(IsOpenHash, open);
            }

            SetPassageEnabled(open);
        }

        private void SetPassageEnabled(bool canPass)
        {
            if (doorCollider == null)
            {
                doorCollider = GetComponent<Collider2D>();
            }

            if (doorCollider != null)
            {
                doorCollider.enabled = !canPass;
            }

            if (spriteRenderer == null) return;

            CacheClosedSorting();
            spriteRenderer.flipX = flipHorizontally;

            if (canPass && elevateOpenFrameSorting)
            {
                if (!string.IsNullOrEmpty(openFrameSortingLayer))
                {
                    int targetLayerID = SortingLayer.NameToID(openFrameSortingLayer);
                    if (SortingLayer.IsValid(targetLayerID))
                    {
                        spriteRenderer.sortingLayerID = targetLayerID;
                    }
                }

                spriteRenderer.sortingOrder = openFrameSortingOrder;
            }
            else
            {
                spriteRenderer.sortingLayerID = closedSortingLayerID;
                spriteRenderer.sortingOrder = closedSortingOrder;
            }

            if (spriteRenderer.transform != transform)
            {
                bool needsTopRowOffset = offsetOpenDownChildSprite && canPass && orientation == DoorOrientation.Down_2x2;
                spriteRenderer.transform.localPosition = needsTopRowOffset
                    ? new Vector3(0f, 0.5f * tileUnitSize, 0f)
                    : Vector3.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = (doorCollider != null && !doorCollider.enabled) ? Color.green : Color.red;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, ClosedColliderDimensions);
            Gizmos.matrix = oldMatrix;
        }
    }
}