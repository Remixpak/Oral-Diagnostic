using System;

[Serializable]
public class Partida
{
    public string Dificultad;

    public bool Lv1Completado;
    public bool Lv2Completado;
    public bool Lv3Completado;

    public Partida()
    {

    }

    public Partida(string dificultad, bool lv1, bool lv2, bool lv3)
    {
        Dificultad = dificultad;
        Lv1Completado = lv1;
        Lv2Completado = lv2;
        Lv3Completado = lv3;
    }
}