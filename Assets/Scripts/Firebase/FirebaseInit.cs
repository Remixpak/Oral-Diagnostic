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