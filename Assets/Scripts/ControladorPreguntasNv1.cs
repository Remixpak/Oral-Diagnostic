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
    [Header("Textos de retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;//correcto o incorrecto
    [SerializeField] public TMP_Text textoRespuesta;//cual era la respuesta

    

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
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.AddListener(() => finished = true);
        canvasRetroalimentacion.gameObject.SetActive(true);

    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        idPatologiaNumerica = indPatologiaAsignada;
        idPatologia = idPatologiaNumerica.ToString();
        botonesAlternativas = new List<Button>(GetComponentsInChildren<Button>());//revisar esto despues
        alternativas = new List<string>();
        ObtnerLesion();
        ObtenerImagen();
        RellenarRespuestas();

        foreach (Button boton in botonesAlternativas)
        {
            boton.onClick.AddListener(() => SeleccionarAlternativa(boton));
        }


    }



    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

    
}
