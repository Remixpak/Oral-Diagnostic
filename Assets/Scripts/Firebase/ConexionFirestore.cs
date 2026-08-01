using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Collections;

public class ConexionFirestore : MonoBehaviour
{
    private static ConexionFirestore _instance;
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

    void Start() => Inicializar();

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

    public void GuardarError(string nivel, string tipo, int indiceSeleccionado, string seleccionado, string correcto, int erroresAcumulados)
    {
        Debug.Log($"Error registrado: {tipo} - {seleccionado} (correcto: {correcto})");
    }

    private bool VerificarConexion() // Verifica si FirebaseInit está listo y si la base de datos está inicializada
    {
        if (!FirebaseInit.IsReady)
        {
            // Silenciamos el error rojo intrusivo si es por modo offline intencional
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

    public void ReservarNumeroJugador(Action<int> callback)
    {
        StartCoroutine(EsperarYReservarNumeroJugador(callback));
    }

    private IEnumerator EsperarYReservarNumeroJugador(Action<int> callback)
    {
        float tiempoEspera = 0f;// tiempo minimo que esperamos la conexion
        float limiteEspera = 4f; // tiempo maximo que esperamos la conexion

        // esperamos hasta que FirebaseInit esté listo o se alcance el límite de espera
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