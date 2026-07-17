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
        Sprite sprite = Resources.Load<Sprite>($"Imagenes/" + patologia.codigoImagen);
        if(sprite == null)
        {
            Debug.LogError("No se pudo cargar la imagen: " + patologia.codigoImagen);
        }
        imagen.sprite = sprite;
       
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
        respuesta = boton.GetComponentInChildren<Text>().text;
        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            Debug.Log("Respuesta Correcta");
        }
        else
        {
            Debug.Log("Respuesta Incorrecta");
        }
    }
    
    public void RellenarRespuestas()
    {
        alternativas.Clear();

        alternativas.Add(respuestaCorrecta);
        List<Lesion> lesiones = CsvManager.Instance.lesiones;
        lesiones.RemoveAll(l => l.nombre == respuestaCorrecta);
        while(alternativas.Count < botonesAlternativas.Count)
        {
            int indice = Random.Range(0, lesiones.Count);
            alternativas.Add(lesiones[indice].nombre);
            lesiones.RemoveAt(indice);
        }
        for(int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(0, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }
        for(int i = 0; i < botonesAlternativas.Count; i++)
        {
            botonesAlternativas[i].GetComponentInChildren<Text>().text = alternativas[i];
        }
    }

    public override void EntregarRetroalimentacion()
    {
        throw new System.NotImplementedException();
    }
}
