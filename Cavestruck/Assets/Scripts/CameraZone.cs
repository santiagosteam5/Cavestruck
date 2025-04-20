using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [Tooltip("Desplazamiento en el eje X")]
    public float moveX = 0f;

    [Tooltip("Desplazamiento en el eje Y")]
    public float moveY = 0f;

    [Tooltip("Desplazamiento en el eje Z")]
    public float moveZ = 0f;

    [Header("Transition Settings")]
    [Tooltip("Duración del movimiento de la cámara en segundos")]
    public float transitionDuration = 1f;

    [Tooltip("Si está marcado, el movimiento será relativo a la posición actual de la cámara")]
    public bool isRelativeMovement = true;

    [Tooltip("Referencia a la cámara principal (si está vacío, usará Camera.main)")]
    public Camera targetCamera;

    private Vector3 targetPosition;
    private bool isTransitioning = false;
    private float transitionTime = 0f;
    private Vector3 initialCameraPosition;

    private void Start()
    {
        // Si no se asigna una cámara específica, usar la cámara principal
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isTransitioning)
        {
            // Manejar la transición suave de la cámara
            transitionTime += Time.deltaTime;
            float progress = Mathf.Clamp01(transitionTime / transitionDuration);

            targetCamera.transform.position = Vector3.Lerp(initialCameraPosition, targetPosition, progress);

            if (progress >= 1f)
            {
                isTransitioning = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el jugador ha entrado en el trigger
        if (other.CompareTag("Player") && !isTransitioning)
        {
            StartCameraMovement();
        }
    }

    private void StartCameraMovement()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("No hay cámara asignada para mover.");
            return;
        }

        initialCameraPosition = targetCamera.transform.position;

        // Calcular la posición objetivo
        if (isRelativeMovement)
        {
            targetPosition = initialCameraPosition + new Vector3(moveX, moveY, moveZ);
        }
        else
        {
            targetPosition = new Vector3(moveX, moveY, moveZ);
        }

        // Iniciar la transición
        isTransitioning = true;
        transitionTime = 0f;
    }

    // Dibujar un gizmo en el editor para visualizar el trigger
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}