using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    Maze maze;
    public Vector2Int gridPosition;
    public float speed;
    public bool dragging;
    Coroutine currentAction;
    public Vector3 TargetPosition => new Vector3(gridPosition.x, transform.position.y, gridPosition.y);
    public bool IsMoving => currentAction != null;

    void Start()
    {
        maze = GetComponentInParent<Maze>();
        gridPosition = new Vector2Int((int) transform.position.x, (int) transform.position.z);
    }

    void Update()
    {
        if (!dragging)
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, speed * Time.deltaTime);

        if (!IsMoving)
        {
            if (Input.GetKey(KeyCode.W)) TryToMove(gridPosition + Vector2Int.up);
            if (Input.GetKey(KeyCode.S)) TryToMove(gridPosition + Vector2Int.down);
            if (Input.GetKey(KeyCode.A)) TryToMove(gridPosition + Vector2Int.left);
            if (Input.GetKey(KeyCode.D)) TryToMove(gridPosition + Vector2Int.right);
        }
    }

    public void TryToMove(Vector2Int to)
    {
        if (dragging) return;
        var source = maze.GetTile(maze.player.gridPosition);
        var target = maze.GetTile(to);
        if (!target) return;
        var foundPath = maze.player.FindPath(source, target);
        if (foundPath != null) maze.player.TracePath(foundPath);
    }

    public List<Tile> FindPath(Tile from, Tile to)
    {
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
                if (current.tileType.HasFlag(Tile.GetTileType(direction)))
                {
                    var neighbor = maze.GetTile(current.gridPosition + direction);
                    if (neighbor && neighbor.tileType.HasFlag(Tile.GetTileType(-direction)) && !visited.Contains(neighbor))
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

    public void TracePath(List<Tile> path)
    {
        IEnumerator DoTracePath()
        {
            foreach (var tile in path)
            {
                SetGridPosition(tile.gridPosition);
                yield return new WaitUntil(() => transform.position == TargetPosition);
            }

            currentAction = null;
        }

        if (currentAction != null)
            StopCoroutine(currentAction);

        currentAction = StartCoroutine(DoTracePath());
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }
}
