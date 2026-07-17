using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class ControladorPreguntasNv1 : ControladorPreguntas
{
    [Header("Atributos")]
    [SerializeField] private string pregunta;
    [SerializeField] private string respuestaCorrecta;
    [SerializeField] private string respuesta;
    [SerializeField] private List<string> alternativas;

    private string idPatologia;
    [SerializeField] private Image imagen;
    [Header("Botones")]
    [SerializeField] private List<Button> botonesAlternativas;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    + ObtenerImagen(): Image
    + ComprobarRespuesta(String, String): void
    + SeleccionarAlternativa(Button): void
    + SeleccionarRespuestaCorrecta(Button): String
    + RellenarRespuestas(Button, Button, Button): void
    
    */
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
        int indice = Random.Range(0, CsvManager.Instance.patologias.Count);
        Patologia patologia = CsvManager.Instance.patologias[indice];
        idPatologia = patologia.id.ToString();

        Lesion lesion = CsvManager.Instance.ObtenerLesionPorId(patologia.lesionID);
        respuestaCorrecta = lesion.nombre;
        alternativas.Add(respuestaCorrecta);

        Debug.Log("Patologia: {patologia.nombre}, Lesion: {lesion.nombre}");
        Debug.Log("Respuesta Correcta: {respuestaCorrecta}");
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
            Debug.Log("Respuesta Correcta");
        }
        else
        {
            boton.GetComponent<Image>().color = Color.red;
            Debug.Log("Respuesta Incorrecta");
        }
        finished = true;
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
        throw new System.NotImplementedException();
    }
}
