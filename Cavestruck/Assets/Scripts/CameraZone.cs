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
    [Tooltip("Duraci�n del movimiento de la c�mara en segundos")]
    public float transitionDuration = 1f;

    [Tooltip("Si est� marcado, el movimiento ser� relativo a la posici�n actual de la c�mara")]
    public bool isRelativeMovement = true;

    [Tooltip("Referencia a la c�mara principal (si est� vac�o, usar� Camera.main)")]
    public Camera targetCamera;

    private Vector3 targetPosition;
    private bool isTransitioning = false;
    private float transitionTime = 0f;
    private Vector3 initialCameraPosition;

    private void Start()
    {
        // Si no se asigna una c�mara espec�fica, usar la c�mara principal
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isTransitioning)
        {
            // Manejar la transici�n suave de la c�mara
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
            Debug.LogWarning("No hay c�mara asignada para mover.");
            return;
        }

        initialCameraPosition = targetCamera.transform.position;

        // Calcular la posici�n objetivo
        if (isRelativeMovement)
        {
            targetPosition = initialCameraPosition + new Vector3(moveX, moveY, moveZ);
        }
        else
        {
            targetPosition = new Vector3(moveX, moveY, moveZ);
        }

        // Iniciar la transici�n
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