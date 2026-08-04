using UnityEngine;
using System.IO;
using System;
using System.Collections;
public class ControladorGuardarDatos : MonoBehaviour
{
    private string rutaPartida;
    private string rutaUsuario;
    public static ControladorGuardarDatos Instance;
    private bool intentandoSincronizar = false; // booleano para evitar múltiples intentos de sincronización al mismo tiempo
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
        StartCoroutine(VerificarYSincronizarUsuarioPendiente()); // inicimos una corrutina para verificar si hay un usuario pendiente de sincronizar con Firebase
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Corrutina para verificar si hay un usuario pendiente de sincronizar con Firebase
    private IEnumerator VerificarYSincronizarUsuarioPendiente()
    {
        // Esperamos a que Firebase esté listo
        while (!FirebaseInit.IsReady)
        {
            yield return new WaitForSeconds(1f);
        }

        // Si ya tenemos usuario creado y su número es 0 (fue creado offline), procedemos a registrarlo en Firestore
        if (ExisteUsuario() && !intentandoSincronizar)
        {
            Usuario usuarioLocal = CargarUsuario();
            if (usuarioLocal != null && usuarioLocal.NumeroJugador == 0)
            {
                intentandoSincronizar = true;
                Debug.Log("Internet detectado: Sincronizando usuario offline temporal con Firestore...");

                ConexionFirestore.Instance.ReservarNumeroJugador(numeroJugadorReal =>
                {
                    usuarioLocal.NumeroJugador = numeroJugadorReal;

                    ConexionFirestore.Instance.RegistrarData(
                        usuarioLocal,
                        "usuarios",
                        idFirestore =>
                        {
                            GuardarUsuario(usuarioLocal);
                            Debug.Log($"¡Usuario sincronizado exitosamente con Firestore! Nuevo ID Jugador: {numeroJugadorReal}");
                            intentandoSincronizar = false;
                        });
                });
            }
        }
    }

    // Corrutina para crear un usuario cuando Firebase esté listo
    public IEnumerator CrearUsuarioCuandoFirebaseEsteListo(string nick)
    {
        if (ExisteUsuario())
        {
            Debug.Log("Ya existe usuario.");
            yield break;
        }

        Debug.Log("Firebase listo? " + FirebaseInit.IsReady);

        float tiempoEspera = 0f;
        float limiteEspera = 4f;

        while (!FirebaseInit.IsReady && tiempoEspera < limiteEspera)
        {
            tiempoEspera += Time.deltaTime;
            yield return null;
        }
        if (!FirebaseInit.IsReady)
        {
            Usuario usuarioVacio = new Usuario();
            usuarioVacio.NumeroJugador = 0; 
            usuarioVacio.Nick = !string.IsNullOrWhiteSpace(nick) ? nick : "Invitado";
            GuardarUsuario(usuarioVacio);
            yield break;
        }
            yield return new WaitUntil(() => FirebaseInit.IsReady);

        Debug.Log("Firebase ya está listo");
        CrearUsuario(nick);
    }

    public void CrearUsuario(string nick)
    {
        if(ExisteUsuario())
        {
            Debug.Log("Existe usuario retornando desde el crear");
            return;
        }

        if (!FirebaseInit.IsReady)
        {
            Debug.Log("Sin internet al intentar crear usuario. Guardando local offline.");
            Usuario usuarioVacio = new Usuario();
            usuarioVacio.NumeroJugador = 0;
            usuarioVacio.Nick = !string.IsNullOrWhiteSpace(nick) ? nick : "Invitado";
            GuardarUsuario(usuarioVacio);
            return;
        }

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
                    Debug.Log("Se va a llamar a guardar usuario");
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
        Usuario usuarioActual = CargarUsuario();// Cargamos el usuario actual desde el archivo local
        if (usuarioActual == null || usuarioActual.NumeroJugador == 0) // Si el usuario no está sincronizado con Firestore (NumeroJugador = 0), no podemos enviar métricas
        {
            Debug.Log("El usuario actual no está sincronizado con Firestore (NumeroJugador = 0)");
            return;
        }


        Metricas metricas = new Metricas();

        metricas.Id = Guid.NewGuid().ToString();

        metricas.Nivel = nivel;

        metricas.ModoJuego = GameManager.Instance.modoActual;

        metricas.NumeroJugador = CargarUsuario().NumeroJugador;

        metricas.TiempoJuego = GameManager.Instance.TiempoJuego;

        metricas.TotalIntentos = GameManager.Instance.TotalIntentos;

        metricas.TotalReinicios = GameManager.Instance.TotalReinicios;

        metricas.TotalAciertos = GameManager.Instance.Aciertos;

        metricas.TotalFallos = GameManager.Instance.Fallos;


        if(!FirebaseInit.IsReady)
        {
            Debug.Log("no hay internet");
            return;
        }

        ConexionFirestore.Instance.RegistrarData(
            metricas,
            "metricas");
    }
}