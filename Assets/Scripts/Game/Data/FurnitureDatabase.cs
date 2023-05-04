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
        public Sprite inventorySprite;

        public DatabaseEntry(string name, string type, Sprite inventorySprite)
        {
            this.name = name;
            this.type = type;
            this.inventorySprite = inventorySprite;
        }
        public void AddObject(GameObject prefab)
        {
            this.prefab = prefab;
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
            //The txt files should always start with name and then type
            switch (allData[i][3]) //That means that number 3 is the type
            {
                case "Furniture":
                {
                    string path = "Furniture/" + allData[i][1] + "/" + allData[i][1] + ".obj";
                    GameObject furnitureObject = Resources.Load<GameObject>("Furniture/" + allData[i][1] + "/" + allData[i][1]);
                    Sprite sprite = Resources.Load<Sprite>("Furniture/" + allData[i][1] + "/" + allData[i][1]);
                    DatabaseEntry furnitureEntry = new DatabaseEntry(allData[i][1], "Furniture", sprite);
                    furnitureEntry.AddObject(furnitureObject);
                    database.Add(furnitureEntry);
                }
                break;
            }
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

