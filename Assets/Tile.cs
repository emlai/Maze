using static Unity.Mathematics.math;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    Vector3 originalPosition;
    Vector3 screenPoint;
    Vector3 offset;
    public Maze maze;

    void OnMouseDown()
    {
        originalPosition = transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(originalPosition);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag()
    {
        var curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        var dragPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        var diff = dragPosition - originalPosition;

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
