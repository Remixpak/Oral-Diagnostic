using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pausa : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelConfirmacion;

    
    public void AbrirPanelPausa()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
            GameManager.Instance.PausarJuego();

        }
    }

    public void CerrarPanelPausa()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
            GameManager.Instance.ReanudarJuego();
        }
    }

    public void ReiniciarNivel()
    {
        ControladorSonido.Instance?.ReproducirClick();

        GameManager.Instance.ReiniciarPartida();
    }

    public void activarConfirmacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        panelConfirmacion.SetActive(true);
    }
    public void cerrarConfirmacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        panelConfirmacion.SetActive(false);
    }

    public void Salir()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("PantallaInicio");
    }

    void Start()
    {
        panelConfirmacion.SetActive(false);
    }

    void Update()
    {

    }
}