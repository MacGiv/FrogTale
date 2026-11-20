namespace FrogGame.Gameplay
{
    using UnityEngine;
    using FrogGame.Core;

    /// <summary>
    /// Projectile script that moves in a direction and damages enemies or bounces off obstacles and shields.
    /// Script del proyectil que avanza e inflige daño a enemigos o rebota contra obstáculos y escudos.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Arrow : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float maxLifetime = 3f;
        [SerializeField] private float destroyAudioDelay = 2f;
        [SerializeField] private GameObject pickupArrowPrefab;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float knockbackDuration = 0.15f;

        [Header("Collision Layers")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask shieldLayer;

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;
        private bool hasHit;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            Destroy(gameObject, maxLifetime);
        }

        /// <summary>
        /// Initializes arrow velocity and rotation direction.
        /// </summary>
        public void Setup(Vector2 direction)
        {
            Vector2 normalizedDir = direction.normalized;

            // Set physics velocity
            rb.linearVelocity = normalizedDir * speed;

            // Rotate visual arrow to facing direction
            float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasHit) return;

            int colLayer = 1 << collision.gameObject.layer;

            // 1. Check if hit obstacle, wall, or enemy shield
            if (((colLayer & obstacleLayer) != 0) || ((colLayer & shieldLayer) != 0))
            {
                hasHit = true;

                PlayImpactAudio(collision, colLayer);
                SpawnPickup();
                DisableAndDelayedDestroy();

                return;
            }

            // 2. Check if hit enemy body directly
            if ((colLayer & enemyLayer) != 0)
            {
                hasHit = true;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemyArrowHit(gameObject);
                }

                if (collision.TryGetComponent<IKnockbackable>(out var knockbackable))
                {
                    Vector2 knockbackDir = rb.linearVelocity != Vector2.zero
                        ? rb.linearVelocity.normalized
                        : (Vector2)(collision.transform.position - transform.position).normalized;

                    knockbackable.ApplyKnockback(knockbackDir, knockbackForce, knockbackDuration);
                }

                IDamageable damageable = collision.GetComponent<IDamageable>();

                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }

                DisableAndDelayedDestroy();
            }
        }

        private void SpawnPickup()
        {
            if (pickupArrowPrefab != null)
            {
                Instantiate(pickupArrowPrefab, transform.position, Quaternion.identity);
            }
        }

        private void PlayImpactAudio(Collider2D collision, int colLayer)
        {
            if (AudioManager.Instance == null) return;

            SurfaceType hitSurface = SurfaceType.Wood;

            if ((colLayer & shieldLayer) != 0)
            {
                hitSurface = SurfaceType.Metal;
            }
            else if (SurfaceDetectorManager.Instance != null)
            {
                Vector2 impactPoint = collision.ClosestPoint(transform.position);
                hitSurface = SurfaceDetectorManager.Instance.GetSurfaceFromCollider(collision, impactPoint, rb.linearVelocity);
            }

            switch (hitSurface)
            {
                case SurfaceType.Grass:

                    AudioManager.Instance.SetArrowHitGrass(gameObject);

                    break;
                case SurfaceType.Metal:

                    AudioManager.Instance.SetArrowHitMetal(gameObject);

                    break;
                case SurfaceType.Wood:

                    AudioManager.Instance.SetArrowHitWood(gameObject);

                    break;
            }

            AudioManager.Instance.PlayPlayerArrowHit(gameObject);
        }

        /// <summary>
        /// Disables visuals and physics immediately while keeping the GameObject alive briefly so Wwise finishes playing the emitter audio.
        /// </summary>
        private void DisableAndDelayedDestroy()
        {
            if (col != null) col.enabled = false;
            if (spriteRenderer != null) spriteRenderer.enabled = false;

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            Destroy(gameObject, destroyAudioDelay);
        }
    }
}