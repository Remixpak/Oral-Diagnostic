using UnityEngine;

/// <summary>
/// Define los diferentes tipos de mecánica o formato de pregunta disponibles en el juego, 
/// categorizados según la complejidad o el nivel educativo en el que se presentan.
/// </summary>
public enum TipoPregunta
{
    // --- Nivel 1: Conceptos Básicos y Diagnóstico Elemental ---
    /// <summary>Pregunta basada en la descripción diagnóstica o clínica básica.</summary>
    Descripciones,
    /// <summary>Pregunta enfocada en identificar la lesión elemental o clínica.</summary>
    Lesion,
    /// <summary>Pregunta sobre las manifestaciones clínicas o síntomas principales.</summary>
    Manifestaciones,

    // --- Nivel 2: Relaciones, Familias y Etiopatogenia ---
    /// <summary>Pregunta para emparejar o seleccionar la relación correcta entre conceptos.</summary>
    RelacionCorrecta,
    /// <summary>Pregunta para clasificar el concepto dentro de su familia o grupo clínico.</summary>
    FamiliaCorrespondiente,
    /// <summary>Pregunta enfocada en el origen y desarrollo de la patología (etiopatogenia).</summary>
    EtiopatogeniaCorrespondiente,
    /// <summary>Pregunta para enlazar una manifestación específica con su etiología o patología.</summary>
    EnlazeManifestacion,
    /// <summary>Pregunta de secuenciación u orden lógico asociado a una manifestación clínica.</summary>
    AsociarSecuenciaConManifestacion,

    // --- Nivel 3: Desafíos Complejos y Selección Múltiple Avanzada ---
    /// <summary>Mecánica de adivinanza / asociación rápida basada en 2 afirmaciones o preguntas.</summary>
    Adivina2Preguntas,
    /// <summary>Mecánica de adivinanza / asociación basada en 4 afirmaciones o preguntas.</summary>
    Adivina4Preguntas,
    /// <summary>Mecánica de adivinanza / asociación basada en 6 afirmaciones o preguntas.</summary>
    Adivina6Preguntas,
    /// <summary>Mecánica de selección o clasificación avanzada entre cuatro conceptos distintos.</summary>
    CuatroConceptos
}

/// <summary>
/// Estructura de datos serializable que representa la configuración de una pregunta dentro de una ronda de juego.
/// Mapea el identificador de la patología con el formato/mecánica de pregunta que debe instanciar la interfaz.
/// 
/// Clases y componentes que utiliza:
/// - System.Serializable (System): Permite que esta clase sea visible y editable directamente desde el Inspector de Unity o serializada en JSON/ScriptableObjects.
/// - TipoPregunta (Enum local): Define la categoría y regla visual/lógica con la que se procesará la patología.
/// </summary>
[System.Serializable]
public class PreguntaRonda
{
    [Tooltip("Identificador único de la patología o tema clínico en la base de datos de preguntas.")]
    public int idPatologia;

    [Tooltip("Tipo de formato/mecánica de pregunta que determinará la plantilla visual y las reglas de respuesta.")]
    public TipoPregunta tipo;
}