namespace FrogGame.Gameplay
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Blinks a red hit-overlay SpriteRenderer when an enemy takes damage,
    /// mirroring the PlayerHUD hit-flash feedback without conflicting with the enemy Animator.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyHitFlash : MonoBehaviour
    {
        [Header("Sprite References")]
        [Tooltip("Main SpriteRenderer driven by the enemy Animator.")]
        [SerializeField] private SpriteRenderer mainSpriteRenderer;
        [Tooltip("Child SpriteRenderer placed on top of the enemy with the red idle sprite.")]
        [SerializeField] private SpriteRenderer hitOverlayRenderer;
        [Tooltip("Red idle sprite displayed during the hit flash.")]
        [SerializeField] private Sprite redIdleSprite;

        [Header("Flash Timing (Matches PlayerHUD)")]
        [Min(1)]
        [SerializeField] private int hitFlashCount = 3;
        [Min(0.01f)]
        [SerializeField] private float hitFlashInterval = 0.08f;
        [SerializeField] private bool syncFlipX = true;

        private Coroutine flashCoroutine;

        private void Awake()
        {
            if (mainSpriteRenderer == null)
            {
                mainSpriteRenderer = GetComponent<SpriteRenderer>();
                if (mainSpriteRenderer == null)
                {
                    mainSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }

            if (hitOverlayRenderer != null)
            {
                if (redIdleSprite != null)
                {
                    hitOverlayRenderer.sprite = redIdleSprite;
                }

                hitOverlayRenderer.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (syncFlipX && hitOverlayRenderer != null && hitOverlayRenderer.enabled && mainSpriteRenderer != null)
            {
                hitOverlayRenderer.flipX = mainSpriteRenderer.flipX;
            }
        }

        private void OnDisable()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            if (hitOverlayRenderer != null)
            {
                hitOverlayRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Triggers the red hit-flash blink sequence, restarting cleanly if hit again mid-flash.
        /// </summary>
        [ContextMenu("Test Hit Flash")]
        public void TriggerHitFlash()
        {
            if (hitOverlayRenderer == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            if (redIdleSprite != null)
            {
                hitOverlayRenderer.sprite = redIdleSprite;
            }

            for (int i = 0; i < hitFlashCount; i++)
            {
                if (syncFlipX && mainSpriteRenderer != null)
                {
                    hitOverlayRenderer.flipX = mainSpriteRenderer.flipX;
                }

                hitOverlayRenderer.enabled = true;
                yield return new WaitForSeconds(hitFlashInterval);

                hitOverlayRenderer.enabled = false;

                if (i < hitFlashCount - 1)
                {
                    yield return new WaitForSeconds(hitFlashInterval);
                }
            }

            hitOverlayRenderer.enabled = false;
            flashCoroutine = null;
        }
    }
}