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
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
            GameManager.Instance.PausarJuego();

        }
    }

    public void CerrarPanelPausa()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
            GameManager.Instance.ReanudarJuego();
        }
    }

    public void ReiniciarNivel()
    {
       GameManager.Instance.ReiniciarPartida();
    }

    public void activarConfirmacion()
    {
        panelConfirmacion.SetActive(true);
    }
    public void cerrarConfirmacion()
    {
        panelConfirmacion.SetActive(false);
    }

    public void Salir()
    {
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