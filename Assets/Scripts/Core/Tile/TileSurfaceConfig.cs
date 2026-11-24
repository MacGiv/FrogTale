namespace FrogGame.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>
    /// ScriptableObject database that maps TileBase assets to their corresponding SurfaceType and Wwise Switch runtime lookups.
    /// </summary>
    [CreateAssetMenu(fileName = "TileSurfaceConfig", menuName = "Frog Tools/Tile/Tile Surface Config")]
    public class TileSurfaceConfig : ScriptableObject
    {
        /// <summary>
        /// Represents a group of tiles sharing the same physical surface type and audio switch.
        /// </summary>
        [Serializable]
        public struct SurfaceEntry
        {
            public string Label;
            public SurfaceType SurfaceType;
            public AK.Wwise.Switch WwiseSwitch;
            public TileBase[] Tiles;
        }

        [Header("Fallback Configuration")]
        [SerializeField] private SurfaceType defaultSurface = SurfaceType.Wood;
        [SerializeField] private AK.Wwise.Switch defaultSwitch;

        [Header("Surface Configuration")]
        [SerializeField] private List<SurfaceEntry> surfaces;

        public SurfaceType DefaultSurface => defaultSurface;

        private Dictionary<TileBase, SurfaceEntry> tileLookup;
        private Dictionary<SurfaceType, AK.Wwise.Switch> typeSwitchLookup;

        public void Initialize()
        {
            if (tileLookup == null)
            {
                BuildLookup();
            }
        }

        /// <summary>
        /// Returns the SurfaceType associated with the specified TileBase, or the configured fallback surface if not found.
        /// </summary>
        public SurfaceType GetSurfaceType(TileBase tile)
        {
            if (tileLookup == null) BuildLookup();

            if (tile != null && tileLookup.TryGetValue(tile, out SurfaceEntry entry))
            {
                return entry.SurfaceType;
            }

            return defaultSurface;
        }

        /// <summary>
        /// Builds internal lookup dictionaries from the serialized surface list for fast runtime queries.
        /// </summary>
        private void BuildLookup()
        {
            tileLookup = new Dictionary<TileBase, SurfaceEntry>();
            typeSwitchLookup = new Dictionary<SurfaceType, AK.Wwise.Switch>();

            if (surfaces == null) return;

            for (int i = 0; i < surfaces.Count; i++)
            {
                SurfaceEntry entry = surfaces[i];

                if (!typeSwitchLookup.ContainsKey(entry.SurfaceType))
                {
                    typeSwitchLookup.Add(entry.SurfaceType, entry.WwiseSwitch);
                }

                if (entry.Tiles == null) continue;

                for (int j = 0; j < entry.Tiles.Length; j++)
                {
                    TileBase tile = entry.Tiles[j];

                    if (tile != null && !tileLookup.ContainsKey(tile))
                    {
                        tileLookup.Add(tile, entry);
                    }
                }
            }
        }
    }
}