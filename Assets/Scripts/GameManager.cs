using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Necesario para las Corrutinas

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum ModoJuego { Carrera, QuickPlay, Custom }
    public ModoJuego modoActual;

    [Header("Metricas de control")]
    [SerializeField] private int TiempoJuego;
    [SerializeField] private int TotalAciertos;
    [SerializeField] private int TotalFallos;
    [SerializeField] private int TotalIntentos;
    [SerializeField] private int TotalReinicios;

    [Header("Prefabs de Niveles")]
    // Cambiamos a tipo ControladorPreguntas para acceder directo a sus funciones
    [SerializeField] private GameObject prefabNv1;
    //[SerializeField] private ControladorPreguntas prefabNv2;
    //[SerializeField] private ControladorPreguntas prefabNv3;

    //[Header("Contenedor de la UI")]
    //[SerializeField] private Transform contenedorPreguntas; // Donde se spawnearán en tu Canvas

    private List<int> idsRondaActual = new List<int>();
    private int preguntaActualIndice = 0;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    void Start()
    {
        IniciarModoCarrera(1);
    }

    // Ejemplo de cómo iniciarías el juego desde tu menú
    public void IniciarModoCarrera(int nivel)
    {
        modoActual = ModoJuego.Carrera;
        ConfigurarJuegoCarrera(nivel);
    }

    public void ConfigurarJuegoCarrera(int nivel)
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
        int cantidadPreguntas = Mathf.Min(5, todasLasIds.Count);
        for (int i = 0; i < cantidadPreguntas; i++)
        {
            idsRondaActual.Add(todasLasIds[i]);
        }

        // 4. Seleccionar el prefab del nivel correspondiente
        GameObject prefabElegido = null;
        if (nivel == 1) prefabElegido = prefabNv1;
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
        switch (modoActual)
        {
            case ModoJuego.Carrera:
                for (preguntaActualIndice = 0; preguntaActualIndice < idsRondaActual.Count; preguntaActualIndice++)
                {
                    // Instanciar el prefab del nivel
                    GameObject nivelInstanciado = Instantiate(prefabNivel);
                    ControladorPreguntas controlador = nivelInstanciado.GetComponent<ControladorPreguntas>();

                    yield return null;
                }
                break;

            case ModoJuego.QuickPlay:
                // Implementar lógica para QuickPlay si es necesario
                break;

            case ModoJuego.Custom:
                // Implementar lógica para Custom si es necesario
                break;
        }
    }

    private void TerminarRonda()
    {
        Debug.Log("¡Ronda Terminada! Mostrando pantalla de resultados.");
        // Aquí activas tu UI de victoria o puntajes finales
    }
}