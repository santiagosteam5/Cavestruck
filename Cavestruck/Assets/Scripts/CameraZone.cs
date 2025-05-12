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

    [Header("Invisible Wall Settings")]
    [Tooltip("Prefab del muro invisible (si est� vac�o, se crear� uno autom�ticamente)")]
    public GameObject invisibleWallPrefab;
    [Tooltip("Escala del muro invisible")]
    public Vector3 wallScale = new Vector3(5f, 3f, 0.1f);
    [Tooltip("Desplazamiento del muro respecto al trigger en X")]
    public float wallOffsetX = 0f;
    [Tooltip("Desplazamiento del muro respecto al trigger en Y")]
    public float wallOffsetY = 0f;
    [Tooltip("Desplazamiento del muro respecto al trigger en Z")]
    public float wallOffsetZ = 0f;

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
        if (isTransitioning && targetCamera != null)
        {
            // Manejar la transici�n suave de la c�mara
            transitionTime += Time.deltaTime;
            float progress = Mathf.Clamp01(transitionTime / transitionDuration);

            // Usar una funci�n de suavizado para hacer la transici�n m�s natural
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Aplicar el movimiento a la c�mara
            targetCamera.transform.position = Vector3.Lerp(initialCameraPosition, targetPosition, smoothProgress);

            // Debug log para verificar que la c�mara se est� moviendo
            if (progress % 0.1f <= 0.01f)
            {
                Debug.Log("Camera moving: " + progress + " Current Pos: " + targetCamera.transform.position + " Target Pos: " + targetPosition);
            }

            if (progress >= 1f)
            {
                isTransitioning = false;
                Debug.Log("Camera transition completed. Final position: " + targetCamera.transform.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el jugador ha entrado en el trigger
        if (other.CompareTag("Player") && !isTransitioning)
        {
            // Iniciamos el movimiento de la c�mara
            StartCameraMovement();

            // Creamos el muro invisible
            CreateInvisibleWall();

            // Destruimos solo el componente Collider para que no se active nuevamente
            // pero el objeto sigue existiendo para completar la animaci�n de la c�mara
            Destroy(GetComponent<Collider>());

            // Programa la destrucci�n del objeto completo despu�s de que termine la transici�n
            Invoke("DestroyTrigger", transitionDuration + 0.1f);
        }
    }

    private void DestroyTrigger()
    {
        // Destruye el objeto completo despu�s de que la transici�n ha terminado
        Destroy(gameObject);
    }

    private void StartCameraMovement()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("No hay c�mara asignada para mover. Intentando encontrar la c�mara principal.");
            targetCamera = Camera.main;

            if (targetCamera == null)
            {
                Debug.LogError("No se pudo encontrar ninguna c�mara. El movimiento no funcionar�.");
                return;
            }
        }

        // Guardar la posici�n inicial de la c�mara
        initialCameraPosition = targetCamera.transform.position;
        Debug.Log("Posici�n inicial de la c�mara: " + initialCameraPosition);

        // Calcular la posici�n objetivo
        if (isRelativeMovement)
        {
            targetPosition = initialCameraPosition + new Vector3(moveX, moveY, moveZ);
        }
        else
        {
            targetPosition = new Vector3(moveX, moveY, moveZ);
        }

        Debug.Log("Iniciando movimiento de c�mara hacia: " + targetPosition + " (desplazamiento: " + moveX + ", " + moveY + ", " + moveZ + ")");

        // Asegurarnos de que los valores sean diferentes para que haya movimiento
        if (Vector3.Distance(initialCameraPosition, targetPosition) < 0.001f)
        {
            Debug.LogWarning("La posici�n objetivo es muy cercana a la posici�n inicial. Es posible que no se note el movimiento.");
        }

        // Iniciar la transici�n
        isTransitioning = true;
        transitionTime = 0f;
    }

    private void CreateInvisibleWall()
    {
        GameObject wall;

        // Si hay un prefab asignado, lo instanciamos
        if (invisibleWallPrefab != null)
        {
            wall = Instantiate(invisibleWallPrefab);
        }
        // Si no hay prefab, creamos un muro invisible b�sico
        else
        {
            wall = new GameObject("InvisibleWall");

            // A�adir collider
            BoxCollider wallCollider = wall.AddComponent<BoxCollider>();
            wallCollider.isTrigger = false; // Es un collider s�lido, no un trigger

            // Agregar un MeshRenderer invisible para debugging (opcional)
            MeshRenderer renderer = wall.AddComponent<MeshRenderer>();
            renderer.enabled = false; // Invisible en tiempo de ejecuci�n

            // Agregar un MeshFilter con un cubo simple
            MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateCubeMesh();

            // Agregar Rigidbody para que el muro sea est�tico
            Rigidbody rb = wall.AddComponent<Rigidbody>();
            rb.isKinematic = true; // El muro no ser� afectado por la f�sica
            rb.useGravity = false;
        }

        // Configurar la posici�n del muro basada en la posici�n del trigger y el offset
        wall.transform.position = transform.position + new Vector3(wallOffsetX, wallOffsetY, wallOffsetZ);

        // Configurar la escala del muro
        wall.transform.localScale = wallScale;

        // Configurar la rotaci�n para que el muro est� correctamente orientado
        wall.transform.rotation = transform.rotation;

        // Establecer una etiqueta para identificar f�cilmente el muro
        wall.tag = "InvisibleWall";
    }

    // M�todo auxiliar para crear un mesh de cubo simple
    private Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "InvisibleWallMesh";

        // V�rtices de un cubo de 1x1x1
        Vector3[] vertices = {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f)
        };

        // Triangulos (6 caras, 2 triangulos por cara, 3 indices por triangulo)
        int[] triangles = {
            // Cara frontal
            0, 2, 1, 0, 3, 2,
            // Cara derecha
            1, 2, 6, 1, 6, 5,
            // Cara trasera
            5, 6, 7, 5, 7, 4,
            // Cara izquierda
            4, 7, 3, 4, 3, 0,
            // Cara superior
            3, 7, 6, 3, 6, 2,
            // Cara inferior
            4, 0, 1, 4, 1, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    // Dibujar un gizmo en el editor para visualizar el trigger
    private void OnDrawGizmos()
    {
        // Dibujar el trigger
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        // Dibujar la posici�n prevista del muro invisible
        Vector3 wallPosition = transform.position + new Vector3(wallOffsetX, wallOffsetY, wallOffsetZ);
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(wallPosition, wallScale);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallPosition, wallScale);
    }
}