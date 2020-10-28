using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class Maze : MonoBehaviour
{
    public Tile[] tiles;
    public int size;
    public GameObject tilePrefab;
    public Player player;

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
                tile.tileType = tileType;
                tile.gridPosition = new Vector2Int(x, y);
            }
        }
    }

    void Awake()
    {
        tiles = new Tile[size * size];

        for (var i = 0; i < size * size; i++)
            tiles[i] = transform.GetChild(i).GetComponent<Tile>();

        player = FindObjectOfType<Player>();
    }

    public Tile GetTile(Vector2Int position)
    {
        return tiles.FirstOrDefault(tile => tile.gridPosition == position);
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
