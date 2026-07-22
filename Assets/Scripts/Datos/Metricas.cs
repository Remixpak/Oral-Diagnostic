using System;
using System.Collections.Generic;

[Serializable]
public class Metricas : IFirestoreData
{
    public string Id;
    public string Nivel;
    public GameManager.ModoJuego ModoJuego;
    public int NumeroJugador;
    public float TiempoJuego;
    public int TotalIntentos;
    public int TotalReinicios;
    public int TotalAciertos;
    public int TotalFallos;

    public Dictionary<string, object> ToFirestore()
    {
        return new Dictionary<string, object>()
        {
            { "id", Id },
            { "nivel", Nivel },
            { "modoJuego", ModoJuego.ToString() },
            { "numeroJugador", NumeroJugador },
            { "tiempoJuego", TiempoJuego },
            { "totalIntentos", TotalIntentos },
            { "totalReinicios", TotalReinicios },
            { "totalAciertos", TotalAciertos },
            { "totalFallos", TotalFallos }
        };
    }
}