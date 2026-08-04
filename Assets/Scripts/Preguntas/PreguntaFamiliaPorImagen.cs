using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaFamiliaPorImagen : ControladorPreguntaBase
{
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

        // Distractores
        List<Familia> restoFamilias = CsvManager.Instance.familias.Where(f => f.id != fCorrecta.id).ToList();

        while (opciones.Count < botonesAlternativas.Count && restoFamilias.Count > 0)
        {
            int idx = Random.Range(0, restoFamilias.Count);
            opciones.Add(restoFamilias[idx].nombre);
            restoFamilias.RemoveAt(idx);
        }
    }
}