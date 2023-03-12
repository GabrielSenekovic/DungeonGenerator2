using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
public partial class Room:MonoBehaviour
{
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
            public enum ReadValue
            {
                UNREAD,
                READ,
                FINISHED, //Has been read all the way up
                READFIRST, //The first value of that wall that got read
                READFIRSTFINISHED
            }
            public ReadValue read;
            public bool error;

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
                read = ReadValue.UNREAD;
                divisions = divisions_in;
                wall = false;
                door = false;
                sidesWhereThereIsWall = new List<TileSides>() { new TileSides(Vector2Int.up), new TileSides(Vector2Int.down), new TileSides(Vector2Int.left), new TileSides(Vector2Int.right) };
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
        public int highestElevation;
        public RoomTemplate(Vector2Int size_in, bool indoors_in, bool surrounding_in, string instructions = "")
        {
            size = size_in;
            positions = new Grid<TileTemplate>(size_in);
            indoors = indoors_in;
            surrounding = surrounding_in;
            CreateRoomTemplate(instructions);
        }
        void CreateRoomTemplate(string instructions)
        {
            //In here it will be determined if the room is a circle, if it is a corridor, etc
            Vector2 roomCenter = new Vector2(size.x / 2, size.y / 2);
            Vector2 wallThickness = new Vector2(UnityEngine.Random.Range(size.x / 2 - 4, size.x / 2), UnityEngine.Random.Range(size.y / 2 - 4, size.y / 2));
            highestElevation = 2;
            if (instructions == "")
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        Vector2Int divisions = new Vector2Int(1, 1); //1,1
                        if (!indoors) { divisions = new Vector2Int(2, 2); }
                        int elevation = surrounding ? 4 : 0;
                        positions.Add(new TileTemplate(elevation, divisions));
                        if (!surrounding)
                        {
                            CreateRoomTemplate_Square(new Vector2(4, 4), x, y, 1); //?Basic thickness. Can't be thinner than 2
                            CreateRoomTemplate_Square(new Vector2(2, 2), x, y, 2); //?Basic thickness. Can't be thinner than 2
                                                                                   //CreateRoomTemplate_Circle(roomCenter, new Vector2(2,2), x, y);
                                                                                   //if (!indoors) { CreateRoomTemplate_Circle(roomCenter, wallThickness, x, y); }
                                                                                   //CreateRoomTemplate_Cross(new Vector2(9,9), x, y, 4);
                        }
                    }
                }
            }
            else
            {
                ParseInstructions(instructions);
            }
            //SmoothenOut();
        }
        void ParseInstructions(string instructions)
        {
            Vector2 roomCenter = new Vector2(size.x / 2, size.y / 2);
            List<Action<int, int>> actions = new List<Action<int, int>>(); 
            for(int i = 0; i < instructions.Length; i++)
            {
                switch(instructions[i])
                {
                    case 'S': //Create square
                        i++; //Skip the square bracket
                        int width = ParseNumber(instructions, ref i);
                        int elevation = ParseNumber(instructions, ref i);
                        if(elevation > highestElevation) { highestElevation = elevation; }
                        actions.Add((x, y) => CreateRoomTemplate_Square(new Vector2(width, width), x, y, elevation));
                        break;
                    case 'C': //Create cross
                        i++;
                        int width2 = ParseNumber(instructions, ref i);
                        int elevation2 = ParseNumber(instructions, ref i);
                        if (elevation2 > highestElevation) { highestElevation = elevation2; }
                        actions.Add((x, y) => CreateRoomTemplate_Cross(new Vector2(width2, width2), x, y, elevation2));
                        break;
                    case 'W': //Create circle
                        i++;
                        int width3 = ParseNumber(instructions, ref i);
                        int elevation3 = ParseNumber(instructions, ref i);
                        if (elevation3 > highestElevation) { highestElevation = elevation3; }
                        actions.Add((x, y) => CreateRoomTemplate_Circle(roomCenter, new Vector2(width3, width3), x, y, elevation3));
                        break;
                }
            }
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for(int i = 0; i < actions.Count; i++)
                    {
                        Vector2Int divisions = new Vector2Int(2, 2); //1,1
                        positions.Add(new TileTemplate(0, divisions));
                        actions[i](x, y);
                    }
                }
            }
        }
        int ParseNumber(string instructions, ref int index)
        {
            string number = "";
            index++;
            while (index < instructions.Count() && char.IsDigit(instructions[index]))
            {
                number += instructions[index];
                index++;
            }
            int.TryParse(number, out int result);
            return result;
        }
        void SmoothenOut()
        {
            //Push up
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int amountOfWallNeighbors = 0;
                    int elevation = positions[x, y].elevation;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (i == 0 && j == 0) { continue; }
                            if (IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if (positions[x + i, y + -j].elevation != positions[x, y].elevation) //If the position is a wall
                                {
                                    if (positions[x + i, y + -j].elevation > positions[x, y].elevation)
                                    {
                                        elevation = positions[x + i, y + -j].elevation;
                                    }
                                    amountOfWallNeighbors++; //Then count up
                                }
                            }
                        }
                    }
                    if (amountOfWallNeighbors > 5)
                    {
                        positions[x, y].elevation = elevation;
                    }
                }
            }
            //Push down
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int amountOfWallNeighbors = 0;
                    int elevation = positions[x, y].elevation;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (i == 0 && j == 0) { continue; }
                            if (IsPositionWithinBounds(new Vector2Int(x + i, -y + j)))
                            {
                                if (positions[x + i, y + -j].elevation != positions[x, y].elevation) //If the position is a wall
                                {
                                    if (positions[x + i, y + -j].elevation < positions[x, y].elevation)
                                    {
                                        elevation = positions[x + i, y + -j].elevation;
                                    }
                                    amountOfWallNeighbors++; //Then count up
                                }
                            }
                        }
                    }
                    if (amountOfWallNeighbors > 5)
                    {
                        positions[x, y].elevation = elevation;
                    }
                }
            }
        }
        void CreateRoomTemplate_Circle(Vector2 center, Vector2 wallThickness, int x, int y, int elevation)
        {
            float distanceToCenter = new Vector2(x + 0.5f - center.x, y + 0.5f - center.y).magnitude;
            if (distanceToCenter > wallThickness.x && 
                distanceToCenter > wallThickness.y && 
                elevation > positions[x + (int)size.x * y].elevation)
            {
                //if the higher limit is 0, then the code just generates a circle, period
                int temp = UnityEngine.Random.Range(0, 0);
                if (temp == 0)
                {
                    positions[x + (int)size.x * y].elevation = elevation;
                }
            }
        }
        void CreateRoomTemplate_Square(Vector2 wallThickness, int x, int y, int elevation)
        {
            if (x < wallThickness.x || x > size.x - wallThickness.x - 1 ||
               y < wallThickness.y || y > size.y - wallThickness.y - 1)
            {
                positions[x + size.x * y].elevation = elevation;
            }
        }
        void CreateRoomTemplate_Cross(Vector2 wallThickness, int x, int y, int elevation)
        {
            if ((x < wallThickness.x || x > size.x - wallThickness.x - 1) &&
               (y < wallThickness.y || y > size.y - wallThickness.y - 1))
            {
                positions[x + (int)size.x * y].elevation = elevation;
            }
        }

        public void AddEntrancesToRoom(Entrances entrances)
        {
            for (int i = 0; i < entrances.entrances.Count; i++)
            {
                if (entrances.entrances[i].spawned && entrances.entrances[i].open)
                {
                    for (int j = 0; j < entrances.entrances[i].positions.Count; j++)
                    {
                        int x = entrances.entrances[i].positions[j].x;
                        int y = entrances.entrances[i].positions[j].y;
                        Debug.Log("Adding entrances to room X:" + x + " Y: " + y + " when the room has " + size);
                        positions[x + size.x * y].door = true; //Turn the position into a door
                        Debug.Log("Success");
                        EnsureEntranceReachability(entrances.entrances[i]);
                    }
                }
            }
            IdentifyWalls();
            //UnscatterWalls();
            BloatWallCrossings();
        }
        void IdentifyWalls()
        {
            //! this function goes through all positions to identify which positions are where walls are supposed to be
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int[] constraints = positions.GetValidConstraints(x, y);
                    for (int x_w = constraints[0]; x_w < constraints[2]; x_w++)
                    {
                        for (int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                        {
                            //If this position has one adjacent position that is a higher elevation from itself, then it is a wall
                            if (positions[x_w, y_w].elevation > positions[x, y].elevation)
                            {
                                positions[x, y].wall = true;
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
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (positions[x, y].wall)
                    {
                        OnUnscatterWalls(x, y);
                    }
                }
            }
        }
        void OnUnscatterWalls(int x, int y)
        {
            int[] constraints = positions.GetValidConstraints(x, y);
            int lowestAdjacentElevation = positions[x, y].elevation;
            for (int x_w = constraints[0]; x_w < constraints[2]; x_w++)
            {
                for (int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                {
                    if (positions[x_w, y_w].elevation < lowestAdjacentElevation) { lowestAdjacentElevation = positions[x_w, y_w].elevation; }

                    if (positions[x_w, y_w].elevation == positions[x, y].elevation &&
                       !positions[x_w, y_w].wall) //if there is one adjacent position has the same elevation and isn't a wall, then this is indeed a wall
                    {
                        return; //This is indeed a wall, keep
                    }
                }
            }
            positions[x, y].wall = false;
            positions[x, y].elevation = lowestAdjacentElevation;
            //No this is a fluke, delete
        }

        void BloatWallCrossings()
        {
            bool bloat = true;
            while (bloat)
            {
                bloat = false;
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        if (positions[x, y].wall)
                        {
                            //Debug.Log("Origin: " + x + " " + y);
                            int[] constraints = positions.GetValidConstraints(x, y);
                            int adjWalls = 0;
                            for (int x_w = constraints[0]; x_w < constraints[2]; x_w++)
                            {
                                for (int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                                {
                                    //Count walls
                                    if ((x_w == x + 1 && y_w == y + 1) || (x_w == x + 1 && y_w == y - 1) || (x_w == x - 1 && y_w == y + 1) || (x_w == x - 1 && y_w == y - 1)) { continue; }
                                    if (positions[x_w, y_w].wall && positions[x_w, y_w].elevation == positions[x, y].elevation)
                                    {
                                        // Debug.Log("Adding: " + x_w + " " + y_w);
                                        adjWalls++;
                                    }
                                }
                            }
                            if (adjWalls > 3)
                            {
                                bloat = true;
                                // Debug.Log("BLOATING IDENTIFIED");
                                //! then this is a wall crossing. Those must be bloated
                                constraints = positions.GetValidConstraints(x, y, 2);
                                for (int x_w = constraints[0]; x_w < constraints[2]; x_w++)
                                {
                                    for (int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                                    {
                                        //Count walls
                                        positions[x_w, y_w].wall = false;
                                        positions[x_w, y_w].elevation = positions[x, y].elevation;
                                    }
                                }
                                for (int x_w = constraints[0]; x_w < constraints[2]; x_w++) //Go around the original position
                                {
                                    for (int y_w = constraints[1]; y_w < constraints[3]; y_w++)
                                    {
                                        int[] constraintsTwo = positions.GetValidConstraints(x_w, y_w);
                                        for (int x_ww = constraintsTwo[0]; x_ww < constraintsTwo[2]; x_ww++) //Go around each adjacent position
                                        {
                                            for (int y_ww = constraintsTwo[1]; y_ww < constraintsTwo[3]; y_ww++)
                                            {
                                                if (positions[x_ww, y_ww].elevation < positions[x_w, y_w].elevation)
                                                {
                                                    positions[x_w, y_w].wall = true;
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
            if (pos.x >= 0 && pos.x < size.x && pos.y <= 0 && pos.y > -size.y)
            {
                return true;
            }
            // Debug.Log(pos + " <color=red>is not within bounds</color>");
            return false;
        }
        public void HasWallNeighbor(Vector2Int pos, int rotation, int currentElevation, out bool hasWallNeighbor, 
            out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn)
        {
            /*HasWallNeighbor
             * 
             * It checks first the angle of rotation
             * Then it checks both turning to right or left, if those positions are valid
             * And if those positions are unread and the correct elevation
             * Then the direction and rotation gets set and the method is exited
             * It is also used specifically for turning around
             * 
             * On an inner corner (1), don't check the position next to the next for a wall, rather, check the adjacent position of where you are
             * Othwerwise, if the corner is just one step, it will overshoot, and not make a wall
            */
            Vector2Int direction = Vector2Int.zero;
            bool value = true;
            int rotationDir = 0;
            higherElevationDirection = new Vector2Int(0, 0);
            if (rotation == 270)
            {
                bool bool1 = positions[pos.x + 1, pos.y].wall;
                bool bool2 = positions[pos.x + 1, pos.y].read == TileTemplate.ReadValue.UNREAD;
                bool bool3 = positions[pos.x, -pos.y - 1].elevation > currentElevation;
                bool bool4 = positions[pos.x, -pos.y - 1].elevation > positions[pos.x + 1, pos.y].elevation;
                if (IsPositionWithinBounds(new Vector2Int(pos.x + 1, pos.y)) && 
                    positions[pos.x + 1, pos.y].wall &&
                    positions[pos.x + 1, pos.y].elevation <= currentElevation &&
                    positions[pos.x, -pos.y - 1].elevation > currentElevation &&
                    positions[pos.x, -pos.y - 1].elevation > positions[pos.x + 1, pos.y].elevation
                    )
                {
                    direction = new Vector2Int(1, 0);
                    rotationDir = 1;
                    higherElevationDirection = new Vector2Int(0, -1);
                }
                else if (IsPositionWithinBounds(new Vector2Int(pos.x - 1, pos.y)) && 
                    positions[pos.x - 1, pos.y].wall && 
                    positions[pos.x - 1, pos.y].read == TileTemplate.ReadValue.UNREAD && 
                    positions[pos.x - 1, pos.y - 1].elevation > currentElevation &&
                    positions[pos.x - 1, pos.y - 1].elevation > positions[pos.x - 1, pos.y].elevation
                    )
                {
                    direction = new Vector2Int(-1, 0);
                    rotationDir = -1;
                    higherElevationDirection = new Vector2Int(0, 1);
                }
                else
                {
                    value = false;
                }
            }
            else if (rotation == 90)
            {
                if (IsPositionWithinBounds(new Vector2Int(pos.x + 1, pos.y)) && 
                    positions[pos.x + 1, -pos.y].wall && 
                    positions[pos.x + 1, -pos.y].read == TileTemplate.ReadValue.UNREAD && 
                    positions[pos.x + 1, -pos.y - 1].elevation > currentElevation &&
                    positions[pos.x + 1, -pos.y - 1].elevation > positions[pos.x + 1, -pos.y].elevation
                    )
                {
                    direction = new Vector2Int(1, 0);
                    rotationDir = -1;
                    higherElevationDirection = new Vector2Int(0, -1);
                }
                else if (IsPositionWithinBounds(new Vector2Int(pos.x - 1, pos.y)) && 
                    positions[pos.x - 1, pos.y].wall && 
                    positions[pos.x - 1, pos.y].elevation <= currentElevation &&
                    positions[pos.x, pos.y - 1].elevation > currentElevation &&
                    positions[pos.x, pos.y - 1].elevation > positions[pos.x - 1, pos.y].elevation
                    )
                {
                    direction = new Vector2Int(-1, 0);
                    rotationDir = 1;
                    higherElevationDirection = new Vector2Int(0, 1);
                }
                else
                {
                    value = false;
                }
            }
            else if (rotation == 180)
            {
                if (IsPositionWithinBounds(new Vector2Int(pos.x, pos.y + 1)) &&
                    positions[pos.x, pos.y + 1].wall &&
                    positions[pos.x, pos.y + 1].elevation <= currentElevation &&
                    //positions[pos.x, pos.y + 1].read == TileTemplate.ReadValue.UNREAD && 
                    positions[pos.x - 1, pos.y].elevation > currentElevation &&
                    positions[pos.x - 1, pos.y].elevation > positions[pos.x, pos.y + 1].elevation
                    )
                {
                    direction = new Vector2Int(0, -1);
                    rotationDir = 1;
                    higherElevationDirection = new Vector2Int(-1, 0);
                }
                else if (IsPositionWithinBounds(new Vector2Int(pos.x, pos.y - 1)) && 
                    positions[pos.x, (-pos.y + 1)].wall && 
                    positions[pos.x, (-pos.y + 1)].read == TileTemplate.ReadValue.UNREAD && 
                    positions[pos.x + 1, (-pos.y + 1)].elevation > currentElevation &&
                    positions[pos.x + 1, (-pos.y + 1)].elevation > positions[pos.x, (-pos.y - 1)].elevation
                    )
                {
                    direction = new Vector2Int(0, 1);
                    rotationDir = -1;
                    higherElevationDirection = new Vector2Int(1, 0);
                }
                else
                {
                    value = false;
                }
            }
            else if (rotation == 0)
            {
                if (IsPositionWithinBounds(new Vector2Int(pos.x, pos.y + 1)) && 
                    positions[pos.x, pos.y + 1].wall && 
                    positions[pos.x, pos.y + 1].read == TileTemplate.ReadValue.UNREAD && 
                    positions[pos.x - 1, pos.y + 1].elevation > currentElevation &&
                    positions[pos.x - 1, pos.y + 1].elevation > positions[pos.x, pos.y + 1].elevation
                    )
                {
                    direction = new Vector2Int(0, -1);
                    rotationDir = -1;
                    higherElevationDirection = new Vector2Int(-1, 0);
                }
                else if (IsPositionWithinBounds(new Vector2Int(pos.x, pos.y - 1)) && 
                    positions[pos.x, pos.y - 1].wall &&
                    positions[pos.x, pos.y - 1].elevation <= currentElevation &&
                    //positions[pos.x, pos.y - 1].read == TileTemplate.ReadValue.UNREAD && 
                    positions[pos.x + 1, pos.y].elevation > currentElevation &&
                    positions[pos.x + 1, pos.y].elevation > positions[pos.x, pos.y].elevation
                    )
                {
                    direction = new Vector2Int(0, 1);
                    rotationDir = 1;
                    higherElevationDirection = new Vector2Int(1, 0);
                }
                else
                {
                    value = false;
                }
            }
            if(!value)
            {
                positions[pos.x, pos.y].error = true;
            }
            hasWallNeighbor = value;
            neighborDirection = direction;
            angleToTurn = rotationDir;
        }

        public List<Tuple<List<MeshMaker.WallData>, bool>> ExtractWalls(Entrances entrances)
        {
            // Debug.Log("<color=green> NEW ROOM</color>");
            List<Tuple<List<MeshMaker.WallData>, bool>> data = new List<Tuple<List<MeshMaker.WallData>, bool>>();
            Vector2Int pos = new Vector2Int(-1, -1);
            int currentAngle = 0;

            //Make one that works for all purposes
            int currentElevation = 0;

            if (entrances != null && entrances.entrances.Count > 0)
            {
                //  Debug.Log("<color=green> NEW WALL</color>");
                for (int i = 0; i < entrances.entrances.Count; i++) //! makes the code only work when theres a door
                {
                    /*----->*/entrances.entrances[i].spawned = true; //<-------REMOVE THIS LATER
                    if (!entrances.entrances[i].open || !entrances.entrances[i].spawned) { continue; }
                    //Go through each entrance, and make a wall to its left. There will only ever be as many walls as there are entrances kappa
                    //Find a wall that has a floor next to it
                    //Debug.Log("Extracting walls");
                    Debug.Log("New entrance wall: " + i + " at position: " + entrances.entrances[i].positions[0]);
                    pos = new Vector2Int(-1, -1);
                    currentAngle = 0; //Current angle should only be 0 if the floor found points down.

                    ExtractWalls_GetStartPosition(ref pos, ref currentAngle, entrances.entrances[i]);
                    Debug.Log("I got start position: " + pos);
                    OnExtractWalls(ref currentAngle, ref pos, ref data, currentElevation);
                    Debug.Log("I got: " + data[data.Count - 1].Item1.Count);
                }
            }
            // else //If this is a closed room without doors
            while (currentElevation < highestElevation)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        //Check if there is floor diagonally right down, because walls can only be drawn from left to right
                        //If there are none, rotate the search three times. If there still are none, then there is an error
                        bool isUnread = positions[x, y].read == TileTemplate.ReadValue.UNREAD;
                        bool isNextToWall = positions[x, y - 1].elevation > positions[x, y].elevation;
                        bool elevationLowerThanCurrent = positions[x, y].elevation <= currentElevation;
                        if (IsPositionWithinBounds(new Vector2Int(x, -y + 1)) &&
                            positions[x, y].wall &&
                            positions[x, y].read == TileTemplate.ReadValue.UNREAD &&
                            positions[x, y - 1].elevation > positions[x, y].elevation && 
                            positions[x, y].elevation <= currentElevation)
                        {
                            pos = new Vector2Int(x, -y);
                            currentAngle = 270;
                            OnExtractWalls(ref currentAngle, ref pos, ref data, currentElevation);
                        }
                    }
                }
                currentElevation++;
                ResetReadValue(currentElevation);
            }
            
            return data;
        }
        void OnExtractWalls(ref int currentAngle, ref Vector2Int pos, ref List<Tuple<List<MeshMaker.WallData>, bool>> data, int currentElevation)
        {
            //Find direction to follow
            Tuple<List<MeshMaker.WallData>, bool> wall = new Tuple<List<MeshMaker.WallData>, bool>(new List<MeshMaker.WallData>(), false);
       
            HasWallNeighbor(pos, currentAngle, currentElevation, out bool hasWallNeighbor, out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn); //Item2 is the direction to go to
            Debug.Log("Did I find wall neighbor? " + hasWallNeighbor);
            currentAngle += 90 * angleToTurn;
            currentAngle = (int)Math.Mod(currentAngle, 360);
            positions[pos.x, pos.y].read = TileTemplate.ReadValue.READFIRST;
            Vector2Int startPosition = pos;

            int safety = 0;
            while (hasWallNeighbor) //If there is a wall neighbor, proceed
            {
                safety++;
                if(safety > 100) 
                {
                    Debug.Log("SAFETY YEET");
                    return; 
                }
                startPosition = pos;
                //Follow that direction until its empty
                int steps = 1;
                if (angleToTurn == -1)
                {
                    pos = new Vector2Int(pos.x + neighborDirection.x, pos.y - neighborDirection.y);
                }
                ExtractWalls_GetSteps(ref pos, ref steps, neighborDirection, higherElevationDirection, currentAngle, currentElevation);

                int isThisWallFollowingOuterCorner = 0;
                if (angleToTurn < 0 && wall.Item1.Count > 0)
                {
                    //If lastWall is less than 0, then this is the following wall after an outer corner, so it must be moved up and shortened
                    isThisWallFollowingOuterCorner = 1;
                }
                HasWallNeighbor(pos, currentAngle, currentElevation, out hasWallNeighbor, out neighborDirection, out higherElevationDirection, out angleToTurn);

                float roundedness = indoors ? 0 : UnityEngine.Random.Range(0.0f, 1.0f);

                if (angleToTurn < 0)
                {
                    //If Item3 is less than 0, then this is an outer corner, so the wall shouldn't go the whole way
                    steps--;
                    if (!indoors)
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

                if (currentAngle == 0)
                {
                    Debug.Log("adding 0 degree wall");
                    wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f + isThisWallFollowingOuterCorner, startPosition.y, 0), startPosition,
                    -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
                }
                if (currentAngle == 90)
                {
                    Debug.Log("adding 90 degree wall");
                    wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f, startPosition.y - isThisWallFollowingOuterCorner, 0), startPosition,
                    -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
                }
                if (currentAngle == 180)
                {
                    Debug.Log("adding 180 degree wall");
                    wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + 0.5f, startPosition.y - 1, 0), startPosition,
                    -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
                }
                if (currentAngle == 270)
                {
                    Debug.Log("adding 270 degree wall");
                    wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f, startPosition.y - 1 + isThisWallFollowingOuterCorner, 0), startPosition,
                    -currentAngle, steps , currentElevation, 0, positions[pos].divisions, curve, roundedness));
                }
                //Sometimes it has to decrease by 90, so it has to know what direction the next wall goes in (fuck)
                currentAngle += 90 * angleToTurn; //This code can only do inner corners atm, not outer corners
                currentAngle = (int)Math.Mod(currentAngle, 360);
                if (positions[pos].read == TileTemplate.ReadValue.READFIRSTFINISHED ||
                    positions[pos].read == TileTemplate.ReadValue.READFIRST)
                { break; }
            }
            // Debug.Log("There is this amount of walls: " + wall.Item1.Count);
            wall = new Tuple<List<MeshMaker.WallData>, bool>(wall.Item1, ExtractWalls_DoesWallWrap(wall.Item1));
            data.Add(wall);
            //if(wall.Item1.Count == 0){DebugLog.WarningMessage("Couldn't create any walls");}
        }
        void ResetReadValue(int currentElevation)
        {
            //Reset all elevations that are the same or lower than currentElevation
            positions.items.Where(i => i.elevation <= currentElevation &&
            i.read == TileTemplate.ReadValue.READFIRSTFINISHED
            ).ToList().ForEach(i => i.read = TileTemplate.ReadValue.FINISHED);

            positions.items.Where(i => i.elevation <= currentElevation && 
            i.read != TileTemplate.ReadValue.FINISHED
            ).ToList().ForEach(i => i.read = TileTemplate.ReadValue.UNREAD);
            //Do this between each elevation
        }
        bool ExtractWalls_DoesWallWrap(List<MeshMaker.WallData> data)
        {
            return true;
        }

        void ExtractWalls_GetStartPosition(ref Vector2Int pos, ref int currentAngle, Entrances.Entrance entrance)
        {
            /* Get Start Position
             * 
             * It takes an entrance and then attempts to make a wall from there
             */
            //Get the position that is to the left of the given entrance
            if (entrance.dir == new Vector2Int(0, 1)) //north
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x + 1, -entrance.positions[entrance.positions.Count - 1].y);
                currentAngle = 0;
            }
            if (entrance.dir == new Vector2Int(0, -1)) //south
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x - 1, -entrance.positions[entrance.positions.Count - 1].y);
                currentAngle = 180;
            }
            if (entrance.dir == new Vector2Int(1, 0)) //right
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x, -entrance.positions[entrance.positions.Count - 1].y - 1);
                currentAngle = 270;
            }
            if (entrance.dir == new Vector2Int(-1, 0)) //left
            {
                pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x, -entrance.positions[entrance.positions.Count - 1].y + 1);
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
        void ExtractWalls_GetSteps(ref Vector2Int pos, ref int steps, Vector2Int direction, Vector2Int directionOfHigherElevation, int currentAngle, int currentElevation)
        {
            /* Get Steps
             * 
             * This function is supposed to keep going in a given direction
             * Checking that each step is the same
             * 
             * */
            while (
                IsPositionWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) && 
                positions[pos.x + direction.x, pos.y - direction.y].wall &&
                positions[pos.x + direction.x, pos.y - direction.y].elevation <= currentElevation &&
                positions[pos.x + directionOfHigherElevation.x, pos.y - directionOfHigherElevation.y].elevation > currentElevation
                )
            {
                steps++;
                pos = new Vector2Int(pos.x + direction.x, pos.y - direction.y);
                // Debug.Log("Checking index: " + (pos.x + size.x * -pos.y));
                if (positions[pos].read == TileTemplate.ReadValue.READFIRSTFINISHED ||
                    positions[pos].read == TileTemplate.ReadValue.READFIRST)
                { 
                    break; 
                }
                if (positions[pos.x + directionOfHigherElevation.x, -pos.y + directionOfHigherElevation.y].elevation == currentElevation + 1)
                {
                    if(positions[pos].read == TileTemplate.ReadValue.READFIRST)
                    {
                        positions[pos].read = TileTemplate.ReadValue.READFIRSTFINISHED;
                    }
                    else
                    {
                        positions[pos.x, -pos.y].read = TileTemplate.ReadValue.FINISHED;
                    }
                }
                else
                {
                    positions[pos.x, -pos.y].read = TileTemplate.ReadValue.READ;
                }
            }
        }
        bool SharesFloor(Vector2Int pos, Vector2Int direction)
        {
            //!If there is a wall ahead in that direction, check if they share a floor. Otherwise, dont go to it

            if (IsPositionWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) && positions[pos.x + direction.x, -pos.y + direction.y].wall)
            {
                List<Vector2Int> temp = new List<Vector2Int>();
                int[] constraints = positions.GetValidConstraints(pos.x, -pos.y);

                for (int x = constraints[0]; x < constraints[2]; x++)
                {
                    for (int y = constraints[1]; y < constraints[3]; y++)
                    {
                        if (positions[x, y].elevation < positions[pos.x, -pos.y].elevation)
                        {
                            temp.Add(new Vector2Int(x, y));
                        }
                    }
                }

                constraints = positions.GetValidConstraints(pos.x + direction.x, -pos.y + direction.y);
                for (int x = constraints[0]; x < constraints[2]; x++)
                {
                    for (int y = constraints[1]; y < constraints[3]; y++)
                    {
                        for (int i = 0; i < temp.Count; i++)
                        {
                            if (temp[i] == new Vector2Int(x, y))
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
        bool ExtractWalls_CheckHasEnoughWallsAhead(Vector2Int pos, Vector2Int direction, int currentAngle, int currentElevation)
        {
            if (IsPositionWithinBounds(new Vector2Int(pos.x + direction.x * 2, pos.y - direction.y * 2)) && 
                positions[pos.x + direction.x * 2, -pos.y + direction.y * 2].wall
                )
            //Check if the next position after the next isn't a wall, cuz then the next position is the last
            //This if statement runs if the next after the next position is the last
            {
                HasWallNeighbor(pos, currentAngle, currentElevation, out bool hasWallNeighbor, out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn); 
                //Check if it's an outer corner, cuz only then should the next check happen

                if (angleToTurn == -1) //If is an outer corner
                {
                    Vector2 temp = (Quaternion.Euler(0, 0, -90) * (Vector2)direction);
                    Vector2Int rotatedDirection = new Vector2Int(Mathf.RoundToInt(temp.x), Mathf.RoundToInt(temp.y));
                    if (IsPositionWithinBounds(new Vector2Int(pos.x + direction.x + rotatedDirection.x * 2, pos.y - direction.y - rotatedDirection.y * 2)) && 
                        positions[pos.x + direction.x + rotatedDirection.x * 2, -pos.y + direction.y + rotatedDirection.y * 2].wall)
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
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    //int elevation = positions[x, y].wall ? 0 : positions[x, y].elevation;
                    int elevation = positions[x, y].elevation;
                    returnData.Add(new MeshMaker.SurfaceData(new Vector3Int(x, -y - 1, elevation), positions[x, y].ceilingVertices, positions[x, y].floorVertices, positions[x, y].divisions.x, positions[x, y].sidesWhereThereIsWall));
                }
            }
            return returnData;
        }
        public List<TileTemplate> GetEntranceTiles()
        {
            List<TileTemplate> tiles = new List<TileTemplate>();
            for (int i = 0; i < positions.Count(); i++)
            {
                if (positions[i].door)
                {
                    tiles.Add(positions[i]);
                }
            }
            return tiles;
        }
        public TileTemplate GetEntranceTile(Vector2Int position)
        {
            return positions[Mathf.Abs(position.x), Mathf.Abs(position.y)];
        }
        public void SetEntranceTileVertices(List<Vector3> vertices, Vector2Int position, Vector3 originRoomPosition, Vector3 destinationRoomPosition, bool start) //The entrance of this room
        {
            if (start)
            {
                positions[position].startVertices = vertices;
            }
            else
            {
                if (positions[position].endVertices.Count > 0)
                {
                    Debug.Log("Tried to add when there is already data");
                    return;
                }
                Debug.Log("Setting end vertices here with: " + vertices.Count + " vertices");
                Vector3 adjustmentVector = originRoomPosition - destinationRoomPosition;
                for (int i = 0; i < vertices.Count; i++)
                {
                    positions[position].endVertices.Add(vertices[i] + adjustmentVector);
                }
            }
        }
        void EnsureEntranceReachability(Entrances.Entrance entrance)
        {
            List<Vector2Int> currentPosition = new List<Vector2Int>();
            for (int i = 0; i < entrance.positions.Count; i++)
            {
                currentPosition.Add(new Vector2Int(entrance.positions[i].x, -entrance.positions[i].y));
                positions[entrance.positions[i]].elevation = 0;
            }

            for (int i = 0; i < currentPosition.Count; i++)
            {
                while (IsPositionWithinBounds(currentPosition[i] - entrance.dir)) //While youre still within bounds
                {
                    currentPosition[i] = currentPosition[i] - entrance.dir;
                    if (positions[currentPosition[i].x, -currentPosition[i].y].elevation > positions[entrance.positions[0]].elevation) //If there is a wall in the direction of this door
                    {
                        positions[currentPosition[i].x, -currentPosition[i].y].elevation = 0;
                    }
                    else if (positions[currentPosition[i].x, -currentPosition[i].y].elevation == 0) //If youve reached a floor, stop
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
            for (int i = 0; i < size.x * size.y; i++)
            {
                float lumValue = (float)grid[i].elevation / 20f + 0.5f;
                colors[i] = Color.HSVToRGB(0.3f, 1, lumValue);
            }
            tex.Finish(colors);
            return tex;
        }
    }
}