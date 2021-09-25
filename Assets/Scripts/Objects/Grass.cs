using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

[System.Serializable]public class ObjectData
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;
    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }

   public ObjectData(Vector3 pos_in, Vector3 scale_in, Quaternion rot_in)
   {
       pos = pos_in; scale = scale_in; rot = rot_in;
   }
}
public class Grass : MonoBehaviour
{
    public int renderDistanceOne;
    public Mesh meshOne; //The grass mesh
    public int renderDistanceTwo;
    public Mesh meshTwo; //The lower res mesh
    public int renderDistanceThree;

    public Mesh meshThree; //Lower res
    Mesh meshFour; //Rasterised
    public Material grassMaterial; //The material used for grass
    public Vector2Int area;
    public int grassPerTile;

    public float burningSpeed;
    public Color fireColor;

    public Vector3 grassRotation;
    public LayerMask layerMask;

    [System.Serializable]public class MeshBatch
    {
        public Vector3 position; //Used for LOD
        public List<ObjectData> batches;

        public Material material;
        public MeshBatch(Vector3 position_in, List<ObjectData> batches_in, Material material_in)
        {
            position = position_in;
            batches = batches_in;
            material = material_in;
        }
    }
    [System.Serializable]public class BurningMeshBatch //Find a way to draw them by the chunks
    {
        public Vector3 position;
        public Material material;
        public VFXData vFX;
        public List<ObjectData> batches;
        public BurningMeshBatch(Vector3 position_in, List<ObjectData> batches_in, Material material_in, VFXData vFX_in)
        {
            position = position_in;
            batches = batches_in;
        }
    }
    public List<MeshBatch> batches = new List<MeshBatch>();
    public List<MeshBatch> DEBUGbatches = new List<MeshBatch>();

    [System.Serializable]public class GrassTile
    {
        //This object has a reference to the grass straws that are on this tile
        public List<Vector3Int> batchIndices = new List<Vector3Int>(); //The x value is the batch list, and the y value is the start index in that batch list and the z value is how many steps forward
        public bool burning = false;
    }

    public Grid<GrassTile> tiles;

    public VisualEffectAsset VFX_Burning; //VFX used for fire

    [System.Serializable]public class VFXData
    {
        public GameObject gameObject;
        public bool playing;
    }
    public List<int> burningGrassIndices = new List<int>();

    public List<VFXData> VFX = new List<VFXData>();
    List<BurningMeshBatch> burningBatches = new List<BurningMeshBatch>();

    float batchDistanceToEdge;
    
    private void Awake() 
    {
        meshOne = new Mesh();
        MeshMaker.CreateTuft(meshOne, 4, 40, 0.1f);
        meshTwo = new Mesh();
        MeshMaker.CreateTuft(meshTwo, 2, 20, 0.2f);
        meshThree = new Mesh();
        MeshMaker.CreateTuft(meshThree, 1, 15, 0.2f);
        renderDistanceOne = 15;
        renderDistanceTwo = 25;
        renderDistanceThree = 35;
        batchDistanceToEdge = 5;
    }
    public void PlantFlora(Room room)
    {
        Debug.Log("NEW FLORA");
        tiles = new Grid<GrassTile>(room.size);
        int batchIndexNum = 0;
        List<ObjectData> currentBatch = new List<ObjectData>();
        int chunkDivision = 10;
        //! Go through every 10x10 chunk of a room and put down grass for it
        for(int i = 0; i < (area.x / chunkDivision) * (area.y / chunkDivision); i++) //Go through every chunk of the area
        {
            int chunk_x = (int)(i % ((float)area.x/chunkDivision));
            int chunk_y = (int)(i / ((float)area.x/chunkDivision));
            Vector3 chunkCenter = new Vector3(chunk_x * chunkDivision + 5, -chunk_y * chunkDivision + 15) + transform.position;
            for(int j = 0; j < chunkDivision; j++)
            {
                for(int k = 0; k < chunkDivision; k++)
                {
                    int index = (j + chunkDivision * chunk_x) * 2 + area.x * 2 * (k + chunkDivision * chunk_y) * 2;
                    //Going through all tiles of that chunk
                    tiles.Add(new GrassTile()); //Create new grass tile
                    int x = j + chunk_x * chunkDivision;
                    int y = k + chunk_y * chunkDivision;
                    int succeededGrasses = 0;
                    for(int l = 0; l < grassPerTile; l++) //Make a set amount of grass for this one tile
                    {
                        float elevation = room.placementGrid[index].elevation;
                        Vector3 position = new Vector3(Random.Range(x, x + 1.0f), Random.Range(y, y-1.0f), -elevation);
                        if(room.RequestPosition(position, new Vector2Int(1,1)))
                        {
                            position = new Vector3(position.x + (int)transform.position.x, -position.y + (int)transform.position.y + 19, position.z);
                            AddObject(currentBatch, i, position); //Adds one singular blade of grass to "currentBatch" 
                            batchIndexNum++;
                            succeededGrasses++;
                            if(batchIndexNum >= 1000)
                            {
                                //If you are currently adding grass to the tile, but you ran out of space in the batch, save the batch and continue adding grass
                                batches.Add(new MeshBatch(chunkCenter, currentBatch, grassMaterial));
                                tiles[tiles.items.Count - 1].batchIndices.Add(new Vector3Int(batches.Count - 1, currentBatch.Count - 1 - succeededGrasses, succeededGrasses + 1)); 
                                //! The x value is the batch list, and the y value is the start index in that batch list and the z value is how many steps forward
                                currentBatch = BuildNewBatch();
                                batchIndexNum = 0;
                            }
                        }
                    }
                    if(tiles.items.Count > 0 && tiles[tiles.items.Count - 1].batchIndices.Count > 0 && tiles[tiles.items.Count - 1].batchIndices[0].z < grassPerTile)
                    {
                        //If there are tiles, and the latest tile has more than zero batch indices, but it doesn't have the max amount of grass in it
                        //Then the tile needs to have two batch indices
                        //This currently doesn't get entered, since the grid has become so small
                        tiles[tiles.items.Count - 1].batchIndices.Add(new Vector3Int(batches.Count, 0, grassPerTile - tiles[tiles.items.Count - 1].batchIndices[0].z)); 
                        //Just Count because it's going into the next batch made. Logically it also starts at 0
                    }
                    else if(tiles.items.Count > 0 && tiles[tiles.items.Count - 1].batchIndices.Count == 0)
                    {
                        //If you went through the for loop and didn't add blades to the tile
                        //This is because you never hit the match batchIndexNum. For instance, the grid is 10x10 so with 3 grass per grid you only get 300 before you restart
                        //Its batches.Count instead of batches.Count - 1, because the new batch hasnt been added yes as opposed to the if statement in the for loop
                        tiles[tiles.items.Count - 1].batchIndices.Add(new Vector3Int(batches.Count, currentBatch.Count - succeededGrasses, succeededGrasses));
                    }
                }
            }
            if(batchIndexNum > 0 && batchIndexNum < 1000)
            {
                batches.Add(new MeshBatch(chunkCenter, currentBatch, grassMaterial)); //Add last batch
                currentBatch = BuildNewBatch();
                batchIndexNum = 0;
            }
        }
    }
    void AddObject(List<ObjectData> currentBatch, int i, Vector3 position)
    {
        Vector2 textureSize = new Vector2(grassMaterial.mainTexture.width, grassMaterial.mainTexture.height);
        currentBatch.Add(new ObjectData(position, new Vector3(1, 1, 1), Quaternion.identity));
    }

    List<ObjectData> BuildNewBatch()
    {
        return new List<ObjectData>();
    }

    private void Update() 
    {
        RenderBatches();
        UpdateFire();
    }

    private void FixedUpdate() 
    {
        //CheckCollision();
        SpreadFire();
        for(int i = 0; i < VFX.Count; i++)
        {
            if(VFX[i].gameObject.GetComponent<VisualEffect>().aliveParticleCount == 0 && !VFX[i].playing)
            {
                GameObject temp_2 = VFX[i].gameObject;
                VFX.RemoveAt(i);
                Destroy(temp_2);
                i--;
            }
        }
    }

    void UpdateFire()
    {
        for(int i = 0; i < burningBatches.Count(); i++)
        {
            float temp = burningBatches[i].material.GetFloat("_Fade");
            if(temp <= 0)
            {
                burningBatches[i].vFX.gameObject.GetComponent<VisualEffect>().Stop();
                burningBatches[i].vFX.playing = false;
                burningBatches.RemoveAt(i); i--;
                break;
            }
            burningBatches[i].material.SetFloat("_Fade", temp - burningSpeed);
        }
    }

    void RenderBatches()
    {
        //Vector3[] frustumCorners = new Vector3[4];
        //Camera.main.CalculateFrustumCorners(new Rect(0, 0, 1, 1), Camera.main.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        foreach(var b in batches)
        {
            //The fives have to account for rotation
            //Vector3 rotation = Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * Vector2.up; rotation.Normalize();
            Vector2 northPoint =  Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);
            Vector2 southPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
            Vector2 leftPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
            Vector2 rightPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);

            if((leftPoint.x > 0 && leftPoint.x < Camera.main.pixelWidth && leftPoint.y > 0 && leftPoint.y < Camera.main.pixelHeight) ||
               (rightPoint.x > 0 && rightPoint.x < Camera.main.pixelWidth && rightPoint.y > 0 && rightPoint.y < Camera.main.pixelHeight) ||
               (northPoint.y > 0 && northPoint.y < Camera.main.pixelHeight && northPoint.x > 0 && northPoint.x < Camera.main.pixelWidth) ||
               (southPoint.y > 0 && southPoint.y < Camera.main.pixelHeight && southPoint.x > 0 && southPoint.x < Camera.main.pixelWidth))
            {
                float dist = (b.position - Camera.main.transform.position).magnitude;
                Mesh mesh = dist < renderDistanceOne ? meshOne : dist < renderDistanceTwo ? meshTwo : dist < renderDistanceThree ? meshThree : meshThree;
                if(mesh != null)
                {
                    Graphics.DrawMeshInstanced(mesh, 0, b.material, b.batches.Select((a) => a.matrix).ToList());
                }
            }
        }
        foreach(var b in DEBUGbatches)
        {
            //The fives have to account for rotation
            //Vector3 rotation = Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * Vector2.up; rotation.Normalize();
            Vector2 northPoint =  Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);
            Vector2 southPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
            Vector2 leftPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
            Vector2 rightPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);

            if((leftPoint.x > 0 && leftPoint.x < Camera.main.pixelWidth && leftPoint.y > 0 && leftPoint.y < Camera.main.pixelHeight) ||
               (rightPoint.x > 0 && rightPoint.x < Camera.main.pixelWidth && rightPoint.y > 0 && rightPoint.y < Camera.main.pixelHeight) ||
               (northPoint.y > 0 && northPoint.y < Camera.main.pixelHeight && northPoint.x > 0 && northPoint.x < Camera.main.pixelWidth) ||
               (southPoint.y > 0 && southPoint.y < Camera.main.pixelHeight && southPoint.x > 0 && southPoint.x < Camera.main.pixelWidth))
            {
                float dist = (b.position - Camera.main.transform.position).magnitude;
                Mesh mesh = dist < renderDistanceOne ? meshOne : dist < renderDistanceTwo ? meshTwo : dist < renderDistanceThree ? meshThree : meshThree;
                if(mesh != null)
                {
                    Graphics.DrawMeshInstanced(mesh, 0, b.material, b.batches.Select((a) => a.matrix).ToList());
                }
            }
        }
        foreach(var b in burningBatches)
        {
            if(CloseEnough(b.position))
            {
                Graphics.DrawMeshInstanced(meshOne, 0, b.material, b.batches.Select((a) => a.matrix).ToList());
            }
        }
    }

    public bool CloseEnough(Vector3 position)
    {
        return (position - Camera.main.transform.position).magnitude < renderDistanceOne;
    }

    public void RenderGrassChunkCenters(Transform trans)
    {
        for(int i = 0; i < batches.Count; i++)
        {
            GLFunctions.DrawSquareFromCorner(batches[i].position, new Vector2(1,1), trans, Color.magenta);
        }
    }

    private void OnDrawGizmos() 
    {
        /*for(int i = 0; i < area.x * area.y; i++)
        {
            Vector3 center = new Vector3((int)(i%(float)area.x) + 0.5f + transform.position.x, (int)(i/(float)area.x) + 0.5f + transform.position.y, -0.5f);
            Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
            Gizmos.DrawCube(center, size);
        }*/
        foreach(var b in batches)
        {
            //? this gizmo shows the point from the camera to each grass batch
            //Gizmos.color = CloseEnough(b.position) ? Color.magenta : Color.blue;
            //Gizmos.DrawLine(Camera.main.transform.position, b.position);

            Vector2 northPoint =  Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position;
            Vector2 southPoint = Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position;
            Vector2 leftPoint = Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position;
            Vector2 rightPoint = Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position;

            Gizmos.DrawLine(northPoint, new Vector3(northPoint.x, northPoint.y, 3));
            Gizmos.DrawLine(southPoint, new Vector3(southPoint.x, southPoint.y, 3));
            Gizmos.DrawLine(leftPoint, new Vector3(leftPoint.x, leftPoint.y, 3));
            Gizmos.DrawLine(rightPoint, new Vector3(rightPoint.x, rightPoint.y, 3));
        }
    }

    public void CheckCollision(Vector2 pos)
    {
        //!this position given has to have the room position subtracted from it
        Vector2Int posInt = pos.ToV2Int();
        if(tiles[posInt].burning){return;}
        Vector3 center = new Vector3((int)pos.x + 0.5f + transform.position.x, pos.y + 0.5f + transform.position.y, -0.5f);
        Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
        //Debug.Log("Position " + pos);
        //Debug.Log("Center " + center);
        Collider[] hitColliders = Physics.OverlapBox(center, size, Quaternion.identity, layerMask, QueryTriggerInteraction.UseGlobal);
        for(int j = 0; j < hitColliders.Length; j++)
        {
            Debug.Log(hitColliders[j].name);
            if(hitColliders[j].gameObject.GetComponent<Fire>()) //If something that is on fire has hit this collider
            {
               Debug.Log("Something was on fire");
               // SetTileOnFire((int)pos.x + area.x * (int)pos.y, center);
               DEBUGColorFire(posInt.x + area.x * posInt.y);
            }
        }
    }
   /* void CheckCollision()
    {
        for(int i = 0; i < area.x * area.y; i++)
        {
            if(tiles[i].burning){continue;}
            Vector3 center = new Vector3((int)(i%(float)area.x) + 0.5f + transform.position.x, (int)(i/(float)area.x) + 0.5f + transform.position.y, -0.5f);
            Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
            Collider[] hitColliders = Physics.OverlapBox(center, size, Quaternion.identity, layerMask, QueryTriggerInteraction.UseGlobal);
            
            for(int j = 0; j < hitColliders.Length; j++)
            {
                if(hitColliders[j].gameObject.GetComponent<Fire>()) //If something that is on fire has hit this collider
                {
                    SetTileOnFire(i, center);
                   // return;
                }
            }
        }
    }*/
    void SpreadFire()
    {
        for(int i = 0; i < burningGrassIndices.Count; i++)
        {
            if(tiles[burningGrassIndices[i]].burning)
            {
                int[] constraints = Math.GetValidConstraints(burningGrassIndices[i], 1, area);
                for(int x = constraints[0]; x < constraints[2]; x++)
                {
                    for(int y = constraints[1]; y < constraints[3]; y++)
                    {
                        if(x + area.x * y != burningGrassIndices[i]) //If were not in the middle
                        {
                            //TODO Use flammability of the material otherwise
                            if(Random.Range(0, 1000) < 2 && !tiles[x + area.x * y].burning)
                            {
                                //Set this tile on fire
                                Vector3 center = new Vector3((int)((x + area.x * y)%(float)area.x) + 0.5f + transform.position.x, (int)((x + area.x * y)/(float)area.x) + 0.5f + transform.position.y, -0.5f);
                                SetTileOnFire(x + area.x * y, center);
                            }
                        }
                    }
                }
            }
        }
    }

    void DEBUGColorFire(int i) 
    {
        List<Vector3Int> indices = tiles[i].batchIndices;
        tiles[i].burning = true; //Setting a bool like this prevents negative indices to be read
        
        //int[] Index = GetBatchIndex(new Vector2((int)i%area.x + transform.position.x, (int)i/area.x + transform.position.y)); //Find the index of the batch that just got hit 

        for(int k = 0; k < indices.Count; k++)
        {
            DEBUGbatches.Add(new MeshBatch(Vector3.zero, new List<ObjectData>(), new Material(grassMaterial)));
            DEBUGbatches[DEBUGbatches.Count - 1].material.SetColor("_Color", Color.red);
            Debug.Log(indices[k].x + " and " + indices[k].y + " and " + indices[k].z);
            Debug.Log(batches[indices[k].x].batches.GetRange(indices[k].y, indices[k].z).Count);
            DEBUGbatches[DEBUGbatches.Count - 1].batches.AddRange(batches[indices[k].x].batches.GetRange(indices[k].y, indices[k].z)); //Add the batch you just hit to the list of burning batches
            
            Debug.Log("Batches count: " + batches[indices[k].x].batches.Count);
            batches[indices[k].x].batches.RemoveRange(indices[k].y,indices[k].z); 
            Debug.Log("Batches count: " + batches[indices[k].x].batches.Count);
            //! if removing, then every single index has to be changed in all the tiles whose indices are in index[k].x
            //! I guess thats the fall I have to take since I can't figure out an alternative
            //! it has to do this because otherwise they're references indices in batches that has been removed (RemoveRange)
            for(int l = i; l < tiles.items.Count; l++)
            {
                //Going through all tiles from the point you hit
                //Start from i, since it was the grass of tile i that was removed. Then work your way up and left shift all the indices by indices[k].z
                if(tiles[l].batchIndices[0].x > indices[k].x) //! if the first indices of this tile are from a batch higher up than the one with removed grass, then you can exit this for loop
                {
                    break;
                }

                //Going through all indices of this tile
                for(int m = 0; m < tiles[l].batchIndices.Count; m++)
                {
                    //If the tile and the tile you hit have the same batch index, then move them down
                    if(tiles[l].batchIndices[m].x == indices[k].x) //! If the indices in this tile has indices on the same batch number as the one where indices were removed, move them further down
                    {
                        tiles[l].batchIndices[m] = new Vector3Int(tiles[l].batchIndices[m].x, tiles[l].batchIndices[m].y - indices[k].z, tiles[l].batchIndices[m].z);
                    }
                }
            }
        }
    }

    void SetTileOnFire(int i, Vector3 center)
    {
        //hitColliders[j].gameObject.SetActive(false);
        List<Vector3Int> indices = tiles[i].batchIndices;
        tiles[i].burning = true;
        burningGrassIndices.Add(i);
        Debug.Log("Hit");
        
        //int[] Index = GetBatchIndex(new Vector2((int)i%area.x + transform.position.x, (int)i/area.x + transform.position.y)); //Find the index of the batch that just got hit 

        for(int k = 0; k < indices.Count; k++)
        {
            burningBatches.Add(new BurningMeshBatch(Vector3.zero, new List<ObjectData>(), new Material(grassMaterial), new VFXData()));
            burningBatches[burningBatches.Count - 1].batches.AddRange(batches[indices[k].x].batches.GetRange(indices[k].y, indices[k].z)); //Add the batch you just hit to the list of burning batches
            
            batches[indices[k].x].batches.RemoveRange(indices[k].y,indices[k].z); 
            //! if removing, then every single index has to be changed in all the tiles whose indices are in index[k].x
            //! I guess thats the fall I have to take since I can't figure out an alternative
            for(int l = i; l < tiles.items.Count; l++)
            {
                //Start from i, since it was the grass of tile i that was removed. Then work your way up and left shift all the indices by indices[k].z
                if(tiles[l].batchIndices[0].x > indices[k].x) //! if the first indices of this tile are from a batch higher up than the one with removed grass, then you can exit this for loop
                {
                    break;
                }

                for(int m = 0; m < tiles[l].batchIndices.Count; m++)
                {
                    //Go through all indices of this particular tile
                    if(tiles[l].batchIndices[m].x == indices[k].x) //! If the indices in this tile has indices on the same batch number as the one where indices were removed, move them further down
                    {
                        tiles[l].batchIndices[m] = new Vector3Int(tiles[l].batchIndices[m].x, tiles[l].batchIndices[m].y - indices[k].z, tiles[l].batchIndices[m].z);
                    }
                }
            }
        }
        
        VFXData temp = burningBatches[burningBatches.Count - 1].vFX;
        temp.gameObject = new GameObject("VFX"); //Create VFX for fire
        temp.gameObject.transform.parent = transform; 
        temp.gameObject.transform.position = center;
        temp.gameObject.AddComponent<VisualEffect>();
        temp.gameObject.GetComponent<VisualEffect>().visualEffectAsset = VFX_Burning;
        temp.playing = true;
        VFX.Add(temp);
    }
}
