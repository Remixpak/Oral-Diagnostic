using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausa : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    [SerializeField] private GameObject panelPausa;

    
    public void AbrirPanelPausa()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CerrarPanelPausa()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }

    void Start()
    {

    }

    void Update()
    {

    }
}