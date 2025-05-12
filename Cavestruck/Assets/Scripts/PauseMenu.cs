using UnityEngine;

public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;
    public GameObject panelSelector;

    private bool enPausa = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enPausa)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        Time.timeScale = 0f;
        panelPausa.SetActive(true);
        enPausa = true;
    }

    public void Reanudar()
    {
        Time.timeScale = 1f;
        panelPausa.SetActive(false);
        panelSelector.SetActive(false);
        enPausa = false;
    }

    public void MostrarSelector()
    {
        panelPausa.SetActive(false);
        panelSelector.SetActive(true);
    }
}
