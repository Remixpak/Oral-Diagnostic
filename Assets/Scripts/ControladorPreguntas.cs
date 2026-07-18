using UnityEngine;

public abstract class ControladorPreguntas : MonoBehaviour
{
    
    //quiza haya que llevar estas variables al gameManager mejor
    public bool finished;

    public void RegistrarMetricas()
    {

    }

    public abstract void EntregarRetroalimentacion();

    public abstract void InicializarPregunta(int indPatologiaAsignada);





    void Start()
    {
        finished = false;
    }

    void Update()
    {
        
    }
}
