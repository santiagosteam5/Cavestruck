using System.Collections;
using UnityEngine;

public class PowerUpEffect : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private float fadeOutSpeed = 2f;

    private Renderer[] renderers;
    private PlayerController playerController;
    private Coroutine effectCoroutine;
    private bool effectActive = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        playerController = GetComponent<PlayerController>();
        SetShaderEffect(0f); // Desactivar efecto al inicio
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            ActivateRainbowEffect();
            
        }
    }

    void Update()
    {
        if (effectActive && playerController != null && playerController.JumpedThisFrame())
        {
            DeactivateRainbowEffect();
        }
    }

    public void ActivateRainbowEffect()
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(TransitionEffect(1f, transitionSpeed));
        effectActive = true;
    }

    public void DeactivateRainbowEffect()
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(TransitionEffect(0f, fadeOutSpeed));
        effectActive = false;
    }

    private IEnumerator TransitionEffect(float targetValue, float speed)
    {
        float currentValue = renderers[0].material.GetFloat("_PowerUpActive");
        float startValue = currentValue;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * speed;
            float value = Mathf.Lerp(startValue, targetValue, progress);
            SetShaderEffect(value);
            yield return null;
        }

        SetShaderEffect(targetValue);
    }

    private void SetShaderEffect(float value)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_PowerUpActive"))
            {
                renderer.material.SetFloat("_PowerUpActive", value);
            }
        }
    }

    private void OnDestroy()
    {
        // Asegurarse de limpiar materiales instanciados
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_PowerUpActive") &&
                Application.isPlaying)
            {
                Destroy(renderer.material);
            }
        }
    }
}