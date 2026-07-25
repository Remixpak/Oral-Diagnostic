using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
public class ControladorPreguntasNv1 : ControladorPreguntas
{
    
    [Header("Atributos")]
    [SerializeField] private string pregunta;
    [SerializeField] private string respuestaCorrecta;
    [SerializeField] private string respuesta;
    [SerializeField] private List<string> alternativas;
    
    private int idPatologiaNumerica;
    private string idPatologia;
    [SerializeField] private Image imagen;
    [Header("Botones")]
    [SerializeField] private List<Button> botonesAlternativas;
    [SerializeField] private Button botonPausa;
    [Header("Textos de retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;//correcto o incorrecto
    [SerializeField] public TMP_Text textoRespuesta;//cual era la respuesta

    private bool yaRespondio = false;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //obtiene la imagen segun la id de la patologia
    public void ObtenerImagen()
    {
        Patologia patologia = CsvManager.Instance.ObtenerPatologiaPorId(int.Parse(idPatologia));
        Debug.Log("Buscando imagn: " + patologia.codigoImagen + " Largo: " + patologia.codigoImagen.Length);
        imagen.sprite = CsvManager.Instance.spritePorCodigo(patologia.codigoImagen);
        Debug.Log("Termino de buscar la imagen");
       
    }
    //obtiene la lesion segun la patologia
    public void ObtnerLesion()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaNumerica);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = l.nombre;
        alternativas.Add(respuestaCorrecta);
    }
    public bool ComprobarRespuesta(string respuesta, string respuestaCorrecta)
    {
        return respuesta == respuestaCorrecta;
    }
    public void SeleccionarAlternativa(Button boton)
    {
        if (yaRespondio) return;
        yaRespondio = true;

        foreach(Button btn in botonesAlternativas)//si el jugador ya respondio los botones se deshabilitan
        {
            btn.interactable=false;
        }
        

        respuesta = boton.GetComponentInChildren<TMP_Text>().text;
        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            boton.GetComponent<Image>().color = Color.green;
            GameManager.Instance.TotalAciertos++;
            Debug.Log("Respuesta Correcta");
        }
        else
        {
            boton.GetComponent<Image>().color = Color.red;
            GameManager.Instance.TotalFallos++;
            Debug.Log("Respuesta Incorrecta");
        }
        StartCoroutine(FinalizarPregunta());
    }
    
    public void RellenarRespuestas()
    {
        alternativas.Clear();

        alternativas.Add(respuestaCorrecta);
        List<Lesion> l = new List<Lesion>(CsvManager.Instance.lesiones);
        l.RemoveAll(l => l.nombre == respuestaCorrecta);
        while(alternativas.Count < botonesAlternativas.Count)
        {
            int indice = Random.Range(0, l.Count);
            alternativas.Add(l[indice].nombre);
            l.RemoveAt(indice);
        }
        for(int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(0, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }
        for(int i = 0; i < botonesAlternativas.Count; i++)
        {
            botonesAlternativas[i].GetComponentInChildren<TMP_Text>().text = alternativas[i];
        }
    }

    public override void EntregarRetroalimentacion()
    {
        canvasJuego.gameObject.SetActive(false);
        if(ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            textoResultado.text = "¡Respuesta Correcta!";
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
        }
        textoRespuesta.text = "La respuesta correcta es: " + respuestaCorrecta;
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.AddListener(() => finished = true);
        canvasRetroalimentacion.gameObject.SetActive(true);

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        Debug.Log("NV1 Inicializar");
        Debug.Log("A");
        yaRespondio = false;
        idPatologiaNumerica = indPatologiaAsignada;
        idPatologia = idPatologiaNumerica.ToString();

        Button[] todosLosBotones = GetComponentsInChildren<Button>();
        botonesAlternativas = new List<Button>();

        foreach (Button btn in todosLosBotones)
        {
            if (btn == botonPausa) continue;
            if (btn.CompareTag("Pausa")) continue;
            if (btn.name.Contains("Pausa")) continue;

            botonesAlternativas.Add(btn);
        }

        alternativas = new List<string>();

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
        }
        Debug.Log("B");
        ObtnerLesion();
        Debug.Log("C");
        ObtenerImagen();
        Debug.Log("D");

        RellenarRespuestas();
        Debug.Log("E");
        foreach (Button boton in botonesAlternativas)
        {
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => SeleccionarAlternativa(boton));
        }
    }



    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

    
}
