using UnityEngine;

public abstract class ControladorPreguntas : MonoBehaviour
{
    [Header("Metricas de control")]
    //Las metricas de control si no son publicas no apareceran en el inspector de unity :p
    private int TiempoJuego;
    private int TotalAciertos;
    private int TotalFallos;
    private int TotalIntentos;
    private int TotalReinicios;
    //quiza haya que llevar estas variables al gameManager mejor
    private bool finished {get; set;}

    public void RegistrarMetricas()
    {

    }

    public abstract void EntregarRetroalimentacion();





    void Start()
    {
        finished = false;
    }

    void Update()
    {
        
    }
}
