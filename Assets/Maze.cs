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
    public Color wallColor;
    public Color floorColor;
    public Color playerColor;
    public Color backgroundColor;
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

                var gameObject = PrefabUtility.InstantiatePrefab(tilePrefab, transform) as GameObject;
                gameObject.transform.position = new Vector3(x, 0, y);
                gameObject.name = $"Tile ({x}, {y})";
                if (tileType != TileType.None) gameObject.transform.Find("Center").gameObject.SetActive(false);
                if (tileType.HasFlag(TileType.Up)) gameObject.transform.Find("Up").gameObject.SetActive(false);
                if (tileType.HasFlag(TileType.Down)) gameObject.transform.Find("Down").gameObject.SetActive(false);
                if (tileType.HasFlag(TileType.Left)) gameObject.transform.Find("Left").gameObject.SetActive(false);
                if (tileType.HasFlag(TileType.Right)) gameObject.transform.Find("Right").gameObject.SetActive(false);

                var tile = gameObject.GetComponent<Tile>();
                tile.maze = this;
                tile.tileType = tileType;
                tile.gridPosition = new Vector2Int(x, y);
            }
        }
    }

    void OnValidate()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.gameObject.name == "Floor")
                renderer.sharedMaterial.color = floorColor;
            else
                renderer.sharedMaterial.color = wallColor;
        }

        GameObject.Find("Player").GetComponent<Renderer>().sharedMaterial.color = playerColor;
        Camera.main.backgroundColor = backgroundColor;
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
