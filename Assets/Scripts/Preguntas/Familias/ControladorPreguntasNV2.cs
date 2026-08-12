using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Recordar quitar las metricas de este controlador ya que seran manejadas desde el gamemanager :p
//recordar corregir metricas correctas y guardar los errores de una forma correcta
public class ControladorPreguntasNV2 : ControladorPreguntas
{
    [Header("Configuracion del Juego")]
    [SerializeField] private int patologiaIDTarget = 1; // Id de la patologia que se debe seleccionar correctamente
    [SerializeField] private string lesionSeleccionadaTexto = "Ninguna";
    [SerializeField] private string familiaSeleccionadaTexto = "Ninguna";
    [SerializeField] private string etiologiaSeleccionadaTexto = "Ninguna";
    [SerializeField] private Color colorNormal = Color.white; // Color por defecto de los botones
    [SerializeField] private Color colorSeleccionado = Color.yellow;// Color cuando un boton esta seleccionado
    [SerializeField] private Button botonPausa;// asignar boton pausa en el inspector

    // Variables de control de seleccion de lesion, familia, etiologia y patologia

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

    private int targetLesionID; // Id de la lesion que se debe seleccionar correctamente
    private int targetFamiliaID;// Id de la familia que se debe seleccionar correctamente
    private int targetEtiologiaID;// Id de la etiologia que se debe seleccionar correctamente
    private int targetPatologiaID;// Id de la patologia que se debe seleccionar correctamente

    //indice de las lesiones, familias, etiologias y patologias seleccionadas por el usuario
    private int indiceLesionSeleccionada = -1;
    private int indiceFamiliaSeleccionada = -1;
    private int indiceEtiologiaSeleccionada = -1;
    private int indicePatologiaSeleccionada = -1;

    // Variables de control para verificar si la seleccion es correcta
    private bool lesionCorrectaSeleccionada = false;
    private bool familiaCorrectaSeleccionada = false;
    private bool etiologiaCorrectaSeleccionada = false;
    private bool patologiaCorrectaSeleccionada = false;

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
        // comentar las siguientes dos lineas para funcionamiento con gamemanager ya que si se deje se duplicara el id de los niveles
        /*
        ObtenerPatologiaAleatoria(); 
        InicializarPregunta(patologiaIDTarget);
        */
    }

    void Update()
    {
        if (botonPausa != null && !botonPausa.interactable)
        {
            botonPausa.interactable = true;
        }
    }

    //metodo para obtener una patologia aleatoria de la lista de patologias del CsvManager
    private void ObtenerPatologiaAleatoria()
    {
        if (CsvManager.Instance != null && CsvManager.Instance.patologias != null && CsvManager.Instance.patologias.Count > 0)
        {
            int indice = Random.Range(0, CsvManager.Instance.patologias.Count);
            Patologia patologia = CsvManager.Instance.patologias[indice];
            patologiaIDTarget = patologia.id;
        }
    }

    //metodo para obtener los datos estructurales de la patologia objetivo desde el CsvManager
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

    //metodo para asignar datos aleatorios a los botones de lesion, familia, etiologia y patologia
    private void AsignarDatosAleatoriosABotones()
    {
        if (CsvManager.Instance == null) return;

        // Bloque 1: Patologias
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

        // Bloque 2: Etiologias
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

        // Bloque 3: Familias
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

        // Bloque 4: Lesiones
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

    //metodops para obtener ids falsos de lesion, familia, etiologia y patologia que no sean iguales a los ids correctos
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

    private int ObtenerIdPatologiaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.id != targetPatologiaID) idsValidos.Add(p.id);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

    //metodo para actualizar el texto del boton de lesion segun el id de lesion asignado al boton
    private void ActualizarTextoBotonLesion(int indice)
    {
        TextMeshProUGUI txt = botonesLesiones[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Lesion l = CsvManager.Instance.ObtenerLesionPorId(idLesionesBotones[indice]);
            txt.text = "Lesión: " + (l != null ? l.nombre : "Desconocida");
        }
    }

    //metodo para actualizar el texto del boton de familia segun el id de familia asignado al boton
    private void ActualizarTextoBotonFamilia(int indice)
    {
        TextMeshProUGUI txt = botonesFamilias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Familia f = CsvManager.Instance.ObtenerFamiliaPorId(idFamiliasBotones[indice]);
            txt.text = "Familia: " + (f != null ? f.nombre : "Desconocida");
        }
    }

    //metodo para actualizar el texto del boton de etiologia segun el id de etiologia asignado al boton
    private void ActualizarTextoBotonEtiologia(int indice)
    {
        TextMeshProUGUI txt = botonesEtiologias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(idEtiologiasBotones[indice]);
            txt.text = "Etiopatogenia: " + (e != null ? e.nombre : "Desconocida");
        }
    }

    //metodo para actualizar la imagen del boton de patologia segun el id de patologia asignado al boton
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
                //Debug.LogError("No se encontro la imagen a traves de CsvManager para el codigo: " + nombreImagenLimpio);
            }

            TextMeshProUGUI txt = botonesPatologias[indice].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";
        }
    }

    //metodo para configurar la interactividad de los botones del arbol de decisiones
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

    //metodo para actualizar la interactividad de los bloques de botones segun las selecciones realizadas
    private void ActualizarInteractividadBloques()
    {
        SetBloqueInteractable(botonesLesiones, indicePatologiaSeleccionada != -1);
        SetBloqueInteractable(botonesFamilias, indicePatologiaSeleccionada != -1 && indiceLesionSeleccionada != -1);
        SetBloqueInteractable(botonesEtiologias, indicePatologiaSeleccionada != -1 && indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1);
    }

    //metodo para establecer la interactividad de un bloque de botones y cambiar su color segun el estado
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

    //metodo para actualizar las lineas conectoras entre los botones seleccionados(usa lineaconectora script)
    private void ActualizarLineas()
    {
        if (lineaConectora == null) return;

        lineaConectora.LimpiarLineas();

        // Linea 1: patologia → lesion
        if (indicePatologiaSeleccionada != -1 && indiceLesionSeleccionada != -1)
        {
            Button origen = botonesPatologias[indicePatologiaSeleccionada];
            Button destino = botonesLesiones[indiceLesionSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // linea 2: lesion → familia
        if (indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1)
        {
            Button origen = botonesLesiones[indiceLesionSeleccionada];
            Button destino = botonesFamilias[indiceFamiliaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // linea 3: familia → etiologia
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
    //metodos para limpiar las selecciones posteriores a la seleccion de lesion, familia y etiologia
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

    private void LimpiarSeleccionesPosterioresALesion()
    {
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    private void LimpiarSeleccionesPosterioresAFamilia()
    {
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    //metodo para restablecer los colores de todos los bloques de botones segun la seleccion actual
    private void RestablecerColoresTodosLosBloques()
    {
        RestablecerColorBloque(botonesPatologias, indicePatologiaSeleccionada);
        RestablecerColorBloque(botonesLesiones, indiceLesionSeleccionada);
        RestablecerColorBloque(botonesFamilias, indiceFamiliaSeleccionada);
        RestablecerColorBloque(botonesEtiologias, indiceEtiologiaSeleccionada);
    }

    //metodos para validar la seleccion de lesion, familia, etiologia y patologia
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

    //metodo para pintar el camino final de seleccion de lesion, familia, etiologia y patologia con colores verde o rojo segun si la seleccion es correcta o incorrecta
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

    //metodo para restablecer el color de un bloque de botones segun el indice seleccionado
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

    //metodo para verificar si el usuario ha completado correctamente el arbol de decisiones y marcar el nivel como terminado
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

    public override void InicializarPregunta(int indPatologiaAsignada)
    {

        if (botonPausa != null)
        {
            botonPausa.gameObject.SetActive(true);//lo activamos de vuelta para cuando se inicialize una pregunta
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
        //funcion para poder mostrar los nombres de lesion, familia y etiologia en el inspector  
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