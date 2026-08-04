using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ClasificacionRango
{
    SinClasificar,
    Estudiante,     // < 60%
    Interno,        // 60% - 74%
    Residente,      // 75% - 84%
    Doctor,         // 85% - 94%
    Especialista    // 95% - 100%
}

public struct ResultadoClasificacion
{
    public ClasificacionRango rango;
    public string titulo;
    public float puntajeFinal;
    public float porcentajeEfectividad;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum ModoJuego { Carrera, QuickPlay, Custom }
    public ModoJuego modoActual;
    public string Dificultad;

    private bool juegoActivo = false;

    [Header("Metricas de control")]
    [SerializeField] public float TiempoJuego;
    [SerializeField] public int TotalIntentos = 0;
    [SerializeField] public int TotalReinicios = 0;
    [SerializeField] public int Aciertos = 0;
    [SerializeField] public int Fallos = 0;

    public int TotalAciertos; public int TotalFallos;

    [Header("Prefabs de Niveles")]
    [SerializeField] private GameObject prefabTrivia;
    [SerializeField] private GameObject prefabArbolDeciciones;
    [SerializeField] private GameObject prefabNv4Conceptos;
    [SerializeField] private GameObject prefabAdivinaQuien;

    [Header("Canvas")]
    [SerializeField] public Canvas canvasResultados;
    [SerializeField] private Canvas canvasVictoria;

    [Header("Textos de resultados")]
    [SerializeField] public TMP_Text textoAciertos;
    [SerializeField] public TMP_Text textoFallos;
    [SerializeField] public TMP_Text textoTiempo;
    [SerializeField] public TMP_Text textoIntentos;
    [SerializeField] public TMP_Text textoReinicios;

    [Header("Textos victoria")]
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
    [SerializeField] private float tiempoEsperadoPorPregunta = 15f; // Segundos razonables por pregunta
    [SerializeField] private float puntosPorAcierto = 100f;
    [SerializeField] private float puntosPorFallo = 30f;
    [SerializeField] private float penalizacionPorReinicio = 50f;
    [SerializeField] private float penalizacionPorSegundoExtra = 2f;

    [SerializeField] public TMP_Text textoClasificacion;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
            switch(modoActual)
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

    void Update()
    {
        if (juegoActivo)
            TiempoJuego += Time.deltaTime;

        Debug.Log("nivel actual: " + nivelActual);
    }

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

    //modo de juego 30 preguntas 
    public void IniciarModoQuickPlay()
    {
        modoActual = ModoJuego.QuickPlay;
        StopAllCoroutines();
        StartCoroutine(LoopPrincipalJuego());
    }

    public void IniciarModoCustom()
    {
        modoActual = ModoJuego.Custom;
        StopAllCoroutines();
        StartCoroutine(LoopPrincipalJuego());
    }
    

    private void ConfigurarQuickPlay()
    {
        PrepararIDs();//mezclamos los ids de patologias 
        colaPreguntas.Clear();

        //niveles disponibles 
        TipoPregunta[] tiposDisponibles = {
            TipoPregunta.Trivia,
            TipoPregunta.Arbol,
            TipoPregunta.Conceptos,
            TipoPregunta.AdivinaQuien
        };

        //generamos las 30 preguntas 
        for (int i = 0; i < 30; i++)
        {
            TipoPregunta tipoAleatorio = tiposDisponibles[Random.Range(0, tiposDisponibles.Length)];//elegimos un nivel aleatorio 
            int idPat = ObtenerID();//le agregamos una patologia aleatoria 

            if (idPat == -1)//si se acaban los ids recargamos la lista 
            {
                PrepararIDs();
                idPat = ObtenerID();
            }
            //agregamos la pregunta a la cola 
            colaPreguntas.Enqueue(new PreguntaRonda { idPatologia = idPat, tipo = tipoAleatorio });
        }
    }

    /*private IEnumerator LoopPrincipalJuego()
    {
        while (modoActual == ModoJuego.Carrera && nivelActual < 4)
        {
            TotalAciertos = 0;
            TotalFallos = 0;
            continuarCarrera = false;

            if (canvasResultados != null)
                canvasResultados.gameObject.SetActive(false);

            ConfigurarCarrera(nivelActual);
            yield return StartCoroutine(MostrarTutorialNivel(nivelActual));

            juegoActivo = true;

            while (colaPreguntas.Count > 0)
            {
                PreguntaRonda pregunta = colaPreguntas.Dequeue();
                GameObject prefab = ObtenerPrefab(pregunta.tipo);

                nivelInstanciado = Instantiate(prefab);
                ControladorPreguntas controlador = nivelInstanciado.GetComponent<ControladorPreguntas>();
                controlador.InicializarPregunta(pregunta.idPatologia);

                yield return new WaitUntil(() => controlador.finished);

                Destroy(nivelInstanciado);
            }

            if (PuedePasar())
            {
                switch (nivelActual)
                {
                    case 1: ActualizarProgreso(true, false, false); break;
                    case 2: ActualizarProgreso(true, true, false); break;
                    case 3: ActualizarProgreso(true, true, true); break;
                }

                if (ControladorGuardarDatos.Instance != null)
                {
                    ControladorGuardarDatos.Instance.GuardarPartida(CrearPartidaData(Dificultad, lv1Completado, lv2Completado, lv3Completado));
                }
            }

            TotalIntentos++;
            juegoActivo = false;

            MostrarResultados();

            continuarCarrera = false;
            yield return new WaitUntil(() => continuarCarrera);

            if (PuedePasar() && nivelActual >= 4)
            {
                nivelActual = 4;
            }
        }
    }*/
    private IEnumerator LoopPrincipalJuego()
    {
        while (!ModoFinalizado())
        {
            yield return StartCoroutine(IniciarRonda()); // se debe usar StartCoroutine para esperar a que IniciarRonda termine antes de continuar

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
                // En QuickPlay y Custom termina la partida después de mostrar resultados.
                break;
            }
        }
    }
    private IEnumerator IniciarRonda() //debe ser un enumerator si o si ya que si no se ejecuta de manera asincrona y no se puede esperar a que termine antes de continuar con el resto del loop
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
             yield return StartCoroutine(MostrarTutorialNivel(nivelActual)); // se debe usar StartCoroutine para esperar a que MostrarTutorialNivel termine antes de continuar
                break;

            case ModoJuego.QuickPlay:
                ConfigurarQuickPlay();
                break;

            case ModoJuego.Custom:
                ConfigurarCustom();
                break;
        }
    }
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
                            ActualizarProgreso(true,false,false);
                            break;

                        case 2:
                            ActualizarProgreso(true,true,false);
                            break;

                        case 3:
                            ActualizarProgreso(true,true,true);
                            break;
                    }

                    ControladorGuardarDatos.Instance.GuardarPartida(CrearPartidaData(Dificultad, lv1Completado, lv2Completado, lv3Completado));
                        
                }

                break;

            case ModoJuego.QuickPlay:
                break;

            case ModoJuego.Custom:
                break;
        }
    }

    private bool ModoFinalizado()
    {
        switch(modoActual)
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
    public void ContinuarCarrera()
    {
        continuarCarrera = true;
    }

    // #cambiar pantalla de resultados por una propia de los resultados del quickplay
    private void MostrarResultadosQuickPlay()
    {
        if (lvPass != null)
        {
            lvPass.MostrarPass(3);
        }

        if (canvasResultados != null)
        {
            canvasResultados.gameObject.SetActive(true);
            if (textoAciertos != null) textoAciertos.text = "Aciertos: " + Aciertos;
            if (textoFallos != null) textoFallos.text = "Fallos: " + Fallos;

            int minutos = Mathf.FloorToInt(TiempoJuego / 60);
            int segundos = Mathf.FloorToInt(TiempoJuego % 60);
            if (textoTiempo != null) textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");

            if (textoIntentos != null) textoIntentos.text = "Partidas: " + TotalIntentos;
            if (textoReinicios != null) textoReinicios.text = "Reinicios: " + TotalReinicios;
        }
    }

    private void MostrarResultados()
    {
        if (modoActual == ModoJuego.Carrera)
        {
            // 1. Caso Victoria Final (Completó el Nivel 3 y supera el porcentaje)
            if (nivelActual == 4 && PuedePasar())
            {
                if (lvPass != null)
                {
                    // Se asume que el avance a nivel 4 indica victoria global
                    ActualizarProgreso(true, true, true); 
                }
                MostrarPantallaVictoria();
                return; // Salimos para evitar activar el canvasResultados
            }

            // 2. Transición o Reintento de Niveles Intermedios (Nivel 1, 2 o Fallo en Nivel 3)
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

            // 3. Activar Canvas de Resultados Parciales
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

            // Guardar Métricas
            if (ControladorGuardarDatos.Instance != null)
            {
                string claveNivel = nivelActual < 4 ? nivelActual.ToString() : "Carrera completada";
                ControladorGuardarDatos.Instance.GuardarMetricas(claveNivel);
            }
        }
        else
        {
            // Modos QuickPlay o Custom
            MostrarPantallaVictoria();
        }
    }

    private void MostrarPantallaVictoria()
    {
        canvasResultados.gameObject.SetActive(false);
        canvasVictoria.gameObject.SetActive(true);
        textoTotalAciertos.text = $"Aciertos de la ronda: {TotalAciertos}";
        textoTotalFallos.text = $"Fallos de la ronda: {TotalFallos}";

        ResultadoClasificacion Rc = CalcularClasificacion();

        rango.text = $"Rango: {Rc.rango} \nTítulo: {Rc.titulo} \nPuntaje: {Rc.puntajeFinal} \nPorcentaje de efectividad: {Rc.porcentajeEfectividad}";

    }

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
                    "Se mostrará una imagen y 4 alternativas, entre ellas deberás seleccionar una de ellas",
                    "Solo hay una respuesta correcta, suerte."
                );
                break;
            case 2:
                tutorial.ConfigurarTutorial(
                    "En el nivel anterior ya has aprendido a reconocer lesiones, así que vamos un paso más allá",
                    "Ahora usarás el árbol de decisiones, donde deberás enlazar la lesión con su familia, la etiopatogenia y su imagen",
                    "Crea el camino correcto hasta dar con la respuesta."
                );
                break;
            case 3:
                tutorial.ConfigurarTutorial(
                    "Finalmente has sabido relacionar cada lesión con su familia y etiopatogenia, por lo que ya estas listo para poder diagnosticar pacientes.",
                    "Aquí aparecerán distintos tipos de preguntas, en una de ellas tendrás la familia, etiopatogenia, lesión e imagen y un monton de letras con las cuales deberás escribir el diagnostico.",
                    "En el segundo habrán distintos diagnosticos y en base preguntas podrás deberás ir descartando las opciones que NO sean la correcta, tendrás una libreta donde podrás ver las respuestas que hayas optenido."
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

    private void ConfigurarCarrera(int nivel)
    {
        PrepararIDs();
        colaPreguntas.Clear();

        switch (nivel)
        {
            case 1:
                for (int i = 0; i < 5; i++)
                    colaPreguntas.Enqueue(new PreguntaRonda { idPatologia = ObtenerID(), tipo = TipoPregunta.Trivia });
                break;
            case 2:
                for (int i = 0; i < 5; i++)
                    colaPreguntas.Enqueue(new PreguntaRonda { idPatologia = ObtenerID(), tipo = TipoPregunta.Arbol });
                break;
            case 3:
                for (int i = 0; i < 5; i++)
                {
                    TipoPregunta tipoRandom = Random.value > 0.5f ? TipoPregunta.Conceptos : TipoPregunta.AdivinaQuien;
                    colaPreguntas.Enqueue(new PreguntaRonda { idPatologia = ObtenerID(), tipo = tipoRandom });
                }
                break;
        }
    }


    private void ConfigurarCustom()
    {
        PrepararIDs();
        colaPreguntas.Clear();

        List<TipoPregunta> tiposDisponibles = new List<TipoPregunta>();

        if (ConfiguracionPartida.Lesiones)
        {
            tiposDisponibles.Add(TipoPregunta.Trivia);
        }

        if (ConfiguracionPartida.FamiliasEtiopatogenias)
        {
            tiposDisponibles.Add(TipoPregunta.Arbol);
        }

        if (ConfiguracionPartida.Diagnosticos)
        {
            tiposDisponibles.Add(TipoPregunta.Conceptos);
            tiposDisponibles.Add(TipoPregunta.AdivinaQuien);
        }

        // Seguridad, aunque el botón ya debería impedir llegar aquí.
        if (tiposDisponibles.Count == 0)
        {
            Debug.LogError("No hay tipos de preguntas seleccionados.");
            return;
        }

        for (int i = 0; i < ConfiguracionPartida.CantidadPreguntas; i++)
        {
            TipoPregunta tipo =
                tiposDisponibles[Random.Range(0, tiposDisponibles.Count)];

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
    private void PrepararIDs()
    {
        List<int> ids = new List<int>();
        foreach (var p in CsvManager.Instance.patologias)
        {
            ids.Add(p.id);
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

    private int ObtenerID()
    {
        if (idsDisponibles.Count == 0) return -1;
        return idsDisponibles.Dequeue();
    }

    private GameObject ObtenerPrefab(TipoPregunta tipo)
    {
        switch (tipo)
        {
            case TipoPregunta.Trivia: return prefabTrivia;
            case TipoPregunta.Arbol: return prefabArbolDeciciones;
            case TipoPregunta.Conceptos: return prefabNv4Conceptos;
            case TipoPregunta.AdivinaQuien: return prefabAdivinaQuien;
        }
        return null;
    }

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

    private Partida CrearPartidaData(string dificulta, bool lv1, bool lv2, bool lv3)
    {
        return new Partida(dificulta,modoActual.ToString() ,lv1, lv2, lv3);
    }

    public void PausarJuego()
    {
        juegoActivo = false;
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        juegoActivo = true;
        Time.timeScale = 1f;
    }

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

    private float ObtenerPorcentajeRequerido()
    {
        string dif = Dificultad != null ? Dificultad.Trim().ToLower() : "";
        switch (dif)
        {
            case "practicante": return 0.80f;
            case "asistente": return 0.85f;
            case "experto": return 0.90f;
            default: return 0.80f;
        }
    }

    private bool PuedePasar()
    {
        int totalRespuestas = Aciertos + Fallos;
        float porcentajeAciertos = totalRespuestas > 0 ? (float)Aciertos / totalRespuestas : 0f;
        return porcentajeAciertos >= ObtenerPorcentajeRequerido();
    }

    


    

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

        // 1. Puntaje Máximo Teórico posible (100% de aciertos sin penalizaciones)
        float puntajeMaximo = totalPreguntas * puntosPorAcierto;

        // 2. Puntaje Base (Aciertos - Fallos)
        float puntajeObtenido = (Aciertos * puntosPorAcierto) - (Fallos * puntosPorFallo);

        // 3. Penalización por Reinicios
        float deduccionReinicios = TotalReinicios * penalizacionPorReinicio;
        puntajeObtenido -= deduccionReinicios;

        // 4. Penalización por Tiempo Excesivo
        float tiempoObjetivo = totalPreguntas * tiempoEsperadoPorPregunta;
        if (TiempoJuego > tiempoObjetivo)
        {
            float segundosExceso = TiempoJuego - tiempoObjetivo;
            float deduccionTiempo = segundosExceso * penalizacionPorSegundoExtra;
            puntajeObtenido -= deduccionTiempo;
        }

        // Aseguramos que el puntaje no sea menor a cero
        puntajeObtenido = Mathf.Max(0f, puntajeObtenido);

        // 5. Calculamos el porcentaje de efectividad respecto al puntaje máximo
        float porcentajeEfectividad = (puntajeObtenido / puntajeMaximo) * 100f;

        // 6. Asignación de Rango según el porcentaje resultante
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