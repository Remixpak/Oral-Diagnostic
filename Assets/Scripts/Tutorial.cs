using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [Header("Textos")]
    [TextArea] [SerializeField] private string texto1;
    [TextArea] [SerializeField] private string texto2;
    [TextArea] [SerializeField] private string texto3;

    [SerializeField] private TMP_Text textoSnackBar;

    [Header("Configuración")]
    [SerializeField] private float tiempoEntreLetras = 0.05f;
    [SerializeField] private float esperaEntreMensajes = 2f;
    [SerializeField] private float esperaFinal = 2f;

    [Header("Objetivos")]
    [SerializeField] private TMP_Text Objetivo;

    public bool Finalizado { get; private set; }

    private Coroutine rutinaTutorial;
    private bool omitido = false;

    public void ActivarTutorial()
    {
        Finalizado = false;
        omitido = false;

        rutinaTutorial = StartCoroutine(MostrarTutorial());
    }

    private IEnumerator MostrarTutorial()
    {
        yield return StartCoroutine(EscribirTexto(texto1));
        if (omitido) yield break;

        yield return new WaitForSecondsRealtime(esperaEntreMensajes); // <--- REALTIME

        yield return StartCoroutine(EscribirTexto(texto2));
        if (omitido) yield break;

        yield return new WaitForSecondsRealtime(esperaEntreMensajes); // <--- REALTIME

        yield return StartCoroutine(EscribirTexto(texto3));
        if (omitido) yield break;

        yield return new WaitForSecondsRealtime(esperaFinal); // <--- REALTIME

        CerrarTutorial();
    }

    private IEnumerator EscribirTexto(string mensaje)
    {
        textoSnackBar.text = "";

        foreach (char letra in mensaje)
        {
            if (omitido)
                yield break;

            textoSnackBar.text += letra;
            yield return new WaitForSecondsRealtime(tiempoEntreLetras); // <--- REALTIME
        }
    }

    // Lo llamas desde el botón "Cerrar"
    public void CerrarTutorial()
    {
        ControladorSonido.Instance?.ReproducirClick();

        if (Finalizado)
            return;

        omitido = true;

        // Detenemos las corrutinas de tipeo de ESTE objeto
        StopAllCoroutines();

        Finalizado = true;
        Debug.Log("Cerrar tutorial " + GetInstanceID());
    }

    private void OnDisable()
    {
        // Si el objeto se deshabilita o destruye a la fuerza durante un reinicio,
        // garantizamos que no deje trabado el WaitUntil del GameManager.
        Finalizado = true;
    }

    public void ConfigurarTutorial(string txt1, string txt2, string txt3, string obj)
    {
        texto1 = txt1;
        texto2 = txt2;
        texto3 = txt3;
        Objetivo.text = obj;
    }
}