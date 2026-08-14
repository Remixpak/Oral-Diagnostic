using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla la secuencia interactiva del tutorial explicativo al inicio de una ronda o nivel.
/// Gestiona la animación de tipo máquina de escribir (typewriter effect) para desplegar mensajes dinámicos
/// en un componente SnackBar, la actualización de objetivos en pantalla y el control de ciclo de vida con corrutinas independientes del tiempo del juego (Realtime).
/// 
/// Clases y componentes que utiliza:
/// - MonoBehaviour / Coroutine / IEnumerator (UnityEngine): Control de ciclo de vida e hilado de secuencias temporizadas de UI via corrutinas.
/// - TMP_Text (TMPro): Renderizado formateado del texto del SnackBar y de la descripción del objetivo.
/// - ControladorSonido (Singleton local): Feedback acústico al cancelar o cerrar la secuencia de tutorial.
/// </summary>
public class Tutorial : MonoBehaviour
{
    [Header("Textos")]
    [TextArea] [SerializeField] private string texto1;
    [TextArea] [SerializeField] private string texto2;
    [TextArea] [SerializeField] private string texto3;

    [Tooltip("Componente TextMeshPro donde se escribirá dinámicamente cada carácter del mensaje.")]
    [SerializeField] private TMP_Text textoSnackBar;

    [Header("Configuración de Tiempos")]
    [Tooltip("Tiempo en segundos reales de espera entre la aparición de cada letra.")]
    [SerializeField] private float tiempoEntreLetras = 0.05f;

    [Tooltip("Tiempo de pausa en segundos reales tras completar un mensaje antes de pasar al siguiente.")]
    [SerializeField] private float esperaEntreMensajes = 2f;

    [Tooltip("Tiempo de pausa final antes de cerrar automáticamente la secuencia del tutorial.")]
    [SerializeField] private float esperaFinal = 2f;

    [Header("Objetivos")]
    [Tooltip("Componente TextMeshPro para mostrar la meta u objetivo principal del nivel actual.")]
    [SerializeField] private TMP_Text Objetivo;

    /// <summary>
    /// Propiedad pública que indica si la secuencia del tutorial concluyó (por haber terminado o por omisión).
    /// Utilizado comúnmente por GameManager en una instrucción WaitUntil para pausar el flujo de la partida.
    /// </summary>
    public bool Finalizado { get; private set; }

    private Coroutine rutinaTutorial;
    private bool omitido = false;

    /// <summary>
    /// Inicializa las banderas de control e inicia la corrutina principal para secuenciar el despliegue de mensajes.
    /// </summary>
    public void ActivarTutorial()
    {
        Finalizado = false;
        omitido = false;

        rutinaTutorial = StartCoroutine(MostrarTutorial());
    }

    /// <summary>
    /// Corrutina secuencial que coordina el efecto de tipeo y las pausas en tiempo real entre cada uno de los 3 mensajes.
    /// </summary>
    private IEnumerator MostrarTutorial()
    {
        // Primer mensaje
        yield return StartCoroutine(EscribirTexto(texto1));
        if (omitido) yield break;

        yield return new WaitForSecondsRealtime(esperaEntreMensajes);

        // Segundo mensaje
        yield return StartCoroutine(EscribirTexto(texto2));
        if (omitido) yield break;

        yield return new WaitForSecondsRealtime(esperaEntreMensajes);

        // Tercer mensaje
        yield return StartCoroutine(EscribirTexto(texto3));
        if (omitido) yield break;

        yield return new WaitForSecondsRealtime(esperaFinal);

        // Finalización automática al completar la secuencia
        CerrarTutorial();
    }

    /// <summary>
    /// Corrutina encargada del efecto "máquina de escribir", agregando carácter por carácter al SnackBar.
    /// Utiliza WaitForSecondsRealtime para funcionar de manera independiente si la escala de tiempo (Time.timeScale) está en 0.
    /// </summary>
    /// <param name="mensaje">Cadena de texto a escribir progresivamente.</param>
    private IEnumerator EscribirTexto(string mensaje)
    {
        if (textoSnackBar == null) yield break;

        textoSnackBar.text = "";

        foreach (char letra in mensaje)
        {
            if (omitido)
                yield break;

            textoSnackBar.text += letra;
            yield return new WaitForSecondsRealtime(tiempoEntreLetras);
        }
    }

    /// <summary>
    /// Cancela o cierra la secuencia del tutorial. Reproduce sonido de interfaz, 
    /// interrumpe la animación en progreso y establece la bandera Finalizado en true.
    /// Generalmente invocado desde el botón 'Saltar' o 'Cerrar' de la UI.
    /// </summary>
    public void CerrarTutorial()
    {
        ControladorSonido.Instance?.ReproducirClick();

        if (Finalizado)
            return;

        omitido = true;

        // Detiene las corrutinas de animación activas en este objeto
        StopAllCoroutines();

        Finalizado = true;
        Debug.Log("Cerrar tutorial " + GetInstanceID());
    }

    /// <summary>
    /// Evento de ciclo de vida de Unity. Garantiza que si el GameObject se deshabilita o destruye inesperadamente, 
    /// se marque la bandera Finalizado en true para evitar congelar los bloques WaitUntil de la partida.
    /// </summary>
    private void OnDisable()
    {
        Finalizado = true;
    }

    /// <summary>
    /// Permite asignar dinámicamente los textos del tutorial y la descripción del objetivo antes de ejecutar la secuencia.
    /// </summary>
    /// <param name="txt1">Primer mensaje explicativo.</param>
    /// <param name="txt2">Segundo mensaje explicativo.</param>
    /// <param name="txt3">Tercer mensaje explicativo.</param>
    /// <param name="obj">Descripción del objetivo del nivel.</param>
    public void ConfigurarTutorial(string txt1, string txt2, string txt3, string obj)
    {
        texto1 = txt1;
        texto2 = txt2;
        texto3 = txt3;

        if (Objetivo != null)
            Objetivo.text = obj;
    }
}