using System;
using System.Collections.Generic;

[Serializable]
public class Metricas : IFirestoreData
{
    public string Id;
    public string TipoNivel;
    public GameManager.ModoJuego ModoJuego;
    public int NumeroJugador;
    public string TiempoJuego; //cambiamos el tiempo de flaot a string para poder formatearlo en minutos y segundos
    public int TotalIntentos;
    public int TotalReinicios;
    public int TotalAciertos;
    public int TotalFallos;

    //public int FalloLesiones;
    //public int FalloFamilias;
    //public int FalloDiagnosticos;

    public Dictionary<string, object> ToFirestore()
    {
        return new Dictionary<string, object>()
        {
            { "id", Id },
            { "nivel", TipoNivel },
            { "modoJuego", ModoJuego.ToString() },
            { "numeroJugador", NumeroJugador },
            { "tiempoJuego", TiempoJuego },
            { "totalIntentos", TotalIntentos },
            { "totalReinicios", TotalReinicios },
            { "totalAciertos", TotalAciertos },
            { "totalFallos", TotalFallos }
            /*{"falloLesiones", FalloLesiones},
            {"falloFamilias", FalloFamilias},
            {"falloDiagnosticos", FalloDiagnosticos}*/
        };
    }
}