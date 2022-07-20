using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public partial class MeshMaker: MonoBehaviour
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
            divisions = new Vector2Int(1, 1);
            curve = curve_in;
            roundedness = roundedness_in;
            actualPosition = actualPosition_in;
            movePosition = movePosition_in;
        }
    }
    public static void CreateWall(GameObject wall, Material wallMaterial, List<WallData> instructions, bool wrap, Grid<Room.RoomTemplate.TileTemplate> tiles)
    {
        if (instructions.Count == 0)
        {
            DebugLog.WarningMessage("There were no instructions sent!");
            return;
        }
        Debug.Log("Enters CreateWall");
        //ref List<Room.EntranceData> entrancesOfThisRoom, ref List<Room.EntranceData> entrancesOfRoomAtStart, ref List<Room.EntranceData> entrancesOfRoomAtEnd
        float jaggedness = 0.04f;
        Vector2 divisions = new Vector2(instructions[0].divisions.x + 1, instructions[0].divisions.y + 1);
        if (instructions[0].divisions.x == 1)
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

        for (int wallIndex = 0; wallIndex < instructions.Count; wallIndex++)
        {
            //Go through each wall
            float x = instructions[wallIndex].position.x - centering;
            float y = instructions[wallIndex].position.y;
            float z = instructions[wallIndex].position.z;

            currentGridPosition = new Vector2Int((int)instructions[wallIndex].actualPosition.x, (int)-instructions[wallIndex].actualPosition.y);
            Vector2Int upperFloorGridPosition = Vector2Int.zero;
            Vector2Int doorGridPosition = Vector2Int.zero; //If this is a door, then you want to save the position the door is on, which is in front

            if (wallIndex > 0 && instructions[wallIndex - 1].rotation == (instructions[wallIndex].rotation - 90) % 360) //! If youre turning after an outer corner, you must push up one step, otherwise it will put a position that has no wall
            {
                if (Math.Mod(instructions[wallIndex].rotation, 360) == 0)
                {
                    currentGridPosition += new Vector2Int(1, 0);
                }
                else if (Math.Mod(instructions[wallIndex].rotation, 360) == 90)
                {
                    currentGridPosition += new Vector2Int(0, -1);
                }
                else if (Math.Mod(instructions[wallIndex].rotation, 360) == 180)
                {
                    currentGridPosition += new Vector2Int(-1, 0);
                }
                else if (Math.Mod(instructions[wallIndex].rotation, 360) == 270)
                {
                    currentGridPosition += new Vector2Int(0, 1);
                }
            }
            if (Math.Mod(instructions[wallIndex].rotation, 360) == 0)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                doorGridPosition = currentGridPosition + new Vector2Int(0, 1);
            }
            else if (Math.Mod(instructions[wallIndex].rotation, 360) == 90)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
                doorGridPosition = currentGridPosition + new Vector2Int(1, 0);
            }
            else if (Math.Mod(instructions[wallIndex].rotation, 360) == 180)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                doorGridPosition = currentGridPosition + new Vector2Int(0, -1);
            }
            else if (Math.Mod(instructions[wallIndex].rotation, 360) == 270)
            {
                upperFloorGridPosition = currentGridPosition + new Vector2Int(1, 0);
                doorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
            }

            int amount_of_faces = (int)(divisions.x * divisions.y);
            const int vertices_per_quad = 4;
            int vertices_per_tile = amount_of_faces * vertices_per_quad;
            int vertices_per_column = vertices_per_tile * instructions[wallIndex].height;

            int lengthOfWall = instructions[wallIndex].length > 0 ? instructions[wallIndex].length : savedLengthOfWall;
            int previousLengthOfWall = instructions[wallIndex].length > 0 && wallIndex > 0 ? instructions[wallIndex - 1].length : savedLengthOfWall; //Get this so you can jump over the previous wall
            savedLengthOfWall = lengthOfWall;

            jump_wall = wallIndex > 0 ? jump_wall + (previousLengthOfWall * vertices_per_column) : 0; //This value always grows. It doesnt reset in each loop

            float tilt_increment = instructions[wallIndex].tilt / (instructions[wallIndex].height * divisions.y);

            for (int columnIndex = 0; columnIndex < savedLengthOfWall; columnIndex++)
            {
                //Debug.Log("New Column! Going this many steps: " + savedLengthOfWall);
                string debug_info = columnIndex + ": ";
                List<int> newIndices = new List<int>();
                List<Vector2> newUV = new List<Vector2>();
                //Go through each column of wall
                for (int tileIndex = 0; tileIndex < instructions[wallIndex].height; tileIndex++)
                {
                    //Go through each square upwards
                    for (int vertexIndex = 0; vertexIndex < divisions.x * divisions.y; vertexIndex++)
                    {
                        //Go through each vertex of the square
                        float x_perc = ((vertexIndex % divisions.x) / divisions.x);

                        float v_x = ((vertexIndex % divisions.x) / divisions.x) + columnIndex;
                        float v_z = (vertexIndex / (int)divisions.x) / divisions.y;
                        int skip_left = (int)(Mathf.RoundToInt(((v_x - columnIndex) * divisions.x) - 1)) * vertices_per_quad;
                        //I have to RoundToInt here because for some reason, on the second wall when doing D, it gave me 2 - 1 = 0.9999999
                        //Epsilons suck
                        int skip_up = (int)(Mathf.RoundToInt((v_z * divisions.y) - 1)) * vertices_per_quad * (int)divisions.x;

                        int steps_up = tileIndex * (int)divisions.y + (int)(v_z * divisions.y); //counts the total row you are on
                        float current_tilt_increment = tilt_increment + tilt_increment * steps_up;

                        //TODO  CHANGE THE WAY THAT THE WALL QUADS ARE MADE SO THAT INSTEAD OF CREATING EACH QUAD FROM RIGHT TO LEFT WHEN THE WALL IS MADE FROM LEFT TO RIGHT, SO YOU DONT HAVE TO DO THE FOLLOWING BULLSHIT 
                        //TODO  JUST TO ACCOMODATE FOR THE ROTATION

                        float rot_a = 0, rot_b = 0, rot_c = 0;

                        float firstQuad_leftVal_x = 1.0f / divisions.x; //1.0f only works for the 0 angle rotated wall, not for the other ones
                        float firstQuad_leftVal_z = 1.0f / divisions.y;

                        bool roundLastColumn = v_x > savedLengthOfWall - 1  //! If the last column of the wall
                                            && wallIndex < instructions.Count - 1 //! If not the last wall, since it must be leading into something
                                            && instructions[wallIndex].roundedness > 0;
                        bool roundFirstColumn = v_x < 1.0f
                                            && wallIndex > 0
                                            && instructions[wallIndex - 1].roundedness > 0;
                        bool saveStart = false; //These are used to decide when to save to these lists, but it has to be a bool because it cant be done before rotation
                        bool saveEnd = false;

                        if (Math.Mod(instructions[wallIndex].rotation, 360) == 0)
                        {
                            rot_b = -1.0f / divisions.x;
                        }
                        else if (Math.Mod(instructions[wallIndex].rotation, 360) == 90)
                        {
                            float mult_1 = 0.5f * (divisions.x - 2);
                            float mult_2 = 1.0f - mult_1;
                            //d and f are wrong atm
                            rot_a = 1.0f / divisions.x * mult_1;
                            rot_b = -1.0f / divisions.x * mult_2;
                            rot_c = 1.0f / divisions.x * mult_1;
                        }
                        else if (Math.Mod(instructions[wallIndex].rotation, 360) == 180)
                        {
                            float mult_1 = divisions.x - 2;
                            float mult_2 = divisions.x - 3;

                            rot_a = 1.0f / divisions.x * mult_1;
                            rot_b = 1.0f / divisions.x * mult_2;
                        }
                        else if (Math.Mod(instructions[wallIndex].rotation, 360) == 270)
                        {
                            float mult_1 = 0.5f * (divisions.x - 2);
                            float mult_2 = 1.0f - mult_1;

                            rot_a = 1.0f / divisions.x * mult_1;
                            rot_b = -1.0f / divisions.x * mult_2;
                            rot_c = -1.0f / divisions.x * mult_1;
                        }
                        //TODO /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        //The 4 is because there's 4 vertices per quad
                        int jump_quad_up = vertices_per_quad * vertexIndex;
                        int jump_tile = amount_of_faces * tileIndex * vertices_per_quad;
                        int jump_quad_side = amount_of_faces * (int)instructions[wallIndex].height * columnIndex * vertices_per_quad;

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
                        if (vertexIndex > divisions.x * divisions.y - divisions.x)
                        {
                            upperJaggedness = 0;
                        }
                        int[] indicesToRotate = { };
                        Vector3 rotateAround = Vector3.zero;
                        int[] verticesToApplyCurveTo = { };

                        if (wrap && wallIndex == instructions.Count - 1 && columnIndex == savedLengthOfWall - 1 && v_z * divisions.y + tileIndex > 0 && Mathf.Round(v_x * divisions.x) == Mathf.Round(columnIndex * divisions.x + divisions.x - 1)) //L
                        {
                            //If on the last wall
                            //Then connect to the first wall
                            debug_info += "L";
                            Debug.Log("L");
                            //Connect tiles upwards to tiles downwards
                            allVertices.Add(allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                            if (divisions.x > 1)
                            {
                                allVertices.Add(allVertices[(3 + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(((4 * (int)divisions.x) + vertices_per_quad - 1) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                            }
                            else
                            {
                                allVertices.Add(allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + tileIndex * vertices_per_tile + (columnIndex - 1) * vertices_per_column]);
                                allVertices.Add(allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x * 2 - 1)) + jump_wall + tileIndex * vertices_per_tile + (columnIndex - 1) * vertices_per_column]);
                            }
                            allVertices.Add(allVertices[2 + skip_up + tileIndex * vertices_per_tile + vertices_per_quad * (int)divisions.x]);
                        }
                        else if (wrap && wallIndex == instructions.Count - 1 && columnIndex == savedLengthOfWall - 1 && Mathf.Round(v_x * divisions.x) == Mathf.Round(columnIndex * divisions.x + divisions.x - 1)) //K
                        {
                            //If on the last wall
                            //Then connect to the first wall
                            debug_info += "K";
                            Debug.Log("K");
                            allVertices.Add(allVertices[1]);
                            if (divisions.x > 1)
                            {
                                allVertices.Add(allVertices[(0 + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(3 + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                            }
                            else //When divisions.x is 1, then G isnt there to connect to the column before. Instead K has to do it, and it isnt normally equipped to do so.
                            {
                                int jump_column = columnIndex > 0 ? (columnIndex - 1) * vertices_per_column : 0;
                                allVertices.Add(allVertices[(0 + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + tileIndex * vertices_per_tile + jump_column]);
                                allVertices.Add(allVertices[(3 + vertices_per_quad * ((int)divisions.x - 1)) + jump_wall + tileIndex * vertices_per_tile + jump_column]);
                            }
                            allVertices.Add(allVertices[2]);
                        }
                        else if (v_x * divisions.x > columnIndex * divisions.x && tileIndex > 0) //F
                        {
                            debug_info += "F";
                            Debug.Log("F");
                            //Connect tiles upwards to tiles downwards
                            if (tiles[doorGridPosition].endVertices.Count < instructions[wallIndex].height * divisions.y * 4 || vertexIndex % (divisions.x) < divisions.x - 1)
                            {
                                if (tileIndex == instructions[wallIndex].height - 1 && (int)(vertexIndex / divisions.x) == instructions[wallIndex].height - 1)
                                {
                                    allVertices.Add(allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(allVertices[(3 + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(allVertices[(((4 * (int)divisions.x) + vertices_per_quad - 1) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(new Vector3((x + v_x), y + current_tilt_increment, ((z - v_z) - firstQuad_leftVal_z) - tileIndex));
                                }
                                else
                                {
                                    allVertices.Add(allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(allVertices[(3 + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(allVertices[(((4 * (int)divisions.x) + vertices_per_quad - 1) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)), y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness), ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) - firstQuad_leftVal_z) - tileIndex));
                                }
                                saveEnd = true;
                                indicesToRotate = new int[] { 3 };
                            }
                            else
                            {
                                allVertices.Add(tiles[doorGridPosition].endVertices[1 + 4 * (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                allVertices.Add(allVertices[(3 + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(((4 * (int)divisions.x) + vertices_per_quad - 1) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(tiles[doorGridPosition].endVertices[2 + 4 * (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                            }
                            rotateAround = instructions[wallIndex].position;

                            if (instructions[wallIndex].curve != null && instructions[wallIndex].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count - 1] = new Vector3(allVertices[allVertices.Count - 1].x, allVertices[allVertices.Count - 1].y - instructions[wallIndex].curve.Evaluate(x_perc + 1.0f / divisions.x), allVertices[allVertices.Count - 1].z);
                            }
                        }
                        else if ((v_z > 0 || tileIndex > 0) && v_x * divisions.x == columnIndex * divisions.x && (columnIndex > 0 || wallIndex > 0) || (tileIndex > 0 || (v_z * divisions.y > 0 && v_x * divisions.x == columnIndex * divisions.x))) //H && J
                        {
                            if ((v_z > 0 || tileIndex > 0) && v_x * divisions.x == columnIndex * divisions.x && (columnIndex > 0 || wallIndex > 0))
                            {
                                int jump_column = columnIndex > 0 ? (columnIndex - 1) * vertices_per_column : 0;
                                int jump_value = columnIndex > 0 ? jump_wall : (jump_wall - vertices_per_column);

                                debug_info += "H"; Debug.Log("H"); //Connect the first quad of each row of right columns to the last quad of each row of left columns

                                allVertices.Add(allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x - 1)) + jump_value + tileIndex * vertices_per_tile + jump_column]);
                                allVertices.Add(allVertices[(3 + skip_up + vertices_per_quad * ((int)divisions.x * 2 - 1)) + jump_value + tileIndex * vertices_per_tile + jump_column]);
                                allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)), y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, upperJaggedness), ((z - v_z - UnityEngine.Random.Range(-upperJaggedness, upperJaggedness)) - firstQuad_leftVal_z) - tileIndex));

                                indicesToRotate = new int[] { 3 };
                                rotateAround = instructions[wallIndex].position;
                            }
                            else if(tileIndex > 0 || (v_z * divisions.y > 0 && v_x * divisions.x == columnIndex * divisions.x))
                            {
                                debug_info += "C"; Debug.Log("C");
                                //1 & 2
                                if (tiles[upperFloorGridPosition].startVertices.Count < instructions[wallIndex].height * divisions.y * 4)
                                {
                                    allVertices.Add(allVertices[(3 + skip_up) + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    allVertices.Add(allVertices[(2 + skip_up) + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                    saveStart = true;
                                }
                                else
                                {
                                    allVertices.Add(tiles[upperFloorGridPosition].startVertices[0 + (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                    allVertices.Add(tiles[upperFloorGridPosition].startVertices[1 + (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                }
                                //3
                                if (tileIndex == instructions[wallIndex].height - 1 && (int)(vertexIndex / divisions.x) == instructions[wallIndex].height - 1)
                                { //if is at the top of the wall
                                    allVertices.Add(new Vector3((x + v_x) + rot_b, y + current_tilt_increment + rot_c, ((z - v_z) - firstQuad_leftVal_z) - tileIndex));
                                }
                                else
                                {
                                    allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_b, y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) - firstQuad_leftVal_z) - tileIndex));
                                }
                                //4
                                allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + rot_a, y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + rot_c, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) - firstQuad_leftVal_z) - tileIndex));
                                indicesToRotate = new int[] { 2, 3 };
                                rotateAround = new Vector3(x, y, z);
                            }
                            if (instructions[wallIndex].curve != null && instructions[wallIndex].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count - 1] = new Vector3(allVertices[allVertices.Count - 1].x, allVertices[allVertices.Count - 1].y - instructions[wallIndex].curve.Evaluate(x_perc + 1.0f / divisions.x), allVertices[allVertices.Count - 1].z);
                            }
                        }
                        else if (v_z * divisions.y > 0 && v_x * divisions.x > columnIndex * divisions.x) //D
                        {
                            debug_info += "D";
                            Debug.Log("D");
                            //Connect quad diagonally up to the left to surrounding quads
                            if (tiles[doorGridPosition].endVertices.Count < instructions[wallIndex].height * divisions.y * 4 || vertexIndex % (divisions.x) < divisions.x - 1)
                            {
                                allVertices.Add(allVertices[((3 + vertices_per_quad) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(3 + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(((4 * (int)divisions.x) + vertices_per_quad - 1) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)), y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness), ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) - firstQuad_leftVal_z) - tileIndex));
                                saveEnd = true;
                                indicesToRotate = new int[] { 3 };
                            }
                            else
                            {
                                Debug.Log("Entered here with D!");
                                allVertices.Add(tiles[doorGridPosition].endVertices[1 + 4 * (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                allVertices.Add(allVertices[(3 + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(((4 * (int)divisions.x) + vertices_per_quad - 1) + skip_up + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(tiles[doorGridPosition].endVertices[2 + 4 * (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                            }
                            rotateAround = instructions[wallIndex].position;

                            if (instructions[wallIndex].curve != null && instructions[wallIndex].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count - 1] = new Vector3(allVertices[allVertices.Count - 1].x, allVertices[allVertices.Count - 1].y - instructions[wallIndex].curve.Evaluate(x_perc + 1.0f / divisions.x), allVertices[allVertices.Count - 1].z);
                            }
                        }
                        else if (v_x * divisions.x > columnIndex * divisions.x) //B
                        {
                            debug_info += "B";
                            Debug.Log("B");
                            //Connect quad to the left to quad to the right
                            if (tiles[doorGridPosition].endVertices.Count < instructions[wallIndex].height * divisions.y * 4 || vertexIndex % (divisions.x) < divisions.x - 1)
                            {
                                allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)), y + UnityEngine.Random.Range(-jaggedness, 0), ((z - v_z)) - tileIndex));
                                allVertices.Add(allVertices[(0 + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(3 + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)), y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness), ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) - firstQuad_leftVal_z) - tileIndex));
                                saveEnd = true;
                                indicesToRotate = new int[] { 0, 3 };
                            }
                            else
                            {
                                allVertices.Add(tiles[doorGridPosition].endVertices[1 + 4 * (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                allVertices.Add(allVertices[(0 + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(allVertices[(3 + skip_left) + jump_wall + tileIndex * vertices_per_tile + columnIndex * vertices_per_column]);
                                allVertices.Add(tiles[doorGridPosition].endVertices[2 + 4 * (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                            }
                            rotateAround = instructions[wallIndex].position;

                            if (instructions[wallIndex].curve != null && instructions[wallIndex].curve.keys.Length > 1)
                            {
                                allVertices[allVertices.Count - 4] = new Vector3(allVertices[allVertices.Count - 4].x, allVertices[allVertices.Count - 4].y - instructions[wallIndex].curve.Evaluate(x_perc + 1.0f / divisions.x), allVertices[allVertices.Count - 4].z);
                                allVertices[allVertices.Count - 1] = new Vector3(allVertices[allVertices.Count - 1].x, allVertices[allVertices.Count - 1].y - instructions[wallIndex].curve.Evaluate(x_perc + 1.0f / divisions.x), allVertices[allVertices.Count - 1].z);
                            }
                        }
                        else //A (is the first square of this column)
                        {
                            int jumpValue = columnIndex > 0 ? ((columnIndex - 1) * vertices_per_column) + jump_wall: jump_wall - vertices_per_column;
                            float localRotA = columnIndex == 0 ? rot_a : 0;
                            float localRotB = columnIndex == 0 ? rot_b : 0;
                            float localRotC = columnIndex == 0 ? rot_c : 0;
                            //If this is not the first column, then jump a certain amount of columns
                            debug_info += "A";
                            //I neede it to be "jump_wall - vertices_per_column", because, jump_wall assumes you're on the current column. Here it connects to the previous"
                            allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + localRotA, y + UnityEngine.Random.Range(-jaggedness, 0) + localRotC, ((z - v_z)) - tileIndex));
                            if((wallIndex > 0 && columnIndex == 0) || columnIndex > 0)
                            {
                                allVertices.Add(allVertices[(0 + vertices_per_quad * ((int)divisions.x - 1)) + jumpValue + tileIndex * vertices_per_tile]);
                                allVertices.Add(allVertices[(3 + vertices_per_quad * ((int)divisions.x - 1)) + jumpValue + tileIndex * vertices_per_tile]);
                                indicesToRotate = new int[] { 0, 3 };
                                verticesToApplyCurveTo = new int[] { 1, 4 };
                                rotateAround = instructions[wallIndex].position;
                            }
                            else
                            {
                                indicesToRotate = new int[] { 0, 1, 2, 3 };
                                verticesToApplyCurveTo = new int[] { 1, 2, 3, 4 };
                                rotateAround = new Vector3(x, y, z);
                                if (tiles[upperFloorGridPosition].startVertices.Count < instructions[wallIndex].height * divisions.y * 4)
                                {

                                    allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + localRotB, y + UnityEngine.Random.Range(-jaggedness, 0) + localRotC, (z - v_z) - tileIndex));
                                    allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + localRotB, y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + localRotC, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) - firstQuad_leftVal_z) - tileIndex));
                                    saveStart = true;
                                }
                                else
                                {
                                    allVertices.Add(tiles[upperFloorGridPosition].startVertices[1 + (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                    allVertices.Add(tiles[upperFloorGridPosition].startVertices[2 + (int)(tileIndex * divisions.y + vertexIndex / divisions.x)]);
                                }
                            }
                            allVertices.Add(new Vector3((x + v_x + UnityEngine.Random.Range(-jaggedness, jaggedness)) + localRotA, y + current_tilt_increment + UnityEngine.Random.Range(-jaggedness, jaggedness) + localRotC, ((z - v_z - UnityEngine.Random.Range(-jaggedness, jaggedness)) - firstQuad_leftVal_z) - tileIndex));
                            if (instructions[wallIndex].curve != null && instructions[wallIndex].curve.keys.Length > 1)
                            {
                                for (int i = 0; i < verticesToApplyCurveTo.Count(); i++)
                                {
                                    allVertices[allVertices.Count - verticesToApplyCurveTo[i]] = new Vector3(
                                        allVertices[allVertices.Count - verticesToApplyCurveTo[i]].x,
                                        allVertices[allVertices.Count - verticesToApplyCurveTo[i]].y - instructions[wallIndex].curve.Evaluate(x_perc + 1.0f / divisions.x),
                                        allVertices[allVertices.Count - verticesToApplyCurveTo[i]].z);
                                }
                            }
                        }

                        CreateWall_RoundColumn(ref allVertices, roundLastColumn, roundFirstColumn, ref indicesToRotate, instructions, wallIndex, columnIndex, x, y, divisions.ToV2Int());
                        CreateWall_Rotate(allVertices, indicesToRotate.ToList(), rotateAround, instructions[wallIndex].rotation);

                        if (saveStart)
                        {
                            for (int m = 4; m > 0; m--)
                            {
                                tiles[doorGridPosition].startVertices.Add(allVertices[allVertices.Count - m]);
                            }
                            //Debug.Log("Saving start positions at: " + doorGridPosition);
                        }
                        /*if(saveEnd)
                        {
                            for(int m = 4; m > 0; m--)
                            {
                                tiles[upperFloorGridPosition].endVertices.Add(allVertices[allVertices.Count-m]);
                            }
                        }*/

                        //! determine if this is the bottom of the wall and save vertices 0 and 1 
                        if (tileIndex == 0 && (int)(vertexIndex / divisions.x) == 0)
                        {
                            tiles[upperFloorGridPosition].floorVertices.Add(allVertices[allVertices.Count - 4] - instructions[wallIndex].movePosition);
                            tiles[upperFloorGridPosition].floorVertices.Add(allVertices[allVertices.Count - 3] - instructions[wallIndex].movePosition);
                            //! get the position on the grid that this wall corresponds to
                        }
                        //! determine if this is the top of the wall and save vertices 2 and 3
                        if (tileIndex == instructions[wallIndex].height - 1 && (int)(vertexIndex / divisions.x) == instructions[wallIndex].height - 1 && !tiles[upperFloorGridPosition].wall)
                        {
                            tiles[upperFloorGridPosition].ceilingVertices.Add(allVertices[allVertices.Count - 2] - instructions[wallIndex].movePosition);
                            tiles[upperFloorGridPosition].ceilingVertices.Add(allVertices[allVertices.Count - 1] - instructions[wallIndex].movePosition);

                            for (int n = 0; n < tiles[upperFloorGridPosition].sidesWhereThereIsWall.Count; n++)
                            {
                                if (tiles[upperFloorGridPosition].sidesWhereThereIsWall[n].side == (upperFloorGridPosition - currentGridPosition))
                                {
                                    tiles[upperFloorGridPosition].sidesWhereThereIsWall[n].floor = false;
                                }
                            }
                            //! get the position on the grid that this wall corresponds to
                        }

                        int[] indexValue = new int[] { 0, 1, 3, 1, 2, 3 };
                        for (int index = 0; index < indexValue.Length; index++) { newIndices.Add(indexValue[index] + jump_quad_up + jump_tile); }

                        newUV.Add(new Vector2(v_x - columnIndex + 1.0f / divisions.x, v_z));                      //1,0
                        newUV.Add(new Vector2(v_x - columnIndex, v_z));                      //0,0
                        newUV.Add(new Vector2(v_x - columnIndex, v_z + 1.0f / divisions.y)); //0,1
                        newUV.Add(new Vector2(v_x - columnIndex + 1.0f / divisions.x, v_z + 1.0f / divisions.y)); //1,1

                       // Debug.Log("So far: " + debug_info);
                    }
                    debug_info += "_";
                    // Debug.Log("So far: " + debug_info);
                }

                GameObject wallObject = new GameObject("Wall Object " + columnIndex);
                wallObject.transform.parent = wall.transform;

                Vector3[] newVertices = new Vector3[] { };
                Array.Resize(ref newVertices, vertices_per_column);
                allVertices.CopyTo(allVertices.Count - vertices_per_column, newVertices, 0, vertices_per_column);

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

                if (Math.Mod(instructions[wallIndex].rotation, 360) == 0)
                {
                    currentGridPosition += new Vector2Int(1, 0);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                    doorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                }
                else if (Math.Mod(instructions[wallIndex].rotation, 360) == 90)
                {
                    currentGridPosition += new Vector2Int(0, -1);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(-1, 0);
                    doorGridPosition = currentGridPosition + new Vector2Int(1, 0);
                }
                else if (Math.Mod(instructions[wallIndex].rotation, 360) == 180)
                {
                    currentGridPosition += new Vector2Int(-1, 0);
                    upperFloorGridPosition = currentGridPosition + new Vector2Int(0, 1);
                    doorGridPosition = currentGridPosition + new Vector2Int(0, -1);
                }
                else if (Math.Mod(instructions[wallIndex].rotation, 360) == 270)
                {
                    currentGridPosition += new Vector2Int(0, 1);
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
    static public void CreateWall_RoundColumn(ref List<Vector3> allVertices, bool roundLastColumn, bool roundFirstColumn, ref int [] indicesToRotate, List<WallData> instructions, int i, int j, float x, float y, Vector2Int divisions)
    {
        if (roundLastColumn)
        {
            for (int index = 0; index < indicesToRotate.Length; index++)
            {
                //3 = -1
                //0 = -4
                //so its indicesToRotate[i] - 4

                Vector2 circleCenter = new Vector2(0, instructions[i].roundedness); //! Center in a vaccuum
                circleCenter += new Vector2(x + j - 1.0f / divisions.x, y); //! Make Center relative to this wall tile
                                                                            //Debug.Log("Center of circle is: " + circleCenter);
                                                                            //Debug.Log("While x and y is: " + new Vector2(x + j,y));
                                                                            //Debug.Log("Position towards: " + (Vector2)newVertices[newVertices.Count +(indicesToRotate[index] - 4)]);

                Vector2 vectorBetween = (circleCenter - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]);
                Vector2 normal = vectorBetween.normalized; //! Take the normal from the Center to this position
                Vector2 movedNormal = new Vector2(x + j - 1.0f / divisions.x, y) - normal;
                // Debug.Log("Normal: " + normal);
                Vector2 thirdPosition = new Vector2(movedNormal.x, allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y); //! The third position to the normal and the original vertex

                float move_x = (thirdPosition - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]).magnitude;
                float move_y = ((movedNormal - thirdPosition)).magnitude;

                // Debug.Log("INDEX: " + (newVertices.Count +(indicesToRotate[index] - 4))); 

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
                //3 = -1
                //0 = -4
                //so its indicesToRotate[i] - 4
                //float x_perc_here = (index == 1 || index == 2)? x_perc: x_perc + 1.0f/divisions.x;

                Vector2 circleCenter = new Vector2(instructions[i - 1].roundedness, instructions[i - 1].roundedness); //! Center in a vaccuum
                circleCenter += new Vector2(x - 1.0f / divisions.x, y - 1.0f / divisions.x); //! Make Center relative to this wall tile
                                                                                             //Debug.Log("Center of circle is: " + circleCenter);
                                                                                             //Debug.Log("While x and y is: " + new Vector2(x,y));
                                                                                             //Debug.Log("Position towards: " + (Vector2)newVertices[newVertices.Count +(indicesToRotate[index] - 4)]);

                Vector2 vectorBetween = (circleCenter - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]);
                Vector2 normal = vectorBetween.normalized; //! Take the normal from the Center to this position
                Vector2 movedNormal = new Vector2(x - 1.0f / divisions.x, y - 1.0f / divisions.x) - normal;
                //Debug.Log("Normal: " + normal);
                Vector2 thirdPosition = new Vector2(movedNormal.x, allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y); //! The third position to the normal and the original vertex

                float move_x = (thirdPosition - (Vector2)allVertices[allVertices.Count + (indicesToRotate[index] - 4)]).magnitude;
                float move_y = ((movedNormal - thirdPosition)).magnitude;

                //Debug.Log("INDEX: " + (newVertices.Count +(indicesToRotate[index] - 4))); 

                allVertices[allVertices.Count + (indicesToRotate[index] - 4)] =
                    new Vector3(
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].x - move_x + 1,
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].y - move_y + 1 + 1.0f / divisions.x,
                        allVertices[allVertices.Count + (indicesToRotate[index] - 4)].z);
            }
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