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

    [Header("Continuar")]
    [SerializeField] private TMP_Text textoAvisoSinPartida; // Texto del panel 
    [SerializeField] private GameObject panelAvisoSinPartida; // Panel que se muestra si no hay partida

    private bool pasoCuentaRegresiva = false;
    private Coroutine corrutinaCuentaRegresiva;

    [Header("borrado")]
    [SerializeField] private GameObject panelBorrado;
    [SerializeField] private TMP_Text textoBorrado;

    private ObjetivoIntro objetivoIntro;
    void Start()
    {
        objetivoIntro = GetComponent<ObjetivoIntro>();
        if (toggleSonido != null && ControladorSonido.Instance != null)
        {
            toggleSonido.SetValue(ControladorSonido.Instance.SonidoActivado(), false);

            bool zurdo = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;
            toggleModoZurdo.SetValue(zurdo, false);
        }
        Debug.Log("comprobando usuario");
        if(!ControladorGuardarDatos.Instance.ExisteUsuario())
        {
            Debug.Log("no existe usuario ");
            StartCoroutine(ControladorGuardarDatos.Instance.CrearUsuarioCuandoFirebaseEsteListo(" "));
        }
        else    
            Debug.Log("si existe usuario");
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
        Debug.Log("pasando a jugar");
        if (!ControladorGuardarDatos.Instance.ExistePartida())
        {
            // 1. Iniciamos el texto del objetivo/intro (cambia el string por tu mensaje real)
            if (objetivoIntro != null)
            {
                objetivoIntro.IniciarMensaje();
            }

            // 2. Ahora sí iniciamos la corrutina que esperará a que termine
            StartCoroutine(CargarEscenaSecuencia());
            Debug.Log("Cargando corutina");
        }
        else
        {
            SceneManager.LoadScene("PantallaSeleccion");
            Debug.Log("Pasando a la escena habia partida");
        }
        
    }
    


    private IEnumerator CargarEscenaSecuencia()
    {
        // Si el script de texto existe y está ejecutando la animación, esperamos
        if (objetivoIntro != null && objetivoIntro.Escribiendo)
        {
            yield return new WaitUntil(() => !objetivoIntro.Escribiendo);
        }

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

        if (panelAjustes != null) panelAjustes.SetActive(false);//cerramos el panel de ajustes
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false); // cerramos el panel de confirmacion
        if (panelAvisoSinPartida != null) panelAvisoSinPartida.SetActive(false); // cerramos el panel de aviso de que no hay partida guardada

        StartCoroutine(PanelBorrado());
    }

    //metodo para continuar partida, si no hay partida guardada se muestra un panel con un texto personalizado
    public void ContinuarPartida() 
    {
        ControladorSonido.Instance?.ReproducirClick();

        if (ControladorGuardarDatos.Instance != null && ControladorGuardarDatos.Instance.ExistePartida())
        {
            Partida partidaGuardada = ControladorGuardarDatos.Instance.CargarPartida();

            if (partidaGuardada != null)
            {
                ConfiguracionPartida.Dificultad = partidaGuardada.Dificultad;
                ConfiguracionPartida.EsContinuacion = true;

                if (System.Enum.TryParse(partidaGuardada.ModoJuego, out GameManager.ModoJuego modoCargado))
                {
                    ConfiguracionPartida.Modo = modoCargado;
                }
                else
                {
                    ConfiguracionPartida.Modo = GameManager.ModoJuego.Carrera;
                }

                SceneManager.LoadScene("MainSecene");
            }
        }
        else
        {
            if (panelAvisoSinPartida != null)
            {
                if (textoAvisoSinPartida != null)
                {
                    textoAvisoSinPartida.text = "No hay partida guardada";
                }

                panelAvisoSinPartida.SetActive(true);

                StartCoroutine(OcultarPanelAviso());
            }
        }
    }

    private IEnumerator OcultarPanelAviso()
    {
        yield return new WaitForSeconds(1f);
        if (panelAvisoSinPartida != null)
        {
            panelAvisoSinPartida.SetActive(false);
        }
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
        yield return new WaitForSeconds(0.5f);
        panelBorrado.gameObject.SetActive(false);
    }
}