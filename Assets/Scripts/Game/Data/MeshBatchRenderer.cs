using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[System.Serializable]public class ObjectData
{
    public Vector3 pos;
    public Vector3 tilePos;
    public Vector3 scale;
    public Quaternion rot;
    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }
    public Matrix4x4 matrixTile
    {
        get
        {
            return Matrix4x4.TRS(tilePos, rot, scale);
        }
    }

   public ObjectData(Vector3 pos_in, Vector3 scale_in, Quaternion rot_in, Vector3 tilesPos_in)
   {
       pos = pos_in; scale = scale_in; rot = rot_in; tilePos = tilesPos_in;
   }
}
public class MeshBatchRenderer : MonoBehaviour
{
    static EntityDatabase database;
    public static bool RenderRandomPositions;

    public void Initialise() 
    {
        database = Resources.Load<EntityDatabase>("EntityDatabase");
        TextAsset reader = Resources.Load<TextAsset>("EntityDatabase");
        //Load the info how to make the grass and tulips from a file
        //Read the text from directly from the test.txt file
        database.Initialise(reader.text);
        Debug.Log("Database loaded");
        RenderRandomPositions = true;
    }
    //Move code from Grass.cs to here, so that other scripts can use it
    public static void CreateBatches(Vegetation vegetation, Room room)
    {
        vegetation.tiles = new Grid<Vegetation.GrassTile>(room.size); vegetation.tiles.Init();
        OnCreateBatches(vegetation, room, database.GetDatabaseEntry("Tulip"), 0.1f);
        OnCreateBatches(vegetation, room, database.GetDatabaseEntry("Poppy"), 0.1f);
        OnCreateBatches(vegetation, room, database.GetDatabaseEntry("Grass"), 1000);
    }
    public static void OnCreateBatches(Vegetation vegetation, Room room, EntityDatabase.DatabaseEntry databaseEntry, float density)
    {
        int batchIndexNum = 0;
        List<ObjectData> currentBatch = new List<ObjectData>();
        int chunkDivision = 10;
        //! Go through every 10x10 chunk of a room and put down grass for it
        for(int i = 0; i < (vegetation.area.x / chunkDivision) * (vegetation.area.y / chunkDivision); i++) //Go through every chunk of the area
        {
            int chunk_x = (int)(i % ((float)vegetation.area.x/chunkDivision));
            int chunk_y = (int)(i / ((float)vegetation.area.x/chunkDivision));
            Vector3 chunkCenter = new Vector3(chunk_x * chunkDivision + 5, -chunk_y * chunkDivision + 15) + vegetation.transform.position;
            for(int j = 0; j < chunkDivision; j++) 
            {
                for(int k = 0; k < chunkDivision; k++)
                {
                    int index = (k + chunkDivision * chunk_x) * 2 + vegetation.area.x * 2 * (j + chunkDivision * chunk_y) * 2;
                    //Going through all tiles of that chunk
                    int x = k + chunk_x * chunkDivision;
                    int y = j + chunk_y * chunkDivision;
                    vegetation.tiles[x,y] = new Vegetation.GrassTile(); //Create new grass tile
                    int succeededGrasses = 0;
                    int amountForThisTile = (int)(vegetation.grassPerTile * Mathf.PerlinNoise(x * density,y * density));
                    for(int l = 0; l < amountForThisTile; l++) //Make a set amount of grass for this one tile
                    {
                        float elevation = room.placementGrid[index].elevation;
                        Vector3 position = new Vector3(Random.Range(x, x + 1.0f), Random.Range(y - 1.0f, y), -elevation);
                        if(room.RequestPosition(position, new Vector2Int(1,1)))
                        {
                            position = new Vector3(position.x + (int)vegetation.transform.position.x, -position.y + (int)vegetation.transform.position.y + 19, position.z);
                            currentBatch.Add(new ObjectData(position, new Vector3(1, 1, 1), Quaternion.identity, new Vector3(x + 0.5f + (int)vegetation.transform.position.x, -(y - 0.5f) + (int)vegetation.transform.position.y + 19, -elevation))); 
                            batchIndexNum++;
                            succeededGrasses++;
                            if(batchIndexNum >= 1000)
                            {
                                //If you are currently adding grass to the tile, but you ran out of space in the batch, save the batch and continue adding grass
                                vegetation.batches.Add(new Vegetation.MeshBatch(chunkCenter, currentBatch, databaseEntry.name, databaseEntry.material));
                                vegetation.tiles[x,y].batchIndices.Add(new Vector3Int(vegetation.batches.Count - 1, currentBatch.Count - 1 - succeededGrasses, succeededGrasses + 1)); 
                                //! The x value is the batch list, and the y value is the start index in that batch list and the z value is how many steps forward
                                currentBatch = BuildNewBatch();
                                batchIndexNum = 0;
                            }
                        }
                    }
                    if(vegetation.tiles[x,y].batchIndices.Count > 0 && vegetation.tiles[x,y].batchIndices[0].z < vegetation.grassPerTile)
                    {
                        //If there are tiles, and the latest tile has more than zero batch indices, but it doesn't have the max amount of grass in it
                        //Then the tile needs to have two batch indices
                        //This currently doesn't get entered, since the grid has become so small
                        vegetation.tiles[x,y].batchIndices.Add(new Vector3Int(vegetation.batches.Count, 0, vegetation.grassPerTile - vegetation.tiles[vegetation.tiles.items.Count - 1].batchIndices[0].z)); 
                        //Just Count because it's going into the next batch made. Logically it also starts at 0
                    }
                    else if(vegetation.tiles[x,y].batchIndices.Count == 0)
                    {
                        //If you went through the for loop and didn't add blades to the tile
                        //This is because you never hit the match batchIndexNum. For instance, the grid is 10x10 so with 3 grass per grid you only get 300 before you restart
                        //Its batches.Count instead of batches.Count - 1, because the new batch hasnt been added yes as opposed to the if statement in the for loop
                        vegetation.tiles[x,y].batchIndices.Add(new Vector3Int(vegetation.batches.Count, currentBatch.Count - succeededGrasses, succeededGrasses));
                    }
                }
            }
            if(batchIndexNum > 0 && batchIndexNum < 1000)
            {
                vegetation.batches.Add(new Vegetation.MeshBatch(chunkCenter, currentBatch, databaseEntry.name, databaseEntry.material)); //Add last batch
                currentBatch = BuildNewBatch();
                batchIndexNum = 0;
            }
        }
    }
    public static List<ObjectData> BuildNewBatch()
    {
        return new List<ObjectData>();
    }
    public static void RenderBatches(Vegetation.MeshBatch b, float batchDistanceToEdge)
    {
        if(Math.IsWithinFrustumRotated(b.position, batchDistanceToEdge))
        {
            float dist = (b.position - Camera.main.transform.position).magnitude;
            bool temp = false;
            EntityDatabase.DatabaseEntry entry = database.GetDatabaseEntry(b.name);
            Mesh mesh = database.GetMesh(b.name, dist, ref temp);
            Material mat = temp ? entry.billBoard : b.material;
            if(mesh != null)
            {
                if (RenderRandomPositions) { Graphics.DrawMeshInstanced(mesh, 0, mat, b.batches.Select((a) => a.matrix).ToList()); }
                else { Graphics.DrawMeshInstanced(mesh, 0, mat, b.batches.Select((a) => a.matrixTile).ToList()); }
            }
        }
    }
    public static void RenderBatches(Vegetation.BurningMeshBatch b, float batchDistanceToEdge)
    {
        if(Math.IsWithinFrustumRotated(b.position, batchDistanceToEdge))
        {
            float dist = (b.position - Camera.main.transform.position).magnitude;
            bool temp = false;
            Mesh mesh = database.GetMesh(b.name, dist, ref temp);
            if(mesh != null)
            {
                Graphics.DrawMeshInstanced(mesh, 0, b.material, b.batches.Select((a) => a.matrix).ToList());
            }
        }
    }
}
