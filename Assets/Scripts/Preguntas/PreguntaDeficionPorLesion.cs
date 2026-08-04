using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaDefinicionPorLesion : ControladorPreguntaBase
{
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null; // No usa imágenes en alternativas
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        Lesion lesionCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);

        if (textoPregunta != null) 
            textoPregunta.text = $"¿Cuál de las siguientes definiciones corresponde a {lesionCorrecta.nombre}?";

        List<Descripcion> descripcionesCorrectas = CsvManager.Instance.ObtenerDescripcionesDeLesion(lesionCorrecta);
        Descripcion descCorrecta = descripcionesCorrectas[Random.Range(0, descripcionesCorrectas.Count)];

        respuestaCorrecta = descCorrecta.texto;
        opciones.Add(respuestaCorrecta);

        // Distractores
        List<Descripcion> restoDescripciones = CsvManager.Instance.descripciones
            .Where(d => !lesionCorrecta.descripcionIDs.Contains(d.id)).ToList();

        while (opciones.Count < botonesAlternativas.Count && restoDescripciones.Count > 0)
        {
            int idx = Random.Range(0, restoDescripciones.Count);
            opciones.Add(restoDescripciones[idx].texto);
            restoDescripciones.RemoveAt(idx);
        }
    }
}