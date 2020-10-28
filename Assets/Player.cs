using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2Int gridPosition;
    public float speed;
    public bool dragging;
    Vector3 TargetPosition => new Vector3(gridPosition.x, transform.position.y, gridPosition.y);

    void Start()
    {
        gridPosition = new Vector2Int((int) transform.position.x, (int) transform.position.z);
    }

    void Update()
    {
        if (!dragging)
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, speed * Time.deltaTime);
    }

    public void SetPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    public IEnumerator TracePath(List<Tile> path)
    {
        foreach (var tile in path)
        {
            SetPosition(tile.gridPosition);
            yield return new WaitUntil(() => transform.position == TargetPosition);
        }
    }
}
