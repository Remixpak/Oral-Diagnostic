using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Define los rangos de clasificación médica que se le pueden otorgar al jugador según su rendimiento.
/// </summary>
public enum ClasificacionRango
{
    SinClasificar,
    Estudiante,     // < 60%
    Interno,        // 60% - 74%
    Residente,      // 75% - 84%
    Doctor,         // 85% - 94%
    Especialista    // 95% - 100%
}

/// <summary>
/// Estructura de datos que almacena el desglose final de rendimiento y clasificación del jugador.
/// </summary>
public struct ResultadoClasificacion
{
    public ClasificacionRango rango;
    public string titulo;
    public float puntajeFinal;
    public float porcentajeEfectividad;
}

/// <summary>
/// Controlador principal del ciclo de vida del juego (Singleton). Coordina los modos de juego (Carrera, QuickPlay, Custom),
/// gestiona la cola de preguntas, instanciamiento de niveles/tutoriales, métricas globales, guardado de datos y el cálculo de la calificación final.
/// 
/// Clases que utiliza y su finalidad:
/// - MonoBehaviour / SceneManager (UnityEngine): Manejo de estados de Unity, corrutinas y la escena activa ("MainSecene").
/// - TMP_Text / Canvas / Image / Button (UnityEngine.UI y TMPro): Componentes de UI para renderizar diálogos, tutoriales, conteos y resultados.
/// - ControladorPreguntas: Clase base abstracta instanciada dinámicamente para inicializar y esperar la resolución de cada pregunta.
/// - ControladorGuardarDatos: Singleton de persistencia encargado de guardar/cargar partidas, actualizar usuario y almacenar métricas por categoría.
/// - CsvManager: Central de datos biomédicos de donde se extraen e identifican las patologías y lesiones.
/// - ConfiguracionPartida: Clase estática/global de configuración desde la que se leen los parámetros del modo Custom y la dificultad.
/// - Tutorial: Componente asignado al prefab de tutorial para guiar al usuario al inicio de cada nivel en modo Carrera.
/// - LvPass: Componente de interfaz UI que gestiona los mensajes de pase de nivel o reintento al finalizar un bloque.
/// - Partida / PreguntaRonda / TipoPregunta: Estructuras y enumeraciones de soporte para empaquetar el estado de juego y las preguntas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum ModoJuego { Carrera, QuickPlay, Custom }
    public ModoJuego modoActual;
    public string Dificultad;

    private bool juegoActivo = false;

    [Header("Métricas de Control")]
    [SerializeField] public float TiempoJuego;
    [SerializeField] public int TotalIntentos = 0;
    [SerializeField] public int TotalReinicios = 0;
    [SerializeField] public int Aciertos = 0;
    [SerializeField] public int Fallos = 0;

    [SerializeField] public int FLesiones;
    [SerializeField] public int FFamilias;
    [SerializeField] public int FDiagnosticos;

    public int TotalAciertos; 
    public int TotalFallos;

    [Header("Prefabs Lesiones")]
    [SerializeField] private GameObject prefabDescripciones;
    [SerializeField] private GameObject prefabLesion;
    [SerializeField] private GameObject prefabManifestaciones;

    [Header("Prefabs Familias")]
    [SerializeField] private GameObject prefabRelacionCorrecta;
    [SerializeField] private GameObject prefabFamiliaCorrespondiente;
    [SerializeField] private GameObject prefabEtiopatogeniaCorrespondiente;
    [SerializeField] private GameObject prefabEnlazeManifestacion;
    [SerializeField] private GameObject prefabAsociarSecuenciaConManifestacion;

    [Header("Prefabs Diagnósticos")]
    [SerializeField] private GameObject prefabAdivina2Preguntas;
    [SerializeField] private GameObject prefabAdivina4Preguntas;
    [SerializeField] private GameObject prefabAdivina6Preguntas;
    [SerializeField] private GameObject prefab4Conceptos;

    [Header("Canvas")]
    [SerializeField] public Canvas canvasResultados;
    [SerializeField] private Canvas canvasVictoria;

    [Header("Textos de Resultados")]
    [SerializeField] public TMP_Text textoAciertos;
    [SerializeField] public TMP_Text textoFallos;
    [SerializeField] public TMP_Text textoTiempo;
    [SerializeField] public TMP_Text textoIntentos;
    [SerializeField] public TMP_Text textoReinicios;

    [Header("Textos Victoria")]
    [SerializeField] private TMP_Text textoTotalAciertos;
    [SerializeField] private TMP_Text textoTotalFallos;
    [SerializeField] private TMP_Text rango;

    [Header("Tutorial")]
    [SerializeField] private GameObject prefabTutorial;
    private Queue<PreguntaRonda> colaPreguntas = new Queue<PreguntaRonda>();
    private Queue<int> idsDisponibles = new Queue<int>();

    private int nivelActual;
    private bool lv1Completado;
    private bool lv2Completado;
    private bool lv3Completado;
    private string dificultadSeleccionada;

    private GameObject nivelInstanciado;
    private GameObject tutorialInstanciado;
    private bool continuarCarrera = false;
    [SerializeField] private LvPass lvPass;

    [Header("Configuración de Calificación")]
    [SerializeField] private float tiempoEsperadoPorPregunta = 15f;
    [SerializeField] private float puntosPorAcierto = 100f;
    [SerializeField] private float puntosPorFallo = 30f;
    [SerializeField] private float penalizacionPorReinicio = 50f;
    [SerializeField] private float penalizacionPorSegundoExtra = 2f;

    [SerializeField] public TMP_Text textoClasificacion;

    /// <summary>
    /// Configura el patrón Singleton al despertar la instancia.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Inicializa las variables de sesión, detecta guardados existentes e inicia el loop correspondiente según el modo activo.
    /// </summary>
    void Start()
    {
        Debug.Log($"Nivel actual inicial: {nivelActual}");

        nivelActual = 1;
        lv1Completado = false;
        lv2Completado = false;
        lv3Completado = false;
        TotalAciertos = 0;
        TotalFallos = 0;

        bool hayPartida = ControladorGuardarDatos.Instance != null && ControladorGuardarDatos.Instance.ExistePartida();

        if (hayPartida)
        {
            var partida = ControladorGuardarDatos.Instance.CargarPartida();
            if (partida != null)
            {
                ActualizarProgreso(partida.Lv1Completado, partida.Lv2Completado, partida.Lv3Completado);
            }
        }

        if (nivelActual >= 4)
        {
            if (ControladorGuardarDatos.Instance != null)
            {
                ControladorGuardarDatos.Instance.EliminarPartida();
            }
            nivelActual = 1;
            lv1Completado = false;
            lv2Completado = false;
            lv3Completado = false;
            hayPartida = false;
        }

        TiempoJuego = 0;
        modoActual = ConfiguracionPartida.Modo;
        Dificultad = ConfiguracionPartida.Dificultad;

        if (lvPass == null)
        {
            lvPass = GetComponentInChildren<LvPass>();
        }

        if (SceneManager.GetActiveScene().name == "MainSecene")
        {
            switch (modoActual)
            {
                case ModoJuego.Carrera:
                    if (hayPartida)
                    {
                        IniciarModoCarrera(false);
                    }
                    else
                    {
                        IniciarModoCarrera(true);
                    }
                    break;
                case ModoJuego.QuickPlay:
                    IniciarModoQuickPlay();
                    break;
                case ModoJuego.Custom:
                    IniciarModoCustom();
                    break;
                default:
                    IniciarModoCarrera();
                    break;
            }
        }
    }

    /// <summary>
    /// Actualiza el temporizador general cuando el juego se encuentra en estado activo.
    /// </summary>
    void Update()
    {
        if (juegoActivo)
            TiempoJuego += Time.deltaTime;
    }

    /// <summary>
    /// Inicia la partida en modo Carrera, opcionalmente reseteando el progreso guardado a nivel 1.
    /// </summary>
    /// <param name="reiniciar">Indica si se deben borrar los datos del progreso actual.</param>
    public void IniciarModoCarrera(bool reiniciar = false)
    {
        if (reiniciar)
        {
            nivelActual = 1;
            lv1Completado = false;
            lv2Completado = false;
            lv3Completado = false;

            if (ControladorGuardarDatos.Instance != null)
            {
                ControladorGuardarDatos.Instance.EliminarPartida();
            }
        }

        modoActual = ModoJuego.Carrera;
        StopAllCoroutines();
        StartCoroutine(LoopPrincipalJuego());
    }

    /// <summary>
    /// Inicia una partida rápida compuesta por 30 preguntas variadas.
    /// </summary>
    public void IniciarModoQuickPlay()
    {
        modoActual = ModoJuego.QuickPlay;
        StopAllCoroutines();
        StartCoroutine(LoopPrincipalJuego());
    }

    /// <summary>
    /// Inicia una partida personalizada con la selección de temas y cantidad de preguntas predefinida en ConfiguracionPartida.
    /// </summary>
    public void IniciarModoCustom()
    {
        modoActual = ModoJuego.Custom;
        StopAllCoroutines();
        StartCoroutine(LoopPrincipalJuego());
    }

    /// <summary>
    /// Corrutina principal que orquesta la secuencia del juego: inicio de ronda, ejecución de preguntas, finalización y despliegue de resultados.
    /// </summary>
    private IEnumerator LoopPrincipalJuego()
    {
        while (!ModoFinalizado())
        {
            yield return StartCoroutine(IniciarRonda());

            yield return EjecutarPreguntas();

            FinalizarRonda();

            Debug.Log($"RESULTADOS -> A:{Aciertos} F:{Fallos}");

            MostrarResultados();

            if (modoActual == ModoJuego.Carrera)
            {
                continuarCarrera = false;
                yield return new WaitUntil(() => continuarCarrera);
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Reinicia métricas locales y prepara la cola de preguntas y tutoriales según el modo activo.
    /// </summary>
    private IEnumerator IniciarRonda()
    {
        Aciertos = 0;
        Fallos = 0;
        Debug.Log("poniendo variables en 0");
        juegoActivo = true;

        if (canvasResultados != null)
            canvasResultados.gameObject.SetActive(false);

        switch (modoActual)
        {
            case ModoJuego.Carrera:
                ConfigurarCarrera(nivelActual);
                yield return StartCoroutine(MostrarTutorialNivel(nivelActual));
                break;

            case ModoJuego.QuickPlay:
                ConfigurarQuickPlay();
                break;

            case ModoJuego.Custom:
                ConfigurarCustom();
                break;
        }
    }

    /// <summary>
    /// Procesa de manera secuencial cada pregunta de la cola instanciando su prefab y esperando a que finalice la interacción.
    /// </summary>
    private IEnumerator EjecutarPreguntas()
    {
        while (colaPreguntas.Count > 0)
        {
            PreguntaRonda pregunta = colaPreguntas.Dequeue();
            GameObject prefab = ObtenerPrefab(pregunta.tipo);

            nivelInstanciado = Instantiate(prefab);
            ControladorPreguntas controlador = nivelInstanciado.GetComponent<ControladorPreguntas>();
            controlador.InicializarPregunta(pregunta.idPatologia);

            Debug.Log($"Comienza pregunta. A:{Aciertos} F:{Fallos}");

            yield return new WaitUntil(() => controlador.finished);

            Debug.Log($"Termina pregunta. A:{Aciertos} F:{Fallos}");

            Destroy(nivelInstanciado);
        }
    }

    /// <summary>
    /// Registra el intento, evalúa si el usuario supera el nivel en modo Carrera y persiste el progreso resultante.
    /// </summary>
    private void FinalizarRonda()
    {
        TotalIntentos++;

        juegoActivo = false;

        switch (modoActual)
        {
            case ModoJuego.Carrera:

                if (PuedePasar())
                {
                    switch (nivelActual)
                    {
                        case 1:
                            ActualizarProgreso(true, false, false);
                            break;

                        case 2:
                            ActualizarProgreso(true, true, false);
                            break;

                        case 3:
                            ActualizarProgreso(true, true, true);
                            break;
                    }

                    ControladorGuardarDatos.Instance.GuardarPartida(CrearPartidaData(Dificultad, lv1Completado, lv2Completado, lv3Completado));
                    ControladorGuardarDatos.Instance.ActualizarUsuario("XXXX", lv3Completado);
                }

                break;

            case ModoJuego.QuickPlay:
                break;

            case ModoJuego.Custom:
                break;
        }
    }

    /// <summary>
    /// Comprueba si la condición de término del modo de juego actual se ha alcanzado.
    /// </summary>
    /// <returns>Verdadero si el modo actual ha finalizado.</returns>
    private bool ModoFinalizado()
    {
        switch (modoActual)
        {
            case ModoJuego.Carrera:
                return nivelActual >= 4;

            case ModoJuego.QuickPlay:
                return TotalIntentos >= 1;

            case ModoJuego.Custom:
                return TotalIntentos >= 1;
        }

        return true;
    }

    /// <summary>
    /// Bandera invocado por la UI para desbloquear la espera de continuación en el loop del modo Carrera.
    /// </summary>
    public void ContinuarCarrera()
    {
        continuarCarrera = true;
    }

    /// <summary>
    /// Controla el despliegue del canvas de resultados parciales, pases de nivel o la pantalla final de victoria según el resultado.
    /// </summary>
    private void MostrarResultados()
    {
        if (modoActual == ModoJuego.Carrera)
        {
            if (nivelActual == 4 && PuedePasar())
            {
                if (lvPass != null)
                {
                    ActualizarProgreso(true, true, true); 
                }
                MostrarPantallaVictoria();
                return;
            }

            if (lvPass != null)
            {
                if (PuedePasar())
                {
                    lvPass.MostrarPass(nivelActual);
                }
                else
                {
                    lvPass.MostrarReintento();
                }
            }
            else
            {
                Debug.LogError("La referencia a LvPass es NULL en el GameManager.");
            }

            if (canvasResultados != null)
            {
                canvasResultados.gameObject.SetActive(true);
                if (textoAciertos != null) textoAciertos.text = "Aciertos: " + Aciertos;
                if (textoFallos != null) textoFallos.text = "Fallos: " + Fallos;

                int minutos = Mathf.FloorToInt(TiempoJuego / 60);
                int segundos = Mathf.FloorToInt(TiempoJuego % 60);
                if (textoTiempo != null) textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");

                if (textoIntentos != null) textoIntentos.text = "Intentos: " + TotalIntentos;
                if (textoReinicios != null) textoReinicios.text = "Reinicios: " + TotalReinicios;
            }

            if (ControladorGuardarDatos.Instance != null)
            {
                string claveNivel;
                switch (nivelActual)
                {
                    case 1:
                        claveNivel = "Lesiones"; 
                        break;
                    case 2:
                        claveNivel = "Familias/Etiopatogenias";
                        break;
                    case 3:
                        claveNivel = "Diagnosticos";
                        break;
                    default:
                        claveNivel = "Carrera completada";
                        break;
                }
                ControladorGuardarDatos.Instance.GuardarMetricas(claveNivel);
            }
        }
        else
        {
            MostrarPantallaVictoria();
        }
    }

    /// <summary>
    /// Despliega la pantalla de victoria final calculando el rango y la efectividad conseguida.
    /// </summary>
    private void MostrarPantallaVictoria()
    {
        canvasResultados.gameObject.SetActive(false);
        canvasVictoria.gameObject.SetActive(true);
        textoTotalAciertos.text = $"Aciertos de la ronda: {TotalAciertos}";
        textoTotalFallos.text = $"Fallos de la ronda: {TotalFallos}";

        ResultadoClasificacion Rc = CalcularClasificacion();

        rango.text = $"Rango: {Rc.rango} \nTítulo: {Rc.titulo} \nPuntaje: {Rc.puntajeFinal} \nPorcentaje de efectividad: {Rc.porcentajeEfectividad}";
    }

    /// <summary>
    /// Instancia e instruye la presentación del tutorial interactivo correspondiente al nivel ingresado.
    /// </summary>
    /// <param name="nivel">Número de nivel a parametrizar.</param>
    private IEnumerator MostrarTutorialNivel(int nivel)
    {
        if (tutorialInstanciado != null)
            Destroy(tutorialInstanciado);

        tutorialInstanciado = Instantiate(prefabTutorial);
        Tutorial tutorial = tutorialInstanciado.GetComponent<Tutorial>();

        switch (nivel)
        {
            case 1:
                tutorial.ConfigurarTutorial(
                    "Bienvenido al nivel 1, El objetivo del nivel es que aprendas a reconocer lesiones de la mucosa oral",
                    "Deberás conocer las descripciones, reconocer la lesión según manifestación clínica o en base a una lesión escoger una manifestación clínica",
                    "Solo hay una respuesta correcta, suerte.",
                    "Objetivo: reconocer las lesiones de la mucosa oral."
                );
                break;
            case 2:
                tutorial.ConfigurarTutorial(
                    "En el nivel anterior ya has aprendido a reconocer lesiones, así que vamos un paso más allá",
                    "Deberás sumar a tus conocimientos las familias y las etiopatogenias de las manifestaciones clínicas",
                    "Deberás reconocer estos términos en preguntas de alternativas y enlazar conceptos para crear un camino partiendo de una manifestación clínica.",
                    "Objetivo: Asociar una manifestación clínica con su lesión básica, familia y etiopatogenia."
                );
                break;
            case 3:
                tutorial.ConfigurarTutorial(
                    "Finalmente has sabido relacionar cada lesión con su familia y etiopatogenia, por lo que ya estas listo para poder diagnosticar pacientes.",
                    "Aquí aparecerán distintos diagnosticos y en base a preguntar por los conceptos de lesión, familia y etiopatogenia deberás ir descartando opciones hasta dar con la correcta",
                    "debes escoger con mucho cuidado para que no descartes el diagnostico correcto.",
                    "Objetivo: Diagnosticar lesiones de la mucosa oral."
                );
                break;
        }

        yield return null;

        tutorial.ActivarTutorial();

        yield return new WaitUntil(() => tutorial == null || tutorial.Finalizado);

        if (tutorialInstanciado != null)
        {
            Destroy(tutorialInstanciado);
            tutorialInstanciado = null;
        }
    }

    /// <summary>
    /// Configura y encola las preguntas correspondientes al nivel asignado dentro del modo Carrera.
    /// </summary>
    /// <param name="nivel">Identificador del nivel (1 al 3).</param>
    private void ConfigurarCarrera(int nivel)
    {
        PrepararIDs();
        colaPreguntas.Clear();

        switch (nivel)
        {
            case 1: // Total: 20 preguntas
                AgregarPreguntasACola(TipoPregunta.Descripciones, 5);
                AgregarPreguntasACola(TipoPregunta.Lesion, 8);
                AgregarPreguntasACola(TipoPregunta.Manifestaciones, 7);
                break;

            case 2: // Total: 20 preguntas
                AgregarPreguntasACola(TipoPregunta.RelacionCorrecta, 3);
                AgregarPreguntasACola(TipoPregunta.FamiliaCorrespondiente, 4);
                AgregarPreguntasACola(TipoPregunta.EtiopatogeniaCorrespondiente, 4);
                AgregarPreguntasACola(TipoPregunta.EnlazeManifestacion, 5);
                AgregarPreguntasACola(TipoPregunta.AsociarSecuenciaConManifestacion, 4);
                break;

            case 3: // Total: 20 preguntas
                AgregarPreguntasACola(TipoPregunta.Adivina2Preguntas, 3);
                AgregarPreguntasACola(TipoPregunta.Adivina4Preguntas, 5);
                AgregarPreguntasACola(TipoPregunta.Adivina6Preguntas, 12);
                break;
        }
    }

    /// <summary>
    /// Genera una cola de 30 preguntas aleatorias entre todos los tipos disponibles para el modo QuickPlay.
    /// </summary>
    private void ConfigurarQuickPlay()
    {
        PrepararIDs();
        colaPreguntas.Clear();

        TipoPregunta[] todosLosTipos = (TipoPregunta[])System.Enum.GetValues(typeof(TipoPregunta));

        int totalPreguntasQuickPlay = 30;

        for (int i = 0; i < totalPreguntasQuickPlay; i++)
        {
            TipoPregunta tipoAleatorio = todosLosTipos[UnityEngine.Random.Range(0, todosLosTipos.Length)];

            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = ObtenerID(),
                tipo = tipoAleatorio
            });
        }
    }

    /// <summary>
    /// Construye una ronda de preguntas personalizada en base a las opciones seleccionadas por el usuario en ConfiguracionPartida.
    /// </summary>
    private void ConfigurarCustom()
    {
        PrepararIDs();
        colaPreguntas.Clear();

        List<TipoPregunta> tiposDisponibles = new List<TipoPregunta>();

        if (ConfiguracionPartida.Lesiones)
        {
            tiposDisponibles.Add(TipoPregunta.Descripciones);
            tiposDisponibles.Add(TipoPregunta.Lesion);
            tiposDisponibles.Add(TipoPregunta.Manifestaciones);
        }

        if (ConfiguracionPartida.FamiliasEtiopatogenias)
        {
            tiposDisponibles.Add(TipoPregunta.RelacionCorrecta);
            tiposDisponibles.Add(TipoPregunta.FamiliaCorrespondiente);
            tiposDisponibles.Add(TipoPregunta.EtiopatogeniaCorrespondiente);
            tiposDisponibles.Add(TipoPregunta.EnlazeManifestacion);
            tiposDisponibles.Add(TipoPregunta.AsociarSecuenciaConManifestacion);
        }

        if (ConfiguracionPartida.Diagnosticos)
        {
            tiposDisponibles.Add(TipoPregunta.Adivina2Preguntas);
            tiposDisponibles.Add(TipoPregunta.Adivina4Preguntas);
            tiposDisponibles.Add(TipoPregunta.Adivina6Preguntas);
            tiposDisponibles.Add(TipoPregunta.CuatroConceptos);
        }

        if (tiposDisponibles.Count == 0)
        {
            Debug.LogError("No hay tipos de preguntas seleccionados.");
            return;
        }

        for (int i = 0; i < ConfiguracionPartida.CantidadPreguntas; i++)
        {
            TipoPregunta tipo = tiposDisponibles[Random.Range(0, tiposDisponibles.Count)];

            int idPat = ObtenerID();

            if (idPat == -1)
            {
                PrepararIDs();
                idPat = ObtenerID();
            }

            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = idPat,
                tipo = tipo
            });
        }
    }

    /// <summary>
    /// Agrega una cantidad determinada de preguntas de un tipo específico a la cola activa.
    /// </summary>
    /// <param name="tipo">Tipo de pregunta a encolar.</param>
    /// <param name="cantidad">Cantidad de preguntas a instanciar.</param>
    private void AgregarPreguntasACola(TipoPregunta tipo, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = ObtenerID(),
                tipo = tipo
            });
        }
    }

    /// <summary>
    /// Filtra y desordena aleatoriamente los identificadores de patologías válidas (excluyendo combinaciones con '/') cargadas por CsvManager.
    /// </summary>
    private void PrepararIDs()
    {
        List<int> ids = new List<int>();

        foreach (var p in CsvManager.Instance.patologias)
        {
            Lesion lesion = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);

            if (lesion != null && !lesion.nombre.Contains("/"))
            {
                ids.Add(p.id);
            }
        }

        for (int i = 0; i < ids.Count; i++)
        {
            int random = Random.Range(i, ids.Count);
            int temp = ids[i];
            ids[i] = ids[random];
            ids[random] = temp;
        }

        idsDisponibles.Clear();
        foreach (int id in ids)
        {
            idsDisponibles.Enqueue(id);
        }
    }

    /// <summary>
    /// Desencola el siguiente identificador de patología disponible.
    /// </summary>
    /// <returns>El identificador numérico o -1 si la cola está vacía.</returns>
    private int ObtenerID()
    {
        if (idsDisponibles.Count == 0) return -1;
        return idsDisponibles.Dequeue();
    }

    /// <summary>
    /// Mapea la enumeración TipoPregunta con su respectivo Prefab asignado en el Inspector.
    /// </summary>
    /// <param name="tipo">El tipo de pregunta solicitado.</param>
    /// <returns>El GameObject del prefab correspondiente.</returns>
    private GameObject ObtenerPrefab(TipoPregunta tipo)
    {
        switch (tipo)
        {
            case TipoPregunta.Descripciones: return prefabDescripciones;
            case TipoPregunta.Lesion: return prefabLesion;
            case TipoPregunta.Manifestaciones: return prefabManifestaciones;

            case TipoPregunta.RelacionCorrecta: return prefabRelacionCorrecta;
            case TipoPregunta.FamiliaCorrespondiente: return prefabFamiliaCorrespondiente;
            case TipoPregunta.EtiopatogeniaCorrespondiente: return prefabEtiopatogeniaCorrespondiente;
            case TipoPregunta.EnlazeManifestacion: return prefabEnlazeManifestacion;
            case TipoPregunta.AsociarSecuenciaConManifestacion: return prefabAsociarSecuenciaConManifestacion;

            case TipoPregunta.Adivina2Preguntas: return prefabAdivina2Preguntas;
            case TipoPregunta.Adivina4Preguntas: return prefabAdivina4Preguntas;
            case TipoPregunta.Adivina6Preguntas: return prefabAdivina6Preguntas;
            case TipoPregunta.CuatroConceptos: return prefab4Conceptos;

            default:
                Debug.LogError($"Tipo de pregunta no mapeado: {tipo}");
                return null;
        }
    }

    /// <summary>
    /// Valida y actualiza los indicadores booleanos de progreso de niveles y ajusta el puntero nivelActual.
    /// </summary>
    /// <param name="lv1">Estado del Nivel 1.</param>
    /// <param name="lv2">Estado del Nivel 2.</param>
    /// <param name="lv3">Estado del Nivel 3.</param>
    private void ActualizarProgreso(bool lv1, bool lv2, bool lv3)
    {
        if (!lv1) { lv2 = false; lv3 = false; }
        else if (!lv2) { lv3 = false; }

        if (lv3) { lv1 = true; lv2 = true; }
        else if (lv2) { lv1 = true; }

        lv1Completado = lv1;
        lv2Completado = lv2;
        lv3Completado = lv3;

        if (!lv1Completado) nivelActual = 1;
        else if (!lv2Completado) nivelActual = 2;
        else if (!lv3Completado) nivelActual = 3;
        else nivelActual = 4;
    }

    /// <summary>
    /// Instancia una nueva estructura Partida para su serialización o guardado.
    /// </summary>
    private Partida CrearPartidaData(string dificulta, bool lv1, bool lv2, bool lv3)
    {
        return new Partida(dificulta, modoActual.ToString(), lv1, lv2, lv3);
    }

    /// <summary>
    /// Detiene la ejecución del tiempo de juego y ajusta Time.timeScale a 0.
    /// </summary>
    public void PausarJuego()
    {
        juegoActivo = false;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Restablece la escala de tiempo a normalidad y reactiva el conteo del temporizador.
    /// </summary>
    public void ReanudarJuego()
    {
        juegoActivo = true;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Limpia el estado de ejecución actual, destruye instancias dinámicas y reinicia la corrutina principal de juego.
    /// </summary>
    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();

        if (nivelInstanciado != null) Destroy(nivelInstanciado);
        if (tutorialInstanciado != null) Destroy(tutorialInstanciado);

        juegoActivo = false;
        TiempoJuego = 0;
        TotalIntentos = 0;
        TotalReinicios++;
        Aciertos = 0;
        Fallos = 0;

        colaPreguntas.Clear();
        idsDisponibles.Clear();

        StartCoroutine(LoopPrincipalJuego());
    }

    /// <summary>
    /// Retorna el umbral decimal de aciertos exigidos según la dificultad seleccionada.
    /// </summary>
    /// <returns>Valor porcentual en rango 0.0f a 1.0f.</returns>
    private float ObtenerPorcentajeRequerido()
    {
        string dif = Dificultad != null ? Dificultad.Trim().ToLower() : "";
        switch (dif)
        {
            case "fácil": return 0.70f;
            case "medio": return 0.80f;
            case "difícil": return 0.90f;
            default: return 0.80f;
        }
    }

    /// <summary>
    /// Evalúa si el porcentaje de aciertos alcanzado en la ronda cumple con la dificultad establecida.
    /// </summary>
    /// <returns>Verdadero si el usuario aprueba la ronda.</returns>
    private bool PuedePasar()
    {
        int totalRespuestas = Aciertos + Fallos;
        float porcentajeAciertos = totalRespuestas > 0 ? (float)Aciertos / totalRespuestas : 0f;
        return porcentajeAciertos >= ObtenerPorcentajeRequerido();
    }

    /// <summary>
    /// Procesa métricas de tiempo, reintentos y aciertos/fallos para dictaminar la clasificación final y rango del jugador.
    /// </summary>
    /// <returns>Estructura ResultadoClasificacion con el desglose final.</returns>
    public ResultadoClasificacion CalcularClasificacion()
    {
        int totalPreguntas = Aciertos + Fallos;

        if (totalPreguntas == 0)
        {
            return new ResultadoClasificacion 
            { 
                rango = ClasificacionRango.SinClasificar, 
                titulo = "Sin Datos", 
                puntajeFinal = 0f, 
                porcentajeEfectividad = 0f 
            };
        }

        float puntajeMaximo = totalPreguntas * puntosPorAcierto;
        float puntajeObtenido = (Aciertos * puntosPorAcierto) - (Fallos * puntosPorFallo);

        float deduccionReinicios = TotalReinicios * penalizacionPorReinicio;
        puntajeObtenido -= deduccionReinicios;

        float tiempoObjetivo = totalPreguntas * tiempoEsperadoPorPregunta;
        if (TiempoJuego > tiempoObjetivo)
        {
            float segundosExceso = TiempoJuego - tiempoObjetivo;
            float deduccionTiempo = segundosExceso * penalizacionPorSegundoExtra;
            puntajeObtenido -= deduccionTiempo;
        }

        puntajeObtenido = Mathf.Max(0f, puntajeObtenido);

        float porcentajeEfectividad = (puntajeObtenido / puntajeMaximo) * 100f;

        ClasificacionRango rangoObtenido;
        string tituloTexto;

        if (porcentajeEfectividad >= 95f)
        {
            rangoObtenido = ClasificacionRango.Especialista;
            tituloTexto = "Especialista Sobresaliente";
        }
        else if (porcentajeEfectividad >= 85f)
        {
            rangoObtenido = ClasificacionRango.Doctor;
            tituloTexto = "Doctor Titulado";
        }
        else if (porcentajeEfectividad >= 75f)
        {
            rangoObtenido = ClasificacionRango.Residente;
            tituloTexto = "Residente Senior";
        }
        else if (porcentajeEfectividad >= 60f)
        {
            rangoObtenido = ClasificacionRango.Interno;
            tituloTexto = "Interno en Práctica";
        }
        else
        {
            rangoObtenido = ClasificacionRango.Estudiante;
            tituloTexto = "Estudiante de Odontología";
        }

        return new ResultadoClasificacion
        {
            rango = rangoObtenido,
            titulo = tituloTexto,
            puntajeFinal = puntajeObtenido,
            porcentajeEfectividad = porcentajeEfectividad
        };
    }
}