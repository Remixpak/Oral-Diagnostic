using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Identifica la preferencia o ajuste global que controla un interruptor (ToggleSwitch) en la interfaz.
/// </summary>
public enum TipoToggle
{
    /// <summary>Controla la activación/desactivación de los efectos de sonido (SFX).</summary>
    Sonido,
    /// <summary>Controla la activación/desactivación de la música de fondo.</summary>
    Musica,
    /// <summary>Controla la adaptación de la interfaz para usuarios zurdos.</summary>
    ModoZurdo
}

/// <summary>
/// Componente de interfaz de usuario personalizado para simular un interruptor deslizable (Toggle Switch).
/// Se encarga del comportamiento visual (animación de relleno, desplazamiento de la manecilla y cambio de texto/color),
/// la persistencia de datos (PlayerPrefs) y la notificación de cambios a los gestores globales (ControladorSonido y ControladorModoZurdo).
/// 
/// Clases y componentes que utiliza:
/// - MonoBehaviour / RectTransform / Color / Mathf (UnityEngine): Ciclo de vida, animaciones de transición suave (Lerp) y manipulación de coordenadas del Canvas.
/// - Image (UnityEngine.UI): Componentes gráficos para el fondo/relleno (fillImage) y la manecilla o perilla (handleImage).
/// - TextMeshProUGUI (TMPro): Renderizado de texto para mostrar el estado interactivo ("Sí" / "No").
/// - UnityEvent&lt;bool&gt; (UnityEngine.Events): Evento personalizado para notificar cambios de estado a otros escuchas de UI.
/// - PlayerPrefs (UnityEngine): Persistencia de datos en disco para guardar configuraciones de usuario.
/// - ControladorSonido / ControladorModoZurdo (Singletons locales): Gestores globales para aplicar las preferencias del usuario.
/// </summary>
public class ToggleSwitch : MonoBehaviour
{
    [Header("Elementos")]
    [Tooltip("Imagen de la barra de fondo que se rellena al activar el interruptor.")]
    [SerializeField] private Image fillImage;

    [Tooltip("Imagen del botón/manecilla deslizable del interruptor.")]
    [SerializeField] private Image handleImage;

    [Tooltip("Etiqueta de texto TextMeshPro que indica visualmente el estado actual ('Sí' / 'No').")]
    [SerializeField] private TextMeshProUGUI stateText;

    /// <summary>
    /// Evento de Unity especializado para transmitir el estado booleano del interruptor a través de suscriptores.
    /// </summary>
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [Header("Eventos")]
    [Tooltip("Invocado cuando cambia el estado activo/inactivo del interruptor.")]
    public BoolEvent OnValueChanged;

    // --- Variables de control interno y animación ---
    private RectTransform handle;
    private bool prendio = false;
    private float targetFillAmount;

    [Header("Configuración Visual")]
    [Tooltip("Color aplicado al texto de estado cuando el interruptor está encendido.")]
    [SerializeField] private Color onColor = Color.white;

    [Tooltip("Color aplicado al texto de estado cuando el interruptor está apagado.")]
    [SerializeField] private Color offColor = new Color(37f / 255f, 37f / 255f, 37f / 255f);

    [Tooltip("Tipo de configuración del sistema que este interruptor modificará.")]
    [SerializeField] private TipoToggle tipo;

    /// <summary>
    /// Fase Awake. Obtiene la referencia del RectTransform de la manecilla para gestionar su posición y pivot.
    /// </summary>
    private void Awake()
    {
        if (handleImage != null)
            handle = handleImage.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Fase Start. Lee el estado guardado o del sistema según el TipoToggle 
    /// e inicializa la posición visual del interruptor sin emitir eventos.
    /// </summary>
    private void Start()
    {
        bool estado = false;

        // Consulta el estado inicial del sistema según la categoría asignada
        switch (tipo)
        {
            case TipoToggle.Sonido:
                estado = ControladorSonido.Instance != null &&
                         ControladorSonido.Instance.SonidoActivado();
                break;

            case TipoToggle.Musica:
                estado = ControladorSonido.Instance != null &&
                         ControladorSonido.Instance.MusicaActivada();
                break;

            case TipoToggle.ModoZurdo:
                estado = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;
                break;
        }

        // Aplica el valor inicial sin disparar notificaciones
        SetValue(estado, false);
    }

    /// <summary>
    /// Alterna el estado del interruptor (al hacer clic/toque). 
    /// Aplica los cambios visuales, actualiza el valor persistente/sistema e invoca los eventos suscriptos.
    /// </summary>
    public void Toggle()
    {
        prendio = !prendio;
        targetFillAmount = prendio ? 1f : 0f;

        UpdateHandlePivot();
        UpdateStateText();

        // Aplica la lógica de negocio correspondiente al tipo de toggle
        switch (tipo)
        {
            case TipoToggle.Sonido:
                ControladorSonido.Instance?.SetSonidoActivado(prendio);
                break;

            case TipoToggle.Musica:
                ControladorSonido.Instance?.SetMusicaActivada(prendio);
                break;

            case TipoToggle.ModoZurdo:
                PlayerPrefs.SetInt("ModoZurdo", prendio ? 1 : 0);
                PlayerPrefs.Save();
                ControladorModoZurdo.Instance?.ActivarModoZurdo(prendio);
                break;
        }

        // Notifica a los suscriptores del cambio
        OnValueChanged?.Invoke(prendio);
    }

    /// <summary>
    /// Establece de manera explícita el estado del interruptor.
    /// </summary>
    /// <param name="value">Nuevo estado booleano (true = encendido, false = apagado).</param>
    /// <param name="notify">Determina si se debe invocar el evento OnValueChanged.</param>
    public void SetValue(bool value, bool notify = true)
    {
        prendio = value;
        targetFillAmount = prendio ? 1f : 0f;

        UpdateHandlePivot();
        UpdateFillAmount();
        UpdateStateText();

        if (notify)
            OnValueChanged?.Invoke(prendio);
    }

    /// <summary>
    /// Propiedad pública de lectura para consultar si el interruptor está activado.
    /// </summary>
    public bool IsOn => prendio;

    /// <summary>
    /// Método de actualización por frame. Interpola suavemente (Lerp) la barra de relleno y la posición 
    /// de la manecilla para lograr un movimiento fluido al conmutar estados.
    /// </summary>
    void Update()
    {
        float fillSpeed = 5f;
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * fillSpeed);

            if (handle != null)
            {
                float posX = prendio ? fillImage.rectTransform.rect.width : 0f;
                handle.anchoredPosition = new Vector2(Mathf.Lerp(handle.anchoredPosition.x, posX, Time.deltaTime * 8f), handle.anchoredPosition.y);
            }
        }
    }

    /// <summary>
    /// Actualiza instantáneamente el relleno visual y la posición de la manecilla sin interpolación suave.
    /// Usado principalmente en la inicialización.
    /// </summary>
    private void UpdateFillAmount()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = targetFillAmount;
            if (handle != null)
            {
                handle.anchoredPosition = new Vector2(targetFillAmount * fillImage.rectTransform.rect.width, handle.anchoredPosition.y);
            }
        }
    }

    /// <summary>
    /// Actualiza el texto formateado ("Sí" / "No") y cambia su color según el estado del interruptor.
    /// </summary>
    private void UpdateStateText()
    {
        if (stateText != null)
        {
            stateText.text = prendio ? "Sí" : "No";
            stateText.color = prendio ? onColor : offColor;
        }
    }

    /// <summary>
    /// Ajusta el punto de pivote horizontal de la manecilla según el estado 
    /// para asegurar que se alinee correctamente con los bordes de la barra de fondo.
    /// </summary>
    private void UpdateHandlePivot()
    {
        if (handle != null)
        {
            if (prendio)
                handle.pivot = new Vector2(1f, 0.5f);
            else
                handle.pivot = new Vector2(0f, 0.5f);
        }
    }
}