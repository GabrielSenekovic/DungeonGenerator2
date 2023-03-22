using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

//Core code
public partial class Room: MonoBehaviour
{
    
    public class EntranceData
    {
        public List<Vector3> leftVertices = new List<Vector3>(); //From inside the room looking towards north door. Left vertices are saved at the end of the left wall, right vertices are saved at the beginning of the right wall;
        public List<Vector3> rightVertices = new List<Vector3>();
    }
    public List<EntranceData> entrances = new List<EntranceData>(); //Save vertices for every single door

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

    public RoomData roomData = new RoomData();
    public Vegetation grass;

    public Vector2 centerPoint; //DEBUGGING

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
        Vector2 radius = new Vector2(Mathf.Abs(roomData.size.x) / 2, -Mathf.Abs(roomData.size.y) / 2);
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

    public Vector2 GetCameraBoundaries()
    {
        return roomData.size;
    }

    public RoomPosition GetRoomPositionType()
    {
        return roomData.roomPosition;
    }

    bool GetIsEndRoom()
    {
        //This gets if the room is an endroom. However, this could be set by having the rooms be endrooms when they spawn, unless they get linked
        //And then set rooms being spawned from as no longer being endrooms
        List<Entrances.Entrance> entrances = new List<Entrances.Entrance> { };
        if(roomData.GetDirections() == null){return false;}
        foreach(Entrances.Entrance entrance in roomData.GetDirections().entrances)
        {
            if(entrance.spawned == true && entrance.open == true)
            {
                entrances.Add(entrance);
            }
        }
        return entrances.Count == 1;
    }
    public bool RequestPositionFromWorldSpace(Vector2 pos, Vector2Int size)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        pos *= 2;
        Vector2Int posInt = new Vector2Int((int)pos.x + 1, (int)pos.y + 1);
        //Transform pos from worldspace to the gridspace, which is about twice as big
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                positions.Add(new Vector2Int(posInt.x + x, posInt.y + y));
                if (!placementGrid.IsWithinBounds(posInt.x + x, -posInt.y + y) ||
                    placementGrid[posInt.x + x, posInt.y + y].occupied ||
                    placementGrid[posInt.x + x, posInt.y + y].elevation != placementGrid[posInt].elevation)
                {
                    //!if adjacent position is occupied or if the adjacent elevation is different
                    //!then this position is bad, continue while loop
                    positions.Clear();
                }
            }
        }
        for (int i = 0; i < positions.Count; i++)
        {
            placementGrid[positions[i]].occupied = true;
        }
        return positions.Count > 0;
    }
    public void DisplayDistance()
    {
        //GetComponentInChildren<Number>().OnDisplayNumber(roomData.stepsAwayFromMainRoom);
    }
    public Color GetColorForRenderPlacementGrid(int i, LevelManager.PlacementRenderMode mode)
    {
        if (placementGrid[i].occupied)
        {
            if (mode == LevelManager.PlacementRenderMode.BUILD)
            {
                return Color.red;
            }
            else
            {
                return Color.black;
            }
        }
        else if (mode == LevelManager.PlacementRenderMode.POSITION)
        {
            return Color.yellow;
        }
        return Color.green;
    }
    public void RenderPlacementGrid(Mesh placementSpot, Material mat, LevelManager.PlacementRenderMode mode)
    {
        for (int i = 0; i < placementGrid.items.Count; i++)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Occupied", GetColorForRenderPlacementGrid(i, mode));
            Vector3 position = placementGrid.Position(i); //This will get the grid position of the index, not the actual real world position
            //use drawmesh this time for convenience sake

            position += new Vector3(1f / 4f, 1f / 4f, 0);

            position = new Vector3(position.x / 2, -position.y / 2, -placementGrid[i].elevation - 0.5f) + transform.position + new Vector3(-10, 10, 0);

            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1f / 4f, 1f / 4f, 1));

            Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

            if (screenPos.x > 0 && screenPos.x < Camera.main.pixelWidth && screenPos.y > 0 && screenPos.y < Camera.main.pixelHeight)
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
        Vector2 radius = new Vector2(Mathf.Abs(roomData.size.x) / 2, -Mathf.Abs(roomData.size.y) / 2);
        Gizmos.DrawWireSphere(centerPoint, radius.magnitude);
    }
}