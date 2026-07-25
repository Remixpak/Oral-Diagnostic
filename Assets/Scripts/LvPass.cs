using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LvPass : MonoBehaviour
{
    [SerializeField] private TMP_Text textoLv;

    public void Continuar()
    {
        GameManager.Instance.ContinuarCarrera();
    }

    public void Salir()
    {
        SceneManager.LoadScene("PantallaInicio");
    }

    public void Mostrar(int nivel)
    {
        nivel--;
        textoLv.text = $"Nivel {nivel} completado";
    }
}