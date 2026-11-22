namespace FrogGame.Gameplay
{
    using System;
    using UnityEngine;
    using FrogGame.Core;

    /// <summary>
    /// Handles the area-of-effect (AoE) sneeze attack, applying damage, knockback, stun, and spawning the sneeze VFX animation.
    /// </summary>
    public class PlayerSneezeHandler : MonoBehaviour
    {
        [Header("Sneeze Settings")]
        [SerializeField] private float range = 2.0f; // ~2 tiles forward
        [SerializeField] private float areaWidth = 1.5f; // AoE width
        [SerializeField] private int damage = 1;
        [SerializeField] private float stunDuration = 2.0f;
        [SerializeField] private float knockbackForce = 10.0f;
        [SerializeField] private float cooldown = 2.0f;

        [Header("Layers & Detection")]
        [SerializeField] private LayerMask enemyLayer;

        [Header("Visual & Feedback References")]
        [SerializeField] private GameObject sneezeVfxPrefab;
        [SerializeField] private Transform sneezeSpawnPoint;
        [SerializeField] private float spawnForwardOffset = 0.5f;
        [SerializeField] private bool rotateVfxWithDirection = true;

        private float lastSneezeTime = -999f;
        private PlayerMovementHandler movementHandler;

        public bool CanSneeze => Time.time >= lastSneezeTime + cooldown;
        public float CooldownDuration => cooldown;

        // Event for UI cooldown updates (remainingTime, totalCooldown)
        public static event Action<float, float> OnSneezeTriggered;

        private void Awake()
        {
            movementHandler = GetComponent<PlayerMovementHandler>();
        }

        /// <summary>
        /// Executes the sneeze attack in the direction the player is facing.
        /// </summary>
        public bool ExecuteSneeze()
        {
            if (!CanSneeze)
            {
                Debug.Log("[PlayerSneezeHandler] Sneeze on cooldown!");
                return false;
            }

            lastSneezeTime = Time.time;
            OnSneezeTriggered?.Invoke(cooldown, cooldown);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlayerBoogerShot(gameObject);
            }

            Vector2 facingDir = movementHandler != null ? movementHandler.FacingDirection : Vector2.right;
            Vector2 attackCenter = (Vector2)transform.position + (facingDir * (range * 0.5f));
            Vector2 boxSize = new Vector2(range, areaWidth);
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;

            // Detect enemies within the AoE box
            Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, boxSize, angle, enemyLayer);

            foreach (var hit in hits)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemyBoogerHit(gameObject);
                }

                // 1. Apply Damage
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }

                // 2. Apply Stun
                if (hit.TryGetComponent<IStunnable>(out var stunnable))
                {
                    stunnable.ApplyStun(stunDuration);
                }

                // 3. Apply Knockback
                if (hit.TryGetComponent<Rigidbody2D>(out var enemyRb))
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
            }

            SpawnSneezeAnimation(facingDir, angle);

            return true;
        }

        private void SpawnSneezeAnimation(Vector2 facingDir, float angle)
        {
            if (sneezeVfxPrefab == null) return;

            Vector3 spawnPos = sneezeSpawnPoint != null
                ? sneezeSpawnPoint.position
                : (Vector3)((Vector2)transform.position + (facingDir.normalized * spawnForwardOffset));

            Quaternion vfxRotation = rotateVfxWithDirection
                ? Quaternion.Euler(0f, 0f, angle)
                : Quaternion.identity;

            GameObject vfxObj = Instantiate(sneezeVfxPrefab, spawnPos, vfxRotation);

            if (vfxObj.TryGetComponent<Animator>(out var vfxAnimator))
            {
                foreach (AnimatorControllerParameter param in vfxAnimator.parameters)
                {
                    if (param.name == "MoveX" && param.type == AnimatorControllerParameterType.Float)
                    {
                        vfxAnimator.SetFloat("MoveX", facingDir.x);
                    }
                    else if (param.name == "MoveY" && param.type == AnimatorControllerParameterType.Float)
                    {
                        vfxAnimator.SetFloat("MoveY", facingDir.y);
                    }
                }

                if (!vfxObj.TryGetComponent<AutoDestroyFX>(out _))
                {
                    vfxObj.AddComponent<AutoDestroyFX>();
                }
            }
            else
            {
                Destroy(vfxObj, 1.0f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 facingDir = movementHandler != null ? movementHandler.FacingDirection : Vector2.right;
            if (!Application.isPlaying && movementHandler == null) facingDir = Vector2.right;

            Vector2 attackCenter = (Vector2)transform.position + (facingDir * (range * 0.5f));
            Vector2 boxSize = new Vector2(range, areaWidth);
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;

            Gizmos.color = new Color(0f, 1f, 0.8f, 0.35f);
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(attackCenter, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            Gizmos.DrawCube(Vector3.zero, boxSize);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = oldMatrix;
        }
    }
}