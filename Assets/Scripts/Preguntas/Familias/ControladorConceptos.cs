using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controlador para el modo de pregunta basado en conceptos. 
/// Selecciona de forma aleatoria un enunciado médico y genera una lista de alternativas 
/// con el concepto clave correcto y sus respectivos distractores para ser mostrados en la interfaz.
/// 
/// Clases de las que depende y su fin:
/// - ControladorPreguntaBase: Clase base de la que hereda para la gestión general del ciclo de vida de la pregunta.
/// - TextMeshProUGUI (TMPro): Componente de interfaz de usuario para desplegar el texto del enunciado seleccionado.
/// - Sprite / Vector / Random (UnityEngine): Tipos e infraestructura de Unity para manejo de recursos visuales y aleatoriedad.
/// </summary>
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

    private readonly List<string> todosLosConceptos = new List<string>()
    {
        "Familia", "Etiopatogenia", "Patología", "Etiología", "Diagnóstico", "Manifestación clínica", "Lesión básica"
    };

    /// <summary>
    /// Configura la pregunta seleccionando un enunciado al azar de la lista, asigna la respuesta correcta 
    /// y construye la lista de alternativas distractoras sin repetir la opción correcta.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador de la patología asignada (no utilizado en este tipo de pregunta).</param>
    /// <param name="opciones">Lista de cadenas de texto de salida con las alternativas para los botones.</param>
    /// <param name="spritesOpciones">Lista de sprites de salida (se establece en null ya que este modo es textual).</param>
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null; 

        int indiceAleatorio = Random.Range(0, listaEnunciados.Count);
        EnunciadoConcepto seleccionado = listaEnunciados[indiceAleatorio];

        if (textoPregunta != null)
        {
            textoPregunta.text = "\"" + seleccionado.enunciado + "\"";
        }

        respuestaCorrecta = seleccionado.conceptoClave;

        opciones = new List<string>();
        opciones.Add(respuestaCorrecta);

        List<string> distractoresDisponibles = new List<string>(todosLosConceptos);
        distractoresDisponibles.Remove(respuestaCorrecta);

        for (int i = 0; i < distractoresDisponibles.Count; i++)
        {
            int j = Random.Range(0, distractoresDisponibles.Count);
            (distractoresDisponibles[i], distractoresDisponibles[j]) = (distractoresDisponibles[j], distractoresDisponibles[i]);
        }

        int cantidadBotones = botonesAlternativas != null ? botonesAlternativas.Count : 4;
        for (int i = 0; i < distractoresDisponibles.Count && opciones.Count < cantidadBotones; i++)
        {
            opciones.Add(distractoresDisponibles[i]);
        }
    }
}