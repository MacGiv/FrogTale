namespace FrogGame.Enemies
{
    using UnityEngine;
    using FrogGame.Core;
    using FrogGame.Gameplay;

    /// <summary>
    /// Brain & FSM Controller for the flying Small Owl enemy.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class SmallOwlBrain : MonoBehaviour, IDamageable, IStunnable, IKnockbackable
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 2;
        [SerializeField] private float flySpeed = 3f;
        [SerializeField] private float swoopSpeed = 9f;

        [Header("Combat & Detection")]
        [SerializeField] private float detectionRadius = 6f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private LayerMask playerLayer;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] waypoints;

        [Header("Visuals & Flip Settings")]
        [SerializeField] private Transform visualsTransform;
        [SerializeField] private bool spriteFacesLeftByDefault = false;

        [Header("FX References")]
        [SerializeField] private Animator alertAnimator;
        [SerializeField] private Animator attackAnimator;
        [SerializeField] private GameObject deathExplosionPrefab;

        private Rigidbody2D rb;
        private int currentHealth;
        private float knockbackTimer = 0f;
        private const float KNOCKBACK_DECELERATION = 30f;

        public FiniteStateMachine FSM { get; private set; }
        public Rigidbody2D Rb => rb;
        public float FlySpeed => flySpeed;
        public float SwoopSpeed => swoopSpeed;
        public float DetectionRadius => detectionRadius;
        public float AttackCooldown => attackCooldown;
        public int AttackDamage => attackDamage;
        public Transform[] Waypoints => waypoints;
        public Transform TargetPlayer { get; private set; }
        public Animator Animator { get; private set; }
        public Animator AlertAnimator => alertAnimator;
        public Animator AttackAnimator => attackAnimator;

        // States
        public SmallOwlPatrolState PatrolState { get; private set; }
        public SmallOwlSwoopState SwoopState { get; private set; }
        public SmallOwlStunnedState StunnedState { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            Animator = GetComponent<Animator>();

            currentHealth = maxHealth;

            FSM = new FiniteStateMachine();

            PatrolState = new SmallOwlPatrolState(this);
            SwoopState = new SmallOwlSwoopState(this);
            StunnedState = new SmallOwlStunnedState(this);
        }

        private void Start()
        {
            FSM.Initialize(PatrolState);
        }

        private void Update()
        {
            DetectPlayer();
            FSM.CurrentState.LogicUpdate();
            HandleFlip();
        }

        private void FixedUpdate()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.fixedDeltaTime;
                rb.linearVelocity = Vector2.MoveTowards(
                    rb.linearVelocity,
                    Vector2.zero,
                    KNOCKBACK_DECELERATION * Time.fixedDeltaTime
                );
                return;
            }

            FSM.CurrentState.PhysicsUpdate();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Apply contact damage during swoop
            if (FSM.CurrentState == SwoopState)
            {
                if (collision.TryGetComponent<IDamageable>(out var damageable) && collision.CompareTag("Player"))
                {
                    damageable.TakeDamage(attackDamage);

                    if (AttackAnimator != null)
                    {
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayEnemyAttack(gameObject);
                        }

                        AttackAnimator.gameObject.SetActive(true);
                        AttackAnimator.Play("SmallOwlScratch", -1, 0f);
                    }
                }
            }
        }

        private void HandleFlip()
        {
            if (visualsTransform == null || knockbackTimer > 0f) return;

            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                bool movingLeft = rb.linearVelocity.x < 0f;
                bool shouldFlip = spriteFacesLeftByDefault ? !movingLeft : movingLeft;

                Vector3 localScale = visualsTransform.localScale;
                localScale.x = shouldFlip ? -1f : 1f;
                visualsTransform.localScale = localScale;
            }
        }

        private void DetectPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            TargetPlayer = hit != null ? hit.transform : null;
        }

        public void TakeDamage(int damageAmount)
        {
            currentHealth -= damageAmount;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyDamage(gameObject);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void ApplyStun(float duration)
        {
            StunnedState.SetStunDuration(duration);
            FSM.ChangeState(StunnedState);
        }

        public void ApplyKnockback(Vector2 direction, float force, float duration = 0.15f)
        {
            knockbackTimer = duration;
            rb.linearVelocity = direction.normalized * force;
        }

        /// <summary>
        /// Triggers death audio, spawns the explosion VFX, and destroys the enemy GameObject.
        /// </summary>
        private void Die()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetEnemyDeadOwl(gameObject);
                AudioManager.Instance.PlayEnemyDead(gameObject);
            }

            SpawnDeathExplosion();
            Destroy(gameObject);
        }

        /// <summary>
        /// Instantiates the death explosion VFX prefab at the enemy's position and ensures automatic cleanup.
        /// </summary>
        private void SpawnDeathExplosion()
        {
            if (deathExplosionPrefab == null) return;

            GameObject explosion = Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
            if (!explosion.TryGetComponent<AutoDestroyFX>(out _))
            {
                explosion.AddComponent<AutoDestroyFX>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}