using UnityEngine;
using System.Collections.Generic;

public class CsvManager : MonoBehaviour
{
    public static CsvManager Instance { get; private set; } 
    public List<Patologia> patologias { get; private set; }
    public List<Lesion> lesiones { get;  private set; }
    public List<Familia> familias { get; private set; }
    public List<Etiologia> etiologias { get; private set; }

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
    }

    private void CargarTodos()
    {
        CargarPatologias();
        CargarLesiones();
        CargarFamilias();
        CargarEtiologias();
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
            p.id = int.Parse(datos[0]);
            p.nombre = datos[1];
            p.lesionID = int.Parse(datos[2]);
            p.familiaID = int.Parse(datos[3]);
            p.etiologiaID = int.Parse(datos[4]);
            p.codigoImagen = datos[5];
            patologias.Add(p);
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
            l.id = int.Parse(datos[0]);
            l.nombre = datos[1];

            lesiones.Add(l);
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
            f.id = int.Parse(datos[0]);
            f.nombre = datos[1];

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
            e.id = int.Parse(datos[0]);
            e.nombre = datos[1];

            etiologias.Add(e);
        }
    }

    public Patologia ObtenerPatologiaPorId(int id)
    {
        return patologias.Find(p => p.id == id);
    }
    public Lesion ObtenerLesionPorId(int id)
    {
        return lesiones.Find(l => l.id == id);
    }
    public Familia ObtenerFamiliaPorId(int id)
    {
        return familias.Find(f => f.id == id);
    }
    public Etiologia ObtenerEtiologiaPorId(int id)
    {
        return etiologias.Find(e => e.id == id);
    }
}
