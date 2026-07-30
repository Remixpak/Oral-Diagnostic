using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using UnityEngine.XR;
public enum TipoToggle
{
    Sonido,
    ModoZurdo
}



public class ToggleSwitch: MonoBehaviour
{
    [Header("Elementos")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image handleImage;
    [SerializeField] private TextMeshProUGUI stateText;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool>{}

    [Header("Eventos")]
    public BoolEvent OnValueChanged;


    private RectTransform handle;
    private bool prendio = false;
    private float targetFillAmount;

    [SerializeField] private Color onColor = Color.white;
    [SerializeField] private Color offColor = new Color(37f/ 255f, 37f / 255f, 37f/ 255f);

    [SerializeField] private TipoToggle tipo;
    private void Awake()
    {
        handle = handleImage.GetComponent<RectTransform>();
    }

    private void Start()
    {
        bool estado = false;

        switch (tipo)
        {
            case TipoToggle.Sonido:
                estado = ControladorSonido.Instance != null &&
                        ControladorSonido.Instance.SonidoActivado();
                break;

            case TipoToggle.ModoZurdo:
                estado = PlayerPrefs.GetInt("ModoZurdo", 0) == 1;
                break;
        }

        SetValue(estado, false);
    }

    public void Toggle()
    {
        prendio = !prendio;
        targetFillAmount = prendio ? 1f : 0f;

        UpdateHandlePivot();
        UpdateStateText();

        switch (tipo)
        {
            case TipoToggle.Sonido:
                ControladorSonido.Instance?.SetSonidoActivado(prendio);
                break;

            case TipoToggle.ModoZurdo:
                PlayerPrefs.SetInt("ModoZurdo", prendio ? 1 : 0);
                PlayerPrefs.Save();
                ControladorModoZurdo.Instance?.ActivarModoZurdo(prendio);
                break;
        }

        OnValueChanged?.Invoke(prendio);
    }

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

    public bool IsOn => prendio;

    void Update()
    {
        float fillSpeed = 5f;
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
        float posX = prendio ? fillImage.rectTransform.rect.width : 0f;
        handle.anchoredPosition = new Vector2(Mathf.Lerp(handle.anchoredPosition.x, posX, Time.deltaTime * 8f), handle.anchoredPosition.y);

    }

    private void UpdateFillAmount()
    {
        fillImage.fillAmount = targetFillAmount;
        handle.anchoredPosition = new Vector2(targetFillAmount * fillImage.rectTransform.rect.width, handle.anchoredPosition.y);
    }

    private void UpdateStateText()
    {
        stateText.text = prendio? "Sí": "No";
        stateText.color = prendio? onColor : offColor;
    }

    private void UpdateHandlePivot()
    {
        if(prendio)
            handle.pivot = new Vector2(1f, 0.5f);
        else
            handle.pivot = new Vector2(0f, 0.5f);
    }
}