
/*using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class PreguntaLesionPorImagen : ControladorPreguntaBase
{
    void Start()
    {
        tipoMateria = "lesion";
    }
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        if (imagenPregunta != null)
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null)
            textoPregunta.text = "¿A qué lesión básica corresponde la manifestación clínica observada en la imagen?";

        Lesion lCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = lCorrecta.nombre;
        opciones.Add(respuestaCorrecta);

        // filtramos las lesiones para obtener las validad
        List<Lesion> restoLesiones = CsvManager.Instance.lesiones
            .Where(l => l.id != lCorrecta.id && !string.IsNullOrWhiteSpace(l.nombre))
            .ToList();

        // los mezclamos para tener un orden aleatorio
        restoLesiones = restoLesiones.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (restoLesiones.Count > 0)
            {
                Lesion lDist = restoLesiones[0];
                restoLesiones.RemoveAt(0);

                //verificamos que el nombre no este vacio 
                if (!string.IsNullOrWhiteSpace(lDist.nombre))
                {
                    opciones.Add(lDist.nombre);
                }
                else
                {
                    // Si el nombre del campo esta vacio pasa al siguiente 
                    continue;
                }
            }
            else
            {
                // si detecta que faltan distractores se genera uno falso para evitar errores
                string distractorFalso = $"Lesión falsa {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        // si nos faltan opciones esta se rellenan 
        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Lesión {opciones.Count + 1}");
        }

        // mezclamos las opciones finales
        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PreguntaLesionPorImagen : ControladorPreguntaBase
{
    void Start()
    {
        tipoMateria = "lesion";
    }

    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        if (imagenPregunta != null)
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null)
            textoPregunta.text = "¿A qué lesión básica corresponde la manifestación clínica observada en la imagen?";

        Lesion lCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = lCorrecta.nombre;
        opciones.Add(respuestaCorrecta);

        // Filtramos las lesiones descartando la correcta, nombres vacíos y los que contengan '/'
        List<Lesion> restoLesiones = CsvManager.Instance.lesiones
            .Where(l => l.id != lCorrecta.id && 
                        !string.IsNullOrWhiteSpace(l.nombre) && 
                        !l.nombre.Contains("/"))
            .ToList();

        // Mezclamos para tener un orden aleatorio
        restoLesiones = restoLesiones.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (restoLesiones.Count > 0)
            {
                Lesion lDist = restoLesiones[0];
                restoLesiones.RemoveAt(0);

                if (!string.IsNullOrWhiteSpace(lDist.nombre))
                {
                    opciones.Add(lDist.nombre);
                }
                else
                {
                    continue;
                }
            }
            else
            {
                // Si faltan distractores válidos se genera uno genérico
                string distractorFalso = $"Lesión falsa {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        // Relleno de seguridad si aún faltan opciones
        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Lesión {opciones.Count + 1}");
        }

        // Mezclamos las opciones finales
        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}

