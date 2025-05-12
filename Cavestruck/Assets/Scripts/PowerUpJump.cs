using UnityEngine;

public class RespawnablePowerUp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float respawnTime = 10f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1f;

    private Vector3 initialPosition;
    private bool isActive = true;
    private float respawnTimer = 0f;
    private Collider powerUpCollider;
    private Renderer powerUpRenderer;

    private void Awake()
    {
        powerUpCollider = GetComponent<Collider>();
        powerUpRenderer = GetComponent<Renderer>();
        initialPosition = transform.position;
    }

    private void Update()
    {
        // Animación de flotar y rotar cuando está activo
        if (isActive)
        {
            FloatAndRotate();
        }
        // Lógica de respawn cuando está inactivo
        else
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                RespawnPowerUp();
            }
        }
    }

    private void FloatAndRotate()
    {
        // Rotación suave
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Flotación vertical
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                DeactivatePowerUp();
            }
        }
    }

    private void DeactivatePowerUp()
    {
        isActive = false;
        respawnTimer = respawnTime;

        // Desactivar renderizado y colisión
        powerUpCollider.enabled = false;
        powerUpRenderer.enabled = false;

        // Opcional: Efecto de partículas al ser recolectado
        // Instantiate(collectEffect, transform.position, Quaternion.identity);

        Debug.Log("PowerUp recolectado. Reaparecerá en " + respawnTime + " segundos.");
    }

    private void RespawnPowerUp()
    {
        isActive = true;

        // Reactivar renderizado y colisión
        powerUpCollider.enabled = true;
        powerUpRenderer.enabled = true;

        // Opcional: Efecto de partículas al reaparecer
        // Instantiate(respawnEffect, transform.position, Quaternion.identity);

        Debug.Log("PowerUp ha reaparecido.");
    }

    // Método público para forzar el respawn (útil para pruebas)
    public void ForceRespawn()
    {
        if (!isActive)
        {
            RespawnPowerUp();
        }
    }
}