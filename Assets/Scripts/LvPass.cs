using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LvPass : MonoBehaviour
{
    [SerializeField] private TMP_Text textoLv;
    [SerializeField] private Button BtnContinar;
    [SerializeField] private Button BtnReintentar;

    public void Continuar()
    {
        GameManager.Instance.ContinuarCarrera();
    }

    public void Salir()
    {
        SceneManager.LoadScene("PantallaInicio");
    }

    public void MostrarPass(int nivel)
    {
        nivel--;
        textoLv.text = $"Nivel {nivel} completado";
        BtnReintentar.gameObject.SetActive(false);
        BtnContinar.gameObject.SetActive(true);
    }
    public void MostrarReintento()
    {
        textoLv.text = "GameOver";
        BtnReintentar.gameObject.SetActive(true);
        BtnContinar.gameObject.SetActive(false);

    }
}