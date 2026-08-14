using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Gestiona la generación, posicionamiento, rotación y ciclo de vida de líneas dinámicas en la interfaz UI.
/// Permite unir elementos UI (como botones de preguntas/conceptos) calculando en tiempo real las transformaciones 
/// locales dentro del Canvas.
/// 
/// Componentes de Unity que utiliza:
/// - MonoBehaviour (UnityEngine): Control de ciclo de vida e interacciones dentro de la escena.
/// - Image / Button / RectTransform (UnityEngine.UI): Componentes UI para renderizado gráfico, detección de eventos y manejo de espacio transformacional 2D.
/// - Vector2 / Vector3 / Quaternion / Mathf (UnityEngine): Operaciones de cálculo vectorial, trigonometría y rotaciones eulerianas.
/// </summary>
public class LineaConectora : MonoBehaviour
{
    [Header("Configuración de Líneas")]
    [Tooltip("Ancho/Grosor en píxeles que tendrá el componente RectTransform de la línea.")]
    [SerializeField] private float grosorLinea = 4f;

    [Tooltip("Color por defecto utilizado al trazar una línea activa en proceso de selección.")]
    [SerializeField] private Color colorLineaSeleccion = Color.yellow;

    [Tooltip("Color aplicado a las conexiones correctas en la validación de la pregunta.")]
    [SerializeField] private Color colorLineaCorrecta = Color.green;

    [Tooltip("Color aplicado a las conexiones erróneas en la validación de la pregunta.")]
    [SerializeField] private Color colorLineaIncorrecta = Color.red;

    [Header("Ajustes de Posición")]
    [Tooltip("Desplazamiento vertical en píxeles aplicado al punto de origen.")]
    [SerializeField] private float offsetOrigenY = 0f;

    [Tooltip("Desplazamiento vertical en píxeles aplicado al punto de destino.")]
    [SerializeField] private float offsetDestinoY = 0f;

    // Listas paralelas para el seguimiento y actualización continua de las conexiones activas
    private List<Image> lineasActivas = new List<Image>();
    private List<RectTransform> origenes = new List<RectTransform>();
    private List<RectTransform> destinos = new List<RectTransform>();

    /// <summary>
    /// Actualiza la posición, escala y rotación de todas las líneas activas por cada frame para soportar layouts dinámicos.
    /// </summary>
    void Update()
    {
        ActualizarPosicionesLineas();
    }

    /// <summary>
    /// Crea e instancia un nuevo GameObject con un componente Image que actúa como línea gráfica conectora entre dos botones UI.
    /// </summary>
    /// <param name="origen">Botón UI desde el cual nace la línea (extremo inferior).</param>
    /// <param name="destino">Botón UI hacia el cual llega la línea (extremo superior).</param>
    /// <param name="color">Tinta inicial asignada a la propiedad Image.color de la línea.</param>
    public void CrearLinea(Button origen, Button destino, Color color)
    {
        if (origen == null || destino == null) return;

        // Instanciación dinámica del objeto contenedor de la línea
        GameObject lineaGO = new GameObject($"Linea_{origen.name}_{destino.name}");
        lineaGO.transform.SetParent(transform);
        lineaGO.transform.SetAsLastSibling();

        // Configuración visual básica
        Image img = lineaGO.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false; // Evita que la línea bloquee raycasts de ratón/touch en la UI

        RectTransform rt = lineaGO.GetComponent<RectTransform>();

        // Registro en colecciones para persistencia en Update
        lineasActivas.Add(img);
        origenes.Add(origen.GetComponent<RectTransform>());
        destinos.Add(destino.GetComponent<RectTransform>());

        // Transformación inicial inmediata
        ActualizarPosicionLinea(rt, origen.GetComponent<RectTransform>(), destino.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Recalcula la posición anclada, el tamaño vectorial y el ángulo de inclinación de un RectTransform de línea entre dos nodos.
    /// </summary>
    /// <param name="lineaRT">RectTransform del objeto línea a transformar.</param>
    /// <param name="origenRT">RectTransform del objeto origen.</param>
    /// <param name="destinoRT">RectTransform del objeto destino.</param>
    private void ActualizarPosicionLinea(RectTransform lineaRT, RectTransform origenRT, RectTransform destinoRT)
    {
        if (lineaRT == null || origenRT == null || destinoRT == null) return;

        // Obtención de coordenadas en espacio de mundo
        Vector3 posOrigen = origenRT.position;
        Vector3 posDestino = destinoRT.position;

        // Cálculo de altura efectiva escalada en píxeles de pantalla
        float alturaOrigen = origenRT.rect.height * origenRT.lossyScale.y;
        float alturaDestino = destinoRT.rect.height * destinoRT.lossyScale.y;

        // Ajuste de anclaje: Borde central inferior del origen
        posOrigen.y -= alturaOrigen / 2 + offsetOrigenY;

        // Ajuste de anclaje: Borde central superior del destino
        posDestino.y += alturaDestino / 2 + offsetDestinoY;

        // Conversión de espacio de mundo a espacio local dentro del Canvas/Contenedor
        Vector2 localOrigen = transform.InverseTransformPoint(posOrigen);
        Vector2 localDestino = transform.InverseTransformPoint(posDestino);

        // Operaciones trigonométricas y algebraicas para orientación y magnitud
        Vector2 centro = (localOrigen + localDestino) / 2;
        Vector2 direccion = localDestino - localOrigen;
        float distancia = direccion.magnitude;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Salvaguarda contra artefactos visuales si los puntos coinciden casi exactamente
        if (distancia < 1f) return;

        // Aplicación de transformaciones al RectTransform
        lineaRT.anchoredPosition = centro;
        lineaRT.sizeDelta = new Vector2(distancia, grosorLinea);
        lineaRT.rotation = Quaternion.Euler(0, 0, angulo);
    }

    /// <summary>
    /// Itera sobre la colección de líneas activas y actualiza dinámicamente sus coordenadas vectoriales.
    /// </summary>
    private void ActualizarPosicionesLineas()
    {
        for (int i = 0; i < lineasActivas.Count; i++)
        {
            if (i < origenes.Count && i < destinos.Count)
            {
                if (lineasActivas[i] != null && origenes[i] != null && destinos[i] != null)
                {
                    ActualizarPosicionLinea(
                        lineasActivas[i].GetComponent<RectTransform>(),
                        origenes[i],
                        destinos[i]
                    );
                }
            }
        }
    }

    /// <summary>
    /// Modifica el color del componente Image de una línea existente en base a su posición dentro del listado.
    /// </summary>
    /// <param name="indice">Índice numérico de la línea dentro de lineasActivas.</param>
    /// <param name="nuevoColor">Nuevo struct Color a aplicar.</param>
    public void CambiarColorLinea(int indice, Color nuevoColor)
    {
        if (indice < lineasActivas.Count && lineasActivas[indice] != null)
        {
            lineasActivas[indice].color = nuevoColor;
        }
    }

    /// <summary>
    /// Destruye físicamente de la escena los GameObjects de todas las líneas creadas y vacía los listados de control.
    /// </summary>
    public void LimpiarLineas()
    {
        foreach (var img in lineasActivas)
        {
            if (img != null)
            {
                Destroy(img.gameObject);
            }
        }
        lineasActivas.Clear();
        origenes.Clear();
        destinos.Clear();
    }

    /// <summary>
    /// Indica si existen líneas dibujadas actualmente en la interfaz.
    /// </summary>
    /// <returns>True si la lista de líneas activas contiene al menos un elemento, False en caso contrario.</returns>
    public bool HayLineas()
    {
        return lineasActivas.Count > 0;
    }

    /// <summary>
    /// Devuelve el total de conexiones representadas visualmente en pantalla.
    /// </summary>
    /// <returns>Entero con la cantidad total de elementos en la lista de líneas activas.</returns>
    public int CantidadLineas()
    {
        return lineasActivas.Count;
    }

    /// <summary>
    /// Obtiene el color predeterminado asignado a las conexiones en estado de selección activa.
    /// </summary>
    public Color GetColorSeleccion() => colorLineaSeleccion;

    /// <summary>
    /// Obtiene el color predeterminado asignado a las conexiones evaluadas como correctas.
    /// </summary>
    public Color GetColorCorrecta() => colorLineaCorrecta;

    /// <summary>
    /// Obtiene el color predeterminado asignado a las conexiones evaluadas como incorrectas.
    /// </summary>
    public Color GetColorIncorrecta() => colorLineaIncorrecta;
}