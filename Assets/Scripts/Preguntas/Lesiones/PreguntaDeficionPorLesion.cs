using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaDefinicionPorLesion : ControladorPreguntaBase
{
    void Start()
    {
        tipoMateria = "lesion";
    }
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        //problema corregido: estaba agarrando campos vacios de los csv :p
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

        List<Descripcion> restoDescripciones = CsvManager.Instance.descripciones // agregamos un filtro para que no se repitan las descripciones correctas y que no sean vacías
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
                string distractor = descripcionesMezcladas[0].texto; // verificamos que no sea nulo o vacío antes de agregarlo a las opciones
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

        while (opciones.Count < botonesAlternativas.Count)// en caso de que no se hayan podido generar suficientes distractores, agregamos opciones falsas
        {
            opciones.Add($"Opcion {opciones.Count + 1}");
        }

        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}