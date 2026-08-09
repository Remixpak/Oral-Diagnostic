using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Seleccion : MonoBehaviour
{

    [Header("Canvas")]
    [SerializeField] private Canvas canvasModo;
    [SerializeField] private Canvas canvasDificultad;

    [SerializeField] private Canvas canvasCustom;

    [SerializeField] private string modoSeleccionado;
    [SerializeField] private string dificultadSeleccionada;
    [Header("Textos")]
    [SerializeField] private TMP_Text descripcionM;
    [SerializeField] private TMP_Text descripcionD;
    [Header("Botones")]
    [SerializeField] private Button siguienteM;
    [SerializeField] private Button siguienteM_alt;
    [SerializeField] private Button sigueinteD;
    [SerializeField] private Button sigueinteD_alt;

    [Header("Descripciones")]
    [SerializeField, TextArea] private string descripcionCarrera;
    [SerializeField, TextArea] private string descripcionQuickPlay;
    [SerializeField, TextArea] private string descripcionCustom;

    [SerializeField, TextArea] private string descripcionFacil;
    [SerializeField, TextArea] private string descripcionMedia;
    [SerializeField, TextArea] private string descripcionDificil;

    [SerializeField] private float velocidadEscritura = 0.03f;

    [Header("Btnes modos de juego")]
    [SerializeField] private Button Quick;
    [SerializeField] private Button Custom;

    private Coroutine escrituraActual;

    void Start()
    {
        siguienteM.interactable = false;
        siguienteM_alt.interactable = false;
        sigueinteD.interactable = false;
        sigueinteD_alt.interactable = false;
        if(ControladorGuardarDatos.Instance.ExisteUsuario())
        {
            Usuario u = ControladorGuardarDatos.Instance.CargarUsuario();
            if(!u.PartidaTerminada)
            {
                Quick.interactable = false;
                Custom.interactable = false;
            }
            else
            {
                Quick.interactable = true;
                Custom.interactable = true;
            }
        }
    }

    public void MostrarDescripcion(TMP_Text textoUI, string mensaje)
    {
        if (textoUI == null) return;

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
        ControladorSonido.Instance?.ReproducirClick();
        canvasModo.gameObject.SetActive(false);
        canvasDificultad.gameObject.SetActive(true);
    }

    public void RegresarAModo()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasDificultad.gameObject.SetActive(false);
        canvasModo.gameObject.SetActive(true);
    }

    public void seleccionarModo(Button boton)
    {
        ControladorSonido.Instance?.ReproducirClick();
        modoSeleccionado = boton.GetComponentInChildren<TMP_Text>().text.Trim();
        siguienteM.interactable = true;
        siguienteM_alt.interactable = true;

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
        ControladorSonido.Instance?.ReproducirClick();
        dificultadSeleccionada = boton.GetComponentInChildren<TMP_Text>().text.Trim();
        ConfiguracionPartida.Dificultad = dificultadSeleccionada;
        sigueinteD.interactable = true;
        sigueinteD_alt.interactable = true;

        switch (dificultadSeleccionada)
        {
            case "Fácil":
                MostrarDescripcion(descripcionD, descripcionFacil);
                break;

            case "Medio":
                MostrarDescripcion(descripcionD, descripcionMedia);
                break;

            case "Difícil":
                MostrarDescripcion(descripcionD, descripcionDificil);
                break;
        }
    }

    public void RegresarAInicio()
    {
        ControladorSonido.Instance?.ReproducirClick();
        SceneManager.LoadScene("PantallaInicio");
    }

    public void IrAJuego()
    {
        ControladorSonido.Instance?.ReproducirClick();
        ControladorGuardarDatos.Instance.EliminarPartida();//eliminamos la partida actual al seleccionar cualquier modo de juego
        SceneManager.LoadScene("MainSecene");
    }
    public void irACustom()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasModo.gameObject.SetActive(false);
        canvasCustom.gameObject.SetActive(true);
    }
    
    public void Bifurcacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (modoSeleccionado == "Custom")
            irACustom();
        else if(modoSeleccionado == "QuickPlay")
            IrAJuego();
        else
            PasarADificultad();
    }

    
}