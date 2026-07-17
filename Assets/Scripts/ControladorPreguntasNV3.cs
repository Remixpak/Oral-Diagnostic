using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorPreguntasNV3 : ControladorPreguntas
{

    [Header("Configuración del Juego")]

    [SerializeField] private string palabraCorrecta = "ULCERA"; //Objetivo a formar
    [SerializeField] private int cantidadLetrasTeclado = 12;// letras totaless

    [Header("Configuración del Juego")]

    [SerializeField] private Transform containerEspacios;
    [SerializeField] private Transform containerTeclado;
    [SerializeField] private GameObject prefabBotonLetra;

    [Header("Pistas clinicas")]
    [SerializeField] private Image uiImagePista;
    [SerializeField] private TextMeshProUGUI uiTextoLesion;
    [SerializeField] private TextMeshProUGUI uiTextoFamilia;
    [SerializeField] private TextMeshProUGUI uiTextoEtiopatogenia;

    [Header("Datos de la pista")]
    [SerializeField] private Sprite imagenPistaSprite;
    [SerializeField] private string NombreLesion = "Lesion Primaria";
    [SerializeField] private string NombreFamilia = "Dermatologica";
    [SerializeField] private string DescripcionEtiopatogenia = "Perdida continuidad de la piel";




    private List<string> letrasTeclado = new List<string>();
    private string[] progresoUsuario; //variable que guarda las letras que el usuario va ingresando

    //botones para interactuar
    private List<Button> botonesEspaciosUI = new List<Button>();
    private List<Button> botonesTecladoUI = new List<Button>();


    void Start()
    {
        progresoUsuario = new string[palabraCorrecta.Length];
        ConfigurarPanelPistas();
        GenerarLetrasTeclado();
        CrearEspaciosPalabra();
        CrearTeclado();
    }
    void Update()
    {
        
    }

    private void ConfigurarPanelPistas()
    {
        if (uiImagePista != null && imagenPistaSprite != null) uiImagePista.sprite = imagenPistaSprite;
        if (uiTextoLesion != null) uiTextoLesion.text = "Lesión: " + NombreLesion;
        if (uiTextoFamilia != null) uiTextoFamilia.text = "Familia: " + NombreFamilia;
        if (uiTextoEtiopatogenia != null) uiTextoEtiopatogenia.text = "Etiopatogenia: " + DescripcionEtiopatogenia;
    }

    //metodo para generarr las teclas en pantalla
    private void GenerarLetrasTeclado()
    {
        for (int i = 0; i < palabraCorrecta.Length; i++)//añade solamente las letras de la palabra correcta
        {
            letrasTeclado.Add(palabraCorrecta[i].ToString());
        }

        //añade con palabras extras hasta formar la cantidad total de palabras
        string abecedario = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        while (letrasTeclado.Count < cantidadLetrasTeclado)
        {
            string letraAleatoria = abecedario[Random.Range(0, abecedario.Length)].ToString();
            letrasTeclado.Add(letraAleatoria);
        }
        
        //mezcla las letras
        for (int i = 0; i < letrasTeclado.Count; i++) 
        {
            string temp = letrasTeclado[i];
            int randomIndex = Random.Range(i, letrasTeclado.Count);
            letrasTeclado[i] = letrasTeclado[randomIndex];
            letrasTeclado[randomIndex] = temp;
        }
    }

    //metodo para crear las casillas vacias de las palabras en pantalla 
    private void CrearEspaciosPalabra()
    {
        for (int i = 0; i < palabraCorrecta.Length; i++)
        {
            int index = i; //obtenemos una copia del listener del boton
            GameObject nuevoBoton = Instantiate(prefabBotonLetra, containerEspacios);
            Button btn = nuevoBoton.GetComponent<Button>();

            btn.GetComponentInChildren<TextMeshProUGUI>().text = "";//lo inicializamos vacio

            btn.onClick.AddListener(() => RemoverLetraDeEspacio(index));//si la letra ya existe y se clickea se elimina 

            botonesEspaciosUI.Add(btn);
        }
    }

    //crea botones para las letras disponibles
    private void CrearTeclado()
    {
        for (int i = 0; i < letrasTeclado.Count; i++)
        {
            int index = i; //obtenemos una copia del listener del boton
            GameObject nuevoBoton = Instantiate(prefabBotonLetra, containerTeclado);
            Button btn = nuevoBoton.GetComponent<Button>();

            string letra = letrasTeclado[index];
            btn.GetComponentInChildren<TextMeshProUGUI>().text = letra;

            //al presionar la tecla se posiciona en su lugar correspondiente
            btn.onClick.AddListener(() => SeleccionarLetraTeclado(index, letra));

            botonesTecladoUI.Add(btn);
        }
    }

    //metodo para seleccionar la tecla
    private void SeleccionarLetraTeclado(int indiceTeclado, string letra)
    {
        for (int i = 0; i < progresoUsuario.Length; i++)//busca espacio de izquierda a derecha
        {
            if (string.IsNullOrEmpty(progresoUsuario[i]))
            {
                progresoUsuario[i] = letra; //guara el progreso del usuario (la letra seleccionada)

                botonesEspaciosUI[i].GetComponentInChildren<TextMeshProUGUI>().text = letra; //actualiza la casilla visualmente

                botonesTecladoUI[indiceTeclado].gameObject.SetActive(false);//se desactiva el boton seleccionado para no repetir la letra 

                botonesEspaciosUI[i].name = indiceTeclado.ToString(); //guarda la referencia del indice del teclado para regresarla en caso de error

                ComprobarResultado();
                break;
            }
        }
    }


    //metodo para devolver la tecla a su posicion original
    private void RemoverLetraDeEspacio(int indiceEspacio)
    {
        if (!string.IsNullOrEmpty(progresoUsuario[indiceEspacio]))
        {
            if (int.TryParse(botonesEspaciosUI[indiceEspacio].name, out int indiceTecladoOriginal)) //obtenemos de que boton provenia la letra seleccionada
            {
                botonesTecladoUI[indiceTecladoOriginal].gameObject.SetActive(true);
            }
            //limpiamos el progreso 
            progresoUsuario[indiceEspacio] = null;
            
            //limpiamos visualmente la casilla 
            botonesEspaciosUI[indiceEspacio].GetComponentInChildren<TextMeshProUGUI>().text = "";
            botonesEspaciosUI[indiceEspacio].name = "Espacio";
            botonesEspaciosUI[indiceEspacio].GetComponent<Image>().color = Color.white; //restablece el color del espacio a blanco
        }
    }

    //verifica el resultado correcto con la palabra respuesta
    private void ComprobarResultado()
    {
        string palabraFormada = "";
        for (int i = 0; i < progresoUsuario.Length; i++)
        {
            if (string.IsNullOrEmpty(progresoUsuario[i])) return;//si hay un espacio vacio no se puede comprobar el resultado
            palabraFormada += progresoUsuario[i];
        }

        if (palabraFormada == palabraCorrecta)
        {
            Debug.Log("<color=green>¡Correcto! Has descubierto el diagnóstico clínico.</color>");
            foreach (Button btn in botonesEspaciosUI)
            {
                btn.GetComponent<Image>().color = Color.green;
            }
        }
        else
        {
            Debug.Log("<color=red>Palabra incorrecta. Sigue intentando.</color>");
            foreach (Button btn in botonesEspaciosUI)
            {
                btn.GetComponent<Image>().color = Color.red;
            }
        }
    }



    public override void EntregarRetroalimentacion()
    {
        // aun no hay la logica
    }

}
