using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Reflection;

/// <summary>
/// Controlador principal encargado de la gestión, persistencia local (JSON) y sincronización remota (Firestore) 
/// de los datos del juego, incluyendo la información de usuario, partidas guardadas y métricas de rendimiento.
/// 
/// Clases dependientes que utiliza:
/// - FirebaseInit: Utilizada para comprobar la disponibilidad y estado de conexión de Firebase/Firestore.
/// - ConexionFirestore: Servicio para registrar usuarios, reservar identificadores y enviar métricas a la base de datos remota.
/// - GameManager: Fuente de datos para extraer tiempos, intentos, aciertos, fallos y el modo de juego actual.
/// - Usuario: Modelo de datos que almacena la información del jugador (nickname, número asignado y estado de la partida).
/// - Partida: Modelo de datos que representa el progreso local guardado en disco.
/// - Metricas: Modelo de datos que empaqueta las estadísticas de desempeño en un nivel para Firestore.
/// </summary>
public class ControladorGuardarDatos : MonoBehaviour
{
    private string rutaPartida;
    private string rutaUsuario;
    public static ControladorGuardarDatos Instance;
    private bool intentandoSincronizar = false;

    /// <summary>
    /// Configura las rutas de almacenamiento persistente en disco e inicializa el patrón Singleton.
    /// </summary>
    private void Awake()
    {
        rutaPartida = Path.Combine(Application.persistentDataPath, "partida.json");
        rutaUsuario = Path.Combine(Application.persistentDataPath, "usuario.json");
        Debug.Log("Ruta partida: " + rutaPartida);
        Debug.Log("RutaUsuario: " + rutaUsuario);
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// Inicia los procesos de verificación de datos pendientes al comenzar la ejecución del script.
    /// </summary>
    void Start()
    {
        StartCoroutine(VerificarYSincronizarUsuarioPendiente());
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Evalúa de forma asíncrona la conexión a Firebase para registrar en Firestore a aquellos usuarios creados en modo offline.
    /// </summary>
    private IEnumerator VerificarYSincronizarUsuarioPendiente()
    {
        while (!FirebaseInit.IsReady)
        {
            yield return new WaitForSeconds(1f);
        }

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

    /// <summary>
    /// Espera de forma asíncrona a que el servicio de Firebase esté disponible para registrar al usuario o guardarlo localmente si expira el tiempo límite.
    /// </summary>
    /// <param name="nick">Nombre o apodo del usuario a registrar.</param>
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

    /// <summary>
    /// Crea un nuevo perfil de usuario, reservando un identificador en Firestore si hay conexión o creándolo en modo offline en disco local.
    /// </summary>
    /// <param name="nick">Nombre o apodo asignado al nuevo usuario.</param>
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
            if(ExistePartida())
                usuario.PartidaTerminada = CargarPartida().Lv3Completado;
            else
                usuario.PartidaTerminada = false;

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
                });
        });
    }

    /// <summary>
    /// Modifica los campos del usuario en el archivo local y reescribe la información.
    /// </summary>
    /// <param name="nick">Nuevo nombre o apodo del usuario.</param>
    /// <param name="partida">Estado de finalización de la partida.</param>
    public void ActualizarUsuario(string nick, bool partida)
    {
        Usuario u = CargarUsuario();
        u.Nick = nick;
        u.PartidaTerminada = partida;
        GuardarUsuario(u);
    }

    /// <summary>
    /// Serializa el objeto Usuario a formato JSON y lo guarda en el disco local.
    /// </summary>
    /// <param name="usuario">Instancia de Usuario a guardar.</param>
    public void GuardarUsuario(Usuario usuario)
    {
        string json = JsonUtility.ToJson(usuario, true);

        File.WriteAllText(rutaUsuario, json);

        Debug.Log("Usuario guardado en: " + rutaUsuario);
    }

    /// <summary>
    /// Lee y deserializa la información del archivo JSON local correspondiente al usuario.
    /// </summary>
    /// <returns>El objeto Usuario leído o null si el archivo no existe.</returns>
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

    /// <summary>
    /// Comprueba la existencia del archivo del usuario guardado en el almacenamiento local.
    /// </summary>
    /// <returns>True si el archivo existe; en caso contrario, False.</returns>
    public bool ExisteUsuario()
    {
        return File.Exists(rutaUsuario);
    }

    /// <summary>
    /// Serializa el objeto Partida a formato JSON y lo escribe en el almacenamiento local.
    /// </summary>
    /// <param name="partida">Instancia de la partida a almacenar.</param>
    public void GuardarPartida(Partida partida)
    {
        string json = JsonUtility.ToJson(partida, true);

        File.WriteAllText(rutaPartida, json);

        Debug.Log("Partida guardada en: " + rutaPartida);
    }

    /// <summary>
    /// Lee y deserializa el archivo JSON local de la partida guardada.
    /// </summary>
    /// <returns>El objeto Partida deserializado o null si no existe.</returns>
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

    /// <summary>
    /// Comprueba si existe un archivo de partida guardada en el almacenamiento local.
    /// </summary>
    /// <returns>True si existe el archivo de partida; de lo contrario, False.</returns>
    public bool ExistePartida()
    {
        return File.Exists(rutaPartida);
    }

    /// <summary>
    /// Elimina el archivo de partida guardada en la ruta local si este existe.
    /// </summary>
    public void EliminarPartida()
    {
        if (File.Exists(rutaPartida))
        {
            File.Delete(rutaPartida);
            Debug.Log("Partida eliminada.");
        }
    }

    /// <summary>
    /// Método reservado para el envío general de datos.
    /// </summary>
    public void EnviarData()
    {
        
    }

    /// <summary>
    /// Recopila las estadísticas actuales del GameManager y las envía a la colección de métricas en Firestore.
    /// </summary>
    /// <param name="nivel">Nombre o identificador del nivel evaluado.</param>
    public void GuardarMetricas(string nivel)
    {
        Usuario usuarioActual = CargarUsuario();
        if (usuarioActual == null || usuarioActual.NumeroJugador == 0)
        {
            Debug.Log("El usuario actual no está sincronizado con Firestore (NumeroJugador = 0)");
            return;
        }

        Metricas metricas = new Metricas();

        metricas.Id = Guid.NewGuid().ToString();

        metricas.TipoNivel = nivel;

        metricas.ModoJuego = GameManager.Instance.modoActual;

        metricas.NumeroJugador = CargarUsuario().NumeroJugador;

        metricas.TiempoJuego = FormatearTiempo(GameManager.Instance.TiempoJuego);

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

    //hacemos un metodo para formatear el tiempo en segundos a un string con minutos y segundos
    private string FormatearTiempo(float tiempoEnSegundos)
    {
        //aproximamos el tiempo a minutos y segundos
        int minutos = Mathf.FloorToInt(tiempoEnSegundos / 60f);
        int segundos = Mathf.FloorToInt(tiempoEnSegundos % 60f);

        if (minutos > 0)
            return $"{minutos}m {segundos}s";
        else
            return $"{segundos}s";
    }

}