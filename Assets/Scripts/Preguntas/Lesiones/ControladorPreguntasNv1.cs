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
    [SerializeField] private TMP_Text textoSeleccionMultiple;

    [Header("Quinta Alternativa")]
    [SerializeField] private Button botonQuintaAlternativa;
    [SerializeField] private GameObject panelQuintaAlternativa;

    private List<string> respuestasCorrectasLista = new List<string>();
    private List<string> respuestasSeleccionadasLista = new List<string>();
    private List<Button> botonesSeleccionados = new List<Button>();

    void Start()
    {
        tipoMateria = "lesion";
    }

    public override void InicializarPregunta(int idPatologiaAsignada)
    {
        //lamentablemente como maestro chasqsuilla por inicializamos el inicializarpregunta aqui ya para poder agregar la quinta opcion ya que no encontre una mejor forma de hacerlo
        // siendo el unico posible problema a futuro es que si se actualiza el controlador padre se deba tambien actualizar el hijo para que no se rompa la funcionalidad de la quinta opcion
        yaRespondio = false;

        foreach (Button btn in botonesAlternativas)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
            btn.onClick.RemoveAllListeners();

            Image childImg = GetImageInChild(btn);
            if (childImg != null) childImg.gameObject.SetActive(false);

            TMP_Text childText = btn.GetComponentInChildren<TMP_Text>();
            if (childText != null) childText.gameObject.SetActive(true);
        }

        if (botonQuintaAlternativa != null)
        {
            botonQuintaAlternativa.interactable = true;
            botonQuintaAlternativa.GetComponent<Image>().color = Color.white;
            botonQuintaAlternativa.onClick.RemoveAllListeners();

            Image childImgQuinta = GetImageInChild(botonQuintaAlternativa);
            if (childImgQuinta != null) childImgQuinta.gameObject.SetActive(false);

            TMP_Text childTextQuinta = botonQuintaAlternativa.GetComponentInChildren<TMP_Text>();
            if (childTextQuinta != null) childTextQuinta.gameObject.SetActive(true);

            botonQuintaAlternativa.gameObject.SetActive(false);
        }

        if (panelQuintaAlternativa != null)
        {
            panelQuintaAlternativa.SetActive(false);
        }

        List<string> opcionesTexto;
        List<Sprite> opcionesSprite;
        ConfigurarPreguntaYRespuestas(idPatologiaAsignada, out opcionesTexto, out opcionesSprite);

        MezclarOpciones(opcionesTexto, opcionesSprite);

        for (int i = 0; i < botonesAlternativas.Count; i++)
        {
            Button btnActual = botonesAlternativas[i];

            if (opcionesSprite != null && opcionesSprite.Count > i && opcionesSprite[i] != null)
            {
                btnActual.gameObject.SetActive(true);

                TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.gameObject.SetActive(false);

                Image imgChild = GetImageInChild(btnActual);
                if (imgChild != null)
                {
                    imgChild.gameObject.SetActive(true);
                    imgChild.sprite = opcionesSprite[i];
                    imgChild.preserveAspect = true;
                }

                string valorRespuesta = opcionesTexto[i];
                btnActual.onClick.AddListener(() => SeleccionarAlternativa(btnActual, valorRespuesta));
            }
            else if (opcionesTexto != null && opcionesTexto.Count > i)
            {
                btnActual.gameObject.SetActive(true);

                Image imgChild = GetImageInChild(btnActual);
                if (imgChild != null) imgChild.gameObject.SetActive(false);

                TMP_Text txt = btnActual.GetComponentInChildren<TMP_Text>(true);
                if (txt != null)
                {
                    txt.gameObject.SetActive(true);
                    txt.text = opcionesTexto[i];
                }

                string valorRespuesta = opcionesTexto[i];
                btnActual.onClick.AddListener(() => SeleccionarAlternativa(btnActual, valorRespuesta));
            }
            else
            {
                btnActual.gameObject.SetActive(false);
            }
        }

        if (opcionesTexto != null && opcionesTexto.Count > botonesAlternativas.Count && botonQuintaAlternativa != null)
        {
            botonQuintaAlternativa.gameObject.SetActive(true);
            if (panelQuintaAlternativa != null)
            {
                panelQuintaAlternativa.SetActive(true);
            }

            TMP_Text txtQuinta = botonQuintaAlternativa.GetComponentInChildren<TMP_Text>(true);
            if (txtQuinta != null)
            {
                txtQuinta.gameObject.SetActive(true);
                txtQuinta.text = opcionesTexto[botonesAlternativas.Count];
            }

            string valorRespuestaQuinta = opcionesTexto[botonesAlternativas.Count];
            botonQuintaAlternativa.onClick.AddListener(() => SeleccionarAlternativa(botonQuintaAlternativa, valorRespuestaQuinta));
        }
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

        if (textoSeleccionMultiple != null)
        {
            bool esMultiple = respuestasCorrectasLista.Count > 1;
            textoSeleccionMultiple.gameObject.SetActive(esMultiple);
            if (esMultiple)
            {
                textoSeleccionMultiple.text = $"Selecciona {respuestasCorrectasLista.Count} alternativas.";
            }
        }

        foreach (string resp in respuestasCorrectasLista)
        {
            opciones.Add(resp);
        }

        HashSet<string> posiblesDistractores = new HashSet<string>();

        foreach (Lesion l in CsvManager.Instance.lesiones)
        {
            if (string.IsNullOrEmpty(l.nombre)) continue;

            string[] nombresDesglosados = l.nombre.Split('/');

            foreach (string nombreUnico in nombresDesglosados)
            {
                string nombreLimpio = nombreUnico.Trim();

                if (!string.IsNullOrEmpty(nombreLimpio) && !respuestasCorrectasLista.Contains(nombreLimpio))
                {
                    posiblesDistractores.Add(nombreLimpio);
                }
            }
        }

        List<string> listaDistractores = posiblesDistractores.ToList();
        listaDistractores = listaDistractores.OrderBy(x => Random.value).ToList();

        int contadorSeguridad = 0;
        int maxIntentos = 100;

        int cantidadTotalDeseada = botonesAlternativas.Count;
        if (respuestasCorrectasLista.Count == 4 && botonQuintaAlternativa != null)
        {
            cantidadTotalDeseada = botonesAlternativas.Count + 1;
        }

        while (opciones.Count < cantidadTotalDeseada && contadorSeguridad < maxIntentos)
        {
            contadorSeguridad++;

            if (listaDistractores.Count > 0)
            {
                string distractor = listaDistractores[0];
                listaDistractores.RemoveAt(0);

                if (!string.IsNullOrWhiteSpace(distractor) && !distractor.Contains("/"))
                {
                    opciones.Add(distractor);
                }
            }
            else
            {
                string distractorFalso = $"Lesión falsa {opciones.Count + 1}";
                opciones.Add(distractorFalso);
            }
        }

        while (opciones.Count < cantidadTotalDeseada)
        {
            opciones.Add($"Lesión {opciones.Count + 1}");
        }

        opciones = opciones.OrderBy(x => Random.value).ToList();
    }

    protected override void SeleccionarAlternativa(Button boton, string valorSeleccionado)
    {
        if (yaRespondio) return;

        if (respuestasCorrectasLista.Count <= 1)
        {
            base.SeleccionarAlternativa(boton, valorSeleccionado);
            return;
        }

        if (botonesSeleccionados.Contains(boton))
        {
            botonesSeleccionados.Remove(boton);
            respuestasSeleccionadasLista.Remove(valorSeleccionado);
            boton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            botonesSeleccionados.Add(boton);
            respuestasSeleccionadasLista.Add(valorSeleccionado);
            boton.GetComponent<Image>().color = new Color(0.8f, 0.9f, 1f);
        }

        if (respuestasSeleccionadasLista.Count >= respuestasCorrectasLista.Count)
        {
            EvaluarRespuestaMultiple();
        }
    }

    private bool esRespuestaCorrecta = false;

    private void EvaluarRespuestaMultiple()
    {
        yaRespondio = true;

        foreach (Button btn in botonesAlternativas)
            btn.interactable = false;

        if (botonQuintaAlternativa != null)
            botonQuintaAlternativa.interactable = false;

        esRespuestaCorrecta = respuestasSeleccionadasLista.Count == respuestasCorrectasLista.Count &&
                            !respuestasSeleccionadasLista.Except(respuestasCorrectasLista).Any();

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

    public override void EntregarRetroalimentacion()
    {
        if (canvasJuego != null) canvasJuego.gameObject.SetActive(false);

        if (respuestasCorrectasLista.Count <= 1)
        {
            esRespuestaCorrecta = (respuestaSeleccionada == respuestaCorrecta);
        }

        if (esRespuestaCorrecta)
        {
            textoResultado.text = "¡Respuesta Correcta!";

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
            textoRespuesta.text = "";
        }

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

    private Image GetImageInChild(Button btn)
    {
        foreach (Image img in btn.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject != btn.gameObject) return img;
        }
        return null;
    }
}