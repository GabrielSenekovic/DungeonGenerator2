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
}
