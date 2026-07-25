using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

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

    [SerializeField] public Canvas canvasResultados;

    [Header("Textos de resultados")]
    [SerializeField] public TMP_Text textoAciertos;
    [SerializeField] public TMP_Text textoFallos;
    [SerializeField] public TMP_Text textoTiempo;
    [SerializeField] public TMP_Text textoIntentos;
    [SerializeField] public TMP_Text textoReinicios;
    [Header("Canvas LvPass")]
    [SerializeField] private Canvas canvasLvPass;
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
    private bool continuarCarrera = false;
    [SerializeField] private LvPass lvPass;

    void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {
        ControladorGuardarDatos.Instance.EliminarPartida();
        Debug.Log($"Nivel actual: {nivelActual}");
        if(ControladorGuardarDatos.Instance.ExistePartida())
        {
            ActualizarProgreso(ControladorGuardarDatos.Instance.CargarPartida().Lv1Completado,
            ControladorGuardarDatos.Instance.CargarPartida().Lv2Completado,
            ControladorGuardarDatos.Instance.CargarPartida().Lv3Completado);
        }
        else
            nivelActual = 1;
        TiempoJuego = 0;
        modoActual = ConfiguracionPartida.Modo;
        Dificultad = ConfiguracionPartida.Dificultad;
        Debug.Log($"Modo: {modoActual}");
        Debug.Log($"Dificultad: {Dificultad}");
        lvPass = GetComponent<LvPass>();
        if(SceneManager.GetActiveScene().name == "MainSecene")
            IniciarModoCarrera();

        // IniciarModoCarrera(); // Comentado para mostrar menú primero
    }

    private GameObject tutorialInstanciado; // Guardamos la referencia para destruirlo si reiniciamos

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

        // Damos 1 frame de espera para que la UI de TextMeshPro en el prefab se despierte e inicialice limpia
        yield return null;

        tutorial.ActivarTutorial();

        // Esperamos a que finalice comprobando que el objeto aún siga existiendo
        yield return new WaitUntil(() => tutorial == null || tutorial.Finalizado);

        if (tutorialInstanciado != null)
        {
            Destroy(tutorialInstanciado);
            tutorialInstanciado = null;
        }
    }

    private void ActualizarProgreso(bool lv1, bool lv2, bool lv3)
    {
        // Mantener consistencia del progreso

        if (!lv1)
        {
            lv2 = false;
            lv3 = false;
        }
        else if (!lv2)
        {
            lv3 = false;
        }

        if (lv3)
        {
            lv1 = true;
            lv2 = true;
        }
        else if (lv2)
        {
            lv1 = true;
        }

        lv1Completado = lv1;
        lv2Completado = lv2;
        lv3Completado = lv3;

        // Calcular el nivel que corresponde jugar

        if (!lv1Completado)
            nivelActual = 1;
        else if (!lv2Completado)
            nivelActual = 2;
        else if (!lv3Completado)
            nivelActual = 3;
        else
            nivelActual = 4; // Carrera completada

        Debug.Log($"Nivel actual: {nivelActual}");
    }

    

    
    private Partida CrearPartidaData(string dificulta, bool lv1, bool lv2, bool lv3)
    {
        return new Partida(dificulta, lv1,lv2,lv3);
    }
    

    void Update()
    {
        if (juegoActivo)
            TiempoJuego += Time.deltaTime;
    }

    public void IniciarModoCarrera()
    {

        // ocultamos la pantalla de inicio al empezar el los niveles
        

        modoActual = ModoJuego.Carrera;
        //juegoActivo = true;
        ConfigurarJuego(ModoJuego.Carrera);

        StartCoroutine(LoopDeJuegoCorrutina());
    }

    public void ConfigurarJuego(ModoJuego modo)
    {
        switch (modo)
        {
            case ModoJuego.Carrera:
                ConfigurarCarrera(nivelActual);
                break;
            case ModoJuego.QuickPlay:
                break;
            case ModoJuego.Custom:
                break;
            default:
                break;
        }
        // NOTA: Quitamos 'StartCoroutine(LoopDeJuegoCorrutina());' de aquí 
        // para controlar explícitamente cuándo arranca.
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
        if (idsDisponibles.Count == 0)
        {
            Debug.LogError("No quedan ids");
            return -1;
        }
        return idsDisponibles.Dequeue();
    }

    private void ConfigurarCarrera(int nivel)
    {
        PrepararIDs();
        Debug.Log($"Se esta configurando el modo modo carrera con nivel: {nivel}");
        switch(nivel)
        {
            case 1: 
                for (int i = 0; i < 5; i++)
                {
                    colaPreguntas.Enqueue(new PreguntaRonda
                    {
                        idPatologia = ObtenerID(),
                        tipo = TipoPregunta.Trivia
                    });
                }
                break;
            case 2: 

                for (int i = 0; i < 5; i++)
                {
                    Debug.Log($"cargando arbol: {i}");
                    colaPreguntas.Enqueue(new PreguntaRonda
                    {
                        
                        idPatologia = ObtenerID(),
                        tipo = TipoPregunta.Arbol
                    });
                }
                break;
            case 3:
                for (int i = 0; i < 5; i++)
                {
                    TipoPregunta tipoRandom = Random.value > 0.5f ? TipoPregunta.Conceptos : TipoPregunta.AdivinaQuien;
                    colaPreguntas.Enqueue(new PreguntaRonda
                    {
                        idPatologia = ObtenerID(),
                        tipo = tipoRandom
                    });
                }
                break;
            default:
                break;
        }
        // NIVEL 1: trivia (5 preguntas)
        /*for (int i = 0; i < 5; i++)
        {
            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = ObtenerID(),
                tipo = TipoPregunta.Trivia
            });
        }
        /*
        // NIVEL 2: arbol (5 preguntas)
        for (int i = 0; i < 5; i++)
        {
            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = ObtenerID(),
                tipo = TipoPregunta.Arbol
            });
        }

        // NIVEL 3: Concepto
        for (int i = 0; i < 5; i++)
        {
            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = ObtenerID(),
                tipo = TipoPregunta.Conceptos
            });
        }*/

        // nivel 3: concepto mezclado con adivina quien
        /*
        for (int i = 0; i < 5; i++)
        {
            TipoPregunta tipoRandom = Random.value > 0.5f ? TipoPregunta.Conceptos : TipoPregunta.AdivinaQuien;
            colaPreguntas.Enqueue(new PreguntaRonda
            {
                idPatologia = ObtenerID(),
                tipo = tipoRandom
            });
        }
        */

    }

    private void ConfigurarQuickPlay()
    {
        TipoPregunta[] tipos =
        {
            TipoPregunta.Trivia,
            TipoPregunta.Arbol,
            TipoPregunta.Conceptos,
            TipoPregunta.AdivinaQuien
        };

        TipoPregunta elegida = tipos[Random.Range(0, tipos.Length)];
    }

    private void ConfigurarCustom()
    {

    }

    private GameObject ObtenerPrefab(TipoPregunta tipo)
    {
        switch (tipo)
        {
            case TipoPregunta.Trivia:
                return prefabTrivia;
            case TipoPregunta.Arbol:
                return prefabArbolDeciciones;
            case TipoPregunta.Conceptos:
                return prefabNv4Conceptos;
            case TipoPregunta.AdivinaQuien:
                return prefabAdivinaQuien;
        }
        return null;
    }

    private IEnumerator LoopDeJuegoCorrutina()
    {
        canvasResultados.gameObject.SetActive(false);

        yield return StartCoroutine(MostrarTutorialNivel(nivelActual));
        juegoActivo = true;
        Debug.Log($"cola preguntas: {colaPreguntas.Count}");
        while (colaPreguntas.Count > 0)
        {
            PreguntaRonda pregunta = colaPreguntas.Dequeue();
            GameObject prefab = ObtenerPrefab(pregunta.tipo);
            Debug.Log(prefab.name);
            nivelInstanciado = Instantiate(prefab);
            Debug.Log(nivelInstanciado.name);
            ControladorPreguntas controlador = nivelInstanciado.GetComponent<ControladorPreguntas>();
            Debug.Log(controlador.GetType().Name);
            Debug.Log("Antes de inicializar");
            controlador.InicializarPregunta(pregunta.idPatologia);
            Debug.Log("Despues de inicializar");
            Debug.Log("Esperando...");
            yield return new WaitUntil(() => controlador.finished);
            Debug.Log("Pregunta terminada");
            Destroy(nivelInstanciado);
        }
        if(modoActual == ModoJuego.Carrera)
        {
            switch(nivelActual)
            {
                case 1:
                    ActualizarProgreso(true, false, false);
                    ControladorGuardarDatos.Instance.GuardarPartida(CrearPartidaData(dificultadSeleccionada, lv1Completado,lv2Completado, lv3Completado));
                    Debug.Log("se llamo a guardar para: " + nivelActual);
                    break;
                case 2: 
                    ActualizarProgreso(true, true, false);
                    ControladorGuardarDatos.Instance.GuardarPartida(CrearPartidaData(dificultadSeleccionada, lv1Completado,lv2Completado, lv3Completado));
                    Debug.Log("se llamo a guardar para: " + nivelActual);
                    break;
                case 3:
                    ActualizarProgreso(true, true, true);
                    ControladorGuardarDatos.Instance.GuardarPartida(CrearPartidaData(dificultadSeleccionada, lv1Completado,lv2Completado, lv3Completado));
                    Debug.Log("se llamo a guardar para: " + nivelActual);
                    break;
                default:
                    break;
            }
           
        }
        TotalIntentos++;
        yield return StartCoroutine(TerminarRonda());

        if (nivelActual < 4)
        {
            ConfigurarCarrera(nivelActual);
            juegoActivo = true;
            StartCoroutine(LoopDeJuegoCorrutina());
        }
    }

    public void ContinuarCarrera()
    {
        
        continuarCarrera = true;
    }
    private void MostrarResultados()
    {
        canvasResultados.gameObject.SetActive(true);
        textoAciertos.text = "Aciertos: " + TotalAciertos;
        textoFallos.text = "Fallos: " + TotalFallos;
        int minutos = Mathf.FloorToInt(TiempoJuego / 60);
        int segundos = Mathf.FloorToInt(TiempoJuego % 60);
        textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");
        textoIntentos.text = "Intentos: " + TotalIntentos;
        textoReinicios.text = "Reinicios: " + TotalReinicios;
        canvasResultados.gameObject.SetActive(true);
        if(nivelActual< 4)
            ControladorGuardarDatos.Instance.GuardarMetricas(nivelActual.ToString());
        else
            ControladorGuardarDatos.Instance.GuardarMetricas("Carrera completada");
    }

    private IEnumerator TerminarRonda()
    {
        juegoActivo = false;

        MostrarResultados();
        lvPass.Mostrar(nivelActual);

        TiempoJuego = 0;

        if (nivelActual < 4)
        {
            
            continuarCarrera = false;

            yield return new WaitUntil(() => continuarCarrera);

            
        }
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
        Debug.Log("Reiniciando Partida...");

        // ¡CRUCIAL! Asegurar que el tiempo del juego no esté pausado
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

        ConfigurarJuego(modoActual);
        StartCoroutine(LoopDeJuegoCorrutina());
    }
    
}