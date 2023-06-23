using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "AleaStory/MaterialDatabase", order = 4)]
public class MaterialDatabase : ScriptableObject
{
    [System.Serializable]
    public class DatabaseEntry
    {
        public string name = "";
        public Material material;
    }
    public List<DatabaseEntry> entries = new List<DatabaseEntry>();
}
