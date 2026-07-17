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

    public void RegistrarMetricas()
    {

    }

    public abstract void EntregarRetroalimentacion();





    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
