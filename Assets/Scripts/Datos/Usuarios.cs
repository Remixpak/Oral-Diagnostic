using System;
using System.Collections.Generic;


[Serializable]

public class Usuario : IFirestoreData
{
    public int NumeroJugador;
    public string Nick;

    public Dictionary<string, object> ToFirestore()
    {
        return new Dictionary<string, object>()
        {
            { "numeroJugador", NumeroJugador },
            { "nick", Nick }
        };
    }
}