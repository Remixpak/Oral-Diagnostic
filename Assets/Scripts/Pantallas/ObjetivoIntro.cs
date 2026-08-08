using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ObjetivoIntro : MonoBehaviour
{
    [Header("Texto")]
    [SerializeField] private TMP_Text texto;
    [SerializeField] private float velocidadEscritura = 0.05f;

    private Coroutine corrutinaTexto;

    [SerializeField] private Canvas canvas;

    [SerializeField] private string mensajeIntro;
    
    // Propiedad pública para saber si la animación sigue en curso
    public bool Escribiendo { get; private set; }

    public void IniciarMensaje()
    {
        MostrarTexto(mensajeIntro);
    }

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

    private IEnumerator EscribirTextoCorrutina(string mensaje)
    {
        Escribiendo = true; // Inicia la escritura
        texto.text = "";

        foreach (char caracter in mensaje)
        {
            texto.text += caracter;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        // Espera los 2 segundos extra después de completar el texto
        yield return new WaitForSeconds(2f);

        Escribiendo = false; // Finalizó completamente la corrutina
        corrutinaTexto = null;
    }
}