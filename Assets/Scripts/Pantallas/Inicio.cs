using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona la pantalla de inicio del juego, controlando la navegación principal, la apertura y cierre de paneles de ajustes, 
/// diálogos de confirmación, eliminación y reanudación de partidas guardadas, además de la configuración de opciones de audio y modo zurdo.
/// 
/// Clases dependientes que utiliza:
/// - ControladorSonido: Administra la reproducción de efectos de sonido (clics) y la configuración global de audio y música.
/// - ControladorModoZurdo: Aplica la preferencia del modo zurdo/diestro en la interfaz.
/// - ControladorGuardarDatos: Permite validar, cargar, guardar y eliminar partidas o usuarios persistentes.
/// - ObjetivoIntro: Controla las secuencias introductorias o animaciones de texto al iniciar una partida nueva.
/// - ToggleSwitch: Componente UI personalizado para alternar los estados de configuración (sonido, música, modo zurdo).
/// - ConfiguracionPartida: Clase de datos global donde se inyecta la dificultad y el modo de juego al reanudar una partida.
/// - GameManager: Define las enumeraciones y modos de juego disponibles (ej. ModoJuego.Carrera).
/// - Partida: Modelo de datos que representa una partida guardada.
/// </summary>
public class Inicio : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private GameObject panelConfirmacion;

    [Header("Audio")]
    [SerializeField] private ToggleSwitch toggleSonido;
    [SerializeField] private ToggleSwitch toggleMusica; 
    [SerializeField] private ToggleSwitch toggleModoZurdo;

    [Header("BntConfirmacion")]
    [SerializeField] private Button btnConfirmar;

    [Header("Continuar")]
    [SerializeField] private TMP_Text textoAvisoSinPartida; 
    [SerializeField] private GameObject panelAvisoSinPartida; 

    private bool pasoCuentaRegresiva = false;
    private Coroutine corrutinaCuentaRegresiva;

    [Header("borrado")]
    [SerializeField] private GameObject panelBorrado;
    [SerializeField] private TMP_Text textoBorrado;

    private ObjetivoIntro objetivoIntro;

    /// <summary>
    /// Inicializa las referencias, sincroniza los controles UI con las preferencias guardadas y valida la existencia del usuario local/remoto.
    /// </summary>
    void Start()
    {
        objetivoIntro = GetComponent<ObjetivoIntro>();

        if (toggleSonido != null && ControladorSonido.Instance != null)
        {
            toggleSonido.OnValueChanged.RemoveAllListeners(); 
            toggleSonido.SetValue(ControladorSonido.Instance.SonidoActivado(), false);
            toggleSonido.OnValueChanged.AddListener(DesactivarSonido);
        }

        if (toggleMusica != null && ControladorSonido.Instance != null)
        {
            toggleMusica.OnValueChanged.RemoveAllListeners(); 
            toggleMusica.SetValue(ControladorSonido.Instance.MusicaActivada(), false);
            toggleMusica.OnValueChanged.AddListener(DesactivarMusica);
        }

        if (toggleModoZurdo != null)
        {
            toggleModoZurdo.OnValueChanged.RemoveAllListeners(); 
            bool zurdo = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;
            toggleModoZurdo.SetValue(zurdo, false);
            toggleModoZurdo.OnValueChanged.AddListener(AlternarModoZurdo);
        }

        Debug.Log("comprobando usuario");
        if (!ControladorGuardarDatos.Instance.ExisteUsuario())
        {
            Debug.Log("no existe usuario ");
            StartCoroutine(ControladorGuardarDatos.Instance.CrearUsuarioCuandoFirebaseEsteListo(" "));
        }
        else
            Debug.Log("si existe usuario");
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Modifica el estado de habilitación de la música de fondo en la aplicación y reproduce el sonido de clic.
    /// </summary>
    /// <param name="activado">Indica si la música debe activarse (True) o desactivarse (False).</param>
    public void DesactivarMusica(bool activado)
    {
        if (ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.SetMusicaActivada(activado);
        }
        ControladorSonido.Instance?.ReproducirClick();
    }

    /// <summary>
    /// Despliega el panel de ajustes y reproduce un sonido de interacción.
    /// </summary>
    public void ActivarAjustes()
    {
        panelAjustes.SetActive(true);
        ControladorSonido.Instance?.ReproducirClick();
    }

    /// <summary>
    /// Oculta el panel de ajustes y reproduce un sonido de interacción.
    /// </summary>
    public void DesactivarAjustes()
    {
        panelAjustes.SetActive(false);
        ControladorSonido.Instance?.ReproducirClick();
    }

    /// <summary>
    /// Inicia el flujo de juego navegando a la pantalla de selección o activando la cinemática de introducción según el estado de la partida del usuario.
    /// </summary>
    public void IrAJugar()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Debug.Log("pasando a jugar");

        bool usuario = ControladorGuardarDatos.Instance.CargarUsuario() != null;
        bool partidaCompletada = usuario && ControladorGuardarDatos.Instance.CargarUsuario().PartidaTerminada;
        
        if (!partidaCompletada)
        {
            if (objetivoIntro != null)
            {
                objetivoIntro.IniciarMensaje();
            }

            StartCoroutine(CargarEscenaSecuencia());
            Debug.Log("Cargando corrutina intro");
        }
        else
        {
            SceneManager.LoadScene("PantallaSeleccion");
            Debug.Log("Partida existente y completada: Pasando a PantallaSeleccion");
        }
    }

    /// <summary>
    /// Corrutina que aguarda la finalización de la secuencia textual introductoria antes de realizar el cambio de escena a "PantallaSeleccion".
    /// </summary>
    private IEnumerator CargarEscenaSecuencia()
    {
        while (objetivoIntro != null && objetivoIntro.Escribiendo)
        {
            yield return null; 
        }

        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene("PantallaSeleccion");
    }

    /// <summary>
    /// Alterna la preferencia del modo zurdo, guardando la configuración en PlayerPrefs y notificando al controlador correspondiente.
    /// </summary>
    /// <param name="activado">Indica si el modo zurdo debe activarse (True) o desactivarse (False).</param>
    public void AlternarModoZurdo(bool activado)
    {
        PlayerPrefs.SetInt("ModoZurdo", activado ? 1 : 0);
        PlayerPrefs.Save();

        if (ControladorModoZurdo.Instance != null)
        {
            ControladorModoZurdo.Instance.ActivarModoZurdo(activado);
        }

        ControladorSonido.Instance?.ReproducirClick();
    }

    /// <summary>
    /// Modifica el estado global de activación de los efectos de sonido.
    /// </summary>
    /// <param name="activado">Indica si los efectos de sonido deben activarse (True) o desactivarse (False).</param>
    public void DesactivarSonido(bool activado)
    {
        Debug.Log($"Inicio recibió: {activado}");
        if (ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.SetSonidoActivado(activado);
        }
    }

    /// <summary>
    /// Muestra el panel emergente de confirmación e inicia un temporizador de cuenta regresiva en el botón de confirmación si no ha finalizado previamente.
    /// </summary>
    public void AbrirConfirmacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
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

    /// <summary>
    /// Oculta el panel emergente de confirmación y detiene la cuenta regresiva en curso si correspondiese.
    /// </summary>
    public void CerrarConfirmacion()
    {
        if (!pasoCuentaRegresiva && corrutinaCuentaRegresiva != null)
        {
            StopCoroutine(corrutinaCuentaRegresiva);
            corrutinaCuentaRegresiva = null;
        }

        panelConfirmacion.SetActive(false);
        ControladorSonido.Instance?.ReproducirClick();
    }

    /// <summary>
    /// Elimina los datos de la partida guardada actual, cierra paneles abiertos y muestra temporalmente la notificación de borrado.
    /// </summary>
    public void BorrarPartida()
    {
        if (ControladorGuardarDatos.Instance.ExistePartida())
            textoBorrado.text = "Partida eliminada con éxito";
        else
            textoBorrado.text = "No existe partida";
        ControladorGuardarDatos.Instance.EliminarPartida();

        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false); 
        if (panelAvisoSinPartida != null) panelAvisoSinPartida.SetActive(false); 

        StartCoroutine(PanelBorrado());
        ControladorSonido.Instance?.ReproducirClick();
    }

    /// <summary>
    /// Reanuda la partida guardada si existe, inyectando su configuración en la clase global y cargando la escena principal; de lo contrario despliega un aviso.
    /// </summary>
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

    /// <summary>
    /// Corrutina que oculta automáticamente el panel de aviso de ausencia de partida tras 1 segundo.
    /// </summary>
    private IEnumerator OcultarPanelAviso()
    {
        yield return new WaitForSeconds(1f);
        if (panelAvisoSinPartida != null)
        {
            panelAvisoSinPartida.SetActive(false);
        }
    }

    /// <summary>
    /// Corrutina que deshabilita el botón de confirmación durante 5 segundos como medida de seguridad antes de permitir una acción crítica.
    /// </summary>
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

    /// <summary>
    /// Corrutina que despliega el panel con la notificación de borrado durante medio segundo.
    /// </summary>
    private IEnumerator PanelBorrado()
    {
        panelBorrado.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        panelBorrado.gameObject.SetActive(false);
    }
}