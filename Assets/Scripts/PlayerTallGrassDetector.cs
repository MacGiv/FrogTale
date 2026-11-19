namespace FrogGame.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Detects when the player's center point is inside a TallGrass layer and plays/pauses
    /// an assigned Animator based on whether the player is actively moving on X or Y.
    /// </summary>
    public class PlayerTallGrassDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private LayerMask tallGrassLayer;
        [SerializeField] private Transform detectionPoint;
        [SerializeField] private float movementThreshold = 0.01f;

        [Header("Visual Effect")]
        [SerializeField] private Animator grassEffectAnimator;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (grassEffectAnimator != null)
            {
                grassEffectAnimator.speed = 0f;
                grassEffectAnimator.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (grassEffectAnimator == null) return;

            Vector2 checkPosition = detectionPoint != null ? detectionPoint.position : transform.position;

            bool isInTallGrass = Physics2D.OverlapPoint(checkPosition, tallGrassLayer) != null;

            if (isInTallGrass)
            {
                if (!grassEffectAnimator.gameObject.activeSelf)
                {
                    grassEffectAnimator.gameObject.SetActive(true);
                }

                bool isMoving = rb.linearVelocity.sqrMagnitude > (movementThreshold * movementThreshold);

                grassEffectAnimator.speed = isMoving ? 1f : 0f;
            }
            else if (grassEffectAnimator.gameObject.activeSelf)
            {
                grassEffectAnimator.speed = 0f;
                grassEffectAnimator.gameObject.SetActive(false);
            }
        }
    }
}