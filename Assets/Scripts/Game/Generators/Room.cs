using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Rendering;

[System.Serializable]public class RoomData
{
    public RoomType m_type = RoomType.NormalRoom;
    public RoomPosition m_roomPosition = RoomPosition.None;
    public int stepsAwayFromMainRoom = 0;
    public bool IsBuilt = false;
}
public enum RoomType
{
    NormalRoom = 0,
    AmbushRoom = 1,
    TreasureRoom = 2, //without puzzle
    PuzzleRoom = 3, //Solve puzzle to get treasure
    BossRoom = 4,
    MiniBossRoom = 5,
    RestingRoom = 6 //Room where enemies cant spawn, and where you can set up a tent. Sometimes theres a merchant here
}

public enum RoomPosition
{
    None = 0,
    DeadEnd = 1
}

//Core code
public partial class Room: MonoBehaviour
{
    [System.Serializable]public class Entrances
    {
        //This class holds and handles all entrances to a room
        [System.Serializable]public class Entrance
        {
            public enum EntranceType
            {
                NormalDoor = 0,
                PuzzleDoor = 1,
                BombableWall = 2,
                LockedDoor = 3,
                MultiLockedDoor = 4, //Uses more than one key
                AmbushDoor = 5 //Locks behind you, defeat all enemies to make them open
            }
            public bool open;
            public bool spawned;

            public List<Vector2Int> positions; //One vector for each position it is on
            public Vector2Int gridPos; //The room that the door is in
            public Vector2Int dir; //The direction this door is pointing

            public Vector2 index;
            EntranceType type;

            public Entrance(Vector2Int gridPos_in, Vector2Int dir_in)
            {
                gridPos = gridPos_in; dir = dir_in;
                index = new Vector2(9,10); //This is the default
                open = false;
                spawned = false;
                positions = new List<Vector2Int>();
                type = EntranceType.NormalDoor;
            }

            public EntranceType GetEntranceType()
            {
                return type;
            }
            public void SetEntranceType(EntranceType type_in)
            {
                type = type_in;
            }
            public void SetOpen(bool value)
            {
                open = value;
            }
            public void Activate()
            {
                open = true;
                spawned = true;
            }

            public void Deactivate()
            {
                open = false;
                spawned = false;
            }
            public void Close() //when linking, in case you cant link rooms and got to acknowledge that theres a spawned room
            {
                open = false;
                spawned = true;
            }
        }
        public List<Entrance> entrances = new List<Entrance>();

        public Entrances(Vector2Int gridPosition, Vector2Int roomSize) //in gridspace, so a 40x40 is 2x2
        {
            for(int x = 0; x < roomSize.x; x++) //Adding north and south entrances
            {
                entrances.Add(new Entrance(gridPosition + new Vector2Int(x,0), new Vector2Int(0,1))); //North
                entrances[entrances.Count-1].positions.Add(new Vector2Int(9 + x * 20, 0));
                entrances[entrances.Count-1].positions.Add(new Vector2Int(10 + x * 20, 0));

                entrances.Add(new Entrance(gridPosition + new Vector2Int(x,-(roomSize.y - 1)), new Vector2Int(0,-1))); //South
                entrances[entrances.Count-1].positions.Add(new Vector2Int(10 + x * 20, roomSize.y * 20 - 1));
                entrances[entrances.Count-1].positions.Add(new Vector2Int(9 + x * 20, roomSize.y * 20 - 1));
            }
            for(int y = 0; y < roomSize.y; y++) //Adding left and right entrances
            {
                entrances.Add(new Entrance(gridPosition + new Vector2Int(roomSize.x - 1,-y), new Vector2Int(1,0))); //Right
                entrances[entrances.Count-1].positions.Add(new Vector2Int(roomSize.x * 20 - 1, 9 + y * 20));
                entrances[entrances.Count-1].positions.Add(new Vector2Int(roomSize.x * 20 - 1, 10 + y * 20));
                entrances.Add(new Entrance(gridPosition + new Vector2Int(0,-y), new Vector2Int(-1,0))); //Left
                entrances[entrances.Count-1].positions.Add(new Vector2Int(0, 10 + y * 20)); 
                entrances[entrances.Count-1].positions.Add(new Vector2Int(0, 9 + y * 20));
            }
        }
        public void OpenAllEntrances()
        {
            for(int i = 0; i < entrances.Count; i++)
            {
                entrances[i].SetOpen(true);
            }
        }
        public Tuple<bool, Entrance> GetEntrance(Vector2Int gridPosition, Vector2Int direction)
        {
            //Debug.Log("Grid pos of this room: " + gridPosition + " looking for this direction: " + direction);
            for(int i = 0; i < entrances.Count; i++)
            {
                //Debug.Log("Found an entrance with grid pos: " + entrances[i].gridPos + " and direction: " + entrances[i].dir);
                if(entrances[i].gridPos == gridPosition && entrances[i].dir == direction)
                {
                    return new Tuple<bool, Entrance>(true, entrances[i]);
                }
            }
            return new Tuple<bool, Entrance>(false, new Entrance(Vector2Int.zero, Vector2Int.zero));
        }
        public void SetEntranceVertices(ref RoomTemplate template, RoomTemplate originTemplate, Entrance entrance, Entrance originEntrance) //The template of this room and the template from the other room
        {
            template.SetEntranceTileVertices(originTemplate.GetEntranceTile(originEntrance.positions[1]).endVertices, entrance.positions[0], true); //Origin end to destination start
            template.SetEntranceTileVertices(originTemplate.GetEntranceTile(originEntrance.positions[0]).startVertices, entrance.positions[1], false); //Origin start to destination end
        }
    }
    public class EntranceData
    {
        public List<Vector3> leftVertices = new List<Vector3>(); //From inside the room looking towards north door. Left vertices are saved at the end of the left wall, right vertices are saved at the beginning of the right wall;
        public List<Vector3> rightVertices = new List<Vector3>();
    }
    public List<EntranceData> entrances = new List<EntranceData>(); //Save vertices for every single door

    [System.Serializable]public struct RoomDebug 
    {
        public enum RoomBuildMode
        {
            NONE,
            CHECK_TEMPLATE,

            BOTH
        }
        public RoomBuildMode CheckTemplate;
        public Color floorColor;
        public Color wallColor;
    }
    public class RoomTemplate
    {
        //This class is given to the CreateWalls in order to draw the meshes for walls
        //It is also given to the CreateFloor in order to draw the floor
        public class TileTemplate
        {
            public int identity; //0 for void, 1 for wall, 2 for floor, 3 for door
            public bool read;

            public Vector2Int divisions; //This also only does something if the identity is a wall
            //If divide into multiple parts, like, three by three quads on one wall tile on outdoor walls for instance. Usually, on indoor walls, its completely flat

            public List<Vector3> endVertices = new List<Vector3>(); //When wall ends, and this list is empty, save all vertices in here otherwise use
            public List<Vector3> startVertices = new List<Vector3>(); //If this is empty when wall starts, fill it up. Otherwise use

            public TileTemplate(int identity_in, Vector2Int divisions_in)
            {
                identity = identity_in;
                read = false;
                divisions = divisions_in;
            }
            public void SetIdentity(int newIdentity)
            {
                identity = newIdentity;
            }
            public void SetRead(bool value)
            {
                read = value;
            }
        }
        public Vector2Int size;
        public List<TileTemplate> positions;
        public bool indoors;
        public RoomTemplate(Vector2Int size_in, List<TileTemplate> positions_in, bool indoors_in)
        {
            size = size_in;
            positions = positions_in;
            indoors = indoors_in;
            CreateRoomTemplate();
        }
        void CreateRoomTemplate()
        {
            //In here it will be determined if the room is a circle, if it is a corridor, etc
            Vector2 roomCenter = new Vector2(size.x / 2, size.y / 2);
            Vector2 wallThickness = new Vector2(UnityEngine.Random.Range(size.x/2 - 4, size.x/2), UnityEngine.Random.Range(size.y/2 - 4, size.y/2));

            for(int y = 0; y < size.y; y++)
            {
                for(int x = 0; x < size.x; x++)
                {
                    Vector2Int divisions = new Vector2Int(1,1); //1,1
                    if(!indoors){divisions = new Vector2Int(3,3);}
                    positions.Add(new RoomTemplate.TileTemplate(2, divisions));

                    CreateRoomTemplate_Square(new Vector2(2,2), x, y); //?Basic thickness. Can't be thinner than 2
                    if(!indoors){CreateRoomTemplate_Circle(roomCenter, wallThickness, x, y);}
                    //CreateRoomTemplate_Cross(wallThickness, x, y);
                }
            }
        }
        void CreateRoomTemplate_Circle(Vector2 center, Vector2 wallThickness, int x, int y)
        {
            float distanceToCenter = new Vector2(x+0.5f - center.x, y+0.5f - center.y).magnitude;
            if (distanceToCenter > wallThickness.x && distanceToCenter > wallThickness.y)
            {
                //if the higher limit is 0, then the code just generates a circle, period
                int temp = UnityEngine.Random.Range(0, 2);
                if (temp == 0)
                {
                    positions[x + (int)size.x * y].identity = 1;
                }
            }
        }
        void CreateRoomTemplate_Square(Vector2 wallThickness, int x, int y)
        {
            if(x < wallThickness.x || x > size.x - wallThickness.x - 1||
               y < wallThickness.y || y > size.y - wallThickness.y - 1)
            {
                positions[x + (int)size.x * y].identity = 1;
            }
        }
        void CreateRoomTemplate_Cross(Vector2 wallThickness, int x, int y)
        {
            if((x < wallThickness.x || x > size.x - wallThickness.x -1)&&
               (y < wallThickness.y || y > size.y - wallThickness.y -1))
            {
                positions[x + (int)size.x * y].identity = 1;
            }
        }

        void UnscatterWalls()
        {
            //First, removes stray positions
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    bool deleteWall = true;
                    for(int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            if((i == 0 && j == 0) || i != 0 && j != 0){continue;} //Only check directly up or directly down
                            if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if(!(positions[x + i + (int)size.x * (y + -j)].identity == 2)) //If the position is not a floor
                                {
                                    deleteWall = false; //Then dont delete
                                }
                            }
                        }
                    }
                    if(deleteWall)
                    {
                        positions[x + (int)size.x * y].identity = 2;
                    }
                }
            }
            //Second, fill in holes
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int amountOfWallNeighbors = 0;
                    for(int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            if(i == 0 && j == 0){continue;}
                            if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if(positions[x + i + (int)size.x * (y + -j)].identity == 1) //If the position is a wall
                                {
                                    amountOfWallNeighbors++; //Then count up
                                }
                            }
                        }
                    }
                    if(amountOfWallNeighbors > 5)
                    {
                        positions[x + (int)size.x * y].identity = 1;
                    }
                }
            }
            //Shave off excess wall
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int amountOfFloorNeighbors = 0;
                    for(int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            if(i == 0 && j == 0){continue;}
                            if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if(positions[x + i + (int)size.x * (y + -j)].identity == 2) //If the position is a floor
                                {
                                    amountOfFloorNeighbors++; //Then count up
                                }
                            }
                        }
                    }
                    if(amountOfFloorNeighbors > 5)
                    {
                        positions[x + (int)size.x * y].identity = 2;
                    }
                }
            }
            //Remove singular wall pieces with floor on opposite sides, because they be causing boogs
            //Also the pesky 1 x 2 bois, who also cause boogs
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    if(IsPositionWithinBounds(new Vector2Int(x - 1, -y)) && positions[x - 1 + size.x * y].identity == 2)
                    {
                        if(IsPositionWithinBounds(new Vector2Int(x + 1, -y)) && positions[x + 1 + size.x * y].identity == 2 )
                        {
                            positions[x + (int)size.x * y].identity = 2;
                        }
                        else if(IsPositionWithinBounds(new Vector2Int(x + 2, -y)) && positions[x + 2 + size.x * y].identity == 2)
                        {
                            positions[x + (int)size.x * y].identity = 2;
                            positions[x + 1 + (int)size.x * y].identity = 2;
                        }
                    }
                    if(IsPositionWithinBounds(new Vector2Int(x, -y - 1)) && positions[x + size.x * (y+1)].identity == 2)
                    {
                        if(IsPositionWithinBounds(new Vector2Int(x, -y + 1)) && positions[x + size.x * (y-1)].identity == 2)
                        {
                            positions[x + (int)size.x * y].identity = 2;
                        }
                        else if(IsPositionWithinBounds(new Vector2Int(x, -y + 2)) && positions[x + size.x * (y - 2)].identity == 2)
                        {
                            positions[x + (int)size.x * y].identity = 2;
                            positions[x + (int)size.x * (y-1)].identity = 2;
                        }
                    }
                }
            }
            //Remove walls with two opposite diagonal floors but the rest is wall, because they cause bugs since they try to fuse together a chunk with a wall and fail
            for(int x = 0; x < size.x;x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    if(IsPositionWithinBounds(new Vector2Int(x - 1, -y - 1)) && positions[x - 1 + size.x * (y +1)].identity == 2)
                    {
                        if(IsPositionWithinBounds(new Vector2Int(x + 1, -y +1)) && positions[x + 1 + size.x * (y -1)].identity == 2 )
                        {
                            positions[x + (int)size.x * y].identity = 2;
                        }
                    }
                    if(IsPositionWithinBounds(new Vector2Int(x +1, -y - 1)) && positions[x +1 + size.x * (y+1)].identity == 2)
                    {
                        if(IsPositionWithinBounds(new Vector2Int(x-1, -y + 1)) && positions[x-1 + size.x * (y-1)].identity == 2)
                        {
                            positions[x + (int)size.x * y].identity = 2;
                        }
                    }
                }
            }
        }
        void WeedOutUnseeableWalls()
        {
            //Cleans out walls so they become void
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    bool isVoid = true;
                    for(int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            if(i == 0 && j == 0){continue;}
                            if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if(!(positions[x + i + (int)size.x * (y + -j)].identity == 1 || positions[x + i + (int)size.x * (y + -j)].identity == 0))
                                {
                                    isVoid = false;
                                }
                            }
                        }
                    }
                    if(isVoid)
                    {
                        positions[x + (int)size.x * y].identity = 0;
                    }
                }
            }
        }

        public bool IsPositionWithinBounds(Vector2Int pos)
        {
            if(pos.x >= 0 && pos.x < size.x && pos.y <= 0 && pos.y > -size.y )
            {
                return true;
            }
           // Debug.Log(pos + " <color=red>is not within bounds</color>");
            return false;
        }
        public Tuple<bool, Vector2Int, int> HasWallNeighbor(Vector2Int pos, int rotation)
        {
            //Debug.Log("Checking if position: " + pos + "Has any free neighbors");
            //Debug.Log("Rotation: " + rotation);
            Vector2Int direction = Vector2Int.zero;
            bool value = true;
            int rotationDir = 0;
            if(rotation == 270)
            {
                if(IsPositionWithinBounds(new Vector2Int(pos.x + 1, pos.y)) && positions[pos.x + 1 + size.x * -pos.y].identity == 1 && !positions[pos.x + 1 + size.x * -pos.y].read)
                {
                    direction = new Vector2Int(1, 0);
                    rotationDir = 1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x - 1, pos.y)) && positions[pos.x - 1 + size.x * -pos.y].identity == 1 && !positions[pos.x - 1 + size.x * -pos.y].read)
                {
                    direction = new Vector2Int(-1, 0);
                    rotationDir = -1;
                }
                else
                {
                    value = false;
                }
            }
            else if(rotation == 90)
            {
                if(IsPositionWithinBounds(new Vector2Int(pos.x + 1, pos.y)) && positions[pos.x + 1 + size.x * -pos.y].identity == 1 && !positions[pos.x + 1 + size.x * -pos.y].read)
                {
                    direction = new Vector2Int(1, 0);
                    rotationDir = -1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x - 1, pos.y)) && positions[pos.x - 1 + size.x * -pos.y].identity == 1 && !positions[pos.x - 1 + size.x * -pos.y].read)
                {
                    direction = new Vector2Int(-1, 0);
                    rotationDir = 1;
                }
                else
                {
                    value = false;
                }
            }
            else if(rotation == 180)
            {
                if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y + 1)) && positions[pos.x + size.x * (-pos.y - 1)].identity == 1 && !positions[pos.x + size.x * (-pos.y - 1)].read)
                {
                    direction = new Vector2Int(0, -1);
                    rotationDir = 1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y - 1)) && positions[pos.x + size.x * (-pos.y + 1)].identity == 1 && !positions[pos.x + size.x * (-pos.y + 1)].read)
                {
                    direction = new Vector2Int(0, 1);
                    rotationDir = -1;
                }
                else
                {
                    value = false;
                }
            }
            else if(rotation == 0)
            {
                if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y + 1)) && positions[pos.x + size.x * (-pos.y - 1)].identity == 1 && !positions[pos.x + size.x * (-pos.y - 1)].read)
                {
                    direction = new Vector2Int(0, -1);
                    rotationDir = -1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y - 1)) && positions[pos.x + size.x * (-pos.y + 1)].identity == 1 && !positions[pos.x + size.x * (-pos.y + 1)].read)
                {
                    direction = new Vector2Int(0, 1);
                    rotationDir = 1;
                }
                else
                {
                    value = false;
                }
            }
            //Debug.Log("It has a neighbor in this direction: " + direction);
            return new Tuple<bool, Vector2Int, int>(value, direction, rotationDir);
        }
        
        public List<Tuple<List<MeshMaker.WallData>, bool>> ExtractWalls(Entrances entrances)
        {
           // Debug.Log("<color=green> NEW ROOM</color>");
            List<Tuple<List<MeshMaker.WallData>, bool>> data = new List<Tuple<List<MeshMaker.WallData>, bool>>();

            if(entrances != null && entrances.entrances.Count > 0)
            {
              //  Debug.Log("<color=green> NEW WALL</color>");
                for(int i = 0; i < entrances.entrances.Count; i++) //! makes the code only work when theres a door
                {
                    if(!entrances.entrances[i].open || !entrances.entrances[i].spawned){continue;}
                    //Go through each entrance, and make a wall to its left. There will only ever be as many walls as there are entrances
                    //Find a wall that has a floor next to it
                    //Debug.Log("Extracting walls");
                    Vector2Int pos = new Vector2Int(-1,-1);
                    int currentAngle = 0; //Current angle should only be 0 if the floor found points down.

                    ExtractWalls_GetStartPosition(ref pos, ref currentAngle, entrances.entrances[i]);

                    //Now we should have a piece of a wall we can start from
                    //!Now, follow the walls and create new WallData everytime it turns once

                    //Find direction to follow
                    Tuple<List<MeshMaker.WallData>, bool> wall = new Tuple<List<MeshMaker.WallData>, bool>(new List<MeshMaker.WallData>(), false);

                    Tuple<bool, Vector2Int, int> returnData = HasWallNeighbor(pos, currentAngle); //Item2 is the direction to go to
                    //Debug.Log("Did I find wall neighbor? " + returnData.Item1);
                    //Debug.Log("<color=yellow>"+currentAngle+"</color>");
                    //Debug.Log("<color=yellow>"+returnData.Item3+"</color>");
                    currentAngle += 90 * returnData.Item3;
                    currentAngle = (int)Math.Mod(currentAngle, 360);
                    //Debug.Log("<color=yellow>"+currentAngle+"</color>");
                    positions[pos.x + size.x * -pos.y].SetRead(true);
                    Vector2Int startPosition = pos;

                    while(returnData.Item1) //If there is a wall neighbor, proceed
                    {
                        startPosition = pos;
                        //Follow that direction until its empty
                        int steps = 1;
                    //Debug.Log("Checking index: " + (pos.x + returnData.Item2.x + size.x * (-pos.y + returnData.Item2.y)));
                        ExtractWalls_GetSteps(ref pos, ref steps, returnData.Item2, currentAngle);
                        
                        int isThisWallFollowingOuterCorner = 0;
                        if(returnData.Item3 < 0 && wall.Item1.Count > 0)
                        {
                            //If lastWall is less than 0, then this is the following wall after an outer corner, so it must be moved up and shortened
                            isThisWallFollowingOuterCorner = 1;
                        }
                        returnData = HasWallNeighbor(pos, currentAngle);
                        int isThisWallEndingWithOuterCorner = 0;

                        float roundedness = indoors ? 0 : UnityEngine.Random.Range(0.0f, 1.0f);

                        if(returnData.Item3 < 0)
                        {
                            //If Item3 is less than 0, then this is an outer corner, so the wall shouldn't go the whole way
                            isThisWallEndingWithOuterCorner = 1;
                            if(!indoors)
                            {
                                roundedness = 1.0f;
                            }
                        }
                        else
                        {
                            roundedness = 0;
                        }
                        
                        //Debug.Log("<color=green>Amount of steps: </color>" + steps + " angle: " + currentAngle);
                        //Only set wrap to true if the last wall ends up adjacent to the first wall

                        //! make one wall curvy but none other
                        AnimationCurve curve = new AnimationCurve();
                        /*Keyframe temp = new Keyframe();
                        temp.time = 0;
                        temp.value = 0;
                        temp.inTangent = 1;
                        curve.AddKey(temp);

                        temp.time = 0.5f;
                        temp.value = 0.25f;
                        temp.inTangent = 0;
                        temp.outTangent = 0;

                        curve.AddKey(temp);

                        temp.time = 1;
                        temp.value = 0;
                        curve.AddKey(temp);*/
                        //! END OF CURVE

                        float divisionModifier = 0; //Apparently different divisions try to put the walls at different places, annoyingly

                        if( positions[pos.x + size.x * -pos.y].divisions.x > 1)
                        {
                            divisionModifier = 1.0f / (((float)positions[pos.x + size.x * -pos.y].divisions.x+1) / ((float)positions[pos.x + size.x * -pos.y].divisions.x - 1)); //This perfectly describes where each wall will stand, based on amount of division
                        }
                        

                        if(currentAngle == 0)
                        {
                            //Debug.Log("adding 0 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - divisionModifier + isThisWallFollowingOuterCorner,startPosition.y,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                        }
                        if(currentAngle == 90)
                        {
                            //Debug.Log("adding 90 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f, startPosition.y - 0.5f + divisionModifier - isThisWallFollowingOuterCorner,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                        }
                        if(currentAngle == 180)
                        {
                            //Debug.Log("adding 180 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + divisionModifier,startPosition.y - 1,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                        }
                        if(currentAngle == 270)
                        {
                            //Debug.Log("adding 270 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f,startPosition.y - 0.5f - divisionModifier + isThisWallFollowingOuterCorner ,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness)); // y - 0.5f
                        }
                        //Sometimes it has to decrease by 90, so it has to know what direction the next wall goes in (fuck)
                        currentAngle += 90 * returnData.Item3; //This code can only do inner corners atm, not outer corners
                        currentAngle = (int)Math.Mod(currentAngle, 360);
                    }
                   // Debug.Log("There is this amount of walls: " + wall.Item1.Count);
                    wall = new Tuple<List<MeshMaker.WallData>, bool>(wall.Item1, ExtractWalls_DoesWallWrap(wall.Item1));

                    data.Add(wall);
                }
            }
            else //If this is a closed room without doors
            {
                Debug.Log("Making a closed room");
                Vector2Int pos = new Vector2Int(-1,-1);
                int currentAngle = 0; //Current angle should only be 0 if the floor found points down.

                for(int x = 0; x < size.x; x++)
                {
                    for(int y = 0; y < size.y; y++)
                    {
                        //Check if there is floor diagonally right down, because walls can only be drawn from left to right
                        //If there are none, rotate the search three times. If there still are none, then there is an error
                        if(IsPositionWithinBounds(new Vector2Int(x + 1, -y - 1)) && x < size.x && positions[x + 1 + size.x * (y + 1)].identity == 2)
                        {
                            pos = new Vector2Int(x, -y); 
                            currentAngle = 90;
                            break; 
                        }
                    }
                    if(pos != new Vector2Int(-1,-1))
                    {
                        break;
                    }
                }

                //Now we should have a piece of a wall we can start from
                //!Now, follow the walls and create new WallData everytime it turns once

                //Find direction to follow
                Tuple<List<MeshMaker.WallData>, bool> wall = new Tuple<List<MeshMaker.WallData>, bool>(new List<MeshMaker.WallData>(), false);

                Tuple<bool, Vector2Int, int> returnData = HasWallNeighbor(pos, currentAngle); //Item2 is the direction to go to
                //Debug.Log("<color=yellow>"+currentAngle+"</color>");
                //Debug.Log("<color=yellow>"+returnData.Item3+"</color>");
                currentAngle += 90 * returnData.Item3;
                currentAngle = (int)Math.Mod(currentAngle, 360);
                //Debug.Log("<color=yellow>"+currentAngle+"</color>");
                positions[pos.x + size.x * -pos.y].SetRead(true);
                Vector2Int startPosition = pos;

                while(returnData.Item1) //If there is a wall neighbor, proceed
                {
                    startPosition = pos;
                    //Follow that direction until its empty
                    int steps = 1;
                //Debug.Log("Checking index: " + (pos.x + returnData.Item2.x + size.x * (-pos.y + returnData.Item2.y)));
                    while(IsPositionWithinBounds(new Vector2Int(pos.x + returnData.Item2.x, pos.y - returnData.Item2.y)) && positions[pos.x + returnData.Item2.x + size.x * (-pos.y + returnData.Item2.y)].identity == 1) //While the position in the next direction is a wall
                    {
                        steps++;
                        pos = new Vector2Int(pos.x + returnData.Item2.x, pos.y - returnData.Item2.y);
                    // Debug.Log("Checking index: " + (pos.x + size.x * -pos.y));
                        positions[pos.x + size.x * -pos.y].SetRead(true);
                    }
                    
                    int isThisWallFollowingOuterCorner = 0;
                    if(returnData.Item3 < 0 && wall.Item1.Count > 0)
                    {
                        //If lastWall is less than 0, then this is the following wall after an outer corner, so it must be moved up and shortened
                        isThisWallFollowingOuterCorner = 1;
                    }
                    returnData = HasWallNeighbor(pos, currentAngle);
                    int isThisWallEndingWithOuterCorner = 0;
                    if(returnData.Item3 < 0)
                    {
                        //If Item3 is less than 0, then this is an outer corner, so the wall shouldn't go the whole way
                        isThisWallEndingWithOuterCorner = 1;
                    }
                    
                    //Debug.Log("<color=green>Amount of steps: </color>" + steps + " angle: " + currentAngle);
                    //Only set wrap to true if the last wall ends up adjacent to the first wall

                    float divisionModifier = 0; //Apparently different divisions try to put the walls at different places, annoyingly

                    if( positions[pos.x + size.x * -pos.y].divisions.x > 1)
                    {
                        divisionModifier = 1.0f / (((float)positions[pos.x + size.x * -pos.y].divisions.x+1) / ((float)positions[pos.x + size.x * -pos.y].divisions.x - 1)); //This perfectly describes where each wall will stand, based on amount of division
                    }

                    float roundedness = indoors ? 0 : UnityEngine.Random.Range(0.0f, 1.0f);
                        
                    if(returnData.Item3 < 0)
                    {
                        //If Item3 is less than 0, then this is an outer corner, so the wall shouldn't go the whole way
                        isThisWallEndingWithOuterCorner = 1;
                        roundedness = 1.0f;
                    }
                    else
                    {
                        roundedness = 0;
                    }

                    AnimationCurve curve = new AnimationCurve();
                    

                    if(currentAngle == 0)
                    {
                        //Debug.Log("adding 0 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - divisionModifier + isThisWallFollowingOuterCorner,startPosition.y,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                    }
                    if(currentAngle == 90)
                    {
                        //Debug.Log("adding 90 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f, startPosition.y - 0.5f + divisionModifier - isThisWallFollowingOuterCorner,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                    }
                    if(currentAngle == 180)
                    {
                        //Debug.Log("adding 180 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + divisionModifier,startPosition.y - 1,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                    }
                    if(currentAngle == 270)
                    {
                        //Debug.Log("adding 270 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f,startPosition.y - 0.5f - divisionModifier + isThisWallFollowingOuterCorner ,0), -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness)); // y - 0.5f
                    }
                    //Sometimes it has to decrease by 90, so it has to know what direction the next wall goes in (fuck)
                    currentAngle += 90 * returnData.Item3; //This code can only do inner corners atm, not outer corners
                    currentAngle = (int)Math.Mod(currentAngle, 360);
                }
                Debug.Log("There is this amount of walls: " + wall.Item1.Count);
                wall = new Tuple<List<MeshMaker.WallData>, bool>(wall.Item1, ExtractWalls_DoesWallWrap(wall.Item1));

                data.Add(wall);
            }
            
            return data;
        }
        bool ExtractWalls_DoesWallWrap(List<MeshMaker.WallData> data)
        {
            return false;
        }

        void ExtractWalls_GetStartPosition(ref Vector2Int pos, ref int currentAngle, Entrances.Entrance entrance)
        {
            //Get the position that is to the left of the given entrance
            if(entrance.dir == new Vector2Int(0,1)) //north
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count-1].x + 1, -entrance.positions[entrance.positions.Count-1].y);
                currentAngle = 0;
            }
            if(entrance.dir == new Vector2Int(0,-1)) //south
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count-1].x - 1, -entrance.positions[entrance.positions.Count-1].y);
                currentAngle = 180;
            }
            if(entrance.dir == new Vector2Int(1,0)) //right
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count-1].x , -entrance.positions[entrance.positions.Count-1].y - 1);
                currentAngle = 270;
            }
            if(entrance.dir == new Vector2Int(-1,0)) //left
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count-1].x , -entrance.positions[entrance.positions.Count-1].y + 1);
                currentAngle = 90;
            }



           /*for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    //TODO Wall reading should actually start at the first found door!
                    //TODO Do this later when the doors have been changed for big rooms

                    //Check if there is floor diagonally right down, because walls can only be drawn from left to right
                    //If there are none, rotate the search three times. If there still are none, then there is an error
                    if(IsPositionWithinBounds(new Vector2Int(x + 1, -y - 1)) && (x < size.x && positions[x + 1 + size.x * (y + 1)].identity == 2 || x < size.x && positions[x + 1 + size.x * (y + 1)].identity == 3))
                    //2 is floor, 3 is door but door also counts as floor
                    {
                        pos = new Vector2Int(x, -y); 
                        currentAngle = 90;
                        break; 
                    }
                        //if none of the directions are a floor, then it is a void
                    // template.positions[x + template.size.x * y].SetIdentity(0);
                }
                if(pos != new Vector2Int(-1,-1))
                {
                    break;
                }
            }*/
            //Debug.Log("The position found to start from, that has a floor next to it: " + pos);
        }
        void ExtractWalls_GetSteps(ref Vector2Int pos, ref int steps, Vector2Int direction, int currentAngle)
        {
            //Check that the next direction is a wall, and also that said position has more than 1 neighbor otherwise you get weird bugs cuz walls try to be built with 0 width
            while(
                IsPositionWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) && positions[pos.x + direction.x + size.x * (-pos.y + direction.y)].identity == 1 &&
                ExtractWalls_CheckHasEnoughWallsAhead(pos, direction, currentAngle)
                //While the position in the next direction is a wall
                )
            {
                steps++;
                pos = new Vector2Int(pos.x + direction.x, pos.y - direction.y);
                // Debug.Log("Checking index: " + (pos.x + size.x * -pos.y));
                positions[pos.x + size.x * -pos.y].SetRead(true);
            }
        }
        bool ExtractWalls_CheckHasEnoughWallsAhead(Vector2Int pos, Vector2Int direction, int currentAngle)
        {
            if(IsPositionWithinBounds(new Vector2Int(pos.x + direction.x * 2, pos.y - direction.y * 2)) && positions[pos.x + direction.x * 2 + size.x * (-pos.y + direction.y * 2)].identity != 1) 
            //Check if the next position after the next isn't a wall, cuz then the next position is the last
            {
                Tuple<bool, Vector2Int, int> returnData = HasWallNeighbor(pos, currentAngle); //Check if it's an outer corner, cuz only then should the next check happen

                if(returnData.Item3 == -1)
                {
                    Vector2 temp = (Quaternion.Euler(0,0,-90) * (Vector2)direction);
                    Vector2Int rotatedDirection = new Vector2Int(Mathf.RoundToInt(temp.x), Mathf.RoundToInt(temp.y));
                    if(IsPositionWithinBounds(new Vector2Int(pos.x + direction.x + rotatedDirection.x *2, pos.y - direction.y - rotatedDirection.y *2)) && positions[pos.x + direction.x  + rotatedDirection.x *2 + size.x * (-pos.y + direction.y + rotatedDirection.y * 2 )].identity != 2)
                    //Check if the next position has enough walls in that direction to go that far 
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true; //If the next position isn't the last, you can proceed
        }
        public List<Vector3Int> ExtractFloor()
        {
            List<Vector3Int> returnData = new List<Vector3Int>();
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    if(positions[x + size.x * y].identity == 0) //!Is supposed to add floor on corners. Thats not how its gonna work later
                    {
                        for(int i = -1; i < 2; i++)
                        {
                            for(int j = -1; j < 2; j++)
                            {
                                if(i == 0 && j == 0){continue;}
                                if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                                {
                                    if(positions[x + i + (int)size.x * (y + -j)].identity == 1) //If the position is a wall
                                    {
                                        //returnData.Add(new Vector3Int(x,-y -1, 0));
                                    }
                                }
                            }
                        }
                    }
                    if(positions[x + size.x * y].identity == 2 || positions[x + size.x * y].identity == 1 || positions[x + size.x * y].identity == 3) //If this position is a floor or a wall
                    {
                        returnData.Add(new Vector3Int(x,-y -1, 0));
                    }
                    else if(!indoors) //If it is a void
                    {
                        returnData.Add(new Vector3Int(x,-y -1, 4));
                    }
                }
            }
            return returnData;
        }
        public void AddEntrancesToRoom(Entrances entrances)
        {
            for(int i = 0; i < entrances.entrances.Count; i++)
            {
                if(entrances.entrances[i].spawned && entrances.entrances[i].open)
                {
                    for(int j = 0; j < entrances.entrances[i].positions.Count; j++)
                    {
                        int x = entrances.entrances[i].positions[j].x;
                        int y = entrances.entrances[i].positions[j].y;
                        //Debug.Log("X: " + x + " Y: " + y);
                        positions[x + size.x * y].identity = 3; //Turn the position into a door
                        EnsureEntranceReachability(entrances.entrances[i]);
                    }
                }
            }
            UnscatterWalls();
            WeedOutUnseeableWalls(); //Later only when indoors
        }
        public List<TileTemplate> GetEntranceTiles()
        {
            List<TileTemplate> tiles = new List<TileTemplate>();
            for(int i = 0; i < positions.Count; i++)
            {
                if(positions[i].identity == 3)
                {
                    tiles.Add(positions[i]);
                }
            }
            return tiles;
        }
        public TileTemplate GetEntranceTile(Vector2Int position)
        {
            return positions[position.x + size.x * position.y];
        }
        public void SetEntranceTileVertices(List<Vector3> vertices, Vector2Int position, bool start) //The entrance of this room
        {
            if(start)
            {
                positions[position.x + size.x * position.y].startVertices = vertices;
            }
            else
            {
                positions[position.x + size.x * position.y].endVertices = vertices;
            }
        }
        void EnsureEntranceReachability(Entrances.Entrance entrance)
        {
            List<Vector2Int> currentPosition = new List<Vector2Int>();
            for(int i = 0; i < entrance.positions.Count; i++)
            {
                currentPosition.Add(new Vector2Int(entrance.positions[i].x, -entrance.positions[i].y));
            }

            for(int i = 0; i < currentPosition.Count; i++)
            {
                while(IsPositionWithinBounds(currentPosition[i] - entrance.dir)) //While youre still within bounds
                {
                    currentPosition[i] = currentPosition[i] - entrance.dir;
                    if(positions[currentPosition[i].x + size.x * -currentPosition[i].y].identity == 1) //If there is a wall in the direction of this door
                    {
                        positions[currentPosition[i].x + size.x * -currentPosition[i].y].identity = 2; //Turn it into a floor
                    }
                    else if(positions[currentPosition[i].x + size.x * -currentPosition[i].y].identity == 2) //If youve reached a floor, stop
                    {
                        break;
                    }
                }
            }
        }
    }

    public Entrances directions;

    public Vector2Int size;

    public RoomData roomData = new RoomData();

    public RoomDebug debug;

    public GameObject debugFloor;
    public int section;

    public void OpenAllEntrances(Vector2Int gridPosition, Vector2Int roomSize) //Roomsize in grid space
    {
        if(directions == null)
        {
            directions = new Entrances(gridPosition, roomSize);
        }
        directions.OpenAllEntrances();
    }
    public void Initialize(Vector2Int roomSize, bool indoors, int section_in, ref List<RoomTemplate> templates)
    {
        Debug.Log("<color=green>Initializing the Origin Room</color>");
        //This Initialize() function is for the origin room specifically, as it already has its own position
        section = section_in;
        OnInitialize(Vector2Int.zero, roomSize, indoors, ref templates);
        OpenAllEntrances(Vector2Int.zero, new Vector2Int(roomSize.x / 20, roomSize.y / 20));
    }

    public void Initialize(Vector2Int location, Vector2Int roomSize,  bool indoors, int section_in, ref List<RoomTemplate> templates)
    {
        transform.position = new Vector2(location.x, location.y);
        section = section_in;
        OnInitialize(new Vector2Int(location.x / 20, location.y / 20), roomSize, indoors, ref templates);
    }
    void OnInitialize(Vector2Int gridPosition, Vector2Int roomSize, bool indoors, ref List<RoomTemplate> templates) 
    {
        size = roomSize;
        directions = new Entrances(gridPosition, roomSize / 20);
        //Build wall meshes all around the start area in a 30 x 30 square
        RoomTemplate template = new RoomTemplate(roomSize, new List<RoomTemplate.TileTemplate>(), indoors);
        //CreateRoom(template, wallMaterial, floorMaterial);
        templates.Add(template);
    }

    public void CreateRoom(ref RoomTemplate template, Material wallMaterial_in, Material floorMaterial_in)
    {
        //This shouldn't actually get called until the doors have all been finalized, which is only when the whole dungeon is done
        //So this should not get called in OnInitialize!!
        Color color = new Color32((byte)UnityEngine.Random.Range(125, 220),(byte)UnityEngine.Random.Range(125, 220),(byte)UnityEngine.Random.Range(125, 220), 255);
        Material wallMaterial = new Material(wallMaterial_in.shader);
        wallMaterial.CopyPropertiesFromMaterial(wallMaterial_in);
        Material floorMaterial = new Material(floorMaterial_in.shader);
        floorMaterial.CopyPropertiesFromMaterial(floorMaterial_in);
        if(template.indoors)
        {
            wallMaterial.mainTexture = floorMaterial_in.mainTexture;
            wallMaterial.color = color + Color.white / 10;
        }
        else
        {
            floorMaterial.SetTexture("_BaseMap", Resources.Load<Texture>("Art/Earth"));
        }
        floorMaterial.color = color;
        CreateWalls(template, wallMaterial);
        CreateFloor(template, floorMaterial);
    }
    void CreateWalls(RoomTemplate template, Material wallMaterial)
    {
        Debug.Log("Creating walls");
        List<Tuple<List<MeshMaker.WallData>, bool>> data = template.ExtractWalls(directions);
        Debug.Log("Data size: " + data.Count);

        if(debug.CheckTemplate == RoomDebug.RoomBuildMode.CHECK_TEMPLATE || debug.CheckTemplate == RoomDebug.RoomBuildMode.BOTH)
        {
            DEBUG_TemplateCheck(template);
        }

        for(int i = 0; i < data.Count; i++)
        {
            GameObject wallObject = new GameObject("Wall");
            wallObject.transform.parent = this.gameObject.transform;

            if(debug.CheckTemplate == RoomDebug.RoomBuildMode.NONE || debug.CheckTemplate == RoomDebug.RoomBuildMode.BOTH)
            {
                MeshMaker.CreateWall(wallObject, wallMaterial, data[i].Item1, data[i].Item2, template.GetEntranceTiles());
            }
            wallObject.transform.localPosition = new Vector3(-9.5f, 10, 0);
        }
    }
    void CreateFloor(RoomTemplate template, Material floorMaterial)
    {
        GameObject floorObject = new GameObject("Floor");
        floorObject.transform.parent = this.gameObject.transform;
        floorObject.AddComponent<MeshFilter>();

        MeshMaker.CreateSurface(template.ExtractFloor(), floorObject.GetComponent<MeshFilter>().mesh);
        floorObject.transform.localPosition = new Vector3(- 10, 10, 0);

        floorObject.AddComponent<MeshRenderer>();
        floorObject.GetComponent<MeshRenderer>().material = floorMaterial;

        MeshCollider mc = floorObject.AddComponent<MeshCollider>();
        mc.sharedMesh = floorObject.GetComponent<MeshFilter>().mesh;

        /*for(int i = 0; i < 17; i++)
        {
            GameObject vase = MeshMaker.CreateVase(floorMaterial);
            vase.transform.parent = this.gameObject.transform;
            vase.transform.localPosition = new Vector3(-i + 8, -1, 0);
        }

        //vase.AddComponent<Rigidbody>(); dont add it yet, because the vase has no bottom!!*/

    }
    void DEBUG_TemplateCheck(RoomTemplate template)
    {
        GameObject debugObject = new GameObject("DEBUG");
        debugObject.transform.parent = transform;
        for(int x = 0; x < template.size.x; x++)
        {
            for(int y = 0; y < template.size.y; y++)
            {
                GameObject temp = Instantiate(debugFloor, new Vector3(x,-y - 0.5f,-1), Quaternion.identity, debugObject.transform);
                temp.GetComponent<MeshRenderer>().material.color = template.positions[x + template.size.x * y].identity == 2 ? debug.floorColor : template.positions[x + template.size.x * y].identity == 0 ? Color.black:
                template.positions[x + template.size.x * y].identity == 3 ? Color.red: 
                template.positions[x + template.size.x * y].read ? debug.wallColor: Color.white;
            }
        }
        debugObject.transform.localPosition = new Vector3(-9.5f, 10, 0);
    }

    public Vector2 GetCameraBoundaries()
    {
        return size;
    }

    public RoomPosition GetRoomPositionType()
    {
        return roomData.m_roomPosition;
    }

    public Entrances GetDirections()
    {
        return directions;
    }
    public List<Entrances.Entrance> GetOpenUnspawnedEntrances()
    {
        List<Entrances.Entrance> openEntrances = new List<Entrances.Entrance>{};
        foreach(Entrances.Entrance entrance in directions.entrances)
        {
            if (entrance.open && !entrance.spawned)
            {
                openEntrances.Add(entrance);
            }
        }
        return openEntrances;
    }

    bool GetIsEndRoom()
    {
        //This gets if the room is an endroom. However, this could be set by having the rooms be endrooms when they spawn, unless they get linked
        //And then set rooms being spawned from as no longer being endrooms
        List<Entrances.Entrance> entrances = new List<Entrances.Entrance> { };
        if(directions == null){return false;}
        foreach(Entrances.Entrance entrance in directions.entrances)
        {
            if(entrance.spawned == true && entrance.open == true)
            {
                entrances.Add(entrance);
            }
        }
        return entrances.Count == 1;
    } 

    public void ChooseRoomType(LevelData data)
    {
        List<RoomType> probabilityList = new List<RoomType> { }; //A list of roomtypes to choose between
        List<RoomType> roomsToCheck = new List<RoomType>{RoomType.AmbushRoom, RoomType.TreasureRoom, RoomType.RestingRoom, RoomType.NormalRoom}; //A list of roomtypes to check the probability of

        if (GetIsEndRoom())
        {
            roomData.m_roomPosition = RoomPosition.DeadEnd;
            probabilityList.Add(RoomType.TreasureRoom);
            probabilityList.Add(RoomType.AmbushRoom);
        }
        else
        {
            for(int i = 0; i < roomsToCheck.Count; i++)
            {
                for(int j = 0; j < data.GetRoomProbability(roomsToCheck[i]); j++)
                {
                    probabilityList.Add(roomsToCheck[i]);
                }
            }
            if (roomData.stepsAwayFromMainRoom > 5)
            {
                probabilityList.Add(RoomType.MiniBossRoom);
                probabilityList.Add(RoomType.AmbushRoom);
            }
        }
        roomData.m_type = probabilityList[UnityEngine.Random.Range(0, probabilityList.Count)];
    }
    public void SetRoomType(RoomType newType)
    {
        roomData.m_type = newType;
    }
    public RoomType GetRoomType()
    {
        return roomData.m_type;
    }
    public void DisplayDistance()
    {
        //GetComponentInChildren<Number>().OnDisplayNumber(roomData.stepsAwayFromMainRoom);
    }
}