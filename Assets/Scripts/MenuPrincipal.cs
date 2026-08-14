using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador de la interfaz del menú principal. Gestiona el despliegue del título del juego,
/// la navegación hacia los modos de juego principales y la apertura/cierre de la ventana de ajustes
/// (preferencias de sonido y accesibilidad como el modo zurdo).
/// 
/// Clases y componentes que utiliza:
/// - MonoBehaviour (UnityEngine): Gestor del ciclo de vida y eventos del objeto en Unity.
/// - Button / Toggle / GameObject (UnityEngine.UI y UnityEngine): Componentes de UI para interacción de usuario y control de visibilidad.
/// - TMP_Text (TMPro): Renderizado de texto formateado para el título del menú.
/// - GameManager (Singleton local): Invocado para iniciar la partida en el modo deseado al pulsar "Jugar".
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Panel de la ventana modal de ajustes.")]
    [SerializeField] private GameObject panelAjustes;

    [Tooltip("Toggle para activar/desactivar la configuración de interfaz para usuarios zurdos.")]
    [SerializeField] private Toggle toggleModoZurdo;

    [Tooltip("Toggle para silenciar o activar el audio general del juego.")]
    [SerializeField] private Toggle toggleDesactivarSondio;

    [Tooltip("Botón de acción principal para iniciar la partida.")]
    [SerializeField] private Button botonJugar;

    [Tooltip("Botón para abrir el panel modal de ajustes.")]
    [SerializeField] private Button botonAjustes;

    [Tooltip("Botón para cerrar el panel modal de ajustes.")]
    [SerializeField] private Button cerrarAjustes;

    [Header("Título")]
    [Tooltip("Componente TextMeshPro para renderizar el nombre del juego.")]
    [SerializeField] private TMP_Text tiutuloTexto;

    [Tooltip("Cadena de texto con el nombre oficial que se mostrará en el título.")]
    [SerializeField] private string tituloJuego = "OralDiagnostic";

    /// <summary>
    /// Método de inicialización en la fase Start. 
    /// Asigna el texto del título, asegura el estado oculto del panel de ajustes y suscribe los oyentes de eventos a los botones y toggles.
    /// </summary>
    void Start()
    {
        // Asignación del título principal
        if (tiutuloTexto != null)
            tiutuloTexto.text = tituloJuego;

        // Estado inicial del panel de ajustes (oculto por defecto)
        if (panelAjustes != null)
            panelAjustes.SetActive(false);

        // Suscripción de oyentes a eventos de clics en botones
        if (botonJugar != null)
            botonJugar.onClick.AddListener(Jugar);

        if (botonAjustes != null)
            botonAjustes.onClick.AddListener(AbrirAjustes);

        if (cerrarAjustes != null)
            cerrarAjustes.onClick.AddListener(CerrarAjustes);

        // Suscripción de oyentes a cambios de valor en Toggles
        if (toggleModoZurdo != null)
            toggleModoZurdo.onValueChanged.AddListener(ModoZurdo);

        if (toggleDesactivarSondio != null)
            toggleDesactivarSondio.onValueChanged.AddListener(DesactivarSonido);
    }

    /// <summary>
    /// Maneja el evento del botón Jugar. Desactiva el menú principal e instruye 
    /// al GameManager para dar inicio al bucle del modo Carrera.
    /// </summary>
    private void Jugar()
    {
        if (GameManager.Instance != null)
        {
            gameObject.SetActive(false);
            GameManager.Instance.IniciarModoCarrera();
        }
    }

    /// <summary>
    /// Activa la visibilidad del panel modal de ajustes en la interfaz.
    /// </summary>
    private void AbrirAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel modal de ajustes.
    /// </summary>
    private void CerrarAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    /// <summary>
    /// Callback invocado cuando cambia el estado del Toggle de modo zurdo.
    /// Aplica la redistribución o adaptación de controles para la accesibilidad.
    /// </summary>
    /// <param name="activado">Indica si la opción se encuentra encendida o apagada.</param>
    private void ModoZurdo(bool activado)
    {
        // Lógica para adaptar la maquetación de UI al modo zurdo
    }

    /// <summary>
    /// Callback invocado cuando cambia el estado del Toggle de sonido.
    /// Comunica el cambio al sistema de audio global para silenciar/reactivar el volumen.
    /// </summary>
    /// <param name="activado">Indica si el estado de silencio está activo.</param>
    private void DesactivarSonido(bool activado)
    {
        // Lógica para silenciar o restablecer los canales de audio
    }

    /// <summary>
    /// Restablece la visibilidad de la pantalla del menú principal y asegura 
    /// que las ventanas modales secundarias permanezcan cerradas.
    /// </summary>
    public void MostrarPantalla()
    {
        gameObject.SetActive(true);
        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    void Update()
    {
        
    }
}