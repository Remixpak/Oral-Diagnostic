using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ControladorPreguntasNV3 : ControladorPreguntas
{
    [Header("Pausa")]
    [SerializeField]private Button botonPausa;

    [Header("Configuraci�n del Juego")]

    [SerializeField] private int patologiaIDTarget = 1; //ID de la patologia que se quiere mostrar
    [SerializeField] private string palabraCorrecta = "ULCERA"; //Objetivo a formar
    [SerializeField] private int cantidadLetrasTeclado = 12;// letras totaless

    [Header("Configuraci�n del Juego")]

    [SerializeField] private Transform containerEspacios;
    [SerializeField] private Transform containerTeclado;
    [SerializeField] private GameObject prefabBotonLetra;

    [Header("Pistas clinicas")]
    [SerializeField] private Image uiImagePista;
    [SerializeField] private TextMeshProUGUI uiTextoLesion;
    [SerializeField] private TextMeshProUGUI uiTextoFamilia;
    [SerializeField] private TextMeshProUGUI uiTextoEtiopatogenia;

    [Header("Datos de la pista")]
    [SerializeField] private Sprite imagenPistaSprite;
    [SerializeField] private string NombreLesion = "Lesion Primaria";
    [SerializeField] private string NombreFamilia = "Dermatologica";
    [SerializeField] private string DescripcionEtiopatogenia = "Perdida continuidad de la piel";

    [Header("Retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;
    [SerializeField] public TMP_Text textoRespuesta;

    private List<string> letrasTeclado = new List<string>();
    private string[] progresoUsuario; //variable que guarda las letras que el usuario va ingresando

    //botones para interactuar
    private List<Button> botonesEspaciosUI = new List<Button>();
    private List<Button> botonesTecladoUI = new List<Button>();

    private bool nivelCompletado = false;
    private int erroresNivel = 0;

    void Start()
    {
        // comentar las siguientes dos lineas para funcionamiento con gamemanager ya que si se deje se duplicara el id de los niveles
        /*
        ObtenerPatologiaAleatoria();
        InicializarPregunta(patologiaIDTarget);
        */
    }
    void Update()
    {
        
    }

    //metodo para obtener una patologia aleatoria del CSV a traves del ID de la patologia
    private void ObtenerPatologiaAleatoria()
    {
        if (CsvManager.Instance != null && CsvManager.Instance.patologias != null && CsvManager.Instance.patologias.Count > 0)
        {
            int indice = Random.Range(0, CsvManager.Instance.patologias.Count);
            Patologia patologia = CsvManager.Instance.patologias[indice];
            patologiaIDTarget = patologia.id;
        }
    }

    //metodo para cargar los datos de la patologia desde el CSV
    private void CargarDatosDesdeCSV()
    {
        if (CsvManager.Instance == null) return;

        Patologia patologiaActual = CsvManager.Instance.ObtenerPatologiaPorId(patologiaIDTarget);//
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
                    Debug.LogError("No se encontr� la imagen en: Assets/Resources/Imagenes/" + nombreImagenLimpio);
                }
            }
        }
    }

    //metodo para configurar el panel de pistas con los datos obtenidos del CSV
    private void ConfigurarPanelPistas()
    {
        if (uiImagePista != null && imagenPistaSprite != null) uiImagePista.sprite = imagenPistaSprite;
        if (uiTextoLesion != null) uiTextoLesion.text = "Lesi�n: " + NombreLesion;
        if (uiTextoFamilia != null) uiTextoFamilia.text = "Familia: " + NombreFamilia;
        if (uiTextoEtiopatogenia != null) uiTextoEtiopatogenia.text = "Etiopatogenia: " + DescripcionEtiopatogenia;
    }

    //metodo para generarr las teclas en pantalla
    private void GenerarLetrasTeclado()
    {
        for (int i = 0; i < palabraCorrecta.Length; i++)//a�ade solamente las letras de la palabra correcta
        {
            if (palabraCorrecta[i] == ' ') continue;
            letrasTeclado.Add(palabraCorrecta[i].ToString());
        }

        //a�ade con palabras extras hasta formar la cantidad total de palabras
        string abecedario = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        while (letrasTeclado.Count < cantidadLetrasTeclado)
        {
            string letraAleatoria = abecedario[Random.Range(0, abecedario.Length)].ToString();
            letrasTeclado.Add(letraAleatoria);
        }

        //mezcla las letras
        for (int i = 0; i < letrasTeclado.Count; i++)
        {
            string temp = letrasTeclado[i];
            int randomIndex = Random.Range(i, letrasTeclado.Count);
            letrasTeclado[i] = letrasTeclado[randomIndex];
            letrasTeclado[randomIndex] = temp;
        }
    }

    //metodo para crear las casillas vacias de las palabras en pantalla 
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

    //crea botones para las letras disponibles
    private void CrearTeclado()
    {
        for (int i = 0; i < letrasTeclado.Count; i++)
        {
            int index = i; //obtenemos una copia del listener del boton
            GameObject nuevoBoton = Instantiate(prefabBotonLetra, containerTeclado);
            Button btn = nuevoBoton.GetComponent<Button>();

            string letra = letrasTeclado[index];
            btn.GetComponentInChildren<TextMeshProUGUI>().text = letra;

            //al presionar la tecla se posiciona en su lugar correspondiente
            btn.onClick.AddListener(() => SeleccionarLetraTeclado(index, letra));

            botonesTecladoUI.Add(btn);
        }
    }

    //metodo para seleccionar la tecla
    private void SeleccionarLetraTeclado(int indiceTeclado, string letra)
    {
        if (nivelCompletado) return;

        for (int i = 0; i < progresoUsuario.Length; i++)//busca espacio de izquierda a derecha
        {
            if (string.IsNullOrEmpty(progresoUsuario[i]))
            {
                progresoUsuario[i] = letra; //guara el progreso del usuario (la letra seleccionada)

                botonesEspaciosUI[i].GetComponentInChildren<TextMeshProUGUI>().text = letra; //actualiza la casilla visualmente

                botonesTecladoUI[indiceTeclado].gameObject.SetActive(false);//se desactiva el boton seleccionado para no repetir la letra 

                botonesEspaciosUI[i].name = indiceTeclado.ToString(); //guarda la referencia del indice del teclado para regresarla en caso de error

                ComprobarResultado();
                break;
            }
        }
    }


    //metodo para devolver la tecla a su posicion original
    private void RemoverLetraDeEspacio(int indiceEspacio)
    {

        if (nivelCompletado) return;
        if (!string.IsNullOrEmpty(progresoUsuario[indiceEspacio]))
        {
            if (int.TryParse(botonesEspaciosUI[indiceEspacio].name, out int indiceTecladoOriginal)) //obtenemos de que boton provenia la letra seleccionada
            {
                botonesTecladoUI[indiceTecladoOriginal].gameObject.SetActive(true);
            }
            //limpiamos el progreso 
            progresoUsuario[indiceEspacio] = null;
            
            //limpiamos visualmente la casilla 
            botonesEspaciosUI[indiceEspacio].GetComponentInChildren<TextMeshProUGUI>().text = "";
            botonesEspaciosUI[indiceEspacio].name = "Espacio";
            botonesEspaciosUI[indiceEspacio].GetComponent<Image>().color = Color.white; //restablece el color del espacio a blanco
        }
    }

    //verifica el resultado correcto con la palabra respuesta
    private void ComprobarResultado()
    {
        if (nivelCompletado) return;

        string palabraFormada = "";
        for (int i = 0; i < progresoUsuario.Length; i++)
        {
            if (string.IsNullOrEmpty(progresoUsuario[i])) return;//si hay un espacio vacio no se puede comprobar el resultado
            palabraFormada += progresoUsuario[i];
        }

        if (palabraFormada == palabraCorrecta)
        {
            nivelCompletado = true;
            Debug.Log("<color=green>�Correcto! Has descubierto el diagn�stico cl�nico.</color>");
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
            }

            StartCoroutine(RestaurarColoresEspacios());
        }
    }

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

    public override void EntregarRetroalimentacion()
    {
        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(false); // desactivamos el boton de pausa en la pantalal de retroalimentacion para evitar acoplamiento
        }

        if (nivelCompletado)
        {
            textoResultado.text = "Respuesta Correcta!";
            textoResultado.color = Color.white;
            textoRespuesta.text = "Has descubierto el diagnostico clinico: " + palabraCorrecta;
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


    public override void InicializarPregunta(int indPatologiaAsignada)
    {

        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(true);//lo activamos de vuelta para cuando se inicialize una pregunta
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
