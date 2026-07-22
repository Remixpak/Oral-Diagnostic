using System.Collections.Generic;

public interface IFirestoreData
{
    Dictionary<string, object> ToFirestore();
}