using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfiguracionCustom : MonoBehaviour
{
    [Header("Cantidad")]
    [SerializeField] private TMP_Text textoCantidad;

    [Header("Preguntas")]

    private int CantidadPreguntas;

    [Header("Toggles")]
    [SerializeField] private Toggle toggleLesiones;
    [SerializeField] private Toggle toggleFamilias;
    [SerializeField] private Toggle toggleDiagnosticos;

    [Header("btn continuar")]

    [SerializeField] private Button btnContinuar;
    [SerializeField] private Button btnContinuar_alt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CantidadPreguntas = 30;

        toggleLesiones.onValueChanged.AddListener(delegate { ActualizarBoton(); });
        toggleFamilias.onValueChanged.AddListener(delegate { ActualizarBoton(); });
        toggleDiagnosticos.onValueChanged.AddListener(delegate { ActualizarBoton(); });

        // Comprobar el estado inicial
        ActualizarBoton();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ActualizarBoton()
    {
        bool activo = toggleLesiones.isOn || toggleFamilias.isOn || toggleDiagnosticos.isOn;

        btnContinuar.interactable = activo;
        btnContinuar_alt.interactable = activo; //agregamos el boton alt para el modo zurdo
    }

    public void sumar()
    {
        if(CantidadPreguntas < 30)
        {
            CantidadPreguntas++;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
            
        else
        {
            CantidadPreguntas = 30;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
            
    }
    public void restar()
    {
        if(CantidadPreguntas > 0)
        {
            CantidadPreguntas--;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
        else
        {
            CantidadPreguntas = 0;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
    }

    private void CargarConfig()
    {
        ConfiguracionPartida.CantidadPreguntas = this.CantidadPreguntas;
        ConfiguracionPartida.Lesiones = toggleLesiones.isOn;
        ConfiguracionPartida.FamiliasEtiopatogenias = toggleFamilias.isOn;
        ConfiguracionPartida.Diagnosticos = toggleDiagnosticos.isOn;
    }

    public void IrAJuegoCustom()
    {
        CargarConfig();
        SceneManager.LoadScene("MainSecene");
    }
}
