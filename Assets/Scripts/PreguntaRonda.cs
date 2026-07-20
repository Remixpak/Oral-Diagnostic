public enum TipoPregunta
{
    Trivia,
    Arbol,
    Conceptos,
    AdivinaQuien
}

[System.Serializable]
public class PreguntaRonda
{
    public int idPatologia;
    public TipoPregunta tipo;
}