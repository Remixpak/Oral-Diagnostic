using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la lógica para preguntas basadas en imágenes donde el usuario debe identificar a qué lesión básica corresponde la manifestación clínica observada.
/// 
/// Clases que utiliza y su finalidad:
/// - ControladorPreguntaBase: Clase base de la que hereda, la cual define la estructura básica y el flujo para el control de preguntas.
/// - CsvManager: Patrón Singleton (CsvManager.Instance) utilizado para consultar la base de datos de patologías, lesiones y recuperar los sprites de las imágenes.
/// - Patologia: Modelo de datos que representa una patología con sus datos asociados (código de imagen, ID de la lesión básica, etc.).
/// - Lesion: Modelo de datos que contiene la información de una lesión básica, como su ID y nombre.
/// - Sprite / UnityEngine: Tipos de Unity necesarios para la asignación de elementos de interfaz visual y la manipulación de listas aleatorias.
/// </summary>
public class PreguntaLesionPorImagen : ControladorPreguntaBase
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
    /// Configura el texto, carga la imagen correspondiente a la patología y genera la lista de opciones (respuesta correcta y distractores) para el ID asignado.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología a consultar.</param>
    /// <param name="opciones">Parámetro de salida que contendrá la lista final de opciones de texto para las alternativas.</param>
    /// <param name="spritesOpciones">Parámetro de salida para sprites opcionales en las alternativas (se establece como null en este tipo de pregunta).</param>
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        if (imagenPregunta != null)
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null)
            textoPregunta.text = "¿A qué lesión básica corresponde la manifestación clínica observada en la imagen?";

        Lesion lCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = lCorrecta.nombre;
        opciones.Add(respuestaCorrecta);

        List<Lesion> restoLesiones = CsvManager.Instance.lesiones
            .Where(l => l.id != lCorrecta.id && 
                        !string.IsNullOrWhiteSpace(l.nombre) && 
                        !l.nombre.Contains("/"))
            .ToList();

        restoLesiones = restoLesiones.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (restoLesiones.Count > 0)
            {
                Lesion lDist = restoLesiones[0];
                restoLesiones.RemoveAt(0);

                if (!string.IsNullOrWhiteSpace(lDist.nombre))
                {
                    opciones.Add(lDist.nombre);
                }
                else
                {
                    continue;
                }
            }
            else
            {
                string distractorFalso = $"Lesión falsa {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Lesión {opciones.Count + 1}");
        }

        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}