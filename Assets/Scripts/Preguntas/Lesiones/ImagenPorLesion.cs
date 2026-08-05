using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaImagenPorLesion : ControladorPreguntaBase
{
    void Start()
    {
        tipoMateria = "lesion";
    }
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

        // Buscar patologías pertenecientes a OTRAS lesiones como distractores
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