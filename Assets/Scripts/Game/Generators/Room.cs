using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Rendering;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Room))]
public class RoomEditor:Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Room room = (Room)target;
        //GUI.DrawTexture(new Rect(0,0,room.templateDEBUG.width * 30, room.templateDEBUG.height * 30), room.templateDEBUG);
        if(room.templateTexture)
        {
            RenderTexture("Template", room.templateTexture);
        }
        if(room.mapTexture)
        {
            RenderTexture("Map", room.mapTexture);
        }
        //GUILayout.Label(RenderTexture("Template", room.templateDEBUG));
    }
    void RenderTexture(string name, Texture2D texture)
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 400;
        GUILayout.Label(name, style);

        style.fixedWidth = texture.height > texture.width? ((float)texture.width / (float)texture.height) * 400 :400;
        style.fixedHeight = texture.width > texture.height? ((float)texture.height / (float)texture.width) * 400 :400;

        style.normal.background = texture;
        GUILayout.Label(new Texture2D(0,0), style);
        //var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(400), GUILayout.Height(400));
        GUILayout.EndVertical();
    }
}

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
        public Color floorColor;
        public Color wallColor;
    }
    public class RoomTemplate
    {
        //This class is given to the CreateWalls in order to draw the meshes for walls
        //It is also given to the CreateFloor in order to draw the floor
        public class TileTemplate
        {
            public int elevation;
            //0 was void, 1 was wall, 2 was floor
            public bool door;
            public bool wall; //Set that there is a wall if this is tile has a higher elevation than a tile next to it
            public bool read;

            public Vector2Int divisions; //This also only does something if the identity is a wall
            //If divide into multiple parts, like, three by three quads on one wall tile on outdoor walls for instance. Usually, on indoor walls, its completely flat

            public List<Vector3> endVertices = new List<Vector3>(); //When wall ends, and this list is empty, save all vertices in here otherwise use
            public List<Vector3> startVertices = new List<Vector3>(); //If this is empty when wall starts, fill it up. Otherwise use

            public List<Vector3> floorVertices = new List<Vector3>();
            public List<Vector3> ceilingVertices = new List<Vector3>(); //Slash upper floor

            public class TileSides
            {
                //! to identify walls
                public Vector2Int side;
                public bool floor; //If yes, then don't draw a triangle on this upper floor. Youre supposed to draw a floor at the base of the wall instead. If no, then this is on the inside of the wall, so it goes on top
                public TileSides(Vector2Int side_in)
                {
                    side = side_in;
                    floor = true;
                }
            }
            public List<TileSides> sidesWhereThereIsWall = new List<TileSides>();

            public TileTemplate(int elevation_in, Vector2Int divisions_in)
            {
                elevation = elevation_in;
                read = false;
                divisions = divisions_in;
                wall = false;
                door = false;
                sidesWhereThereIsWall = new List<TileSides>(){new TileSides(Vector2Int.up),new TileSides(Vector2Int.down),new TileSides(Vector2Int.left),new TileSides(Vector2Int.right)};
            }
            public void SetElevation(int newElevation)
            {
                elevation = newElevation;
            }
        }
        public Vector2Int size;
        public Grid<TileTemplate> positions;
        public bool indoors;
        public bool surrounding;
        public RoomTemplate(Vector2Int size_in, Grid<TileTemplate> positions_in, bool indoors_in, bool surrounding_in)
        {
            size = size_in;
            positions = positions_in;
            indoors = indoors_in;
            surrounding = surrounding_in;
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
                    int elevation = surrounding ? 4 : 0;
                    positions.Add(new RoomTemplate.TileTemplate(elevation, divisions));

                    if(!surrounding)
                    {
                        CreateRoomTemplate_Square(new Vector2(2,2), x, y, 4); //?Basic thickness. Can't be thinner than 2
                        if(!indoors){CreateRoomTemplate_Circle(roomCenter, wallThickness, x, y);}
                        //CreateRoomTemplate_Cross(wallThickness, x, y);
                    }
                }
            }
            SmoothenOut();
        }
        void SmoothenOut()
        {
            //Push up
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int amountOfWallNeighbors = 0;
                    int elevation = positions[x,y].elevation;
                    for(int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            if(i == 0 && j == 0){continue;}
                            if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if(positions[x + i , y + -j].elevation != positions[x,y].elevation) //If the position is a wall
                                {
                                    if(positions[x + i , y + -j].elevation > positions[x,y].elevation)
                                    {
                                        elevation = positions[x + i , y + -j].elevation;
                                    }
                                    amountOfWallNeighbors++; //Then count up
                                }
                            }
                        }
                    }
                    if(amountOfWallNeighbors > 5)
                    {
                        positions[x,y].elevation = elevation;
                    }
                }
            }
            //Push down
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int amountOfWallNeighbors = 0;
                    int elevation = positions[x,y].elevation;
                    for(int i = -1; i < 2; i++)
                    {
                        for(int j = -1; j < 2; j++)
                        {
                            if(i == 0 && j == 0){continue;}
                            if(IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if(positions[x + i , y + -j].elevation != positions[x,y].elevation) //If the position is a wall
                                {
                                    if(positions[x + i , y + -j].elevation < positions[x,y].elevation)
                                    {
                                        elevation = positions[x + i , y + -j].elevation;
                                    }
                                    amountOfWallNeighbors++; //Then count up
                                }
                            }
                        }
                    }
                    if(amountOfWallNeighbors > 5)
                    {
                        positions[x,y].elevation = elevation;
                    }
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
                    positions[x + (int)size.x * y].elevation = 4;
                }
            }
        }
        void CreateRoomTemplate_Square(Vector2 wallThickness, int x, int y, int elevation)
        {
            if(x < wallThickness.x || x > size.x - wallThickness.x - 1||
               y < wallThickness.y || y > size.y - wallThickness.y - 1)
            {
                positions[x + (int)size.x * y].elevation = elevation;
            }
        }
        void CreateRoomTemplate_Cross(Vector2 wallThickness, int x, int y, int elevation)
        {
            if((x < wallThickness.x || x > size.x - wallThickness.x -1)&&
               (y < wallThickness.y || y > size.y - wallThickness.y -1))
            {
                positions[x + (int)size.x * y].elevation = elevation;
            }
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
                        positions[x + size.x * y].door = true; //Turn the position into a door
                        EnsureEntranceReachability(entrances.entrances[i]);
                    }
                }
            }
            IdentifyWalls();
            UnscatterWalls();
            BloatWallCrossings();
        }
        void IdentifyWalls()
        {
            //! this function goes through all positions to identify which positions are where walls are supposed to be
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int[] constraints = positions.GetValidConstraints(x,y);
                    for(int x_w = constraints[0]; x_w < constraints[2]; x_w++)
                    {
                        for(int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                        {
                            //If this position has one adjacent position that is a lower elevation from itself, then it is a wall
                            if(positions[x_w, y_w].elevation < positions[x,y].elevation)
                            {
                                positions[x,y].wall = true;
                            }
                        }
                    }
                }
            }
        }
        void UnscatterWalls()
        {
            //! this function gets rid of positions that cant be made into walls
            //! find all positions that have one height and is a wall but no surrounding positions of a different height and eliminate them
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    if(positions[x,y].wall)
                    {
                        OnUnscatterWalls(x,y);
                    }
                }
            }
        }
        void OnUnscatterWalls(int x, int y)
        {
            int[] constraints = positions.GetValidConstraints(x,y);
            int lowestAdjacentElevation = positions[x,y].elevation;
            for(int x_w = constraints[0]; x_w < constraints[2]; x_w++)
            {
                for(int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                {
                    if(positions[x_w, y_w].elevation < lowestAdjacentElevation){lowestAdjacentElevation = positions[x_w, y_w].elevation;}

                    if(positions[x_w, y_w].elevation == positions[x,y].elevation &&
                       !positions[x_w, y_w].wall) //if there is one adjacent position has the same elevation and isn't a wall, then this is indeed a wall
                        {
                            return; //This is indeed a wall, keep
                        }
                }
            }
            positions[x,y].wall = false;
            positions[x,y].elevation = lowestAdjacentElevation;
            //No this is a fluke, delete
        }

        void BloatWallCrossings()
        {
            bool bloat = true;
            while(bloat)
            {
                bloat = false;
                for(int x = 0; x < size.x; x++)
                {
                    for(int y = 0; y < size.y; y++)
                    {
                        if(positions[x,y].wall)
                        {
                            //Debug.Log("Origin: " + x + " " + y);
                            int[] constraints = positions.GetValidConstraints(x,y);
                            int adjWalls = 0;
                            for(int x_w = constraints[0]; x_w < constraints[2]; x_w++)
                            {
                                for(int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                                {
                                    //Count walls
                                    if((x_w == x+1 && y_w == y+1) || (x_w == x+1 && y_w == y-1) ||(x_w == x-1 && y_w == y+1) || (x_w == x-1 && y_w == y-1)){continue;}
                                    if(positions[x_w, y_w].wall && positions[x_w, y_w].elevation == positions[x,y].elevation)
                                    {
                                       // Debug.Log("Adding: " + x_w + " " + y_w);
                                        adjWalls++;
                                    }
                                }
                            }
                            if(adjWalls > 3)
                            {
                                bloat = true;
                               // Debug.Log("BLOATING IDENTIFIED");
                                //! then this is a wall crossing. Those must be bloated
                                constraints = positions.GetValidConstraints(x,y,2);
                                for(int x_w = constraints[0]; x_w < constraints[2]; x_w++)
                                {
                                    for(int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                                    {
                                        //Count walls
                                        positions[x_w, y_w].wall = false;
                                        positions[x_w, y_w].elevation = positions[x,y].elevation;
                                    }
                                }
                                for(int x_w = constraints[0]; x_w < constraints[2]; x_w++) //Go around the original position
                                {
                                    for(int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                                    {
                                        int[] constraintsTwo = positions.GetValidConstraints(x_w, y_w);
                                        for(int x_ww = constraintsTwo[0]; x_ww < constraintsTwo[2]; x_ww++) //Go around each adjacent position
                                        {
                                            for(int y_ww = constraintsTwo[1]; y_ww < constraintsTwo[3]; y_ww++)
                                            {
                                                if(positions[x_ww, y_ww].elevation < positions[x_w,y_w].elevation)
                                                {
                                                    positions[x_w,y_w].wall = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
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
                if(IsPositionWithinBounds(new Vector2Int(pos.x + 1, pos.y)) && positions[pos.x + 1, -pos.y].wall && !positions[pos.x + 1 , -pos.y].read && SharesFloor(pos, Vector2Int.right))
                {
                    direction = new Vector2Int(1, 0);
                    rotationDir = 1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x - 1, pos.y)) && positions[pos.x - 1 , -pos.y].wall && !positions[pos.x - 1 , -pos.y].read && SharesFloor(pos, Vector2Int.left))
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
                if(IsPositionWithinBounds(new Vector2Int(pos.x + 1, pos.y)) && positions[pos.x + 1 , -pos.y].wall && !positions[pos.x + 1 , -pos.y].read && SharesFloor(pos, Vector2Int.right))
                {
                    direction = new Vector2Int(1, 0);
                    rotationDir = -1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x - 1, pos.y)) && positions[pos.x - 1 ,-pos.y].wall && !positions[pos.x - 1 , -pos.y].read && SharesFloor(pos, Vector2Int.left))
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
                if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y + 1)) && positions[pos.x , (-pos.y - 1)].wall && !positions[pos.x , (-pos.y - 1)].read && SharesFloor(pos, Vector2Int.down))
                {
                    direction = new Vector2Int(0, -1);
                    rotationDir = 1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y - 1)) && positions[pos.x , (-pos.y + 1)].wall && !positions[pos.x , (-pos.y + 1)].read && SharesFloor(pos, Vector2Int.up))
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
                if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y + 1)) && positions[pos.x , (-pos.y - 1)].wall && !positions[pos.x , (-pos.y - 1)].read && SharesFloor(pos, Vector2Int.down)) 
                {
                    direction = new Vector2Int(0, -1);
                    rotationDir = -1;
                }
                else if(IsPositionWithinBounds(new Vector2Int(pos.x, pos.y - 1)) && positions[pos.x ,(-pos.y + 1)].wall && !positions[pos.x , (-pos.y + 1)].read  && SharesFloor(pos, Vector2Int.up))
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
                    positions[pos.x, -pos.y].read = true;
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
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - divisionModifier + isThisWallFollowingOuterCorner,startPosition.y,0), startPosition, 
                            new Vector3(-divisionModifier, 0, 0), 
                            -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                        }
                        if(currentAngle == 90)
                        {
                            //Debug.Log("adding 90 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f, startPosition.y - 0.5f + divisionModifier - isThisWallFollowingOuterCorner,0), startPosition, 
                            new Vector3(-0.5f, -0.5f + divisionModifier), //-0.5f + divisionModifier - isThisWallFollowingOuterCorner
                            -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                        }
                        if(currentAngle == 180)
                        {
                            //Debug.Log("adding 180 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + divisionModifier,startPosition.y - 1,0), startPosition,
                            new Vector3(-divisionModifier, 0, 0), 
                            -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                        }
                        if(currentAngle == 270)
                        {
                            //Debug.Log("adding 270 degree wall");
                            wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f,startPosition.y - 0.5f - divisionModifier + isThisWallFollowingOuterCorner ,0), startPosition, 
                            new Vector3(-0.5f, -0.5f + divisionModifier), //-0.5f - divisionModifier + isThisWallFollowingOuterCorner
                            -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness)); // y - 0.5f
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
                        if(IsPositionWithinBounds(new Vector2Int(x + 1, -y - 1)) && x < size.x && !positions[x + 1 + size.x * (y + 1)].wall)
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
                positions[pos.x, -pos.y].read = true;
                Vector2Int startPosition = pos;

                while(returnData.Item1) //If there is a wall neighbor, proceed
                {
                    startPosition = pos;
                    //Follow that direction until its empty
                    int steps = 1;
                //Debug.Log("Checking index: " + (pos.x + returnData.Item2.x + size.x * (-pos.y + returnData.Item2.y)));
                    while(IsPositionWithinBounds(new Vector2Int(pos.x + returnData.Item2.x, pos.y - returnData.Item2.y)) && positions[pos.x + returnData.Item2.x , -pos.y + returnData.Item2.y].wall) //While the position in the next direction is a wall
                    {
                        steps++;
                        pos = new Vector2Int(pos.x + returnData.Item2.x, pos.y - returnData.Item2.y);
                        //Debug.Log("Checking index: " + (pos.x + size.x * -pos.y));
                        positions[pos.x, -pos.y].read = true;
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
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - divisionModifier + isThisWallFollowingOuterCorner,startPosition.y,0), startPosition, 
                        new Vector3(),
                        -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                    }
                    if(currentAngle == 90)
                    {
                        //Debug.Log("adding 90 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f, startPosition.y - 0.5f + divisionModifier - isThisWallFollowingOuterCorner,0), startPosition, 
                        new Vector3(0.5f, -0.5f),
                        -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                    }
                    if(currentAngle == 180)
                    {
                        //Debug.Log("adding 180 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + divisionModifier,startPosition.y - 1,0), startPosition, 
                        new Vector3(0,-1,0),
                        -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness));
                    }
                    if(currentAngle == 270)
                    {
                        //Debug.Log("adding 270 degree wall");
                        wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f,startPosition.y - 0.5f - divisionModifier + isThisWallFollowingOuterCorner ,0), startPosition, 
                        new Vector3(-0.5f, -0.5f,0),
                        -currentAngle, steps - isThisWallEndingWithOuterCorner - isThisWallFollowingOuterCorner, 4, 0, positions[pos.x + size.x * -pos.y].divisions, curve, roundedness)); // y - 0.5f
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
                IsPositionWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) && positions[pos.x + direction.x, -pos.y + direction.y].wall &&
                ExtractWalls_CheckHasEnoughWallsAhead(pos, direction, currentAngle) &&
                SharesFloor(pos, direction)
                //While the position in the next direction is a wall
                )
            {
                steps++;
                pos = new Vector2Int(pos.x + direction.x, pos.y - direction.y);
                // Debug.Log("Checking index: " + (pos.x + size.x * -pos.y));
                positions[pos.x , -pos.y].read = true;
            }
        }
        bool SharesFloor(Vector2Int pos, Vector2Int direction)
        {
            //!If there is a wall ahead in that direction, check if they share a floor. Otherwise, dont go to it

            if(IsPositionWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y )) && positions[pos.x + direction.x, -pos.y + direction.y].wall)
            {
                List<Vector2Int> temp = new List<Vector2Int>();
                int[] constraints = positions.GetValidConstraints(pos.x, -pos.y);
                
                for(int x = constraints[0]; x < constraints[2]; x++)
                {
                    for(int y = constraints[1]; y < constraints[3]; y++)
                    {
                        if(positions[x,y].elevation < positions[pos.x, -pos.y].elevation)
                        {
                            temp.Add(new Vector2Int(x,y));
                        }
                    }
                }

                constraints = positions.GetValidConstraints(pos.x + direction.x, -pos.y + direction.y);
                for(int x = constraints[0]; x < constraints[2]; x++)
                {
                    for(int y = constraints[1]; y < constraints[3]; y++)
                    {
                        for(int i = 0; i < temp.Count; i++)
                        {
                            if(temp[i] == new Vector2Int(x,y))
                            {
                                //Then this means the next wall shares this floor
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            return true;
        }
        bool ExtractWalls_CheckHasEnoughWallsAhead(Vector2Int pos, Vector2Int direction, int currentAngle)
        {
            if(IsPositionWithinBounds(new Vector2Int(pos.x + direction.x * 2, pos.y - direction.y * 2)) && !positions[pos.x + direction.x * 2, -pos.y + direction.y * 2].wall) 
            //Check if the next position after the next isn't a wall, cuz then the next position is the last
            //This if statement runs if the next after the next position is the last
            {
                Tuple<bool, Vector2Int, int> returnData = HasWallNeighbor(pos, currentAngle); //Check if it's an outer corner, cuz only then should the next check happen

                if(returnData.Item3 == -1)
                {
                    Vector2 temp = (Quaternion.Euler(0,0,-90) * (Vector2)direction);
                    Vector2Int rotatedDirection = new Vector2Int(Mathf.RoundToInt(temp.x), Mathf.RoundToInt(temp.y));
                    if(IsPositionWithinBounds(new Vector2Int(pos.x + direction.x + rotatedDirection.x *2, pos.y - direction.y - rotatedDirection.y *2)) && positions[pos.x + direction.x  + rotatedDirection.x *2 ,-pos.y + direction.y + rotatedDirection.y * 2].wall)
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
        public List<MeshMaker.SurfaceData> ExtractFloor()
        {
            List<MeshMaker.SurfaceData> returnData = new List<MeshMaker.SurfaceData>();
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    int elevation = positions[x,y].wall ? 0: positions[x,y].elevation;
                    returnData.Add(new MeshMaker.SurfaceData(new Vector3Int(x,-y -1, elevation), positions[x,y].ceilingVertices, positions[x,y].floorVertices, positions[x,y].divisions.x, positions[x,y].sidesWhereThereIsWall));
                }
            }
            return returnData;
        }
        public List<TileTemplate> GetEntranceTiles()
        {
            List<TileTemplate> tiles = new List<TileTemplate>();
            for(int i = 0; i < positions.Count(); i++)
            {
                if(positions[i].door)
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
                positions[position].startVertices = vertices;
            }
            else
            {
                positions[position].endVertices = vertices;
            }
        }
        void EnsureEntranceReachability(Entrances.Entrance entrance)
        {
            List<Vector2Int> currentPosition = new List<Vector2Int>();
            for(int i = 0; i < entrance.positions.Count; i++)
            {
                currentPosition.Add(new Vector2Int(entrance.positions[i].x, -entrance.positions[i].y));
                positions[entrance.positions[i]].elevation = 0;
            }

            for(int i = 0; i < currentPosition.Count; i++)
            {
                while(IsPositionWithinBounds(currentPosition[i] - entrance.dir)) //While youre still within bounds
                {
                    currentPosition[i] = currentPosition[i] - entrance.dir;
                    if(positions[currentPosition[i].x, -currentPosition[i].y].elevation > positions[entrance.positions[0]].elevation) //If there is a wall in the direction of this door
                    {
                        positions[currentPosition[i].x, -currentPosition[i].y].elevation = 0;
                    }
                    else if(positions[currentPosition[i].x, -currentPosition[i].y].elevation == 0) //If youve reached a floor, stop
                    {
                        break;
                    }
                }
            }
        }
        public Texture2D CreateMap()
        {
            Texture2D tex = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
            Color[] colors = new Color[size.x * size.y];
            Grid<TileTemplate> grid = positions.FlipVertically();
            for(int i = 0; i < size.x * size.y; i++)
            {
                float lumValue = (float)grid[i].elevation / 20f + 0.5f;
                colors[i] = Color.HSVToRGB(0.3f, 1, lumValue);
            }
            tex.Finish(colors);
            return tex;
        }
    }

    public class PlacementGridReference
    {
        public GameObject obj;
        public bool occupied; //Can be occupied without an object, in which case no placement square will be rendered
        public int elevation;

        public PlacementGridReference(GameObject obj_in, int elevation_in)
        {
            elevation = elevation_in;
            obj = obj_in;
        }
    }
    public Grid<PlacementGridReference> placementGrid;
    public Entrances directions;

    public Vector2Int size;

    public RoomData roomData = new RoomData();

    public RoomDebug debug;
    public Texture2D templateTexture;
    public Texture2D mapTexture;
    public int section;
    public Vegetation grass;

    public void OpenAllEntrances(Vector2Int gridPosition, Vector2Int roomSize) //Roomsize in grid space
    {
        if(directions == null)
        {
            directions = new Entrances(gridPosition, roomSize);
        }
        directions.OpenAllEntrances();
    }
    public void Initialize(Vector2Int roomSize, bool indoors, int section_in, ref List<RoomTemplate> templates, bool surrounding)
    {
        Debug.Log("<color=green>Initializing the Origin Room</color>");
        //This Initialize() function is for the origin room specifically, as it already has its own position
        section = section_in;
        OnInitialize(Vector2Int.zero, roomSize, indoors, ref templates, surrounding);
        OpenAllEntrances(Vector2Int.zero, new Vector2Int(roomSize.x / 20, roomSize.y / 20));
    }

    public void Initialize(Vector2Int location, Vector2Int roomSize,  bool indoors, int section_in, ref List<RoomTemplate> templates, bool surrounding)
    {
        transform.position = new Vector2(location.x, location.y);
        section = section_in;
        OnInitialize(new Vector2Int(location.x / 20, location.y / 20), roomSize, indoors, ref templates, surrounding);
    }
    void OnInitialize(Vector2Int gridPosition, Vector2Int roomSize, bool indoors, ref List<RoomTemplate> templates, bool surrounding) 
    {
        size = roomSize;
        directions = new Entrances(gridPosition, roomSize / 20);
        //Build wall meshes all around the start area in a 30 x 30 square
        RoomTemplate template = new RoomTemplate(roomSize, new Grid<RoomTemplate.TileTemplate>(roomSize), indoors, surrounding);
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
            floorMaterial.color = color;
        }
        else
        {
            floorMaterial.SetTexture("_BaseMap", Resources.Load<Texture>("Art/Earth"));
        }
        CreateWalls(template, wallMaterial);
        CreateFloor(template, floorMaterial);
        SavePlacementGrid(template);
        SaveTemplateTexture(template);
        Furnish(floorMaterial);
        mapTexture = template.CreateMap();
    }
    void CreateWalls(RoomTemplate template, Material wallMaterial)
    {
        //Debug.Log("Creating walls");
        List<Tuple<List<MeshMaker.WallData>, bool>> data = template.ExtractWalls(directions);
       // Debug.Log("Data size: " + data.Count);

        for(int i = 0; i < data.Count; i++)
        {
            GameObject wallObject = new GameObject("Wall");
            wallObject.transform.parent = this.gameObject.transform;
            MeshMaker.CreateWall(wallObject, wallMaterial, data[i].Item1, data[i].Item2, template.positions);
            wallObject.transform.localPosition = new Vector3(-9.5f, 10, 0);
        }
    }
    void CreateFloor(RoomTemplate template, Material floorMaterial)
    {
        GameObject floorObject = new GameObject("Floor");
        floorObject.transform.parent = gameObject.transform;


        MeshMaker.CreateSurface(template.ExtractFloor(), floorObject.transform, floorMaterial);
        floorObject.transform.localPosition = new Vector3(- 10, 10, 0);
    }

    void Furnish(Material mat)
    {
        int amountOfVases = UnityEngine.Random.Range(3, 6);
        for(int i = 0; i < amountOfVases; i++)
        {
            GameObject vase = MeshMaker.CreateVase(mat);
            vase.transform.parent = gameObject.transform;
            vase.transform.localPosition = FindRandomPlacementPositionOfSize(vase, new Vector2Int(2,2));
        }
    }

    Vector3 FindRandomPlacementPositionOfSize(GameObject obj, Vector2Int size)
    {
        bool searching = true;
        List<Vector2Int> positions = new List<Vector2Int>();
        do
        {
            searching = false;
            Vector2Int startPos = placementGrid.GetRandomPosition();

            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    positions.Add(new Vector2Int(startPos.x + x, startPos.y + y));
                    if(!placementGrid.IsWithinBounds(startPos.x + x, -startPos.y - y) || 
                        placementGrid[startPos.x + x, startPos.y + y].occupied || 
                        placementGrid[startPos.x + x, startPos.y + y].elevation != placementGrid[startPos].elevation)
                    {
                        //!if adjacent position is occupied or if the adjacent elevation is different
                        //!then this position is bad, continue while loop
                        searching = true;
                        positions.Clear();
                    }
                }
            }
        }
        while(searching);

        for(int i = 0; i < positions.Count; i++)
        {
            placementGrid[positions[i]].occupied = true;
            placementGrid[positions[i]].obj = obj;
        }

        return new Vector3((float)positions[0].x / 2f, -(float)positions[0].y / 2f, -placementGrid[positions[0]].elevation) + new Vector3(- 9.5f, 9.75f, 0); 
        //!This is a magic number, I know. It centers the vase to the position its supposed to be on
    }

    public bool RequestPosition(Vector2 pos, Vector2Int size)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        Vector2Int posInt = (pos * 2).ToV2Int();
        //Transform pos from worldspace to the gridspace, which is about twice as big
        for(int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                positions.Add(new Vector2Int(posInt.x + x, posInt.y + y));
                if(!placementGrid.IsWithinBounds(posInt.x + x, -posInt.y + y) || 
                    placementGrid[posInt.x + x, posInt.y + y].occupied || 
                    placementGrid[posInt.x + x, posInt.y + y].elevation != placementGrid[posInt].elevation)
                {
                    //!if adjacent position is occupied or if the adjacent elevation is different
                    //!then this position is bad, continue while loop
                    positions.Clear();
                }
            }
        }
        for(int i = 0; i < positions.Count; i++)
        {
            placementGrid[positions[i]].occupied = true;
        }
        return positions.Count > 0;
    }

    void SavePlacementGrid(RoomTemplate template)
    {
        placementGrid = new Grid<PlacementGridReference>(new Vector2Int(template.size.x * 2, template.size.y * 2));
        for(int y = 0; y < template.size.y * 2; y++)
        {
            for(int x = 0; x < template.size.x * 2; x++)
            {
                float eq_x = (float)x / 2f;
                float eq_y = (float)y / 2f;
                int index = (int)eq_x + template.size.x * (int)eq_y;
                int elevation = template.positions[index].wall ? 0 : template.positions[index].elevation;
                placementGrid.Add(new PlacementGridReference(null, elevation));
            }
        }
    }

    void SaveTemplateTexture(RoomTemplate template)
    {
        templateTexture = new Texture2D(template.size.x, template.size.y, TextureFormat.ARGB32, false);
        Grid<RoomTemplate.TileTemplate> grid = template.positions.FlipVertically();

        for(int x = 0; x < template.size.x; x++)
        {
            for(int y = 0; y < template.size.y; y++)
            {
                RoomTemplate.TileTemplate temp = grid[x,y];
                Color color = temp.ceilingVertices.Count > 0 ? (Color)new Color32(160, 30, 200, 255): temp.door ? Color.red : temp.read ? debug.wallColor: temp.wall ? Color.white : debug.floorColor;
                templateTexture.SetPixel(x, y, color);
            }
        }
        templateTexture.Apply();
        templateTexture.filterMode = FilterMode.Point;
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
    public void RenderPlacementGrid(Mesh placementSpot, Material mat)
    {
        for(int i = 0; i < placementGrid.items.Count; i++)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Occupied", placementGrid[i].occupied ? Color.red : Color.green);
            Vector3 position = placementGrid.Position(i); //This will get the grid position of the index, not the actual real world position
            //use drawmesh this time for convenience sake

            position += new Vector3(1f / 4f, 1f / 4f, 0);

            position = new Vector3(position.x / 2, -position.y / 2, -placementGrid[i].elevation - 0.5f) + transform.position + new Vector3(- 10, 10, 0);

            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1f / 4f, 1f/4f, 1));

            Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

            if(screenPos.x > 0 && screenPos.x < Camera.main.pixelWidth && screenPos.y > 0 && screenPos.y < Camera.main.pixelHeight)
            {
                Graphics.DrawMesh(placementSpot, matrix, mat, 0, null, 0, block);
            }
        }
    }
}