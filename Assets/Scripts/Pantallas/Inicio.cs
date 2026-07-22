using UnityEngine;
using UnityEngine.SceneManagement;

public class Inicio: MonoBehaviour
{
    [Header("PanelAjustes")]
    [SerializeField] private GameObject panelAjustes;

    public void ActivarAjustes()
    {
        panelAjustes.SetActive(true);
    }
    public void DesactivarAjustes()
    {
        panelAjustes.SetActive(false);
    }

    public void ModoZurdo(bool activado)
    {
        //logica para el modo zurdo :p
    }

    public void DesactivarSonido(bool activado)
    {
        //logica para desactivar los sonidos
    }

    public void IrAJugar()
    {
        SceneManager.LoadScene("PantallaSeleccion");
    }
}