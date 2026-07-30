using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] public int TotalAciertos = 0;
    [SerializeField] public int TotalFallos = 0;

    [Header("Prefabs de Niveles")]
    [SerializeField] private GameObject prefabTrivia;
    [SerializeField] private GameObject prefabArbolDeciciones;
    [SerializeField] private GameObject prefabNv4Conceptos;
    [SerializeField] private GameObject prefabAdivinaQuien;

    [Header("Resultados")]
    [SerializeField] public Canvas canvasResultados;

    [Header("Textos de resultados")]
    [SerializeField] public TMP_Text textoAciertos;
    [SerializeField] public TMP_Text textoFallos;
    [SerializeField] public TMP_Text textoTiempo;
    [SerializeField] public TMP_Text textoIntentos;
    [SerializeField] public TMP_Text textoReinicios;

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
        if (ControladorGuardarDatos.Instance != null && ControladorGuardarDatos.Instance.ExistePartida())
        {
            var partida = ControladorGuardarDatos.Instance.CargarPartida();
            ActualizarProgreso(partida.Lv1Completado, partida.Lv2Completado, partida.Lv3Completado);
        }
        else
        {
            nivelActual = 1;
        }

        if (nivelActual >= 4)
        {
            Debug.Log("[GameManager] El juego ya estaba completado. Reiniciando desde nivel 1...");

            if (ControladorGuardarDatos.Instance != null)
            {
                ControladorGuardarDatos.Instance.EliminarPartida();
            }

            nivelActual = 1;
            lv1Completado = false;
            lv2Completado = false;
            lv3Completado = false;
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
            if (modoActual == ModoJuego.QuickPlay)
            {
                IniciarModoQuickPlay();
            }
            else
            {
                IniciarModoCarrera();
            }
        }
    }

    void Update()
    {
        if (juegoActivo)
            TiempoJuego += Time.deltaTime;
    }

    public void IniciarModoCarrera()
    {
        modoActual = ModoJuego.Carrera;
        StopAllCoroutines();
        StartCoroutine(LoopPrincipalJuego());
    }

    //modo de juego 30 preguntas 
    public void IniciarModoQuickPlay()
    {
        modoActual = ModoJuego.QuickPlay;
        StopAllCoroutines();
        StartCoroutine(LoopQuickPlay());
    }

    private IEnumerator LoopQuickPlay()
    {
        //reiniciamos las metricas 
        TotalAciertos = 0;
        TotalFallos = 0;
        continuarCarrera = false;

        //ocultamos el canvas de resultados anterior 
        if (canvasResultados != null)
            canvasResultados.gameObject.SetActive(false);

        ConfigurarQuickPlay();//generamos las 30 preguntas 

        juegoActivo = true;

        while (colaPreguntas.Count > 0)
        {
            //tomamos la pregunta de una cola 
            PreguntaRonda pregunta = colaPreguntas.Dequeue();
            GameObject prefab = ObtenerPrefab(pregunta.tipo);
            //instaciamos el prefab del nivel 
            nivelInstanciado = Instantiate(prefab);
            ControladorPreguntas controlador = nivelInstanciado.GetComponent<ControladorPreguntas>();
            controlador.InicializarPregunta(pregunta.idPatologia);

            yield return new WaitUntil(() => controlador.finished); //esperamos a que el jugador termine 

            Destroy(nivelInstanciado);
        }
        //mostramos resultados 
        TotalIntentos++;
        juegoActivo = false;

        MostrarResultadosQuickPlay();
        //esperemos a que el jugador apachurre el continuar 
        continuarCarrera = false;
        yield return new WaitUntil(() => continuarCarrera);
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

    private IEnumerator LoopPrincipalJuego()
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
            if (textoAciertos != null) textoAciertos.text = "Aciertos: " + TotalAciertos;
            if (textoFallos != null) textoFallos.text = "Fallos: " + TotalFallos;

            int minutos = Mathf.FloorToInt(TiempoJuego / 60);
            int segundos = Mathf.FloorToInt(TiempoJuego % 60);
            if (textoTiempo != null) textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");

            if (textoIntentos != null) textoIntentos.text = "Partidas: " + TotalIntentos;
            if (textoReinicios != null) textoReinicios.text = "Reinicios: " + TotalReinicios;
        }
    }

    private void MostrarResultados()
    {
        if (lvPass != null)
        {
            if (PuedePasar())
                lvPass.MostrarPass(nivelActual);
            else
                lvPass.MostrarReintento();
        }
        else
        {
            Debug.LogError("¡Atención! La referencia a LvPass es NULL en el GameManager.");
        }

        if (canvasResultados != null)
        {
            canvasResultados.gameObject.SetActive(true);
            if (textoAciertos != null) textoAciertos.text = "Aciertos: " + TotalAciertos;
            if (textoFallos != null) textoFallos.text = "Fallos: " + TotalFallos;

            int minutos = Mathf.FloorToInt(TiempoJuego / 60);
            int segundos = Mathf.FloorToInt(TiempoJuego % 60);
            if (textoTiempo != null) textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");

            if (textoIntentos != null) textoIntentos.text = "Intentos: " + TotalIntentos;
            if (textoReinicios != null) textoReinicios.text = "Reinicios: " + TotalReinicios;
        }

        if (ControladorGuardarDatos.Instance != null)
        {
            string claveNivel = nivelActual < 4 ? nivelActual.ToString() : "Carrera completada";
            ControladorGuardarDatos.Instance.GuardarMetricas(claveNivel);
        }
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
        return new Partida(dificulta, lv1, lv2, lv3);
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
        TotalAciertos = 0;
        TotalFallos = 0;

        colaPreguntas.Clear();
        idsDisponibles.Clear();

        if (modoActual == ModoJuego.QuickPlay)
            StartCoroutine(LoopQuickPlay());
        else
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
        int totalRespuestas = TotalAciertos + TotalFallos;
        float porcentajeAciertos = totalRespuestas > 0 ? (float)TotalAciertos / totalRespuestas : 0f;
        return porcentajeAciertos >= ObtenerPorcentajeRequerido();
    }
}