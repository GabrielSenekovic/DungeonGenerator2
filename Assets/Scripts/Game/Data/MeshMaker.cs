using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MeshMaker : MonoBehaviour
{
    public struct WallData
    {
        public int length; //How many walls will I make in this direction
        public int tilt; //How inclined the wall is into the tile
        public int height; //How tall are the walls
        public int rotation; //Determines what direction the walls are drawn in. Sides, up, down, diagonal etc

        public float roundedness; //At full roundedness, the corner is perfectly circular besides jaggedness. At zero roundedness, the corner is sharp

        public Vector2Int divisions;

        public Vector3 position; //The start position to draw the wall from

        public Vector2Int actualPosition;
        public Vector3 movePosition; //The position the wall must be moved in order to be right, because of my stupid way im making the wall, kill me

        public AnimationCurve curve;

        public WallData(Vector3 position_in, Vector2Int actualPosition_in, Vector2 movePosition_in, int rotation_in, int length_in, int height_in, int tilt_in, Vector2Int divisions_in, AnimationCurve curve_in, float roundedness_in)
        {
            position = position_in;
            rotation = rotation_in;
            length = length_in;
            height = height_in;
            tilt = tilt_in;
            divisions = divisions_in;
            curve = curve_in;
            roundedness = roundedness_in;
            actualPosition = actualPosition_in;
            movePosition = movePosition_in;
        }
        public WallData(Vector3 position_in, Vector2Int actualPosition_in, Vector2 movePosition_in, int rotation_in, int height_in, int tilt_in, AnimationCurve curve_in, float roundedness_in) //If this wall has the same length as the previous one, you don't have to define length
        {
            position = position_in;
            rotation = rotation_in;
            length = 0;
            height = height_in;
            tilt = tilt_in;
            divisions = new Vector2Int(1,1);
            curve = curve_in;
            roundedness = roundedness_in;
            actualPosition = actualPosition_in;
            movePosition = movePosition_in;
        }
    }
    public struct SurfaceData
    {
        public Vector3Int position;
        public List<Vector3> ceilingVertices;
        public List<Vector3> floorVertices;
        public int divisions; // needed for when determining how many times a position is removed in create surface when connecting vertices isnt 0
        public List<Room.RoomTemplate.TileTemplate.TileSides> sidesWhereThereIsWall;

        public SurfaceData(Vector3Int position_in, List<Vector3> ceilingVertices_in, List<Vector3> floorVertices_in, int divisions_in, List<Room.RoomTemplate.TileTemplate.TileSides> sidesWhereThereIsWall_in)
        {
            position = position_in;
            ceilingVertices = ceilingVertices_in;
            floorVertices = floorVertices_in;
            divisions = divisions_in;
            sidesWhereThereIsWall = sidesWhereThereIsWall_in;
        }
    }  
    public static void CreateBush(Mesh mesh, int height)
    {
        //This requires the CreateStone() function
    }
    public static void CreateChest(Mesh mesh, int length)
    {
        //Specify how long the chest is. Length determines how long it is from left to right, if the opening side is in front
        //Start off by creating just a cube chest
        //Like in Minecraft
        //Start from the back then forward. Save the points on the top where the lid will rotate around
    }
    public static void CreateCylinder(ref List<Vector3> positions, ref float currentHeight, int limit, int stepsAroundCenter, float heightIncrement, AnimationCurve curve)
    {
        float angle = 0; float angleIncrement = 360 / stepsAroundCenter;

        for(int i = 0; i < limit; i++)
        {
            for(int j = 0; j < stepsAroundCenter; j++)
            {
                float radius = curve != null? curve.Evaluate((float)i/(float)limit): (1f/32f);
                positions.Add(new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle* Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight += heightIncrement;
        }
    }

    public static Texture2D CreateFlower(Mesh mesh, Material flowerMaterial, float height, float bulbHeight, int whorls, int merosity, float openness, Vector2 offset, AnimationCurve curve, AnimationCurve flowerShape, List<Color> colors, bool spread)
    {
        //Create a stalk, create leaves and add a perianth to the top of the stalk
        //The stalk is triangular so it can be seen from all angles
        //Tulip grow about 30cm high
        Debug.Log("Creating flower");
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        height = Math.ConvertCentimetersToPixels((int)height);
        bulbHeight = Math.ConvertCentimetersToPixels((int)bulbHeight);

        //!The height will also determine the height of the texture
        Debug.Log("Making texture");
        Texture2D texture = new Texture2D(2, (int)(height * 16), TextureFormat.ARGB32, false); //1 in height corresponds to 16 pixels
        for(int i = 0; i < texture.width * texture.height; i++)
        {
            if(i/texture.width < (int)(height * 16 - bulbHeight * 16))
            {
                texture.SetPixel(i%texture.width, i/texture.width, colors[0]);
            }
            else
            {
                texture.SetPixel(i%texture.width, i/texture.width, colors[1]);
            }
        }
        Debug.Log("Applying texture");
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        flowerMaterial.SetTexture("_MainTex", texture);

        List<Vector3> positions = new List<Vector3>();
        float currentHeight = 0;
        float amountOfVerticesVertical = 5;
        float heightIncrement = (height - bulbHeight) / amountOfVerticesVertical;

        int stepsAroundCenter = 3;

        Debug.Log("Creating stalk");

        CreateCylinder(ref positions, ref currentHeight, (int)amountOfVerticesVertical, stepsAroundCenter, heightIncrement, null);

        for(int j = 0; j < positions.Count -3; j++)
        {
            newVertices.Add(positions[0 + j]);
            newVertices.Add(positions[(1 + j)%stepsAroundCenter + stepsAroundCenter * (int)((float)j/(float)stepsAroundCenter)]);
            newVertices.Add(positions[(stepsAroundCenter + 1 + j)%stepsAroundCenter + stepsAroundCenter * (int)((float)(j+3)/(float)stepsAroundCenter)]);
            newVertices.Add(positions[stepsAroundCenter + j]);

            int[] indexValue = new int[]{0,1,3,1,2,3};

            for(int index = 0; index < indexValue.Length; index++)
            {
                newTriangles.Add(indexValue[index] + j * 4);
            }
        }
        float amountOfQuadsVertical = amountOfVerticesVertical -1;
        for(int j = 0; j < amountOfQuadsVertical; j++)
        {
            for(int k = 0; k < stepsAroundCenter; k++) //Minus one because there are 4 quads if there are 5 vertices upwards
            {
                newUV.Add(new Vector2 (1, ((height - bulbHeight)/height) / amountOfQuadsVertical * j));     //1,0         1.0f / amountOfQuadsVertical * j
                newUV.Add(new Vector2 (0, ((height - bulbHeight)/height) / amountOfQuadsVertical * j));     //0,0
                newUV.Add(new Vector2 (0, ((height - bulbHeight)/height) / amountOfQuadsVertical * (j+1))); //0,1
                newUV.Add(new Vector2 (1, ((height - bulbHeight)/height) / amountOfQuadsVertical * (j+1))); //1,1
            }
        }
        //Stalk done, lets add Perianth
        Debug.Log("Creating Perianth");
        CreatePerianth(ref newVertices, ref newTriangles, ref newUV, currentHeight - heightIncrement, bulbHeight, whorls, merosity, (height - bulbHeight)/height, openness, offset, curve, flowerShape, spread); 
        //!height - bulbheight only works because height is 1, so its already procentual

        DebugLog.Report(newVertices, newTriangles, newUV);

        mesh.Clear ();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateNormals();

        return texture;
    }

    public static void CreatePerianth(ref List<Vector3> vertices, ref List<int> indices, ref List<Vector2> UV, float startPoint, float height, int whorls, int merosity, float UVStart, float openness, Vector2 offset, AnimationCurve curve, AnimationCurve flowerShape, bool spread)
    {
        List<Vector3> positions = new List<Vector3>();
        float angle = 0;
        float angleIncrement = 360f / (merosity * 3f); //Time 3 since each petal stretches one step outwards on each side
        float currentHeight = startPoint;
        float heightIncrement = height / 2f;

        float amountOfVerticesVertical = 4;
        float amountOfQuadsVertical = amountOfVerticesVertical - 1;

        for(int i = 0; i < amountOfVerticesVertical; i++) 
        {
            float radius = curve.Evaluate((float)i/(float)amountOfVerticesVertical);
            float petalValueSaved = 0;
            if(flowerShape != null && flowerShape.keys.Length > 0)
            {
                petalValueSaved = flowerShape.Evaluate((float)i/(float)amountOfVerticesVertical);
            }
            Vector2 normalizedDir = Vector2.zero;
            for(int j = 0; j < merosity * 3; j++) //Three steps around, times three because each petal stretches one step outwards on each side
            {

                float petalValue = j % 3 == 1 ? 0 : j % 3 == 0 ? -petalValueSaved : petalValueSaved;

                Vector3 position = new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad + petalValue) + offset.x, radius * Mathf.Sin(angle* Mathf.Deg2Rad + petalValue) + offset.y, -currentHeight);

                if(j % 3 == 0 && !spread)
                {
                    normalizedDir = new Vector3(radius * Mathf.Cos((angle + angleIncrement) * Mathf.Deg2Rad) + offset.x, radius * Mathf.Sin((angle + angleIncrement) * Mathf.Deg2Rad) + offset.y, -currentHeight);
                    normalizedDir.Normalize();
                }
                else if(spread)
                {
                    normalizedDir = position.normalized;
                }
                
                if(i > 0)
                {
                    GameObject temp = new GameObject();
                    Vector3 origin = new Vector3(positions[j].x, positions[j].y, -startPoint);

                    temp.transform.position = new Vector3(position.x, position.y, position.z); //Set to the vertex position
                    temp.transform.RotateAround(origin, new Vector2(normalizedDir.y, -normalizedDir.x), openness * 90); //Rotate vertex around
           
                    position = temp.transform.position;
                    Destroy(temp);
                }
                positions.Add(position);
                angle += angleIncrement;
                angle %= 360;
            }
            currentHeight += heightIncrement;
        }

        int startIndex = vertices.Count;

        int m = 0;
        for(int j = 0; j < positions.Count - merosity * 3; j++)
        {
            //I have to skip one quad
            if(j % 3 == 2){continue;}
            vertices.Add(positions[0 + j]);
            vertices.Add(positions[(1 + j)%(merosity * 3) + (merosity * 3) * (int)((float)j/(merosity * 3))]);
            vertices.Add(positions[((merosity * 3 + 1) + j)%(merosity * 3) + (merosity * 3) * (int)((float)(j+(merosity * 3))/(merosity * 3))]);
            vertices.Add(positions[merosity * 3 + j]);

            int[] indexValue = new int[]{0,3,1,1,3,2};

            for(int index = 0; index < indexValue.Length; index++)
            {
                indices.Add(startIndex + indexValue[index] + m * 4 );
            }
            m++;
        }
        for(int j = 0; j < amountOfQuadsVertical; j++)
        {
            for(int k = 0; k < merosity * 3; k++) 
            {
                if(k % 3 == 0) {continue;}
                UV.Add(new Vector2 (1, UVStart + (1f - UVStart) / amountOfQuadsVertical * j));     //1,0 + height / amountOfQuadsVertical * j
                UV.Add(new Vector2 (0, UVStart + (1f - UVStart) / amountOfQuadsVertical * j));     //0,0
                UV.Add(new Vector2 (0, UVStart + (1f - UVStart) / amountOfQuadsVertical * (j+1))); //0,1
                UV.Add(new Vector2 (1, UVStart + (1f - UVStart) / amountOfQuadsVertical * (j+1))); //1,1
            } 
        }
    }
    
    public static void CreateTuft(Mesh mesh, int quadsPerGrass, int amountOfStraws, float grassWidth)
    {
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        //Creates Grass
        //Later also Sedges, Rushes and Reeds (Different Graminoids)
        float angle = 0;
        float angle_increase = (360.0f / 1.618033f) * Mathf.Deg2Rad;
        float radius = 0.05f;
        float maxRadius = 0.3f;
        float radius_increase = (maxRadius - radius) / amountOfStraws;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, 0);
        curve.AddKey(0.3f, 0.1f);
        curve.AddKey(1, 0.9f);

        for(int i = 0; i < amountOfStraws; i++)
        {
            float secondAngle = 2 * Mathf.Asin((grassWidth/2) / (radius / Mathf.Sin(Mathf.Deg2Rad * 90))); //Law of Sines of half of the isosceles triangle to figure out the inner angle

            float height = 1.0f - curve.Evaluate(1.0f / amountOfStraws * i) - UnityEngine.Random.Range(-0.5f, 0.5f);

            for(int k = 0; k < quadsPerGrass; k++)
            {
                //Debug.Log(-(1.0f / height * k));
               //Go up the straw
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle),          -(height / quadsPerGrass * k)));
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle),   -(height / quadsPerGrass * k)));
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle),   -(height / quadsPerGrass * (k+1))));
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle),                       -(height / quadsPerGrass * (k+1))));

                newUV.Add(new Vector2 (1, 1.0f / quadsPerGrass * k));     //1,0
                newUV.Add(new Vector2 (0, 1.0f / quadsPerGrass * k));     //0,0
                newUV.Add(new Vector2 (0, 1.0f / quadsPerGrass * (k+1))); //0,1
                newUV.Add(new Vector2 (1, 1.0f / quadsPerGrass * (k+1))); //1,1

                int[] indexValue = new int[]{0,1,3,1,2,3};

                int temp = newVertices.Count - 4;

                for(int index = 0; index < indexValue.Length; index++)
                {
                    newTriangles.Add(indexValue[index] + temp);
                }
            }
           /* for(int k = (int)quadsPerGrass - 1; k >= 0; k--)
            {
                //Go down the straw
                
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle),    -(height / quadsPerGrass * k)));
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle),                        -(height / quadsPerGrass * k)));
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle),                       -(height / quadsPerGrass * (k+1))));
                newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle),   -(height / quadsPerGrass * (k+1)) ));

                newUV.Add(new Vector2 (1, 1.0f / quadsPerGrass * k));     //1,0
                newUV.Add(new Vector2 (0, 1.0f / quadsPerGrass * k));     //0,0
                newUV.Add(new Vector2 (0, 1.0f / quadsPerGrass * (k+1))); //0,1
                newUV.Add(new Vector2 (1, 1.0f / quadsPerGrass * (k+1))); //1,1
                
                int[] indexValue = new int[]{0,1,3,1,2,3};

                int temp = newVertices.Count - 4;

                for(int index = 0; index < indexValue.Length; index++)
                {
                    newTriangles.Add(indexValue[index] + temp);
                }
            }*/
            //Slowly increase radius
            radius += radius_increase;
            angle += angle_increase;
        }

        //Debug.Log(newVertices.Count);

        mesh.Clear ();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
    
    public static GameObject CreateVase(Material material)
    {
        GameObject vase = new GameObject("Vase");
        vase.AddComponent<MeshFilter>();
        AnimationCurve curve = CreateVase_GetCurve();
        
        MeshMaker.OnCreateVase(vase.GetComponent<MeshFilter>().mesh, UnityEngine.Random.Range(0.8f, 2.0f), curve);
        vase.AddComponent<MeshRenderer>();

        Material vaseMaterial = new Material(material.shader);
        vaseMaterial.CopyPropertiesFromMaterial(material);
        vaseMaterial.color = Math.GetRandomSaturatedColor(0.8f);

        vase.GetComponent<MeshRenderer>().material = vaseMaterial;

        SphereCollider col = vase.AddComponent<SphereCollider>();
        col.radius = 0.5f;

        HealthModel health = vase.AddComponent<HealthModel>();
        health.deathSound = "pot_break";
        health.maxHealth = 1; health.currentHealth = 1;

        vase.AddComponent<DropItems>();
        if(vase.GetComponent<DropItems>())
        {
            vase.GetComponent<DropItems>().Initialize(UIManager.GetCurrency());
        }
        vase.AddComponent<AnimationCurveTest>();
        vase.GetComponent<AnimationCurveTest>().curve = curve;

        return vase;
    }
    public static AnimationCurve CreateVase_GetCurve()
    {

        AnimationCurve curve = new AnimationCurve();
        Keyframe temp = new Keyframe();
        //! POINT NUMBER 1
        temp.time = 0;
        temp.value = UnityEngine.Random.Range(0.2f, 0.4f);
        curve.AddKey(temp);

        //! POINT NUMBER 2
        temp.time = UnityEngine.Random.Range(0.25f, 0.75f); //Upper end makes a meiping ish shape, lower end makes a fat pot shape
        temp.value = UnityEngine.Random.Range(0.3f, 0.5f); //The fatest part of the vase, the radius. It determines how much space the vase takes up
        
        if(UnityEngine.Random.Range(0,1) == 1) //Carinate
        {
            temp.outTangent = -1.5f;
        }
        curve.AddKey(temp);

        curve.SmoothTangents(0, 0);

        //! POINT NUMBER 3
        float upperRange = 1.0f - curve.keys[curve.keys.Length-1].time - 0.1f; //-0.1f because it's not allowed to be 1. It can at most be 0.9f
        temp.time = curve.keys[curve.keys.Length-1].time + UnityEngine.Random.Range(0.1f, upperRange);
        temp.value = 0.2f;
        curve.AddKey(temp);

        temp.time = 1;
        temp.value = UnityEngine.Random.Range(0.05f, 0.35f);
        curve.AddKey(temp);

        //! POINT NUMBER 4
        
        if(UnityEngine.Random.Range(0,1)==1) //Set straight neck
        {
            curve.SmoothTangents(2, 0);
        }
        curve.SmoothTangents(3, 0); 

        return curve;
    }
    public static void OnCreateVase(Mesh mesh, float height, AnimationCurve curve)
    {
        //Create a vase from a sinewave controlling a radius
        List<Vector3> positions = new List<Vector3>();
        int amountOfQuadsVertical = 15;
        float angleIncrement = 360.0f / 10.0f;
        float currentHeight = 0;
        float heightIncrement = height / (float)amountOfQuadsVertical;

        CreateCylinder(ref positions, ref currentHeight, amountOfQuadsVertical, 10, heightIncrement, curve);

        currentHeight-=heightIncrement;

        for(int j = amountOfQuadsVertical -1; j >= 0; j--)
        {
            //Go through all levels downwards
            float angle = 0;
            float currentRadius = curve.Evaluate((float)j/(float)amountOfQuadsVertical);

            for(int k = 9; k >= 0; k--)
            {
                //Go around the circle
                positions.Add(new Vector3(currentRadius * Mathf.Cos(angle * Mathf.Deg2Rad), currentRadius * Mathf.Sin(angle* Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight -= heightIncrement;
        }
       // Debug.Log(positions.Count);

        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        for(int j = 0; j < positions.Count -10; j++)
        {
            newVertices.Add(positions[0 + j]);
            newVertices.Add(positions[(1 + j)%10 + 10 * (int)((float)j/10.0f)]);
            newVertices.Add(positions[(11 + j)%10 + 10 * (int)((float)(j+10)/10.0f)]);
            newVertices.Add(positions[10 + j]);

            newUV.Add(new Vector2 (1, 0));                      //1,0
            newUV.Add(new Vector2 (0, 0));                      //0,0
            newUV.Add(new Vector2 (0, 1)); //0,1
            newUV.Add(new Vector2 (1, 1)); //1,1

            int[] indexValue = new int[]{0,1,3,1,2,3};

            for(int index = 0; index < indexValue.Length; index++)
            {
                //Debug.Log(indexValue[index] + j * 4);
                newTriangles.Add(indexValue[index] + j * 4);
            }
        }

        mesh.Clear ();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
    public static void CreateWall(GameObject wall, Material wallMaterial, List<WallData> instructions, bool wrap, Grid<Room.RoomTemplate.TileTemplate> tiles)
    {
        if(instructions.Count == 0)
        {
            DebugLog.WarningMessage("There were no instructions sent!"); return;
        }
        Debug.Log("Enters CreateWall");
        //ref List<Room.EntranceData> entrancesOfThisRoom, ref List<Room.EntranceData> entrancesOfRoomAtStart, ref List<Room.EntranceData> entrancesOfRoomAtEnd
        float jaggedness = 0.04f;
        Vector2 divisions = new Vector2(instructions[0].divisions.x +1, instructions[0].divisions.y+1);
        if(instructions[0].divisions.x == 1)
        {
            jaggedness = 0;
        }
        Vector2Int currentGridPosition = Vector2Int.zero;
       // if(divisions.x == 1){jaggedness = 0;}
        //dim x = width, y = tilt, z = height
        //divisions = how many vertices per unit tile
        //instructions tells us how many steps to go before turning, and what direction to turn
        float centering = 0.5f - ((1 / divisions.x) * (divisions.x - 1)); //the mesh starts at 0 and then goes to the left, which essentially makes it start in the middle of a ground tile and stretch outside it. This float will fix that

        List<Vector3> allVertices = new List<Vector3>();

        int jump_wall = 0; //it needs to accumulate all the walls vertices

        int savedLengthOfWall = instructions[0].length; //If one of the walls have length zero, use this value instead

        for(int i = 0; i < instructions.Count; i++)
        {
            //Go through each wall
            float x = instructions[i].position.x - centering;
            float y = instructions[i].position.y;
            float z = instructions[i].position.z;

            currentGridPosition = new Vector2Int((int)instructions[i].actualPosition.x, (int)-instructions[i].actualPosition.y);
            Vector2Int upperFloorGridPosition = Vector2Int.zero;
            Vector2Int doorGridPosition = Vector2Int.zero; //If this is a door, then you want to save the position the door is on, which is in front
            
            if(i > 0 && instructions[i-1].rotation == (instructions[i].rotation -90)%360) //! If youre turning after an outer corner, you must push up one step, otherwise it will put a position that has no wall
            {
                if(Math.Mod(instructions[i].rotation, 360) == 0)
                {
                    currentGridPosition += new Vector2Int(1,0);
                }
                else if(Math.Mod(instructions[i].rotation,360) == 90)
                {
                    currentGridPosition += new Vector2Int(0,-1);
                }
                else if(Math.Mod(instructions[i].rotation, 360) == 180)
                {
                    currentGridPosition += new Vector2Int(-1,0);
                }
                else if(Math.Mod(instructions[i].rotation, 360) == 270)
                {
                    currentGridPosition += new Vector2Int(0,1);
                }
            }
            if(Math.Mod(instructions[i].rotation, 360) == 0)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                doorGridPosition = currentGridPosition + new Vector2Int(0, 1);
            }
            else if(Math.Mod(instructions[i].rotation,360) == 90)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
                doorGridPosition = currentGridPosition + new Vector2Int(1, 0);
            }
            else if(Math.Mod(instructions[i].rotation, 360) == 180)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                doorGridPosition = currentGridPosition + new Vector2Int(0, -1);
            }
            else if(Math.Mod(instructions[i].rotation, 360) == 270)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(1, 0);
                doorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
            }

            int amount_of_faces = (int)(divisions.x * divisions.y);
            const int vertices_per_quad = 4;
            int vertices_per_tile = amount_of_faces * vertices_per_quad;
            int vertices_per_column = vertices_per_tile * instructions[i].height;

            int lengthOfWall = instructions[i].length > 0 ? instructions[i].length : savedLengthOfWall;
            int previousLengthOfWall = instructions[i].length > 0 && i > 0 ? instructions[i-1].length : savedLengthOfWall; //Get this so you can jump over the previous wall
            savedLengthOfWall = lengthOfWall;

            jump_wall = i > 0? jump_wall + (previousLengthOfWall * vertices_per_column) :0; //This value always grows. It doesnt reset in each loop

            float tilt_increment = instructions[i].tilt / (instructions[i].height * divisions.y);

            for(int j = 0; j < savedLengthOfWall; j++)
            {
                //Debug.Log("New Column! Going this many steps: " + savedLengthOfWall);
                string debug_info = j+ ": ";
                List<int> newIndices = new List<int>();
                List<Vector2> newUV = new List<Vector2>();
                //Go through each column of wall
                for(int k = 0; k < instructions[i].height; k++)
                {
                    //Go through each square upwards
                    for(int l = 0; l < divisions.x * divisions.y; l++)
                    {
                        //Go through each vertex of the square
                        float x_perc = ((l % divisions.x) / divisions.x);

                        float v_x = ((l % divisions.x) / divisions.x) + j;
                        float v_z = (l / (int)divisions.x) / divisions.y;
                        int skip_left = (int)(Mathf.RoundToInt(((v_x - j) * divisions.x)-1 )) * vertices_per_quad;
                        //I have to RoundToInt here because for some reason, on the second wall when doing D, it gave me 2 - 1 = 0.9999999
                        //Epsilons suck
                        int skip_up = (int)(Mathf.RoundToInt((v_z * divisions.y)-1)) * vertices_per_quad * (int)divisions.x;

                        int steps_up = k * (int)divisions.y + (int)(v_z * divisions.y); //counts the total row you are on
                        float current_tilt_increment = tilt_increment + tilt_increment * steps_up;

                        //TODO  CHANGE THE WAY THAT THE WALL QUADS ARE MADE SO THAT INSTEAD OF CREATING EACH QUAD FROM RIGHT TO LEFT WHEN THE WALL IS MADE FROM LEFT TO RIGHT, SO YOU DONT HAVE TO DO THE FOLLOWING BULLSHIT 
                        //TODO  JUST TO ACCOMODATE FOR THE ROTATION

                        float rot_a = 0, rot_b = 0, rot_c = 0;
                        
                        float firstQuad_leftVal_x = 1.0f / divisions.x; //1.0f only works for the 0 angle rotated wall, not for the other ones
                        float firstQuad_leftVal_z = 1.0f / divisions.y;

                        bool roundLastColumn = v_x > savedLengthOfWall - 1  //! If the last column of the wall
                                            && i < instructions.Count - 1 //! If not the last wall, since it must be leading into something
                                            && instructions[i].roundedness > 0;
                        bool roundFirstColumn = v_x < 1.0f
                                            && i > 0
                                            && instructions[i - 1].roundedness > 0;
                        bool saveStart = false; //These are used to decide when to save to these lists, but it has to be a bool because it cant be done before rotation
                        bool saveEnd = false;

                        if(Math.Mod(instructions[i].rotation, 360) == 0)
                        {
                            rot_b = -1.0f / divisions.x;
                        }
                        else if(Math.Mod(instructions[i].rotation,360) == 90)
                        {
                            float mult_1 = 0.5f * (divisions.x -2);
                            float mult_2 = 1.0f - mult_1;
                            //d and f are wrong atm
                            rot_a = 1.0f / divisions.x * mult_1;
                            rot_b = -1.0f / divisions.x * mult_2;
                            rot_c = 1.0f/divisions.x * mult_1;
                        }
                        else if(Math.Mod(instructions[i].rotation, 360) == 180)
                        {
                            float mult_1 = divisions.x - 2;
                            float mult_2 = divisions.x - 3; 

                            rot_a = 1.0f / divisions.x * mult_1;
                            rot_b = 1.0f / divisions.x * mult_2;
                        }
                        else if(Math.Mod(instructions[i].rotation, 360) == 270)
                        {
                            float mult_1 = 0.5f * (divisions.x -2);
                            float mult_2 = 1.0f - mult_1;

                            rot_a = 1.0f / divisions.x * mult_1;
                            rot_b = -1.0f / divisions.x * mult_2;
                            rot_c = -1.0f/divisions.x * mult_1;
                        }
                        //TODO /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        //The 4 is because there's 4 vertices per quad
                        int jump_quad_up = vertices_per_quad * l;
                        int jump_tile = amount_of_faces * k * vertices_per_quad;
                        int jump_quad_side = amount_of_faces * (int)instructions[i].height * j * vertices_per_quad;

                        //******************************************************************************************||
                        //*Shorthand for what things mean                                                           ||
                        //* v_x = Vertex x position in decimal                                                      ||
                        //* v_y = Vertex y position in decimal                                                      ||
                        //*                                                                                         ||
                        //* v_x * division.x = Vertex x position not in decimal                                     ||
                        //* j * divisions.x = Start vertex position of each column                                  ||
                        //* v_x * divisions.x == j * divisions.x = Is this vertex at the start of the column?       ||
                        //* v_x * divisions.x > j * divisions.x = Is this vertex not at the start of the column?    ||
                        //* j * divisions.x + division.x - 1 = The last vertex position of each column              ||
                        //*                                                                                         ||
                        //* k == 0 && l / divisions.x == 0  This quad is on the lowest row                          ||
                        //* (int)(k * divisions.y + l/divisions.x) Quad one the way up                              ||
                        //*                                                                                         ||
                        //******************************************************************************************||

                        float upperJaggedness = jaggedness;
                        if(l > divisions.x * divisions.y - divisions.x)
                        {
                            upperJaggedness = 0;
                        }
                        int[] indicesToRotate = {};
                        Vector3 rotateAround = Vector3.zero;
                    
                        if(wrap && i == instructions.Count - 1 && j == savedLengthOfWall - 1 && v_z * divisions.y + k > 0 && Mathf.Round(v_x * divisions.x) == Mathf.Round(j * divisions.x + divisions.x - 1)) //L
                        {
                            //If on the last wall
                            //Then connect to the first wall
                            debug_info += "L";
                            //Connect tiles upwards to tiles downwards
                            allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            if(divisions.x > 1)
                            {
                                allVertices.Add( allVertices[(3 + skip_up + skip_left )+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            }
                            else
                            {
                                allVertices.Add( allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + k * vertices_per_tile + (j-1) * vertices_per_column]);
                                allVertices.Add( allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x * 2 - 1)) + jump_wall + k * vertices_per_tile + (j-1) * vertices_per_column]);
                            }
                            allVertices.Add( allVertices[2 + skip_up + k * vertices_per_tile + vertices_per_quad * (int)divisions.x]);
                        }
                        else if(wrap && i == instructions.Count - 1 && j == savedLengthOfWall - 1 && Mathf.Round(v_x * divisions.x) == Mathf.Round(j * divisions.x + divisions.x - 1)) //K
                        {
                            //If on the last wall
                            //Then connect to the first wall
                            debug_info += "K";
                            allVertices.Add( allVertices[1]);
                            if(divisions.x > 1)
                            {
                                allVertices.Add( allVertices[(0 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(3 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            }
                            else //When divisions.x is 1, then G isnt there to connect to the column before. Instead K has to do it, and it isnt normally equipped to do so.
                            {
                                int jump_column = j > 0 ? (j-1) * vertices_per_column: 0;
                                allVertices.Add( allVertices[(0 + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + k * vertices_per_tile + jump_column]);
                                allVertices.Add( allVertices[(3 + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + k * vertices_per_tile + jump_column]);
                            }
                            allVertices.Add( allVertices[2]);
                        }
                        else if(v_x * divisions.x > j * divisions.x && k > 0) //F
                        {
                            debug_info += "F";
                            //Connect tiles upwards to tiles downwards
                            if(tiles[doorGridPosition].endVertices.Count < instructions[i].height * divisions.y * 4 || l % (divisions.x) < divisions.x - 1)
                            {
                                if(k == instructions[i].height - 1 && (int)(l / divisions.x) == instructions[i].height - 1)
                                {
                                    allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                    allVertices.Add( allVertices[(3 + skip_up + skip_left )+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                    allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                    allVertices.Add(  new Vector3 ((x + v_x)                     , y + current_tilt_increment , ((z - v_z) -firstQuad_leftVal_z) - k));
                                }
                                else
                                {
                                    allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                    allVertices.Add( allVertices[(3 + skip_up + skip_left )+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                    allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                    allVertices.Add(  new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                                }
                                saveEnd = true;
                                indicesToRotate = new int[] {3};
                            }
                            else
                            {
                                allVertices.Add( tiles[doorGridPosition].endVertices[1 + 4 * (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add( allVertices[(3 + skip_up + skip_left )+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( tiles[doorGridPosition].endVertices[2 + 4 * (int)(k * divisions.y + l/divisions.x)]);
                            }
                            rotateAround = instructions[i].position;
                            
                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if((v_z > 0 || k > 0) && v_x * divisions.x == j * divisions.x && j > 0) //H
                        {
                            debug_info += "H";
                            //Connect the first quad of each row of right columns to the last quad of each row of left columns
                            allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + k * vertices_per_tile + (j-1) * vertices_per_column]);
                            allVertices.Add( allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x * 2 - 1)) + jump_wall + k * vertices_per_tile + (j-1) * vertices_per_column]);
                            allVertices.Add(  new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {3};
                            rotateAround = instructions[i].position;

                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if((v_z > 0 || k > 0) && v_x * divisions.x == j * divisions.x && i > 0) //J
                        {
                            debug_info += "J";
                            int jump_column = j > 0 ? (j-1) * vertices_per_column: 0;
                            //Connect the first quad of each row of right columns to the last quad of each row of previous wall
                            allVertices.Add( allVertices[((3 + jump_wall + skip_up + vertices_per_quad)+ skip_left) + k * vertices_per_tile+ j * vertices_per_column + jump_column]);
                            allVertices.Add( allVertices[(3 + (jump_wall- vertices_per_column) + skip_up + vertices_per_quad * ((int)divisions.x - 1)) + k * vertices_per_tile + jump_column]);
                            allVertices.Add( allVertices[(3 + (jump_wall - vertices_per_column) + skip_up + vertices_per_quad * ((int)divisions.x * 2 - 1)) + k * vertices_per_tile + jump_column]);
                            allVertices.Add(  new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {3};
                            rotateAround = instructions[i].position;

                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if(k > 0) //E
                        {
                            debug_info += "E";
                            if(tiles[upperFloorGridPosition].startVertices.Count < instructions[i].height * divisions.y * 4)
                            {
                                if(k == instructions[i].height - 1 && (int)(l / divisions.x) == instructions[i].height - 1)
                                {
                                    allVertices.Add( allVertices[((3 + skip_up) + k * vertices_per_tile) + j * vertices_per_column]);
                                    allVertices.Add( allVertices[((2 + skip_up) + k * vertices_per_tile) + j * vertices_per_column]);
                                    allVertices.Add( new Vector3 ((x + v_x) + rot_b , y + current_tilt_increment + rot_c, ((z - v_z) -firstQuad_leftVal_z) - k));
                                    allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a   , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) + rot_c , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                                }
                                else
                                {
                                    allVertices.Add( allVertices[((3 + skip_up) + k * vertices_per_tile) + j * vertices_per_column]);
                                    allVertices.Add( allVertices[((2 + skip_up) + k * vertices_per_tile) + j * vertices_per_column]);
                                    allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness)  + rot_c, ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                                    allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a   , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) + rot_c , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));  
                                }
                                saveStart = true;
                            }
                            else
                            {
                                Debug.Log(((int)(k * divisions.y + l/divisions.x)) + " is " +  tiles[upperFloorGridPosition].startVertices[(int)(k * divisions.y + l/divisions.x)]);
                                if(k == instructions[i].height - 1 && (int)(l / divisions.x) == instructions[i].height - 1)
                                {
                                    allVertices.Add( tiles[upperFloorGridPosition].startVertices[0 + (int)(k * divisions.y + l/divisions.x)]);
                                    allVertices.Add( tiles[upperFloorGridPosition].startVertices[1 + (int)(k * divisions.y + l/divisions.x)]);
                                    allVertices.Add( new Vector3 ((x + v_x) + rot_b , y + current_tilt_increment + rot_c, ((z - v_z) -firstQuad_leftVal_z) - k));
                                    allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a   , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) + rot_c , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                                }
                                else
                                {
                                    allVertices.Add( tiles[upperFloorGridPosition].startVertices[0 + (int)(k * divisions.y + l/divisions.x)]);
                                    allVertices.Add( tiles[upperFloorGridPosition].startVertices[1 + (int)(k * divisions.y + l/divisions.x)]);
                                    allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness)  + rot_c, ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                                    allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a   , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) + rot_c , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));  
                                }
                            }
                            indicesToRotate = new int[] {2, 3};
                            rotateAround = new Vector3(x,y,z);

                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            { 
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if(v_z * divisions.y > 0 && v_x * divisions.x > j * divisions.x) //D
                        {
                            debug_info += "D";
                            //Connect quad diagonally up to the left to surrounding quads
                            if(tiles[doorGridPosition].endVertices.Count < instructions[i].height * divisions.y * 4 || l % (divisions.x) < divisions.x - 1)
                            {
                                allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(3 + skip_up + skip_left )  + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add(  new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) , ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                saveEnd = true;
                                indicesToRotate = new int[] {3};
                            }
                            else
                            {
                                Debug.Log("Entered here with D!");
                                allVertices.Add( tiles[doorGridPosition].endVertices[1 + 4 * (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add( allVertices[(3 + skip_up + skip_left )  + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( tiles[doorGridPosition].endVertices[2 + 4 * (int)(k * divisions.y + l/divisions.x)]);
                            }
                            rotateAround = instructions[i].position;
                            
                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if(v_z * divisions.y > 0 && v_x * divisions.x == j * divisions.x) //C
                        {
                            debug_info += "C";
                            //Connect quad upwards to quad downwards
                            if(tiles[upperFloorGridPosition].startVertices.Count < instructions[i].height * divisions.y * 4)
                            {
                                allVertices.Add( allVertices[(3 + skip_up) + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(2 + skip_up) + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                saveStart = true;
                            }
                            else
                            {
                                allVertices.Add(tiles[upperFloorGridPosition].startVertices[0 + (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add(tiles[upperFloorGridPosition].startVertices[1 + (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            }
                            indicesToRotate = new int[] {2, 3};
                            rotateAround = new Vector3(x,y,z);

                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            { 
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if(v_x * divisions.x > j * divisions.x) //B
                        {
                            debug_info += "B";
                            //Connect quad to the left to quad to the right
                            if(tiles[doorGridPosition].endVertices.Count < instructions[i].height * divisions.y * 4 || l % (divisions.x) < divisions.x - 1)
                            {
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + UnityEngine.Random.Range(-jaggedness, 0) , ((z - v_z )) - k));
                                allVertices.Add( allVertices[(0 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(3 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) , ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                saveEnd = true;
                                indicesToRotate = new int[] {0, 3};
                            }
                            else
                            {
                                allVertices.Add( tiles[doorGridPosition].endVertices[1 + 4 * (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add( allVertices[(0 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( allVertices[(3 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                                allVertices.Add( tiles[doorGridPosition].endVertices[2 + 4 * (int)(k * divisions.y + l/divisions.x)]);
                            }
                            rotateAround = instructions[i].position;

                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            { 
                                allVertices[allVertices.Count-4] = new Vector3(allVertices[allVertices.Count-4].x, allVertices[allVertices.Count-4].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-4].z);
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if(i > 0 && j == 0) //I
                        {
                            debug_info += "I";
                            int jump_column = j > 0 ? (j-1) * vertices_per_column: 0;
                            //Connect the first quad of the second wall to the last quad of the first row of the first wall
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                    , y + UnityEngine.Random.Range(-jaggedness, 0) , ((z - v_z ) ) - k));
                            allVertices.Add( allVertices[(0 + (jump_wall - vertices_per_column) + vertices_per_quad * ((int)divisions.x - 1)) + k * vertices_per_tile]);
                            allVertices.Add( allVertices[(3 + (jump_wall - vertices_per_column) + vertices_per_quad * ((int)divisions.x - 1)) + k * vertices_per_tile]);
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) , ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {0, 3};
                            rotateAround = instructions[i].position;
                            
                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            { 
                                allVertices[allVertices.Count-4] = new Vector3(allVertices[allVertices.Count-4].x, allVertices[allVertices.Count-4].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-4].z);
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else if(j > 0) //G
                        {
                            debug_info += "G";
                            int jump_column = j > 0 ? (j-1) * vertices_per_column: 0;
                            //Connect first quad of column to the right to the last quad of the first row of the first column
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                    , y + UnityEngine.Random.Range(-jaggedness, 0) , ((z - v_z ) ) - k));
                            allVertices.Add( allVertices[(0 + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + k * vertices_per_tile + jump_column]);
                            allVertices.Add( allVertices[(3 + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + k * vertices_per_tile + jump_column]);
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) , ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {0, 3};
                            rotateAround = instructions[i].position;
                            
                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            { 
                                allVertices[allVertices.Count-4] = new Vector3(allVertices[allVertices.Count-4].x, allVertices[allVertices.Count-4].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-4].z);
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc+ 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        else //A
                        {
                            debug_info += "A";
                            //Make lone quad
                            if(tiles[upperFloorGridPosition].startVertices.Count < instructions[i].height * divisions.y * 4)
                            {
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a        , y + UnityEngine.Random.Range(-jaggedness, 0) + rot_c, (z - v_z ) - k));
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b        , y  + UnityEngine.Random.Range(-jaggedness, 0) + rot_c, (z - v_z ) - k));
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b        , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a        , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                                saveStart = true;
                            }
                            else
                            {
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a        , y + UnityEngine.Random.Range(-jaggedness, 0) + rot_c, (z - v_z ) - k));
                                allVertices.Add(tiles[upperFloorGridPosition].startVertices[1 + (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add(tiles[upperFloorGridPosition].startVertices[2 + (int)(k * divisions.y + l/divisions.x)]);
                                allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a        , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            }
                            indicesToRotate = new int[] {0, 1, 2, 3};
                            rotateAround = new Vector3(x,y,z);
                            
                            if(instructions[i].curve != null && instructions[i].curve.keys.Length > 1)
                            { 
                                allVertices[allVertices.Count-4] = new Vector3(allVertices[allVertices.Count-4].x, allVertices[allVertices.Count-4].y - instructions[i].curve.Evaluate(x_perc + 1.0f/divisions.x), allVertices[allVertices.Count-4].z);
                                allVertices[allVertices.Count-3] = new Vector3(allVertices[allVertices.Count-3].x, allVertices[allVertices.Count-3].y, allVertices[allVertices.Count-3].z);
                                allVertices[allVertices.Count-2] = new Vector3(allVertices[allVertices.Count-2].x, allVertices[allVertices.Count-2].y, allVertices[allVertices.Count-2].z);
                                allVertices[allVertices.Count-1] = new Vector3(allVertices[allVertices.Count-1].x, allVertices[allVertices.Count-1].y - instructions[i].curve.Evaluate(x_perc + 1.0f/divisions.x), allVertices[allVertices.Count-1].z);
                            }
                        }
                        if(roundLastColumn)
                        {
                            for(int index = 0; index < indicesToRotate.Length; index++)
                            {
                                //3 = -1
                                //0 = -4
                                //so its indicesToRotate[i] - 4

                                Vector2 circleCenter = new Vector2(0, instructions[i].roundedness); //! Center in a vaccuum
                                circleCenter += new Vector2(x + j - 1.0f/divisions.x, y); //! Make Center relative to this wall tile
                                //Debug.Log("Center of circle is: " + circleCenter);
                                //Debug.Log("While x and y is: " + new Vector2(x + j,y));
                                //Debug.Log("Position towards: " + (Vector2)newVertices[newVertices.Count +(indicesToRotate[index] - 4)]);

                                Vector2 vectorBetween = (circleCenter - (Vector2)allVertices[allVertices.Count +(indicesToRotate[index] - 4)]);
                                Vector2 normal = vectorBetween.normalized; //! Take the normal from the Center to this position
                                Vector2 movedNormal = new Vector2(x + j - 1.0f/divisions.x,y) - normal;
                                // Debug.Log("Normal: " + normal);
                                Vector2 thirdPosition = new Vector2(movedNormal.x , allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].y); //! The third position to the normal and the original vertex

                                float move_x = (thirdPosition - (Vector2)allVertices[allVertices.Count+ (indicesToRotate[index] - 4)]).magnitude;
                                float move_y = ((movedNormal - thirdPosition)).magnitude;
                                
                               // Debug.Log("INDEX: " + (newVertices.Count +(indicesToRotate[index] - 4))); 

                                allVertices[allVertices.Count +(indicesToRotate[index] - 4)] = 
                                    new Vector3(
                                        allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].x - move_x, 
                                        allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].y - move_y + 1, //+1
                                        allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].z);
                            }
                        }
                        else if(roundFirstColumn)
                        {
                            for(int index = 0; index < indicesToRotate.Length; index++)
                            {
                                //3 = -1
                                //0 = -4
                                //so its indicesToRotate[i] - 4
                                //float x_perc_here = (index == 1 || index == 2)? x_perc: x_perc + 1.0f/divisions.x;

                                Vector2 circleCenter = new Vector2(instructions[i - 1].roundedness, instructions[i - 1].roundedness); //! Center in a vaccuum
                                circleCenter += new Vector2(x - 1.0f/divisions.x, y - 1.0f/divisions.x); //! Make Center relative to this wall tile
                                //Debug.Log("Center of circle is: " + circleCenter);
                                //Debug.Log("While x and y is: " + new Vector2(x,y));
                                //Debug.Log("Position towards: " + (Vector2)newVertices[newVertices.Count +(indicesToRotate[index] - 4)]);

                                Vector2 vectorBetween = (circleCenter - (Vector2)allVertices[allVertices.Count +(indicesToRotate[index] - 4)]);
                                Vector2 normal = vectorBetween.normalized; //! Take the normal from the Center to this position
                                Vector2 movedNormal = new Vector2(x - 1.0f/divisions.x,y- 1.0f/divisions.x) - normal;
                                //Debug.Log("Normal: " + normal);
                                Vector2 thirdPosition = new Vector2(movedNormal.x , allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].y); //! The third position to the normal and the original vertex

                                float move_x = (thirdPosition - (Vector2)allVertices[allVertices.Count+ (indicesToRotate[index] - 4)]).magnitude;
                                float move_y = ((movedNormal - thirdPosition)).magnitude;
                                
                             //Debug.Log("INDEX: " + (newVertices.Count +(indicesToRotate[index] - 4))); 

                                allVertices[allVertices.Count +(indicesToRotate[index] - 4)] = 
                                    new Vector3(
                                        allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].x - move_x + 1, 
                                        allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].y - move_y + 1 + 1.0f/divisions.x, 
                                        allVertices[allVertices.Count+ (indicesToRotate[index] - 4)].z);
                            }
                        }

                        CreateWall_Rotate(allVertices, indicesToRotate.ToList(), rotateAround, instructions[i].rotation);

                        if(saveStart)
                        {
                            for(int m = 4; m > 0; m--)
                            {
                                tiles[doorGridPosition].startVertices.Add(allVertices[allVertices.Count-m]);
                            }
                            Debug.Log("Saving start positions at: " + doorGridPosition);
                        }
                        /*if(saveEnd)
                        {
                            for(int m = 4; m > 0; m--)
                            {
                                tiles[upperFloorGridPosition].endVertices.Add(allVertices[allVertices.Count-m]);
                            }
                        }*/

                        //! determine if this is the bottom of the wall and save vertices 0 and 1 
                        if(k == 0 && (int)(l / divisions.x) == 0)
                        {
                            tiles[upperFloorGridPosition].floorVertices.Add(allVertices[allVertices.Count - 4] - instructions[i].movePosition);
                            tiles[upperFloorGridPosition].floorVertices.Add(allVertices[allVertices.Count - 3] - instructions[i].movePosition);
                            //! get the position on the grid that this wall corresponds to
                        }
                        //! determine if this is the top of the wall and save vertices 2 and 3
                        if(k == instructions[i].height - 1 && (int)(l / divisions.x) == instructions[i].height - 1 && !tiles[upperFloorGridPosition].wall)
                        {
                            tiles[upperFloorGridPosition].ceilingVertices.Add(allVertices[allVertices.Count - 2] - instructions[i].movePosition);
                            tiles[upperFloorGridPosition].ceilingVertices.Add(allVertices[allVertices.Count - 1] - instructions[i].movePosition);

                            for(int n = 0; n < tiles[upperFloorGridPosition].sidesWhereThereIsWall.Count; n++)
                            {
                                if( tiles[upperFloorGridPosition].sidesWhereThereIsWall[n].side == (upperFloorGridPosition - currentGridPosition))
                                {
                                    tiles[upperFloorGridPosition].sidesWhereThereIsWall[n].floor = false;
                                }
                            }
                            //! get the position on the grid that this wall corresponds to
                        }
                        
                        int[] indexValue = new int[]{0,1,3,1,2,3};
                        for(int index = 0; index < indexValue.Length; index++){newIndices.Add(indexValue[index] + jump_quad_up + jump_tile);}

                        newUV.Add(new Vector2 (v_x - j + 1.0f / divisions.x, v_z));                      //1,0
                        newUV.Add(new Vector2 (v_x - j                     , v_z));                      //0,0
                        newUV.Add(new Vector2 (v_x - j                     , v_z + 1.0f / divisions.y)); //0,1
                        newUV.Add(new Vector2 (v_x - j + 1.0f / divisions.x, v_z + 1.0f / divisions.y)); //1,1
                        
                        //goto end;
                    }
                    debug_info += "_";
                   // Debug.Log("So far: " + debug_info);
                }

                GameObject wallObject = new GameObject("Wall Object " + j);
                wallObject.transform.parent = wall.transform;

                Vector3[] newVertices = new Vector3[]{};
                Array.Resize(ref newVertices, vertices_per_column);
                allVertices.CopyTo(allVertices.Count-vertices_per_column, newVertices, 0, vertices_per_column);

                WallDebugData wallDebugData = wallObject.AddComponent<WallDebugData>();
                wallDebugData.quadID = debug_info;
                wallDebugData.doorGridPosition = doorGridPosition;

                wallObject.AddComponent<MeshFilter>();
                wallObject.GetComponent<MeshFilter>().mesh.Clear();
                wallObject.GetComponent<MeshFilter>().mesh.vertices = newVertices;
                wallObject.GetComponent<MeshFilter>().mesh.triangles = newIndices.ToArray();
                wallObject.GetComponent<MeshFilter>().mesh.uv = newUV.ToArray(); 
                wallObject.GetComponent<MeshFilter>().mesh.Optimize();
                wallObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                wallObject.AddComponent<MeshRenderer>();
                wallObject.GetComponent<MeshRenderer>().material = wallMaterial;

                MeshCollider mc = wallObject.AddComponent<MeshCollider>();
                mc.sharedMesh = wallObject.GetComponent<MeshFilter>().mesh;

                wallObject.isStatic = true;

                if(Math.Mod(instructions[i].rotation, 360) == 0)
                {
                    currentGridPosition += new Vector2Int(1,0);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                    doorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                }
                else if(Math.Mod(instructions[i].rotation,360) == 90)
                {
                    currentGridPosition += new Vector2Int(0,-1);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
                    doorGridPosition = currentGridPosition + new Vector2Int(1, 0);
                }
                else if(Math.Mod(instructions[i].rotation, 360) == 180)
                {
                    currentGridPosition += new Vector2Int(-1,0);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                    doorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                }
                else if(Math.Mod(instructions[i].rotation, 360) == 270)
                {
                    currentGridPosition += new Vector2Int(0,1);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(1, 0);
                    doorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
                }
            }
        }
        DebugLog.SuccessMessage("Successfully created a wall!");
    }
    static public void CreateWall_AddVertices(List<Vector3> vertices)
    {

    }
    static public void CreateWall_RoundColumn(List<Vector3> vertices)
    {

    }
    static public void CreateWall_Rotate(List<Vector3> vertices, List<int> indices, Vector3 origin, int rotation)
    {
        //Rotates the indices and vertices just made
        //Without this function, the wall would only be able to span infinitely in the direction they were first made
        //Thanks to this function, you can have corners, and also close a wall into a room
        if(Math.Mod(rotation,360) == 0){return;}
        if(indices.Count <= 0)
        {
            return;
        }
        int j = vertices.Count - 4;
        for(int i = 0; i < indices.Count; i++)
        {
            Vector3 dir = vertices[j + indices[i]] - origin;
            dir = Quaternion.Euler(0,0,rotation) * dir;
            vertices[j + indices[i]] = dir + origin;
        }
    }
    static public void CreateSurface(List<SurfaceData> positions, Transform trans, Material floorMaterial)
    {
        //Positions literally mean the position of every single quad on the grid
        List<Vector3> allVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        int lastAmountVert = 0;

        for(int i = 0; i < positions.Count; i++)
        {
             //When it connects to connectingVertices, it needs to know what sides of the tile has been made so it can fill in the rest
            Vector3 centerOfTile = new Vector3(positions[i].position.x + 0.5f, positions[i].position.y + 0.5f, -positions[i].position.z);
            if(positions[i].ceilingVertices.Count > 0)
            {
                for(int j = 0; j < positions[i].ceilingVertices.Count;j+=2)
                {
                    allVertices.Add(new Vector3(positions[i].ceilingVertices[j].x, positions[i].ceilingVertices[j].y, -positions[i].position.z));
                    allVertices.Add(new Vector3(positions[i].ceilingVertices[j+1].x, positions[i].ceilingVertices[j+1].y, -positions[i].position.z));
                    allVertices.Add(centerOfTile);
                    allVertices.Add(centerOfTile);

                    newUV.AddRange(Math.calcUV(allVertices).ToList());
                }
            }
            else
            {
                allVertices.Add(new Vector3(positions[i].position.x,     positions[i].position.y,     -positions[i].position.z));
                allVertices.Add(new Vector3(positions[i].position.x + 1, positions[i].position.y,     -positions[i].position.z));
                allVertices.Add(new Vector3(positions[i].position.x + 1, positions[i].position.y + 1, -positions[i].position.z));
                allVertices.Add(new Vector3(positions[i].position.x,     positions[i].position.y + 1, -positions[i].position.z));

                newUV.AddRange(Math.calcUV(allVertices).ToList());
            }
            //!Then this is a tile with connecting vertices, and it needs to be filled up
            for(int j = 0; j < positions[i].sidesWhereThereIsWall.Count; j++)
            {
                if(positions[i].sidesWhereThereIsWall[j].floor)
                {
                    allVertices.Add(new Vector3(centerOfTile.x - positions[i].sidesWhereThereIsWall[j].side.x * 0.5f + positions[i].sidesWhereThereIsWall[j].side.y * 0.5f, 
                        centerOfTile.y + positions[i].sidesWhereThereIsWall[j].side.y * 0.5f + positions[i].sidesWhereThereIsWall[j].side.x * 0.5f, -positions[i].position.z));
                    allVertices.Add(new Vector3(centerOfTile.x - positions[i].sidesWhereThereIsWall[j].side.x * 0.5f - positions[i].sidesWhereThereIsWall[j].side.y * 0.5f, 
                        centerOfTile.y + positions[i].sidesWhereThereIsWall[j].side.y * 0.5f - positions[i].sidesWhereThereIsWall[j].side.x * 0.5f, -positions[i].position.z));
                    allVertices.Add(centerOfTile);
                    allVertices.Add(centerOfTile);

                    newUV.AddRange(Math.calcUV(allVertices).ToList());

                    positions[i].sidesWhereThereIsWall.RemoveAt(j); j--;
                }
            }
            if(positions[i].floorVertices.Count > 0 && positions[i].sidesWhereThereIsWall.Count > 1)
            {
                for(int j = 0; j < positions[i].floorVertices.Count; j+=2)
                {
                    //! we create a fan structure to the corner
                    Vector3 corner = (positions[i].sidesWhereThereIsWall[0].side - positions[i].sidesWhereThereIsWall[1].side).ToV3() + new Vector3(positions[i].position.x, positions[i].position.y, 0);
                    allVertices.Add(new Vector3(positions[i].floorVertices[j].x,   positions[i].floorVertices[j].y, 0));
                    allVertices.Add(new Vector3(positions[i].floorVertices[j+1].x, positions[i].floorVertices[j+1].y, 0));
                    allVertices.Add(corner);
                    allVertices.Add(corner);

                    newUV.AddRange(Math.calcUV(allVertices).ToList());
                }
            }
            if(allVertices.Count > lastAmountVert + 1000)
            {
                GameObject floorObject = new GameObject("Floor");

                Vector3[] newVertices = new Vector3[]{};
                Array.Resize(ref newVertices, allVertices.Count - lastAmountVert);
                allVertices.CopyTo(lastAmountVert, newVertices, 0, allVertices.Count - lastAmountVert);
                lastAmountVert = allVertices.Count;
                List<int> newIndices = new List<int>();

                for(int j = 0; j < newVertices.Length; j+=4)
                {
                    newIndices.Add(3 + j); //0, 1, 3, 1, 2, 3
                    newIndices.Add(1 + j);
                    newIndices.Add(0 + j);
                    newIndices.Add(3 + j);
                    newIndices.Add(2 + j);
                    newIndices.Add(1 + j);
                }

                floorObject.AddComponent<MeshFilter>();
                floorObject.GetComponent<MeshFilter>().mesh.Init(newVertices.ToArray(), newIndices.ToArray(), newUV.ToArray());

                floorObject.AddComponent<MeshRenderer>();
                floorObject.GetComponent<MeshRenderer>().material = floorMaterial;

                floorObject.isStatic = true;
                floorObject.transform.parent = trans;
                newUV.Clear();

                MeshCollider mc = floorObject.AddComponent<MeshCollider>();
                mc.sharedMesh = floorObject.GetComponent<MeshFilter>().mesh;
            }
        }
        GameObject floorObject2 = new GameObject("Floor");

        Vector3[] newVertices2 = new Vector3[]{};
        Array.Resize(ref newVertices2, allVertices.Count - lastAmountVert);
        allVertices.CopyTo(lastAmountVert, newVertices2, 0, allVertices.Count - lastAmountVert);

        List<int> newIndices2 = new List<int>();
        
        for(int j = 0; j < newVertices2.Length; j+=4)
        {
            newIndices2.Add(3 + j); //0, 1, 3, 1, 2, 3
            newIndices2.Add(1 + j);
            newIndices2.Add(0 + j);
            newIndices2.Add(3 + j);
            newIndices2.Add(2 + j);
            newIndices2.Add(1 + j);
        }

        floorObject2.AddComponent<MeshFilter>();
        floorObject2.GetComponent<MeshFilter>().mesh.Init(newVertices2.ToArray(), newIndices2.ToArray(), newUV.ToArray());

        floorObject2.AddComponent<MeshRenderer>();
        floorObject2.GetComponent<MeshRenderer>().material = floorMaterial;

        floorObject2.isStatic = true;
        floorObject2.transform.parent = trans; 

        MeshCollider mc2 = floorObject2.AddComponent<MeshCollider>();
        mc2.sharedMesh = floorObject2.GetComponent<MeshFilter>().mesh;
    }
    static public void CreateSurface(Mesh mesh, float height)
    {
        //When you are making a completely flat room without walls
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        for(int i = 0; i < 20 * 20; i++) //20 is the standard room size
        {
            newVertices.Add(new Vector3(i % 20, -i / 20 -1, -height));
            newVertices.Add(new Vector3(i % 20 + 1, -i / 20 -1, -height));
            newVertices.Add(new Vector3(i % 20 + 1, -i / 20, -height));
            newVertices.Add(new Vector3(i % 20, -i / 20, -height));

            newTriangles.Add(3 + 4 * i); //0, 1, 3, 1, 2, 3
            newTriangles.Add(1 + 4 * i);
            newTriangles.Add(0 + 4 * i);
            newTriangles.Add(3 + 4 * i);
            newTriangles.Add(2 + 4 * i);
            newTriangles.Add(1 + 4 * i);

            newUV.Add(new Vector2 (1,0));                      //1,0
            newUV.Add(new Vector2 (0,0));                      //0,0
            newUV.Add(new Vector2 (0,1)); //0,1
            newUV.Add(new Vector2 (1,1)); //1,1
        }

        mesh.Clear ();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    static public Mesh GetQuad()
    {
        Mesh mesh = new Mesh();
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        newVertices.Add(new Vector3(1,1,0));
        newVertices.Add(new Vector3(0,1,0));
        newVertices.Add(new Vector3(0,0,0));
        newVertices.Add(new Vector3(1,0,0));

        newTriangles.Add(3);
        newTriangles.Add(1);
        newTriangles.Add(0);
        newTriangles.Add(3);
        newTriangles.Add(2);
        newTriangles.Add(1);

        newUV.Add(new Vector2 (1,0));                      //1,0
        newUV.Add(new Vector2 (0,0));                      //0,0
        newUV.Add(new Vector2 (0,1)); //0,1
        newUV.Add(new Vector2 (1,1)); 

        mesh.Clear ();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}