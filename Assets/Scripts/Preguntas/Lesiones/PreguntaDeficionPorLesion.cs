using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la lógica para preguntas basadas en texto donde el usuario debe identificar cuál de las definiciones mostradas corresponde al nombre de una lesión básica específica.
/// 
/// Clases que utiliza y su finalidad:
/// - ControladorPreguntaBase: Clase base de la que hereda, la cual define la estructura básica y el flujo para el control de preguntas.
/// - CsvManager: Patrón Singleton (CsvManager.Instance) utilizado para consultar la base de datos de patologías, lesiones y sus descripciones asociadas.
/// - Patologia: Modelo de datos que representa una patología y permite obtener el ID de la lesión a consultar.
/// - Lesion: Modelo de datos que contiene los detalles de una lesión básica y sus referencias a IDs de descripción.
/// - Descripcion: Modelo de datos que representa el texto descriptivo/definición de una lesión.
/// - UnityEngine: Módulo de Unity utilizado para la generación de índices aleatorios en la selección de opciones.
/// </summary>
public class PreguntaDefinicionPorLesion : ControladorPreguntaBase
{
    /// <summary>
    /// Inicializa los parámetros específicos de este tipo de pregunta al iniciar el objeto.
    /// Define el tipo de materia como "lesion".
    /// </summary>
    void Start()
    {
        tipoMateria = "lesion";
    }

    /// <summary>
    /// Configura el enunciado con el nombre de la lesión y genera la lista de alternativas de texto (definición correcta y distractores de otras lesiones) para el ID asignado.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología a consultar.</param>
    /// <param name="opciones">Parámetro de salida que contendrá la lista de definiciones de texto para las alternativas.</param>
    /// <param name="spritesOpciones">Parámetro de salida para sprites opcionales (se establece como null en este tipo de pregunta).</param>
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        Lesion lesionCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);

        if (textoPregunta != null)
            textoPregunta.text = $"¿Cuál de las siguientes definiciones corresponde a {lesionCorrecta.nombre}?";

        List<Descripcion> descripcionesCorrectas = CsvManager.Instance.ObtenerDescripcionesDeLesion(lesionCorrecta);
        Descripcion descCorrecta = descripcionesCorrectas[Random.Range(0, descripcionesCorrectas.Count)];

        respuestaCorrecta = descCorrecta.texto;
        opciones.Add(respuestaCorrecta);

        List<Descripcion> restoDescripciones = CsvManager.Instance.descripciones
            .Where(d => !lesionCorrecta.descripcionIDs.Contains(d.id)
                        && !string.IsNullOrWhiteSpace(d.texto))
            .ToList();

        List<Descripcion> descripcionesMezcladas = restoDescripciones.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (descripcionesMezcladas.Count > 0)
            {
                string distractor = descripcionesMezcladas[0].texto;
                if (!string.IsNullOrWhiteSpace(distractor))
                {
                    opciones.Add(distractor);
                }
                descripcionesMezcladas.RemoveAt(0);
            }
            else
            {
                string distractorFalso = $"Definicion falsa {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Opcion {opciones.Count + 1}");
        }

        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}