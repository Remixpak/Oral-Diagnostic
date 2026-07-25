using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LvPass : MonoBehaviour
{
    [SerializeField] private TMP_Text textoLv;
    [SerializeField] private Button BtnContinar;
    [SerializeField] private Button BtnReintentar;

    private void Awake()
    {
        // Nos aseguramos por código de que los botones escuchen los clics/toques
        if (BtnContinar != null)
        {
            BtnContinar.onClick.RemoveAllListeners();
            BtnContinar.onClick.AddListener(Continuar);
        }

        if (BtnReintentar != null)
        {
            BtnReintentar.onClick.RemoveAllListeners();
            BtnReintentar.onClick.AddListener(Reintentar);
        }
    }

    public void Continuar()
    {
        Debug.Log("Btn Continuar presionado");
        GameManager.Instance.ContinuarCarrera();
    }

    public void Reintentar()
    {
        Debug.Log("Btn Reintentar presionado");
        GameManager.Instance.ContinuarCarrera(); // Desbloquea la espera del WaitUntil para repetir el nivel
    }

    public void Salir()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("PantallaInicio");
    }

    public void MostrarPass(int nivel)
    {
        nivel--;
        if (textoLv != null)
            textoLv.text = $"Nivel {nivel} completado";

        if (BtnReintentar != null) BtnReintentar.gameObject.SetActive(false);
        if (BtnContinar != null) BtnContinar.gameObject.SetActive(true);
    }

    public void MostrarReintento()
    {
        if (textoLv != null)
            textoLv.text = "GameOver";

        if (BtnReintentar != null) BtnReintentar.gameObject.SetActive(true);
        if (BtnContinar != null) BtnContinar.gameObject.SetActive(false);
    }
}