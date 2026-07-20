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
    [SerializeField] private Color colorNormal = Color.white; // Color por defecto de los botones
    [SerializeField] private Color colorSeleccionado = Color.yellow;// Color cuando un boton esta seleccionado
    [SerializeField] private Button botonPausa;// asignar boton pausa en el inspector

    [Header("Configuracion de Reinicio")]
    [SerializeField] private float tiempoReinicio = 2f; // Tiempo en segundos antes de reiniciar

    // Variables de control de seleccion de lesion, familia, etiologia y patologia
    [Header("Componentes de la UI - Bloque 1: Lesiones (Texto)")]
    [SerializeField] private Button[] botonesLesiones;
    [SerializeField] private int[] idLesionesBotones;

    [Header("Componentes de la UI - Bloque 2: Familias (Texto)")]
    [SerializeField] private Button[] botonesFamilias;
    [SerializeField] private int[] idFamiliasBotones;

    [Header("Componentes de la UI - Bloque 3: Etiologias (Texto)")]
    [SerializeField] private Button[] botonesEtiologias;
    [SerializeField] private int[] idEtiologiasBotones;

    [Header("Componentes de la UI - Bloque 4: Patologias (Imagenes)")]
    [SerializeField] private Button[] botonesPatologias;
    [SerializeField] private int[] idPatologiasBotones;

    [Header("Lineas Conectoras")]
    [SerializeField] private LineaConectora lineaConectora;

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

    // Variables para metricas
    private float tiempoInicioNivel;
    private int erroresTotales = 0;
    private List<Dictionary<string, object>> erroresDetalle = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> respuestas = new List<Dictionary<string, object>>();

    private bool reiniciando = false;

    void Start()
    {
        tiempoInicioNivel = Time.time;
        erroresTotales = 0;
        reiniciando = false;

        // configuracion de boton pausa para que no tome el tiempo de juego cuando se pausa y que siempre este activo
        if (botonPausa != null)
        {
            botonPausa.onClick.RemoveAllListeners();
            botonPausa.onClick.AddListener(() => {
                Time.timeScale = Time.timeScale == 1 ? 0 : 1;
            });
            botonPausa.interactable = true;
        }

        ObtenerPatologiaAleatoria();
        CargarDatosEstructuralesCSV();
        AsignarDatosAleatoriosABotones();
        ConfigurarInteractividadArbol();
        ActualizarInteractividadBloques();
    }

    void Update()
    {
        // Mantener el boton de pausa siempre activo
        if (botonPausa != null && !botonPausa.interactable)
        {
            botonPausa.interactable = true;
        }
    }

    //metodo para reiniciar el nivel despues de un tiempo de espera en caso de cometer error
    private IEnumerator ReiniciarNivel()
    {
        reiniciando = true;

        yield return new WaitForSeconds(tiempoReinicio);

        indiceLesionSeleccionada = -1;
        indiceFamiliaSeleccionada = -1;
        indiceEtiologiaSeleccionada = -1;
        indicePatologiaSeleccionada = -1;

        lesionCorrectaSeleccionada = false;
        familiaCorrectaSeleccionada = false;
        etiologiaCorrectaSeleccionada = false;
        patologiaCorrectaSeleccionada = false;

        respuestas.Clear();
        erroresDetalle.Clear();
        erroresTotales = 0;

        RestablecerColorBloque(botonesLesiones, -1);
        RestablecerColorBloque(botonesFamilias, -1);
        RestablecerColorBloque(botonesEtiologias, -1);
        RestablecerColorBloque(botonesPatologias, -1);

        if (lineaConectora != null)
        {
            lineaConectora.LimpiarLineas();
        }
        ActualizarInteractividadBloques();

        reiniciando = false;
        Debug.Log("Nivel reiniciado correctamente");
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

        // Bloque 1: Lesiones
        idLesionesBotones = new int[botonesLesiones.Length];
        int indiceCorrectoLesion = Random.Range(0, botonesLesiones.Length);
        idLesionesBotones[indiceCorrectoLesion] = targetLesionID;

        for (int i = 0; i < botonesLesiones.Length; i++)
        {
            if (i != indiceCorrectoLesion)
            {
                int idFalso = ObtenerIdLesionFalsa();
                idLesionesBotones[i] = idFalso;
            }
            ActualizarTextoBotonLesion(i);
        }

        // Bloque 2: Familias
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

        // Bloque 3: Etiologias
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

        // Bloque 4: Patologias
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
            if (l != null) txt.text = l.nombre;
        }
    }

    //metodo para actualizar el texto del boton de familia segun el id de familia asignado al boton
    private void ActualizarTextoBotonFamilia(int indice)
    {
        TextMeshProUGUI txt = botonesFamilias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Familia f = CsvManager.Instance.ObtenerFamiliaPorId(idFamiliasBotones[indice]);
            if (f != null) txt.text = f.nombre;
        }
    }

    //metodo para actualizar el texto del boton de etiologia segun el id de etiologia asignado al boton
    private void ActualizarTextoBotonEtiologia(int indice)
    {
        TextMeshProUGUI txt = botonesEtiologias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(idEtiologiasBotones[indice]);
            if (e != null) txt.text = e.nombre;
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

        for (int i = 0; i < botonesPatologias.Length; i++)
        {
            int index = i;
            if (botonesPatologias[index] != null)
            {
                botonesPatologias[index].onClick.AddListener(() => ValidarSeleccionPatologia(index));
            }
        }
    }

    //metodo para actualizar la interactividad de los bloques de botones segun las selecciones realizadas
    private void ActualizarInteractividadBloques()
    {
        SetBloqueInteractable(botonesFamilias, indiceLesionSeleccionada != -1);
        SetBloqueInteractable(botonesEtiologias, indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1);
        SetBloqueInteractable(botonesPatologias, indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1 && indiceEtiologiaSeleccionada != -1);
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

        // Linea 1: lesion → familia
        if (indiceLesionSeleccionada != -1 && indiceFamiliaSeleccionada != -1)
        {
            Button origen = botonesLesiones[indiceLesionSeleccionada];
            Button destino = botonesFamilias[indiceFamiliaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // linea 2: familia → etiologia
        if (indiceFamiliaSeleccionada != -1 && indiceEtiologiaSeleccionada != -1)
        {
            Button origen = botonesFamilias[indiceFamiliaSeleccionada];
            Button destino = botonesEtiologias[indiceEtiologiaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // linea 3: etiologia → patologia
        if (indiceEtiologiaSeleccionada != -1 && indicePatologiaSeleccionada != -1)
        {
            Button origen = botonesEtiologias[indiceEtiologiaSeleccionada];
            Button destino = botonesPatologias[indicePatologiaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // Si todas las selecciones estan completas, cambiar colores a los colores correctos o incorrectos segun la validacion
        if (lineaConectora.HayLineas() &&
            indiceLesionSeleccionada != -1 &&
            indiceFamiliaSeleccionada != -1 &&
            indiceEtiologiaSeleccionada != -1 &&
            indicePatologiaSeleccionada != -1)
        {
            int idx = 0;

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
                idx++;
            }

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (etiologiaCorrectaSeleccionada && patologiaCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
            }

            if (!lesionCorrectaSeleccionada || !familiaCorrectaSeleccionada ||
                !etiologiaCorrectaSeleccionada || !patologiaCorrectaSeleccionada)
            {
                if (!reiniciando && !finished)
                {
                    Debug.Log("Intento incorrecto-Reiniciando en " + tiempoReinicio + " segundos...");
                    StartCoroutine(ReiniciarNivel());
                }
            }
        }
    }

    //metodos para limpiar las selecciones posteriores a la seleccion de lesion, familia y etiologia
    private void LimpiarSeleccionesPosterioresALesion()
    {
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        indicePatologiaSeleccionada = -1;
        patologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    private void LimpiarSeleccionesPosterioresAFamilia()
    {
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        indicePatologiaSeleccionada = -1;
        patologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    private void LimpiarSeleccionesPosterioresAEtiologia()
    {
        indicePatologiaSeleccionada = -1;
        patologiaCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    //metodo para restablecer los colores de todos los bloques de botones segun la seleccion actual
    private void RestablecerColoresTodosLosBloques()
    {
        RestablecerColorBloque(botonesLesiones, indiceLesionSeleccionada);
        RestablecerColorBloque(botonesFamilias, indiceFamiliaSeleccionada);
        RestablecerColorBloque(botonesEtiologias, indiceEtiologiaSeleccionada);
        RestablecerColorBloque(botonesPatologias, indicePatologiaSeleccionada);
    }

    //metodos para validar la seleccion de lesion, familia, etiologia y patologia
    private void ValidarSeleccionLesion(int indice)
    {
        if (reiniciando) return;

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

        respuestas.Add(new Dictionary<string, object>()
        {
            { "tipo", "lesion" },
            { "indice", indice },
            { "correcto", lesionCorrectaSeleccionada }
        });

        if (!lesionCorrectaSeleccionada)
        {
            erroresTotales++;
            erroresDetalle.Add(new Dictionary<string, object>()
            {
                { "tipo", "lesion" },
                { "seleccionado", indice },
                { "correcto", targetLesionID }
            });
        }

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    private void ValidarSeleccionFamilia(int indice)
    {
        if (reiniciando) return;

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

        respuestas.Add(new Dictionary<string, object>()
        {
            { "tipo", "familia" },
            { "indice", indice },
            { "correcto", familiaCorrectaSeleccionada }
        });

        if (!familiaCorrectaSeleccionada)
        {
            erroresTotales++;
            erroresDetalle.Add(new Dictionary<string, object>()
            {
                { "tipo", "familia" },
                { "seleccionado", indice },
                { "correcto", targetFamiliaID }
            });
        }

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    private void ValidarSeleccionEtiologia(int indice)
    {
        if (reiniciando) return;

        if (indiceEtiologiaSeleccionada == indice)
        {
            botonesEtiologias[indice].GetComponent<Image>().color = colorNormal;
            indiceEtiologiaSeleccionada = -1;
            etiologiaCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresAEtiologia();
            ActualizarInteractividadBloques();
            ActualizarLineas();
            return;
        }

        indiceEtiologiaSeleccionada = indice;
        etiologiaCorrectaSeleccionada = (indice < idEtiologiasBotones.Length && idEtiologiasBotones[indice] == targetEtiologiaID);

        respuestas.Add(new Dictionary<string, object>()
        {
            { "tipo", "etiologia" },
            { "indice", indice },
            { "correcto", etiologiaCorrectaSeleccionada }
        });

        if (!etiologiaCorrectaSeleccionada)
        {
            erroresTotales++;
            erroresDetalle.Add(new Dictionary<string, object>()
            {
                { "tipo", "etiologia" },
                { "seleccionado", indice },
                { "correcto", targetEtiologiaID }
            });
        }

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    private void ValidarSeleccionPatologia(int indice)
    {
        if (reiniciando) return;

        if (indicePatologiaSeleccionada == indice)
        {
            botonesPatologias[indice].GetComponent<Image>().color = colorNormal;
            indicePatologiaSeleccionada = -1;
            patologiaCorrectaSeleccionada = false;
            RestablecerColoresTodosLosBloques();
            ActualizarLineas();
            ActualizarInteractividadBloques();
            return;
        }

        indicePatologiaSeleccionada = indice;
        patologiaCorrectaSeleccionada = (indice < idPatologiasBotones.Length && idPatologiasBotones[indice] == targetPatologiaID);

        respuestas.Add(new Dictionary<string, object>()
        {
            { "tipo", "patologia" },
            { "indice", indice },
            { "correcto", patologiaCorrectaSeleccionada }
        });

        if (!patologiaCorrectaSeleccionada)
        {
            erroresTotales++;
            erroresDetalle.Add(new Dictionary<string, object>()
            {
                { "tipo", "patologia" },
                { "seleccionado", indice },
                { "correcto", targetPatologiaID }
            });
        }

        RestablecerColoresTodosLosBloques();
        PintarCaminoFinal();
        ActualizarLineas();
        ActualizarInteractividadBloques();
        VerificarProgresoArbol();
    }

    //metodo para pintar el camino final de seleccion de lesion, familia, etiologia y patologia con colores verde o rojo segun si la seleccion es correcta o incorrecta
    private void PintarCaminoFinal()
    {
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

        if (indicePatologiaSeleccionada != -1)
        {
            botonesPatologias[indicePatologiaSeleccionada].GetComponent<Image>().color = patologiaCorrectaSeleccionada ? Color.green : Color.red;
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

    //metodo para verificar si el usuario ha completado correctamente el arbol de decisiones y marcar el nivel como terminado
    private void VerificarProgresoArbol()
    {
        if (lesionCorrectaSeleccionada && familiaCorrectaSeleccionada &&
            etiologiaCorrectaSeleccionada && patologiaCorrectaSeleccionada)
        {
            finished = true;
            float tiempoTotal = Time.time - tiempoInicioNivel;

            Debug.Log("<color=green>¡Nivel Completado!</color>");

            if (ConexionFirestore.Instance != null)
            {
                ConexionFirestore.Instance.GuardarPartida(
                    nivel: "Nivel_3",
                    patologiaID: targetPatologiaID,
                    patologiaOK: patologiaCorrectaSeleccionada,
                    etiologiaOK: etiologiaCorrectaSeleccionada,
                    familiaOK: familiaCorrectaSeleccionada,
                    lesionOK: lesionCorrectaSeleccionada,
                    errores: erroresTotales,
                    tiempo: tiempoTotal,
                    erroresDetalle: erroresDetalle,
                    respuestas: respuestas
                );
            }
        }
        else
        {
            finished = false;
        }
    }

    public override void EntregarRetroalimentacion()
    {

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {

    }
}