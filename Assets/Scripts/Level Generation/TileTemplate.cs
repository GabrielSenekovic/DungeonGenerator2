using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public enum TileType
    {
        NONE = 0,
        OUTSIDE_WALL = 1, //Outdoor wall
        HOUSE_WALL = 2, //So create the walls on the outer edges
        HOUSE_FLOOR = 3
    }
    public ReadValue read;
    public TileType tileType;
    public bool error;
    public string ID;

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
        ID = "";
    }
    public void SetID(string ID)
    {
        this.ID = ID;
    }
    public void SetElevation(int newElevation)
    {
        elevation = newElevation;
    }
}