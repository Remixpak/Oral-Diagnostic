using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona la pantalla de selección de modos de juego y dificultades.
/// Administra la navegación entre lienzos de interfaz (Canvas), la animación de texto progresivo 
/// para las descripciones y la habilitación condicional de modos según el estado del jugador.
/// 
/// Clases dependientes que utiliza:
/// - ControladorSonido: Maneja los efectos de audio de la interfaz (reproducción de clics).
/// - ControladorGuardarDatos: Consulta y elimina el estado guardado del usuario o partida.
/// - ConfiguracionPartida: Almacena de forma estática el modo de juego y la dificultad seleccionados para la siguiente escena.
/// - GameManager: Contiene la definición de la enumeración <c>ModoJuego</c> (Carrera, QuickPlay, Custom).
/// - Usuario: Modelo de datos del jugador para validar si completó previamente el juego.
/// </summary>
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

    /// <summary>
    /// Deshabilita los botones de navegación iniciales y valida el progreso del usuario 
    /// para desbloquear o bloquear los modos QuickPlay y Custom.
    /// </summary>
    void Start()
    {
        siguienteM.interactable = false;
        siguienteM_alt.interactable = false;
        sigueinteD.interactable = false;
        sigueinteD_alt.interactable = false;

        if (ControladorGuardarDatos.Instance.ExisteUsuario())
        {
            Usuario u = ControladorGuardarDatos.Instance.CargarUsuario();
            if (!u.PartidaTerminada)
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

    /// <summary>
    /// Cancela cualquier animación de escritura en curso e inicia una nueva corrutina para mostrar el mensaje progresivamente.
    /// </summary>
    /// <param name="textoUI">Componente de texto UI objetivo donde se escribirá el mensaje.</param>
    /// <param name="mensaje">Cadena de caracteres que se desea mostrar en pantalla.</param>
    public void MostrarDescripcion(TMP_Text textoUI, string mensaje)
    {
        if (textoUI == null) return;

        if (escrituraActual != null)
            StopCoroutine(escrituraActual);

        escrituraActual = StartCoroutine(EscribirTexto(textoUI, mensaje));
    }

    /// <summary>
    /// Corrutina encargada del efecto de máquina de escribir letra por letra.
    /// </summary>
    /// <param name="textoUI">Componente de texto objetivo.</param>
    /// <param name="mensaje">Texto completo a ser escrito.</param>
    private IEnumerator EscribirTexto(TMP_Text textoUI, string mensaje)
    {
        textoUI.text = "";

        foreach (char letra in mensaje)
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }
    }

    /// <summary>
    /// Oculta el menú de selección de modo y despliega el lienzo de selección de dificultad.
    /// </summary>
    public void PasarADificultad()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasModo.gameObject.SetActive(false);
        canvasDificultad.gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta el lienzo de selección de dificultad y regresa al panel de selección de modo.
    /// </summary>
    public void RegresarAModo()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasDificultad.gameObject.SetActive(false);
        canvasModo.gameObject.SetActive(true);
    }

    /// <summary>
    /// Procesa la interacción con un botón de modo de juego, actualizando la configuración global y mostrando su descripción.
    /// </summary>
    /// <param name="boton">Botón de interfaz presionado que contiene el texto identificador del modo.</param>
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

    /// <summary>
    /// Procesa la interacción con un botón de dificultad, configurando los parámetros globales y desplegando la descripción asociada.
    /// </summary>
    /// <param name="boton">Botón presionado con la etiqueta de dificultad correspondiente.</param>
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

    /// <summary>
    /// Carga de nuevo la escena de menú principal ("PantallaInicio").
    /// </summary>
    public void RegresarAInicio()
    {
        ControladorSonido.Instance?.ReproducirClick();
        SceneManager.LoadScene("PantallaInicio");
    }

    /// <summary>
    /// Reinicia la partida previa eliminando los datos existentes e inicia la escena principal de juego ("MainSecene").
    /// </summary>
    public void IrAJuego()
    {
        ControladorSonido.Instance?.ReproducirClick();
        ControladorGuardarDatos.Instance.EliminarPartida();
        SceneManager.LoadScene("MainSecene");
    }

    /// <summary>
    /// Muestra la interfaz de configuración personalizada ocultando el lienzo de selección de modo.
    /// </summary>
    public void irACustom()
    {
        ControladorSonido.Instance?.ReproducirClick();
        canvasModo.gameObject.SetActive(false);
        canvasCustom.gameObject.SetActive(true);
    }

    /// <summary>
    /// Evalúa el modo de juego seleccionado para redirigir al jugador a la interfaz correspondiente (Custom, Partida Rápida o Dificultad).
    /// </summary>
    public void Bifurcacion()
    {
        ControladorSonido.Instance?.ReproducirClick();
        if (modoSeleccionado == "Custom")
            irACustom();
        else if (modoSeleccionado == "QuickPlay")
            IrAJuego();
        else
            PasarADificultad();
    }
}