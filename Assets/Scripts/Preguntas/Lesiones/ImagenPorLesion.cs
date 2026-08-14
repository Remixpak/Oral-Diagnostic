using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la lógica para preguntas donde el enunciado indica el nombre de una lesión básica y el usuario debe seleccionar la imagen/sprite correcta entre varias alternativas visuales.
/// 
/// Clases que utiliza y su finalidad:
/// - ControladorPreguntaBase: Clase base de la que hereda, la cual define la estructura y el flujo de configuración de la pregunta.
/// - CsvManager: Patrón Singleton (CsvManager.Instance) utilizado para obtener los datos de patologías, lesiones y recuperar los sprites de las imágenes mediante su código.
/// - Patologia: Modelo de datos que representa una patología con su código de imagen asociado y el ID de la lesión a la que pertenece.
/// - Lesion: Modelo de datos que contiene la información de la lesión básica, como su ID y nombre.
/// - Sprite / UnityEngine: Tipos de Unity para el manejo de imágenes en la interfaz de usuario y generación de valores aleatorios.
/// </summary>
public class PreguntaImagenPorLesion : ControladorPreguntaBase
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
    /// Configura el texto de la pregunta indicando la lesión básica y genera la lista de opciones e imágenes (sprites) correspondientes a la respuesta correcta y a los distractores.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología elegida como respuesta correcta.</param>
    /// <param name="opciones">Parámetro de salida que contendrá los códigos de las imágenes para las alternativas.</param>
    /// <param name="spritesOpciones">Parámetro de salida que contendrá la lista de Sprites a mostrar en los botones de selección.</param>
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        opciones = new List<string>();
        spritesOpciones = new List<Sprite>();

        Patologia pCorrecta = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(pCorrecta.lesionID);

        if (textoPregunta != null) 
            textoPregunta.text = $"¿Cuál de las siguientes imágenes corresponde a la lesión {l.nombre}?";

        respuestaCorrecta = pCorrecta.codigoImagen;
        opciones.Add(respuestaCorrecta);
        spritesOpciones.Add(CsvManager.Instance.spritePorCodigo(pCorrecta.codigoImagen));

        List<Patologia> patologiasDistractoras = CsvManager.Instance.patologias
            .Where(p => p.lesionID != l.id).ToList();

        while (opciones.Count < botonesAlternativas.Count && patologiasDistractoras.Count > 0)
        {
            int idx = Random.Range(0, patologiasDistractoras.Count);
            Patologia pDist = patologiasDistractoras[idx];
            
            opciones.Add(pDist.codigoImagen);
            spritesOpciones.Add(CsvManager.Instance.spritePorCodigo(pDist.codigoImagen));
            
            patologiasDistractoras.RemoveAt(idx);
        }
    }
}