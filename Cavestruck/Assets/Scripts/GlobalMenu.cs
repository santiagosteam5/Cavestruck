using UnityEngine;

public class MenuGlobal : MonoBehaviour
{
    public static MenuGlobal instancia;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject); // Evita duplicados si ya existe
        }
    }
}
