using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Seleccion: MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas canvasModo;
    [SerializeField] private Canvas canvasDificultad;
    
    [SerializeField] private string modoSeleccionado;
    [SerializeField] private string dificultadSeleccionada;
    [Header("Textos")]
    [SerializeField] private TMP_Text descripcionM;
    [SerializeField] private TMP_Text descripcionD;
    [Header("Botones")]
    [SerializeField] private Button siguienteM;
    [SerializeField] private Button sigueinteD;

    [Header("Descripciones")]
    [SerializeField, TextArea] private string descripcionCarrera;
    [SerializeField, TextArea] private string descripcionQuickPlay;
    [SerializeField, TextArea] private string descripcionCustom;

    [SerializeField, TextArea] private string descripcionFacil;
    [SerializeField, TextArea] private string descripcionMedia;
    [SerializeField, TextArea] private string descripcionDificil;

    [SerializeField] private float velocidadEscritura = 0.03f;

    private Coroutine escrituraActual;


    void Start()
    {
        siguienteM.interactable = false;
        sigueinteD.interactable = false;
    }
    

    private void MostrarDescripcion(TMP_Text textoUI, string mensaje)
    {
        if (escrituraActual != null)
            StopCoroutine(escrituraActual);

        escrituraActual = StartCoroutine(EscribirTexto(textoUI, mensaje));
    }

    private IEnumerator EscribirTexto(TMP_Text textoUI, string mensaje)
    {
        textoUI.text = "";

        foreach (char letra in mensaje)
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }
    }
    public void PasarADificultad()
    {
        canvasModo.gameObject.SetActive(false);
        canvasDificultad.gameObject.SetActive(true);
    }
    public void RegresarAModo()
    {
        canvasDificultad.gameObject.SetActive(false);
        canvasModo.gameObject.SetActive(true);
    }
    public void seleccionarModo(Button boton)
    {
        modoSeleccionado = boton.GetComponentInChildren<TMP_Text>().text.Trim();
        siguienteM.interactable = true;
        switch (modoSeleccionado)
        {
            case "Carrera":
                ConfiguracionPartida.Modo = GameManager.ModoJuego.Carrera;
                MostrarDescripcion(descripcionM, descripcionCarrera);
                break;

            case "QuickPlay":
                ConfiguracionPartida.Modo = GameManager.ModoJuego.QuickPlay;
                MostrarDescripcion(descripcionM, descripcionQuickPlay);
                break;

            case "Custom":
                ConfiguracionPartida.Modo = GameManager.ModoJuego.Custom;
                MostrarDescripcion(descripcionM, descripcionCustom);
                break;
        }
    }
    public void SeleccionarDificultad(Button boton)
    {
        dificultadSeleccionada = boton.GetComponentInChildren<TMP_Text>().text.Trim();
        ConfiguracionPartida.Dificultad = dificultadSeleccionada;
        sigueinteD.interactable = true;
        switch (dificultadSeleccionada)
        {
            case "Practicante":
                MostrarDescripcion(descripcionD, descripcionFacil);
                break;

            case "Asistente":
                MostrarDescripcion(descripcionD, descripcionMedia);
                break;

            case "Experto":
                MostrarDescripcion(descripcionD, descripcionDificil);
                break;
        }
    }



    public void RegresarAInicio()
    {
        SceneManager.LoadScene("PantallaInicio");
    }
    public void IrAJuego()
    {
        SceneManager.LoadScene("MainSecene");
    }

    


}