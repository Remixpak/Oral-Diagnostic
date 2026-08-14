using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

/// <summary>
/// Clase abstracta base que gestiona el ciclo de vida general, la interfaz de usuario, la selección de alternativas y la retroalimentación para cualquier tipo de pregunta en el juego.
/// 
/// Clases que utiliza y su finalidad:
/// - ControladorPreguntas: Clase base de la que hereda, definiendo la firma general para los controladores de preguntas.
/// - GameManager: Singleton (GameManager.Instance) utilizado para actualizar estadísticas del jugador (aciertos, fallos, fallos por materia) y pausar el juego.
/// - ControladorSonido: Singleton (ControladorSonido.Instance) encargado de reproducir efectos de audio (clics, victoria, derrota).
/// - TMP_Text / Image / Button / Canvas (UnityEngine.UI y TMPro): Componentes de la interfaz de usuario de Unity para renderizar enunciados, imágenes, alternativas interactuables y pantallas de retroalimentación.
/// </summary>
public abstract class ControladorPreguntaBase : ControladorPreguntas
{
    [Header("UI General")]
    [SerializeField] protected TMP_Text textoPregunta;
    [SerializeField] protected Image imagenPregunta;
    [SerializeField] protected TMP_Text textoNombreEntidad;

    [Header("Botones y Opciones")]
    [SerializeField] protected List<Button> botonesAlternativas;
    [SerializeField] protected Button botonPausa;

    [Header("Retroalimentación")]
    [SerializeField] protected TMP_Text textoResultado;
    [SerializeField] protected TMP_Text textoRespuesta;

    [SerializeField] protected string respuestaCorrecta;
    protected string respuestaSeleccionada;
    protected bool yaRespondio = false;

    protected string tipoMateria;

    /// <summary>
    /// Método abstracto que deben implementar las clases hijas para configurar los textos, imágenes y generar las opciones (correctas y distractores) específicas según el tipo de pregunta.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología asignada a la pregunta.</param>
    /// <param name="opciones">Lista de salida con los textos de las alternativas.</param>
    /// <param name="spritesOpciones">Lista de salida con las imágenes/sprites de las alternativas (o null si la pregunta es solo de texto).</param>
    protected abstract void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones);

    /// <summary>
    /// Inicializa la pregunta restableciendo los botones de la UI, configurando el botón de pausa, obteniendo los datos desde la subclase y asignando las respuestas (texto o imagen) a los botones correspondientes.
    /// </summary>
    /// <param name="idPatologiaAsignada">Identificador único de la patología a evaluar.</param>
    public override void InicializarPregunta(int idPatologiaAsignada)
    {
        ConfigurarBotonPausa();
        yaRespondio = false;

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
            btn.onClick.RemoveAllListeners();
            
            Image childImg = GetImageInChild(btn);
            if (childImg != null) childImg.gameObject.SetActive(false);
            
            TMP_Text childText = btn.GetComponentInChildren<TMP_Text>();
            if (childText != null) childText.gameObject.SetActive(true);
        }

        List<string> opcionesTexto;
        List<Sprite> opcionesSprite;
        ConfigurarPreguntaYRespuestas(idPatologiaAsignada, out opcionesTexto, out opcionesSprite);

        MezclarOpciones(opcionesTexto, opcionesSprite);

        for (int i = 0; i < botonesAlternativas.Count; i++)
        {
            Button btnActual = botonesAlternativas[i];

            if (opcionesSprite != null && opcionesSprite.Count > i && opcionesSprite[i] != null)
            {
                TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.gameObject.SetActive(false);

                Image imgChild = GetImageInChild(btnActual);
                if (imgChild != null)
                {
                    imgChild.gameObject.SetActive(true);
                    imgChild.sprite = opcionesSprite[i];
                    imgChild.preserveAspect = true;
                }
                else
                {
                    Debug.LogError($"[PreguntaUI] El botón '{btnActual.name}' no tiene una Image hija para mostrar el Sprite.");
                }

                string valorRespuesta = opcionesTexto[i];
                btnActual.onClick.AddListener(() => SeleccionarAlternativa(btnActual, valorRespuesta));
            }
            else if (opcionesTexto != null && opcionesTexto.Count > i)
            {
                Image imgChild = GetImageInChild(btnActual);
                if (imgChild != null) imgChild.gameObject.SetActive(false);

                TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
                if (txt != null)
                {
                    txt.gameObject.SetActive(true);
                    txt.text = opcionesTexto[i];
                }

                string valorRespuesta = opcionesTexto[i];
                btnActual.onClick.AddListener(() => SeleccionarAlternativa(btnActual, valorRespuesta));
            }
        }
    }

    /// <summary>
    /// Maneja el evento de selección de una alternativa por parte del usuario, validando si es correcta o incorrecta, cambiando el color del botón, actualizando las estadísticas globales en GameManager y reproduciendo sonido.
    /// </summary>
    /// <param name="boton">Botón de la interfaz que fue presionado.</param>
    /// <param name="valorSeleccionado">Valor de texto o identificador asociado a la alternativa seleccionada.</param>
    protected virtual void SeleccionarAlternativa(Button boton, string valorSeleccionado)
    {
        if (yaRespondio) return;
        ControladorSonido.Instance?.ReproducirClick();
        yaRespondio = true;

        foreach (Button btn in botonesAlternativas) btn.interactable = false;

        respuestaSeleccionada = valorSeleccionado;

        if (respuestaSeleccionada == respuestaCorrecta)
        {
            boton.GetComponent<Image>().color = Color.green;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Aciertos++;
                GameManager.Instance.TotalAciertos++;
            }
        }
        else
        {
            boton.GetComponent<Image>().color = Color.red;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Fallos++;
                GameManager.Instance.TotalFallos++;
                if(tipoMateria == "lesion")
                    GameManager.Instance.FLesiones++;
                else if(tipoMateria == "familia")
                    GameManager.Instance.FFamilias++;
            }
        }

        StartCoroutine(FinalizarPreguntaRoutine());
    }

    /// <summary>
    /// Muestra la interfaz de retroalimentación final indicando si el usuario acertó o falló, mostrando la respuesta correcta y configurando el botón para avanzar al siguiente estado.
    /// </summary>
    public override void EntregarRetroalimentacion()
    {
        if (canvasJuego != null) canvasJuego.gameObject.SetActive(false);

        if (respuestaSeleccionada == respuestaCorrecta)
        {
            ControladorSonido.Instance?.ReproducirWin();
            textoResultado.text = "¡Respuesta Correcta!";
            textoRespuesta.text = "La respuesta correcta es: " + respuestaCorrecta;
        }
        else
        {
            ControladorSonido.Instance?.ReproducirLoss();
            textoResultado.text = "Respuesta Incorrecta";
            textoRespuesta.text = " ";
        }

        if (canvasRetroalimentacion != null)
        {
            Button btnContinuar = canvasRetroalimentacion.GetComponentInChildren<Button>();
            btnContinuar.onClick.RemoveAllListeners();
            btnContinuar.onClick.AddListener(() => {
                ControladorSonido.Instance?.ReproducirClick();
                finished = true;
            });
            canvasRetroalimentacion.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Corrutina que introduce una pequeña pausa temporal tras responder antes de llamar a la pantalla de retroalimentación.
    /// </summary>
    /// <returns>IEnumerator para el control del flujo asíncrono de Unity.</returns>
    private IEnumerator FinalizarPreguntaRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

    /// <summary>
    /// Reordena aleatoriamente los elementos de la lista de textos y sincroniza dicho reordenamiento con la lista de sprites correspondientes.
    /// </summary>
    /// <param name="textos">Lista de textos de las opciones a mezclar.</param>
    /// <param name="sprites">Lista de sprites de las opciones a mezclar de forma paritaria con los textos.</param>
    protected void MezclarOpciones(List<string> textos, List<Sprite> sprites)
    {
        for (int i = 0; i < textos.Count; i++)
        {
            int j = Random.Range(0, textos.Count);
            (textos[i], textos[j]) = (textos[j], textos[i]);
            if (sprites != null && sprites.Count == textos.Count)
            {
                (sprites[i], sprites[j]) = (sprites[j], sprites[i]);
            }
        }
    }

    /// <summary>
    /// Busca y retorna el primer componente Image que sea hijo del botón especificado (excluyendo la imagen principal del propio botón).
    /// </summary>
    /// <param name="btn">El botón en el cual buscar la imagen hija.</param>
    /// <returns>El componente Image encontrado en los hijos, o null si no existe.</returns>
    private Image GetImageInChild(Button btn)
    {
        foreach (Image img in btn.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject != btn.gameObject) return img;
        }
        return null;
    }

    /// <summary>
    /// Configura el listener y estado visual del botón de pausa en la interfaz de usuario.
    /// </summary>
    private void ConfigurarBotonPausa()
    {
        if (botonPausa == null) return;
        botonPausa.onClick.RemoveAllListeners();
        botonPausa.onClick.AddListener(() => GameManager.Instance?.PausarJuego());
        botonPausa.interactable = true;
        botonPausa.gameObject.SetActive(true);
    }
}