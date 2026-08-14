using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el nivel de preguntas NV2 basado en un árbol de decisiones para relacionar patología,
/// lesión, familia y etiología. Hereda de ControladorPreguntas y utiliza CsvManager para obtener
/// los datos del CSV, LineaConectora para representar las relaciones entre selecciones,
/// ControladorSonido para reproducir sonidos y GameManager para registrar los resultados del nivel.
/// También utiliza componentes de Unity UI y TextMeshPro para gestionar la interfaz.
/// </summary>
public class ControladorPreguntasNV2 : ControladorPreguntas
{
    [Header("Configuracion del Juego")]
    [SerializeField] private int patologiaIDTarget = 1; 
    [SerializeField] private string lesionSeleccionadaTexto = "Ninguna";
    [SerializeField] private string familiaSeleccionadaTexto = "Ninguna";
    [SerializeField] private string etiologiaSeleccionadaTexto = "Ninguna";
    [SerializeField] private Color colorNormal = Color.white; 
    [SerializeField] private Color colorSeleccionado = Color.yellow;
    [SerializeField] private Button botonPausa;

    [Header("Componentes de la UI - Bloque 1: Patologias (Imagenes)")]
    [SerializeField] private Button[] botonesPatologias;
    [SerializeField] private int[] idPatologiasBotones;

    [Header("Componentes de la UI - Bloque 2: Etiologias (Texto)")]
    [SerializeField] private Button[] botonesEtiologias;
    [SerializeField] private int[] idEtiologiasBotones;

    [Header("Componentes de la UI - Bloque 3: Familias (Texto)")]
    [SerializeField] private Button[] botonesFamilias;
    [SerializeField] private int[] idFamiliasBotones;

    [Header("Componentes de la UI - Bloque 4: Lesiones (Texto)")]
    [SerializeField] private Button[] botonesLesiones;
    [SerializeField] private int[] idLesionesBotones;

    [Header("Lineas Conectoras")]
    [SerializeField] private LineaConectora lineaConectora;

    [Header("Retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;
    [SerializeField] public TMP_Text textoRespuesta;

    private int targetLesionID; 
    private int targetFamiliaID;
    private int targetEtiologiaID;
    private int targetPatologiaID;

    private int indiceLesionSeleccionada = -1;
    private int indiceFamiliaSeleccionada = -1;
    private int indiceEtiologiaSeleccionada = -1;
    private int indicePatologiaSeleccionada = -1;

    private bool lesionCorrectaSeleccionada = false;
    private bool familiaCorrectaSeleccionada = false;
    private bool etiologiaCorrectaSeleccionada = false;
    private bool patologiaCorrectaSeleccionada = false;

    private int erroresNivel = 0;

    /// <summary>

    /// Inicializa el comportamiento del botón de pausa y deja preparado el controlador para recibir la pregunta desde el GameManager.

    /// </summary>

    void Start()
    {
        if (botonPausa != null)
        {
            botonPausa.onClick.RemoveAllListeners();
            botonPausa.onClick.AddListener(() => {
                Time.timeScale = Time.timeScale == 1 ? 0 : 1;
            });
            botonPausa.interactable = true;
        }

    }

    /// <summary>

    /// Mantiene habilitado el botón de pausa durante la ejecución del nivel.

    /// </summary>

    void Update()
    {
        if (botonPausa != null && !botonPausa.interactable)
        {
            botonPausa.interactable = true;
        }
    }

    /// <summary>

    /// Obtiene una patología aleatoria desde CsvManager y utiliza su ID como patología objetivo.

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

    /// Carga desde CsvManager los IDs de la patología, lesión, familia y etiología que forman la respuesta correcta.

    /// </summary>

    private void CargarDatosEstructuralesCSV()
    {
        if (CsvManager.Instance == null) return;

        Patologia patologiaActual = CsvManager.Instance.ObtenerPatologiaPorId(patologiaIDTarget);
        if (patologiaActual != null)
        {
            targetLesionID = patologiaActual.lesionID;
            targetFamiliaID = patologiaActual.familiaID;
            targetEtiologiaID = patologiaActual.etiologiaID;
            targetPatologiaID = patologiaActual.id;
        }
    }

    /// <summary>

    /// Distribuye aleatoriamente las respuestas correctas y falsas en los cuatro bloques de botones y actualiza su contenido visual.

    /// </summary>

    private void AsignarDatosAleatoriosABotones()
    {
        if (CsvManager.Instance == null) return;

        idPatologiasBotones = new int[botonesPatologias.Length];
        int indiceCorrectoPatologia = Random.Range(0, botonesPatologias.Length);
        idPatologiasBotones[indiceCorrectoPatologia] = targetPatologiaID;

        for (int i = 0; i < botonesPatologias.Length; i++)
        {
            if (i != indiceCorrectoPatologia)
            {
                idPatologiasBotones[i] = ObtenerIdPatologiaFalsa();
            }
            ActualizarImagenBotonPatologia(i);
        }

        idEtiologiasBotones = new int[botonesEtiologias.Length];
        int indiceCorrectoEtiologia = Random.Range(0, botonesEtiologias.Length);
        idEtiologiasBotones[indiceCorrectoEtiologia] = targetEtiologiaID;

        for (int i = 0; i < botonesEtiologias.Length; i++)
        {
            if (i != indiceCorrectoEtiologia)
            {
                idEtiologiasBotones[i] = ObtenerIdEtiologiaFalsa();
            }
            ActualizarTextoBotonEtiologia(i);
        }

        idFamiliasBotones = new int[botonesFamilias.Length];
        int indiceCorrectoFamilia = Random.Range(0, botonesFamilias.Length);
        idFamiliasBotones[indiceCorrectoFamilia] = targetFamiliaID;

        for (int i = 0; i < botonesFamilias.Length; i++)
        {
            if (i != indiceCorrectoFamilia)
            {
                idFamiliasBotones[i] = ObtenerIdFamiliaFalsa();
            }
            ActualizarTextoBotonFamilia(i);
        }

        idLesionesBotones = new int[botonesLesiones.Length];
        int indiceCorrectoLesion = Random.Range(0, botonesLesiones.Length);
        idLesionesBotones[indiceCorrectoLesion] = targetLesionID;

        for (int i = 0; i < botonesLesiones.Length; i++)
        {
            if (i != indiceCorrectoLesion)
            {
                idLesionesBotones[i] = ObtenerIdLesionFalsa();
            }
            ActualizarTextoBotonLesion(i);
        }
    }

    /// <summary>

    /// Obtiene un ID de lesión diferente al ID de la lesión correcta.

    /// </summary>

    private int ObtenerIdLesionFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.lesionID != targetLesionID && !idsValidos.Contains(p.lesionID))
                idsValidos.Add(p.lesionID);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

    /// <summary>

    /// Obtiene un ID de familia diferente al ID de la familia correcta.

    /// </summary>

    private int ObtenerIdFamiliaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.familiaID != targetFamiliaID && !idsValidos.Contains(p.familiaID))
                idsValidos.Add(p.familiaID);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

    /// <summary>

    /// Obtiene un ID de etiología diferente al ID de la etiología correcta.

    /// </summary>

    private int ObtenerIdEtiologiaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.etiologiaID != targetEtiologiaID && !idsValidos.Contains(p.etiologiaID))
                idsValidos.Add(p.etiologiaID);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

    /// <summary>

    /// Obtiene un ID de patología diferente al ID de la patología correcta.

    /// </summary>

    private int ObtenerIdPatologiaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.id != targetPatologiaID) idsValidos.Add(p.id);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

    /// <summary>

    /// Actualiza el texto de un botón de lesión utilizando el ID asignado y los datos de CsvManager.

    /// </summary>

    private void ActualizarTextoBotonLesion(int indice)
    {
        TextMeshProUGUI txt = botonesLesiones[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Lesion l = CsvManager.Instance.ObtenerLesionPorId(idLesionesBotones[indice]);
            txt.text = "Lesión: " + (l != null ? l.nombre : "Desconocida");
        }
    }

    /// <summary>

    /// Actualiza el texto de un botón de familia utilizando el ID asignado y los datos de CsvManager.

    /// </summary>

    private void ActualizarTextoBotonFamilia(int indice)
    {
        TextMeshProUGUI txt = botonesFamilias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Familia f = CsvManager.Instance.ObtenerFamiliaPorId(idFamiliasBotones[indice]);
            txt.text = "Familia: " + (f != null ? f.nombre : "Desconocida");
        }
    }

    /// <summary>

    /// Actualiza el texto de un botón de etiología utilizando el ID asignado y los datos de CsvManager.

    /// </summary>

    private void ActualizarTextoBotonEtiologia(int indice)
    {
        TextMeshProUGUI txt = botonesEtiologias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(idEtiologiasBotones[indice]);
            txt.text = "Etiopatogenia: " + (e != null ? e.nombre : "Desconocida");
        }
    }

    /// <summary>

    /// Actualiza la imagen de un botón de patología utilizando el código de imagen asociado en CsvManager.

    /// </summary>

    private void ActualizarImagenBotonPatologia(int indice)
    {
        Patologia patologiaActual = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiasBotones[indice]);
        if (patologiaActual != null)
        {
            string nombreImagenLimpio = patologiaActual.codigoImagen.Trim().Replace("\r", "").Replace("\n", "");

            Sprite img = CsvManager.Instance.spritePorCodigo(nombreImagenLimpio);

            Image botonImg = botonesPatologias[indice].GetComponent<Image>();
            if (botonImg != null && img != null)
            {
                botonImg.sprite = img;
            }
            else if (img == null)
            {
                
            }

            TextMeshProUGUI txt = botonesPatologias[indice].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";
        }
    }

    /// <summary>

    /// Asigna los eventos de clic de los botones para validar las selecciones de patología, lesión, familia y etiología.

    /// </summary>

    private void ConfigurarInteractividadArbol()
    {
        for (int i = 0; i < botonesPatologias.Length; i++)
        {
            int index = i;
            if (botonesPatologias[index] != null)
            {
                botonesPatologias[index].onClick.AddListener(() => ValidarSeleccionPatologia(index));
            }
        }

        for (int i = 0; i < botonesLesiones.Length; i++)
        {
            int index = i;
            if (botonesLesiones[index] != null)
            {
                botonesLesiones[index].onClick.AddListener(() => ValidarSeleccionLesion(index));
            }
        }

        for (int i = 0; i < botonesFamilias.Length; i++)
        {
            int index = i;
            if (botonesFamilias[index] != null)
            {
                botonesFamilias[index].onClick.AddListener(() => ValidarSeleccionFamilia(index));
            }
        }

        for (int i = 0; i < botonesEtiologias.Length; i++)
        {
            int index = i;
            if (botonesEtiologias[index] != null)
            {
                botonesEtiologias[index].onClick.AddListener(() => ValidarSeleccionEtiologia(index));
            }
        }
    }

    /// <summary>

    /// Controla qué bloques de botones pueden interactuarse según el progreso de las selecciones del usuario.

    /// </summary>

    private void ActualizarInteractividadBloques()
    {
        SetBloqueInteractable(botonesLesiones, indicePatologiaSeleccionada != -1);
        SetBloqueInteractable(botonesFamilias, indicePatologiaSeleccionada != -1 && indiceLesionSeleccionada != -1);
        SetBloqueInteractable(botonesEtiologias, indicePatologiaSeleccionada != -1 && indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1);
    }

    /// <summary>

    /// Establece la interactividad de un bloque de botones y restaura el color de los botones cuando el bloque está deshabilitado.

    /// </summary>

    private void SetBloqueInteractable(Button[] bloque, bool estado)
    {
        for (int i = 0; i < bloque.Length; i++)
        {
            if (bloque[i] != null)
            {
                if (bloque[i] == botonPausa) continue;

                bloque[i].interactable = estado;
                if (!estado)
                {
                    bloque[i].GetComponent<Image>().color = colorNormal;
                }
            }
        }
    }

    /// <summary>

    /// Actualiza las líneas conectoras entre las selecciones y determina sus colores según si cada relación es correcta o incorrecta.

    /// </summary>

    private void ActualizarLineas()
    {
        if (lineaConectora == null) return;

        lineaConectora.LimpiarLineas();

        if (indicePatologiaSeleccionada != -1 && indiceLesionSeleccionada != -1)
        {
            Button origen = botonesPatologias[indicePatologiaSeleccionada];
            Button destino = botonesLesiones[indiceLesionSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        if (indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1)
        {
            Button origen = botonesLesiones[indiceLesionSeleccionada];
            Button destino = botonesFamilias[indiceFamiliaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        if (indiceFamiliaSeleccionada != -1 && indiceEtiologiaSeleccionada != -1)
        {
            Button origen = botonesFamilias[indiceFamiliaSeleccionada];
            Button destino = botonesEtiologias[indiceEtiologiaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        if (lineaConectora.HayLineas() &&
            indicePatologiaSeleccionada != -1 &&
            indiceLesionSeleccionada != -1 &&
            indiceFamiliaSeleccionada != -1 &&
            indiceEtiologiaSeleccionada != -1)
        {
            int idx = 0;

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (patologiaCorrectaSeleccionada && lesionCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
                idx++;
            }

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (lesionCorrectaSeleccionada && familiaCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
                idx++;
            }

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (familiaCorrectaSeleccionada && etiologiaCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
            }

            if (!patologiaCorrectaSeleccionada || !lesionCorrectaSeleccionada ||
                !familiaCorrectaSeleccionada || !etiologiaCorrectaSeleccionada)
            {
                if (!finished)
                {
                    EntregarRetroalimentacion();
                }
            }
        }
    }
    
    /// <summary>
    
    /// Limpia las selecciones de lesión, familia y etiología posteriores a una selección de patología.
    
    /// </summary>
    
    private void LimpiarSeleccionesPosterioresAPatologia()
    {
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    /// <summary>

    /// Limpia las selecciones de familia y etiología posteriores a una selección de lesión.

    /// </summary>

    private void LimpiarSeleccionesPosterioresALesion()
    {
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    /// <summary>

    /// Limpia la selección de etiología posterior a una selección de familia.

    /// </summary>

    private void LimpiarSeleccionesPosterioresAFamilia()
    {
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    /// <summary>

    /// Restablece los colores de los cuatro bloques de botones de acuerdo con las selecciones actuales.

    /// </summary>

    private void RestablecerColoresTodosLosBloques()
    {
        RestablecerColorBloque(botonesPatologias, indicePatologiaSeleccionada);
        RestablecerColorBloque(botonesLesiones, indiceLesionSeleccionada);
        RestablecerColorBloque(botonesFamilias, indiceFamiliaSeleccionada);
        RestablecerColorBloque(botonesEtiologias, indiceEtiologiaSeleccionada);
    }

    /// <summary>

    /// Valida y actualiza la selección de una lesión, incluyendo su estado correcto, colores, interactividad y progreso.

    /// </summary>

    private void ValidarSeleccionLesion(int indice)
    {
        ControladorSonido.Instance?.ReproducirClick(); 

        if (indiceLesionSeleccionada == indice)
        {
            botonesLesiones[indice].GetComponent<Image>().color = colorNormal;
            indiceLesionSeleccionada = -1;
            lesionCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresALesion();
            ActualizarInteractividadBloques();
            ActualizarLineas();
            return;
        }

        indiceLesionSeleccionada = indice;
        lesionCorrectaSeleccionada = (indice < idLesionesBotones.Length && idLesionesBotones[indice] == targetLesionID);

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
        VerificarProgresoArbol();
    }

    /// <summary>

    /// Valida y actualiza la selección de una familia, incluyendo su estado correcto, colores e interactividad.

    /// </summary>

    private void ValidarSeleccionFamilia(int indice)
    {
        ControladorSonido.Instance?.ReproducirClick();

        if (indiceFamiliaSeleccionada == indice)
        {
            botonesFamilias[indice].GetComponent<Image>().color = colorNormal;
            indiceFamiliaSeleccionada = -1;
            familiaCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresAFamilia();
            ActualizarInteractividadBloques();
            ActualizarLineas();
            return;
        }

        indiceFamiliaSeleccionada = indice;
        familiaCorrectaSeleccionada = (indice < idFamiliasBotones.Length && idFamiliasBotones[indice] == targetFamiliaID);

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    /// <summary>

    /// Valida y actualiza la selección de una etiología, incluyendo su estado correcto, colores, líneas, interactividad y progreso.

    /// </summary>

    private void ValidarSeleccionEtiologia(int indice)
    {
        ControladorSonido.Instance?.ReproducirClick();

        if (indiceEtiologiaSeleccionada == indice)
        {
            botonesEtiologias[indice].GetComponent<Image>().color = colorNormal;
            indiceEtiologiaSeleccionada = -1;
            etiologiaCorrectaSeleccionada = false;
            RestablecerColoresTodosLosBloques();
            ActualizarLineas();
            ActualizarInteractividadBloques();
            return;
        }

        indiceEtiologiaSeleccionada = indice;
        etiologiaCorrectaSeleccionada = (indice < idEtiologiasBotones.Length && idEtiologiasBotones[indice] == targetEtiologiaID);

        RestablecerColoresTodosLosBloques();
        PintarCaminoFinal();
        ActualizarLineas();
        ActualizarInteractividadBloques();
        VerificarProgresoArbol();
    }
    /// <summary>
    /// Valida y actualiza la selección de una patología, incluyendo su estado correcto, colores, líneas e interactividad.
    /// </summary>
    private void ValidarSeleccionPatologia(int indice)
    {
        ControladorSonido.Instance?.ReproducirClick();

        if (indicePatologiaSeleccionada == indice)
        {
            botonesPatologias[indice].GetComponent<Image>().color = colorNormal;
            indicePatologiaSeleccionada = -1;
            patologiaCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresAPatologia();
            ActualizarInteractividadBloques();
            ActualizarLineas();
            return;
        }

        indicePatologiaSeleccionada = indice;
        patologiaCorrectaSeleccionada = (indice < idPatologiasBotones.Length && idPatologiasBotones[indice] == targetPatologiaID);

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    /// <summary>

    /// Pinta las selecciones realizadas con verde o rojo según si cada respuesta es correcta o incorrecta.

    /// </summary>

    private void PintarCaminoFinal()
    {
        if (indicePatologiaSeleccionada != -1)
        {
            botonesPatologias[indicePatologiaSeleccionada].GetComponent<Image>().color = patologiaCorrectaSeleccionada ? Color.green : Color.red;
        }

        if (indiceLesionSeleccionada != -1)
        {
            botonesLesiones[indiceLesionSeleccionada].GetComponent<Image>().color = lesionCorrectaSeleccionada ? Color.green : Color.red;
        }

        if (indiceFamiliaSeleccionada != -1)
        {
            botonesFamilias[indiceFamiliaSeleccionada].GetComponent<Image>().color = familiaCorrectaSeleccionada ? Color.green : Color.red;
        }

        if (indiceEtiologiaSeleccionada != -1)
        {
            botonesEtiologias[indiceEtiologiaSeleccionada].GetComponent<Image>().color = etiologiaCorrectaSeleccionada ? Color.green : Color.red;
        }
    }

    /// <summary>

    /// Restablece el color de cada botón de un bloque y destaca el botón actualmente seleccionado.

    /// </summary>

    private void RestablecerColorBloque(Button[] bloque, int indiceSeleccionado)
    {
        for (int i = 0; i < bloque.Length; i++)
        {
            if (bloque[i] != null)
            {
                if (i == indiceSeleccionado)
                {
                    bloque[i].GetComponent<Image>().color = colorSeleccionado;
                }
                else
                {
                    bloque[i].GetComponent<Image>().color = colorNormal;
                }
            }
        }
    }

    /// <summary>

    /// Muestra la retroalimentación final del nivel, actualiza las métricas del GameManager y configura el botón para continuar.

    /// </summary>

    public override void EntregarRetroalimentacion()
    {
        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(false);
        }

        if (patologiaCorrectaSeleccionada && lesionCorrectaSeleccionada &&
            familiaCorrectaSeleccionada && etiologiaCorrectaSeleccionada)
        {
            ControladorSonido.Instance?.ReproducirWin();
            textoResultado.text = "¡Respuesta Correcta!";
            textoResultado.color = Color.white;
            textoRespuesta.text = "¡Todos los bloques son correctos!";

            if(GameManager.Instance != null)
            {
                GameManager.Instance.Aciertos++;
                GameManager.Instance.TotalAciertos++;
            }
        }
        else
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance.Fallos++;
                GameManager.Instance.TotalFallos++;
            }
            ControladorSonido.Instance?.ReproducirLoss(); 
            textoResultado.text = "Respuesta Incorrecta";
            textoResultado.color = Color.white;

            string erroresTexto = "";
            if (!patologiaCorrectaSeleccionada) erroresTexto += "- Patologia incorrecta\n";
            if (!lesionCorrectaSeleccionada) erroresTexto += "- Lesion incorrecta\n";
            if (!familiaCorrectaSeleccionada) erroresTexto += "- Familia incorrecta\n";
            if (!etiologiaCorrectaSeleccionada) erroresTexto += "- Etiologia incorrecta\n";
            textoRespuesta.text = " ";
        }

        if (canvasRetroalimentacion != null)
        {
            Button continuarBtn = canvasRetroalimentacion.GetComponentInChildren<Button>();
            if (continuarBtn != null)
            {
                continuarBtn.onClick.RemoveAllListeners();
                continuarBtn.onClick.AddListener(() => {
                    ControladorSonido.Instance?.ReproducirClick(); 
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

    /// Comprueba si las cuatro selecciones son correctas y entrega la retroalimentación cuando el árbol está completo.

    /// </summary>

    private void VerificarProgresoArbol()
    {
        if (patologiaCorrectaSeleccionada && lesionCorrectaSeleccionada &&
            familiaCorrectaSeleccionada && etiologiaCorrectaSeleccionada)
        {
            EntregarRetroalimentacion();
        }
        else
        {
            finished = false;
        }
    }

    /// <summary>

    /// Inicializa una nueva pregunta, restablece las selecciones y estados, carga los datos asociados y configura nuevamente los botones del árbol.

    /// </summary>

    public override void InicializarPregunta(int indPatologiaAsignada)
    {

        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(true);
        }

        patologiaIDTarget = indPatologiaAsignada;
        erroresNivel = 0;

        indiceLesionSeleccionada = -1;
        indiceFamiliaSeleccionada = -1;
        indiceEtiologiaSeleccionada = -1;
        indicePatologiaSeleccionada = -1;

        lesionCorrectaSeleccionada = false;
        familiaCorrectaSeleccionada = false;
        etiologiaCorrectaSeleccionada = false;
        patologiaCorrectaSeleccionada = false;

        if (canvasRetroalimentacion != null)
        {
            canvasRetroalimentacion.gameObject.SetActive(false);
        }
        
        if (CsvManager.Instance != null)
        {
            Patologia patologia = CsvManager.Instance.ObtenerPatologiaPorId(patologiaIDTarget);
            if (patologia != null)
            {
                Lesion l = CsvManager.Instance.ObtenerLesionPorId(patologia.lesionID);
                lesionSeleccionadaTexto = (l != null) ? l.nombre : "Desconocida";

                Familia f = CsvManager.Instance.ObtenerFamiliaPorId(patologia.familiaID);
                familiaSeleccionadaTexto = (f != null) ? f.nombre : "Desconocida";

                Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(patologia.etiologiaID);
                etiologiaSeleccionadaTexto = (e != null) ? e.nombre : "Desconocida";
            }
            else
            {
                lesionSeleccionadaTexto = "Ninguna";
                familiaSeleccionadaTexto = "Ninguna";
                etiologiaSeleccionadaTexto = "Ninguna";
            }
        }

        CargarDatosEstructuralesCSV();
        AsignarDatosAleatoriosABotones();
        ConfigurarInteractividadArbol();
        ActualizarInteractividadBloques();

        if (lineaConectora != null)
        {
            lineaConectora.LimpiarLineas();
        }
    }
}