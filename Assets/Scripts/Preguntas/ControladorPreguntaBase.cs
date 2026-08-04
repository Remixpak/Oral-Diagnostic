using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public abstract class ControladorPreguntaBase : ControladorPreguntas
{
    [Header("UI General")]
    [SerializeField] protected TMP_Text textoPregunta;
    [SerializeField] protected Image imagenPregunta; // Opcional según la pregunta
    [SerializeField] protected TMP_Text textoNombreEntidad; // Para preguntas estilo "Definición de X"

    [Header("Botones y Opciones")]
    [SerializeField] protected List<Button> botonesAlternativas;
    [SerializeField] protected Button botonPausa;

    [Header("Retroalimentación")]
    [SerializeField] protected TMP_Text textoResultado;
    [SerializeField] protected TMP_Text textoRespuesta;

    [SerializeField] protected string respuestaCorrecta;
    protected string respuestaSeleccionada;
    protected bool yaRespondio = false;

    // Métodos abstractos que cada tipo de pregunta implementará a su manera
    protected abstract void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones);

    public override void InicializarPregunta(int idPatologiaAsignada)
    {
        ConfigurarBotonPausa();
        yaRespondio = false;

        // Resetear visualmente botones
        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
            btn.onClick.RemoveAllListeners();
            
            // Limpiar imágenes de botones por si el prefab anterior usaba sprites
            Image childImg = GetImageInChild(btn);
            if (childImg != null) childImg.gameObject.SetActive(false);
            
            TMP_Text childText = btn.GetComponentInChildren<TMP_Text>();
            if (childText != null) childText.gameObject.SetActive(true);
        }

        // Obtener datos desde la subclase
        List<string> opcionesTexto;
        List<Sprite> opcionesSprite;
        ConfigurarPreguntaYRespuestas(idPatologiaAsignada, out opcionesTexto, out opcionesSprite);

        // Mezclar alternativas en conjunto
        MezclarOpciones(opcionesTexto, opcionesSprite);

        // Asignar alternativas a los botones UI
        // Dentro de InicializarPregunta(...) en ControladorPreguntaBase.cs:

        for (int i = 0; i < botonesAlternativas.Count; i++)
        {
            Button btnActual = botonesAlternativas[i];

            if (opcionesSprite != null && opcionesSprite.Count > i && opcionesSprite[i] != null)
            {
                // 1. Ocultar el texto del botón
                TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.gameObject.SetActive(false);

                // 2. Buscar y asignar la imagen
                Image imgChild = GetImageInChild(btnActual);
                if (imgChild != null)
                {
                    imgChild.gameObject.SetActive(true);
                    imgChild.sprite = opcionesSprite[i];
                    imgChild.preserveAspect = true; // Mantener la proporción de la imagen médica
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
                // Ocultar imagen hija si es pregunta de texto
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

    protected virtual void SeleccionarAlternativa(Button boton, string valorSeleccionado)
    {
        if (yaRespondio) return;
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
            }
        }

        StartCoroutine(FinalizarPreguntaRoutine());
    }

    public override void EntregarRetroalimentacion()
    {
        if (canvasJuego != null) canvasJuego.gameObject.SetActive(false);

        if (respuestaSeleccionada == respuestaCorrecta)
        {
            textoResultado.text = "¡Respuesta Correcta!";
            textoRespuesta.text = "La respuesta correcta es: " + respuestaCorrecta;
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
            textoRespuesta.text = " ";
        }

        if (canvasRetroalimentacion != null)
        {
            Button btnContinuar = canvasRetroalimentacion.GetComponentInChildren<Button>();
            btnContinuar.onClick.RemoveAllListeners();
            btnContinuar.onClick.AddListener(() => finished = true);
            canvasRetroalimentacion.gameObject.SetActive(true);
        }
    }

    private IEnumerator FinalizarPreguntaRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

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

    private Image GetImageInChild(Button btn)
    {
        // Pasar true para buscar también en GameObjects hijos desactivados
        foreach (Image img in btn.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject != btn.gameObject) return img;
        }
        return null;
    }

    private void ConfigurarBotonPausa()
    {
        if (botonPausa == null) return;
        botonPausa.onClick.RemoveAllListeners();
        botonPausa.onClick.AddListener(() => GameManager.Instance?.PausarJuego());
        botonPausa.interactable = true;
        botonPausa.gameObject.SetActive(true);
    }
}