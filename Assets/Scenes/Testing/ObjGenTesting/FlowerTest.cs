using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class FlowerTest : MonoBehaviour
{
    // Start is called before the first frame update
    EntityDatabase database;

    [System.Serializable]public class FlowerTestObject
    {
        public string name; //What name to search for meshes
        public ObjectData data;
        public Material material;
        public Material billBoard;
        public CreationData creationData;

        public FlowerTestObject(string name_in, ObjectData data_in, CreationData creation_in, Material mat_in, Material billBoard_in)
        {
            name = name_in; data = data_in; material = mat_in;
            creationData = creation_in;
            billBoard = billBoard_in;
        }
    }
    [System.Serializable]public class CreationData
    {

    }
    [System.Serializable]public class FlowerCreationData:CreationData
    {
        public float height = 0; 
        public float bulbHeight = 0; 
        public int whorls = 0; 
        public int merosity = 0; 
        public AnimationCurve curve = null; 
        public List<Color> colors = new List<Color>(); 
        public float openness = 0;
        public AnimationCurve petalShape = null;

        public bool spread = false;
    }

    public List<FlowerTestObject> objects = new List<FlowerTestObject>();
    int focusedPlant = 0;

    public Slider openness;
    public Slider xOffset;
    public Slider yOffset;

    public Mesh quad;
    public float billBoardDistance;
    void Start()
    {
        database = Resources.Load<EntityDatabase>("EntityDatabase");
        //Load the info how to make the grass and tulips from a file
        string path = "Assets/Resources/EntityDatabase.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path); 
        database.Initialise(reader.ReadToEnd());
        reader.Close();
        Debug.Log("Database loaded");

        for(int i = 0; i < database.database.Count; i++)
        {
            objects.Add(new FlowerTestObject(database.database[i].name, new ObjectData(new Vector3(i, 0, 0), new Vector3(100, 100, 100), Quaternion.identity, new Vector3(0,0,0)), GetCreationData(database.database[i].name), database.database[i].material, database.database[i].billBoard));
        }
        UpdateUI();
        quad = MeshMaker.GetBillBoard();
       /* foreach(FlowerTestObject obj in objects)
        {
            GameObject temp = new GameObject(obj.name);
            MeshFilter filter = temp.AddComponent<MeshFilter>();
            MeshRenderer render = temp.AddComponent<MeshRenderer>();
            filter.mesh = quad;
            render.material = obj.billBoard;
            temp.transform.position = obj.data.pos + new Vector3(0, 2, -1);
            temp.transform.localScale = new Vector3(2,2,2);
            //temp.transform.rotation = Quaternion.Euler(90, 0, 0);
        }*/
    }

    FlowerCreationData GetCreationData(string name)
    {
        Debug.Log(name);
        string path = "Assets/Resources/EntityDatabase.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path); 
        string[] words = reader.ReadToEnd().Split(' ', '\n');
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
        reader.Close();
        FlowerCreationData flowerCreationData = new FlowerCreationData();
        for(int i = 0; i < allData.Count; i++)
        {
            for(int j = 0; j < allData[i].Count; j++)
            {
                if(allData[i][1] == name)
                {
                    switch(allData[i][j])
                    {
                        case "Height:": float.TryParse(allData[i][j+1], out flowerCreationData.height); break;
                        case "Bulb:": float.TryParse(allData[i][j+1], out flowerCreationData.bulbHeight); break;
                        case "Whorls:": int.TryParse(allData[i][j+1], out flowerCreationData.whorls); break;
                        case "Merosity:": int.TryParse(allData[i][j+1], out flowerCreationData.merosity); break;
                        case "Openness:": float.TryParse(allData[i][j+1], out flowerCreationData.openness); break;
                        case "Spread:": bool.TryParse(allData[i][j+1], out flowerCreationData.spread); break;
                        case "Curve:": 
                            {
                                flowerCreationData.curve = new AnimationCurve(); Keyframe keyFrame = new Keyframe();
                                int k = 1;
                                while(!allData[i][j+k].Any(x => char.IsLetter(x)))
                                {
                                    float time, curveValue;
                                    float.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""), out time);
                                    float.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""), out curveValue);
                                    keyFrame.time = time; keyFrame.value = curveValue;
                                    flowerCreationData.curve.AddKey(keyFrame);
                                    k+= 2;
                                }
                            }
                        break;
                        case "PetalShape:":
                            {
                                flowerCreationData.petalShape = new AnimationCurve(); Keyframe keyFrame = new Keyframe();
                                int k = 1;
                                while(!allData[i][j+k].Any(x => char.IsLetter(x)))
                                {
                                    float time, curveValue;
                                    float.TryParse(allData[i][j+k].Replace("(", "").Replace(")", "").Replace(",", ""), out time);
                                    float.TryParse(allData[i][j+k+1].Replace("(", "").Replace(")", "").Replace(",", ""), out curveValue);
                                    keyFrame.time = time; keyFrame.value = curveValue;
                                    flowerCreationData.petalShape.AddKey(keyFrame);
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
                                    flowerCreationData.colors.Add(new Color32((byte)r,(byte)g,(byte)b,255));
                                    k+= 3;
                                }
                            }
                        break; 
                    }
                }
            }
        }
        return flowerCreationData;
    }
    private void Update() 
    {
        bool updated = false;
        if(Input.GetKeyDown(KeyCode.D) && Camera.main.transform.position.x < objects.Count - 1)
        {
            Camera.main.transform.position += new Vector3(1,0,0);
            updated = true;
        }
        if(Input.GetKeyDown(KeyCode.A) && Camera.main.transform.position.x > 0)
        {
            Camera.main.transform.position -= new Vector3(1,0,0);
            updated = true;
        }
        if(updated)
        {
            UpdateUI();
            updated = false;
        }
    }
    void UpdateUI()
    {
        if(database.database[(int)Camera.main.transform.position.x].type == "Flower")
        {
            openness.gameObject.SetActive(true);
            xOffset.gameObject.SetActive(true);
            yOffset.gameObject.SetActive(true);
            openness.value = (objects[(int)Camera.main.transform.position.x].creationData as FlowerCreationData).openness;
        }
        else
        {
            openness.gameObject.SetActive(false);
            xOffset.gameObject.SetActive(false);
            yOffset.gameObject.SetActive(false);
        }
    }
    public void UpdateFlower()
    {
        bool temp = false;
        FlowerCreationData data = objects[(int)Camera.main.transform.position.x].creationData as FlowerCreationData;
        //MeshMaker.CreateFlower(database.GetMesh(objects[(int)Camera.main.transform.position.x].name, objects[(int)Camera.main.transform.position.x].variety, 0, ref temp), objects[(int)Camera.main.transform.position.x].material, 
        //data.height, data.bulbHeight, data.whorls, data.merosity, openness.value, Vector2.zero, data.curve, data.petalShape, data.colors, data.spread);
    }

    private void LateUpdate() 
    {
        foreach(FlowerTestObject obj in objects)
        {
            bool temp = false;
            //Mesh mesh = database.GetMesh(obj.name, 0, ref temp);
            //Graphics.DrawMesh(mesh, obj.data.pos, Quaternion.identity, obj.material, 0, Camera.main);
            //Graphics.DrawMesh(quad, obj.data.pos + new Vector3(0, billBoardDistance, 0), Quaternion.identity, obj.billBoard, 0, Camera.main);
        }
    }
}
