using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 5f; // Distancia configurable
    public float moveSpeed = 2f;   // Velocidad configurable

    private Vector3 startPosition;
    private bool movingForward = true;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Vector3 targetPosition = startPosition + Vector3.right * moveDistance;

        if (movingForward)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                movingForward = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);

            if (transform.position == startPosition)
            {
                movingForward = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Hacer que el jugador sea hijo de la plataforma
            collision.collider.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Quitar al jugador como hijo de la plataforma
            collision.collider.transform.SetParent(null);
        }
    }
}
