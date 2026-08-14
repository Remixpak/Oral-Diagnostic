using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Gestiona la mecánica del minijuego o modo "Adivina quien", en el cual el jugador debe deducir la patología correcta
/// utilizando pistas (lesión, familia, etiopatogenia, descripciones), descartando opciones o seleccionando un diagnóstico final.
/// 
/// Clases de las que depende y su fin:
/// - ControladorPreguntas: Clase base de la que hereda para integrar la lógica general del flujo de preguntas.
/// - CsvManager: Instancia Singleton utilizada para obtener la información de las patologías, lesiones, familias, etiopatogenias, descripciones y sprites.
/// - GameManager: Instancia Singleton usada para registrar los aciertos, fallos y estadísticas globales del juego.
/// - ControladorSonido: Instancia Singleton utilizada para reproducir los efectos de sonido de interacciones (clicks, victoria, derrota).
/// - Patologia, Lesion, Familia, Etiologia, Descripcion: Clases/Estructuras de datos que representan los elementos médicos cargados desde el sistema.
/// </summary>
public class ControladorAdivina : ControladorPreguntas
{
    [Header("Botones")]
    [SerializeField] private List<Button> diagnosticos = new List<Button>();

    [Header("Datos")]
    [SerializeField] private string patologiaCorrecta;
    [SerializeField] private string patologiaSeleccionada;

    private int idPatologia;

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

    /// <summary>
    /// Inicializa las colecciones internas de la clase y suscribe los eventos necesarios en la interfaz del panel de selección.
    /// </summary>
    private void Awake()
    {
        patologiasOpciones = new List<Patologia>();

        if (cerrarSeleccion != null)
        {
            cerrarSeleccion.onClick.RemoveAllListeners();
            cerrarSeleccion.onClick.AddListener(CerrarPanelSeleccion);
        }
    }

    /// <summary>
    /// Cierra el panel de confirmación/selección de diagnóstico, reproduce un sonido de interacción y limpia el botón seleccionado.
    /// </summary>
    public void CerrarPanelSeleccion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasSeleccion.gameObject.SetActive(false);
        Seleccionado = null; 
    }

    /// <summary>
    /// Busca y retorna un componente Image en los componentes hijos del botón que no corresponda al objeto del botón en sí.
    /// </summary>
    /// <param name="btn">El botón sobre el cual se buscará la imagen hija.</param>
    /// <returns>El componente Image encontrado o null si no existe.</returns>
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

    /// <summary>
    /// Obtiene y mezcla la patología correcta junto con distractoras desde CsvManager, y asigna los nombres y sprites a los botones de diagnóstico.
    /// </summary>
    private void ObtenerDiagnosticos()
    {
        patologiasOpciones.Clear();

        Patologia correcta = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        patologiasOpciones.Add(correcta);

        List<Patologia> listaAux = new List<Patologia>(CsvManager.Instance.patologias);
        listaAux.RemoveAll(p => p.id == idPatologia || p.nombre == patologiaCorrecta);

        while (patologiasOpciones.Count < diagnosticos.Count && listaAux.Count > 0)
        {
            int indice = Random.Range(0, listaAux.Count);
            patologiasOpciones.Add(listaAux[indice]);
            listaAux.RemoveAt(indice);
        }

        for (int i = 0; i < patologiasOpciones.Count; i++)
        {
            int j = Random.Range(i, patologiasOpciones.Count);
            (patologiasOpciones[i], patologiasOpciones[j]) = (patologiasOpciones[j], patologiasOpciones[i]);
        }

        for (int i = 0; i < diagnosticos.Count; i++)
        {
            if (i >= patologiasOpciones.Count) break;

            Button btnActual = diagnosticos[i];
            Patologia patologiaActual = patologiasOpciones[i];

            TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
            {
                txt.text = patologiaActual.nombre;
                txt.gameObject.SetActive(true);
            }

            Image imgHija = ObtenerImagenHija(btnActual);
            if (imgHija != null)
            {
                Sprite spritePatologia = CsvManager.Instance.ObtenerSpriteDePatologia(patologiaActual);

                if (spritePatologia != null)
                {
                    imgHija.sprite = spritePatologia;
                    imgHija.gameObject.SetActive(true);
                }
                else
                {
                    imgHija.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Selecciona una alternativa de la lista de botones, activa la interfaz de confirmación e indica la patología elegida.
    /// </summary>
    /// <param name="boton">Botón de la alternativa seleccionada por el jugador.</param>
    public void Seleccionar(Button boton)
    {
        ControladorSonido.Instance?.ReproducirClick();
        Seleccionado = boton;
        textoDiag.text = boton.GetComponentInChildren<TMP_Text>().text;

        Debug.Log("Seleccionado: " + boton.name);

        canvasSeleccion.gameObject.SetActive(true);
    }

    /// <summary>
    /// Descarta el botón seleccionado actualmente. Si la patología descartada era la correcta, marca el fallo e inicia el fin de la pregunta.
    /// </summary>
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

    /// <summary>
    /// Confirma la selección del diagnóstico actual, valida si es correcto o incorrecto, actualiza las métricas en GameManager y finaliza la pregunta.
    /// </summary>
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

    /// <summary>
    /// Comprueba cuántas opciones quedan disponibles en pantalla y deshabilita el botón de descartar cuando solo resta la opción correcta.
    /// </summary>
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

    /// <summary>
    /// Corrutina que introduce un pequeño tiempo de espera antes de mostrar la retroalimentación de la pregunta.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(0.5f);
        EntregarRetroalimentacion();
    }

    /// <summary>
    /// Compara la respuesta entregada con la patología correcta de la pregunta actual.
    /// </summary>
    /// <param name="respuesta">Texto de la patología seleccionada.</param>
    /// <returns>True si coincide con la patología correcta; False en caso contrario.</returns>
    private bool ValidarRespuesta(string respuesta)
    {
        return respuesta == patologiaCorrecta;
    }

    /// <summary>
    /// Obtiene la lección asociada a la patología y muestra una pista visual en pantalla.
    /// </summary>
    public void PreguntarLesion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        string pista = $"La lesión es: {l.nombre}";
        MostrarPista(pista);
    }

    /// <summary>
    /// Obtiene la familia asociada a la patología y muestra una pista visual en pantalla.
    /// </summary>
    public void PreguntarFamilia()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Familia f = CsvManager.Instance.ObtenerFamiliaPorId(p.familiaID);
        string pista = $"La familia es: {f.nombre}";
        MostrarPista(pista);
    }

    /// <summary>
    /// Obtiene la etiopatogenia/etiología asociada a la patología y muestra una pista visual en pantalla.
    /// </summary>
    public void PreguntarEtiologia()
    {
        ControladorSonido.Instance?.ReproducirClick();
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(p.etiologiaID);
        string pista = $"La Etiopatogenia es: {e.nombre}";
        MostrarPista(pista);
    }

    /// <summary>
    /// Obtiene la lista de descripciones de la lesión asociada y las muestra como pista enumerada en el snackbar.
    /// </summary>
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

    /// <summary>
    /// Configura las propiedades visuales, de posición y límites de dimensión del panel de avisos/pistas (snackbar).
    /// </summary>
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

    /// <summary>
    /// Corrutina que anima la aparición progresiva texto por texto dentro del snackbar, ajusta su tamaño y luego lo oculta con un desvanecimiento.
    /// </summary>
    /// <param name="mensaje">Mensaje o pista a mostrar en el snackbar.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
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

    /// <summary>
    /// Detiene cualquier animación previa del snackbar, reconfigura sus parámetros y lanza la corrutina para desplegar una nueva pista.
    /// </summary>
    /// <param name="mensaje">Texto con la pista que se mostrará.</param>
    private void MostrarPista(string mensaje)
    {
        if (snackbarCoroutine != null)
            StopCoroutine(snackbarCoroutine);
        ConfigurarSnackbar();
        snackbarCoroutine = StartCoroutine(MostrarSnackbar(mensaje));
    }

    /// <summary>
    /// Desactiva las pantallas de juego, reproduce los efectos de audio correspondientes (victoria/derrota) y activa el panel de retroalimentación final.
    /// </summary>
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

    /// <summary>
    /// Prepara la pregunta para una nueva patología asignada, reinicia el estado de respuesta, obtiene las opciones de diagnóstico y configura sus botones.
    /// </summary>
    /// <param name="indPatologiaAsignada">Identificador de la patología asignada para esta pregunta.</param>
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