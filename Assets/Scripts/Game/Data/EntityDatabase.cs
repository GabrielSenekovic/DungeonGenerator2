using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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
        public string name = "";
        public string variety = "";
        public string type = "";

        public int amountPerTile;
        public Material material;

        public Material billBoard;
        public Texture billBoard_Tex;
        public List<MeshLOD> mesh;

        public DatabaseEntry(string name_in, string type_in)
        {
            name = name_in;
            type = type_in;
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
                    List<DatabaseEntry> flowerEntries = new List<DatabaseEntry>();
                    List<List<Color>> colorsPerVariety = new List<List<Color>>();
                    float height = 0; float bulbHeight = 0; int whorls = 0; int merosity = 0; AnimationCurve curve = null; float renderDistance = 0;
                    float openness = 0; AnimationCurve flowerShape = null; bool spread = false; int amount = 0;
                    int variety = 0;
                    for(int j = 3; j < allData[i].Count; j++)
                    {
                        switch(allData[i][j])
                        {
                            case "Amount:":
                                    int.TryParse(allData[i][j+1], out amount); break;
                            case "Variety:":
                                    flowerEntries.Add(new DatabaseEntry(allData[i][1], "Flower"));
                                    flowerEntries[variety].variety = allData[i][j + 1]; variety++;
                                    break;
                            case "Height:": float.TryParse(allData[i][j+1],NumberStyles.Any, CultureInfo.InvariantCulture, out height); break;
                            case "Bulb:": float.TryParse(allData[i][j+1],NumberStyles.Any, CultureInfo.InvariantCulture, out bulbHeight); break;
                            case "Whorls:": int.TryParse(allData[i][j+1], out whorls); break;
                            case "Merosity:": int.TryParse(allData[i][j+1], out merosity); break;
                            case "RenderDistance:": float.TryParse(allData[i][j+1],NumberStyles.Any, CultureInfo.InvariantCulture, out renderDistance); renderDistance = renderDistance == -1 ? Mathf.Infinity : renderDistance; break;
                            case "Openness:": float.TryParse(allData[i][j+1],NumberStyles.Any, CultureInfo.InvariantCulture, out openness); break;
                            case "Spread:": bool.TryParse(allData[i][j+1], out spread); break;
                            case "Curve:": 
                                {
                                    curve = new AnimationCurve(); Keyframe keyFrame = new Keyframe();
                                    int k = 1;
                                    while(!allData[i][j+k].Any(x => char.IsLetter(x)))
                                    {
                                        float time, curveValue;
                                        float.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""),NumberStyles.Any, CultureInfo.InvariantCulture, out time);
                                        float.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""),NumberStyles.Any, CultureInfo.InvariantCulture, out curveValue);
                                        keyFrame.time = time; keyFrame.value = curveValue;
                                        curve.AddKey(keyFrame);
                                        k+= 2;
                                    }
                                }
                            break;
                            case "PetalShape:":
                                {
                                    flowerShape = new AnimationCurve(); Keyframe keyFrame = new Keyframe();
                                    int k = 1;
                                    while(!allData[i][j+k].Any(x => char.IsLetter(x)))
                                    {
                                        float time, curveValue;
                                        float.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""),NumberStyles.Any, CultureInfo.InvariantCulture, out time);
                                        float.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""),NumberStyles.Any, CultureInfo.InvariantCulture,out curveValue);
                                        keyFrame.time = time; keyFrame.value = curveValue;
                                        flowerShape.AddKey(keyFrame);
                                        k+= 2;
                                    }
                                }
                            break;
                            case "Colors:": 
                                {
                                    colorsPerVariety.Add(new List<Color>());
                                    int k = 1;
                                    while(j+k < allData[i].Count && !allData[i][j+k].Any(x => char.IsLetter(x)))
                                    {
                                        int r, g, b;
                                        int.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""), out r);
                                        int.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""), out g);
                                        int.TryParse(allData[i][j+k+2].Replace("(", "").Replace(")", "").Replace(",", ""), out b);
                                        colorsPerVariety[variety-1].Add(new Color32((byte)r,(byte)g,(byte)b,255));
                                        k+= 3;
                                    }
                                }
                            break; 
                        }
                    }
                    //Debug.Log("Creating a flower with: " + "height: " + height + " bulb: " + bulbHeight + " whorls: " + whorls + " merosity: " + merosity + " openness: " + openness);
                    if(curve == null)
                    {
                    }
                    for(int j = 0; j < variety; j++)
                    {
                        Material flowerMaterial = new Material(defaultMaterial);
                        flowerMaterial.SetFloat("_Gravity", 0);
                        flowerMaterial.SetColor("_Color", Color.white);

                        Mesh flowerMesh = new Mesh();

                        Texture2D flowerTexture = MeshMaker.CreateFlower(flowerMesh, flowerMaterial, height, bulbHeight, whorls, merosity, openness, Vector2.zero, curve, flowerShape, colorsPerVariety[j], spread);
                        
                        flowerMaterial.SetTexture("_MainTex", flowerTexture);
                        flowerEntries[j].material = flowerMaterial;

                        DatabaseEntry.MeshLOD tempFlower = new DatabaseEntry.MeshLOD(flowerMesh, renderDistance);
                        flowerEntries[j].amountPerTile = amount;
                        flowerEntries[j].AddMesh(tempFlower);
                        flowerEntries[j].AddMesh(new DatabaseEntry.MeshLOD(MeshMaker.GetBillBoard(), Mathf.Infinity));
                        flowerEntries[j].billBoard = GetBillBoard(flowerEntries[j].mesh[0].mesh, flowerEntries[j].material, ref flowerEntries[j].billBoard_Tex);
                        database.Add(flowerEntries[j]);
                    }
                    
                }
                break;
                case "Tuft":
                {
                    DatabaseEntry tuftEntry = new DatabaseEntry(allData[i][1], "Tuft");
                    Mesh tuftMesh = new Mesh();
                    int quads = 0; int straws = 0; float width = 0; float renderDistance = 0; int amount = 0;
                    for(int j = 3; j < allData[i].Count; j++)
                    {
                        switch(allData[i][j])
                        {
                            case "Amount:": int.TryParse(allData[i][j+1], out amount); break;
                            case "RenderDistance:": //Make sure the renderdistance part is at the end of each LOD mesh
                                float.TryParse(allData[i][j+1], NumberStyles.Any, CultureInfo.InvariantCulture, out renderDistance); 
                                renderDistance = renderDistance == -1 ? Mathf.Infinity : renderDistance;
                                MeshMaker.CreateTuft(tuftMesh, quads, straws, (float)width);
                                DatabaseEntry.MeshLOD tempTuft = new DatabaseEntry.MeshLOD(tuftMesh, (float)renderDistance);
                                tuftEntry.AddMesh(tempTuft);
                                tuftMesh = new Mesh();
                            break;
                            case "Quads:": int.TryParse(allData[i][j+1], out quads); break;
                            case "Straws:":int.TryParse(allData[i][j+1], out straws); break;
                            case "Width:": float.TryParse(allData[i][j+1], NumberStyles.Any, CultureInfo.InvariantCulture, out width); 
                            break;
                        }
                    }
                    tuftEntry.material = defaultMaterial;
                    tuftEntry.AddMesh(new DatabaseEntry.MeshLOD(MeshMaker.GetBillBoard(), Mathf.Infinity));
                    tuftEntry.billBoard = GetBillBoard(tuftEntry.mesh[0].mesh, tuftEntry.material, ref tuftEntry.billBoard_Tex);
                    tuftEntry.amountPerTile = amount;
                    database.Add(tuftEntry);
                }
                break;
            }
        }
    }
    public Material GetBillBoard(Mesh mesh, Material baseMaterial, ref Texture tex)
    {
        int dim = 128*2;
        var renderTexture = RenderTexture.GetTemporary(dim, dim, 24);
        var billboardTexture = new Texture2D(dim, dim, TextureFormat.RGBA32, false);
        Graphics.SetRenderTarget(renderTexture);
        baseMaterial.SetPass(0);
        Graphics.DrawMeshNow(mesh, new Vector3(0,-1,0), Quaternion.Euler(90,0,0));
        billboardTexture.ReadPixels(new Rect(0,0,dim,dim), 0,0);
        billboardTexture.Apply();
        tex = billboardTexture;
        Material mat = new Material(Resources.Load<Material>("Materials/BillBoard"));
        mat.SetTexture("_BaseMap", billboardTexture);
        return mat;
    }

    public Mesh GetMesh(string value, string variety, float distance, out bool billBoard)
    {
        billBoard = false;
        DatabaseEntry entry = GetDatabaseEntry(value, variety); //Get the mesh collection of this name
        if(entry != null) //If it was found
        {
            for(int i = 0; i < entry.mesh.Count; i++) //Go through all of them
            {
                if(distance < entry.mesh[i].renderDistance) //If the given distance is smaller than the distance of the mesh
                {
                    if(i == entry.mesh.Count - 1)
                    {
                        billBoard = true;
                    }
                    //Debug.Log("Distance: " + distance + " was smaller than: " + entry.mesh[i].renderDistance);
                    //Debug.Break();
                    return entry.mesh[i].mesh; //Return it
                }
            }
        }
        return null;
    }

    public DatabaseEntry GetDatabaseEntry(string value, string variety)
    {
        for(int i = 0; i < database.Count; i++)
        {
            if(database[i].name == value && database[i].variety == variety)
            {
                return database[i];
            }
        }
        return null;
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
}
