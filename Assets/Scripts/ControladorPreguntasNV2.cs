using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorDecisionNV3 : ControladorPreguntas
{
    [Header("Configuración del Juego")]
    [SerializeField] private int patologiaIDTarget = 1; // Id de la patologia que se debe seleccionar correctamente
    [SerializeField] private Color colorNormal = Color.white; // Color por defecto de los botones
    [SerializeField] private Color colorSeleccionado = Color.yellow;// Color cuando un botón está seleccionado
    [SerializeField] private Button botonPausa;// asignar boton pausa en el inspector

    // Variables de control de selección de patología, etiología, familia y lesión
    [Header("Componentes de la UI - Bloque 1: Patologías (Imagenes)")]
    [SerializeField] private Button[] botonesPatologias;
    [SerializeField] private int[] idPatologiasBotones;

    [Header("Componentes de la UI - Bloque 2: Etiologías (Texto)")]
    [SerializeField] private Button[] botonesEtiologias;
    [SerializeField] private int[] idEtiologiasBotones;

    [Header("Componentes de la UI - Bloque 3: Familias (Texto)")]
    [SerializeField] private Button[] botonesFamilias;
    [SerializeField] private int[] idFamiliasBotones;

    [Header("Componentes de la UI - Bloque 4: Lesiones (Texto/Imagen)")]
    [SerializeField] private Button[] botonesLesiones;
    [SerializeField] private int[] idLesionesBotones;

    [Header("Líneas Conectoras")]
    [SerializeField] private LineaConectora lineaConectora;

    private int targetEtiologiaID; // Id de la etiología que se debe seleccionar correctamente
    private int targetFamiliaID;// Id de la familia que se debe seleccionar correctamente
    private int targetLesionID;// Id de la lesión que se debe seleccionar correctamente

    //indice de las patologias, etiologias, familias y lesiones seleccionadas por el usuario
    private int indicePatologiaSeleccionada = -1;
    private int indiceEtiologiaSeleccionada = -1;
    private int indiceFamiliaSeleccionada = -1;
    private int indiceLesionSeleccionada = -1;

    // Variables de control para verificar si la selección es correcta
    private bool patologiaCorrectaSeleccionada = false;
    private bool etiologiaCorrectaSeleccionada = false;
    private bool familiaCorrectaSeleccionada = false;
    private bool lesionCorrectaSeleccionada = false;

    void Start()
    {
        ObtenerPatologiaAleatoria();
        CargarDatosEstructuralesCSV();
        AsignarDatosAleatoriosABotones();
        ConfigurarInteractividadArbol();
        ActualizarInteractividadBloques();
    }

    void Update()
    {

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
            targetEtiologiaID = patologiaActual.etiologiaID;
            targetFamiliaID = patologiaActual.familiaID;
            targetLesionID = patologiaActual.lesionID;
        }
    }

    //metodo para asignar datos aleatorios a los botones de patologia, etiologia, familia y lesion
    private void AsignarDatosAleatoriosABotones()
    {
        if (CsvManager.Instance == null) return;

        idPatologiasBotones = new int[botonesPatologias.Length];
        int indiceCorrectoPatologia = Random.Range(0, botonesPatologias.Length);
        idPatologiasBotones[indiceCorrectoPatologia] = patologiaIDTarget;

        for (int i = 0; i < botonesPatologias.Length; i++)
        {
            if (i != indiceCorrectoPatologia)
            {
                int idFalso = ObtenerIdPatologiaFalsa();
                idPatologiasBotones[i] = idFalso;
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

    //metodops para obtener ids falsos de patologia, etiologia, familia y lesion que no sean iguales a los ids correctos
    private int ObtenerIdPatologiaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.id != patologiaIDTarget) idsValidos.Add(p.id);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }

    //metodo para obtener un id falso de etiologia que no sea igual al id correcto
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

    //metodo para obtener un id falso de familia que no sea igual al id correcto
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

    //metodo para obtener un id falso de lesion que no sea igual al id correcto
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
                Debug.LogError("No se encontro la imagen a través de CsvManager para el código: " + nombreImagenLimpio);
            }

            TextMeshProUGUI txt = botonesPatologias[indice].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";
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

        for (int i = 0; i < botonesEtiologias.Length; i++)
        {
            int index = i;
            if (botonesEtiologias[index] != null)
            {
                botonesEtiologias[index].onClick.AddListener(() => ValidarSeleccionEtiologia(index));
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

        for (int i = 0; i < botonesLesiones.Length; i++)
        {
            int index = i;
            if (botonesLesiones[index] != null)
            {
                botonesLesiones[index].onClick.AddListener(() => ValidarSeleccionLesion(index));
            }
        }
    }

    //metodo para actualizar la interactividad de los bloques de botones según las selecciones realizadas
    private void ActualizarInteractividadBloques()
    {
        SetBloqueInteractable(botonesEtiologias, indicePatologiaSeleccionada != -1);
        SetBloqueInteractable(botonesFamilias, indicePatologiaSeleccionada != -1 && indiceEtiologiaSeleccionada != -1);
        SetBloqueInteractable(botonesLesiones, indicePatologiaSeleccionada != -1 && indiceEtiologiaSeleccionada != -1 && indiceFamiliaSeleccionada != -1);
    }

    //metodo para establecer la interactividad de un bloque de botones y cambiar su color según el estado
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

        // Linea 1: patologia → etiologia
        if (indicePatologiaSeleccionada != -1 && indiceEtiologiaSeleccionada != -1)
        {
            Button origen = botonesPatologias[indicePatologiaSeleccionada];
            Button destino = botonesEtiologias[indiceEtiologiaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // linea 2: etiologia → familia
        if (indiceEtiologiaSeleccionada != -1 && indiceFamiliaSeleccionada != -1)
        {
            Button origen = botonesEtiologias[indiceEtiologiaSeleccionada];
            Button destino = botonesFamilias[indiceFamiliaSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // linea 3: familia → lesion
        if (indiceFamiliaSeleccionada != -1 && indiceLesionSeleccionada != -1)
        {
            Button origen = botonesFamilias[indiceFamiliaSeleccionada];
            Button destino = botonesLesiones[indiceLesionSeleccionada];
            Color color = lineaConectora.GetColorSeleccion();
            lineaConectora.CrearLinea(origen, destino, color);
        }

        // Si todas las selecciones estan completas, cambiar colores a los colores correctos o incorrectos según la validación
        if (lineaConectora.HayLineas() &&
            indicePatologiaSeleccionada != -1 &&
            indiceEtiologiaSeleccionada != -1 &&
            indiceFamiliaSeleccionada != -1 &&
            indiceLesionSeleccionada != -1)
        {
            int idx = 0;

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (patologiaCorrectaSeleccionada && etiologiaCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
                idx++;
            }

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (etiologiaCorrectaSeleccionada && familiaCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
                idx++;
            }

            if (lineaConectora.CantidadLineas() > idx)
            {
                Color color = (familiaCorrectaSeleccionada && lesionCorrectaSeleccionada)
                    ? lineaConectora.GetColorCorrecta()
                    : lineaConectora.GetColorIncorrecta();
                lineaConectora.CambiarColorLinea(idx, color);
            }
        }
    }

    //metodos para limpiar las selecciones posteriores a la seleccion de patologia, etiologia y familia
    private void LimpiarSeleccionesPosterioresAPatologia()
    {
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    //metodo para limpiar las selecciones posteriores a la seleccion de etiologia
    private void LimpiarSeleccionesPosterioresAEtiologia()
    {
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    //metodo para limpiar las selecciones posteriores a la seleccion de familia
    private void LimpiarSeleccionesPosterioresAFamilia()
    {
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
    }

    //metodo para restablecer los colores de todos los bloques de botones según la selección actual
    private void RestablecerColoresTodosLosBloques()
    {
        RestablecerColorBloque(botonesPatologias, indicePatologiaSeleccionada);
        RestablecerColorBloque(botonesEtiologias, indiceEtiologiaSeleccionada);
        RestablecerColorBloque(botonesFamilias, indiceFamiliaSeleccionada);
        RestablecerColorBloque(botonesLesiones, indiceLesionSeleccionada);
    }

    //metodos para validar la seleccion de patologia, etiologia, familia y lesion
    private void ValidarSeleccionPatologia(int indice)
    {
        if (indicePatologiaSeleccionada == indice)
        {
            botonesPatologias[indice].GetComponent<Image>().color = colorNormal;
            indicePatologiaSeleccionada = -1;
            patologiaCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresAPatologia();
            ActualizarInteractividadBloques();
            return;
        }

        indicePatologiaSeleccionada = indice;
        patologiaCorrectaSeleccionada = (indice < idPatologiasBotones.Length && idPatologiasBotones[indice] == patologiaIDTarget);

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    //metodos para validar la seleccion de etiologia, familia y lesion
    private void ValidarSeleccionEtiologia(int indice)
    {
        if (indiceEtiologiaSeleccionada == indice)
        {
            botonesEtiologias[indice].GetComponent<Image>().color = colorNormal;
            indiceEtiologiaSeleccionada = -1;
            etiologiaCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresAEtiologia();
            ActualizarInteractividadBloques();
            return;
        }

        indiceEtiologiaSeleccionada = indice;
        etiologiaCorrectaSeleccionada = (indice < idEtiologiasBotones.Length && idEtiologiasBotones[indice] == targetEtiologiaID);

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    //metodos para validar la seleccion de familia y lesion
    private void ValidarSeleccionFamilia(int indice)
    {
        if (indiceFamiliaSeleccionada == indice)
        {
            botonesFamilias[indice].GetComponent<Image>().color = colorNormal;
            indiceFamiliaSeleccionada = -1;
            familiaCorrectaSeleccionada = false;
            LimpiarSeleccionesPosterioresAFamilia();
            ActualizarInteractividadBloques();
            return;
        }

        indiceFamiliaSeleccionada = indice;
        familiaCorrectaSeleccionada = (indice < idFamiliasBotones.Length && idFamiliasBotones[indice] == targetFamiliaID);

        RestablecerColoresTodosLosBloques();
        ActualizarLineas();
        ActualizarInteractividadBloques();
    }

    //metodos para validar la seleccion de lesion
    private void ValidarSeleccionLesion(int indice)
    {
        if (indiceLesionSeleccionada == indice)
        {
            botonesLesiones[indice].GetComponent<Image>().color = colorNormal;
            indiceLesionSeleccionada = -1;
            lesionCorrectaSeleccionada = false;
            RestablecerColoresTodosLosBloques();
            ActualizarLineas();
            ActualizarInteractividadBloques();
            return;
        }

        indiceLesionSeleccionada = indice;
        lesionCorrectaSeleccionada = (indice < idLesionesBotones.Length && idLesionesBotones[indice] == targetLesionID);

        RestablecerColoresTodosLosBloques();
        PintarCaminoFinal();
        ActualizarLineas();
        ActualizarInteractividadBloques();
        VerificarProgresoArbol();
    }

    //metodo para pintar el camino final de seleccion de patologia, etiologia, familia y lesion con colores verde o rojo segun si la seleccion es correcta o incorrecta
    private void PintarCaminoFinal()
    {
        if (indicePatologiaSeleccionada != -1)
        {
            botonesPatologias[indicePatologiaSeleccionada].GetComponent<Image>().color = patologiaCorrectaSeleccionada ? Color.green : Color.red;
        }

        if (indiceEtiologiaSeleccionada != -1)
        {
            botonesEtiologias[indiceEtiologiaSeleccionada].GetComponent<Image>().color = etiologiaCorrectaSeleccionada ? Color.green : Color.red;
        }

        if (indiceFamiliaSeleccionada != -1)
        {
            botonesFamilias[indiceFamiliaSeleccionada].GetComponent<Image>().color = familiaCorrectaSeleccionada ? Color.green : Color.red;
        }

        if (indiceLesionSeleccionada != -1)
        {
            botonesLesiones[indiceLesionSeleccionada].GetComponent<Image>().color = lesionCorrectaSeleccionada ? Color.green : Color.red;
        }
    }

    //metodo para restablecer el color de un bloque de botones según el índice seleccionado
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
        if (patologiaCorrectaSeleccionada && etiologiaCorrectaSeleccionada && familiaCorrectaSeleccionada && lesionCorrectaSeleccionada)
        {
            finished = true;
            Debug.Log("<color=green>¡Nivel Completado! Árbol de decisiones resuelto con éxito.</color>");
        }
    }

    public override void EntregarRetroalimentacion()
    {

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {

    }
}