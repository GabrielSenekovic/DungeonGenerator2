using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "EntityDatabase", menuName = "Dungeon Generator/EntityDatabase", order = 2)]
public class EntityDatabase :ScriptableObject
{
    [System.Serializable]public class DatabaseEntry
    {
        [System.Serializable]public class MeshLOD
        {
            public Mesh mesh;
            public float renderDistance;
        }
        public string name;
        public List<MeshLOD> mesh;
    }
    public List<DatabaseEntry> database;

    public void Initialise(string value)
    {
        string[] words = value.Split(' ', '\n');
        for(int i = 0; i < words.Length; i++)
        {
            //Split "words" into arrays where the values are grouped together
        }
    }

    public Mesh GetMesh(string value, float distance)
    {
        DatabaseEntry entry = GetMeshLOD(value);
        if(entry != null)
        {
            for(int i = 0; i < entry.mesh.Count; i++)
            {
                if(distance < entry.mesh[i].renderDistance)
                {
                    return entry.mesh[i].mesh;
                }
            }
        }
        return null;
    }

    public DatabaseEntry GetMeshLOD(string value)
    {
        for(int i = 0; i < database.Count; i++)
        {
            if(database[i].name == value)
            {
                return database[i];
            }
        }
        return null;
    }
}
