using UnityEngine;
using System.Collections.Generic;
using System.Collections; 
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum ModoJuego { Carrera, QuickPlay, Custom }
    public ModoJuego modoActual;

    private bool juegoActivo = false;

    [Header("Metricas de control")]
    [SerializeField] public float TiempoJuego;
    
    [SerializeField] public int TotalIntentos = 0;
    [SerializeField] public int TotalReinicios = 0;

    [SerializeField] public int TotalAciertos = 0;
    [SerializeField] public int TotalFallos = 0;

    [Header("Prefabs de Niveles")]
    // Cambiamos a tipo ControladorPreguntas para acceder directo a sus funciones
    [SerializeField] private GameObject prefabTrivia;
    [SerializeField] private GameObject prefabArbolDeciciones;
    [SerializeField] private GameObject prefabNv4Conceptos;
    [SerializeField] private GameObject prefabAdivinaQuien;
    [SerializeField] public Canvas canvasResultados;
    [Header("Textos de resultados")]
    [SerializeField] public TMP_Text textoAciertos;//cuantos aciertos tuvo el jugador
    [SerializeField] public TMP_Text textoFallos;//cuantos fallos tuvo el jugador
    [SerializeField] public TMP_Text textoTiempo;//cuanto tiempo tomo completar la pregunta
    [SerializeField] public TMP_Text textoIntentos;//cuantos intentos tuvo el jugador
    [SerializeField] public TMP_Text textoReinicios;//cuantos reinicios tuvo el jugador

   

    private Queue<PreguntaRonda> colaPreguntas = new Queue<PreguntaRonda>();
    private Queue<int> idsDisponibles = new Queue<int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    void Start()
    {
        TiempoJuego = 0;
        IniciarModoCarrera();//por ahora prueba
    }
    void Update()
    {
        if(juegoActivo)
            TiempoJuego += Time.deltaTime;
    }

    // Ejemplo de cómo iniciarías el juego desde tu menú
    public void IniciarModoCarrera()
    {
        modoActual = ModoJuego.Carrera;
        juegoActivo = true;
        ConfigurarJuego(ModoJuego.Carrera);
        
    }

    public void ConfigurarJuego(ModoJuego modo)//resivira mas parametros dependiendo del modo
    {
        

        
        switch(modo)
        {
            case ModoJuego.Carrera:
                ConfigurarCarrera(1);
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
        foreach(var p in CsvManager.Instance.patologias)
        {
            ids.Add(p.id);
        }
        //fisher-Yates pa revolver
        for(int i = 0; i < ids.Count; i++)
        {
            int random = Random.Range(i, ids.Count);
            int temp = ids[i];
            ids[i] = ids[random];
            ids[random] = temp;
        }
        idsDisponibles.Clear();
        foreach(int id in ids)
        {
            idsDisponibles.Enqueue(id);
        }
    }

    private int ObtenerID()
    {
        if(idsDisponibles.Count == 0)
        {
            Debug.LogError("No quedan ids");
            return -1;
        }
        return idsDisponibles.Dequeue();
    }

    private void ConfigurarCarrera(int nivel)
    {
        PrepararIDs();
        switch(nivel)
        {
            case 1:
                for(int i = 0; i < 5; i++)
                {
                    colaPreguntas.Enqueue(
                        new PreguntaRonda
                        {
                            idPatologia = ObtenerID(),
                            tipo = TipoPregunta.Trivia
                        });
                }
                break;
            case 2:
                for(int i = 0; i < 5; i++)
                {
                    colaPreguntas.Enqueue(
                        new PreguntaRonda
                        {
                            idPatologia = ObtenerID(),
                            tipo = TipoPregunta.Arbol
                        });
                }
                break;
            case 3:
                for(int i = 0; i < 5; i++)
                {
                    TipoPregunta tipoRandom = Random.value > 0.5f? TipoPregunta.Conceptos: TipoPregunta.AdivinaQuien;
                    colaPreguntas.Enqueue(
                        new PreguntaRonda
                        {
                            idPatologia = ObtenerID(),
                            tipo = tipoRandom
                        });
                }
                break;
        }
        
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
        switch(tipo)
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

    // Este es el verdadero Loop que controla el flujo por turnos
    private IEnumerator LoopDeJuegoCorrutina()
    {
    
    
        while(colaPreguntas.Count > 0)
        {
            PreguntaRonda pregunta = colaPreguntas.Dequeue();
            GameObject prefab = ObtenerPrefab(pregunta.tipo);
            GameObject nivelInstanciado = Instantiate(prefab);
            ControladorPreguntas controlador = nivelInstanciado.GetComponent<ControladorPreguntas>();
            controlador.InicializarPregunta(pregunta.idPatologia);
            yield return new WaitUntil(() => controlador.finished);
            Destroy(nivelInstanciado);
            
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
    }

    private void TerminarRonda()
    {
        juegoActivo = false;
        MostrarResultados();
        TiempoJuego = 0;
        Debug.Log("¡Ronda Terminada! Mostrando pantalla de resultados.");
        
    }
}