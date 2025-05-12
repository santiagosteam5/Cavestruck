using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject textContainer;
    public TextMeshProUGUI textDisplay;

    [Header("Configuración del texto")]
    [TextArea(3, 10)]
    public string textToDisplay;
    public float typingSpeed = 0.05f;
    public float fadeOutDuration = 1.0f;

    [Header("Configuración de flotación")]
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 1.0f;

    private bool isPlayerInTrigger = false;
    private Coroutine typingCoroutine;
    private Coroutine floatingCoroutine;
    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Asegurarse de que tengamos un CanvasGroup para el fade
        canvasGroup = textContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textContainer.AddComponent<CanvasGroup>();
        }

        // Ocultar el texto al inicio
        textContainer.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerInTrigger)
        {
            isPlayerInTrigger = true;
            ShowText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isPlayerInTrigger)
        {
            isPlayerInTrigger = false;
            HideText();
        }
    }

    private void ShowText()
    {
        // Activar el contenedor de texto
        textContainer.SetActive(true);
        originalPosition = textContainer.transform.localPosition;

        // Detener coroutinas anteriores si existen
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Iniciar nuevas coroutinas
        StartCoroutine(FadeIn());
        typingCoroutine = StartCoroutine(TypeText());
        floatingCoroutine = StartCoroutine(FloatEffect());
    }

    private void HideText()
    {
        // Detener coroutinas
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
            floatingCoroutine = null;
        }

        // Iniciar fade out
        StartCoroutine(FadeOut());
    }

    private IEnumerator TypeText()
    {
        textDisplay.text = "";
        string[] words = textToDisplay.Split(' ');
        string currentText = "";

        for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            string wordToAdd = words[wordIndex];

            // Añadir la palabra actual al texto completo
            if (wordIndex < words.Length - 1)
            {
                // Preparamos la palabra con un espacio (excepto la última)
                wordToAdd = wordToAdd + " ";
            }

            // Guardar el texto acumulado hasta ahora
            string previousText = currentText;

            // Añadir la nueva palabra con fade
            yield return StartCoroutine(FadeInWord(previousText, wordToAdd));

            // Actualizar el texto acumulado
            currentText += wordToAdd;
        }
    }

    private IEnumerator FadeInWord(string previousText, string wordToAdd)
    {
        float fadeDuration = typingSpeed * 0.8f;
        float elapsedTime = 0f;

        // Crear un color temporal con alpha 0 para la nueva palabra
        Color transparent = new Color(textDisplay.color.r, textDisplay.color.g, textDisplay.color.b, 0f);
        Color opaque = textDisplay.color;

        // Mostrar la palabra con efecto de fade
        while (elapsedTime < fadeDuration)
        {
            // Calcular alpha actual basado en el tiempo transcurrido
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            // Construir el texto con rich text para controlar la transparencia de la nueva palabra
            string colorHex = ColorUtility.ToHtmlStringRGBA(Color.Lerp(transparent, opaque, alpha));
            textDisplay.text = previousText + $"<color=#{colorHex}>{wordToAdd}</color>";

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que la palabra esté completamente visible al final
        textDisplay.text = previousText + wordToAdd;

        // Esperar un poco antes de continuar con la siguiente palabra
        yield return new WaitForSeconds(typingSpeed * 0.2f);
    }

    private IEnumerator FloatEffect()
    {
        float time = 0f;

        while (isPlayerInTrigger)
        {
            time += Time.deltaTime;

            // Calcular el desplazamiento vertical basado en una función seno
            float yOffset = Mathf.Sin(time * floatFrequency) * floatAmplitude;

            // Aplicar el desplazamiento
            textContainer.transform.localPosition = originalPosition + new Vector3(0f, yOffset, 0f);

            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;

        // Fade in durante 0.5 segundos
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        // Fade out
        while (elapsedTime < fadeOutDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        textContainer.SetActive(false);
    }
}