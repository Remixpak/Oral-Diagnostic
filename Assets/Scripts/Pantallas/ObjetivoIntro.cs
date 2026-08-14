using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla la animación estilo maquina de escribir (efecto Typewriter) para los mensajes introductorios,
/// gestionando la activación del Canvas de interfaz y el tiempo de espera asociado.
/// 
/// Clases dependientes que utiliza:
/// - TMP_Text: Componente de TextMeshPro utilizado para renderizar progresivamente los caracteres.
/// - Canvas: Elemento de la interfaz de usuario de Unity que se activa para visualizar el cuadro de texto intro.
/// </summary>
public class ObjetivoIntro : MonoBehaviour
{
    [Header("Texto")]
    [SerializeField] private TMP_Text texto;
    [SerializeField] private float velocidadEscritura = 0.05f;

    private Coroutine corrutinaTexto;

    [SerializeField] private Canvas canvas;

    [SerializeField] private string mensajeIntro;
    
    /// <summary>
    /// Indica si la animación de escritura o la pausa posterior se encuentran actualmente en ejecución.
    /// </summary>
    public bool Escribiendo { get; private set; }

    /// <summary>
    /// Inicia el despliegue del mensaje introductorio preconfigurado en el inspector.
    /// </summary>
    public void IniciarMensaje()
    {
        MostrarTexto(mensajeIntro);
    }

    /// <summary>
    /// Activa el Canvas asociado y detiene cualquier secuencia de texto previa antes de iniciar la corrutina de escritura.
    /// </summary>
    /// <param name="mensaje">Cadena de caracteres que será animada en pantalla.</param>
    private void MostrarTexto(string mensaje)
    {
        if (corrutinaTexto != null)
        {
            StopCoroutine(corrutinaTexto);
            Debug.Log("Se detuvo");
        }
        canvas.gameObject.SetActive(true);
        Debug.Log("Prendio");
        corrutinaTexto = StartCoroutine(EscribirTextoCorrutina(mensaje));
    }

    /// <summary>
    /// Corrutina que añade progresivamente cada carácter de la cadena al componente de texto y realiza una pausa final.
    /// </summary>
    /// <param name="mensaje">Texto completo a ser escrito caracter por caracter.</param>
    private IEnumerator EscribirTextoCorrutina(string mensaje)
    {
        Escribiendo = true;
        texto.text = "";

        foreach (char caracter in mensaje)
        {
            texto.text += caracter;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        yield return new WaitForSeconds(2f);

        Escribiendo = false;
        corrutinaTexto = null;
    }
}