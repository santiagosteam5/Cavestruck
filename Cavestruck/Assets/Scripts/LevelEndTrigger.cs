using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelEndSequence : MonoBehaviour
{
    public string nextSceneName;
    public TextMeshProUGUI victoryText;
    public Image fadeImage;
    public AudioSource victoryAudio;
    public AudioSource backgroundMusic; // Nueva variable
    public float delayBeforeFade = 2f;
    public float fadeDuration = 2f;
    public float delayBeforeSceneLoad = 1f;

    private bool hasEnded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasEnded && other.CompareTag("Player"))
        {
            hasEnded = true;
            StartCoroutine(EndLevelSequence());
        }
    }

    IEnumerator EndLevelSequence()
    {
        // Mostrar texto de victoria
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.text = "LEVEL COMPLETE!";
        }

        // Detener música de fondo
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        // Reproducir canción de victoria
        if (victoryAudio != null)
        {
            victoryAudio.Play();
        }

        // Esperar antes del fundido
        yield return new WaitForSeconds(delayBeforeFade);

        // Fundido a negro
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color color = fadeImage.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
        }

        // Espera adicional y carga la siguiente escena
        yield return new WaitForSeconds(delayBeforeSceneLoad);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
