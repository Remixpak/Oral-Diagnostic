using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Necesario para las Corrutinas
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
    [SerializeField] private GameObject prefabNv1;
    [SerializeField] private GameObject prefabNv2;
    [SerializeField] private GameObject prefabNv3;
    [SerializeField] public Canvas canvasResultados;
    [Header("Textos de resultados")]
    [SerializeField] public TMP_Text textoAciertos;//cuantos aciertos tuvo el jugador
    [SerializeField] public TMP_Text textoFallos;//cuantos fallos tuvo el jugador
    [SerializeField] public TMP_Text textoTiempo;//cuanto tiempo tomo completar la pregunta
    [SerializeField] public TMP_Text textoIntentos;//cuantos intentos tuvo el jugador
    [SerializeField] public TMP_Text textoReinicios;//cuantos reinicios tuvo el jugador

    private List<int> idsRondaActual = new List<int>();
    private int preguntaActualIndice = 0;

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
        ConfigurarJuego(ModoJuego.Carrera, 1);
    }

    public void ConfigurarJuego(ModoJuego modo, int nivel)//resivira mas parametros dependiendo del modo
    {
        idsRondaActual.Clear();
        preguntaActualIndice = 0;

        // 1. Obtener todas las IDs disponibles en el CSV
        List<int> todasLasIds = new List<int>();
        foreach (var patologia in CsvManager.Instance.patologias)
        {
            todasLasIds.Add(patologia.id);
        }

        // 2. Barajar las IDs (Algoritmo Fisher-Yates) para que queden aleatorias pero sin repetir
        for (int i = 0; i < todasLasIds.Count; i++)
        {
            int temp = todasLasIds[i];
            int randomIndex = Random.Range(i, todasLasIds.Count);
            todasLasIds[i] = todasLasIds[randomIndex];
            todasLasIds[randomIndex] = temp;
        }

        // 3. Tomar las primeras 5 IDs únicas
        int cantidadPreguntas = Mathf.Min(5, todasLasIds.Count);//cantidad preguntas será un parametro despues para el modo custom
        for (int i = 0; i < cantidadPreguntas; i++)
        {
            idsRondaActual.Add(todasLasIds[i]);
        }

        // 4. Seleccionar el prefab del nivel correspondiente
        GameObject prefabElegido = null;
        switch (nivel)
        {
            case 1:
                prefabElegido = prefabNv1;
                break;
            case 2:
                prefabElegido = prefabNv2;
                break;
            case 3:
                prefabElegido = prefabNv3;
                break;
            default:
                Debug.LogWarning("Nivel no reconocido. Usando nivel 1 por defecto.");
                prefabElegido = prefabNv1;
                break;
        }
        // else if (nivel == 2) prefabElegido = prefabNv2; // etc...

        // 5. Arrancar el Loop de juego de forma secuencial
        if (prefabElegido != null)
        {
            StartCoroutine(LoopDeJuegoCorrutina(prefabElegido));
        }
    }

    // Este es el verdadero Loop que controla el flujo por turnos
    private IEnumerator LoopDeJuegoCorrutina(GameObject prefabNivel)
{
    
    while (preguntaActualIndice < idsRondaActual.Count)
    {
        int idPregunta = idsRondaActual[preguntaActualIndice];

        GameObject nivelInstanciado = Instantiate(prefabNivel);

        ControladorPreguntas controlador =
            nivelInstanciado.GetComponent<ControladorPreguntas>();

        controlador.InicializarPregunta(idPregunta);//le pasa la id de la patologia por la que pregunta
        /*
        por ejemplo para el nivel 1 le pasa la id de la patologia 30 
        y el controlador de nivel 1 obtiene la lesion y la imagen de la patologia 
        con ese ID
        */

        // Esperar hasta que el jugador responda
        yield return new WaitUntil(() => controlador.finished);//todos los controladores de nivel deben setear finished a true cuando el jugador responda

        Destroy(nivelInstanciado);//destruye el nivel actual para pasar al siguiente

        preguntaActualIndice++;//ahora le pasara el sigiente id de la lista de ids
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