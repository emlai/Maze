using System;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[Flags]
enum Tile
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public class Maze : MonoBehaviour
{
    Tile[,] tiles;
    public int size;
    public GameObject tilePrefab;

    public void Generate()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        tiles = new Tile[size, size];
        var random = new Random((uint) Environment.TickCount);

        for (var x = 0; x < tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tiles.GetLength(1); y++)
            {
                var tile = Tile.None;
                if (random.NextBool()) tile |= Tile.Up;
                if (random.NextBool()) tile |= Tile.Down;
                if (random.NextBool()) tile |= Tile.Left;
                if (random.NextBool()) tile |= Tile.Right;
                tiles[x, y] = tile;

                var gameObject = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                gameObject.name = $"Tile ({x}, {y})";
                if (tile != Tile.None) DestroyImmediate(gameObject.transform.Find("Center").gameObject);
                if (tile.HasFlag(Tile.Up)) DestroyImmediate(gameObject.transform.Find("Up").gameObject);
                if (tile.HasFlag(Tile.Down)) DestroyImmediate(gameObject.transform.Find("Down").gameObject);
                if (tile.HasFlag(Tile.Left)) DestroyImmediate(gameObject.transform.Find("Left").gameObject);
                if (tile.HasFlag(Tile.Right)) DestroyImmediate(gameObject.transform.Find("Right").gameObject);
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
