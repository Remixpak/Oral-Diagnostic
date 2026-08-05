using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControladorConceptos : ControladorPreguntaBase
{
    private struct EnunciadoConcepto
    {
        public string enunciado;
        public string conceptoClave;


        public EnunciadoConcepto(string enunciado, string conceptoClave)
        {
            this.enunciado = enunciado;
            this.conceptoClave = conceptoClave;
        }
    }

    //lista de enunciados con su concepto correspondiente 
    private readonly List<EnunciadoConcepto> listaEnunciados = new List<EnunciadoConcepto>()
    {
        new EnunciadoConcepto(
            "Conjunto de alteraciones tisulares o tumorales que comparten origen celular, características histológicas, perfil genético o un espectro evolutivo común, aunque presenten nombres o grados distintos.",
            "Familia"),

        new EnunciadoConcepto(
            "Origen de una enfermedad y sus mecanismos de progresión.",
            "Etiopatogenia"),

        new EnunciadoConcepto(
            "Parte de la medicina que estudia los mecanismos que producen una enfermedad.",
            "Patología"),

        new EnunciadoConcepto(
            "Causas de una enfermedad.",
            "Etiología"),

        new EnunciadoConcepto(
            "Proceso en el que se identifica una enfermedad, afección o lesión por sus signos y síntomas.",
            "Diagnóstico"),

        new EnunciadoConcepto(
            "Acontecimiento, fenómeno, sensación o alteración que puede apreciar el enfermo (síntoma) o el médico (signo) como consecuencia de una enfermedad.",
            "Manifestación clínica"),

        new EnunciadoConcepto(
            "Daño o alteración morfológica más simple y originaria que sufre un tejido.",
            "Lesión básica")
    };

    //lista de conceptos 
    private readonly List<string> todosLosConceptos = new List<string>()
    {
        "Familia", "Etiopatogenia", "Patología", "Etiología", "Diagnóstico", "Manifestación clínica", "Lesión básica"
    };

    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null; 

        //elegimos de forma aleatoria un enunciado de la lista de enunciados
        int indiceAleatorio = Random.Range(0, listaEnunciados.Count);
        EnunciadoConcepto seleccionado = listaEnunciados[indiceAleatorio];

        //asignamos el texto del enunciado a un TextMeshProUGUI para mostrarlo en la UI
        if (textoPregunta != null)
        {
            textoPregunta.text = "\"" + seleccionado.enunciado + "\"";
        }

        respuestaCorrecta = seleccionado.conceptoClave;

        // generamos las alternativas de respuesta para los botones
        opciones = new List<string>();
        opciones.Add(respuestaCorrecta); //agregamos la respuesta correcta :p

        //seleccionamos de la lista las opciones distractoras para evitar repetir la respuesta correcta
        List<string> distractoresDisponibles = new List<string>(todosLosConceptos);
        distractoresDisponibles.Remove(respuestaCorrecta);

        //mezclamos las alternativas
        for (int i = 0; i < distractoresDisponibles.Count; i++)
        {
            int j = Random.Range(0, distractoresDisponibles.Count);
            (distractoresDisponibles[i], distractoresDisponibles[j]) = (distractoresDisponibles[j], distractoresDisponibles[i]);
        }

        //alternativas distractoras
        int cantidadBotones = botonesAlternativas != null ? botonesAlternativas.Count : 4;
        for (int i = 0; i < distractoresDisponibles.Count && opciones.Count < cantidadBotones; i++)
        {
            opciones.Add(distractoresDisponibles[i]);
        }
    }
void Start()
    {
        if (botonPausa != null)
        {
            botonPausa.onClick.RemoveAllListeners();
            botonPausa.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    if (Time.timeScale == 1f)
                        GameManager.Instance.PausarJuego();
                    else
                        GameManager.Instance.ReanudarJuego();
                }
                else
                {
                    Time.timeScale = Time.timeScale == 1 ? 0 : 1;
                }
            });
            botonPausa.interactable = true;
        }
    }

    void Update()
    {
        if (botonPausa != null && !botonPausa.interactable)
        {
            botonPausa.interactable = true;
        }
    }
}