using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using TileType = Room.RoomTemplate.TileTemplate.TileType;

public partial class MeshMaker: MonoBehaviour
{
    public struct WallData
    {
        public int length; //How many walls will I make in this direction
        public int tilt; //How inclined the wall is into the tile
        public int elevation;
        public int rotation; //Determines what direction the walls are drawn in. Sides, up, down, diagonal etc

        public Vector2Int divisions;

        public Vector3 position; //The start position to draw the wall from

        public Vector2Int actualPosition;
        public AnimationCurve curve;
        public int angleToTurn;
        public TileType type;

        public WallData(Vector3 position_in, Vector2Int actualPosition_in, int rotation_in, int length_in, int height_in, int tilt_in, Vector2Int divisions_in, AnimationCurve curve_in, int angleToTurn, TileType type)
        {
            position = position_in;
            rotation = rotation_in;
            length = length_in;
            elevation = height_in;
            tilt = tilt_in;
            divisions = divisions_in;
            curve = curve_in;
            actualPosition = actualPosition_in;
            this.angleToTurn = angleToTurn;
            this.type = type;
        }
        public WallData(Vector3 position_in, Vector2Int actualPosition_in, int rotation_in, int height_in, int tilt_in, AnimationCurve curve_in, int angleToTurn, TileType type) //If this wall has the same length as the previous one, you don't have to define length
        {
            position = position_in;
            rotation = rotation_in;
            length = 0;
            elevation = height_in;
            tilt = tilt_in;
            divisions = new Vector2Int(1, 1);
            curve = curve_in;
            actualPosition = actualPosition_in;
            this.angleToTurn = angleToTurn;
            this.type = type;
        }
    }
    [System.Flags]
    public enum WallType
    {
        NONE = 0,
        LAST = 1, //when the the wall is rounded
        FIRST = 1 << 1, //when the previous wall is rounded at the end
        INNER = 1 << 2 //We only need one, not both inner and outer
    }
    public static void CreateWall(GameObject wall, MaterialDatabase materialDatabase, WallInstructions instructions, bool wrap, Grid<Room.RoomTemplate.TileTemplate> tiles, float roundedness)
    {
        if (instructions.Count == 0)
        {
            DebugLog.WarningMessage("There were no instructions sent!");
            return;
        }
        List<WallData> data = instructions.Data;

        Vector2Int currentGridPosition = Vector2Int.zero;
        List<Vector3> allVertices = new List<Vector3>();
        List<int> allIndices = new List<int>();
        int indexJump = 0; //Saves the vertex count of all previous walls, so indices knows where to go
        List<Vector2> allUVs = new List<Vector2>();

        for (int wallIndex = 0; wallIndex < instructions.Count; wallIndex++)
        {
            WallData currentWall = data[wallIndex];
            if (currentWall.type == TileType.HOUSE_WALL)
            {
                OnCreateHouseWall(data, ref currentGridPosition, wrap, tiles, wallIndex, ref indexJump, roundedness, ref allVertices, ref allIndices, ref allUVs);
            }
            else
            {
                OnCreateOutdoorsWall(data, ref currentGridPosition, wrap, tiles, wallIndex, ref indexJump, roundedness, ref allVertices, ref allIndices, ref allUVs);
            }
            if (allVertices.Count > 10000 || (wallIndex > 0 && currentWall.elevation != data[wallIndex-1].elevation))
            {
                Material mat = materialDatabase.entries.First(m => m.name == instructions.MaterialName).material;
                CreateWall_Finish(wall, data[instructions.Count - 1], ref allVertices, ref allIndices, ref allUVs, mat);
            }
        }
        Material mat2 = materialDatabase.entries.First(m => m.name == instructions.MaterialName).material;
        CreateWall_Finish(wall, data[instructions.Count-1], ref allVertices, ref allIndices, ref allUVs, mat2);
    }
    public static void OnCreateOutdoorsWall(List<WallData> instructions, ref Vector2Int currentGridPosition, bool wrap, Grid<Room.RoomTemplate.TileTemplate> tiles, int wallIndex, ref int indexJump, float roundedness, ref List<Vector3> allVertices, ref List<int> allIndices, ref List<Vector2> allUVs)
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
            int limit = (currentWall.divisions.x + 1) * currentWall.length;
            int limitMinusLastWall = limit - (currentWall.divisions.x + 1);
            for (int x = 0; x <= limit; x++) //If division is 0, we want to run this at normal length
            {
                float x_increment = 1 / ((float)currentWall.divisions.x + 1);
                float y_increment = 1 / ((float)currentWall.divisions.y + 1);

                Vector3 newVertex = new Vector3(
                    currentWall.position.x + x * x_increment,
                    currentWall.position.y,
                    currentWall.position.z - y * y_increment);

                WallType wallType = Mathf.FloorToInt(x) <= (currentWall.divisions.x + 1) ? WallType.FIRST : Mathf.CeilToInt(x) > limitMinusLastWall ? WallType.LAST : WallType.NONE;
                if (wallIndex > 0 && wallType != WallType.NONE)
                {
                    if (wallType == WallType.FIRST && instructions[wallIndex - 1].angleToTurn == 1)
                    {
                        wallType |= WallType.INNER;
                    }
                    else if (wallType == WallType.LAST && instructions[wallIndex].angleToTurn == 1)
                    {
                        wallType |= WallType.INNER;
                    }
                }
                Vector3 gridPosition = Vector3.zero;
                if (wallType != WallType.NONE)
                {
                    gridPosition = new Vector3(currentWall.position.x + (int)(x * x_increment - x_increment), currentWall.position.y);
                }

                CreateWall_RoundColumn(ref newVertex, gridPosition, wallType, currentWall.divisions, roundedness);

                newVertices.Add(newVertex);
            }
        }
        CreateWall_Rotate(newVertices, instructions[wallIndex]);

        int[] indexValues = new int[]
        {
                2 + (currentWall.divisions.x + 1) * currentWall.length,
                1,
                0,

                1 + (currentWall.divisions.x + 1) * currentWall.length,
                2 + (currentWall.divisions.x + 1) * currentWall.length,
                0
        };
        //Add all indices
        for (int y = 0; y < currentWall.divisions.y + 1; y++) //If divisions is 0, then we want to go through once
        {
            for (int x = 0; x < (currentWall.divisions.x + 1) * currentWall.length; x++) //If divisions is 0, then we want to go through all except the end
            {
                int i = x + ((currentWall.divisions.x + 1) * currentWall.length + 1) * y; //The width here is the vertex to start from
                foreach (var indexValue in indexValues)
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
    }
    public static void OnCreateHouseWall(List<WallData> instructions, ref Vector2Int currentGridPosition, bool wrap, Grid<Room.RoomTemplate.TileTemplate> tiles, int wallIndex, ref int indexJump, float roundedness, ref List<Vector3> allVertices, ref List<int> allIndices, ref List<Vector2> allUVs)
    {
        //Go through each wall
        WallData currentWall = instructions[wallIndex];

        currentGridPosition = new Vector2Int(instructions[wallIndex].actualPosition.x, -instructions[wallIndex].actualPosition.y);

        int rotation = (int)Math.Mod(instructions[wallIndex].rotation, 360);

        float[] offset = new float[2] { 0.2f, -0.2f };

        Vector2Int savedGridPosition = currentGridPosition;

        for (int i = 0; i < 2; i++)
        {
            int length = currentWall.length - i * 2;
            Vector3 wallPosition = currentWall.position;
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newIndices = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();
            currentGridPosition = savedGridPosition;
            wallPosition += i * new Vector3Int(1, 0, 0);
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
            //Add all vertices on length. We assume the wall is only one tile high
            for (int y = 0; y <= currentWall.divisions.y + 1; y++) //if divisions is 0, we want it to run twice
            {
                int limit = (currentWall.divisions.x + 1) * length;
                int limitMinusLastWall = limit - (currentWall.divisions.x + 1);
                float y_increment = 1 / ((float)currentWall.divisions.y + 1);
                float x_increment = 0;
                int x;
                Vector3 newVertex;

                //First, add the start of the outer/inner wall
                if (i == 0)
                {
                    newVertex = new Vector3(
                            wallPosition.x - offset[i],
                            wallPosition.y - offset[i],
                            wallPosition.z - y * y_increment);
                    newVertices.Add(newVertex);
                }
                else
                {
                    newVertex = new Vector3(
                            wallPosition.x - 1 - offset[i],
                            wallPosition.y - offset[i],
                            wallPosition.z - y * y_increment);
                    newVertices.Add(newVertex);
                }

                for (x = 0; x <= limit; x++) //If division is 0, we want to run this at normal length
                {
                    x_increment = 1 / ((float)currentWall.divisions.x + 1);
                    
                    newVertex = new Vector3(
                        wallPosition.x + x * x_increment,
                        wallPosition.y - offset[i],
                        wallPosition.z - y * y_increment);

                    WallType wallType = Mathf.FloorToInt(x) <= (currentWall.divisions.x + 1) ? WallType.FIRST : Mathf.CeilToInt(x) > limitMinusLastWall ? WallType.LAST : WallType.NONE;
                    if (wallIndex > 0 && wallType != WallType.NONE)
                    {
                        if (wallType == WallType.FIRST && instructions[wallIndex - 1].angleToTurn == 1)
                        {
                            wallType |= WallType.INNER;
                        }
                        else if (wallType == WallType.LAST && instructions[wallIndex].angleToTurn == 1)
                        {
                            wallType |= WallType.INNER;
                        }
                    }
                    Vector3 gridPosition = Vector3.zero;
                    if (wallType != WallType.NONE)
                    {
                        gridPosition = new Vector3(wallPosition.x + (int)(x * x_increment - x_increment), currentWall.position.y);
                    }

                    CreateWall_RoundColumn(ref newVertex, gridPosition, wallType, currentWall.divisions, roundedness);

                    newVertices.Add(newVertex);
                }
                //First, add the end of the outer/inner wall
                if(i == 0)
                {
                    newVertex = new Vector3(
                        wallPosition.x + (x - 1) * x_increment + offset[i],
                        wallPosition.y - offset[i],
                        wallPosition.z - y * y_increment);
                    newVertices.Add(newVertex);
                }
                else
                {
                    newVertex = new Vector3(
                        wallPosition.x + x * x_increment + offset[i],
                        wallPosition.y - offset[i],
                        wallPosition.z - y * y_increment);
                    newVertices.Add(newVertex);
                }
            }
            CreateWall_Rotate(newVertices, instructions[wallIndex]);

            int lengthWithAddedStartAndEnd = length + 2;
            int[] indexValues;
            if (i == 0)
            {
                indexValues = new int[]
                {
                2 + (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd,
                1,
                0,

                1 + (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd,
                2 + (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd,
                0
                };
            }
            else
            {
                indexValues = new int[]
                {
                2 + (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd,
                0,
                1,

                1 + (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd,
                0,
                2 + (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd
                };
            }
            //Add all indices
            for (int y = 0; y < currentWall.divisions.y + 1; y++) //If divisions is 0, then we want to go through once
            {
                for (int x = 0; x < (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd; x++) //If divisions is 0, then we want to go through all except the end
                {
                    int j = x + ((currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd + 1) * y; //The width here is the vertex to start from
                    foreach (var indexValue in indexValues)
                    {
                        newIndices.Add(indexJump + indexValue + j);
                    }
                }
            }
            //Add all UVs
            for (int y = 0; y <= currentWall.divisions.y + 1; y++)
            {
                for (int x = 0; x <= (currentWall.divisions.x + 1) * lengthWithAddedStartAndEnd; x++)
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
        }
    }
    static void CreateWall_Finish(GameObject wall, WallData currentWall, ref List<Vector3> allVertices,ref List<int> allIndices, ref List<Vector2> allUVs, Material wallMaterial)
    {
        GameObject wallObject = new GameObject("Wall Object");
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
    static public void CreateWall_RoundColumn(ref Vector3 newVertex, Vector2 gridPos, WallType cornerType, Vector2Int divisions, float roundedness)
    {
        if(roundedness == 0 || cornerType == WallType.NONE) { return; }
        int innerOuter = cornerType.HasFlag(WallType.INNER) ? -1 : 1; 
        if (cornerType.HasFlag(WallType.LAST))
        {
            float x = newVertex.x;
            float y = newVertex.y;
            Vector2 circleCenter = new Vector2(0, roundedness * innerOuter); //! Center in a vaccuum
            circleCenter += gridPos; //! Make Center relative to this wall tile

            Vector2 vectorBetween = circleCenter - (Vector2)newVertex;
            vectorBetween.Normalize(); //! Take the normal from the Center to this position
            Vector2 movedNormal = circleCenter - vectorBetween;

            newVertex = new Vector3(movedNormal.x, movedNormal.y, newVertex.z);
        }
        else if (cornerType.HasFlag(WallType.FIRST))
        {
            float x = newVertex.x;
            float y = newVertex.y;
            Vector2 circleCenter = new Vector2(roundedness, roundedness * innerOuter); //! Center in a vaccuum
            circleCenter += gridPos; //! Make Center relative to this wall tile
                                                                                            
            Vector2 vectorBetween = circleCenter - (Vector2)newVertex;
            vectorBetween.Normalize(); //! Take the normal from the Center to this position
            Vector2 movedNormal = circleCenter - vectorBetween;

            newVertex = new Vector3(movedNormal.x, movedNormal.y, newVertex.z);
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