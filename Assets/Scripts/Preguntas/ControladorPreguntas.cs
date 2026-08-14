using UnityEngine;
using TMPro;

/// <summary>
/// Clase abstracta de alto nivel que define la interfaz y la estructura base para los controladores de preguntas, gestionando los elementos principales de Canvas y el ciclo de vida básico del flujo de preguntas.
/// 
/// Clases que utiliza y su finalidad:
/// - MonoBehaviour: Clase base de Unity de la que hereda para poder adjuntarse como componente a GameObjects.
/// - Canvas (UnityEngine): Componentes de interfaz gráfica para alternar y controlar la visibilidad del juego principal y de la retroalimentación.
/// - TMPro (TMPro): Utilizado para la gestión de componentes de texto tipográfico de TextMeshPro en las clases derivadas.
/// </summary>
public abstract class ControladorPreguntas : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] public Canvas canvasJuego;
    [SerializeField] public Canvas canvasRetroalimentacion;

    public bool finished;

    /// <summary>
    /// Método reservado para registrar métricas o estadísticas derivadas del desempeño en la pregunta.
    /// </summary>
    public void RegistrarMetricas()
    {

    }

    /// <summary>
    /// Método abstracto que debe ser implementado por las clases derivadas para desplegar la pantalla o interfaz de retroalimentación tras responder.
    /// </summary>
    public abstract void EntregarRetroalimentacion();

    /// <summary>
    /// Método abstracto que debe ser implementado por las clases derivadas para inicializar el estado de la pregunta a partir de un identificador de patología.
    /// </summary>
    /// <param name="indPatologiaAsignada">Identificador único de la patología asignada a la pregunta.</param>
    public abstract void InicializarPregunta(int indPatologiaAsignada);

    /// <summary>
    /// Inicializa las variables de estado al arrancar el componente, estableciendo la bandera de finalización en falso.
    /// </summary>
    void Start()
    {
        finished = false;
    }

    /// <summary>
    /// Método de actualización ejecutado en cada frame por Unity.
    /// </summary>
    void Update()
    {
        
    }
}