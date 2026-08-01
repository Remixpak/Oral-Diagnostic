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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
{
    CantidadPreguntas = 52;

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
        btnContinuar.interactable =
            toggleLesiones.isOn ||
            toggleFamilias.isOn ||
            toggleDiagnosticos.isOn;
    }

    public void sumar()
    {
        if(CantidadPreguntas < 52)
        {
            CantidadPreguntas++;
            textoCantidad.text = "Cantidad de preguntas: " + CantidadPreguntas;
        }
            
        else
        {
            CantidadPreguntas = 52;
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
