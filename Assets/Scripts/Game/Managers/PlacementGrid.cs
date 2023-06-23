using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RoomTemplate = Room.RoomTemplate;
[System.Serializable]
public class PlacementGrid
{
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

    public Grid<PlacementGridReference> grid;
    [SerializeField] float renderDistanceFromPlayer = 4;

    List<Matrix4x4> occupiedSpaces = new List<Matrix4x4>();
    List<Matrix4x4> freeSpaces = new List<Matrix4x4>();

    MaterialPropertyBlock freeBlock = new MaterialPropertyBlock();
    MaterialPropertyBlock occupiedBlock = new MaterialPropertyBlock();

    public PlacementGrid(RoomTemplate template)
    {
        SavePlacementGrid(template);
    }

    void SavePlacementGrid(RoomTemplate template)
    {
        grid = new Grid<PlacementGridReference>(new Vector2Int(template.size.x * 2, template.size.y * 2));
        for (int y = (template.size.y * 2) - 1; y >= 0; y--)
        {
            for (int x = 0; x < template.size.x * 2; x++)
            {
                float eq_x = (float)x / 2f;
                float eq_y = (float)y / 2f;
                int index = (int)eq_x + template.size.x * (int)eq_y;
                int elevation = template.positions[index].elevation;
                grid.Add(new PlacementGridReference(null, elevation));
            }
        }
        freeBlock.SetColor("_Occupied", Color.black);
        occupiedBlock.SetColor("_Occupied", Color.red);
    }
    void UpdateGrid()
    {

    }

    public Vector2Int GetRandomPosition()
    {
        return grid.GetRandomPosition();
    }
    public Vector3 FindRandomPlacementPositionOfSize(GameObject obj, Vector2Int size)
    {
        bool searching = true;
        List<Vector2Int> positions = new List<Vector2Int>();
        do
        {
            searching = false;
            Vector2Int startPos = GetRandomPosition();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    positions.Add(new Vector2Int(startPos.x + x, startPos.y + y));
                    bool isWithinBounds = grid.IsWithinBounds(startPos.x + x, -startPos.y - y);
                    

                    if (!isWithinBounds ||
                        (isWithinBounds && grid[startPos.x + x, startPos.y + y].occupied) ||
                        (isWithinBounds && grid[startPos.x + x, startPos.y + y].elevation != grid[startPos].elevation))
                    {
                        searching = true;
                        positions.Clear();
                        break;
                    }
                }
                if (searching) { break; }
            }
        }
        while (searching);

        for (int i = 0; i < positions.Count; i++)
        {
            grid[positions[i]].occupied = true;
            grid[positions[i]].obj = obj;
        }

        return new Vector3((float)positions[0].x / 2f, -(float)positions[0].y / 2f, -grid[positions[0]].elevation) + new Vector3(-9.5f, 9.75f, 0);
        //!This is a magic number, I know. It centers the vase to the position its supposed to be on
    }
    public bool RequestPosition(Vector2 pos, Vector2Int size)
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
                if (!grid.IsWithinBounds(posInt.x + x, -posInt.y + y) ||
                    grid[posInt.x + x, posInt.y + y].occupied ||
                    grid[posInt.x + x, posInt.y + y].elevation != grid[posInt].elevation)
                {
                    //!if adjacent position is occupied or if the adjacent elevation is different
                    //!then this position is bad, continue while loop
                    positions.Clear();
                }
            }
        }
        for (int i = 0; i < positions.Count; i++)
        {
            grid[positions[i]].occupied = true;
        }
        return positions.Count > 0;
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
                if (!grid.IsWithinBounds(posInt.x + x, -posInt.y + y) ||
                    grid[posInt.x + x, posInt.y + y].occupied ||
                    grid[posInt.x + x, posInt.y + y].elevation != grid[posInt].elevation)
                {
                    //!if adjacent position is occupied or if the adjacent elevation is different
                    //!then this position is bad, continue while loop
                    positions.Clear();
                }
            }
        }
        for (int i = 0; i < positions.Count; i++)
        {
            grid[positions[i]].occupied = true;
        }
        return positions.Count > 0;
    }
    public bool GetValueForRenderPlacementGrid(int i, LevelManager.PlacementRenderMode mode)
    {
        if (grid[i].occupied)
        {
            if (mode == LevelManager.PlacementRenderMode.BUILD)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (mode == LevelManager.PlacementRenderMode.POSITION)
        {
            return true;
        }
        return false;
    }
    public void RenderPlacementGrid(Mesh placementSpot, Material mat, LevelManager.PlacementRenderMode mode, RoomData roomData)
    {
        occupiedSpaces.Clear();
        freeSpaces.Clear();
        for (int i = 0; i < grid.items.Count; i++)
        {
            Vector3 position = grid.Position(i); //This will get the grid position of the index, not the actual real world position
            //use drawmesh this time for convenience sake

            position += new Vector3(1f / 4f, 1f / 4f, 0);

            Vector3 roomPosition = new Vector3(roomData.originalPosition.x - 10 + Mathf.Clamp(roomData.size.x + 20, Mathf.NegativeInfinity, 0)
                                , -roomData.originalPosition.y + 10 - Mathf.Clamp(roomData.size.y + 20, Mathf.NegativeInfinity, 0), 0);
            position = new Vector3(position.x / 2, -position.y / 2, -grid[i].elevation - 0.5f) + roomPosition;

            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1f / 4f, 1f / 4f, 1));

            Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

            float distanceFromPlayer = (Party.instance.GetPartyLeader().GetPMM().transform.position - position).magnitude;

            if (screenPos.x > 0 && screenPos.x < Camera.main.pixelWidth && screenPos.y > 0 && screenPos.y < Camera.main.pixelHeight && distanceFromPlayer < renderDistanceFromPlayer)
            {
                if (GetValueForRenderPlacementGrid(i, mode))
                {
                    occupiedSpaces.Add(matrix);
                }
                else
                {
                    freeSpaces.Add(matrix);
                }
            }
        }
        for (int i = 0; i < occupiedSpaces.Count; i++)
        {
            Graphics.DrawMesh(placementSpot, occupiedSpaces[i], mat, 0, null, 0, occupiedBlock);
        }
        for (int i = 0; i < freeSpaces.Count; i++)
        {
            Graphics.DrawMesh(placementSpot, freeSpaces[i], mat, 0, null, 0, freeBlock);
        }
    }
}
