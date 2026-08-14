using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla la lógica y los paneles de la interfaz de pausa durante la partida.
/// Gestiona la congelación/reanudación del tiempo de juego a través de GameManager, 
/// la navegación hacia subpaneles modales (como el de confirmación de salida) y la transición a la escena principal.
/// 
/// Clases y componentes que utiliza:
/// - MonoBehaviour / SceneManager (UnityEngine): Ciclo de vida del script y carga de la escena del menú ("PantallaInicio").
/// - GameObject (UnityEngine): Control de activación/desactivación de las ventanas de la UI (Panel de Pausa y Confirmación).
/// - GameManager (Singleton local): Encargado de controlar los estados del ciclo de juego (PausarJuego, ReanudarJuego, ReiniciarPartida).
/// - ControladorSonido (Singleton local): Sistema de audio para reproducir el feedback acústico al interactuar con los botones.
/// </summary>
public class Pausa : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    [Tooltip("Objeto raíz del panel visual de pausa.")]
    [SerializeField] private GameObject panelPausa;

    [Tooltip("Objeto raíz del subpanel modal de confirmación (ej: confirmar salir de la partida).")]
    [SerializeField] private GameObject panelConfirmacion;

    /// <summary>
    /// Método de inicialización de Unity. 
    /// Asegura que el panel modal de confirmación comience oculto al cargar el componente.
    /// </summary>
    void Start()
    {
        if (panelConfirmacion != null)
            panelConfirmacion.SetActive(false);
    }

    /// <summary>
    /// Despliega el panel de pausa, reproduce el sonido de clic 
    /// e instruye al GameManager para suspender la lógica del juego.
    /// </summary>
    public void AbrirPanelPausa()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
            GameManager.Instance.PausarJuego();
        }
    }

    /// <summary>
    /// Oculta el panel de pausa, reproduce el sonido de clic 
    /// e instruye al GameManager para reanudar el tiempo de juego.
    /// </summary>
    public void CerrarPanelPausa()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
            GameManager.Instance.ReanudarJuego();
        }
    }

    /// <summary>
    /// Reinicia la partida en curso. Reproduce el feedback de audio 
    /// y solicita al GameManager restablecer la ronda actual.
    /// </summary>
    public void ReiniciarNivel()
    {
        ControladorSonido.Instance?.ReproducirClick();
        GameManager.Instance.ReiniciarPartida();
    }

    /// <summary>
    /// Muestra el panel modal de confirmación secundaria.
    /// </summary>
    public void activarConfirmacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (panelConfirmacion != null)
            panelConfirmacion.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel modal de confirmación secundaria.
    /// </summary>
    public void cerrarConfirmacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (panelConfirmacion != null)
            panelConfirmacion.SetActive(false);
    }

    /// <summary>
    /// Restablece la velocidad del tiempo en Unity a su valor normal (1.0f) 
    /// y redirige a la escena principal ("PantallaInicio").
    /// </summary>
    public void Salir()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("PantallaInicio");
    }

    void Update()
    {

    }
}