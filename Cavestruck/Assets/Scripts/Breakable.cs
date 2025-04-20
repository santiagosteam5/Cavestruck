using UnityEngine;
using System.Collections; // Required for IEnumerator

public class RompibleTile3D : MonoBehaviour
{
    private Renderer objectRenderer;
    private Collider objectCollider;
    private Color originalColor;

    [SerializeField] private float fadeTime = 2f;
    [SerializeField] private float respawnTime = 5f;

    private bool isActive = true;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
        originalColor = objectRenderer.material.color;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isActive && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(BreakTile());
        }
    }

    private IEnumerator BreakTile()
    {
        isActive = false;

        // Fade out effect
        float timer = 0f;
        while (timer < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            objectRenderer.material.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        // Disable completely
        objectCollider.enabled = false;
        objectRenderer.enabled = false;

        // Wait for respawn
        yield return new WaitForSeconds(respawnTime);

        // Re-enable
        objectCollider.enabled = true;
        objectRenderer.enabled = true;
        objectRenderer.material.color = originalColor;
        isActive = true;
    }
}