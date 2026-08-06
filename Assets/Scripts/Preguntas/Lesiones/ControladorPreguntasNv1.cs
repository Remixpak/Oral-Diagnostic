/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class ControladorPreguntasNv1 : ControladorPreguntas
{
    [Header("Atributos")]
    [SerializeField] private string pregunta;
    [SerializeField] private string respuestaCorrecta;
    [SerializeField] private string respuesta;
    [SerializeField] private List<string> alternativas;

    private int idPatologiaNumerica;
    private string idPatologia;
    [SerializeField] private Image imagen;

    [Header("Botones")]
    [SerializeField] private List<Button> botonesAlternativas;
    [SerializeField] private Button botonPausa;

    [Header("Textos de retroalimentacion")]
    [SerializeField] public TMP_Text textoResultado;
    [SerializeField] public TMP_Text textoRespuesta;

    private bool yaRespondio = false;

    void Start()
    {

    }

    void Update()
    {

    }

    public void ObtenerImagen()
    {
        Patologia patologia = CsvManager.Instance.ObtenerPatologiaPorId(int.Parse(idPatologia));
        imagen.sprite = CsvManager.Instance.spritePorCodigo(patologia.codigoImagen);
    }

    public void ObtnerLesion()
    {
        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaNumerica);
        Lesion l = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = l.nombre;
        alternativas.Add(respuestaCorrecta);
    }

    public bool ComprobarRespuesta(string respuesta, string respuestaCorrecta)
    {
        return respuesta == respuestaCorrecta;
    }

    public void SeleccionarAlternativa(Button boton)
    {
        if (yaRespondio) return;
        yaRespondio = true;

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = false;
        }

        respuesta = boton.GetComponentInChildren<TMP_Text>().text;

        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            boton.GetComponent<Image>().color = Color.green;
            GameManager.Instance.Aciertos++;
            GameManager.Instance.TotalAciertos++;
        }
        else
        {
            boton.GetComponent<Image>().color = Color.red;
            GameManager.Instance.Fallos++;
            GameManager.Instance.TotalFallos++;
        }

        StartCoroutine(FinalizarPregunta());
    }

    public void RellenarRespuestas()
    {
        alternativas.Clear();

        alternativas.Add(respuestaCorrecta);
        List<Lesion> l = new List<Lesion>(CsvManager.Instance.lesiones);
        l.RemoveAll(l => l.nombre == respuestaCorrecta);

        while (alternativas.Count < botonesAlternativas.Count)
        {
            int indice = Random.Range(0, l.Count);
            alternativas.Add(l[indice].nombre);
            l.RemoveAt(indice);
        }

        for (int i = 0; i < alternativas.Count; i++)
        {
            int j = Random.Range(0, alternativas.Count);
            (alternativas[i], alternativas[j]) = (alternativas[j], alternativas[i]);
        }

        for (int i = 0; i < botonesAlternativas.Count; i++)
        {
            botonesAlternativas[i].GetComponentInChildren<TMP_Text>().text = alternativas[i];
        }
    }

    public override void EntregarRetroalimentacion()
    {
        canvasJuego.gameObject.SetActive(false);

        if (ComprobarRespuesta(respuesta, respuestaCorrecta))
        {
            textoResultado.text = "¡Respuesta Correcta!";
            textoRespuesta.text = "La respuesta correcta es: " + respuestaCorrecta;
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
            textoRespuesta.text = " ";
        }

        
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        canvasRetroalimentacion.GetComponentInChildren<Button>().onClick.AddListener(() => finished = true);
        canvasRetroalimentacion.gameObject.SetActive(true);
    }

    public override void InicializarPregunta(int indPatologiaAsignada)
    {
        ConfigurarBotonPausa();
        yaRespondio = false;
        idPatologiaNumerica = indPatologiaAsignada;
        idPatologia = idPatologiaNumerica.ToString();

        alternativas = new List<string>();

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
        }

        ObtnerLesion();
        ObtenerImagen();
        RellenarRespuestas();

        foreach (Button boton in botonesAlternativas)
        {
            boton.onClick.RemoveAllListeners();
            Button botonActual = boton;
            botonActual.onClick.AddListener(() => SeleccionarAlternativa(botonActual));
        }
    }

    private IEnumerator FinalizarPregunta()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }

    private void ConfigurarBotonPausa()
    {
        if (botonPausa == null)
        {
            Debug.LogWarning("[PausaDebug] El botonPausa es NULL en el Inspector.");
            return;
        }

        Debug.Log($"[PausaDebug] Configurando boton pausa. Nombre: {botonPausa.name}, Interactable antes: {botonPausa.interactable}, ActiveInHierarchy antes: {botonPausa.gameObject.activeInHierarchy}");

        botonPausa.onClick.RemoveAllListeners();
        botonPausa.onClick.AddListener(() => {
            Debug.Log("[PausaDebug] ¡Se presionó el botón de pausa!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PausarJuego();
            }
            else
            {
                Debug.LogError("[PausaDebug] GameManager.Instance es NULL al intentar pausar.");
            }
        });

        if (!botonPausa.interactable)
        {
            botonPausa.interactable = true;
            Debug.Log("[PausaDebug] El botonPausa estaba en false, se forzó a true.");
        }

        if (!botonPausa.gameObject.activeInHierarchy)
        {
            botonPausa.gameObject.SetActive(true);
            Debug.Log("[PausaDebug] El GameObject del botonPausa estaba inactivo, se activó.");
        }
    }
}*/

/*using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class PreguntaLesionPorImagen : ControladorPreguntaBase
{
    void Start()
    {
        tipoMateria = "lesion";
    }
    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        if (imagenPregunta != null)
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null)
            textoPregunta.text = "¿A qué lesión básica corresponde la manifestación clínica observada en la imagen?";

        Lesion lCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);
        respuestaCorrecta = lCorrecta.nombre;
        opciones.Add(respuestaCorrecta);

        // filtramos las lesiones para obtener las validad
        List<Lesion> restoLesiones = CsvManager.Instance.lesiones
            .Where(l => l.id != lCorrecta.id && !string.IsNullOrWhiteSpace(l.nombre))
            .ToList();

        // los mezclamos para tener un orden aleatorio
        restoLesiones = restoLesiones.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        while (opciones.Count < botonesAlternativas.Count && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (restoLesiones.Count > 0)
            {
                Lesion lDist = restoLesiones[0];
                restoLesiones.RemoveAt(0);

                //verificamos que el nombre no este vacio 
                if (!string.IsNullOrWhiteSpace(lDist.nombre))
                {
                    opciones.Add(lDist.nombre);
                }
                else
                {
                    // Si el nombre del campo esta vacio pasa al siguiente 
                    continue;
                }
            }
            else
            {
                // si detecta que faltan distractores se genera uno falso para evitar errores
                string distractorFalso = $"Lesión falsa {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        // si nos faltan opciones esta se rellenan 
        while (opciones.Count < botonesAlternativas.Count)
        {
            opciones.Add($"Lesión {opciones.Count + 1}");
        }

        // mezclamos las opciones finales
        opciones = opciones.OrderBy(x => Random.value).ToList();
    }
}*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class PreguntaLesionPorImagen : ControladorPreguntaBase
{
    [Header("Configuración Múltiple Selección")]
    [SerializeField] private TMP_Text textoSeleccionMultiple; // Texto o panel que se activa cuando hay más de 1 respuesta correcta

    private List<string> respuestasCorrectasLista = new List<string>();
    private List<string> respuestasSeleccionadasLista = new List<string>();
    private List<Button> botonesSeleccionados = new List<Button>();

    void Start()
    {
        tipoMateria = "lesion";
    }

    protected override void ConfigurarPreguntaYRespuestas(int idPatologiaAsignada, out List<string> opciones, out List<Sprite> spritesOpciones)
    {
        spritesOpciones = null;
        opciones = new List<string>();
        respuestasCorrectasLista.Clear();
        respuestasSeleccionadasLista.Clear();
        botonesSeleccionados.Clear();

        Patologia p = CsvManager.Instance.ObtenerPatologiaPorId(idPatologiaAsignada);
        
        if (imagenPregunta != null) 
            imagenPregunta.sprite = CsvManager.Instance.spritePorCodigo(p.codigoImagen);

        if (textoPregunta != null) 
            textoPregunta.text = "¿A qué lesión(es) básica(s) corresponde la manifestación clínica observada en la imagen?";

        Lesion lCorrecta = CsvManager.Instance.ObtenerLesionPorId(p.lesionID);

        respuestaCorrecta = lCorrecta.nombre;

        // 1. Parsear respuestas correctas
        if (lCorrecta.nombre.Contains("/"))
        {
            respuestasCorrectasLista = lCorrecta.nombre.Split('/')
                                                    .Select(s => s.Trim())
                                                    .Where(s => !string.IsNullOrEmpty(s))
                                                    .ToList();
        }
        else
        {
            respuestasCorrectasLista.Add(lCorrecta.nombre.Trim());
        }

        // Activar o desactivar texto indicador de múltiple selección
        if (textoSeleccionMultiple != null)
        {
            bool esMultiple = respuestasCorrectasLista.Count > 1;
            textoSeleccionMultiple.gameObject.SetActive(esMultiple);
            if (esMultiple)
            {
                textoSeleccionMultiple.text = $"Selecciona {respuestasCorrectasLista.Count} alternativas.";
            }
        }

        // Agregar todas las respuestas correctas a las opciones
        foreach (string resp in respuestasCorrectasLista)
        {
            opciones.Add(resp);
        }

        // 2. Generar lista de distractores LIMPIOS (haciendo Split de las combinadas)
        HashSet<string> posiblesDistractores = new HashSet<string>();

        foreach (Lesion l in CsvManager.Instance.lesiones)
        {
            if (string.IsNullOrEmpty(l.nombre)) continue;

            // Si la lesión en el CSV tiene '/', las dividimos en nombres individuales
            string[] nombresDesglosados = l.nombre.Split('/');
            
            foreach (string nombreUnico in nombresDesglosados)
            {
                string nombreLimpio = nombreUnico.Trim();

                // Solo agregamos si no es una respuesta correcta y no está vacía
                if (!string.IsNullOrEmpty(nombreLimpio) && !respuestasCorrectasLista.Contains(nombreLimpio))
                {
                    posiblesDistractores.Add(nombreLimpio);
                }
            }
        }

        // Convertir el HashSet a Lista para selección aleatoria
        List<string> listaDistractores = posiblesDistractores.ToList();

        // 3. Rellenar los botones restantes con distractores únicos e individuales
        while (opciones.Count < botonesAlternativas.Count && listaDistractores.Count > 0)
        {
            int idx = Random.Range(0, listaDistractores.Count);
            opciones.Add(listaDistractores[idx]);
            listaDistractores.RemoveAt(idx); // Evita duplicar el mismo distractor en la misma pregunta
        }
    }

    protected override void SeleccionarAlternativa(Button boton, string valorSeleccionado)
    {
        if (yaRespondio) return;

        // Caso A: Solo hay 1 respuesta correcta (Comportamiento estándar por defecto)
        if (respuestasCorrectasLista.Count <= 1)
        {
            base.SeleccionarAlternativa(boton, valorSeleccionado);
            return;
        }

        // Caso B: Hay Múltiples respuestas correctas
        // Toggle de selección al presionar el botón
        if (botonesSeleccionados.Contains(boton))
        {
            // Deseleccionar
            botonesSeleccionados.Remove(boton);
            respuestasSeleccionadasLista.Remove(valorSeleccionado);
            boton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            // Seleccionar
            botonesSeleccionados.Add(boton);
            respuestasSeleccionadasLista.Add(valorSeleccionado);
            boton.GetComponent<Image>().color = new Color(0.8f, 0.9f, 1f); // Color Azul/Gris claro indicando selección activa
        }

        // Verificar si ya se alcanzó el número de selecciones requeridas
        if (respuestasSeleccionadasLista.Count >= respuestasCorrectasLista.Count)
        {
            EvaluarRespuestaMultiple();
        }
    }

    // Añade este campo arriba con las demás variables de la clase
    private bool esRespuestaCorrecta = false; 

    // Reemplaza el método EvaluarRespuestaMultiple con esta versión que actualiza la variable esRespuestaCorrecta:
    private void EvaluarRespuestaMultiple()
    {
        yaRespondio = true;

        foreach (Button btn in botonesAlternativas) 
            btn.interactable = false;

        // Verificar si acertó a todas las opciones requeridas y no seleccionó ninguna incorrecta
        esRespuestaCorrecta = respuestasSeleccionadasLista.Count == respuestasCorrectasLista.Count &&
                            !respuestasSeleccionadasLista.Except(respuestasCorrectasLista).Any();

        // Visualizar respuestas en verde / rojo
        for (int i = 0; i < botonesSeleccionados.Count; i++)
        {
            Button btn = botonesSeleccionados[i];
            string resp = respuestasSeleccionadasLista[i];

            if (respuestasCorrectasLista.Contains(resp))
            {
                btn.GetComponent<Image>().color = Color.green;
            }
            else
            {
                btn.GetComponent<Image>().color = Color.red;
            }
        }

        // Registrar estadísticas
        if (GameManager.Instance != null)
        {
            if (esRespuestaCorrecta)
            {
                GameManager.Instance.Aciertos++;
                GameManager.Instance.TotalAciertos++;
            }
            else
            {
                GameManager.Instance.Fallos++;
                GameManager.Instance.TotalFallos++;
                if (tipoMateria == "lesion")
                    GameManager.Instance.FLesiones++;
                else if (tipoMateria == "familia")
                    GameManager.Instance.FFamilias++;
            }
        }

        if (textoSeleccionMultiple != null)
            textoSeleccionMultiple.gameObject.SetActive(false);

        StartCoroutine(FinalizarPreguntaRoutineLocal());
    }

    // Sobrescribimos el método de retroalimentación para adaptar el texto según sea selección simple o múltiple
    public override void EntregarRetroalimentacion()
    {
        if (canvasJuego != null) canvasJuego.gameObject.SetActive(false);

        // Caso A: Si es selección simple (1 sola respuesta correcta), usamos la validación base
        if (respuestasCorrectasLista.Count <= 1)
        {
            esRespuestaCorrecta = (respuestaSeleccionada == respuestaCorrecta);
        }

        // Evaluamos el resultado
        if (esRespuestaCorrecta)
        {
            textoResultado.text = "¡Respuesta Correcta!";
            
            // Si hay varias respuestas, se muestran formateadas adecuadamente (separadas por coma)
            if (respuestasCorrectasLista.Count > 1)
            {
                textoRespuesta.text = "Las respuestas correctas son: " + string.Join(", ", respuestasCorrectasLista);
            }
            else
            {
                textoRespuesta.text = "La respuesta correcta es: " + respuestaCorrecta;
            }
        }
        else
        {
            textoResultado.text = "Respuesta Incorrecta";
            textoRespuesta.text = "Las respuestas correctas eran: " + string.Join(", ", respuestasCorrectasLista);
        }

        // Activar Canvas de Retroalimentación
        if (canvasRetroalimentacion != null)
        {
            Button btnContinuar = canvasRetroalimentacion.GetComponentInChildren<Button>();
            btnContinuar.onClick.RemoveAllListeners();
            btnContinuar.onClick.AddListener(() => finished = true);
            canvasRetroalimentacion.gameObject.SetActive(true);
        }
    }

    private System.Collections.IEnumerator FinalizarPreguntaRoutineLocal()
    {
        yield return new WaitForSeconds(1.5f);
        EntregarRetroalimentacion();
    }
}