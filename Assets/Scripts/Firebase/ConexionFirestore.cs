using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Collections;

/// <summary>
/// Gestiona la conexión con Firebase Firestore y la persistencia de datos remotos como partidas, 
/// estadísticas de usuario, contadores autoincrementables y métricas de juego. 
/// Implementa el patrón Singleton.
/// 
/// Clases dependientes que utiliza:
/// - FirebaseInit: Utilizada para verificar la inicialización, obtener la instancia activa de Firestore y recuperar el ID del usuario autenticado.
/// - IFirestoreData: Interfaz requerida por el método de registro de datos para transformar objetos en diccionarios compatibles con Firestore.
/// </summary>
public class ConexionFirestore : MonoBehaviour
{
    private static ConexionFirestore _instance;

    /// <summary>
    /// Acceso global Singleton a la instancia de ConexionFirestore. Si no existe, crea un GameObject dedicado.
    /// </summary>
    public static ConexionFirestore Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ConexionFirestore");
                _instance = go.AddComponent<ConexionFirestore>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private FirebaseFirestore db;
    private bool isReady = false;

    /// <summary>
    /// Garantiza la unicidad del Singleton al despertar el componente.
    /// </summary>
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Inicia el proceso de vinculación con Firebase en el primer frame.
    /// </summary>
    void Start() => Inicializar();

    /// <summary>
    /// Comprueba recursivamente si FirebaseInit está listo para asignar la referencia de la base de datos Firestore.
    /// </summary>
    public void Inicializar()
    {
        if (FirebaseInit.IsReady)
        {
            db = FirebaseInit.Db;
            isReady = true;
            Debug.Log("ConexionFirestore lista");
        }
        else
        {
            Invoke(nameof(Inicializar), 0.5f);
        }
    }

    /// <summary>
    /// Guarda el registro detallado de una partida en la subcolección del usuario actual dentro de Firestore.
    /// </summary>
    /// <param name="nivel">Nombre o id del nivel jugado.</param>
    /// <param name="patologiaID">ID de la patología asociada.</param>
    /// <param name="patologiaOK">Indica si la respuesta de patología fue correcta.</param>
    /// <param name="etiologiaOK">Indica si la respuesta de etiología fue correcta.</param>
    /// <param name="familiaOK">Indica si la respuesta de familia fue correcta.</param>
    /// <param name="lesionOK">Indica si la respuesta de lesión fue correcta.</param>
    /// <param name="errores">Cantidad total de errores cometidos.</param>
    /// <param name="tiempo">Tiempo empleado en segundos.</param>
    /// <param name="erroresDetalle">Estructura con el desglose de los errores.</param>
    /// <param name="respuestas">Estructura con el detalle de las respuestas seleccionadas.</param>
    public void GuardarPartida(
        string nivel,
        int patologiaID,
        bool patologiaOK,
        bool etiologiaOK,
        bool familiaOK,
        bool lesionOK,
        int errores,
        float tiempo,
        List<Dictionary<string, object>> erroresDetalle,
        List<Dictionary<string, object>> respuestas)
    {
        if (!VerificarConexion()) return;

        var datos = new Dictionary<string, object>()
        {
            { "nivel", nivel },
            { "fecha", DateTime.Now.ToString("yyyy-MM-dd") },
            { "hora", DateTime.Now.ToString("HH:mm:ss") },
            { "patologia_id", patologiaID },
            { "patologia_correcta", patologiaOK },
            { "etiologia_correcta", etiologiaOK },
            { "familia_correcta", familiaOK },
            { "lesion_correcta", lesionOK },
            { "errores", errores },
            { "tiempo_segundos", Mathf.RoundToInt(tiempo) },
            { "completado", patologiaOK && etiologiaOK && familiaOK && lesionOK },
            { "errores_detalle", erroresDetalle },
            { "respuestas", respuestas }
        };

        string userId = FirebaseInit.GetUserId();
        DocumentReference docRef = db.Collection("usuarios").Document(userId)
                                     .Collection("partidas").Document();

        docRef.SetAsync(datos).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
            }
            else
            {
                ActualizarEstadisticasUsuario();
            }
        });
    }

    /// <summary>
    /// Recalcula el total de partidas e historia de completado del usuario y actualiza su documento principal en Firestore.
    /// </summary>
    private void ActualizarEstadisticasUsuario()
    {
        if (!VerificarConexion()) return;

        string userId = FirebaseInit.GetUserId();

        db.Collection("usuarios").Document(userId)
          .Collection("partidas").GetSnapshotAsync().ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted) return;

              int total = task.Result.Count;
              int completadas = 0;

              foreach (var doc in task.Result.Documents)
              {
                  if (doc.Exists && doc.TryGetValue("completado", out bool completado) && completado)
                  {
                      completadas++;
                  }
              }

              var stats = new Dictionary<string, object>()
              {
                  { "total_partidas", total },
                  { "partidas_completadas", completadas },
                  { "ultima_actualizacion", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
              };

              db.Collection("usuarios").Document(userId).UpdateAsync(stats);
          });
    }

    /// <summary>
    /// Muestra por consola la información contextual referente a un error cometido durante el juego.
    /// </summary>
    /// <param name="nivel">Identificador del nivel.</param>
    /// <param name="tipo">Tipo de opción fallada.</param>
    /// <param name="indiceSeleccionado">Índice del elemento marcado.</param>
    /// <param name="seleccionado">Valor del elemento marcado.</param>
    /// <param name="correcto">Valor que correspondía a la respuesta correcta.</param>
    /// <param name="erroresAcumulados">Cantidad acumulada de fallos.</param>
    public void GuardarError(string nivel, string tipo, int indiceSeleccionado, string seleccionado, string correcto, int erroresAcumulados)
    {
        Debug.Log($"Error registrado: {tipo} - {seleccionado} (correcto: {correcto})");
    }

    /// <summary>
    /// Comprueba la disponibilidad de la conexión a Firebase e inicializa la referencia a la base de datos de ser necesario.
    /// </summary>
    /// <returns>True si Firebase y Firestore están listos; de lo contrario, False.</returns>
    private bool VerificarConexion()
    {
        if (!FirebaseInit.IsReady)
        {
            Debug.Log("Firebase no está listo (Modo Offline o sin conexión).");
            return false;
        }
        if (db == null)
        {
            db = FirebaseInit.Db;
            if (db == null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Solicita secuencialmente la asignación de un número correlativo global de jugador.
    /// </summary>
    /// <param name="callback">Acción a ejecutar retornando el número asignado.</param>
    public void ReservarNumeroJugador(Action<int> callback)
    {
        StartCoroutine(EsperarYReservarNumeroJugador(callback));
    }

    /// <summary>
    /// Corrutina encargada de esperar la disponibilidad del servicio y ejecutar una transacción atómica 
    /// en Firestore para incrementar y obtener el contador de jugadores.
    /// </summary>
    /// <param name="callback">Acción a invocar tras completar la transacción.</param>
    private IEnumerator EsperarYReservarNumeroJugador(Action<int> callback)
    {
        float tiempoEspera = 0f;
        float limiteEspera = 4f;

        while (!FirebaseInit.IsReady && tiempoEspera < limiteEspera)
        {
            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        if (!FirebaseInit.IsReady)
        {
            yield break;
        }

        if (db == null)
        {
            db = FirebaseInit.Db;
        }

        if (db == null)
        {
            yield break;
        }

        DocumentReference contadorRef = db.Collection("config").Document("contador");

        db.RunTransactionAsync(async transaction =>
        {
            DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorRef);

            int ultimoNumero = 0;

            if (snapshot.Exists && snapshot.ContainsField("ultimoNumeroJugador"))
            {
                ultimoNumero = snapshot.GetValue<int>("ultimoNumeroJugador");
            }

            int siguienteNumero = ultimoNumero + 1;

            transaction.Update(contadorRef, "ultimoNumeroJugador", siguienteNumero);

            return siguienteNumero;
        })
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                return;
            }

            callback?.Invoke(task.Result);
        });
    }

    /// <summary>
    /// Registra un nuevo documento en la colección especificada a partir de un objeto que implemente IFirestoreData.
    /// </summary>
    /// <param name="data">Instancia del objeto transformable a datos de Firestore.</param>
    /// <param name="collection">Nombre de la colección destino.</param>
    /// <param name="onSuccess">Callback opcional que recibe el ID del nuevo documento generado.</param>
    public void RegistrarData(IFirestoreData data, string collection, Action<string> onSuccess = null)
    {
        if (!VerificarConexion())
            return;

        DocumentReference doc = db.Collection(collection).Document();

        doc.SetAsync(data.ToFirestore()).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                return;
            }

            onSuccess?.Invoke(doc.Id);
        });
    }
}