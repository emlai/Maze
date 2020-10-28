using System;
using System.Collections.Generic;
using System.Linq;
using static Unity.Mathematics.math;
using UnityEngine;

[Flags]
public enum TileType
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public class Tile : MonoBehaviour
{
    public Maze maze;
    public TileType tileType;
    public Vector2Int gridPosition;
    Vector3 originalPosition;
    Vector3 screenPoint;
    Vector3 offset;
    bool didDrag;

    void OnMouseDown()
    {
        originalPosition = transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(originalPosition);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        didDrag = false;
    }

    void OnMouseDrag()
    {
        var curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        var dragPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        var diff = dragPosition - originalPosition;
        var threshold = 0.1f;

        if (abs(diff.x) >= threshold || abs(diff.z) >= threshold)
            didDrag = true;
        else
            return;

        maze.player.dragging = true;

        foreach (var tile in maze.tiles)
        {
            tile.SetPosition(new Vector3(tile.gridPosition.x, 0, tile.gridPosition.y));
        }

        if (abs(diff.x) > abs(diff.z))
        {
            foreach (var tile in maze.tiles)
                if (tile.gridPosition.y == (int) originalPosition.z)
                    tile.SetPosition(new Vector3(tile.gridPosition.x + diff.x, 0, originalPosition.z));
        }
        else
        {
            foreach (var tile in maze.tiles)
                if (tile.gridPosition.x == (int) originalPosition.x)
                    tile.SetPosition(new Vector3(originalPosition.x, 0, tile.gridPosition.y + diff.z));
        }
    }

    List<Tile> FindPath(Tile from, Tile to)
    {
        static TileType GetTileType(Vector2Int direction)
        {
            if (direction == Vector2Int.up)
                return TileType.Up;
            else if (direction == Vector2Int.down)
                return TileType.Down;
            else if (direction == Vector2Int.left)
                return TileType.Left;
            else if (direction == Vector2Int.right)
                return TileType.Right;
            else
                return TileType.None;
        }

        var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        var visited = new List<Tile>();
        var queue = new Queue<List<Tile>>();
        queue.Enqueue(new List<Tile> { from });
        visited.Add(from);

        while (queue.Any())
        {
            var path = queue.Dequeue();
            var current = path.Last();
            if (current == to)
                return path;

            foreach (var direction in directions)
            {
                if (current.tileType.HasFlag(GetTileType(direction)))
                {
                    var neighbor = maze.GetTile(current.gridPosition + direction);
                    if (neighbor && !visited.Contains(neighbor) && neighbor.tileType.HasFlag(GetTileType(-direction)))
                    {
                        visited.Add(neighbor);
                        var newPath = new List<Tile>(path);
                        newPath.Add(neighbor);
                        queue.Enqueue(newPath);
                    }
                }
            }
        }

        return null;
    }

    void OnMouseUp()
    {
        maze.player.dragging = false;

        if (!didDrag)
        {
            var sourceTile = maze.GetTile(maze.player.gridPosition);
            var foundPath = FindPath(sourceTile, this);
            if (foundPath != null) StartCoroutine(maze.player.TracePath(foundPath));
            return;
        }

        Vector2Int? newPlayerPosition = null;

        foreach (var tile in maze.tiles)
        {
            var roundedPosition = round(tile.transform.position);
            var newGridPosition = new Vector2Int((int) roundedPosition.x, (int) roundedPosition.z);

            if (maze.player.gridPosition == tile.gridPosition && newPlayerPosition == null)
                newPlayerPosition = newGridPosition;

            tile.gridPosition = newGridPosition;

            // TODO: Snap smoothly instead of instantly.
            tile.transform.position = roundedPosition;
        }

        if (newPlayerPosition != null)
            maze.player.SetPosition(newPlayerPosition.Value);
    }

    void SetPosition(Vector3 position, bool snap = false)
    {
        if (maze.player.gridPosition == gridPosition)
            maze.player.transform.position = new Vector3(position.x, maze.player.transform.position.y, position.z);

        transform.position = position;
    }
}
