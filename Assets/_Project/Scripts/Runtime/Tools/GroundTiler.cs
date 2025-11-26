// Purpose: Quickly spawns a grid of ground tiles in 2D (SRP: ground tiling tool)
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public sealed class GroundTiler : MonoBehaviour
{
    [TabGroup("Setup")] [SerializeField] private GameObject tilePrefab;
    [TabGroup("Setup")] [SerializeField] private int rows = 10;
    [TabGroup("Setup")] [SerializeField] private int cols = 10;
    [TabGroup("Setup")] [SerializeField] private float tileSize = 5f;
    [TabGroup("Setup")] [SerializeField] private Vector2 origin = Vector2.zero;
    [TabGroup("Setup")] [SerializeField] private Transform parent;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    [Button("Generate Tiles")]
    private void GenerateTiles()
    {
        ClearTiles();

        if (tilePrefab == null)
        {
            Debug.LogWarning("[GroundTiler] Missing tile prefab");
            return;
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(origin.x + c * tileSize, origin.y + r * tileSize, 0f);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, parent);
                _spawned.Add(tile);
            }
        }

        Debug.Log($"[GroundTiler] Spawned {rows * cols} tiles");
    }

    [Button("Clear Tiles")]
    private void ClearTiles()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null)
            {
                DestroyImmediate(_spawned[i]);
            }
        }
        _spawned.Clear();
        Debug.Log("[GroundTiler] Cleared tiles");
    }
}
