using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Inicio : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private GameObject panelConfirmacion;

    [Header("Audio")]
    [SerializeField] private ToggleSwitch toggleSonido;
    [SerializeField] private ToggleSwitch toggleModoZurdo;

    [Header("BntConfirmacion")]
    [SerializeField] private Button btnConfirmar;

    private bool pasoCuentaRegresiva = false;
    private Coroutine corrutinaCuentaRegresiva;

    [Header("borrado")]
    [SerializeField] private GameObject panelBorrado;
    [SerializeField] private TMP_Text textoBorrado;
    void Start()
    {
        if (toggleSonido != null && ControladorSonido.Instance != null)
        {
            toggleSonido.SetValue(ControladorSonido.Instance.SonidoActivado(), false);

            bool zurdo = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;
            toggleModoZurdo.SetValue(zurdo, false);
        }

        StartCoroutine(ControladorGuardarDatos.Instance.CrearUsuarioCuandoFirebaseEsteListo(" "));

        //ConfigurarToggleModoZurdo();
    }

    void Update()
    {
        
    }

    /*private void ConfigurarToggleModoZurdo()
    {
        if (toggleModoZurdo == null)
        {
            return;
        }

        bool estadoGuardado = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;

        toggleModoZurdo.SetIsOnWithoutNotify(estadoGuardado);

        toggleModoZurdo.onValueChanged.RemoveAllListeners();
        toggleModoZurdo.onValueChanged.AddListener(AlternarModoZurdo);

    }*/

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
        Debug.Log($"Inicio recibió: {activado}");
        if (ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.SetSonidoActivado(activado);
        }
        
    }

    public void AbrirConfirmacion()
    {
        panelConfirmacion.SetActive(true);
        if(!pasoCuentaRegresiva)
        {
            if(corrutinaCuentaRegresiva != null)
            {
                StopCoroutine(corrutinaCuentaRegresiva);
            }
            corrutinaCuentaRegresiva = StartCoroutine(CuentaRegresivaConfirmacion());
        }
        else
        {
            btnConfirmar.interactable = true;
            btnConfirmar.GetComponentInChildren<TMP_Text>().text = "Confirmar";
        }
    }
    public void CerrarConfirmacion()
    {
        if (!pasoCuentaRegresiva && corrutinaCuentaRegresiva != null)
        {
            StopCoroutine(corrutinaCuentaRegresiva);
            corrutinaCuentaRegresiva = null;
        }

        panelConfirmacion.SetActive(false);
    }

    public void BorrarPartida()
    {
        if(ControladorGuardarDatos.Instance.ExistePartida())
            textoBorrado.text = "Partida eliminada con éxito";
        else
            textoBorrado.text = "No existe partida";
        ControladorGuardarDatos.Instance.EliminarPartida();
        StartCoroutine(PanelBorrado());
    }

    private IEnumerator CuentaRegresivaConfirmacion()
    {
        btnConfirmar.interactable = false;
        int tiempoRestante = 5;
        while(tiempoRestante > 0)
        {
            btnConfirmar.GetComponentInChildren<TMP_Text>().text = $"({tiempoRestante})";
            yield return new WaitForSeconds(1f);
            tiempoRestante--;
        }

        btnConfirmar.GetComponentInChildren<TMP_Text>().text = "Confirmar";
        btnConfirmar.interactable = true;
        pasoCuentaRegresiva = true;
    }


    private IEnumerator PanelBorrado()
    {
        
        panelBorrado.SetActive(true);
        yield return new WaitForSeconds(2f);
        panelBorrado.gameObject.SetActive(false);
    }
}