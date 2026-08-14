using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de usuario para la configuración de partidas personalizadas (Custom). 
/// Permite ajustar el número de preguntas y seleccionar las categorías activas antes de iniciar la escena de juego.
/// 
/// Clases dependientes que utiliza:
/// - ConfiguracionPartida: Clase estática o de datos donde se persisten las opciones seleccionadas (cantidad de preguntas, toggles de lesiones, familias y diagnósticos) para ser consumidas por el bucle principal de juego.
/// - SceneManager: Módulo de Unity encargado del flujo y transición hacia la escena principal ("MainSecene").
/// </summary>
public class ConfiguracionCustom : MonoBehaviour
{
    [Header("Cantidad")]
    [SerializeField] private TMP_Text textoCantidad;

    [Header("Preguntas")]
    private int CantidadPreguntas;

    [Header("Toggles")]
    [SerializeField] private Toggle toggleLesiones;
    [SerializeField] private Toggle toggleFamilias;
    [SerializeField] private Toggle toggleDiagnosticos;

    [Header("btn continuar")]
    [SerializeField] private Button btnContinuar;
    [SerializeField] private Button btnContinuar_alt;

    /// <summary>
    /// Establece la cantidad inicial de preguntas, registra los escuchadores de eventos en los Toggles y actualiza la interactividad de los botones.
    /// </summary>
    void Start()
    {
        CantidadPreguntas = 30;

        toggleLesiones.onValueChanged.AddListener(delegate { ActualizarBoton(); });
        toggleFamilias.onValueChanged.AddListener(delegate { ActualizarBoton(); });
        toggleDiagnosticos.onValueChanged.AddListener(delegate { ActualizarBoton(); });

        ActualizarBoton();
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Habilita o deshabilita los botones de continuar asegurando que al menos uno de los Toggles esté marcado.
    /// </summary>
    private void ActualizarBoton()
    {
        bool activo = toggleLesiones.isOn || toggleFamilias.isOn || toggleDiagnosticos.isOn;

        btnContinuar.interactable = activo;
        btnContinuar_alt.interactable = activo;
    }

    /// <summary>
    /// Incrementa la cantidad de preguntas permitidas en la partida con un tope máximo de 30 y actualiza la interfaz de texto.
    /// </summary>
    public void sumar()
    {
        if(CantidadPreguntas < 30)
        {
            CantidadPreguntas++;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
        else
        {
            CantidadPreguntas = 30;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
    }

    /// <summary>
    /// Decrementa la cantidad de preguntas permitidas en la partida con un límite mínimo de 0 y actualiza la interfaz de texto.
    /// </summary>
    public void restar()
    {
        if(CantidadPreguntas > 0)
        {
            CantidadPreguntas--;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
        else
        {
            CantidadPreguntas = 0;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
    }

    /// <summary>
    /// Transfiere las selecciones hechas en la interfaz hacia la clase de configuración global de la partida.
    /// </summary>
    private void CargarConfig()
    {
        ConfiguracionPartida.CantidadPreguntas = this.CantidadPreguntas;
        ConfiguracionPartida.Lesiones = toggleLesiones.isOn;
        ConfiguracionPartida.FamiliasEtiopatogenias = toggleFamilias.isOn;
        ConfiguracionPartida.Diagnosticos = toggleDiagnosticos.isOn;
    }

    /// <summary>
    /// Guarda los parámetros de configuración y realiza el cambio de escena hacia la pantalla principal de juego.
    /// </summary>
    public void IrAJuegoCustom()
    {
        CargarConfig();
        SceneManager.LoadScene("MainSecene");
    }
}