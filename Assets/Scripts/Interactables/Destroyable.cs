namespace FrogGame.Gameplay
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>
    /// Handles destruction of environmental props/bushes, clears tilemap cells, 
    /// and instantiates dropped items based on percentage chances.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Destroyable : MonoBehaviour
    {
        [System.Serializable]
        public struct DropEntry
        {
            [Tooltip("Prefab of the pickup item to drop (e.g. PickupHealth, PickupArrow)")]
            public GameObject itemPrefab;

            [Tooltip("Drop chance percentage (0 to 100%)")]
            [Range(0f, 100f)] public float dropChance;
        }

        [Header("Layer Detection")]
        [SerializeField] private LayerMask damageLayers;

        [Header("Effects")]
        [SerializeField] private GameObject explosionVfxPrefab;

        [Header("Loot Drop Table")]
        [Tooltip("Overall chance that this object drops ANY item upon destruction (0-100%).")]
        [Range(0f, 100f)][SerializeField] private float overallDropChance = 60f;

        [Tooltip("List of potential items and their individual drop probabilities.")]
        [SerializeField] private List<DropEntry> lootTable = new List<DropEntry>();

        private bool isDestroyed = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isDestroyed) return;

            if (((1 << collision.gameObject.layer) & damageLayers) != 0)
            {
                DestroyBush();
            }
        }

        /// <summary>
        /// Executes destruction routines, spawns destruction VFX, triggers loot table drops, and clears tilemap cell.
        /// </summary>
        public void DestroyBush()
        {
            isDestroyed = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnvRockDrop(gameObject);
            }

            // 1. Instanciar VFX de destrucción
            if (explosionVfxPrefab != null)
            {
                Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            }

            // 2. Intentar dropear ítem según la tabla de probabilidad
            TrySpawnLoot();

            // 3. Limpiar tile del Tilemap si el objeto forma parte de uno
            Tilemap tilemap = GetComponentInParent<Tilemap>();
            if (tilemap != null)
            {
                Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
                tilemap.SetTile(cellPosition, null);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Calculates loot drop chance and instantiates the selected item prefab.
        /// </summary>
        private void TrySpawnLoot()
        {
            if (lootTable == null || lootTable.Count == 0) return;

            // Tirada general para ver si este arbusto suelta algo
            float roll = Random.Range(0f, 100f);
            if (roll > overallDropChance) return;

            // Tirada de selección ponderada entre los ítems configurados
            float totalWeight = 0f;
            foreach (var entry in lootTable)
            {
                totalWeight += entry.dropChance;
            }

            if (totalWeight <= 0f) return;

            float itemRoll = Random.Range(0f, totalWeight);
            float currentSum = 0f;

            foreach (var entry in lootTable)
            {
                currentSum += entry.dropChance;
                if (itemRoll <= currentSum)
                {
                    if (entry.itemPrefab != null)
                    {
                        Instantiate(entry.itemPrefab, transform.position, Quaternion.identity);
                    }
                    break;
                }
            }
        }
    }
}