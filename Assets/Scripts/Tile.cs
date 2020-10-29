using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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

public enum DragState
{
    NotDragging,
    DraggingHorizontally,
    DraggingVertically
}

public class Tile : MonoBehaviour
{
    Maze maze;
    public TileType tileType;
    public Vector2Int gridPosition;
    Vector3 originalPosition;
    Vector3 screenPoint;
    Vector3 offset;
    DragState dragState;

    void Start()
    {
        maze = GetComponentInParent<Maze>();

        if (!transform.Find("Up").gameObject.activeSelf) tileType |= TileType.Up;
        if (!transform.Find("Down").gameObject.activeSelf) tileType |= TileType.Down;
        if (!transform.Find("Left").gameObject.activeSelf) tileType |= TileType.Left;
        if (!transform.Find("Right").gameObject.activeSelf) tileType |= TileType.Right;

        gridPosition = new Vector2Int((int) transform.position.x, (int) transform.position.z);
    }

    void OnMouseDown()
    {
        originalPosition = transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(originalPosition);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        dragState = DragState.NotDragging;
    }

    void OnMouseDrag()
    {
        if (maze.player.IsMoving)
            return;

        var curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        var dragPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        var diff = dragPosition - originalPosition;
        var threshold = 0.1f;

        if (math.abs(diff.x) >= threshold || math.abs(diff.z) >= threshold)
        {
            if (dragState == DragState.NotDragging)
                dragState = math.abs(diff.x) > math.abs(diff.z) ? DragState.DraggingHorizontally : DragState.DraggingVertically;
        }
        else
            dragState = DragState.NotDragging;

        maze.player.dragging = true;

        foreach (var tile in maze.tiles)
            tile.SetPosition(new Vector3(tile.gridPosition.x, 0, tile.gridPosition.y));

        if (dragState == DragState.DraggingHorizontally && diff.x != 0)
        {
            var pushedTiles = 0;

            for (var i = 0; i < 100; i++)
            {
                var nextPosition = new Vector2Int(this.gridPosition.x + i * (int) math.sign(diff.x), (int) originalPosition.z);
                var nextTile = maze.tiles.FirstOrDefault(tile => tile.gridPosition == nextPosition);
                if (nextTile)
                {
                    var totalDiff = diff.x + math.sign(diff.x) * pushedTiles;

                    if (math.abs(this.gridPosition.x - nextTile.gridPosition.x) < math.abs(totalDiff))
                        nextTile.SetPosition(new Vector3(this.gridPosition.x + totalDiff, 0, originalPosition.z));

                    pushedTiles++;
                }
            }
        }
        else if (dragState == DragState.DraggingVertically && diff.z != 0)
        {
            var pushedTiles = 0;

            for (var i = 0; i < 100; i++)
            {
                var nextPosition = new Vector2Int((int) originalPosition.x, this.gridPosition.y + i * (int) math.sign(diff.z));
                var nextTile = maze.tiles.FirstOrDefault(tile => tile.gridPosition == nextPosition);
                if (nextTile)
                {
                    var totalDiff = diff.z + math.sign(diff.z) * pushedTiles;

                    if (math.abs(this.gridPosition.y - nextTile.gridPosition.y) < math.abs(totalDiff))
                        nextTile.SetPosition(new Vector3(originalPosition.x, 0, this.gridPosition.y + totalDiff));

                    pushedTiles++;
                }
            }
        }
    }

    List<Tile> FindPath(Tile from, Tile to)
    {
        static TileType GetTileType(Vector2Int direction)
        {
            if (direction == Vector2Int.up) return TileType.Up;
            if (direction == Vector2Int.down) return TileType.Down;
            if (direction == Vector2Int.left) return TileType.Left;
            if (direction == Vector2Int.right) return TileType.Right;
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

        if (dragState == DragState.NotDragging)
        {
            var sourceTile = maze.GetTile(maze.player.gridPosition);
            var foundPath = FindPath(sourceTile, this);
            if (foundPath != null) maze.player.TracePath(foundPath);
            return;
        }

        Vector2Int? newPlayerPosition = null;

        foreach (var tile in maze.tiles)
        {
            var roundedPosition = math.round(tile.transform.position);
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
