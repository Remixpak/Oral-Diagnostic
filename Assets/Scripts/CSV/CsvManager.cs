using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la carga, almacenamiento y consulta centralizada de datos estructurados desde archivos CSV 
/// y recursos de imágenes en Unity. Implementa el patrón Singleton para persistir entre escenas.
/// 
/// Clases dependientes que utiliza:
/// - Patologia: Representa datos de afecciones clínicas (relaciona lesión, familia, etiología e imagen).
/// - Lesion: Define el tipo de lesión asociada y mantiene sus identificadores de descripción.
/// - Familia: Clasificación taxonómica o agrupador de patologías.
/// - Etiologia: Origen o causa de la condición médica.
/// - Descripcion: Textos descriptivos vinculados individualmente a las lesiones.
/// </summary>
public class CsvManager : MonoBehaviour
{
    public static CsvManager Instance { get; private set; } 
    public List<Patologia> patologias { get; private set; }
    public List<Lesion> lesiones { get; private set; }
    public List<Familia> familias { get; private set; }
    public List<Etiologia> etiologias { get; private set; }
    public List<Descripcion> descripciones { get; private set; }

    private Dictionary<string, Sprite> imagenes;

    /// <summary>
    /// Configura la instancia Singleton, inicializa las colecciones y desencadena el proceso de carga de datos e imágenes.
    /// </summary>
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

    /// <summary>
    /// Ejecuta de manera secuencial la lectura de todos los archivos CSV guardados en Resources.
    /// </summary>
    private void CargarTodos()
    {
        CargarDescripciones();
        CargarPatologias();
        CargarLesiones();
        CargarFamilias();
        CargarEtiologias();
    }

    /// <summary>
    /// Carga todos los sprites almacenados en la carpeta Resources/Imagenes dentro de un diccionario para acceso rápido.
    /// </summary>
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

    /// <summary>
    /// Lee y procesa el archivo CSV 'descripciones' instanciando la lista de Descripcion.
    /// </summary>
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

    /// <summary>
    /// Lee y procesa el archivo CSV 'lesiones' instanciando objetos Lesion y asociando los IDs de sus descripciones.
    /// </summary>
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

    /// <summary>
    /// Lee y procesa el archivo CSV 'patologias' creando instancias de Patologia vinculadas a sus relaciones.
    /// </summary>
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

    /// <summary>
    /// Lee y procesa el archivo CSV 'familias' llenando la lista de categorias Familia.
    /// </summary>
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

    /// <summary>
    /// Lee y procesa el archivo CSV 'etiologias' registrando los orígenes de las patologías.
    /// </summary>
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

    /// <summary>
    /// Busca y retorna un objeto Patologia según su ID identificador.
    /// </summary>
    public Patologia ObtenerPatologiaPorId(int id) => patologias.Find(p => p.id == id);

    /// <summary>
    /// Busca y retorna un objeto Lesion según su ID identificador.
    /// </summary>
    public Lesion ObtenerLesionPorId(int id) => lesiones.Find(l => l.id == id);

    /// <summary>
    /// Busca y retorna un objeto Familia según su ID identificador.
    /// </summary>
    public Familia ObtenerFamiliaPorId(int id) => familias.Find(f => f.id == id);

    /// <summary>
    /// Busca y retorna un objeto Etiologia según su ID identificador.
    /// </summary>
    public Etiologia ObtenerEtiologiaPorId(int id) => etiologias.Find(e => e.id == id);

    /// <summary>
    /// Busca y retorna un objeto Descripcion según su ID identificador.
    /// </summary>
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

    /// <summary>
    /// Obtiene un Sprite previamente cargado desde el diccionario utilizando su código identificador.
    /// </summary>
    public Sprite spritePorCodigo(string codigo)
    {
        if(imagenes.TryGetValue(codigo, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogError("No se encontró la imagen con el código: " + codigo);
        return null;
    }

    /// <summary>
    /// Filtra y retorna la lista de patologías vinculadas a una lesión específica por su ID.
    /// </summary>
    public List<Patologia> ObtenerPatologiasPorLesion(int lesionId)
    {
        return patologias.Where(p => p.lesionID == lesionId).ToList();
    }

    /// <summary>
    /// Obtiene el Sprite correspondiente a una patología utilizando únicamente su ID.
    /// </summary>
    public Sprite ObtenerSpritePorPatologiaId(int patologiaId)
    {
        Patologia p = ObtenerPatologiaPorId(patologiaId);
        return p != null ? spritePorCodigo(p.codigoImagen) : null;
    }

    /// <summary>
    /// Carga de forma directa un Sprite desde Resources según el código de imagen indicado en la patología.
    /// </summary>
    public Sprite ObtenerSpriteDePatologia(Patologia patologia)
    {
        if (patologia == null) return null;
        
        return Resources.Load<Sprite>($"Imagenes/{patologia.codigoImagen}"); 
    }
}