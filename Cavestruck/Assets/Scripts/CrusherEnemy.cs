using System.Collections;
using UnityEngine;

public class CubeEnemy : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float detectionRange = 2.0f;
    [SerializeField] private float fallSpeed = 10.0f;
    [SerializeField] private float returnSpeed = 4.0f;
    [SerializeField] private float resetDelay = 2.0f;
    [SerializeField] private float cooldownTime = 1.0f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float groundCheckOffset = 0.1f; // Pequeño offset para evitar flotación

    private Vector3 originalPosition;
    private bool isActive = true;
    private bool isReturning = false;
    private bool isCooldown = false;
    private bool isFalling = false;
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
        rb.mass = 10f; // Masa suficiente para evitar rebotes exagerados
        rb.linearDamping = 1f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        col.isTrigger = false;
    }

    void Update()
    {
        if (isActive && !isReturning && !isFalling && !isCooldown)
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
        if (Physics.Raycast(transform.position, Vector3.down, out hit, detectionRange))
        {
            if (hit.collider.CompareTag(playerTag))
            {
                StartFalling();
            }
        }
    }

    private void StartFalling()
    {
        isActive = false;
        isFalling = true;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.down * fallSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isFalling) return;

        // Verifica todos los puntos de contacto
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.CompareTag(groundTag))
            {
                HandleGroundCollision(contact.point);
                return;
            }
        }
    }

    private void HandleGroundCollision(Vector3 contactPoint)
    {
        isFalling = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Ajusta la posición para evitar hundimiento
        Vector3 newPosition = transform.position;
        newPosition.y = contactPoint.y + col.bounds.extents.y + groundCheckOffset;
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
        Gizmos.DrawRay(transform.position, Vector3.down * detectionRange);
    }
}