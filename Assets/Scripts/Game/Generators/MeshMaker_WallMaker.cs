using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public partial class MeshMaker: MonoBehaviour
{
    public struct WallData
    {
        public int length; //How many walls will I make in this direction
        public int tilt; //How inclined the wall is into the tile
        public int elevation;
        public int rotation; //Determines what direction the walls are drawn in. Sides, up, down, diagonal etc

        public float roundedness; //At full roundedness, the corner is perfectly circular besides jaggedness. At zero roundedness, the corner is sharp

        public Vector2Int divisions;

        public Vector3 position; //The start position to draw the wall from

        public Vector2Int actualPosition;
        public AnimationCurve curve;

        public WallData(Vector3 position_in, Vector2Int actualPosition_in, int rotation_in, int length_in, int height_in, int tilt_in, Vector2Int divisions_in, AnimationCurve curve_in, float roundedness_in)
        {
            position = position_in;
            rotation = rotation_in;
            length = length_in;
            elevation = height_in;
            tilt = tilt_in;
            divisions = divisions_in;
            curve = curve_in;
            roundedness = roundedness_in;
            actualPosition = actualPosition_in;
        }
        public WallData(Vector3 position_in, Vector2Int actualPosition_in, int rotation_in, int height_in, int tilt_in, AnimationCurve curve_in, float roundedness_in) //If this wall has the same length as the previous one, you don't have to define length
        {
            position = position_in;
            rotation = rotation_in;
            length = 0;
            elevation = height_in;
            tilt = tilt_in;
            divisions = new Vector2Int(1, 1);
            curve = curve_in;
            roundedness = roundedness_in;
            actualPosition = actualPosition_in;
        }
    }
    public static void CreateWall(GameObject wall, Material wallMaterial, List<WallData> instructions, bool wrap, Grid<Room.RoomTemplate.TileTemplate> tiles)
    {
        if (instructions.Count == 0)
        {
            DebugLog.WarningMessage("There were no instructions sent!");
            return;
        }

        Vector2Int currentGridPosition = Vector2Int.zero;
        List<Vector3> allVertices = new List<Vector3>();
        List<int> allIndices = new List<int>();
        int indexJump = 0; //Saves the vertex count of all previous walls, so indices knows where to go
        List<Vector2> allUVs = new List<Vector2>();

        for (int wallIndex = 0; wallIndex < instructions.Count; wallIndex++)
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newIndices = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();
            //Go through each wall
            WallData currentWall = instructions[wallIndex];

            currentGridPosition = new Vector2Int((int)instructions[wallIndex].actualPosition.x, (int)-instructions[wallIndex].actualPosition.y);
            Vector2Int upperFloorGridPosition = Vector2Int.zero;
            Vector2Int doorGridPosition = Vector2Int.zero; //If this is a door, then you want to save the position the door is on, which is in front

            int rotation = instructions[wallIndex].rotation % 360;

            if (wallIndex > 0 && instructions[wallIndex - 1].rotation == (instructions[wallIndex].rotation - 90) % 360) //! If youre turning after an outer corner, you must push up one step, otherwise it will put a position that has no wall
            {
                if (rotation == 0)
                {
                    currentGridPosition += new Vector2Int(1, 0);
                }
                else if (rotation == 90)
                {
                    currentGridPosition += new Vector2Int(0, -1);
                }
                else if (rotation == 180)
                {
                    currentGridPosition += new Vector2Int(-1, 0);
                }
                else if (rotation == 270)
                {
                    currentGridPosition += new Vector2Int(0, 1);
                }
            }
            if (rotation == 0)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                doorGridPosition = currentGridPosition + new Vector2Int(0, 1);
            }
            else if (rotation == 90)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
                doorGridPosition = currentGridPosition + new Vector2Int(1, 0);
            }
            else if (rotation == 180)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                doorGridPosition = currentGridPosition + new Vector2Int(0, -1);
            }
            else if (rotation == 270)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(1, 0);
                doorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
            }
            //Add all vertices on length. We assume the wall is only one tile high

            for (int y = 0; y <= currentWall.divisions.y + 1; y++) //if divisions is 0, we want it to run twice
            {
                for (int x = 0; x <= (currentWall.divisions.x + 1) * currentWall.length; x++) //If division is 0, we want to run this at normal length
                {
                    float x_increment = 1 / ((float)currentWall.divisions.x + 1);
                    float y_increment = 1 / ((float)currentWall.divisions.y + 1);
                    newVertices.Add(new Vector3(
                        currentWall.position.x + x * x_increment, 
                        currentWall.position.y, 
                        currentWall.position.z - y * y_increment));
                }
            }

            CreateWall_Rotate(newVertices, instructions[wallIndex]);

            int[] indexValues = new int[] 
            { 
                2 + (currentWall.divisions.x + 1) * currentWall.length, 
                1, 
                0,

                1 + (currentWall.divisions.x + 1) * currentWall.length,
                2 +(currentWall.divisions.x + 1) * currentWall.length,
                0
            };
            //Add all indices
            for (int y = 0; y < currentWall.divisions.y + 1; y++) //If divisions is 0, then we want to go through once
            {
                for (int x = 0; x < (currentWall.divisions.x + 1) * currentWall.length; x++) //If divisions is 0, then we want to go through all except the end
                {
                    int i = x + ((currentWall.divisions.x + 1) * currentWall.length + 1) * y; //The width here is the vertex to start from
                    foreach(var indexValue in indexValues)
                    {
                        newIndices.Add(indexJump + indexValue + i);
                    }
                }
            }
            //Add all UVs
            for (int y = 0; y <= currentWall.divisions.y + 1; y++)
            {
                for (int x = 0; x <= (currentWall.divisions.x + 1) * currentWall.length; x++)
                {
                    float xIncrement = 1 / ((float)currentWall.divisions.x + 1);
                    float yIncrement = 1 / ((float)currentWall.divisions.y + 1);
                    newUVs.Add(new Vector2(x * xIncrement, y * yIncrement));
                }
            }
            allVertices.AddRange(newVertices);
            allIndices.AddRange(newIndices);
            allUVs.AddRange(newUVs);
            indexJump = allVertices.Count;
            if (allVertices.Count > 10000 || (wallIndex > 0 && currentWall.elevation != instructions[wallIndex-1].elevation))
            {
                CreateWall_Finish(wall, instructions[instructions.Count - 1], ref allVertices, ref allIndices, ref allUVs, wallMaterial);
            }
        }
        CreateWall_Finish(wall, instructions[instructions.Count-1], ref allVertices, ref allIndices, ref allUVs, wallMaterial);
    }
    static void CreateWall_Finish(GameObject wall, WallData currentWall, ref List<Vector3> allVertices,ref List<int> allIndices, ref List<Vector2> allUVs, Material wallMaterial)
    {
        GameObject wallObject = new GameObject("Wall Object ");
        wallObject.transform.parent = wall.transform;
        wallObject.transform.position -= new Vector3(0, 0, currentWall.elevation);

        wallObject.AddComponent<MeshFilter>();
        wallObject.GetComponent<MeshFilter>().mesh.Clear();
        wallObject.GetComponent<MeshFilter>().mesh.vertices = allVertices.ToArray();
        wallObject.GetComponent<MeshFilter>().mesh.triangles = allIndices.ToArray();
        wallObject.GetComponent<MeshFilter>().mesh.uv = allUVs.ToArray();
        wallObject.GetComponent<MeshFilter>().mesh.Optimize();
        wallObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        wallObject.AddComponent<MeshRenderer>();
        wallObject.GetComponent<MeshRenderer>().material = wallMaterial;

        MeshCollider mc = wallObject.AddComponent<MeshCollider>();
        mc.sharedMesh = wallObject.GetComponent<MeshFilter>().mesh;

        allVertices.Clear();
        allIndices.Clear();
        allUVs.Clear();
    }
   
    static public void CreateWall_AddVertices(List<Vector3> vertices)
    {

    }
    static public void CreateWall_AddJaggedness(ref List<Vector3> vertices, int[] indicesToRotate, float jaggedness)
    {
        int count = vertices.Count;
        foreach(int i in indicesToRotate)
        {
            vertices[count - 4 + i] += new Vector3(Random.Range(-jaggedness, jaggedness), Random.Range(-jaggedness, jaggedness), 0);
        }
    }
    static public void CreateWall_RoundColumn(ref List<Vector3> allVertices, bool roundLastColumn, bool roundFirstColumn, ref int [] indicesToRotate, List<WallData> instructions, int i, int j, float x, float y, Vector2Int divisions)
    {
        if (roundLastColumn)
        {
            for (int index = 0; index < indicesToRotate.Length; index++)
            {
                Vector2 circleCenter = new Vector2(0, instructions[i].roundedness); //! Center in a vaccuum
                circleCenter += new Vector2(x + j, y); //! Make Center relative to this wall tile

                Vector2 vectorBetween = (circleCenter - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]);
                Vector2 normal = vectorBetween.normalized; //! Take the normal from the Center to this position
                Vector2 movedNormal = new Vector2(x + j, y) - normal;

                Vector2 thirdPosition = new Vector2(movedNormal.x, allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y); //! The third position to the normal and the original vertex
                
                float move_x = (thirdPosition - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]).magnitude;
                float move_y = ((movedNormal - thirdPosition)).magnitude;


                allVertices[allVertices.Count + (indicesToRotate[index] - 4)] =
                    new Vector3(
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].x - move_x,
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y - move_y + 1, //+1
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].z);
            }
        }
        else if (roundFirstColumn)
        {
            for (int index = 0; index < indicesToRotate.Length; index++)
            {

                Vector2 circleCenter = new Vector2(instructions[i - 1].roundedness, instructions[i - 1].roundedness); //! Center in a vaccuum
                circleCenter += new Vector2(x, y); //! Make Center relative to this wall tile
                                                                                            
                Vector2 vectorBetween = (circleCenter - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]);
                Vector2 normal = vectorBetween.normalized; //! Take the normal from the Center to this position
                Vector2 movedNormal = new Vector2(x, y) - normal;

                Vector2 thirdPosition = new Vector2(movedNormal.x, allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y); //! The third position to the normal and the original vertex

                float move_x = (thirdPosition - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]).magnitude;
                float move_y = ((movedNormal - thirdPosition)).magnitude;

                //Debug.Log("INDEX: " + (newVertices.Count +(indicesToRotate[index] - 4))); 

                allVertices[allVertices.Count + (indicesToRotate[index] - 4)] =
                    new Vector3(
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].x - move_x + 1,
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y - move_y + 1,
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].z);
            }
        }
    }
    static public void CreateWall_Rotate(List<Vector3> vertices, WallData wall)
    {
        //Rotates the indices and vertices just made
        //Without this function, the wall would only be able to span infinitely in the direction they were first made
        //Thanks to this function, you can have corners, and also close a wall into a room
        if (Math.Mod(wall.rotation, 360) == 0) { return; }
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 dir = vertices[i] - wall.position;
            dir = Quaternion.Euler(0, 0, wall.rotation) * dir;
            vertices[i] = dir + wall.position;
        }
    }
    static public void CreateWall_Rotate(List<Vector3> vertices, List<int> indices, Vector3 origin, int rotation)
    {
        //Rotates the indices and vertices just made
        //Without this function, the wall would only be able to span infinitely in the direction they were first made
        //Thanks to this function, you can have corners, and also close a wall into a room
        if (Math.Mod(rotation, 360) == 0) { return; }
        if (indices.Count <= 0)
        {
            return;
        }
        int j = vertices.Count - 4;
        for (int i = 0; i < indices.Count; i++)
        {
            Vector3 dir = vertices[j + indices[i]] - origin;
            dir = Quaternion.Euler(0, 0, rotation) * dir;
            vertices[j + indices[i]] = dir + origin;
        }
    }
}