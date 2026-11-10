namespace FrogGame.Gameplay
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Controls tongue extension, raycasting impact timing, and attached item retrieval.
    /// Controla la extensión de la lengua, el timing de impacto del raycast y el retorno de objetos acoplados.
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
        [SerializeField] private LayerMask shieldLayer;
        [SerializeField] private LayerMask wallLayer;

        [Header("References & Visuals")]
        [SerializeField] private Transform tongueOrigin;
        [SerializeField] private Transform tongueTip;

        private LineRenderer lineRenderer;

        public enum TongueHitResult { None, Grapple, Grabbable }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            SetVisualsActive(false);
        }

        /// <summary>
        /// Shoots tongue out, triggers object grab on impact, and retracts attached item back to player.
        /// </summary>
        public IEnumerator ShootTongueRoutine(Vector2 direction, System.Action<TongueHitResult, Vector2> onComplete)
        {
            SetVisualsActive(true);

            Vector2 origin = tongueOrigin != null ? (Vector2)tongueOrigin.position : (Vector2)transform.position;
            Vector2 targetPoint = origin + direction * maxDistance;
            TongueHitResult hitResult = TongueHitResult.None;
            Vector2 hitPosition = targetPoint;
            Collider2D hitCollider = null;

            LayerMask combinedMask = grappleLayer | grabbableLayer | shieldLayer | wallLayer;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, combinedMask);

            if (hit.collider != null)
            {
                hitPosition = hit.point;
                hitCollider = hit.collider;
                int hitLayer = 1 << hit.collider.gameObject.layer;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPlayerTongueHit(gameObject);
                }

                if ((hitLayer & grappleLayer) != 0)
                {
                    hitResult = TongueHitResult.Grapple;
                }
                else if (((hitLayer & grabbableLayer) != 0) || ((hitLayer & shieldLayer) != 0))
                {
                    hitResult = TongueHitResult.Grabbable;
                }
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Phase 1: Extend Tongue (La lengua viaja hacia el objetivo)
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

            // IMPACTO: Engancha el objeto recién al llegar a la punta
            GrabbableItem grabbedItem = null;

            if (hitResult == TongueHitResult.Grabbable && hitCollider != null)
            {
                grabbedItem = hitCollider.GetComponent<GrabbableItem>();
                if (grabbedItem == null)
                {
                    grabbedItem = hitCollider.GetComponentInParent<GrabbableItem>();
                }

                if (grabbedItem != null)
                {
                    grabbedItem.OnGrabStarted();
                }
            }

            // Phase 2: Retract Tongue (Retrae la lengua trayendo el objeto pegado a la punta)
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

            // CONSUMACIÓN: Entrega el objeto al jugador al terminar la retracción
            if (grabbedItem != null)
            {
                grabbedItem.OnGrabCompleted(transform);
            }

            SetVisualsActive(false);
            onComplete?.Invoke(hitResult, hitPosition);
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