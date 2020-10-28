using System;
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

        foreach (var tile in maze.tiles)
        {
            tile.SetPosition(new Vector3(tile.gridPosition.x, 0, tile.gridPosition.y));
        }

        if (abs(diff.x) > abs(diff.z))
        {
            foreach (var tile in maze.tiles)
            {
                if (tile.gridPosition.y == (int) originalPosition.z)
                {
                    tile.SetPosition(new Vector3(tile.gridPosition.x + diff.x, 0, originalPosition.z));
                }
            }
        }
        else
        {
            foreach (var tile in maze.tiles)
            {
                if (tile.gridPosition.x == (int) originalPosition.x)
                {
                    tile.SetPosition(new Vector3(originalPosition.x, 0, tile.gridPosition.y + diff.z));
                }
            }
        }
    }

    void OnMouseUp()
    {
        if (!didDrag)
        {
            var playerPosition = new Vector2Int((int) maze.player.transform.position.x, (int) maze.player.transform.position.z);
            var sourceTile = maze.tiles.First(tile => tile.gridPosition == playerPosition);
            var diff = gridPosition - playerPosition;
            var allowMove = false;

            if (diff == Vector2Int.up)
                allowMove = sourceTile.tileType.HasFlag(TileType.Up) && tileType.HasFlag(TileType.Down);
            else if (diff == Vector2Int.down)
                allowMove = sourceTile.tileType.HasFlag(TileType.Down) && tileType.HasFlag(TileType.Up);
            else if (diff == Vector2Int.left)
                allowMove = sourceTile.tileType.HasFlag(TileType.Left) && tileType.HasFlag(TileType.Right);
            else if (diff == Vector2Int.right)
                allowMove = sourceTile.tileType.HasFlag(TileType.Right) && tileType.HasFlag(TileType.Left);

            if (allowMove)
                maze.player.transform.position = new Vector3(gridPosition.x, maze.player.transform.position.y, gridPosition.y);
        }

        foreach (var tile in maze.tiles)
        {
            var roundedPosition = round(tile.transform.position);
            tile.gridPosition = new Vector2Int((int) roundedPosition.x, (int) roundedPosition.z);

            // TODO: Snap smoothly instead of instantly.
            tile.SetPosition(roundedPosition);
        }
    }

    void SetPosition(Vector3 position)
    {
        if (maze.player.transform.position.x == transform.position.x && maze.player.transform.position.z == transform.position.z)
            maze.player.transform.position = new Vector3(position.x, maze.player.transform.position.y, position.z);

        transform.position = position;
    }
}
