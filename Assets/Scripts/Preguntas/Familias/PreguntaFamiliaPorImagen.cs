using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la lógica para preguntas basadas en imágenes donde el usuario debe identificar a qué familia corresponde la manifestación clínica mostrada.
/// 
/// Clases que utiliza y su finalidad:
/// - ControladorPreguntaBase: Clase base de la que hereda, la cual define la estructura y el flujo principal para el control de preguntas.
/// - CsvManager: Patrón Singleton (CsvManager.Instance) utilizado para consultar la información de patologías, familias y obtener los sprites de imágenes desde los datos almacenados.
/// - Patologia: Modelo de datos que representa una patología con sus identificadores, código de imagen y referencia al ID de la familia a la que pertenece.
/// - Familia: Modelo de datos que contiene los detalles de una familia patológica, como su ID y nombre.
/// - Sprite / UnityEngine: Elementos de Unity para la renderización de imágenes e interfaz, así como para la generación de aleatoriedad.
/// </summary>
public class PreguntaFamiliaPorImagen : ControladorPreguntaBase
{
    /// <summary>
    /// Inicializa los parámetros específicos de este tipo de pregunta al iniciar el objeto.
    /// Define el tipo de materia como "familia".
    /// </summary>
    void Start()
    {
        tipoMateria = "familia";
    }

    /// <summary>
    /// Configura el texto, carga la imagen correspondiente a la patología y genera la lista de opciones (respuesta correcta y distractores) para el ID asignado.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología a consultar.</param>
    /// <param name="opciones">Parámetro de salida que contendrá la lista final de opciones de texto para las alternativas.</param>
    /// <param name="spritesOpciones">Parámetro de salida para sprites en las alternativas (se establece como null ya que las opciones son de texto).</param>
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        if (imagenPregunta != null)
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null)
            textoPregunta.text = "¿A qué familia corresponde la siguiente manifestación clínica?";

        Familia fCorrecta = CsvManager.Instance.ObtenerFamiliaPorId(p.familiaID);
        respuestaCorrecta = fCorrecta.nombre;
        opciones.Add(respuestaCorrecta);

        List<Familia> restoFamilias = CsvManager.Instance.familias
            .Where(f => f.id != fCorrecta.id && !string.IsNullOrWhiteSpace(f.nombre))
            .ToList();

        restoFamilias = restoFamilias.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (restoFamilias.Count > 0)
            {
                Familia fDist = restoFamilias[0];
                restoFamilias.RemoveAt(0);

                if (!string.IsNullOrWhiteSpace(fDist.nombre))
                {
                    opciones.Add(fDist.nombre);
                }
            }
            else
            {
                string distractorFalso = $"Familia {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Familia {opciones.Count + 1}");
        }

        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}