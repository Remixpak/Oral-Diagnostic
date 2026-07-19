using UnityEngine;
using TMPro;
public abstract class ControladorPreguntas : MonoBehaviour
{
    
    //quiza haya que llevar estas variables al gameManager mejor
    
    public bool finished;
    /*
    todas las clases que hereden de esta deben implementar coasa
    el camvas de juego es lo que construye el nivel
    el canvas de retroalimentacion es lo que se muestra al final de la pregunta
    y el canvas de resultados muestra las metricas
    
    
    
    */
    [Header("Canvas")]
    [SerializeField] public Canvas canvasJuego;
    [SerializeField] public Canvas canvasRetroalimentacion;
    

    [Header("Textos de retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;//correcto o incorrecto
    [SerializeField] public TMP_Text textoRespuesta;//cual era la respuesta

    

    public void RegistrarMetricas()
    {

    }

    public abstract void EntregarRetroalimentacion();



    public abstract void InicializarPregunta(int indPatologiaAsignada);





    void Start()
    {
        finished = false;
    }

    void Update()
    {
        
    }
}
