using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la lógica para preguntas basadas en imágenes donde el usuario debe identificar la etiología (causa o etiopatogenia) asociada a una manifestación clínica mostrada.
/// 
/// Clases que utiliza y su finalidad:
/// - ControladorPreguntaBase: Clase base de la que hereda, la cual define la estructura básica y el flujo para el control de preguntas.
/// - CsvManager: Patrón Singleton (CsvManager.Instance) utilizado para obtener los datos de las patologías, etiologías y los sprites de las imágenes almacenados en la base de datos/CSV.
/// - Patologia: Modelo de datos que representa una patología con sus IDs, códigos de imagen y referencias de etiología.
/// - Etiologia: Modelo de datos que contiene los detalles de una etiología, como su ID y nombre.
/// - Sprite / UnityEngine: Tipos de Unity utilizados para renderizar elementos visuales y gestionar la aleatoriedad.
/// </summary>
public class PreguntaEtiologiaPorImagen : ControladorPreguntaBase
{
    /// <summary>
    /// Inicializa los parámetros específicos de este tipo de pregunta al comenzar.
    /// Define el tipo de materia como "familia".
    /// </summary>
    void Start()
    {
        tipoMateria = "familia";
    }

    /// <summary>
    /// Configura el texto, la imagen de la pregunta y genera la lista de opciones (respuesta correcta y distractores) para el ID de patología asignado.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología a consultar.</param>
    /// <param name="opciones">Parámetro de salida que contendrá la lista de opciones de texto para los botones.</param>
    /// <param name="spritesOpciones">Parámetro de salida para sprites opcionales en las respuestas (se asigna a null en este tipo de pregunta).</param>
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        if (imagenPregunta != null)
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null)
            textoPregunta.text = "¿Con qué etiopatogenia se asocia la siguiente manifestación clínica?";

        Etiologia eCorrecta = CsvManager.Instance.ObtenerEtiologiaPorId(p.etiologiaID);
        respuestaCorrecta = eCorrecta.nombre;
        opciones.Add(respuestaCorrecta);

        List<Etiologia> restoEtiologias = CsvManager.Instance.etiologias
            .Where(e => e.id != eCorrecta.id && !string.IsNullOrWhiteSpace(e.nombre))
            .ToList();

        restoEtiologias = restoEtiologias.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (restoEtiologias.Count > 0)
            {
                Etiologia eDist = restoEtiologias[0];
                restoEtiologias.RemoveAt(0);

                if (!string.IsNullOrWhiteSpace(eDist.nombre))
                {
                    opciones.Add(eDist.nombre);
                }
            }
            else
            {
                string distractorFalso = $"Etiología {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Etiología {opciones.Count + 1}");
        }
        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}