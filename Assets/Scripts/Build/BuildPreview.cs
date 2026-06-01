using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Grid;

namespace SkyHubTycoon.Build
{
    public class BuildPreview : MonoBehaviour
    {
        public GridManager grid;
        public GameObject previewTilePrefab;
        public Material validMaterial;
        public Material invalidMaterial;
        public Material warningMaterial;

        private readonly List<GameObject> activeTiles = new List<GameObject>();

        public void Show(PlacementResult result)
        {
            Clear();
            if (grid == null || previewTilePrefab == null || result.cells == null) return;

            Material material = result.valid ? (result.inefficient ? warningMaterial : validMaterial) : invalidMaterial;
            for (int i = 0; i < result.cells.Length; i++)
            {
                GameObject tile = Instantiate(previewTilePrefab, grid.GridToWorld(result.cells[i]) + Vector3.up * 0.04f, Quaternion.identity, transform);
                tile.transform.localScale = new Vector3(grid.cellSize * 0.95f, 0.03f, grid.cellSize * 0.95f);
                Renderer renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer != null && material != null) renderer.sharedMaterial = material;
                activeTiles.Add(tile);
            }
        }

        public void Clear()
        {
            for (int i = activeTiles.Count - 1; i >= 0; i--)
            {
                if (activeTiles[i] != null) Destroy(activeTiles[i]);
            }
            activeTiles.Clear();
        }
    }
}
