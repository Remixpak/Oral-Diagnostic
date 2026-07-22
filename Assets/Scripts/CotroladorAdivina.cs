using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;


public class ControladorAdivina : ControladorPreguntas
{
    [Header("Botones")]
    [SerializeField] private List<Button> diagnosticos = new List<Button>();

    [Header("Datos")]
    [SerializeField] private string patologiaCorrecta;
    [SerializeField] private string patologiaSeleccionada;

    private int idPatologia;

    private List<string> alternativas;

    [Header("Snackbar")]
    [SerializeField] private GameObject panelSnackbar;
    [SerializeField] private TMP_Text textoSnackbar;

    [Header("Libreta")]
    /*[SerializeField] private GameObject libretaCanvas;
    [SerializeField] private Transform contenidoLibreta;
    [SerializeField] private GameObject prefabNota;
    private HashSet<string> notas = new HashSet<string>();
    [SerializeField] private float margenEntreNotas = 15f;

    private float siguientePosicionY = 0f;*/
    [SerializeField] private GameObject libretaCanvas;
    [SerializeField] private TMP_Text textoNota;
    [Header("Seleccion")]
    [SerializeField] private TMP_Text textoDiag;
    [SerializeField] private Canvas canvasSeleccion;
    [SerializeField] private Button Seleccionado;

    private List<string> notas = new List<string>();
    private int indiceNotaActual = 0;

    private Coroutine snackbarCoroutine;

    [Header("Retroalimentacion")]
    [SerializeField] private TMP_Text textoRespuesta;
    [SerializeField] private TMP_Text textoResultado;

    private void Awake()
    {
        alternativas = new List<string>();
    }

    private void AgregarNota(string texto)
    {
        if(notas.Contains(texto))
            return;
        notas.Add(texto);
        if(notas.Count == 1)
        {
            indiceNotaActual = 0;
            ActualizarNota();
        }
    }
    private void ActualizarNota()
    {
        if(notas.Count == 0)
        {
            textoNota.text = "no hay nada escrito";
            return;
        }
        textoNota.text = notas[indiceNotaActual];
    }

    public void SiguienteNota()
    {
        if(notas.Count == 0)
            return;
        indiceNotaActual++;
        if(indiceNotaActual >= notas.Count)
            indiceNotaActual = notas.Count - 1;
        ActualizarNota();
    }
    public void NotaAnterior()
    {
        if(notas.Count == 0)
            return;
        indiceNotaActual--;
        if(indiceNotaActual < 0)
            indiceNotaActual = (indiceNotaActual + 1)% notas.Count;
        ActualizarNota();
        
    }


    public void AbrirLibreta()
    {
        libretaCanvas.SetActive(true);
    }
    public void CerrarLibreta()
    {
        libretaCanvas.SetActive(false);
    }

    private void ObtenerDiagnosticos()
    {
        alternativas.Clear();

        // Agrega la correcta
        alternativas.Add(patologiaCorrecta);

        // Copia de la lista de patologías
        List<Patologia> lista = new List<Patologia>(CsvManager.Instance.patologias);

        // Elimina la correcta
        lista.RemoveAll(p => p.nombre == patologiaCorrecta);

        // Rellena con respuestas aleatorias
        while (alternativas.Count < diagnosticos.Count)
        {
            int indice = Random.Range(0, lista.Count);

            alternativas.Add(lista[indice].nombre);

            lista.RemoveAt(indice);
        }

        // Barajar respuestas
        for (int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(i, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }

        // Asignar textos a los botones
        for (int i = 0; i < diagnosticos.Count; i++)
        {
            diagnosticos[i].GetComponentInChildren<TMP_Text>().text = alternativas[i];
        }
    }

    public void Seleccionar(Button boton)
    {
        Seleccionado = boton;
        textoDiag.text = boton.GetComponentInChildren<TMP_Text>().text;

        Debug.Log("Seleccionado: " + boton.name);

        canvasSeleccion.gameObject.SetActive(true);
    }
    public void Descartar()
    {
        Debug.Log("Descartando: " + Seleccionado);

        diagnosticos.Remove(Seleccionado);
        Destroy(Seleccionado.gameObject);

        Seleccionado = null;
        canvasSeleccion.gameObject.SetActive(false);
    }
    public void SeleccionarDiagnostico()
    {
        patologiaSeleccionada = Seleccionado.GetComponentInChildren<TMP_Text>().text;
        if(ValidarRespuesta(patologiaSeleccionada))
            GameManager.Instance.TotalAciertos++;
        else   
            GameManager.Instance.TotalFallos++;
        StartCoroutine(FinalizarPregunta());


        /*
        respuesta = boton.GetComponentInChildren<TMP_Text>().text;
        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            boton.GetComponent<Image>().color = Color.green;
            GameManager.Instance.TotalAciertos++;
            Debug.Log("Respuesta Correcta");
        }
        else
        {
            boton.GetComponent<Image>().color = Color.red;
            GameManager.Instance.TotalFallos++;
            Debug.Log("Respuesta Incorrecta");
        }
        StartCoroutine(FinalizarPregunta());
    }
        */
        

    }

    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(1.0f);
        EntregarRetroalimentacion();
    }
    private bool ValidarRespuesta(string respuesta)
    {
        return respuesta == patologiaCorrecta;
    }


    public void PreguntarLesion()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        string pista = $"La lesión es: {l.nombre}";
        MostrarPista(pista);
        AgregarNota(pista);

    }
    public void PreguntarFamilia()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Familia f = CsvManager.Instance.ObtenerFamiliaPorId(p.familiaID);
        string pista = $"La familia es: {f.nombre}";
        MostrarPista(pista);
        AgregarNota(pista);
    }
    public void PreguntarEtiologia()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(p.etiologiaID);
        string pista = $"La Etiologia es: {e.nombre}";
        MostrarPista(pista);
        AgregarNota(pista);
    }
    public void PreguntarDescripcion()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        if (p == null) return;

        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        if (l == null) return;

        List<Descripcion> listaDescripciones = CsvManager.Instance.ObtenerDescripcionesDeLesion(l);

        if (listaDescripciones == null || listaDescripciones.Count == 0)
        {
            MostrarPista("No hay descripciones disponibles para esta lesión.");
            return;
        }

        // Construye una lista numerada o con viñetas de todas las descripciones
        string pista = "Descripciones de la lesión:";
        for (int i = 0; i < listaDescripciones.Count; i++)
        {
            pista += $"\n{i + 1}. {listaDescripciones[i].texto}";
        }

        MostrarPista(pista);
        AgregarNota(pista);
    }

    private IEnumerator MostrarSnackbar(string mensaje)
    {
        panelSnackbar.SetActive(true);
        textoSnackbar.text = "";

        foreach (char c in mensaje)
        {
            textoSnackbar.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(3f);

        panelSnackbar.SetActive(false);
    }
    private void MostrarPista(string mensaje)
    {
        if (snackbarCoroutine != null)
            StopCoroutine(snackbarCoroutine);

        snackbarCoroutine = StartCoroutine(MostrarSnackbar(mensaje));
    }

    public override void EntregarRetroalimentacion()
    {
        canvasJuego.gameObject.SetActive(false);
        canvasSeleccion.gameObject.SetActive(false);
        if(ValidarRespuesta(patologiaSeleccionada))
            textoResultado.text = "Respuesta correcta";
        else    
            textoResultado.text = "Respuesta incorrecta";
        textoRespuesta.text = "La respuesta es: " + patologiaCorrecta;
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.AddListener(() => finished = true);
        canvasRetroalimentacion.gameObject.SetActive(true);

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        idPatologia = indPatologiaAsignada;

        notas.Clear();
        indiceNotaActual = 0;
        textoNota.text = "";
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        patologiaCorrecta = p.nombre;

        ObtenerDiagnosticos();

        foreach (Button boton in diagnosticos)
        {
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => Seleccionar(boton));
        }
    }
}