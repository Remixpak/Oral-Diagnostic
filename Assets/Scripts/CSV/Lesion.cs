using System;
using System.Collections.Generic;

[Serializable]
public class Lesion
{
    public int id;
    public string nombre;
    public List<int> descripcionIDs = new List<int>();
}

[Serializable]
public class Descripcion
{
    public int id;
    public string texto;
}

