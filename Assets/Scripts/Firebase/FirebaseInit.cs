using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestiona la verificación de dependencias e inicialización de los servicios del SDK de Firebase 
/// (Firestore y Analytics) en Unity, además de exponer propiedades globales sobre el estado de la conexión.
/// 
/// Clases dependientes que utiliza:
/// - FirebaseApp: Permite validar y reparar el entorno/dependencias nativas de Firebase en la plataforma de ejecución.
/// - FirebaseFirestore: Inicializa la base de datos remota para su consumo estático global por parte de otras clases del proyecto.
/// - FirebaseAnalytics: Servicio habilitado para la recolección y análisis de métricas de telemetría de la aplicación.
/// </summary>
public class FirebaseInit : MonoBehaviour
{
    /// <summary>
    /// Indica si las dependencias de Firebase se validaron e inicializaron correctamente.
    /// </summary>
    public static bool IsReady { get; private set; } = false;

    /// <summary>
    /// Instancia estática global de la base de datos Firebase Firestore.
    /// </summary>
    public static FirebaseFirestore Db { get; private set; }

    /// <summary>
    /// Identificador único del usuario configurado dentro de Firebase.
    /// </summary>
    public static string UserId { get; private set; } = "";

    /// <summary>
    /// Nombre o alias del usuario configurado.
    /// </summary>
    public static string UserName { get; private set; } = "";

    /// <summary>
    /// Indica si el servicio de Firebase Analytics está activo e inicializado.
    /// </summary>
    public static bool AnalyticsEnabled { get; private set; } = false;

    /// <summary>
    /// Asigna persistencia al GameObject a través de las escenas y desencadena la inicialización del SDK de Firebase.
    /// </summary>
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InicializarFirebase();
    }

    /// <summary>
    /// Comprueba de forma asíncrona las dependencias nativas e inicializa Firestore y Analytics si están disponibles.
    /// </summary>
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

    /// <summary>
    /// Registra en Firestore la fecha y hora de la conexión más reciente del usuario.
    /// </summary>
    private void ActualizarConexion()
    {
        if (!IsReady || Db == null) return;

        var datos = new Dictionary<string, object>()
        {
            { "ultima_conexion", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        Db.Collection("usuarios").Document(UserId).UpdateAsync(datos);
    }

    /// <summary>
    /// Retorna el identificador del usuario.
    /// </summary>
    /// <returns>Identificador de usuario (UserId).</returns>
    public static string GetUserId() => UserId;

    /// <summary>
    /// Retorna el nombre del usuario.
    /// </summary>
    /// <returns>Nombre del usuario (UserName).</returns>
    public static string GetUserName() => UserName;
}