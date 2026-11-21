namespace FrogGame.Core
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>
    /// Centralized manager responsible for inspecting colliders and Tilemaps to resolve SurfaceTypes.
    /// </summary>
    public class SurfaceDetectorManager : MonoBehaviour
    {
        public static SurfaceDetectorManager Instance { get; private set; }

        [Header("Surface Configuration")]
        [SerializeField] private TileSurfaceConfig surfaceConfig;
        [SerializeField] private Tilemap[] groundTilemaps;

        private const float PENETRATION_OFFSET = 0.08f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (surfaceConfig != null)
            {
                surfaceConfig.Initialize();
            }
        }

        /// <summary>
        /// Inspects a Collider2D (such as a TilemapCollider2D) at the impact point to determine its SurfaceType.
        /// </summary>
        public SurfaceType GetSurfaceFromCollider(Collider2D hitCollider, Vector2 impactPoint, Vector2 impactDirection)
        {
            if (hitCollider == null)
            {
                return GetDefaultSurface();
            }

            Tilemap tilemap = hitCollider.GetComponentInParent<Tilemap>();
            Vector2 sampleWorldPos = impactPoint + (impactDirection.normalized * PENETRATION_OFFSET);

            return GetSurfaceAtPosition(tilemap, sampleWorldPos);
        }

        /// <summary>
        /// Direct world position query on a specific Tilemap.
        /// </summary>
        public SurfaceType GetSurfaceAtPosition(Tilemap targetTilemap, Vector2 worldPosition)
        {
            if (targetTilemap == null || surfaceConfig == null)
            {
                return GetDefaultSurface();
            }

            Vector3Int cellPos = targetTilemap.WorldToCell(worldPosition);
            TileBase hitTile = targetTilemap.GetTile(cellPos);

            return surfaceConfig.GetSurfaceType(hitTile);
        }

        /// <summary>
        /// Direct world position query across configured ground Tilemaps without needing physics colliders.
        /// </summary>
        public SurfaceType GetGroundSurfaceAtPosition(Vector2 worldPosition)
        {
            if (groundTilemaps == null || surfaceConfig == null)
            {
                return GetDefaultSurface();
            }

            for (int i = 0; i < groundTilemaps.Length; i++)
            {
                if (groundTilemaps[i] == null) continue;

                SurfaceType surface = GetSurfaceAtPosition(groundTilemaps[i], worldPosition);

                if (surface != SurfaceType.Default && surface != GetDefaultSurface())
                {
                    return surface;
                }
            }

            return GetDefaultSurface();
        }

        private SurfaceType GetDefaultSurface() => surfaceConfig != null ? surfaceConfig.DefaultSurface : SurfaceType.Default;
    }
}