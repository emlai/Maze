using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2Int gridPosition;
    public float speed;
    public bool dragging;

    void Start()
    {
        gridPosition = new Vector2Int((int) transform.position.x, (int) transform.position.z);
    }

    void Update()
    {
        if (!dragging)
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(gridPosition.x, transform.position.y, gridPosition.y), speed * Time.deltaTime);
    }

    public void SetPosition(Vector2Int position)
    {
        gridPosition = position;
    }
}
