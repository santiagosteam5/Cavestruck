using UnityEngine;
using System.Collections; // Required for IEnumerator

public class Trampoline : MonoBehaviour
{
    [Header("Trampoline Settings")]
    [SerializeField] private float bounceForce = 20f;
    [SerializeField] private float animationTime = 0.5f;
    [SerializeField] private float compressedScale = 0.5f;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private ParticleSystem bounceParticles;
     
    private AudioSource audioSource;

    private Vector3 originalScale;
    
    private bool isAnimating = false;

    private void Start()
    {
        originalScale = transform.localScale;

        // Ensure there's an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAnimating)
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply bounce force
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

                // Play effects
                if (bounceSound != null) audioSource.PlayOneShot(bounceSound);
                if (bounceParticles != null) bounceParticles.Play();

                // Start animation
                StartCoroutine(BounceAnimation());
                if (bounceSound != null) audioSource.PlayOneShot(bounceSound);
            }
        }
    }

    private IEnumerator BounceAnimation()
    {
        isAnimating = true;
        float elapsedTime = 0f;

        // Compress down
        while (elapsedTime < animationTime / 2)
        {
            transform.localScale = Vector3.Lerp(
                originalScale,
                new Vector3(originalScale.x, compressedScale, originalScale.z),
                elapsedTime / (animationTime / 2));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Return to normal
        elapsedTime = 0f;
        while (elapsedTime < animationTime / 2)
        {
            transform.localScale = Vector3.Lerp(
                new Vector3(originalScale.x, compressedScale, originalScale.z),
                originalScale,
                elapsedTime / (animationTime / 2));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }
}
