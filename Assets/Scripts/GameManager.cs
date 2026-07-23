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

    private Queue<PreguntaRonda> colaPreguntas = new Queue<PreguntaRonda>();
    private Queue<int> idsDisponibles = new Queue<int>();

    private int nivelActual;
    private bool lv1Completado;
    private bool lv2Completado;
    private bool lv3Completado;
    private string dificultadSeleccionada;

    private GameObject nivelInstanciado;

    void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {
        TiempoJuego = 0;
        modoActual = ConfiguracionPartida.Modo;
        Dificultad = ConfiguracionPartida.Dificultad;
        Debug.Log($"Modo: {modoActual}");
        Debug.Log($"Dificultad: {Dificultad}");
        if(SceneManager.GetActiveScene().name == "MainSecene")
            IniciarModoCarrera();

        // IniciarModoCarrera(); // Comentado para mostrar menú primero
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
        juegoActivo = true;
        ConfigurarJuego(ModoJuego.Carrera);
    }

    public void ConfigurarJuego(ModoJuego modo)
    {
        switch (modo)
        {
            case ModoJuego.Carrera:
                ConfigurarCarrera();
                break;
            case ModoJuego.QuickPlay:
                break;
            case ModoJuego.Custom:
                break;
            default:
                break;
        }
        StartCoroutine(LoopDeJuegoCorrutina());
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

    private void ConfigurarCarrera()
    {
        PrepararIDs();

        // NIVEL 1: trivia (5 preguntas)
        for (int i = 0; i < 5; i++)
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
        TerminarRonda();
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

    private void TerminarRonda()
    {
        juegoActivo = false;
        MostrarResultados();
        TiempoJuego = 0;
        Debug.Log("¡Ronda Terminada! Mostrando pantalla de resultados.");
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
        StopAllCoroutines();
        if(nivelInstanciado != null)
            Destroy(nivelInstanciado);

        juegoActivo = false;

        TiempoJuego = 0;
        TotalIntentos = 0;
        TotalReinicios++;
        TotalAciertos = 0;
        TotalFallos = 0;

        colaPreguntas.Clear();
        idsDisponibles.Clear();

        ConfigurarJuego(modoActual);
    }
}