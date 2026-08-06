/*using UnityEngine;
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
}*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class PreguntaDefinicionPorLesion : ControladorPreguntaBase
{
    [Header("Configuración Múltiple Selección")]
    [SerializeField] private TMP_Text textoSeleccionMultiple;

    private List<string> respuestasCorrectasLista = new List<string>();
    private List<string> respuestasSeleccionadasLista = new List<string>();
    private List<Button> botonesSeleccionados = new List<Button>();
    private bool esRespuestaCorrecta = false;

    void Start()
    {
        tipoMateria = "lesion";
    }

    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();
        respuestasCorrectasLista.Clear();
        respuestasSeleccionadasLista.Clear();
        botonesSeleccionados.Clear();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        Lesion lesionPrincipal = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);

        // 1. Obtener los nombres limpios de las lesiones (separando por '/')
        List<string> nombresLesiones = lesionPrincipal.nombre
            .Split('/')
            .Select(n => n.Trim())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        // 2. Buscar todos los objetos Lesion correspondientes a los nombres desglosados
        List<Lesion> lesionesEfectivas = new List<Lesion>();
        foreach (string nom in nombresLesiones)
        {
            Lesion encontrada = CsvManager.Instance.lesiones.FirstOrDefault(l => l.nombre.Trim() == nom);
            if (encontrada != null)
            {
                lesionesEfectivas.Add(encontrada);
            }
        }

        // Si no se encontraron coincidencias exactas por nombre, usamos la principal
        if (lesionesEfectivas.Count == 0)
        {
            lesionesEfectivas.Add(lesionPrincipal);
        }

        // 3. Formatear el enunciado SIN barras '/'
        if (textoPregunta != null)
        {
            if (nombresLesiones.Count > 1)
            {
                string nombresFormateados = string.Join(" y ", nombresLesiones);
                textoPregunta.text = $"¿Cuáles de las siguientes definiciones corresponden a {nombresFormateados}?";
            }
            else
            {
                textoPregunta.text = $"¿Cuál de las siguientes definiciones corresponde a {nombresLesiones[0]}?";
            }
        }

        // 4. Obtener TODAS las descripciones de las lesiones identificadas
        HashSet<int> idsDescripcionesCorrectas = new HashSet<int>();

        foreach (Lesion l in lesionesEfectivas)
        {
            List<Descripcion> descripciones = CsvManager.Instance.ObtenerDescripcionesDeLesion(l);
            foreach (Descripcion d in descripciones)
            {
                if (!respuestasCorrectasLista.Contains(d.texto))
                {
                    respuestasCorrectasLista.Add(d.texto);
                }
            }

            if (l.descripcionIDs != null)
            {
                foreach (int idDesc in l.descripcionIDs)
                {
                    idsDescripcionesCorrectas.Add(idDesc);
                }
            }
        }

        if (respuestasCorrectasLista.Count > 0)
        {
            respuestaCorrecta = respuestasCorrectasLista[0];
        }

        // 5. Activar indicador visual de Selección Múltiple si hay > 1 descripción correcta
        if (textoSeleccionMultiple != null)
        {
            bool esMultiple = respuestasCorrectasLista.Count > 1;
            textoSeleccionMultiple.gameObject.SetActive(esMultiple);
            if (esMultiple)
            {
                textoSeleccionMultiple.text = $"Selecciona {respuestasCorrectasLista.Count} opciones correctas.";
            }
        }

        // 6. Cargar respuestas correctas en las opciones
        foreach (string resp in respuestasCorrectasLista)
        {
            opciones.Add(resp);
        }

        // 7. Cargar distractores válidos (descripciones que no pertenezcan a ninguna de estas lesiones)
        List<Descripcion> restoDescripciones = CsvManager.Instance.descripciones
            .Where(d => !idsDescripcionesCorrectas.Contains(d.id) && !respuestasCorrectasLista.Contains(d.texto))
            .ToList();

        while (opciones.Count < botonesAlternativas.Count && restoDescripciones.Count > 0)
        {
            int idx = Random.Range(0, restoDescripciones.Count);
            opciones.Add(restoDescripciones[idx].texto);
            restoDescripciones.RemoveAt(idx);
        }
    }

    protected override void SeleccionarAlternativa(Button boton, string valorSeleccionado)
    {
        if (yaRespondio) return;

        // Si solo hay 1 respuesta correcta en TOTAL (después de desglosar)
        if (respuestasCorrectasLista.Count <= 1)
        {
            base.SeleccionarAlternativa(boton, valorSeleccionado);
            return;
        }

        // Selección múltiple activa
        if (botonesSeleccionados.Contains(boton))
        {
            botonesSeleccionados.Remove(boton);
            respuestasSeleccionadasLista.Remove(valorSeleccionado);
            boton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            botonesSeleccionados.Add(boton);
            respuestasSeleccionadasLista.Add(valorSeleccionado);
            boton.GetComponent<Image>().color = new Color(0.8f, 0.9f, 1f);
        }

        if (respuestasSeleccionadasLista.Count >= respuestasCorrectasLista.Count)
        {
            EvaluarRespuestaMultiple();
        }
    }

    private void EvaluarRespuestaMultiple()
    {
        yaRespondio = true;

        foreach (Button btn in botonesAlternativas)
            btn.interactable = false;

        esRespuestaCorrecta = respuestasSeleccionadasLista.Count == respuestasCorrectasLista.Count &&
                              !respuestasSeleccionadasLista.Except(respuestasCorrectasLista).Any();

        for (int i = 0; i < botonesSeleccionados.Count; i++)
        {
            Button btn = botonesSeleccionados[i];
            string resp = respuestasSeleccionadasLista[i];

            if (respuestasCorrectasLista.Contains(resp))
            {
                btn.GetComponent<Image>().color = Color.green;
            }
            else
            {
                btn.GetComponent<Image>().color = Color.red;
            }
        }

        if (GameManager.Instance != null)
        {
            if (esRespuestaCorrecta)
            {
                GameManager.Instance.Aciertos++;
                GameManager.Instance.TotalAciertos++;
            }
            else
            {
                GameManager.Instance.Fallos++;
                GameManager.Instance.TotalFallos++;
                if (tipoMateria == "lesion")
                    GameManager.Instance.FLesiones++;
                else if (tipoMateria == "familia")
                    GameManager.Instance.FFamilias++;
            }
        }

        if (textoSeleccionMultiple != null)
            textoSeleccionMultiple.gameObject.SetActive(false);

        StartCoroutine(FinalizarPreguntaRoutineLocal());
    }

    private System.Collections.IEnumerator FinalizarPreguntaRoutineLocal()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

    public override void EntregarRetroalimentacion()
    {
        if (canvasJuego != null) canvasJuego.gameObject.SetActive(false);

        if (respuestasCorrectasLista.Count <= 1)
        {
            esRespuestaCorrecta = (respuestaSeleccionada == respuestaCorrecta);
        }

        if (esRespuestaCorrecta)
        {
            textoResultado.text = "¡Respuesta Correcta!";
            if (respuestasCorrectasLista.Count > 1)
            {
                textoRespuesta.text = "Las descripciones correctas son:\n• " + string.Join("\n• ", respuestasCorrectasLista);
            }
            else
            {
                textoRespuesta.text = "La descripción correcta es:\n" + respuestaCorrecta;
            }
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
            if (respuestasCorrectasLista.Count > 1)
            {
                textoRespuesta.text = "Las descripciones correctas eran:\n• " + string.Join("\n• ", respuestasCorrectasLista);
            }
            else
            {
                textoRespuesta.text = "La descripción correcta era:\n" + respuestaCorrecta;
            }
        }

        if (canvasRetroalimentacion != null)
        {
            Button btnContinuar = canvasRetroalimentacion.GetComponentInChildren<Button>();
            btnContinuar.onClick.RemoveAllListeners();
            btnContinuar.onClick.AddListener(() => finished = true);
            canvasRetroalimentacion.gameObject.SetActive(true);
        }
    }
}