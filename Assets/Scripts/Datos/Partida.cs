using System;

[Serializable]
public class Partida
{
    public string Dificultad;
    public string ModoJuego;  // "Carrera", "QuickPlay" o "Custom"
    public bool Lv1Completado;
    public bool Lv2Completado;
    public bool Lv3Completado;

    public Partida()
    {

    }

    public Partida(string dificultad,string modoJuego, bool lv1, bool lv2, bool lv3)
    {
        Dificultad = dificultad;
        ModoJuego = modoJuego;
        Lv1Completado = lv1;
        Lv2Completado = lv2;
        Lv3Completado = lv3;
    }
}