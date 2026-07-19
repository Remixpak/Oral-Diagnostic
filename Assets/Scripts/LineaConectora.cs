using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LineaConectora : MonoBehaviour
{
    [Header("Configuración de Líneas")]
    [SerializeField] private float grosorLinea = 4f;
    [SerializeField] private Color colorLineaSeleccion = Color.yellow;
    [SerializeField] private Color colorLineaCorrecta = Color.green;
    [SerializeField] private Color colorLineaIncorrecta = Color.red;

    [Header("Ajustes de Posición")]
    [SerializeField] private float offsetOrigenY = 0f;
    [SerializeField] private float offsetDestinoY = 0f;

    private List<Image> lineasActivas = new List<Image>();
    private List<RectTransform> origenes = new List<RectTransform>();
    private List<RectTransform> destinos = new List<RectTransform>();

    void Update()
    {
        ActualizarPosicionesLineas();
    }

    //creamos una línea entre dos botones, con un color específico mediante su componente Image y obtenemos su posicion y tamaño para ajustarla correctamente entre los botones
    public void CrearLinea(Button origen, Button destino, Color color)
    {
        if (origen == null || destino == null) return;

        GameObject lineaGO = new GameObject($"Linea_{origen.name}_{destino.name}");
        lineaGO.transform.SetParent(transform);
        lineaGO.transform.SetAsLastSibling();

        Image img = lineaGO.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        RectTransform rt = lineaGO.GetComponent<RectTransform>();

        lineasActivas.Add(img);
        origenes.Add(origen.GetComponent<RectTransform>());
        destinos.Add(destino.GetComponent<RectTransform>());

        ActualizarPosicionLinea(rt, origen.GetComponent<RectTransform>(), destino.GetComponent<RectTransform>());
    }

    private void ActualizarPosicionLinea(RectTransform lineaRT, RectTransform origenRT, RectTransform destinoRT)
    {
        if (lineaRT == null || origenRT == null || destinoRT == null) return;

        //usamos posiciones generales
        Vector3 posOrigen = origenRT.position;
        Vector3 posDestino = destinoRT.position;

        // obtenemos el tamaño de los pixeles
        float alturaOrigen = origenRT.rect.height * origenRT.lossyScale.y;
        float alturaDestino = destinoRT.rect.height * destinoRT.lossyScale.y;

        // obtenemos el centro inferior del boton
        posOrigen.y -= alturaOrigen / 2 + offsetOrigenY;

        // obtenemos el centro superior del boton
        posDestino.y += alturaDestino / 2 + offsetDestinoY;

        // convertimos las coordenas del canvas
        Vector2 localOrigen = transform.InverseTransformPoint(posOrigen);
        Vector2 localDestino = transform.InverseTransformPoint(posDestino);

        Vector2 centro = (localOrigen + localDestino) / 2;
        Vector2 direccion = localDestino - localOrigen;
        float distancia = direccion.magnitude;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // evitamos las lineas cortas en longitud 0
        if (distancia < 1f) return;

        lineaRT.anchoredPosition = centro;
        lineaRT.sizeDelta = new Vector2(distancia, grosorLinea);
        lineaRT.rotation = Quaternion.Euler(0, 0, angulo);
    }

    //actualizamos la posición de todas las lineas activas en cada frame
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

    //cambiamos el color de una línea específica mediante su índice en la lista de lineas activas
    public void CambiarColorLinea(int indice, Color nuevoColor)
    {
        if (indice < lineasActivas.Count && lineasActivas[indice] != null)
        {
            lineasActivas[indice].color = nuevoColor;
        }
    }

    //eliminamos todas las lineas activas y limpiamos las listas de origenes y destinos
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

    // hay lineas activas en la lista
    public bool HayLineas()
    {
        return lineasActivas.Count > 0;
    }

    // obtenemos la cantidad de lineas activas en la lista
    public int CantidadLineas()
    {
        return lineasActivas.Count;
    }

    // obtenemos el color de la línea de selección, correcta e incorrecta
    public Color GetColorSeleccion() => colorLineaSeleccion;
    public Color GetColorCorrecta() => colorLineaCorrecta;
    public Color GetColorIncorrecta() => colorLineaIncorrecta;
}