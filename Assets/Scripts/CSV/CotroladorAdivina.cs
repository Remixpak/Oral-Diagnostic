using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControladorAdivina : ControladorPreguntas
{
    [Header("Botones")]
    [SerializeField] private List<Button> diagnosticos = new List<Button>();

    [Header("Datos")]
    [SerializeField] private string patologiaCorrecta;
    [SerializeField] private string patologiaSeleccionada;

    private int idPatologia;

    private List<string> alternativas;

    [Header("Snackbar")]
    [SerializeField] private GameObject panelSnackbar;
    [SerializeField] private TMP_Text textoSnackbar;

    [Header("Libreta")]
    [SerializeField] private GameObject libretaCanvas;
    [SerializeField] private Transform contenidoLibreta;
    [SerializeField] private GameObject prefabNota;
    private HashSet<string> notas = new HashSet<string>();

    private Coroutine snackbarCoroutine;

    private void Awake()
    {
        alternativas = new List<string>();
    }

    private void AgregarNota(string texto)
    {
        if(!notas.Add(texto))
            return;
        GameObject nota = Instantiate(prefabNota, contenidoLibreta);
        nota.GetComponentInChildren<TMP_Text>().text = texto;
    }

    public void AbrirLibreta()
    {
        libretaCanvas.SetActive(true);
    }
    public void CerrarLibreta()
    {
        libretaCanvas.SetActive(false);
    }

    private void ObtenerDiagnosticos()
    {
        alternativas.Clear();

        // Agrega la correcta
        alternativas.Add(patologiaCorrecta);

        // Copia de la lista de patologías
        List<Patologia> lista = new List<Patologia>(CsvManager.Instance.patologias);

        // Elimina la correcta
        lista.RemoveAll(p => p.nombre == patologiaCorrecta);

        // Rellena con respuestas aleatorias
        while (alternativas.Count < diagnosticos.Count)
        {
            int indice = Random.Range(0, lista.Count);

            alternativas.Add(lista[indice].nombre);

            lista.RemoveAt(indice);
        }

        // Barajar respuestas
        for (int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(i, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }

        // Asignar textos a los botones
        for (int i = 0; i < diagnosticos.Count; i++)
        {
            diagnosticos[i].GetComponentInChildren<TMP_Text>().text = alternativas[i];
        }
    }

    private void Seleccionar(Button boton)
    {
        // Se implementará más adelante
    }

    public void PreguntarLesion()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        string pista = $"La lesión es: {l.nombre}";
        MostrarPista(pista);
        AgregarNota(pista);

    }
    public void PreguntarFamilia()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Familia f = CsvManager.Instance.ObtenerFamiliaPorId(p.familiaID);
        string pista = $"La familia es: {f.nombre}";
        MostrarPista(pista);
        AgregarNota(pista);
    }
    public void PreguntarEtiologia()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        Etiologia e = CsvManager.Instance.ObtenerEtiologiaPorId(p.etiologiaID);
        string pista = $"La Etiologia es: {e.nombre}";
        MostrarPista(pista);
        AgregarNota(pista);
    }
    private void PreguntarDescripcion()
    {
        
    }

    private IEnumerator MostrarSnackbar(string mensaje)
    {
        panelSnackbar.SetActive(true);
        textoSnackbar.text = "";

        foreach (char c in mensaje)
        {
            textoSnackbar.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(3f);

        panelSnackbar.SetActive(false);
    }
    private void MostrarPista(string mensaje)
    {
        if (snackbarCoroutine != null)
            StopCoroutine(snackbarCoroutine);

        snackbarCoroutine = StartCoroutine(MostrarSnackbar(mensaje));
    }

    public override void EntregarRetroalimentacion()
    {

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        idPatologia = indPatologiaAsignada;

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologia);
        patologiaCorrecta = p.nombre;

        ObtenerDiagnosticos();

        foreach (Button boton in diagnosticos)
        {
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => Seleccionar(boton));
        }
    }
}