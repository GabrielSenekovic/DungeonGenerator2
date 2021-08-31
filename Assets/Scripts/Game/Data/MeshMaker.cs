﻿using System.Collections;
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

        public AnimationCurve curve;

        public WallData(Vector3 position_in, int rotation_in, int length_in, int height_in, int tilt_in, Vector2Int divisions_in, AnimationCurve curve_in, float roundedness_in)
        {
            position = position_in;
            rotation = rotation_in;
            length = length_in;
            height = height_in;
            tilt = tilt_in;
            divisions = divisions_in;
            curve = curve_in;
            roundedness = roundedness_in;
        }
        public WallData(Vector3 position_in, int rotation_in, int height_in, int tilt_in, AnimationCurve curve_in, float roundedness_in) //If this wall has the same length as the previous one, you don't have to define length
        {
            position = position_in;
            rotation = rotation_in;
            length = 0;
            height = height_in;
            tilt = tilt_in;
            divisions = new Vector2Int(1,1);
            curve = curve_in;
            roundedness = roundedness_in;
        }
    }
    public static void CreateChest(Mesh mesh, int length)
    {
        //Specify how long the chest is. Length determines how long it is from left to right, if the opening side is in front
        //Start off by creating just a cube chest
        //Like in Minecraft
        //Start from the back then forward. Save the points on the top where the lid will rotate around
    }
    public static void CreateCylinder(Mesh mesh, float height)
    {
        //Create a vase from a sinewave controlling a radius
        List<Vector3> positions = new List<Vector3>();
        float angleIncrement = 360.0f / 10.0f;
        float currentHeight = 0;
        float heightIncrement = height / 10.0f;

        for(int i = 0; i < 10; i++)
        {
            //Go through all levels upwards
            float angle = 0;
            float currentRadius = 0.5f;

            for(int j = 0; j < 10; j++)
            {
                //Go around the circle
                positions.Add(new Vector3(currentRadius * Mathf.Sin(angle * Mathf.Deg2Rad), currentRadius * Mathf.Cos(angle* Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight += heightIncrement;
        }
        Debug.Log("Amount of positions: " + positions.Count);

        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        //Now fill up the vertices
        for(int i = 0; i < positions.Count - 10; i++)
        {
            newVertices.Add(positions[0 + i]);
            newVertices.Add(positions[(1 + i)%10 + 10 * (int)((float)i/10.0f)]);
            newVertices.Add(positions[(11 + i)%10 + 10 * (int)((float)(i+10)/10.0f)]);
            newVertices.Add(positions[10 + i]);

            newUV.Add(new Vector2 (1, 0));                      //1,0
            newUV.Add(new Vector2 (0, 0));                      //0,0
            newUV.Add(new Vector2 (0, 1)); //0,1
            newUV.Add(new Vector2 (1, 1)); //1,1

            int[] indexValue = new int[]{0,1,3,1,2,3};
            for(int index = 0; index < indexValue.Length; index++)
            {
                newTriangles.Add(indexValue[index] + i * 4);
            }
        }

        mesh.Clear ();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
    
    public static void CreateTuft(Mesh mesh)
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
        float amountOfStraws = 40;
        float radius_increase = (maxRadius - radius) / amountOfStraws;
        float grassWidth = 0.1f;

        float quadsPerGrass = 4; //10

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
        health.maxHealth = 1; health.currentHealth = 1;

        vase.AddComponent<DropItems>();
        if(vase.GetComponent<DropItems>())
        {
            Debug.Log("This vase has drop items");
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

        for(int j = 0; j < amountOfQuadsVertical; j++)
        {
            //Go through all levels upwards
            float angle = 0;
            //0.5f radius is base radius, cuz it fills up one tile
            float currentRadius = curve.Evaluate((float)j/(float)amountOfQuadsVertical);

            for(int k = 0; k < 10; k++)
            {
                //Go around the circle
                positions.Add(new Vector3(currentRadius * Mathf.Sin(angle * Mathf.Deg2Rad), currentRadius * Mathf.Cos(angle* Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight += heightIncrement;
        }

        currentHeight-=heightIncrement;

        for(int j = amountOfQuadsVertical -1; j >= 0; j--)
        {
            //Go through all levels downwards
            float angle = 0;
            float currentRadius = curve.Evaluate((float)j/(float)amountOfQuadsVertical);

            for(int k = 9; k >= 0; k--)
            {
                //Go around the circle
                positions.Add(new Vector3(currentRadius * Mathf.Sin(angle * Mathf.Deg2Rad), currentRadius * Mathf.Cos(angle* Mathf.Deg2Rad), -currentHeight));
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
    public static void CreateWall(GameObject wall, Material wallMaterial, List<WallData> instructions, bool wrap, List<Room.RoomTemplate.TileTemplate> tiles)
    {
        Debug.Log("Enters CreateWall");
        //ref List<Room.EntranceData> entrancesOfThisRoom, ref List<Room.EntranceData> entrancesOfRoomAtStart, ref List<Room.EntranceData> entrancesOfRoomAtEnd
        float jaggedness = 0.04f;
        Debug.Log("Instructions: " + instructions.Count);
        Vector2 divisions = new Vector2(instructions[0].divisions.x +1, instructions[0].divisions.y+1);
        if(instructions[0].divisions.x == 1)
        {
            jaggedness = 0;
        }
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
                            allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(3 + skip_up + skip_left )+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left)+ jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add(  new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {3};
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
                            allVertices.Add( allVertices[((3 + skip_up) + k * vertices_per_tile) + j * vertices_per_column]);
                            allVertices.Add( allVertices[((2 + skip_up) + k * vertices_per_tile) + j * vertices_per_column]);
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness)  + rot_c, ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a   , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness) + rot_c , ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) -firstQuad_leftVal_z) - k));
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
                            allVertices.Add( allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(3 + skip_up + skip_left )  + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(((4 * (int)divisions.x) + vertices_per_quad -1) + skip_up + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add(  new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) , ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {3};
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
                            allVertices.Add( allVertices[(3 + skip_up) + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(2 + skip_up) + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a    , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
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
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + UnityEngine.Random.Range(-jaggedness, 0) , ((z - v_z )) - k));
                            allVertices.Add( allVertices[(0 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( allVertices[(3 + skip_left) + jump_wall + k * vertices_per_tile + j * vertices_per_column]);
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness))                     , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) , ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            indicesToRotate = new int[] {0, 3};
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
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a        , y + UnityEngine.Random.Range(-jaggedness, 0) + rot_c, (z - v_z ) - k));
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b        , y  + UnityEngine.Random.Range(-jaggedness, 0) + rot_c, (z - v_z ) - k));
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b        , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
                            allVertices.Add( new Vector3 ((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a        , y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) -firstQuad_leftVal_z) - k));
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
                        
                        /*int[] indexValue = new int[]{0,1,3,1,2,3};
                        for(int index = 0; index < indexValue.Length; index++){newTriangles.Add(indexValue[index] + jump_quad_up + jump_tile + jump_quad_side + jump_wall);}*/
                        int[] indexValue = new int[]{0,1,3,1,2,3};
                        for(int index = 0; index < indexValue.Length; index++){newIndices.Add(indexValue[index] + jump_quad_up + jump_tile);}

                        newUV.Add(new Vector2 (v_x - j + 1.0f / divisions.x, v_z));                      //1,0
                        newUV.Add(new Vector2 (v_x - j                     , v_z));                      //0,0
                        newUV.Add(new Vector2 (v_x - j                     , v_z + 1.0f / divisions.y)); //0,1
                        newUV.Add(new Vector2 (v_x - j + 1.0f / divisions.x, v_z + 1.0f / divisions.y)); //1,1
                        //goto end;
                    }
                    debug_info += "_";
                    //Debug.Log("So far: " + debug_info);
                }
               // Debug.Log(debug_info);

                GameObject wallObject = new GameObject("Wall Object " + j);
                wallObject.transform.parent = wall.transform;

                Vector3[] newVertices = new Vector3[]{};
                Array.Resize(ref newVertices, vertices_per_column);
                allVertices.CopyTo(allVertices.Count-vertices_per_column, newVertices, 0, vertices_per_column);

                wallObject.AddComponent<MeshFilter>();
                wallObject.GetComponent<MeshFilter>().mesh.Clear();
                wallObject.GetComponent<MeshFilter>().mesh.vertices = newVertices;
                wallObject.GetComponent<MeshFilter>().mesh.triangles = newIndices.ToArray();
                wallObject.GetComponent<MeshFilter>().mesh.uv = newUV.ToArray(); 
                wallObject.GetComponent<MeshFilter>().mesh.Optimize();
                wallObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                wallObject.AddComponent<MeshRenderer>();
                wallObject.GetComponent<MeshRenderer>().material = wallMaterial;

                wallObject.isStatic = true;

                if(i == instructions[i].length - 1 && j == savedLengthOfWall - 1) //If last wall and last column
                {
                    //Collect every last vertices
                }
            }
           // BoxCollider box = wall.AddComponent<BoxCollider>();
           // box.size = new Vector3( instructions[i].length,1, instructions[i].height);
            //box.center = new Vector3(box.size.x / 2 - 0.5f,0.5f,-box.size.z / 2);
        }
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
       // Debug.Log("<color=magenta>Origin: " + origin + " and rotation: " + rotation + "</color>");
        for(int i = 0; i < indices.Count; i++)
        {
            Vector3 dir = vertices[j + indices[i]] - origin;
            dir = Quaternion.Euler(0,0,rotation) * dir;
           // Debug.Log("Dir: " + dir + " and i: " + i);
            vertices[j + indices[i]] = dir + origin;
        }
    }
    static public void CreateSurface(List<Vector3Int> positions, Mesh mesh)
    {
        //Positions literally mean the position of every single quad on the grid
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        for(int i = 0; i < positions.Count; i++)
        {
            newVertices.Add(new Vector3(positions[i].x,     positions[i].y,     -positions[i].z));
            newVertices.Add(new Vector3(positions[i].x + 1, positions[i].y,     -positions[i].z));
            newVertices.Add(new Vector3(positions[i].x + 1, positions[i].y + 1, -positions[i].z));
            newVertices.Add(new Vector3(positions[i].x,     positions[i].y + 1, -positions[i].z));

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
}