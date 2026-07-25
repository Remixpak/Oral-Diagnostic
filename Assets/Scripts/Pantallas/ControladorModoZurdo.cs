using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ControladorModoZurdo : MonoBehaviour
{

    //basicamente al script le pasas dos botones, uno que quede visible al estar desactivado el modo zurdo y otro que se quiera ver si se activa el modo zurdo, diviertete y asegurate que otro boton
    //no este isntanciando el mismo o pasaran cosas feas
    public static ControladorModoZurdo Instance { get; private set; }

    [System.Serializable]
    public class ParBotones
    {
        public GameObject botonDiestro;
        public GameObject botonZurdo;
    }

    [Header("Pares de botones")]
    [SerializeField] private List<ParBotones> pares = new List<ParBotones>();

    private bool modoZurdo = false;

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

    public void ActivarModoZurdo(bool activado)
    {
        if (modoZurdo == activado) return;

        modoZurdo = activado;
        PlayerPrefs.SetInt("ModoZurdo", activado ? 1 : 0);
        PlayerPrefs.Save();
        AplicarModoZurdo(activado);
    }

    public bool EsModoZurdo() => modoZurdo;

    public void AlternarModoZurdo() => ActivarModoZurdo(!modoZurdo);

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