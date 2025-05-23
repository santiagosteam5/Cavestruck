using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenu : MonoBehaviour
{
    public GameObject panelPausa;
    public GameObject panelSelector;
    public GameObject menuPrincipal;

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

    public void Empezar()
    {
        Time.timeScale = 1f;
        menuPrincipal.SetActive(false);
    }

    public void MostrarSelector()
    {
        panelSelector.SetActive(true);
        panelPausa.SetActive(false);
    }

    public void OcultarSelector()
    {
        panelSelector.SetActive(false);
        panelPausa.SetActive(true);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OcultarMenusAlCargar;

    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OcultarMenusAlCargar;
    }

    void OcultarMenusAlCargar(Scene scene, LoadSceneMode mode)
    {
        panelPausa.SetActive(false);
        panelSelector.SetActive(false);
        Time.timeScale = 1f;
        enPausa = false;
    }
}
