using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CsvManager : MonoBehaviour
{
    public static CsvManager Instance { get; private set; } 
    public List<Patologia> patologias { get; private set; }
    public List<Lesion> lesiones { get; private set; }
    public List<Familia> familias { get; private set; }
    public List<Etiologia> etiologias { get; private set; }
    public List<Descripcion> descripciones { get; private set; }

    private Dictionary<string, Sprite> imagenes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        patologias = new List<Patologia>();
        lesiones = new List<Lesion>();
        familias = new List<Familia>();
        etiologias = new List<Etiologia>();
        descripciones = new List<Descripcion>();
        imagenes = new Dictionary<string, Sprite>();

        CargarTodos();
        CargarImagenes();
    }

    private void CargarTodos()
    {
        CargarDescripciones(); // Cargamos primero las descripciones
        CargarPatologias();
        CargarLesiones();
        CargarFamilias();
        CargarEtiologias();
    }

    private void CargarImagenes()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Imagenes");
        foreach(Sprite s in sprites)
        {
            if(!imagenes.ContainsKey(s.name))
            {
                imagenes.Add(s.name, s);
            }
            else
            {
                Debug.LogWarning("Imagen duplicada: " + s.name);
            }
        }

        Debug.Log("Se cargaron " + imagenes.Count + " imágenes.");
    }

    private void CargarDescripciones()
    {
        TextAsset csv = Resources.Load<TextAsset>("CSV/descripciones");
        if (csv == null) return;

        string[] lineas = csv.text.Split('\n');
        for (int i = 1; i < lineas.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lineas[i]))
                continue;

            string[] datos = lineas[i].Split(',');
            Descripcion d = new Descripcion();
            d.id = int.Parse(datos[0].Trim());
            d.texto = datos[1].Trim();

            descripciones.Add(d);
        }
    }

    private void CargarLesiones()
    {
        TextAsset csv = Resources.Load<TextAsset>("CSV/lesiones");
        string[] lineas = csv.text.Split('\n');

        for (int i = 1; i < lineas.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lineas[i]))
                continue;

            string[] datos = lineas[i].Split(',');
            Lesion l = new Lesion();
            l.id = int.Parse(datos[0].Trim());
            l.nombre = datos[1].Trim();

            // Si la 3ra columna (índice 2) contiene IDs separados por ';' (ej: "1;3;5")
            if (datos.Length > 2 && !string.IsNullOrWhiteSpace(datos[2]))
            {
                string[] idsDesc = datos[2].Trim().Split(';');
                foreach (string idStr in idsDesc)
                {
                    if (int.TryParse(idStr.Trim(), out int idDesc))
                    {
                        l.descripcionIDs.Add(idDesc);
                    }
                }
            }

            lesiones.Add(l);
        }
    }

    private void CargarPatologias()
    {
        TextAsset csv = Resources.Load<TextAsset>("CSV/patologias");
        string[] lineas = csv.text.Split('\n');
        for (int i = 1; i < lineas.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lineas[i]))
                continue;

            string[] datos = lineas[i].Split(',');
            Patologia p = new Patologia();
            p.id = int.Parse(datos[0].Trim());
            p.nombre = datos[1].Trim();
            p.lesionID = int.Parse(datos[2].Trim());
            p.familiaID = int.Parse(datos[3].Trim());
            p.etiologiaID = int.Parse(datos[4].Trim());
            p.codigoImagen = datos[5].Trim();

            patologias.Add(p);
        }
    }

    private void CargarFamilias()
    {
        TextAsset csv = Resources.Load<TextAsset>("CSV/familias");
        string[] lineas = csv.text.Split('\n');
        for (int i = 1; i < lineas.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lineas[i]))
                continue;

            string[] datos = lineas[i].Split(',');
            Familia f = new Familia();
            f.id = int.Parse(datos[0].Trim());
            f.nombre = datos[1].Trim();

            familias.Add(f);
        }
    }

    private void CargarEtiologias()
    {
        TextAsset csv = Resources.Load<TextAsset>("CSV/etiologias");
        string[] lineas = csv.text.Split('\n');
        for (int i = 1; i < lineas.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lineas[i]))
                continue;

            string[] datos = lineas[i].Split(',');
            Etiologia e = new Etiologia();
            e.id = int.Parse(datos[0].Trim());
            e.nombre = datos[1].Trim();

            etiologias.Add(e);
        }
    }

    // --- Métodos de Búsqueda ---

    public Patologia ObtenerPatologiaPorId(int id) => patologias.Find(p => p.id == id);
    public Lesion ObtenerLesionPorId(int id) => lesiones.Find(l => l.id == id);
    public Familia ObtenerFamiliaPorId(int id) => familias.Find(f => f.id == id);
    public Etiologia ObtenerEtiologiaPorId(int id) => etiologias.Find(e => e.id == id);
    public Descripcion ObtenerDescripcionPorId(int id) => descripciones.Find(d => d.id == id);

    /// <summary>
    /// Devuelve la lista completa de objetos Descripcion pertenecientes a una lesión.
    /// </summary>
    public List<Descripcion> ObtenerDescripcionesDeLesion(Lesion lesion)
    {
        if (lesion == null || lesion.descripcionIDs == null) 
            return new List<Descripcion>();

        return descripciones.Where(d => lesion.descripcionIDs.Contains(d.id)).ToList();
    }

    /// <summary>
    /// Sobrecarga para obtener descripciones directamente pasándole el ID de la Lesión.
    /// </summary>
    public List<Descripcion> ObtenerDescripcionesDeLesionPorId(int lesionId)
    {
        Lesion l = ObtenerLesionPorId(lesionId);
        return ObtenerDescripcionesDeLesion(l);
    }

    public Sprite spritePorCodigo(string codigo)
    {
        if(imagenes.TryGetValue(codigo, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogError("No se encontró la imagen con el código: " + codigo);
        return null;
    }
}