using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Inicio : MonoBehaviour
{
    [Header("PanelAjustes")]
    [SerializeField] private GameObject panelAjustes;

    [Header("Audio")]
    [SerializeField] private Toggle toggleSonido;

    [Header("Modo zurdo")]
    [SerializeField] private Toggle toggleModoZurdo;

    void Start()
    {
        if (toggleSonido != null && ControladorSonido.Instance != null)
        {
            toggleSonido.SetIsOnWithoutNotify(ControladorSonido.Instance.SonidoActivado());
            toggleSonido.onValueChanged.RemoveAllListeners();
            toggleSonido.onValueChanged.AddListener(DesactivarSonido);
        }

        ConfigurarToggleModoZurdo();
    }

    private void ConfigurarToggleModoZurdo()
    {
        if (toggleModoZurdo == null)
        {
            return;
        }

        bool estadoGuardado = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;

        toggleModoZurdo.SetIsOnWithoutNotify(estadoGuardado);

        toggleModoZurdo.onValueChanged.RemoveAllListeners();
        toggleModoZurdo.onValueChanged.AddListener(AlternarModoZurdo);

    }

    public void ActivarAjustes()
    {
        panelAjustes.SetActive(true);
        ControladorSonido.Instance?.ReproducirClick();
    }

    public void DesactivarAjustes()
    {
        panelAjustes.SetActive(false);
        ControladorSonido.Instance?.ReproducirClick();
    }

    public void IrAJugar()
    {
        ControladorSonido.Instance?.ReproducirClick();
        SceneManager.LoadScene("PantallaSeleccion");
    }

    public void AlternarModoZurdo(bool activado)
    {
        PlayerPrefs.SetInt("ModoZurdo", activado ? 1 : 0);
        PlayerPrefs.Save();

        if (ControladorModoZurdo.Instance != null)
        {
            ControladorModoZurdo.Instance.ActivarModoZurdo(activado);
        }
        else
        {
        }

        ControladorSonido.Instance?.ReproducirClick();
    }

    public void DesactivarSonido(bool activado)
    {
        if (ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.SetSonidoActivado(activado);
        }
    }
}