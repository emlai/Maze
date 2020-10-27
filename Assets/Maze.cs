using System;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[Flags]
public enum TileType
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public class Maze : MonoBehaviour
{
    public Tile[,] tiles;
    public int size;
    public GameObject tilePrefab;

    public void Generate()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        var random = new Random((uint) Environment.TickCount);

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                var tileType = TileType.None;
                if (random.NextBool()) tileType |= TileType.Up;
                if (random.NextBool()) tileType |= TileType.Down;
                if (random.NextBool()) tileType |= TileType.Left;
                if (random.NextBool()) tileType |= TileType.Right;

                var gameObject = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                gameObject.name = $"Tile ({x}, {y})";
                if (tileType != TileType.None) DestroyImmediate(gameObject.transform.Find("Center").gameObject);
                if (tileType.HasFlag(TileType.Up)) DestroyImmediate(gameObject.transform.Find("Up").gameObject);
                if (tileType.HasFlag(TileType.Down)) DestroyImmediate(gameObject.transform.Find("Down").gameObject);
                if (tileType.HasFlag(TileType.Left)) DestroyImmediate(gameObject.transform.Find("Left").gameObject);
                if (tileType.HasFlag(TileType.Right)) DestroyImmediate(gameObject.transform.Find("Right").gameObject);

                var tile = gameObject.GetComponent<Tile>();
                tile.maze = this;
                tile.gridPosition = new Vector2Int(x, y);
            }
        }
    }

    void Awake()
    {
        tiles = new Tile[size, size];

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                tiles[x, y] = transform.GetChild(x * size + y).GetComponent<Tile>();
            }
        }
    }
}

[CustomEditor(typeof(Maze))]
class MazeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            ((Maze) target).Generate();
        }
    }
}
