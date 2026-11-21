namespace FrogGame.Gameplay
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>
    /// Detects standing surface layer (Ground, Water, or Moving Platform) using a Box Gizmo positioned at the player's feet.
    /// Preserves ground-based respawn coordinates when standing on moving platforms and synchronizes with trigger platforms.
    /// </summary>
    public class PlayerWaterDetector : MonoBehaviour
    {
        [Header("Layer Detection")]
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask platformLayer;

        [Header("Feet Detection Settings")]
        [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.35f);
        [SerializeField] private Vector2 boxSize = new Vector2(0.1f, 0.05f);
        [SerializeField] private Color gizmoColor = Color.yellow;

        private Grid currentGrid;
        private Tilemap groundTilemap;
        private Tilemap waterTilemap;
        private PlayerBrain brain;

        private Vector3Int currentCell;
        private Vector3 lastSafeTileCenter;
        private MovingPlatform lastBoardedTriggerPlatform;

        public Vector3 LastSafeTileCenter => lastSafeTileCenter;

        private void Awake() => brain = GetComponent<PlayerBrain>();

        private void Start()
        {
            InitializeGridReferences();

            if (currentGrid != null)
            {
                currentCell = currentGrid.WorldToCell(GetFeetPosition());
                lastSafeTileCenter = currentGrid.GetCellCenterWorld(currentCell);
            }
            else
            {
                lastSafeTileCenter = transform.position;
            }
        }

        /// <summary>
        /// Returns the world-space position of the player's feet detection box.
        /// </summary>
        public Vector2 GetFeetPosition() => (Vector2)transform.position + feetOffset;

        /// <summary>
        /// Updates the safe respawn tile to the nearest valid ground tile around a world reference point (such as a platform waypoint).
        /// </summary>
        public void UpdateSafeRespawnNear(Vector3 worldReferencePos)
        {
            if (currentGrid != null && TryFindNearestGroundTile(worldReferencePos, out Vector3 nearestGroundCenter))
            {
                currentCell = currentGrid.WorldToCell(nearestGroundCenter);
                lastSafeTileCenter = nearestGroundCenter;
            }
            else
            {
                lastSafeTileCenter = worldReferencePos;
            }
        }

        private void Update()
        {
            if (brain == null || brain.FSM == null || brain.FSM.CurrentState == null) return;

            if (brain.FSM.CurrentState is PlayerGrappleState || brain.FSM.CurrentState is PlayerDrownState)
            {
                return;
            }

            Vector2 feetPos = GetFeetPosition();

            // 1. Stand on platform: Ignore water checks BUT keep previous ground position as safe respawn point
            Collider2D platformHit = Physics2D.OverlapBox(feetPos, boxSize, 0f, platformLayer);
            if (platformHit != null)
            {
                MovingPlatform platform = platformHit.GetComponentInParent<MovingPlatform>();
                if (platform != null && platform.Mode == MovingPlatform.PlatformMode.OnTrigger)
                {
                    lastBoardedTriggerPlatform = platform;
                }

                return;
            }

            // 2. Check for water trigger collider
            Collider2D waterHit = Physics2D.OverlapBox(feetPos, boxSize, 0f, waterLayer);

            if (waterHit != null)
            {
                Vector3 closestPoint = waterHit.ClosestPoint(feetPos);
                Vector3 dirToWater = (closestPoint - (Vector3)feetPos).normalized;

                if (dirToWater == Vector3.zero && brain.MovementHandler != null)
                {
                    dirToWater = (Vector3)brain.MovementHandler.FacingDirection;
                }

                Vector3 pointInsideWater = closestPoint + (dirToWater * 0.2f);
                Vector3 waterCenter = pointInsideWater;

                if (currentGrid != null)
                {
                    Vector3Int waterCell = currentGrid.WorldToCell(pointInsideWater);
                    waterCenter = currentGrid.GetCellCenterWorld(waterCell);
                }

                Vector3 respawnPos = ResolveSafeRespawnPosition();
                brain.DrownState.SetTargetPositions(waterCenter, respawnPos);
                brain.FSM.ChangeState(brain.DrownState);
                return;
            }

            // 3. Check for water tilemap tiles and update ground safe point using feet position
            if (currentGrid != null)
            {
                Vector3Int feetCellPos = currentGrid.WorldToCell(feetPos);

                if (waterTilemap != null && waterTilemap.HasTile(feetCellPos))
                {
                    Vector3 waterCenter = currentGrid.GetCellCenterWorld(feetCellPos);
                    Vector3 respawnPos = ResolveSafeRespawnPosition();
                    brain.DrownState.SetTargetPositions(waterCenter, respawnPos);
                    brain.FSM.ChangeState(brain.DrownState);
                    return;
                }

                if (groundTilemap != null && groundTilemap.HasTile(feetCellPos))
                {
                    // Player stepped back onto solid ground; clear boarded trigger platform tracking
                    lastBoardedTriggerPlatform = null;

                    if (feetCellPos != currentCell)
                    {
                        currentCell = feetCellPos;
                        lastSafeTileCenter = currentGrid.GetCellCenterWorld(currentCell);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that if the player drowned after riding an OnTrigger platform to a new shore without stepping off,
        /// the respawn point matches the platform's reset shore instead of the previous departure shore.
        /// </summary>
        private Vector3 ResolveSafeRespawnPosition()
        {
            if (lastBoardedTriggerPlatform != null && lastBoardedTriggerPlatform.Mode == MovingPlatform.PlatformMode.OnTrigger)
            {
                Vector3 platformOrigin = lastBoardedTriggerPlatform.CurrentOrigin;
                Vector3 platformOpposite = lastBoardedTriggerPlatform.TargetPosition;

                float distToOrigin = Vector3.Distance(lastSafeTileCenter, platformOrigin);
                float distToOpposite = Vector3.Distance(lastSafeTileCenter, platformOpposite);

                if (distToOrigin > distToOpposite)
                {
                    UpdateSafeRespawnNear(platformOrigin);
                }

                lastBoardedTriggerPlatform = null;
            }

            return lastSafeTileCenter;
        }

        /// <summary>
        /// Searches expanding grid rings around a world position to find the closest walkable ground tile free of water.
        /// </summary>
        private bool TryFindNearestGroundTile(Vector3 worldPos, out Vector3 groundCenter)
        {
            groundCenter = worldPos;
            if (currentGrid == null || groundTilemap == null) return false;

            Vector3Int centerCell = currentGrid.WorldToCell(worldPos);
            float bestDistSqr = float.MaxValue;
            bool found = false;

            const int maxSearchRadius = 4;

            for (int r = 0; r <= maxSearchRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;

                        Vector3Int candidateCell = new Vector3Int(centerCell.x + dx, centerCell.y + dy, centerCell.z);
                        bool hasGround = groundTilemap.HasTile(candidateCell);
                        bool hasWater = waterTilemap != null && waterTilemap.HasTile(candidateCell);

                        if (hasGround && !hasWater)
                        {
                            Vector3 candidateCenter = currentGrid.GetCellCenterWorld(candidateCell);
                            float distSqr = (candidateCenter - worldPos).sqrMagnitude;

                            if (distSqr < bestDistSqr)
                            {
                                bestDistSqr = distSqr;
                                groundCenter = candidateCenter;
                                found = true;
                            }
                        }
                    }
                }

                if (found) return true;
            }

            return false;
        }

        private void InitializeGridReferences()
        {
            currentGrid = FindFirstObjectByType<Grid>();

            if (currentGrid == null)
            {
                Debug.LogWarning("[PlayerWaterDetector] No Grid instance found in scene!");
                return;
            }

            Tilemap[] allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);

            foreach (Tilemap tilemap in allTilemaps)
            {
                int tilemapLayer = 1 << tilemap.gameObject.layer;

                if ((tilemapLayer & groundLayer) != 0) groundTilemap = tilemap;
                if ((tilemapLayer & waterLayer) != 0) waterTilemap = tilemap;
            }
        }

        private void OnDrawGizmos()
        {
            Vector2 feetPos = GetFeetPosition();
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(feetPos, boxSize);
        }

        private void OnDrawGizmosSelected()
        {
            if (currentGrid != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(lastSafeTileCenter, currentGrid.cellSize * 0.9f);
            }
        }
    }
}