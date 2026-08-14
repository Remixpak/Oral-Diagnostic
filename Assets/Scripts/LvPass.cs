using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz gráfica de transición al finalizar un nivel o ronda de juego (Pantalla de Pase de Nivel / GameOver).
/// Gestiona la visualización de los mensajes de avance, la activación según el resultado (aprobado/reintento) 
/// y la suscripción de eventos a los botones de flujo para interactuar con GameManager, ControladorSonido y SceneManager.
/// 
/// Clases y componentes que utiliza:
/// - MonoBehaviour / SceneManager (UnityEngine): Ciclo de vida del objeto y carga/transición entre escenas de Unity ("PantallaInicio").
/// - Button (UnityEngine.UI): Componente de UI interactivo para recibir clics/toques de usuario.
/// - TMP_Text (TMPro): Renderizado de texto para comunicar la aprobación del nivel o el estado de Game Over.
/// - GameManager (Singleton local): Lógica principal del juego; invocado para avanzar de nivel o reiniciar rondas en sus distintos modos.
/// - ControladorSonido (Singleton local): Sistema de audio para la reproducción del feedback sonoro (clic de interfaz).
/// </summary>
public class LvPass : MonoBehaviour
{
    [Header("Componentes de Interfaz")]
    [Tooltip("Componente TextMeshPro para desplegar el título del estado actual (Ej: 'Nivel 1 completado' o 'GameOver').")]
    [SerializeField] private TMP_Text textoLv;

    [Tooltip("Botón interactivo para avanzar al siguiente bloque o nivel de la carrera.")]
    [SerializeField] private Button BtnContinar;

    [Tooltip("Botón interactivo para repetir la ronda actual en caso de reprobar o reintentar el modo.")]
    [SerializeField] private Button BtnReintentar;

    /// <summary>
    /// Método de inicialización en la fase Awake. 
    /// Remueve oyentes previos y suscribe programáticamente las acciones de clic a los botones de la UI.
    /// </summary>
    private void Awake()
    {
        // Limpieza y reasignación explícita de eventos de escucha para el botón Continuar
        if (BtnContinar != null)
        {
            BtnContinar.onClick.RemoveAllListeners();
            BtnContinar.onClick.AddListener(Continuar);
        }

        // Limpieza y reasignación explícita de eventos de escucha para el botón Reintentar
        if (BtnReintentar != null)
        {
            BtnReintentar.onClick.RemoveAllListeners();
            BtnReintentar.onClick.AddListener(Reintentar);
        }
    }

    /// <summary>
    /// Maneja el evento de clic en el botón Continuar. Reproduce el feedback de audio 
    /// e instruye al GameManager para que libere la espera y proceda al siguiente nivel.
    /// </summary>
    public void Continuar()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Debug.Log("Btn Continuar presionado");
        GameManager.Instance.ContinuarCarrera();
    }

    /// <summary>
    /// Maneja el evento de clic en el botón Reintentar. Identifica el modo de juego activo 
    /// en GameManager para reiniciar el bucle de preguntas correspondiente.
    /// </summary>
    public void Reintentar()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Debug.Log("Btn Reintentar presionado");

        switch (GameManager.Instance.modoActual)
        {
            case GameManager.ModoJuego.Carrera:
                // Desbloquea la espera del WaitUntil para repetir la ronda del nivel actual en Carrera
                GameManager.Instance.ContinuarCarrera(); 
                break;

            case GameManager.ModoJuego.QuickPlay:
                GameManager.Instance.IniciarModoQuickPlay();
                break;

            case GameManager.ModoJuego.Custom:
                GameManager.Instance.IniciarModoCustom();
                break;
        }
    }

    /// <summary>
    /// Maneja el evento de salida de la ronda. Restablece la escala de tiempo a la normalidad 
    /// y redirige al usuario a la escena del menú principal ("PantallaInicio").
    /// </summary>
    public void Salir()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("PantallaInicio");
    }

    /// <summary>
    /// Configura y muestra el panel visual de nivel superado.
    /// Ajusta el texto informativo y activa exclusivamente el botón de Continuar.
    /// </summary>
    /// <param name="nivel">Índice del nivel actual para calcular el nivel completado.</param>
    public void MostrarPass(int nivel)
    {
        nivel--; // Ajuste de offset para mostrar el nivel recién superado en pantalla

        if (textoLv != null)
            textoLv.text = $"Nivel {nivel} completado";

        if (BtnReintentar != null) BtnReintentar.gameObject.SetActive(false);
        if (BtnContinar != null) BtnContinar.gameObject.SetActive(true);
    }

    /// <summary>
    /// Configura y muestra el panel visual de nivel fallido o Game Over.
    /// Muestra la indicación de fin de juego y activa exclusivamente el botón de Reintentar.
    /// </summary>
    public void MostrarReintento()
    {
        if (textoLv != null)
            textoLv.text = "GameOver";

        if (BtnReintentar != null) BtnReintentar.gameObject.SetActive(true);
        if (BtnContinar != null) BtnContinar.gameObject.SetActive(false);
    }
}