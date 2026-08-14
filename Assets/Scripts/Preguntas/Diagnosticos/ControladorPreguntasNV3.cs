using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de preguntas para el Nivel 3. Implementa una mecánica de juego estilo "Ahorcado" o sopa de letras
/// donde el usuario debe formar el nombre de la patología objetivo mediante un teclado virtual en pantalla.
/// 
/// Clases dependientes que utiliza:
/// - ControladorPreguntas: Clase base heredada que define la estructura general de control de preguntas y estado de finalización.
/// - CsvManager: Provee la carga de datos de patologías, lesiones, familias, etiologías y sprites de imágenes asociadas.
/// - GameManager: Registra las estadísticas globales del jugador (aciertos, fallos y fallos de diagnóstico).
/// - Patologia / Lesion / Familia / Etiologia: Modelos de datos para estructurar la información médica obtenida del CSV.
/// </summary>
public class ControladorPreguntasNV3 : ControladorPreguntas
{
    [Header("Pausa")]
    [SerializeField] private Button botonPausa;

    [Header("Configuración del Juego")]
    [SerializeField] private int patologiaIDTarget = 1;
    [SerializeField] private string palabraCorrecta = "ULCERA";
    [SerializeField] private int cantidadLetrasTeclado = 12;

    [Header("UI Contenedores")]
    [SerializeField] private Transform containerEspacios;
    [SerializeField] private Transform containerTeclado;
    [SerializeField] private GameObject prefabBotonLetra;

    [Header("Pistas clínicas")]
    [SerializeField] private Image uiImagePista;
    [SerializeField] private TextMeshProUGUI uiTextoLesion;
    [SerializeField] private TextMeshProUGUI uiTextoFamilia;
    [SerializeField] private TextMeshProUGUI uiTextoEtiopatogenia;

    [Header("Datos de la pista")]
    [SerializeField] private Sprite imagenPistaSprite;
    [SerializeField] private string NombreLesion = "Lesión Primaria";
    [SerializeField] private string NombreFamilia = "Dermatológica";
    [SerializeField] private string DescripcionEtiopatogenia = "Pérdida de continuidad de la piel";

    [Header("Retroalimentación")]
    [SerializeField] public TMP_Text textoResultado;
    [SerializeField] public TMP_Text textoRespuesta;

    private List<string> letrasTeclado = new List<string>();
    private string[] progresoUsuario;

    private List<Button> botonesEspaciosUI = new List<Button>();
    private List<Button> botonesTecladoUI = new List<Button>();

    private bool nivelCompletado = false;
    private int erroresNivel = 0;

    void Start()
    {

    }

    /// <summary>
    /// Selecciona aleatoriamente una patología de la base de datos CSV y actualiza el ID objetivo del nivel.
    /// </summary>
    private void ObtenerPatologiaAleatoria()
    {
        if (CsvManager.Instance != null && CsvManager.Instance.patologias != null && CsvManager.Instance.patologias.Count > 0)
        {
            int indice = Random.Range(0, CsvManager.Instance.patologias.Count);
            Patologia patologia = CsvManager.Instance.patologias[indice];
            patologiaIDTarget = patologia.id;
        }
    }

    /// <summary>
    /// Consulta al CsvManager para obtener los datos detallados de la patología actual (nombre, lesión, familia, etiología e imagen).
    /// </summary>
    private void CargarDatosDesdeCSV()
    {
        if (CsvManager.Instance == null) return;

        Patologia patologiaActual = CsvManager.Instance.ObtenerPatologiaPorId(patologiaIDTarget);
        if (patologiaActual != null)
        {
            palabraCorrecta = patologiaActual.nombre.ToUpper().Trim();

            Lesion lesion = CsvManager.Instance.ObtenerLesionPorId(patologiaActual.lesionID);
            if (lesion != null) NombreLesion = lesion.nombre;

            Familia familia = CsvManager.Instance.ObtenerFamiliaPorId(patologiaActual.familiaID);
            if (familia != null) NombreFamilia = familia.nombre;

            Etiologia etiologia = CsvManager.Instance.ObtenerEtiologiaPorId(patologiaActual.etiologiaID);
            if (etiologia != null) DescripcionEtiopatogenia = etiologia.nombre;

            if (!string.IsNullOrEmpty(patologiaActual.codigoImagen))
            {
                string nombreImagenLimpio = patologiaActual.codigoImagen.Trim().Replace("\r", "").Replace("\n", "");

                Sprite spriteCargado = CsvManager.Instance.spritePorCodigo(nombreImagenLimpio);
                if (spriteCargado != null)
                {
                    imagenPistaSprite = spriteCargado;
                }
                else
                {
                    Debug.LogError("No se encontró la imagen en: Assets/Resources/Imagenes/" + nombreImagenLimpio);
                }
            }
        }
    }

    /// <summary>
    /// Asigna los datos médicos cargados a los componentes visuales del panel de pistas clínicas.
    /// </summary>
    private void ConfigurarPanelPistas()
    {
        if (uiImagePista != null && imagenPistaSprite != null) uiImagePista.sprite = imagenPistaSprite;
        if (uiTextoLesion != null) uiTextoLesion.text = "Lesión: " + NombreLesion;
        if (uiTextoFamilia != null) uiTextoFamilia.text = "Familia: " + NombreFamilia;
        if (uiTextoEtiopatogenia != null) uiTextoEtiopatogenia.text = "Etiopatogenia: " + DescripcionEtiopatogenia;
    }

    /// <summary>
    /// Genera la lista de letras del teclado combinando las letras de la palabra objetivo con letras aleatorias hasta completar la cantidad especificada.
    /// </summary>
    private void GenerarLetrasTeclado()
    {
        for (int i = 0; i < palabraCorrecta.Length; i++)
        {
            if (palabraCorrecta[i] == ' ') continue;
            letrasTeclado.Add(palabraCorrecta[i].ToString());
        }

        string abecedario = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        while (letrasTeclado.Count < cantidadLetrasTeclado)
        {
            string letraAleatoria = abecedario[Random.Range(0, abecedario.Length)].ToString();
            letrasTeclado.Add(letraAleatoria);
        }

        for (int i = 0; i < letrasTeclado.Count; i++)
        {
            string temp = letrasTeclado[i];
            int randomIndex = Random.Range(i, letrasTeclado.Count);
            letrasTeclado[i] = letrasTeclado[randomIndex];
            letrasTeclado[randomIndex] = temp;
        }
    }

    /// <summary>
    /// Instancia dinámicamente los contenedores y botones de espacio en blanco correspondientes a la palabra objetivo.
    /// </summary>
    private void CrearEspaciosPalabra()
    {
        string[] palabras = palabraCorrecta.Split(' ');
        int letraGlobalIndex = 0;

        for (int w = 0; w < palabras.Length; w++)
        {
            string palabraActual = palabras[w];

            GameObject subContenedor = new GameObject("SubContainer_" + palabraActual, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            subContenedor.transform.SetParent(containerEspacios, false);

            HorizontalLayoutGroup layoutGroup = subContenedor.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 8f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = subContenedor.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < palabraActual.Length; i++)
            {
                int index = letraGlobalIndex;
                GameObject nuevoBoton = Instantiate(prefabBotonLetra, subContenedor.transform);
                Button btn = nuevoBoton.GetComponent<Button>();

                btn.GetComponentInChildren<TextMeshProUGUI>().text = "";
                btn.onClick.AddListener(() => RemoverLetraDeEspacio(index));

                botonesEspaciosUI.Add(btn);
                letraGlobalIndex++;
            }

            if (w < palabras.Length - 1)
            {
                int indexEspacio = letraGlobalIndex;
                progresoUsuario[indexEspacio] = " ";

                GameObject espacioInvis = new GameObject("EspacioSeparador", typeof(RectTransform));
                espacioInvis.transform.SetParent(subContenedor.transform, false);

                GameObject btnEspacioFake = Instantiate(prefabBotonLetra, containerEspacios);
                btnEspacioFake.SetActive(false);
                botonesEspaciosUI.Add(btnEspacioFake.GetComponent<Button>());

                letraGlobalIndex++;
            }
        }
    }

    /// <summary>
    /// Instancia los botones correspondientes a las letras disponibles en el contenedor del teclado virtual.
    /// </summary>
    private void CrearTeclado()
    {
        for (int i = 0; i < letrasTeclado.Count; i++)
        {
            int index = i;
            GameObject nuevoBoton = Instantiate(prefabBotonLetra, containerTeclado);
            Button btn = nuevoBoton.GetComponent<Button>();

            string letra = letrasTeclado[index];
            btn.GetComponentInChildren<TextMeshProUGUI>().text = letra;

            btn.onClick.AddListener(() => SeleccionarLetraTeclado(index, letra));

            botonesTecladoUI.Add(btn);
        }
    }

    /// <summary>
    /// Maneja la pulsación de un botón del teclado virtual, posicionando la letra en la primera casilla vacía disponible.
    /// </summary>
    /// <param name="indiceTeclado">Índice del botón presionado en el teclado.</param>
    /// <param name="letra">Carácter correspondiente a la tecla.</param>
    private void SeleccionarLetraTeclado(int indiceTeclado, string letra)
    {
        if (nivelCompletado) return;

        for (int i = 0; i < progresoUsuario.Length; i++)
        {
            if (string.IsNullOrEmpty(progresoUsuario[i]))
            {
                progresoUsuario[i] = letra;

                botonesEspaciosUI[i].GetComponentInChildren<TextMeshProUGUI>().text = letra;

                botonesTecladoUI[indiceTeclado].gameObject.SetActive(false);

                botonesEspaciosUI[i].name = indiceTeclado.ToString();

                ComprobarResultado();
                break;
            }
        }
    }

    /// <summary>
    /// Quita la letra seleccionada de una casilla objetivo y reactiva la tecla correspondiente en el teclado.
    /// </summary>
    /// <param name="indiceEspacio">Índice del espacio de respuesta que se desea limpiar.</param>
    private void RemoverLetraDeEspacio(int indiceEspacio)
    {
        if (nivelCompletado) return;
        if (!string.IsNullOrEmpty(progresoUsuario[indiceEspacio]))
        {
            if (int.TryParse(botonesEspaciosUI[indiceEspacio].name, out int indiceTecladoOriginal))
            {
                botonesTecladoUI[indiceTecladoOriginal].gameObject.SetActive(true);
            }

            progresoUsuario[indiceEspacio] = null;

            botonesEspaciosUI[indiceEspacio].GetComponentInChildren<TextMeshProUGUI>().text = "";
            botonesEspaciosUI[indiceEspacio].name = "Espacio";
            botonesEspaciosUI[indiceEspacio].GetComponent<Image>().color = Color.white;
        }
    }

    /// <summary>
    /// Valida si el usuario ha completado todas las letras del progreso y verifica si la palabra formada coincide con la correcta.
    /// </summary>
    private void ComprobarResultado()
    {
        if (nivelCompletado) return;

        string palabraFormada = "";
        for (int i = 0; i < progresoUsuario.Length; i++)
        {
            if (string.IsNullOrEmpty(progresoUsuario[i])) return;
            palabraFormada += progresoUsuario[i];
        }

        if (palabraFormada == palabraCorrecta)
        {
            nivelCompletado = true;
            Debug.Log("<color=green>¡Correcto! Has descubierto el diagnóstico clínico.</color>");
            foreach (Button btn in botonesEspaciosUI)
            {
                btn.GetComponent<Image>().color = Color.green;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Aciertos++;
                GameManager.Instance.TotalAciertos++;
            }

            EntregarRetroalimentacion();
        }
        else
        {
            erroresNivel++;
            Debug.Log("<color=red>Palabra incorrecta. Sigue intentando.</color>");
            foreach (Button btn in botonesEspaciosUI)
            {
                btn.GetComponent<Image>().color = Color.red;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Fallos++;
                GameManager.Instance.TotalFallos++;
                GameManager.Instance.FDiagnosticos++;
            }

            StartCoroutine(RestaurarColoresEspacios());
        }
    }

    /// <summary>
    /// Corrutina que restablece el color original blanco en los casilleros tras un intento incorrecto.
    /// </summary>
    private IEnumerator RestaurarColoresEspacios()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (Button btn in botonesEspaciosUI)
        {
            if (btn != null && !nivelCompletado)
            {
                btn.GetComponent<Image>().color = Color.white;
            }
        }
    }

    /// <summary>
    /// Muestra el lienzo de retroalimentación en pantalla con el resultado final (éxito o fracaso) y deshabilita la pausa.
    /// </summary>
    public override void EntregarRetroalimentacion()
    {
        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(false);
        }

        if (nivelCompletado)
        {
            textoResultado.text = "¡Respuesta Correcta!";
            textoResultado.color = Color.white;
            textoRespuesta.text = "Has descubierto el diagnóstico clínico: " + palabraCorrecta;
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
            textoResultado.color = Color.white;
            textoRespuesta.text = " ";
        }

        if (canvasRetroalimentacion != null)
        {
            Button continuarBtn = canvasRetroalimentacion.GetComponentInChildren<Button>();
            if (continuarBtn != null)
            {
                continuarBtn.onClick.RemoveAllListeners();
                continuarBtn.onClick.AddListener(() => {
                    finished = true;
                });
            }
            canvasRetroalimentacion.gameObject.SetActive(true);
        }
        else
        {
            finished = true;
        }
    }

    /// <summary>
    /// Inicializa y resetea los elementos del Nivel 3 para una nueva pregunta con base en la patología indicada.
    /// </summary>
    /// <param name="indPatologiaAsignada">Identificador de la patología a cargar.</param>
    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(true);
        }

        patologiaIDTarget = indPatologiaAsignada;
        nivelCompletado = false;
        erroresNivel = 0;
        letrasTeclado.Clear();

        if (canvasRetroalimentacion != null)
        {
            canvasRetroalimentacion.gameObject.SetActive(false);
        }

        CargarDatosDesdeCSV();
        progresoUsuario = new string[palabraCorrecta.Length];
        ConfigurarPanelPistas();
        GenerarLetrasTeclado();
        CrearEspaciosPalabra();
        CrearTeclado();
    }
}