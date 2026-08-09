/*using System.Collections.Generic;
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

    private float siguientePosicionY = 0f;
    [SerializeField] private GameObject libretaCanvas;
    [SerializeField] private TMP_Text textoNota;
    [SerializeField] private RectTransform panelNota;

    [Header("Seleccion")]
    [SerializeField] private TMP_Text textoDiag;
    [SerializeField] private Canvas canvasSeleccion;
    [SerializeField] private Button Seleccionado;
    [SerializeField] private Button cerrarSeleccion;
    [SerializeField] private Button btnDescartar;

    private List<string> notas = new List<string>();
    private int indiceNotaActual = 0;

    private Coroutine snackbarCoroutine;
    private float alturaMaximaSnackbar;
    private float alturaMinimaSnackbar = 80f;
    private bool respuestaCorrectaDescartada = false; 

    [Header("Retroalimentacion")]
    [SerializeField] private TMP_Text textoRespuesta;
    [SerializeField] private TMP_Text textoResultado;

    private void Awake()
    {
        alternativas = new List<string>();

        if (cerrarSeleccion != null)
        {
            cerrarSeleccion.onClick.RemoveAllListeners();
            cerrarSeleccion.onClick.AddListener(CerrarPanelSeleccion);
        }
    }

    public void CerrarPanelSeleccion()
    {
        canvasSeleccion.gameObject.SetActive(false);
        Seleccionado = null; 
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

        alternativas.Add(patologiaCorrecta);

        List<Patologia> lista = new List<Patologia>(CsvManager.Instance.patologias);

        lista.RemoveAll(p => p.nombre == patologiaCorrecta);

        while (alternativas.Count < diagnosticos.Count)
        {
            int indice = Random.Range(0, lista.Count);

            alternativas.Add(lista[indice].nombre);

            lista.RemoveAt(indice);
        }

        for (int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(i, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }

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
        if (Seleccionado == null) return;
        string diagnosticoSeleccionado = Seleccionado.GetComponentInChildren<TMP_Text>().text;

        if (diagnosticoSeleccionado == patologiaCorrecta)
        {
            respuestaCorrectaDescartada = true;
            canvasSeleccion.gameObject.SetActive(false);
            Seleccionado = null;
            StartCoroutine(FinalizarPregunta());
            return;
        }

        Debug.Log("Descartando: " + Seleccionado.name);
        diagnosticos.Remove(Seleccionado);
        Destroy(Seleccionado.gameObject);
        Seleccionado = null;
        canvasSeleccion.gameObject.SetActive(false);

        VerificarOpcionesDisponibles();
    }

    public void SeleccionarDiagnostico()
    {
        patologiaSeleccionada = Seleccionado.GetComponentInChildren<TMP_Text>().text;
        if(ValidarRespuesta(patologiaSeleccionada))
        {
            GameManager.Instance.Aciertos++;
            GameManager.Instance.TotalAciertos++;
        }
        else
        {
            
        
            GameManager.Instance.Fallos++;
            GameManager.Instance.TotalFallos++;
        }
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
        
       
    }

    private void VerificarOpcionesDisponibles()
    {
        if (btnDescartar == null) return;

        int opcionesRestantes = 0;
        int opcionesCorrectas = 0;

        foreach (Button boton in diagnosticos)
        {
            if (boton != null)
            {
                opcionesRestantes++;
                string texto = boton.GetComponentInChildren<TMP_Text>().text;
                if (texto == patologiaCorrecta)
                    opcionesCorrectas++;
            }
        }
        if (opcionesRestantes == 1 && opcionesCorrectas == 1)
        {
            btnDescartar.interactable = false;
        }
        else
        {
            btnDescartar.interactable = true;
        }
    }

    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(0.5f);
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

        string pista = "Descripciones de la lesión:";
        for (int i = 0; i < listaDescripciones.Count; i++)
        {
            pista += $"\n{i + 1}. {listaDescripciones[i].texto}";
        }

        MostrarPista(pista);
        AgregarNota(pista);
    }

    private void ConfigurarSnackbar()
    {
        if (panelSnackbar == null || textoSnackbar == null) return;

        RectTransform panelRect = panelSnackbar.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.offsetMin = new Vector2(30, 40);
            panelRect.offsetMax = new Vector2(-30, 100);
            panelRect.localScale = Vector3.one;
        }

        textoSnackbar.alignment = TextAlignmentOptions.MidlineLeft;
        textoSnackbar.color = Color.white;

        Image img = panelSnackbar.GetComponent<Image>();
        if (img == null)
            img = panelSnackbar.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.18f, 0.92f);
        img.raycastTarget = false;

        alturaMaximaSnackbar = Screen.height * 0.35f;
        if (alturaMaximaSnackbar < 100f)
            alturaMaximaSnackbar = 100f;

        panelSnackbar.SetActive(false);
    }


    private IEnumerator MostrarSnackbar(string mensaje)
    {
        panelSnackbar.SetActive(true);
        textoSnackbar.text = "";

        RectTransform panelRect = panelSnackbar.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.offsetMin = new Vector2(30, 40);
            panelRect.offsetMax = new Vector2(-30, 100);
        }

        string[] lineas = mensaje.Split('\n');
        List<string> lineasVisibles = new List<string>();
        string textoActual = "";

        foreach (string linea in lineas)
        {
            string lineaActual = "";
            for (int i = 0; i < linea.Length; i++)
            {
                lineaActual += linea[i];
                textoActual = "";
                foreach (string l in lineasVisibles)
                    textoActual += l + "\n";
                textoActual += lineaActual;

                textoSnackbar.text = textoActual;

                yield return null;
                textoSnackbar.ForceMeshUpdate();
                float alturaTexto = textoSnackbar.preferredHeight;

                if (alturaTexto > alturaMaximaSnackbar && lineasVisibles.Count > 0)
                {
                    lineasVisibles.RemoveAt(0);
                    textoActual = "";
                    foreach (string l in lineasVisibles)
                        textoActual += l + "\n";
                    textoActual += lineaActual;
                    textoSnackbar.text = textoActual;
                    textoSnackbar.ForceMeshUpdate();
                }

                if (panelRect != null)
                {
                    float alturaFinal = Mathf.Clamp(textoSnackbar.preferredHeight + 30f, alturaMinimaSnackbar, alturaMaximaSnackbar);
                    panelRect.offsetMax = new Vector2(-30, alturaFinal + 20);
                }

                yield return new WaitForSeconds(0.025f);
            }

            lineasVisibles.Add(lineaActual);
        }

        string textoFinal = "";
        foreach (string l in lineasVisibles)
            textoFinal += l + "\n";
        textoSnackbar.text = textoFinal;
        textoSnackbar.ForceMeshUpdate();

        if (panelRect != null)
        {
            float alturaFinal = Mathf.Clamp(textoSnackbar.preferredHeight + 30f, alturaMinimaSnackbar, alturaMaximaSnackbar);
            panelRect.offsetMax = new Vector2(-30, alturaFinal + 20);
        }
        yield return new WaitForSeconds(3f);

        CanvasGroup canvasGroup = panelSnackbar.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panelSnackbar.AddComponent<CanvasGroup>();

        float tiempo = 0f;
        while (tiempo < 0.3f)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = 1f - (tiempo / 0.3f);
            yield return null;
        }

        panelSnackbar.SetActive(false);
        canvasGroup.alpha = 1f;
    }

    private void MostrarPista(string mensaje)
    {
        if (snackbarCoroutine != null)
            StopCoroutine(snackbarCoroutine);
        ConfigurarSnackbar();
        snackbarCoroutine = StartCoroutine(MostrarSnackbar(mensaje));
    }

    public override void EntregarRetroalimentacion()
    {
        canvasJuego.gameObject.SetActive(false);
        canvasSeleccion.gameObject.SetActive(false);

        if (respuestaCorrectaDescartada)
        {
            textoResultado.text = "Has fallado";
            textoRespuesta.text = " ";
        }

        else if(ValidarRespuesta(patologiaSeleccionada))
        {
            textoResultado.text = "Respuesta correcta";
            textoRespuesta.text = "La respuesta es: " + patologiaCorrecta;
        }
        else    
        {
            textoResultado.text = "Respuesta incorrecta";
            textoRespuesta.text = " ";
        }
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.AddListener(() => finished = true);
        canvasRetroalimentacion.gameObject.SetActive(true);

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        idPatologia = indPatologiaAsignada;
        respuestaCorrectaDescartada = false;
        notas.Clear();
        indiceNotaActual = 0;
        textoNota.text = "";
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        patologiaCorrecta = p.nombre;

        ObtenerDiagnosticos();

        foreach (Button boton in diagnosticos)
        {
            Button btnactual = boton;
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => Seleccionar(btnactual));
        }

        if (btnDescartar != null)
        {
            btnDescartar.interactable = true;
        }
        VerificarOpcionesDisponibles();
    }
}*/

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

    // Guardaremos los objetos Patologia en lugar de solo los nombres
    // para poder acceder tanto al nombre como a la ruta de la imagen
    private List<Patologia> patologiasOpciones = new List<Patologia>();

    [Header("Snackbar")]
    [SerializeField] private GameObject panelSnackbar;
    [SerializeField] private TMP_Text textoSnackbar;

    
    [Header("Seleccion")]
    [SerializeField] private TMP_Text textoDiag;
    [SerializeField] private Canvas canvasSeleccion;
    [SerializeField] private Button Seleccionado;
    [SerializeField] private Button cerrarSeleccion;
    [SerializeField] private Button btnDescartar;

   
  

    private Coroutine snackbarCoroutine;
    private float alturaMaximaSnackbar;
    private float alturaMinimaSnackbar = 80f;
    private bool respuestaCorrectaDescartada = false; 

    [Header("Retroalimentacion")]
    [SerializeField] private TMP_Text textoRespuesta;
    [SerializeField] private TMP_Text textoResultado;

    private void Awake()
    {
        patologiasOpciones = new List<Patologia>();

        if (cerrarSeleccion != null)
        {
            cerrarSeleccion.onClick.RemoveAllListeners();
            cerrarSeleccion.onClick.AddListener(CerrarPanelSeleccion);
        }
    }

    public void CerrarPanelSeleccion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasSeleccion.gameObject.SetActive(false);
        Seleccionado = null; 
    }

    

    

   

    /// <summary>
    /// Busca una Image hija que NO sea la imagen principal del fondo del botón.
    /// </summary>
    private Image ObtenerImagenHija(Button btn)
    {
        Image[] imagenes = btn.GetComponentsInChildren<Image>(true);
        foreach (Image img in imagenes)
        {
            if (img.gameObject != btn.gameObject)
            {
                return img;
            }
        }
        return null;
    }

    private void ObtenerDiagnosticos()
    {
        patologiasOpciones.Clear();

        Patologia correcta = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        patologiasOpciones.Add(correcta);

        List<Patologia> listaAux = new List<Patologia>(CsvManager.Instance.patologias);
        listaAux.RemoveAll(p => p.id == idPatologia || p.nombre == patologiaCorrecta);

        // Seleccionar patologías distractoras aleatorias
        while (patologiasOpciones.Count < diagnosticos.Count && listaAux.Count > 0)
        {
            int indice = Random.Range(0, listaAux.Count);
            patologiasOpciones.Add(listaAux[indice]);
            listaAux.RemoveAt(indice);
        }

        // Mezclar las opciones (Fisher-Yates)
        for (int i = 0; i < patologiasOpciones.Count; i++)
        {
            int j = Random.Range(i, patologiasOpciones.Count);
            (patologiasOpciones[i], patologiasOpciones[j]) = (patologiasOpciones[j], patologiasOpciones[i]);
        }

        // Asignar texto e imagen a cada botón
        for (int i = 0; i < diagnosticos.Count; i++)
        {
            if (i >= patologiasOpciones.Count) break;

            Button btnActual = diagnosticos[i];
            Patologia patologiaActual = patologiasOpciones[i];

            // 1. Asignar Texto (Pie de Página)
            TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
            {
                txt.text = patologiaActual.nombre;
                txt.gameObject.SetActive(true);
            }

            // 2. Cargar y Asignar Imagen de la Patología
            Image imgHija = ObtenerImagenHija(btnActual);
            if (imgHija != null)
            {
                // Carga la imagen desde Resources usando la ruta/nombre guardado en CsvManager
                Sprite spritePatologia = CsvManager.Instance.ObtenerSpriteDePatologia(patologiaActual);

                if (spritePatologia != null)
                {
                    imgHija.sprite = spritePatologia;
                    imgHija.preserveAspect = true;
                    imgHija.gameObject.SetActive(true);
                }
                else
                {
                    // Si no se encuentra sprite, ocultamos el contenedor de imagen
                    imgHija.gameObject.SetActive(false);
                }
            }
        }
    }

    public void Seleccionar(Button boton)
    {
        ControladorSonido.Instance?.ReproducirClick();
        Seleccionado = boton;
        textoDiag.text = boton.GetComponentInChildren<TMP_Text>().text;

        Debug.Log("Seleccionado: " + boton.name);

        canvasSeleccion.gameObject.SetActive(true);
    }

    public void Descartar()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (Seleccionado == null) return;
        string diagnosticoSeleccionado = Seleccionado.GetComponentInChildren<TMP_Text>().text;

        if (diagnosticoSeleccionado == patologiaCorrecta)
        {
            respuestaCorrectaDescartada = true;
            canvasSeleccion.gameObject.SetActive(false);
            Seleccionado = null;
            StartCoroutine(FinalizarPregunta());
            return;
        }

        Debug.Log("Descartando: " + Seleccionado.name);
        diagnosticos.Remove(Seleccionado);
        Destroy(Seleccionado.gameObject);
        Seleccionado = null;
        canvasSeleccion.gameObject.SetActive(false);

        VerificarOpcionesDisponibles();
    }

    public void SeleccionarDiagnostico()
    {
        ControladorSonido.Instance?.ReproducirClick();
        patologiaSeleccionada = Seleccionado.GetComponentInChildren<TMP_Text>().text;
        if(ValidarRespuesta(patologiaSeleccionada))
        {
            GameManager.Instance.Aciertos++;
            GameManager.Instance.TotalAciertos++;
        }
        else
        {
            GameManager.Instance.Fallos++;
            GameManager.Instance.TotalFallos++;
        }
        StartCoroutine(FinalizarPregunta());
    }

    private void VerificarOpcionesDisponibles()
    {
        if (btnDescartar == null) return;

        int opcionesRestantes = 0;
        int opcionesCorrectas = 0;

        foreach (Button boton in diagnosticos)
        {
            if (boton != null)
            {
                opcionesRestantes++;
                string texto = boton.GetComponentInChildren<TMP_Text>().text;
                if (texto == patologiaCorrecta)
                    opcionesCorrectas++;
            }
        }
        if (opcionesRestantes == 1 && opcionesCorrectas == 1)
        {
            btnDescartar.interactable = false;
        }
        else
        {
            btnDescartar.interactable = true;
        }
    }

    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(0.5f);
        EntregarRetroalimentacion();
    }

    private bool ValidarRespuesta(string respuesta)
    {
        return respuesta == patologiaCorrecta;
    }

    public void PreguntarLesion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        string pista = $"La lesión es: {l.nombre}";
        MostrarPista(pista);
        
    }

    public void PreguntarFamilia()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Familia f = CsvManager.Instance.ObtenerFamiliaPorId(p.familiaID);
        string pista = $"La familia es: {f.nombre}";
        MostrarPista(pista);
        
    }

    public void PreguntarEtiologia()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(p.etiologiaID);
        string pista = $"La Etiopatogenia es: {e.nombre}";
        MostrarPista(pista);
        
    }

    public void PreguntarDescripcion()
    {
        ControladorSonido.Instance?.ReproducirClick();
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

        string pista = "Descripciones de la lesión:";
        for (int i = 0; i < listaDescripciones.Count; i++)
        {
            pista += $"\n{i + 1}. {listaDescripciones[i].texto}";
        }

        MostrarPista(pista);
        
    }

    private void ConfigurarSnackbar()
    {
        if (panelSnackbar == null || textoSnackbar == null) return;

        RectTransform panelRect = panelSnackbar.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.offsetMin = new Vector2(30, 40);
            panelRect.offsetMax = new Vector2(-30, 100);
            panelRect.localScale = Vector3.one;
        }

        textoSnackbar.alignment = TextAlignmentOptions.MidlineLeft;
        textoSnackbar.color = Color.white;

        Image img = panelSnackbar.GetComponent<Image>();
        if (img == null)
            img = panelSnackbar.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.18f, 0.92f);
        img.raycastTarget = false;

        alturaMaximaSnackbar = Screen.height * 0.35f;
        if (alturaMaximaSnackbar < 100f)
            alturaMaximaSnackbar = 100f;

        panelSnackbar.SetActive(false);
    }

    private IEnumerator MostrarSnackbar(string mensaje)
    {
        panelSnackbar.SetActive(true);
        textoSnackbar.text = "";

        RectTransform panelRect = panelSnackbar.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.offsetMin = new Vector2(30, 40);
            panelRect.offsetMax = new Vector2(-30, 100);
        }

        string[] lineas = mensaje.Split('\n');
        List<string> lineasVisibles = new List<string>();
        string textoActual = "";

        foreach (string linea in lineas)
        {
            string lineaActual = "";
            for (int i = 0; i < linea.Length; i++)
            {
                lineaActual += linea[i];
                textoActual = "";
                foreach (string l in lineasVisibles)
                    textoActual += l + "\n";
                textoActual += lineaActual;

                textoSnackbar.text = textoActual;

                yield return null;
                textoSnackbar.ForceMeshUpdate();
                float alturaTexto = textoSnackbar.preferredHeight;

                if (alturaTexto > alturaMaximaSnackbar && lineasVisibles.Count > 0)
                {
                    lineasVisibles.RemoveAt(0);
                    textoActual = "";
                    foreach (string l in lineasVisibles)
                        textoActual += l + "\n";
                    textoActual += lineaActual;
                    textoSnackbar.text = textoActual;
                    textoSnackbar.ForceMeshUpdate();
                }

                if (panelRect != null)
                {
                    float alturaFinal = Mathf.Clamp(textoSnackbar.preferredHeight + 30f, alturaMinimaSnackbar, alturaMaximaSnackbar);
                    panelRect.offsetMax = new Vector2(-30, alturaFinal + 20);
                }

                yield return new WaitForSeconds(0.025f);
            }

            lineasVisibles.Add(lineaActual);
        }

        string textoFinal = "";
        foreach (string l in lineasVisibles)
            textoFinal += l + "\n";
        textoSnackbar.text = textoFinal;
        textoSnackbar.ForceMeshUpdate();

        if (panelRect != null)
        {
            float alturaFinal = Mathf.Clamp(textoSnackbar.preferredHeight + 30f, alturaMinimaSnackbar, alturaMaximaSnackbar);
            panelRect.offsetMax = new Vector2(-30, alturaFinal + 20);
        }
        yield return new WaitForSeconds(3f);

        CanvasGroup canvasGroup = panelSnackbar.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panelSnackbar.AddComponent<CanvasGroup>();

        float tiempo = 0f;
        while (tiempo < 0.3f)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = 1f - (tiempo / 0.3f);
            yield return null;
        }

        panelSnackbar.SetActive(false);
        canvasGroup.alpha = 1f;
    }

    private void MostrarPista(string mensaje)
    {
        if (snackbarCoroutine != null)
            StopCoroutine(snackbarCoroutine);
        ConfigurarSnackbar();
        snackbarCoroutine = StartCoroutine(MostrarSnackbar(mensaje));
    }

    public override void EntregarRetroalimentacion()
    {
        canvasJuego.gameObject.SetActive(false);
        canvasSeleccion.gameObject.SetActive(false);

        if (respuestaCorrectaDescartada)
        {
            ControladorSonido.Instance?.ReproducirLoss();
            textoResultado.text = "Has fallado";
            textoRespuesta.text = " ";
        }
        else if (ValidarRespuesta(patologiaSeleccionada))
        {
            ControladorSonido.Instance?.ReproducirWin();
            textoResultado.text = "Respuesta correcta";
            textoRespuesta.text = "La respuesta es: " + patologiaCorrecta;
        }
        else
        {
            ControladorSonido.Instance?.ReproducirLoss();
            textoResultado.text = "Respuesta incorrecta";
            textoRespuesta.text = " ";
        }

        Button btnContinuar = canvasRetroalimentacion.GetComponentInChildren<Button>();
        btnContinuar.onClick.RemoveAllListeners();
        btnContinuar.onClick.AddListener(() => {
            ControladorSonido.Instance?.ReproducirClick();
            finished = true;
        });
        canvasRetroalimentacion.gameObject.SetActive(true);
    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        idPatologia = indPatologiaAsignada;
        respuestaCorrectaDescartada = false;

        
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        patologiaCorrecta = p.nombre;

        ObtenerDiagnosticos();

        foreach (Button boton in diagnosticos)
        {
            Button btnactual = boton;
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => Seleccionar(btnactual));
        }

        if (btnDescartar != null)
        {
            btnDescartar.interactable = true;
        }
        VerificarOpcionesDisponibles();
    }
}