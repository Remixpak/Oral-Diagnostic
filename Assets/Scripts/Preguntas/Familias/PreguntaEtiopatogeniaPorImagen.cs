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
