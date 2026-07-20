using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;

//ola recuerda asignarme a un objeto en blanco en la escena para funcioanr! 

public class FirebaseInit : MonoBehaviour
{
    public static bool IsReady { get; private set; } = false; // Indica si Firebase se ha inicializado correctamente
    public static FirebaseFirestore Db { get; private set; } // Instancia de Firebase Firestore
    public static string UserId { get; private set; } = "";// Identificador único del usuario
    public static string UserName { get; private set; } = ""; // Nombre del usuario
    public static bool AnalyticsEnabled { get; private set; } = false; // Indica si Firebase Analytics está habilitado

    // funcion que se ejecuta al iniciar el juego, inicializa Firebase y configura el usuario
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InicializarFirebase();
    }

    // Función para inicializar Firebase y configurar el usuario
    private void InicializarFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                IsReady = true;
                Db = FirebaseFirestore.DefaultInstance;
                AnalyticsEnabled = true;

                InicializarUsuario();

                Debug.Log($"Firebase inicializado");
                Debug.Log($"Usuario: {UserName}");
            }
            else
            {
                Debug.LogError($" Error inicializando Firebase: {task.Result}");
            }
        });
    }

    // Función para inicializar el usuario, recuperando datos guardados o generando nuevos
    private void InicializarUsuario()
    {
        string userNameGuardado = PlayerPrefs.GetString("UserName", "");
        string userIdGuardado = PlayerPrefs.GetString("UserId", "");

        if (!string.IsNullOrEmpty(userNameGuardado) && !string.IsNullOrEmpty(userIdGuardado))
        {
            UserName = userNameGuardado;
            UserId = userIdGuardado;
            Debug.Log($"Usuario recuperado: {UserName}");

            ActualizarConexion();
            return;
        }

        UserName = GenerarNombreAleatorio(10);
        UserId = System.Guid.NewGuid().ToString();

        PlayerPrefs.SetString("UserName", UserName);
        PlayerPrefs.SetString("UserId", UserId);
        PlayerPrefs.Save();

        Debug.Log($"Nuevo usuario: {UserName}");
        RegistrarUsuarioEnFirestore();
    }

    // Función para generar un nombre aleatorio de longitud especificada
    private string GenerarNombreAleatorio(int longitud)
    {
        string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string nombre = "";
        for (int i = 0; i < longitud; i++)
        {
            nombre += caracteres[Random.Range(0, caracteres.Length)];
        }
        return nombre;
    }

    // Función para registrar un nuevo usuario en Firestore con datos iniciales
    private void RegistrarUsuarioEnFirestore()
    {
        if (!IsReady || Db == null) return;

        var datos = new Dictionary<string, object>()
        {
            { "nombre", UserName },
            { "fecha_registro", System.DateTime.Now.ToString("yyyy-MM-dd") },
            { "dispositivo", SystemInfo.deviceModel },
            { "version", Application.version },
            { "ultima_conexion", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "total_partidas", 0 },
            { "partidas_completadas", 0 }
        };

        Db.Collection("usuarios").Document(UserId).SetAsync(datos);
    }

    // Función para actualizar la última conexión del usuario en Firestore
    private void ActualizarConexion()
    {
        if (!IsReady || Db == null) return;

        var datos = new Dictionary<string, object>()
        {
            { "ultima_conexion", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        Db.Collection("usuarios").Document(UserId).UpdateAsync(datos);
    }

    public static string GetUserId() => UserId;
    public static string GetUserName() => UserName;
}