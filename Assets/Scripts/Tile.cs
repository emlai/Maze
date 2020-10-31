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
    public bool immovable;
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
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        dragState = DragState.NotDragging;
    }

    void OnMouseDrag()
    {
        if (maze.player.IsMoving)
            return;

        var dragPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)) + offset;
        var diff = dragPosition - new Vector3(gridPosition.x, 0, gridPosition.y);
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
            tile.SetVisualPosition(new Vector3(tile.gridPosition.x, 0, tile.gridPosition.y));

        if (dragState == DragState.DraggingHorizontally && diff.x != 0)
        {
            var sign = (int) math.sign(diff.x);
            var pushedTiles = 0;

            for (var i = 0; i < 100; i++)
            {
                var nextPosition = new Vector2Int(gridPosition.x + i * sign, gridPosition.y);
                var nextTile = maze.tiles.FirstOrDefault(tile => tile.gridPosition == nextPosition);
                if (!nextTile) continue;
                if (nextTile.immovable) break;

                var totalDiff = diff.x + pushedTiles * sign;

                for (var j = 0; j < 100; j++)
                {
                    var farthestPushPosition = new Vector2Int(gridPosition.x + j * sign, gridPosition.y);
                    var farthestPushedTile = maze.tiles.FirstOrDefault(tile => tile.gridPosition == farthestPushPosition);
                    if (farthestPushedTile && farthestPushedTile.immovable)
                    {
                        var numberOfTilesBetween = maze.tiles.Count(tile => tile.gridPosition.x.IsBetween(gridPosition.x, farthestPushPosition.x) && tile.gridPosition.y == gridPosition.y);
                        var limit = (farthestPushedTile.gridPosition.x - gridPosition.x) - (1 + numberOfTilesBetween - pushedTiles) * sign;
                        totalDiff = totalDiff > 0 ? math.min(totalDiff, limit) : math.max(totalDiff, limit);
                        break;
                    }
                }

                if (math.abs(gridPosition.x - nextTile.gridPosition.x) < math.abs(totalDiff))
                {
                    nextTile.SetVisualPosition(new Vector3(gridPosition.x + totalDiff, 0, gridPosition.y));
                    pushedTiles++;
                }
            }
        }
        else if (dragState == DragState.DraggingVertically && diff.z != 0)
        {
            var sign = (int) math.sign(diff.z);
            var pushedTiles = 0;

            for (var i = 0; i < 100; i++)
            {
                var nextPosition = new Vector2Int(gridPosition.x, gridPosition.y + i * sign);
                var nextTile = maze.tiles.FirstOrDefault(tile => tile.gridPosition == nextPosition);
                if (!nextTile) continue;
                if (nextTile.immovable) break;

                var totalDiff = diff.z + pushedTiles * sign;

                for (var j = 0; j < 100; j++)
                {
                    var farthestPushPosition = new Vector2Int(gridPosition.x, gridPosition.y + j * sign);
                    var farthestPushedTile = maze.tiles.FirstOrDefault(tile => tile.gridPosition == farthestPushPosition);
                    if (farthestPushedTile && farthestPushedTile.immovable)
                    {
                        var numberOfTilesBetween = maze.tiles.Count(tile => tile.gridPosition.y.IsBetween(gridPosition.y, farthestPushPosition.y) && tile.gridPosition.x == gridPosition.x);
                        var limit = (farthestPushedTile.gridPosition.y - gridPosition.y) - (1 + numberOfTilesBetween - pushedTiles) * sign;
                        totalDiff = totalDiff > 0 ? math.min(totalDiff, limit) : math.max(totalDiff, limit);
                        break;
                    }
                }

                if (math.abs(gridPosition.y - nextTile.gridPosition.y) < math.abs(totalDiff))
                {
                    nextTile.SetVisualPosition(new Vector3(gridPosition.x, 0, gridPosition.y + totalDiff));
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

    void SetVisualPosition(Vector3 position, bool snap = false)
    {
        if (maze.player.gridPosition == gridPosition)
            maze.player.transform.position = new Vector3(position.x, maze.player.transform.position.y, position.z);

        transform.position = position;
    }
}
