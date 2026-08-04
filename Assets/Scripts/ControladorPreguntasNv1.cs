using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class ControladorPreguntasNv1 : ControladorPreguntas
{
    [Header("Atributos")]
    [SerializeField] private string pregunta;
    [SerializeField] private string respuestaCorrecta;
    [SerializeField] private string respuesta;
    [SerializeField] private List<string> alternativas;

    private int idPatologiaNumerica;
    private string idPatologia;
    [SerializeField] private Image imagen;

    [Header("Botones")]
    [SerializeField] private List<Button> botonesAlternativas;
    [SerializeField] private Button botonPausa;

    [Header("Textos de retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;
    [SerializeField] public TMP_Text textoRespuesta;

    private bool yaRespondio = false;

    void Start()
    {

    }

    void Update()
    {

    }

    public void ObtenerImagen()
    {
        Patologia patologia = CsvManager.Instance.ObtenerPatologiaPorId(int.Parse(idPatologia));
        imagen.sprite = CsvManager.Instance.spritePorCodigo(patologia.codigoImagen);
    }

    public void ObtnerLesion()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaNumerica);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = l.nombre;
        alternativas.Add(respuestaCorrecta);
    }

    public bool ComprobarRespuesta(string respuesta, string respuestaCorrecta)
    {
        return respuesta == respuestaCorrecta;
    }

    public void SeleccionarAlternativa(Button boton)
    {
        if (yaRespondio) return;
        yaRespondio = true;

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = false;
        }

        respuesta = boton.GetComponentInChildren<TMP_Text>().text;

        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            boton.GetComponent<Image>().color = Color.green;
            GameManager.Instance.Aciertos++;
            GameManager.Instance.TotalAciertos++;
        }
        else
        {
            boton.GetComponent<Image>().color = Color.red;
            GameManager.Instance.Fallos++;
            GameManager.Instance.TotalFallos++;
        }

        StartCoroutine(FinalizarPregunta());
    }

    public void RellenarRespuestas()
    {
        alternativas.Clear();

        alternativas.Add(respuestaCorrecta);
        List<Lesion> l = new List<Lesion>(CsvManager.Instance.lesiones);
        l.RemoveAll(l => l.nombre == respuestaCorrecta);

        while (alternativas.Count < botonesAlternativas.Count)
        {
            int indice = Random.Range(0, l.Count);
            alternativas.Add(l[indice].nombre);
            l.RemoveAt(indice);
        }

        for (int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(0, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }

        for (int i = 0; i < botonesAlternativas.Count; i++)
        {
            botonesAlternativas[i].GetComponentInChildren<TMP_Text>().text = alternativas[i];
        }
    }

    public override void EntregarRetroalimentacion()
    {
        canvasJuego.gameObject.SetActive(false);

        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            textoResultado.text = "¡Respuesta Correcta!";
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
        }

        textoRespuesta.text = "La respuesta correcta es: " + respuestaCorrecta;
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.AddListener(() => finished = true);
        canvasRetroalimentacion.gameObject.SetActive(true);
    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        ConfigurarBotonPausa();
        yaRespondio = false;
        idPatologiaNumerica = indPatologiaAsignada;
        idPatologia = idPatologiaNumerica.ToString();

        alternativas = new List<string>();

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
        }

        ObtnerLesion();
        ObtenerImagen();
        RellenarRespuestas();

        foreach (Button boton in botonesAlternativas)
        {
            boton.onClick.RemoveAllListeners();
            Button botonActual = boton;
            botonActual.onClick.AddListener(() => SeleccionarAlternativa(botonActual));
        }
    }

    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

    private void ConfigurarBotonPausa()
    {
        if (botonPausa == null)
        {
            Debug.LogWarning("[PausaDebug] El botonPausa es NULL en el Inspector.");
            return;
        }

        Debug.Log($"[PausaDebug] Configurando boton pausa. Nombre: {botonPausa.name}, Interactable antes: {botonPausa.interactable}, ActiveInHierarchy antes: {botonPausa.gameObject.activeInHierarchy}");

        botonPausa.onClick.RemoveAllListeners();
        botonPausa.onClick.AddListener(() => {
            Debug.Log("[PausaDebug] ¡Se presionó el botón de pausa!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PausarJuego();
            }
            else
            {
                Debug.LogError("[PausaDebug] GameManager.Instance es NULL al intentar pausar.");
            }
        });

        if (!botonPausa.interactable)
        {
            botonPausa.interactable = true;
            Debug.Log("[PausaDebug] El botonPausa estaba en false, se forzó a true.");
        }

        if (!botonPausa.gameObject.activeInHierarchy)
        {
            botonPausa.gameObject.SetActive(true);
            Debug.Log("[PausaDebug] El GameObject del botonPausa estaba inactivo, se activó.");
        }
    }
}