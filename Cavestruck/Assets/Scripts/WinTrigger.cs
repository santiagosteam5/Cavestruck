using UnityEngine;
using UnityEngine.UI;

public class VictoryTrigger : MonoBehaviour
{
    public GameObject victoryTextUI; // Arrastra un objeto UI de texto aquí (puede ser un Text o un panel con texto)

    void Start()
    {
        if (victoryTextUI != null)
        {
            victoryTextUI.SetActive(false); // Oculta el texto al inicio
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Ganaste");
            if (victoryTextUI != null)
            {
                victoryTextUI.SetActive(true); // Muestra el texto al tocar el objeto
            }
        }
    }
}
