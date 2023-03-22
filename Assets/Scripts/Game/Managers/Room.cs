using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
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
    public Vector2Int originalPosition; //Used for debugging

    public RoomData roomData = new RoomData();

    public RoomDebug debug;
    public Texture2D templateTexture;
    public Texture2D mapTexture;
    public int section;
    public Vegetation grass;

    public Vector2 centerPoint; //DEBUGGING

    public void OpenAllEntrances(Vector2Int gridPosition, Vector2Int roomSize) //Roomsize in grid space
    {
        if(directions == null)
        {
            directions = new Entrances(gridPosition, roomSize, (transform.position / 20).ToV2Int());
        }
        directions.OpenAllEntrances();
    }
    public void Initialize(Vector2Int roomSize, bool indoors, int section_in, ref List<RoomTemplate> templates, bool surrounding, string instructions = "")
    {
        Debug.Log("<color=green>Initializing the Origin Room</color>");
        //This Initialize() function is for the origin room specifically, as it already has its own position
        section = section_in;
        OnInitialize(Vector2Int.zero, roomSize, indoors, ref templates, surrounding, instructions);
        OpenAllEntrances(Vector2Int.zero, new Vector2Int(roomSize.x / 20, roomSize.y / 20));
    }

    public void Initialize(Vector2Int location, Vector2Int roomSize,  bool indoors, int section_in, ref List<RoomTemplate> templates, bool surrounding)
    {
        DebugLog.AddToMessage("Step", "Initializing with size: " + roomSize);
        //Location here only refers to the gridposition where it connects to its origin room. If it has expanded, we want the transform.position to be the upper left corner
        //That is to say, if size is positive (doesnt point down or right), then it should be pushed by its size
        transform.position = new Vector2(Mathf.Sign(roomSize.x) == 1 ? location.x: location.x + roomSize.x + 20, Mathf.Sign(roomSize.y) == 1 ? location.y + roomSize.y - 20: location.y);
        section = section_in;
        OnInitialize(new Vector2Int(location.x / 20, location.y / 20), roomSize, indoors, ref templates, surrounding);
    }
    void OnInitialize(Vector2Int gridPosition, Vector2Int roomSize, bool indoors, ref List<RoomTemplate> templates, bool surrounding, string instructions = "") 
    {
        size = roomSize;
        directions = new Entrances(gridPosition, roomSize / 20, transform.position.ToV2Int());
        Vector2Int absSize = new Vector2Int(Mathf.Abs(size.x), Mathf.Abs(size.y));
        RoomTemplate template = new RoomTemplate(absSize, indoors, surrounding, instructions);
        templates.Add(template);
    }
    public Texture2D CreateMaps(ref RoomTemplate template)
    {
        mapTexture = template.CreateMap();
        SaveTemplateTexture(template);
        return mapTexture;
    }
    public void CreateRoom(ref RoomTemplate template, Material floorMaterial_in)
    {
        Color color = new Color32((byte)UnityEngine.Random.Range(125, 220),(byte)UnityEngine.Random.Range(125, 220),(byte)UnityEngine.Random.Range(125, 220), 255);
        Material furnitureMaterial = new Material(floorMaterial_in.shader);
        furnitureMaterial.CopyPropertiesFromMaterial(floorMaterial_in);
        furnitureMaterial.color = color;
        SavePlacementGrid(template);
        Furnish(furnitureMaterial);
    }

    private void Update() 
    {
        if(!grass){return;}
        Vector2 radius = new Vector2(Mathf.Abs(size.x) / 2, -Mathf.Abs(size.y) / 2);
        centerPoint = transform.position - new Vector3(10,-10, 0) + (Vector3)radius; //10,10 to make the position the corner and then push it to the middle'
        //These variables are for the frustum culling. But I havent gotten those to work yet

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        if(renderers.Any(r => r.isVisible))
        {
            grass.UpdateVegetation();
        }
    }
    private void FixedUpdate() 
    {
        if(!grass){return;}
        grass.FixedUpdateVegetation();
    }

    void Furnish(Material mat)
    {
        DebugLog.AddToMessage("Substep", "Furnishing");
        /*int amountOfVases = UnityEngine.Random.Range(3, 6);
        for(int i = 0; i < amountOfVases; i++)
        {
            GameObject vase = MeshMaker.CreateVase(mat);
            vase.transform.parent = gameObject.transform;
            vase.transform.localPosition = FindRandomPlacementPositionOfSize(vase, new Vector2Int(2,2));
        }*/

        GameObject rock = new GameObject("Rock");
        rock.transform.parent = transform;
        MeshRenderer rend = rock.AddComponent<MeshRenderer>();
        MeshFilter filt = rock.AddComponent<MeshFilter>();
        filt.mesh = MeshMaker.CreateRock();
        Material matz = Resources.Load<Material>("Materials/Stone");
        rend.material = matz;
        rock.AddComponent<SphereCollider>();

        //GameObject chest = new GameObject("Chest");
        //MeshMaker.CreateChest(chest, 0);
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
        pos *= 2;
        Vector2Int posInt = new Vector2Int((int)pos.x + 1, (int)pos.y + 1);
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
    public List<Entrances.Entrance> GetEntrances(bool open, bool spawned)
    {
        DebugLog.AddToMessage("Substep", "Getting open unspawned entrances");
        List<Entrances.Entrance> openEntrances = new List<Entrances.Entrance>{};
        foreach(Entrances.Entrance entrance in directions.entrances)
        {
            if (open == entrance.open && spawned == entrance.spawned) //open !spawned
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
    public void OnReset()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Vector2 radius = new Vector2(Mathf.Abs(size.x) / 2, -Mathf.Abs(size.y) / 2);
        Gizmos.DrawWireSphere(centerPoint, radius.magnitude);
    }
}