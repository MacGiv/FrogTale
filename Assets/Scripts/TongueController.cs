namespace FrogGame.Gameplay
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Controls tongue rendering, dynamic sorting order when shooting upward, raycasting, and object interaction (including grabbables and enemy shields).
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TongueController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxDistance = 5.0f;
        [SerializeField] private float extendSpeed = 25.0f;
        [SerializeField] private float retractSpeed = 30.0f;

        [Header("Layer Masks")]
        [SerializeField] private LayerMask grappleLayer;
        [SerializeField] private LayerMask grabbableLayer;
        [SerializeField] private LayerMask shieldLayer; // Enemy shield layer
        [SerializeField] private LayerMask obstaclesLayer;

        [Header("References & Visuals")]
        [SerializeField] private Transform tongueOrigin;
        [SerializeField] private Transform tongueTip;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;

        private LineRenderer lineRenderer;
        private SpriteRenderer[] tipSpriteRenderers;

        public enum TongueHitResult { None, Grapple, Grabbable }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            if (tongueTip != null)
            {
                tipSpriteRenderers = tongueTip.GetComponentsInChildren<SpriteRenderer>(true);
            }

            ResolvePlayerSpriteRenderer();
            SetVisualsActive(false);
        }

        private void ResolvePlayerSpriteRenderer()
        {
            if (playerSpriteRenderer != null) return;

            // Prefer the SpriteRenderer attached to the player's Animator to avoid grabbing shadow or FX child renderers
            Animator playerAnimator = GetComponentInParent<Animator>();
            if (playerAnimator != null && playerAnimator.TryGetComponent<SpriteRenderer>(out var animRenderer))
            {
                playerSpriteRenderer = animRenderer;
                return;
            }

            if (transform.root != null)
            {
                Animator rootAnimator = transform.root.GetComponentInChildren<Animator>(true);
                if (rootAnimator != null && rootAnimator.TryGetComponent<SpriteRenderer>(out var rootAnimRenderer))
                {
                    playerSpriteRenderer = rootAnimRenderer;
                    return;
                }
            }
        }

        /// <summary>
        /// Shoots the tongue out and retracts it. Calls callback on completion.
        /// </summary>
        public IEnumerator ShootTongueRoutine(Vector2 direction, System.Action<TongueHitResult, Vector2> onComplete)
        {
            UpdateTongueSortingOrder(direction);
            SetVisualsActive(true);

            Vector2 origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
            Vector2 targetPoint = origin + direction * maxDistance;
            TongueHitResult hitResult = TongueHitResult.None;
            Vector2 hitPosition = targetPoint;
            Collider2D hitCollider = null;

            LayerMask combinedMask = grappleLayer | grabbableLayer | shieldLayer | obstaclesLayer;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, combinedMask);

            if (hit.collider != null)
            {
                hitPosition = hit.point;
                hitCollider = hit.collider;
                int hitLayer = 1 << hit.collider.gameObject.layer;

                if ((hitLayer & grappleLayer) != 0)
                {
                    hitResult = TongueHitResult.Grapple;
                }
                else if (((hitLayer & grabbableLayer) != 0) || ((hitLayer & shieldLayer) != 0))
                {
                    hitResult = TongueHitResult.Grabbable;
                }
            }

            // Play tongue hit sound
            if (AudioManager.Instance != null)
            {
                TriggerTongueImpactAudio(hitCollider);
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Phase 1: Extend Tongue
            float currentDist = 0f;
            float totalDist = Vector2.Distance(origin, hitPosition);

            while (currentDist < totalDist)
            {
                currentDist += extendSpeed * Time.deltaTime;
                Vector2 currentTipPos = Vector2.MoveTowards(origin, hitPosition, currentDist);

                UpdateTongueVisuals(origin, currentTipPos, angle);

                yield return null;
                origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
            }

            UpdateTongueVisuals(origin, hitPosition, angle);

            // Phase 1.5: Impact - Initiate grab if object is not locked on a switch
            GrabbableItem grabbedItem = null;

            if (hitResult == TongueHitResult.Grabbable && hitCollider != null)
            {
                grabbedItem = hitCollider.GetComponent<GrabbableItem>();
                if (grabbedItem == null)
                {
                    grabbedItem = hitCollider.GetComponentInParent<GrabbableItem>();
                }

                if (grabbedItem != null && !grabbedItem.IsLockedOnSwitch)
                {
                    grabbedItem.OnGrabStarted();
                }
                else
                {
                    grabbedItem = null;
                    hitResult = TongueHitResult.None;
                }
            }

            // Phase 2: Retract Tongue (Only if not grappling)
            if (hitResult != TongueHitResult.Grapple)
            {
                while (currentDist > 0f)
                {
                    currentDist -= retractSpeed * Time.deltaTime;
                    Vector2 currentTipPos = Vector2.MoveTowards(origin, hitPosition, currentDist);

                    UpdateTongueVisuals(origin, currentTipPos, angle);

                    if (grabbedItem != null)
                    {
                        grabbedItem.transform.position = currentTipPos;
                    }

                    yield return null;
                    origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
                }
            }

            if (grabbedItem != null)
            {
                grabbedItem.OnGrabCompleted(transform);
            }

            SetVisualsActive(false);
            onComplete?.Invoke(hitResult, hitPosition);
        }

        private void TriggerTongueImpactAudio(Collider2D hitCollider)
        {
            if (hitCollider != null)
            {
                if (hitCollider.CompareTag("Rock"))
                {
                    AudioManager.Instance.SetPlayerTongueHitRock(gameObject);
                }
                else
                {
                    AudioManager.Instance.SetPlayerTongueHitRandom(gameObject);
                }
            }
            else
            {
                AudioManager.Instance.SetPlayerTongueShot(gameObject);
            }

            AudioManager.Instance.PlayPlayerTongueHit(gameObject);
        }
        
        /// <summary>
        /// Places the tongue behind the player sprite when shooting upward, and in front for all other directions,
        /// preserving the player's standardized Sorting Layer.
        /// </summary>
        private void UpdateTongueSortingOrder(Vector2 direction)
        {
            if (playerSpriteRenderer == null)
            {
                ResolvePlayerSpriteRenderer();
                if (playerSpriteRenderer == null) return;
            }

            bool isShootingUp = direction.y > 0.1f && Mathf.Abs(direction.y) >= Mathf.Abs(direction.x);
            int targetSortingOrder = isShootingUp
                ? playerSpriteRenderer.sortingOrder - 1
                : playerSpriteRenderer.sortingOrder + 1;

            lineRenderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            lineRenderer.sortingOrder = targetSortingOrder;

            if (tipSpriteRenderers != null)
            {
                for (int i = 0; i < tipSpriteRenderers.Length; i++)
                {
                    if (tipSpriteRenderers[i] != null)
                    {
                        tipSpriteRenderers[i].sortingLayerID = playerSpriteRenderer.sortingLayerID;
                        tipSpriteRenderers[i].sortingOrder = targetSortingOrder;
                    }
                }
            }
        }

        private void UpdateTongueVisuals(Vector2 origin, Vector2 tipPos, float angle)
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, tipPos);

            if (tongueTip != null)
            {
                tongueTip.position = tipPos;
                tongueTip.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void SetVisualsActive(bool active)
        {
            lineRenderer.enabled = active;
            if (tongueTip != null)
            {
                tongueTip.gameObject.SetActive(active);
            }
        }

        public void DisableLine() => SetVisualsActive(false);
    }
}