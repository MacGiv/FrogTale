namespace FrogGame.Enemies
{
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using FrogGame.Core;

    /// <summary>
    /// Brain & FSM Controller for the Fox enemy.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FoxBrain : MonoBehaviour, IDamageable, IStunnable, IKnockbackable
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float patrolSpeed = 2.5f;
        [SerializeField] private float chaseSpeed = 4.5f;

        [Header("Detection & Combat Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Environment Avoidance")]
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private float waterCheckDistance = 0.6f;
        [SerializeField] private float waterCheckRadius = 0.25f;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] waypoints;

        [Header("Visuals & Flip Settings")]
        [SerializeField] private Transform visualsTransform;
        [SerializeField] private bool spriteFacesLeftByDefault = false;

        [Header("FX References")]
        [SerializeField] private Animator alertAnimator;
        [SerializeField] private Animator attackAnimator;

        private Rigidbody2D rb;
        private int currentHealth;
        private float knockbackTimer = 0f;
        private const float KNOCKBACK_DECELERATION = 30f;

        private Grid currentGrid;
        private Tilemap waterTilemap;

        public FiniteStateMachine FSM { get; private set; }
        public Rigidbody2D Rb => rb;
        public float PatrolSpeed => patrolSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float DetectionRadius => detectionRadius;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public Transform[] Waypoints => waypoints;
        public Transform TargetPlayer { get; private set; }
        public Animator Animator { get; private set; }
        public Animator AlertAnimator => alertAnimator;
        public Animator AttackAnimator => attackAnimator;

        // States
        public FoxPatrolState PatrolState { get; private set; }
        public FoxChaseState ChaseState { get; private set; }
        public FoxAttackState AttackState { get; private set; }
        public FoxStunnedState StunnedState { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            currentHealth = maxHealth;

            Animator = GetComponent<Animator>();

            FSM = new FiniteStateMachine();

            PatrolState = new FoxPatrolState(this);
            ChaseState = new FoxChaseState(this);
            AttackState = new FoxAttackState(this);
            StunnedState = new FoxStunnedState(this);
        }

        private void Start()
        {
            InitializeWaterReferences();
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

        private void InitializeWaterReferences()
        {
            currentGrid = FindFirstObjectByType<Grid>();
            if (currentGrid == null) return;

            Tilemap[] allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (Tilemap tilemap in allTilemaps)
            {
                int tilemapLayer = 1 << tilemap.gameObject.layer;
                if ((tilemapLayer & waterLayer) != 0)
                {
                    waterTilemap = tilemap;
                    break;
                }
            }
        }

        /// <summary>
        /// Checks whether moving in the specified direction enters water colliders or water tiles.
        /// </summary>
        public bool IsWaterInDirection(Vector2 direction)
        {
            if (direction == Vector2.zero) return false;

            Vector2 checkPos = (Vector2)transform.position + (direction.normalized * waterCheckDistance);

            if (Physics2D.OverlapCircle(checkPos, waterCheckRadius, waterLayer) != null)
            {
                return true;
            }

            if (currentGrid != null && waterTilemap != null)
            {
                Vector3Int cell = currentGrid.WorldToCell(checkPos);
                if (waterTilemap.HasTile(cell))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a safe normalized movement vector that avoids stepping into water, sliding along shorelines if possible.
        /// </summary>
        public Vector2 GetSafeMoveDirection(Vector2 desiredDirection)
        {
            if (desiredDirection == Vector2.zero) return Vector2.zero;

            Vector2 normalized = desiredDirection.normalized;
            if (!IsWaterInDirection(normalized))
            {
                return normalized;
            }

            // Try sliding horizontally along the water edge
            Vector2 dirX = new Vector2(normalized.x, 0f);
            if (Mathf.Abs(dirX.x) > 0.05f && !IsWaterInDirection(dirX.normalized))
            {
                return dirX.normalized;
            }

            // Try sliding vertically along the water edge
            Vector2 dirY = new Vector2(0f, normalized.y);
            if (Mathf.Abs(dirY.y) > 0.05f && !IsWaterInDirection(dirY.normalized))
            {
                return dirY.normalized;
            }

            return Vector2.zero;
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

        /// <summary>
        /// Detects player within range using OverlapCircle.
        /// </summary>
        private void DetectPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            if (hit != null)
            {
                TargetPlayer = hit.transform;
            }
            else
            {
                TargetPlayer = null;
            }
        }

        public void TakeDamage(int damageAmount)
        {
            currentHealth -= damageAmount;
            Debug.Log($"{gameObject.name} took {damageAmount} damage. Health left: {currentHealth}");

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

        private void Die()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetEnemyDeadFox(gameObject);
                AudioManager.Instance.PlayEnemyDead(gameObject);
            }

            // TODO: Efectos visuales, drops o eventos de muerte
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            // Visualización de rangos en el editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * waterCheckDistance, waterCheckRadius);
        }
    }
}