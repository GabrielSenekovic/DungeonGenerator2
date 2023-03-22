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
            //BloatWallCrossings();
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
                while (positions.IsWithinBounds(currentPosition[i] - entrance.dir)) //While youre still within bounds
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