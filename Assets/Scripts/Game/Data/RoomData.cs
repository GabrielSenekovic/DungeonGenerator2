using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

using RoomTemplate = Room.RoomTemplate;
using Entrances = Room.Entrances;
using System.Diagnostics;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

public enum RoomPosition
{
    None = 0,
    DeadEnd = 1
}
[System.Serializable]
public class RoomData
{
    [System.Serializable]
    public struct RoomDebug
    {
        public Color floorColor;
        public Color wallColor;
    }
    public RoomPosition roomPosition = RoomPosition.None;
    public int stepsAwayFromMainRoom = 0;
    public bool IsBuilt = false;

    public Vector3 position;
    public Vector2Int originalPosition; //Used for debugging
    Entrances directions;
    public int section;

    public Vector2Int size;
    public string name;

    public RoomDebug debug;

    public Texture2D templateTexture;
    public Texture2D mapTexture;

    public void Initialise(Vector2Int roomSize, int section_in, ref List<RoomTemplate> templates, string instructions = "")
    {
        Debug.Log("<color=green>Initializing the Origin Room</color>");
        //This Initialize() function is for the origin room specifically, as it already has its own position
        section = section_in;
        OnInitialize(Vector2Int.zero, roomSize, ref templates, instructions);
        OpenAllEntrances(Vector2Int.zero, new Vector2Int(roomSize.x / 20, roomSize.y / 20));
    }
    public void Initialize(Vector2Int location, Vector2Int roomSize, int section_in, ref List<RoomTemplate> templates)
    {
        DebugLog.AddToMessage("Step", "Initializing with size: " + roomSize);
        //Location here only refers to the gridposition where it connects to its origin room. If it has expanded, we want the transform.position to be the upper left corner
        //That is to say, if size is positive (doesnt point down or right), then it should be pushed by its size
        position = new Vector2(Mathf.Sign(roomSize.x) == 1 ? location.x : location.x + roomSize.x + 20, Mathf.Sign(roomSize.y) == 1 ? location.y + roomSize.y - 20 : location.y);
        section = section_in;
        OnInitialize(new Vector2Int(location.x / 20, location.y / 20), roomSize, ref templates);
    }
    void OnInitialize(Vector2Int gridPosition, Vector2Int roomSize, ref List<RoomTemplate> templates, string instructions = "")
    {
        size = roomSize;
        directions = new Entrances(gridPosition, roomSize / 20, position.ToV2Int());
        Vector2Int absSize = new Vector2Int(Mathf.Abs(size.x), Mathf.Abs(size.y));
        RoomTemplate template = new RoomTemplate(absSize, instructions);
        templates.Add(template);
    }
    public void OpenAllEntrances(Vector2Int gridPosition, Vector2Int roomSize) //Roomsize in grid space
    {
        if (directions == null)
        {
            directions = new Entrances(gridPosition, roomSize, (position / 20).ToV2Int());
        }
        directions.OpenAllEntrances();
    }
    public Entrances GetDirections() => directions;
    public List<Entrances.Entrance> GetEntrances(bool open, bool spawned)
    {
        DebugLog.AddToMessage("Substep", "Getting open unspawned entrances");
        List<Entrances.Entrance> openEntrances = new List<Entrances.Entrance> { };
        foreach (Entrances.Entrance entrance in directions.entrances)
        {
            if (open == entrance.open && spawned == entrance.spawned) //open !spawned
            {
                openEntrances.Add(entrance);
            }
        }
        return openEntrances;
    }
    public Texture2D CreateMaps(ref RoomTemplate template)
    {
        mapTexture = template.CreateMap();
        SaveTemplateTexture(template);
        return mapTexture;
    }
    void SaveTemplateTexture(RoomTemplate template)
    {
        templateTexture = new Texture2D(template.size.x, template.size.y, TextureFormat.ARGB32, false);
        Grid<RoomTemplate.TileTemplate> grid = template.positions.FlipVertically();

        for (int x = 0; x < template.size.x; x++)
        {
            for (int y = 0; y < template.size.y; y++)
            {
                RoomTemplate.TileTemplate temp = grid[x, y];
                Color color = /*temp.startVertices.Count > 0 ? Color.white: temp.endVertices.Count > 0? Color.black : temp.ceilingVertices.Count > 0 ? (Color)new Color32(160, 30, 200, 255):*/
                    temp.door ? Color.red :
                    temp.error ? Color.green :
                    temp.read == RoomTemplate.TileTemplate.ReadValue.FINISHED ? debug.wallColor :
                    temp.read == RoomTemplate.TileTemplate.ReadValue.READFIRST ? Color.magenta :
                    temp.wall ? Color.white : debug.floorColor;
                templateTexture.SetPixel(x, y, color);
            }
        }
        templateTexture.Apply();
        templateTexture.filterMode = FilterMode.Point;
    }
}
