public enum TipoPregunta
{
    // Nivel 1
    Descripciones,
    Lesion,
    Manifestaciones,

    // Nivel 2
    RelacionCorrecta,
    FamiliaCorrespondiente,
    EtiopatogeniaCorrespondiente,
    EnlazeManifestacion,
    AsociarSecuenciaConManifestacion,

    // Nivel 3
    Adivina2Preguntas,
    Adivina4Preguntas,
    Adivina6Preguntas,
    CuatroConceptos
}

[System.Serializable]
public class PreguntaRonda
{
    public int idPatologia;
    public TipoPregunta tipo;
}