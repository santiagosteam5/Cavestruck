using System.Collections;
using UnityEngine;

public class HorizontalCubeEnemy : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private float detectionRange = 2.0f;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float returnSpeed = 4.0f;
    [SerializeField] private float resetDelay = 2.0f;
    [SerializeField] private float cooldownTime = 1.0f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private float wallCheckOffset = 0.1f; // Peque�o offset para evitar penetraci�n

    private Vector3 originalPosition;
    private bool isActive = true;
    private bool isReturning = false;
    private bool isCooldown = false;
    private bool isMoving = false;
    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        originalPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        ConfigureRigidbody();
    }

    private void ConfigureRigidbody()
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.mass = 10f;
        rb.linearDamping = 1f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        col.isTrigger = false;
    }

    void Update()
    {
        if (isActive && !isReturning && !isMoving && !isCooldown)
        {
            DetectPlayer();
        }

        if (isReturning)
        {
            ReturnToOriginalPosition();
        }
    }

    private void DetectPlayer()
    {
        RaycastHit hit;
        // Disparamos un rayo hacia la izquierda (eje X negativo)
        if (Physics.Raycast(transform.position, Vector3.left, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(playerTag))
            {
                StartMoving();
            }
        }
    }

    private void StartMoving()
    {
        isActive = false;
        isMoving = true;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.left * moveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isMoving) return;

        // Verifica todos los puntos de contacto
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.CompareTag(wallTag))
            {
                HandleWallCollision(contact.point);
                return;
            }
        }
    }

    private void HandleWallCollision(Vector3 contactPoint)
    {
        isMoving = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Ajusta la posici�n para evitar penetraci�n
        Vector3 newPosition = transform.position;
        newPosition.x = contactPoint.x + col.bounds.extents.x + wallCheckOffset;
        transform.position = newPosition;

        StartCoroutine(WaitAndReturn());
    }

    private IEnumerator WaitAndReturn()
    {
        yield return new WaitForSeconds(resetDelay);
        isReturning = true;
    }

    private void ReturnToOriginalPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
        {
            transform.position = originalPosition;
            isReturning = false;
            StartCoroutine(StartCooldown());
        }
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
        isActive = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.left * detectionRange);
    }
}