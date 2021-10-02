using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

            public MeshLOD(Mesh mesh_in, float renderDistance_in)
            {
                mesh = mesh_in;
                renderDistance = renderDistance_in;
            }
        }
        public string name;

        public int amountPerTile;
        public Material material;
        public List<MeshLOD> mesh;

        public DatabaseEntry(string name_in)
        {
            name = name_in;
            mesh = new List<MeshLOD>();
        }
        public void AddMesh(MeshLOD mesh_in)
        {
            mesh.Add(mesh_in);
        }
    }
    public List<DatabaseEntry> database;

    public void Initialise(string value)
    {
        database = new List<DatabaseEntry>();
        Material defaultMaterial = Resources.Load<Material>("Materials/Grass");
        string[] words = value.Split(' ', '\n');
        List<string> currentData = new List<string>();
        List<List<string>> allData = new List<List<string>>();
        for(int i = 0; i < words.Length; i++)
        {
            //Split "words" into arrays where the values are grouped together
            if(words[i].Length == 1) //If this word is just empty (I couldnt figure out what was the actual value of the string)
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
        for(int i = 0; i < allData.Count; i++)
        {
            //The txt files should always start with name and then type
            switch(allData[i][3]) //That means that number 3 is the type
            {
                case "Flower":
                {
                    DatabaseEntry flowerEntry = new DatabaseEntry(allData[i][1]);
                    Mesh flowerMesh = new Mesh();
                    Material flowerMaterial = new Material(defaultMaterial);
                    flowerMaterial.SetFloat("_Gravity", 0);
                    flowerMaterial.SetColor("_Color", Color.white);
                    float height = 0; float bulbHeight = 0; int whorls = 0; int merosity = 0; AnimationCurve curve = null; List<Color> colors = new List<Color>(); float renderDistance = 0; int amount = 0;
                    for(int j = 3; j < allData[i].Count; j++)
                    {
                        switch(allData[i][j])
                        {
                            case "Amount:": int.TryParse(allData[i][j+1], out amount); break;
                            case "Height:": float.TryParse(allData[i][j+1], out height); break;
                            case "Bulb:": float.TryParse(allData[i][j+1], out bulbHeight); break;
                            case "Whorls:": int.TryParse(allData[i][j+1], out whorls); break;
                            case "Merosity:": int.TryParse(allData[i][j+1], out merosity); break;
                            case "RenderDistance:": float.TryParse(allData[i][j+i], out renderDistance); renderDistance = renderDistance == -1 ? Mathf.Infinity : renderDistance; break;
                            case "Curve:": 
                                {
                                    curve = new AnimationCurve(); Keyframe keyFrame = new Keyframe();
                                    int k = 1;
                                    while(!allData[i][j+k].Any(x => char.IsLetter(x)))
                                    {
                                        float time, curveValue;
                                        float.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""), out time);
                                        float.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""), out curveValue);
                                        keyFrame.time = time; keyFrame.value = curveValue;
                                        curve.AddKey(keyFrame);
                                        k+= 2;
                                    }
                                }
                            break;
                            case "Colors:": 
                                {
                                    int k = 1;
                                    while(j+k < allData[i].Count && !allData[i][j+k].Any(x => char.IsLetter(x)))
                                    {
                                        int r, g, b;
                                        int.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""), out r);
                                        int.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""), out g);
                                        int.TryParse(allData[i][j+k+2].Replace("(", "").Replace(")", "").Replace(",", ""), out b);
                                        colors.Add(new Color32((byte)r,(byte)g,(byte)b,255));
                                        k+= 3;
                                    }
                                }
                            break; 
                        }
                    }
                    Texture2D flowerTexture = MeshMaker.CreateFlower(flowerMesh, flowerMaterial, height, bulbHeight, whorls, merosity, curve, colors);
                    flowerMaterial.SetTexture("_MainTex", flowerTexture);
                    DatabaseEntry.MeshLOD tempFlower = new DatabaseEntry.MeshLOD(flowerMesh, renderDistance);
                    flowerEntry.AddMesh(tempFlower);
                    flowerEntry.material = flowerMaterial;
                    flowerEntry.amountPerTile = amount;
                    database.Add(flowerEntry);
                }
                break;
                case "Tuft":
                {
                    DatabaseEntry tuftEntry = new DatabaseEntry(allData[i][1]);
                    Mesh tuftMesh = new Mesh();
                    int quads = 0; int straws = 0; float width = 0; float renderDistance = 0; int amount = 0;
                    for(int j = 3; j < allData[i].Count; j++)
                    {
                        switch(allData[i][j])
                        {
                            case "Amount:": int.TryParse(allData[i][j+1], out amount); break;
                            case "RenderDistance:": //Make sure the renderdistance part is at the end of each LOD mesh
                                float.TryParse(allData[i][j+1], out renderDistance); 
                                renderDistance = renderDistance == -1 ? Mathf.Infinity : renderDistance;
                                MeshMaker.CreateTuft(tuftMesh, quads, straws, width);
                                DatabaseEntry.MeshLOD tempTuft = new DatabaseEntry.MeshLOD(tuftMesh, renderDistance);
                                tuftEntry.AddMesh(tempTuft);
                            break;
                            case "Quads:": int.TryParse(allData[i][j+1], out quads); break;
                            case "Straws:":int.TryParse(allData[i][j+1], out straws); break;
                            case "Width:": float.TryParse(allData[i][j+1], out width); break;
                        }
                    }
                    tuftEntry.material = defaultMaterial;
                    tuftEntry.amountPerTile = amount;
                    database.Add(tuftEntry);
                }
                break;
            }
        }
    }

    public Mesh GetMesh(string value, float distance)
    {
        DatabaseEntry entry = GetDatabaseEntry(value);
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

    public DatabaseEntry GetDatabaseEntry(string value)
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
