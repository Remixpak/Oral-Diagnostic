using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorDecisionNV3 : ControladorPreguntas
{
    [Header("Configuración del Juego")]
    [SerializeField] private int patologiaIDTarget = 1; // ID de la patología objetivo para el nivel
    [SerializeField] private Color colorNormal = Color.white; // Color por defecto para los botones
    [SerializeField] private Color colorSeleccionado = Color.yellow; // Color para el botón seleccionado
    [SerializeField] private Button botonPausa; // Botón de pausa para el juego

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

    // Variables para almacenar los IDs correctos de cada bloque
    private int targetEtiologiaID;
    private int targetFamiliaID;
    private int targetLesionID;
    // Variables para almacenar los índices seleccionados y si son correctos
    private int indicePatologiaSeleccionada = -1;
    private int indiceEtiologiaSeleccionada = -1;
    private int indiceFamiliaSeleccionada = -1;
    private int indiceLesionSeleccionada = -1;
    // Variables para verificar si la selección es correcta
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

    //metodo para obtener una patología aleatoria de la lista de patologías del CsvManager
    private void ObtenerPatologiaAleatoria()
    {
        if (CsvManager.Instance != null && CsvManager.Instance.patologias != null && CsvManager.Instance.patologias.Count > 0)
        {
            int indice = Random.Range(0, CsvManager.Instance.patologias.Count);
            Patologia patologia = CsvManager.Instance.patologias[indice];
            patologiaIDTarget = patologia.id;
        }
    }

    //metodo para cargar los datos estructurales de la patología objetivo desde el CsvManager
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

    //metodo para asignar datos aleatorios a los botones de cada bloque, asegurando que uno de ellos sea el correcto
    private void AsignarDatosAleatoriosABotones()
    {
        if (CsvManager.Instance == null) return;

        idPatologiasBotones = new int[botonesPatologias.Length];
        int indiceCorrectoPatologia = Random.Range(0, botonesPatologias.Length);
        idPatologiasBotones[indiceCorrectoPatologia] = patologiaIDTarget;

        for (int i = 0; i < botonesPatologias.Length; i++) // Recorrer todos los botones de patologías
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

        for (int i = 0; i < botonesEtiologias.Length; i++) // Recorrer todos los botones de etiologías
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

        for (int i = 0; i < botonesFamilias.Length; i++) // Recorrer todos los botones de familias
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

        for (int i = 0; i < botonesLesiones.Length; i++) // Recorrer todos los botones de lesiones
        {
            if (i != indiceCorrectoLesion)
            {
                idLesionesBotones[i] = ObtenerIdLesionFalsa();
            }
            ActualizarTextoBotonLesion(i);
        }
    }
    //metodo para obtener un id de patología falso, diferente al id de la patología objetivo
    private int ObtenerIdPatologiaFalsa()
    {
        List<int> idsValidos = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            if (p.id != patologiaIDTarget) idsValidos.Add(p.id);
        }
        return idsValidos.Count > 0 ? idsValidos[Random.Range(0, idsValidos.Count)] : 0;
    }
    //metodo para obtener un id de etiología falso, diferente al id de la etiología objetivo
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
    //metodo para obtener un id de familia falso, diferente al id de la familia objetivo
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
    //metodo para obtener un id de lesión falso, diferente al id de la lesión objetivo
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
    //metodo para actualizar la imagen del botón de patología según el índice y el id de patología correspondiente
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
                Debug.LogError("No se encontró la imagen a través de CsvManager para el código: " + nombreImagenLimpio);
            }

            TextMeshProUGUI txt = botonesPatologias[indice].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "";
        }
    }
    //metodo para actualizar el texto del botón de etiología según el índice y el id de etiología correspondiente
    private void ActualizarTextoBotonEtiologia(int indice)
    {
        TextMeshProUGUI txt = botonesEtiologias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(idEtiologiasBotones[indice]);
            if (e != null) txt.text = e.nombre;
        }
    }
    //metodo para actualizar el texto del botón de familia según el índice y el id de familia correspondiente
    private void ActualizarTextoBotonFamilia(int indice)
    {
        TextMeshProUGUI txt = botonesFamilias[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Familia f = CsvManager.Instance.ObtenerFamiliaPorId(idFamiliasBotones[indice]);
            if (f != null) txt.text = f.nombre;
        }
    }
    //metodo para actualizar el texto del botón de lesión según el índice y el id de lesión correspondiente
    private void ActualizarTextoBotonLesion(int indice)
    {
        TextMeshProUGUI txt = botonesLesiones[indice].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            Lesion l = CsvManager.Instance.ObtenerLesionPorId(idLesionesBotones[indice]);
            if (l != null) txt.text = l.nombre;
        }
    }
    //metodo para configurar la interactividad de los botones del árbol de decisiones, asignando los eventos de click a cada botón
    private void ConfigurarInteractividadArbol()
    {
        for (int i = 0; i < botonesPatologias.Length; i++)// Recorrer todos los botones de patologías
        {
            int index = i;
            if (botonesPatologias[index] != null)
            {
                botonesPatologias[index].onClick.AddListener(() => ValidarSeleccionPatologia(index));// Agregar el listener para el botón de patología
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

        for (int i = 0; i < botonesLesiones.Length; i++)// Recorrer todos los botones de lesiones
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

    //metodo para establecer la interactividad de un bloque de botones, habilitando o deshabilitando los botones según el estado proporcionado
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

    //metodos para limpiar las selecciones posteriores a cada bloque, restableciendo los índices y estados de selección, y restableciendo los colores de los botones
    private void LimpiarSeleccionesPosterioresAPatologia()
    {
        indiceEtiologiaSeleccionada = -1;
        etiologiaCorrectaSeleccionada = false;
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
    }

    //metodo para limpiar las selecciones posteriores al bloque de etiología, restableciendo los índices y estados de selección, y restableciendo los colores de los botones
    private void LimpiarSeleccionesPosterioresAEtiologia()
    {
        indiceFamiliaSeleccionada = -1;
        familiaCorrectaSeleccionada = false;
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
    }
    //metodo para limpiar las selecciones posteriores al bloque de familia, restableciendo los índices y estados de selección, y restableciendo los colores de los botones
    private void LimpiarSeleccionesPosterioresAFamilia()
    {
        indiceLesionSeleccionada = -1;
        lesionCorrectaSeleccionada = false;
        RestablecerColoresTodosLosBloques();
    }

    //metodo para restablecer los colores de todos los bloques de botones, resaltando el botón seleccionado y restableciendo los demás al color normal
    private void RestablecerColoresTodosLosBloques()
    {
        RestablecerColorBloque(botonesPatologias, indicePatologiaSeleccionada);
        RestablecerColorBloque(botonesEtiologias, indiceEtiologiaSeleccionada);
        RestablecerColorBloque(botonesFamilias, indiceFamiliaSeleccionada);
        RestablecerColorBloque(botonesLesiones, indiceLesionSeleccionada);
    }
    //metodos para validar la selección de cada bloque, verificando si el índice seleccionado es el mismo que el previamente seleccionado, y actualizando los estados y colores de los botones en consecuencia
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
        ActualizarInteractividadBloques();
    }

    //metodo para validar la selección del bloque de etiología, verificando si el índice seleccionado es el mismo que el previamente seleccionado, y actualizando los estados y colores de los botones en consecuencia
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
        ActualizarInteractividadBloques();
    }

    //metodo para validar la selección del bloque de familia, verificando si el índice seleccionado es el mismo que el previamente seleccionado, y actualizando los estados y colores de los botones en consecuencia
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
        ActualizarInteractividadBloques();
    }

    //metodo para validar la selección del bloque de lesión, verificando si el índice seleccionado es el mismo que el previamente seleccionado, y actualizando los estados y colores de los botones en consecuencia
    private void ValidarSeleccionLesion(int indice)
    {
        if (indiceLesionSeleccionada == indice)
        {
            botonesLesiones[indice].GetComponent<Image>().color = colorNormal;
            indiceLesionSeleccionada = -1;
            lesionCorrectaSeleccionada = false;
            RestablecerColoresTodosLosBloques();
            ActualizarInteractividadBloques();
            return;
        }

        indiceLesionSeleccionada = indice;
        lesionCorrectaSeleccionada = (indice < idLesionesBotones.Length && idLesionesBotones[indice] == targetLesionID);

        RestablecerColoresTodosLosBloques();
        PintarCaminoFinal();
        ActualizarInteractividadBloques();
        VerificarProgresoArbol();
    }

    //metodo para pintar el camino final del árbol de decisiones, resaltando los botones seleccionados en verde si son correctos o en rojo si son incorrectos
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

    //metodo para restablecer el color de un bloque de botones, resaltando el botón seleccionado y restableciendo los demás al color normal
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

    //metodo para verificar el progreso del árbol de decisiones, comprobando si todas las selecciones son correctas y marcando el nivel como completado si es así
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