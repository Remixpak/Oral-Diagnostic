using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaEtiologiaPorImagen : ControladorPreguntaBase
{
    void Start()
    {
        tipoMateria = "familia";
    }
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

        // Distractores
        List<Etiologia> restoEtiologias = CsvManager.Instance.etiologias.Where(e => e.id != eCorrecta.id).ToList();

        while (opciones.Count < botonesAlternativas.Count && restoEtiologias.Count > 0)
        {
            int idx = Random.Range(0, restoEtiologias.Count);
            opciones.Add(restoEtiologias[idx].nombre);
            restoEtiologias.RemoveAt(idx);
        }
    }
}