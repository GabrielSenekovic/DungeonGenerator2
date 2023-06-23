using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

[System.Serializable]
[CreateAssetMenu(fileName = "FurnitureDatabase", menuName = "AleaStory/FurnitureDatabase", order = 3)]
public class FurnitureDatabase : ScriptableObject
{
    [System.Serializable]
    public class DatabaseEntry
    {
        public string name = "";
        public string type = "";
        public GameObject prefab;
        public Mesh mesh;
        public Sprite inventorySprite;
        public Vector2Int dimensions;
        public Vector3Int rotation;

        public DatabaseEntry(string name, string type, Sprite inventorySprite, Vector3Int? rotation = null, Vector2Int? dimensions = null)
        {
            this.name = name;
            this.type = type;
            this.inventorySprite = inventorySprite;
            if(dimensions != null)
            {
                this.dimensions = dimensions.Value;
            }
            if(rotation != null)
            {
                this.rotation = rotation.Value;
            }
        }
        public void AddObject(GameObject prefab)
        {
            this.prefab = prefab;
        }
        public void AddObject(Mesh mesh) //For generated at runtime
        {
            this.mesh = mesh;
        }
    }
    public List<DatabaseEntry> database;
    public Material baseMaterial;

    public void Initialise(string value)
    {
        database = new List<DatabaseEntry>();
        Material defaultMaterial = Resources.Load<Material>("Materials/Grass");
        string[] words = value.Split(' ', '\n');
        List<string> currentData = new List<string>();
        List<List<string>> allData = new List<List<string>>();
        for (int i = 0; i < words.Length; i++)
        {
            //Split "words" into arrays where the values are grouped together
            if (words[i].Length == 1) //If this word is just empty (I couldnt figure out what was the actual value of the string)
            {
                allData.Add(new List<string>(currentData));
                currentData.Clear();
            }
            else
            {
                currentData.Add(new string(words[i].Where(c => !char.IsControl(c)).ToArray()));
            }
        }
        allData.Add(new List<string>(currentData));
        for (int i = 0; i < allData.Count; i++)
        {
            string path = "Furniture/" + allData[i][1] + "/" + allData[i][1] + ".obj";
            GameObject furnitureObject = Resources.Load<GameObject>("Furniture/" + allData[i][1] + "/" + allData[i][1]);
            Sprite sprite = Resources.Load<Sprite>("Furniture/" + allData[i][1] + "/" + allData[i][1]);
            DatabaseEntry furnitureEntry = new DatabaseEntry(allData[i][1], "Furniture", sprite, new Vector3Int(0, 90, -90), new Vector2Int(2, 2));
            furnitureEntry.AddObject(furnitureObject);
            database.Add(furnitureEntry);
        }
        //Create vases
        for(int i = 0; i < 5; i++)
        {
            GameObject vase = MeshMaker.CreateVase(Resources.Load<Material>("Materials/Ground"));
            DatabaseEntry furnitureEntry = new DatabaseEntry("Vase " + i, "Furniture", null, Vector3Int.zero, new Vector2Int(2, 2)) ;
            furnitureEntry.AddObject(vase);
            database.Add(furnitureEntry);
        }
    }
    public DatabaseEntry GetRandomVarietyOfDatabaseEntry(string value)
    {
        List<DatabaseEntry> entries = new List<DatabaseEntry>();
        for (int i = 0; i < database.Count; i++)
        {
            if (database[i].name == value)
            {
                entries.Add(database[i]);
            }
        }
        return entries.GetRandom();
    }
    public DatabaseEntry GetDatabaseEntry(string value)
    {
        for (int i = 0; i < database.Count; i++)
        {
            if (database[i].name == value)
            {
                return database[i];
            }
        }
        return null;
    }
    public DatabaseEntry GetDatabaseEntry(int i)
    {
        if(i >= database.Count) { return database[database.Count - 1]; }
        return database[i];
    }
}

