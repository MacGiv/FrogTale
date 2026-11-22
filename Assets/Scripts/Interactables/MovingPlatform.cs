namespace FrogGame.Gameplay
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Trigger-based moving platform for top-down gameplay.
    /// Supports bi-directional travel (Point A <-> Point B), carries the player smoothly via Rigidbody2D deltas,
    /// and resets cleanly to the current trip's origin shore upon player drowning.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum PlatformMode { Always, OnTrigger }

        [Header("Movement Settings")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float waitTimeAtWaypoints = 0.5f;
        [SerializeField] private PlatformMode mode = PlatformMode.Always;

        [Header("3x3 Center Tile Detection")]
        [Tooltip("Max distance from platform center (0,0) to consider the player is on the center tile.")]
        [SerializeField] private float centerTileRadius = 0.7f;

        private Rigidbody2D rb;
        private Vector3 targetPosition;
        private Vector3 currentOrigin; // Dynamic origin shore for current trip

        private Transform playerTransform;
        private Rigidbody2D playerRb;
        private PlayerBrain playerBrain;
        private PlayerWaterDetector playerWaterDetector;
        private bool isPlayerOnPlatform = false;
        private bool isHeadingToB = true;
        private bool isMoving = false;
        private bool isWaiting = false;
        private bool isMovingAudioPlaying = false;
        private float postRespawnCooldown = 0f;

        public PlatformMode Mode => mode;
        public Vector3 CurrentOrigin => currentOrigin;
        public Vector3 TargetPosition => targetPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
        }

        private void OnEnable()
        {
            PlayerDrownState.OnPlayerDrowned += HandlePlayerDrowned;
        }

        private void OnDisable()
        {
            PlayerDrownState.OnPlayerDrowned -= HandlePlayerDrowned;
            StopMovementAudio();
        }

        private void Start()
        {
            if (pointA != null && pointB != null)
            {
                isHeadingToB = true;
                currentOrigin = pointA.position;
                targetPosition = pointB.position;
            }

            ResetToCurrentOrigin();
        }

        private void Update()
        {
            if (postRespawnCooldown > 0f)
            {
                postRespawnCooldown -= Time.deltaTime;
            }
        }

        private void HandlePlayerDrowned()
        {
            isPlayerOnPlatform = false;
            playerTransform = null;
            playerRb = null;
            playerBrain = null;
            playerWaterDetector = null;

            // Brief cooldown prevents immediate re-trigger if the respawn tile borders the dock trigger
            postRespawnCooldown = 0.35f;

            if (mode == PlatformMode.OnTrigger)
            {
                ResetToCurrentOrigin();
            }
        }

        /// <summary>
        /// Resets the platform to the starting shore of its current journey leg and clears pending physics motion.
        /// </summary>
        public void ResetToCurrentOrigin()
        {
            StopAllCoroutines();
            isWaiting = false;
            isMoving = (mode == PlatformMode.Always);

            if (pointA != null && pointB != null)
            {
                currentOrigin = isHeadingToB ? pointA.position : pointB.position;
                targetPosition = isHeadingToB ? pointB.position : pointA.position;
            }

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = currentOrigin;
            rb.position = currentOrigin;
            Physics2D.SyncTransforms();

            if (isMoving)
            {
                StartMovementAudio();
            }
            else
            {
                StopMovementAudio();
            }
        }

        private void FixedUpdate()
        {
            if (!isMoving || isWaiting || pointA == null || pointB == null) return;

            Vector2 currentRbPos = rb.position;
            Vector2 newPos = Vector2.MoveTowards(currentRbPos, targetPosition, speed * Time.fixedDeltaTime);
            Vector2 platformDelta = newPos - currentRbPos;

            rb.MovePosition(newPos);

            // Carry the player in exact lockstep with the Rigidbody2D delta so the platform never leaves them behind
            if (isPlayerOnPlatform && platformDelta != Vector2.zero)
            {
                if (playerRb != null)
                {
                    playerRb.position += platformDelta;
                }
                else if (playerTransform != null)
                {
                    playerTransform.position += (Vector3)platformDelta;
                }
            }

            if (Vector2.Distance(newPos, targetPosition) <= 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
                transform.position = targetPosition;
                rb.position = targetPosition;
                OnWaypointReached();
            }
        }

        /// <summary>
        /// Commits the reached waypoint as the new origin shore immediately before starting the arrival cooldown,
        /// and updates the player's safe respawn shore if the player is riding the platform.
        /// </summary>
        private void OnWaypointReached()
        {
            // The reached waypoint becomes the new origin for the return trip
            isHeadingToB = !isHeadingToB;
            currentOrigin = isHeadingToB ? pointA.position : pointB.position;
            targetPosition = isHeadingToB ? pointB.position : pointA.position;

            if (mode == PlatformMode.OnTrigger && isPlayerOnPlatform && playerWaterDetector != null)
            {
                playerWaterDetector.UpdateSafeRespawnNear(currentOrigin);
            }

            StopMovementAudio();

            if (mode == PlatformMode.OnTrigger)
            {
                isMoving = false;
            }

            StartCoroutine(WaypointCooldownRoutine());
        }

        private IEnumerator WaypointCooldownRoutine()
        {
            isWaiting = true;
            yield return new WaitForSeconds(waitTimeAtWaypoints);
            isWaiting = false;

            if (isMoving)
            {
                StartMovementAudio();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            CachePlayerReferences(other);
            if (IsPlayerDrowning()) return;

            isPlayerOnPlatform = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            CachePlayerReferences(other);
            if (IsPlayerDrowning()) return;

            isPlayerOnPlatform = true;

            if (mode == PlatformMode.OnTrigger && !isMoving && !isWaiting && postRespawnCooldown <= 0f)
            {
                float distanceToCenter = Vector2.Distance(other.transform.position, transform.position);

                if (distanceToCenter <= centerTileRadius)
                {
                    isMoving = true;
                    StartMovementAudio();

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayEnvPlatformPlayer(gameObject);
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            isPlayerOnPlatform = false;
            playerTransform = null;
            playerRb = null;
            playerBrain = null;
            playerWaterDetector = null;
        }

        private void CachePlayerReferences(Collider2D playerCollider)
        {
            if (playerTransform != playerCollider.transform)
            {
                playerTransform = playerCollider.transform;
                playerRb = playerCollider.attachedRigidbody;
                playerBrain = playerCollider.GetComponentInParent<PlayerBrain>();
                playerWaterDetector = playerCollider.GetComponentInParent<PlayerWaterDetector>();
            }
        }

        private bool IsPlayerDrowning()
        {
            return playerBrain != null && playerBrain.FSM != null && playerBrain.FSM.CurrentState is PlayerDrownState;
        }

        private void StartMovementAudio()
        {
            if (!isMovingAudioPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnvPlatform(gameObject);
                isMovingAudioPlaying = true;
            }
        }

        private void StopMovementAudio()
        {
            if (isMovingAudioPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopEnvPlatform(gameObject);
                isMovingAudioPlaying = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pointA.position, pointB.position);
                Gizmos.DrawWireSphere(pointA.position, 0.3f);
                Gizmos.DrawWireSphere(pointB.position, 0.3f);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, centerTileRadius);
        }
    }
}