using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Gestiona la preferencia global del modo zurdo/diestro en la interfaz de usuario, alternando
/// la visibilidad entre pares de GameObjects de botones asignados y guardando la preferencia localmente.
/// Implementa el patrón Singleton.
/// 
/// Clases dependientes que utiliza:
/// - PlayerPrefs: Utilizada para guardar y recuperar la preferencia del usuario ("ModoZurdo") de forma local e intradominio.
/// - ParBotones: Estructura serializable interna que asocia un objeto de botón para diestro con su alternativa para zurdo.
/// </summary>
public class ControladorModoZurdo : MonoBehaviour
{
    /// <summary>
    /// Acceso global Singleton a la instancia activa del ControladorModoZurdo.
    /// </summary>
    public static ControladorModoZurdo Instance { get; private set; }

    /// <summary>
    /// Estructura contenedora que vincula la versión diestra y zurda de un elemento de interfaz.
    /// </summary>
    [System.Serializable]
    public class ParBotones
    {
        /// <summary>
        /// Objeto del botón orientado a la disposición predeterminada (diestra).
        /// </summary>
        public GameObject botonDiestro;

        /// <summary>
        /// Objeto del botón orientado a la disposición adaptada (zurda).
        /// </summary>
        public GameObject botonZurdo;
    }

    [Header("Pares de botones")]
    [SerializeField] private List<ParBotones> pares = new List<ParBotones>();

    private bool modoZurdo = false;

    /// <summary>
    /// Configura la instancia Singleton, carga el estado persistente del modo zurdo y aplica la visibilidad inicial.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        modoZurdo = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;
        AplicarModoZurdo(modoZurdo);
    }

    /// <summary>
    /// Establece explícitamente el estado del modo zurdo, persiste la elección en PlayerPrefs y actualiza la interfaz.
    /// </summary>
    /// <param name="activado">True para activar el modo zurdo, False para el modo diestro.</param>
    public void ActivarModoZurdo(bool activado)
    {
        if (modoZurdo == activado) return;

        modoZurdo = activado;
        PlayerPrefs.SetInt("ModoZurdo", activado ? 1 : 0);
        PlayerPrefs.Save();
        AplicarModoZurdo(activado);
    }

    /// <summary>
    /// Consulta si el modo zurdo está actualmente activo.
    /// </summary>
    /// <returns>True si el modo zurdo está habilitado; de lo contrario, False.</returns>
    public bool EsModoZurdo() => modoZurdo;

    /// <summary>
    /// Invierte el estado actual del modo zurdo (de activo a inactivo o viceversa).
    /// </summary>
    public void AlternarModoZurdo() => ActivarModoZurdo(!modoZurdo);

    /// <summary>
    /// Alterna el estado de activación en la jerarquía de Unity para los botones diestros y zurdos registrados.
    /// </summary>
    /// <param name="activado">Indica si se deben mostrar los elementos para zurdos (True) o diestros (False).</param>
    private void AplicarModoZurdo(bool activado)
    {
        if (pares == null || pares.Count == 0)
        {
            return;
        }

        foreach (var par in pares)
        {
            if (par == null) continue;

            if (activado)
            {
                if (par.botonDiestro != null)
                {
                    par.botonDiestro.SetActive(false);
                }
                if (par.botonZurdo != null)
                {
                    par.botonZurdo.SetActive(true);
                }
            }
            else
            {
                if (par.botonDiestro != null)
                {
                    par.botonDiestro.SetActive(true);
                }
                if (par.botonZurdo != null)
                {
                    par.botonZurdo.SetActive(false);
                }
            }
        }
    }
}