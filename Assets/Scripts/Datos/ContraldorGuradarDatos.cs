using UnityEngine;
using System.IO;
using System;
using System.Collections;
public class ControladorGuardarDatos : MonoBehaviour
{
    private string rutaPartida;
    private string rutaUsuario;
    public static ControladorGuardarDatos Instance;

    private void Awake()
    {
        rutaPartida = Path.Combine(Application.persistentDataPath, "partida.json");
        rutaUsuario = Path.Combine(Application.persistentDataPath, "usuario.json");
        Debug.Log("Ruta partida: " + rutaPartida);
        Debug.Log("RutaUsuario: " + rutaUsuario);
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator CrearUsuarioCuandoFirebaseEsteListo(string nick)
    {
        if(ExisteUsuario())
            yield break;
        yield return new WaitUntil(() => FirebaseInit.IsReady);

        CrearUsuario(nick);
    }

    public void CrearUsuario(string nick)
    {
        if(ExisteUsuario())
            return;
        ConexionFirestore.Instance.ReservarNumeroJugador(numeroJugador =>
        {
            Usuario usuario = new Usuario();

            usuario.NumeroJugador = numeroJugador;
            usuario.Nick = nick;

            ConexionFirestore.Instance.RegistrarData(
                usuario,
                "usuarios",
                idFirestore =>
                {
                    GuardarUsuario(usuario);
                    Debug.Log($"Usuario creado.");

                    Debug.Log($"Jugador: {numeroJugador}");

                    Debug.Log($"Id Firestore: {idFirestore}");

                    // Aquí puedes guardar el usuario en tu JSON local
                });
        });
    }
    public void GuardarUsuario(Usuario usuario)
    {
        string json = JsonUtility.ToJson(usuario, true);

        File.WriteAllText(rutaUsuario, json);

        Debug.Log("Usuario guardado en: " + rutaUsuario);
    }
    public Usuario CargarUsuario()
    {
        if (!File.Exists(rutaUsuario))
        {
            Debug.Log("No existe un usuario guardado.");
            return null;
        }

        string json = File.ReadAllText(rutaUsuario);

        Usuario usuario = JsonUtility.FromJson<Usuario>(json);

        Debug.Log("Usuario cargado.");

        return usuario;
    }
    public bool ExisteUsuario()
    {
        return File.Exists(rutaUsuario);
    }
    public void GuardarPartida(Partida partida)
    {
        string json = JsonUtility.ToJson(partida, true);

        File.WriteAllText(rutaPartida, json);

        Debug.Log("Partida guardada en: " + rutaPartida);
    }

    public Partida CargarPartida()
    {
        if (!File.Exists(rutaPartida))
        {
            Debug.Log("No existe una partida guardada.");
            return null;
        }

        string json = File.ReadAllText(rutaPartida);

        Partida partida = JsonUtility.FromJson<Partida>(json);

        Debug.Log("Partida cargada.");

        return partida;
    }

    public bool ExistePartida()
    {
        return File.Exists(rutaPartida);
    }

    public void EliminarPartida()
    {
        if (File.Exists(rutaPartida))
        {
            File.Delete(rutaPartida);
            Debug.Log("Partida eliminada.");
        }
    }
    public void EnviarData()
    {
        
    }
    public void GuardarMetricas(string nivel)
    {
        Metricas metricas = new Metricas();

        metricas.Id = Guid.NewGuid().ToString();

        metricas.Nivel = nivel;

        metricas.ModoJuego = GameManager.Instance.modoActual;

        metricas.NumeroJugador = CargarUsuario().NumeroJugador;

        metricas.TiempoJuego = GameManager.Instance.TiempoJuego;

        metricas.TotalIntentos = GameManager.Instance.TotalIntentos;

        metricas.TotalReinicios = GameManager.Instance.TotalReinicios;

        metricas.TotalAciertos = GameManager.Instance.TotalAciertos;

        metricas.TotalFallos = GameManager.Instance.TotalFallos;

        ConexionFirestore.Instance.RegistrarData(
            metricas,
            "metricas");
    }
}
