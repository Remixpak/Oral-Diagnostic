using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorSecuenciaNV2 : ControladorPreguntas
{
    [Header("Configuracion del Nivel Secuencia")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSeleccionado = Color.yellow;
    [SerializeField] private Button botonPausa;
    [SerializeField] private int idPatologiaCorrectaObjetivo = -1;


    [Header("Botones o Referencias Fijas para las Líneas Base")]
    [SerializeField] private Button botonLesionFijo;
    [SerializeField] private Button botonFamiliaFijo;
    [SerializeField] private Button botonEtiologiaFijo;

    [Header("Componentes de la UI - Bloque de Patologias a Seleccionar")]
    [SerializeField] private Button[] botonesPatologias;
    [SerializeField] private int[] idPatologiasBotones;

    [Header("Lineas Conectoras")]
    [SerializeField] private LineaConectora lineaConectora;

    [Header("Retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;
    [SerializeField] public TMP_Text textoRespuesta;

    private int targetLesionID;
    private int targetFamiliaID;
    private int targetEtiologiaID;
    private List<int> targetPatologiasIDs = new List<int>(); // cargamos las 3 patologias como alternativas 


    private List<int> indicesPatologiasSeleccionadas = new List<int>();
    private bool secuenciaCorrectaSeleccionada = false;

    private int erroresNivel = 0;

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

    void Update()
    {
        if (botonPausa != null && !botonPausa.interactable)
        {
            botonPausa.interactable = true;
        }
    }
    private void ConfigurarDatosAleatoriosSecuencia() // seleccionamos una patologia aleatoria y obtenemos sus ids de lesion, familia y etiologia
    {
        if (CsvManager.Instance == null || CsvManager.Instance.patologias == null || CsvManager.Instance.patologias.Count == 0) return;

        int indiceAleatorio = Random.Range(0, CsvManager.Instance.patologias.Count);
        Patologia patologiaBase = CsvManager.Instance.patologias[indiceAleatorio];

        targetLesionID = patologiaBase.lesionID;
        targetFamiliaID = patologiaBase.familiaID;
        targetEtiologiaID = patologiaBase.etiologiaID;

        targetPatologiasIDs.Clear();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.lesionID == targetLesionID && p.familiaID == targetFamiliaID && p.etiologiaID == targetEtiologiaID)
            {
                if (!targetPatologiasIDs.Contains(p.id))
                {
                    targetPatologiasIDs.Add(p.id);
                }
            }
        }

        if (targetPatologiasIDs.Count > 0)
        {
            idPatologiaCorrectaObjetivo = targetPatologiasIDs[0];
        }

        MostrarTextosFijos();
    }

    private void MostrarTextosFijos() //textos de los nombres de lesion, familia y etiologia en los botones fijos (cambian segun el id)
    {
        if (CsvManager.Instance == null) return;

        if (botonLesionFijo != null)
        {
            TextMeshProUGUI txt = botonLesionFijo.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                Lesion l = CsvManager.Instance.ObtenerLesionPorId(targetLesionID);
                string nombre = l != null ? l.nombre : "Lesión Desconocida";
                txt.text = "Lesión: " + nombre;
            }
        }

        if (botonFamiliaFijo != null)
        {
            TextMeshProUGUI txt = botonFamiliaFijo.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                Familia f = CsvManager.Instance.ObtenerFamiliaPorId(targetFamiliaID);
                string nombre = f != null ? f.nombre : "Familia Desconocida";
                txt.text = "Familia: " + nombre;
            }
        }

        if (botonEtiologiaFijo != null)
        {
            TextMeshProUGUI txt = botonEtiologiaFijo.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(targetEtiologiaID);
                string nombre = e != null ? e.nombre : "Etiopatogenia Desconocida";
                txt.text = "Etiopatogenia: " + nombre;
            }
        }
    }

    private void AsignarPatologiasABotones()
    {
        if (CsvManager.Instance == null || botonesPatologias == null) return;

        idPatologiasBotones = new int[botonesPatologias.Length];

        List<int> idsDisponiblesParaBotones = new List<int>();

        foreach (int idCorrecto in targetPatologiasIDs)
        {
            idsDisponiblesParaBotones.Add(idCorrecto);
        }

        while (idsDisponiblesParaBotones.Count < botonesPatologias.Length)
        {
            int idFalso = ObtenerIdPatologiaFalsa();
            if (!idsDisponiblesParaBotones.Contains(idFalso))
            {
                idsDisponiblesParaBotones.Add(idFalso);
            }
        }

        idsDisponiblesParaBotones = MezclarLista(idsDisponiblesParaBotones);

        for (int i = 0; i < botonesPatologias.Length; i++)
        {
            idPatologiasBotones[i] = idsDisponiblesParaBotones[i];
            ActualizarImagenBotonPatologia(i);
        }
    }

    private List<T> MezclarLista<T>(List<T> input)
    {
        List<T> listaMapeada = new List<T>(input);
        for (int i = 0; i < listaMapeada.Count; i++)
        {
            T temp = listaMapeada[i];
            int randomIndex = Random.Range(i, listaMapeada.Count);
            listaMapeada[i] = listaMapeada[randomIndex];
            listaMapeada[randomIndex] = temp;
        }
        return listaMapeada;
    }

    private int ObtenerIdPatologiaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (!targetPatologiasIDs.Contains(p.id) && !idsValidos.Contains(p.id))
            {
                idsValidos.Add(p.id);
            }
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

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

            TextMeshProUGUI txt = botonesPatologias[indice].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";
        }
    }

    private void ConfigurarInteractividadBotones() // configuramos los botones de patologias para que al hacer click se valide la seleccion
    {
        for (int i = 0; i < botonesPatologias.Length; i++)
        {
            int index = i;
            if (botonesPatologias[index] != null)
            {
                botonesPatologias[index].onClick.RemoveAllListeners();
                botonesPatologias[index].onClick.AddListener(() => ValidarSeleccionPatologia(index));
            }
        }
    }

    private void ValidarSeleccionPatologia(int indice)// validamos la seleccion de la patologia y actualizamos el color del boton seleccionado
    {

        ControladorSonido.Instance?.ReproducirClick(); 
        if (indicesPatologiasSeleccionadas.Count > 0 && indicesPatologiasSeleccionadas[0] == indice)
        {
            botonesPatologias[indice].GetComponent<Image>().color = colorNormal;
            indicesPatologiasSeleccionadas.Clear();
            secuenciaCorrectaSeleccionada = false;
            ActualizarLineasConexion();
            return;
        }
        if (indicesPatologiasSeleccionadas.Count > 0)
        {
            int indiceAnterior = indicesPatologiasSeleccionadas[0];
            if (indiceAnterior < botonesPatologias.Length && botonesPatologias[indiceAnterior] != null)
            {
                botonesPatologias[indiceAnterior].GetComponent<Image>().color = colorNormal;
            }
            indicesPatologiasSeleccionadas.Clear();
        }
        indicesPatologiasSeleccionadas.Add(indice);
        botonesPatologias[indice].GetComponent<Image>().color = colorSeleccionado;

        ActualizarLineasConexion();
        VerificarSecuenciaCompleta();
    }

    private void VerificarSecuenciaCompleta() // verificamos si la secuencia seleccionada es correcta y actualizamos los contadores de aciertos y fallos
    {
        //solo tenemos una respuesta correcta 
        bool esCorrecta = false;

        if (indicesPatologiasSeleccionadas.Count > 0)
        {
            int idx = indicesPatologiasSeleccionadas[0];
            int idSeleccionado = idPatologiasBotones[idx];
            esCorrecta = targetPatologiasIDs.Contains(idSeleccionado);
        }

        secuenciaCorrectaSeleccionada = esCorrecta;

        if (GameManager.Instance != null)
        {
            if (secuenciaCorrectaSeleccionada)
            {
                GameManager.Instance.Aciertos++;
                GameManager.Instance.TotalAciertos++;
            }
            else
            {
                GameManager.Instance.Fallos++;
                GameManager.Instance.TotalFallos++;
                GameManager.Instance.FFamilias++;
                erroresNivel++;
            }
        }

        PintarBotonesSeleccionados();
        ActualizarLineasConexion();
        VerificarProgresoNivel();
    }

    // dibumos lass lineas base  (Lesión -> Familia -> Etiología) 
    private void ActualizarLineasConexion()
    {
        if (lineaConectora == null) return;

        lineaConectora.LimpiarLineas();

        Color colorBase = lineaConectora.GetColorSeleccion();

        // lineas fijas Lesión -> Familia -> Etiología
        if (botonLesionFijo != null && botonFamiliaFijo != null)
        {
            lineaConectora.CrearLinea(botonLesionFijo, botonFamiliaFijo, colorBase);
        }
        if (botonFamiliaFijo != null && botonEtiologiaFijo != null)
        {
            lineaConectora.CrearLinea(botonFamiliaFijo, botonEtiologiaFijo, colorBase);
        }

        if (indicesPatologiasSeleccionadas.Count > 0)
        {
            int idx = indicesPatologiasSeleccionadas[0];
            Button botonPatologiaSeleccionada = botonesPatologias[idx];

            if (botonEtiologiaFijo != null && botonPatologiaSeleccionada != null)
            {
                lineaConectora.CrearLinea(botonEtiologiaFijo, botonPatologiaSeleccionada, colorBase);
            }
        }

        if (indicesPatologiasSeleccionadas.Count > 0 && (secuenciaCorrectaSeleccionada || erroresNivel > 0))
        {
            Color colorResultado = secuenciaCorrectaSeleccionada ? lineaConectora.GetColorCorrecta() : lineaConectora.GetColorIncorrecta();
            int ultimaLineaIndex = lineaConectora.CantidadLineas() - 1;
            if (ultimaLineaIndex >= 0)
            {
                lineaConectora.CambiarColorLinea(ultimaLineaIndex, colorResultado);
            }
        }
    }

    private void PintarBotonesSeleccionados()
    {
        foreach (int idx in indicesPatologiasSeleccionadas)
        {
            bool esAcertado = targetPatologiasIDs.Contains(idPatologiasBotones[idx]);
            botonesPatologias[idx].GetComponent<Image>().color = esAcertado ? Color.green : Color.red;
        }
    }

    public override void EntregarRetroalimentacion()
    {
        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(false);
        }

        if (secuenciaCorrectaSeleccionada)
        {
            ControladorSonido.Instance?.ReproducirWin();
            textoResultado.text = "¡Respuesta Correcta!";
            textoResultado.color = Color.white;
            textoRespuesta.text = "¡Has conectado los bloques de patologías correctamente!";
        }
        else
        {
            ControladorSonido.Instance?.ReproducirLoss();

            textoResultado.text = "Respuesta Incorrecta";
            textoResultado.color = Color.white;
            textoRespuesta.text = "Los bloques de patologías seleccionados no corresponden a la secuencia correcta.";
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

    private void VerificarProgresoNivel()
    {
        if (secuenciaCorrectaSeleccionada)
        {
            EntregarRetroalimentacion();
        }
        else
        {
            EntregarRetroalimentacion();
        }
    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(true);
        }

        erroresNivel = 0;
        indicesPatologiasSeleccionadas.Clear();
        secuenciaCorrectaSeleccionada = false;
        idPatologiaCorrectaObjetivo = -1;

        if (canvasRetroalimentacion != null)
        {
            canvasRetroalimentacion.gameObject.SetActive(false);
        }

        ConfigurarDatosAleatoriosSecuencia();
        AsignarPatologiasABotones();
        ConfigurarInteractividadBotones();


        ActualizarLineasConexion();
    }
}