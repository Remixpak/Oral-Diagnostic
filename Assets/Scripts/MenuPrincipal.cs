using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private Toggle toggleModoZurdo;
    [SerializeField] private Toggle toggleDesactivarSondio;
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonAjustes;
    [SerializeField] private Button cerrarAjustes;

    [Header("Titulo")]
    [SerializeField] private TMP_Text tiutuloTexto;
    [SerializeField] private string tituloJuego = "OralDiagnostic";


    void Start()
    {
        if (tiutuloTexto!=null)
            tiutuloTexto.text = tituloJuego;

        if(panelAjustes != null)
            panelAjustes.SetActive(false);

        if (botonJugar != null)
            botonJugar.onClick.AddListener(Jugar);

        if (botonAjustes != null)
            botonAjustes.onClick.AddListener(AbrirAjustes);

        if (botonAjustes != null)
            cerrarAjustes.onClick.AddListener(CerrarAjustes);

        if (toggleModoZurdo != null)
            toggleModoZurdo.onValueChanged.AddListener(ModoZurdo);

        if (toggleDesactivarSondio != null)
            toggleDesactivarSondio.onValueChanged.AddListener(DesactivarSonido);
    }

    private void Jugar()
    {
        if (GameManager.Instance != null)
        {
            gameObject.SetActive(false);
            GameManager.Instance.IniciarModoCarrera();
        }
        
    }

    private void AbrirAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(true);
    }

    private void CerrarAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(false);
    }

    private void ModoZurdo(bool activado)
    {
        //logica para el modo zurdo :p
    }

    private void DesactivarSonido(bool activado)
    {
        //logica para desactivar los sonidos
    }

    public void MostrarPantalla()
    {
        gameObject.SetActive(true);
            if (panelAjustes != null)
                panelAjustes.SetActive(false);
        }

    void Update()
    {
        
    }
}
