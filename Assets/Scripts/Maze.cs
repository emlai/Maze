using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public enum DragState
{
    NotDragging,
    DraggingHorizontally,
    DraggingVertically
}

[ExecuteInEditMode]
public class Maze : MonoBehaviour
{
    public Tile[] tiles;
    public Color wallColor;
    public Color floorColor;
    public Color playerColor;
    public Color backgroundColor;
    public Player player;
    public DragState dragState;

    void OnValidate()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.sharedMaterial.name == "Floor Material")
                renderer.sharedMaterial.color = floorColor;
            else if (renderer.sharedMaterial.name == "Wall Material")
                renderer.sharedMaterial.color = wallColor;
        }

        GameObject.Find("Player").GetComponent<Renderer>().sharedMaterial.color = playerColor;
        Camera.main.backgroundColor = backgroundColor;
    }

    void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
#if UNITY_EDITOR
        foreach (var transform in Selection.transforms)
        {
            if (!transform.name.Contains("Tile") && !transform.name.Contains("Player")) continue;
            var snappedPosition = math.round(transform.position);
            snappedPosition.y = 0;
            transform.position = snappedPosition;
            transform.parent = this.transform; // TODO: Fix "The root GameObject of the opened Prefab has been moved out of the Prefab Stage scene by a script."
        }
#endif
    }

    public Tile GetTile(Vector2Int position)
    {
        return tiles.FirstOrDefault(tile => tile.gridPosition == position);
    }
}
