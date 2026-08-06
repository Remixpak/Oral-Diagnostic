using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaFamiliaPorImagen : ControladorPreguntaBase
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

                // se verifica que el nombre del distractor no esté vacío antes de agregarlo a las opciones
                if (!string.IsNullOrWhiteSpace(fDist.nombre))
                {
                    opciones.Add(fDist.nombre);
                }
                // Si el nombre está vacio se continuae con el siguiente
            }
            else
            {
                // si no hay mas distractores válidos, se crean distractores falsos
                string distractorFalso = $"Familia {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        // si faltan opciones se rellenan con distractores falsos
        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Familia {opciones.Count + 1}");
        }

        // mezclamos las  opciones finales
        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}
