using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

public class Vegetation : MonoBehaviour
{
    public Vector2Int area;
    public int grassPerTile;

    public float burningSpeed;
    public Color fireColor;

    public Vector3 grassRotation;
    public LayerMask layerMask;

    [System.Serializable]public class MeshBatch
    {
        public Vector3 position; //Used for LOD
        public string name; //What name to search for meshes
        public List<ObjectData> batches;
        public Material material;

        public MeshBatch(Vector3 position_in, List<ObjectData> batches_in, string name_in, Material material_in)
        {
            position = position_in;
            batches = batches_in;
            name = name_in;
            material = material_in;
        }
    }
    [System.Serializable]public class BurningMeshBatch //Find a way to draw them by the chunks
    {
        public Vector3 position;
        public string name;
        public Material material;
        public VFXData vFX;
        public List<ObjectData> batches;
        public BurningMeshBatch(Vector3 position_in, List<ObjectData> batches_in, Material material_in, VFXData vFX_in, string name_in)
        {
            position = position_in;
            batches = batches_in;
            name = name_in;
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
        batchDistanceToEdge = 5;
    }
    public void PlantFlora(Room room)
    {
        MeshBatchRenderer.CreateBatches(this, room);
    }

    void Update() 
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
        foreach(var b in batches)
        {
            MeshBatchRenderer.RenderBatches(b, batchDistanceToEdge);
        }
       /* foreach(var b in DEBUGbatches)
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
        }*/
        foreach(var b in burningBatches)
        {
            MeshBatchRenderer.RenderBatches(b, batchDistanceToEdge);
        }
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
       /* for(int i = 0; i < area.x * area.y; i++)
        {
            Vector3 center = new Vector3((int)(i%(float)area.x) + 0.5f + transform.position.x, (int)(i/(float)area.x) + 0.5f + transform.position.y, -0.5f);
            Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
            Gizmos.DrawCube(center, size);
        }
       /* foreach(var b in batches)
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
        }*/
    }

    public void CheckCollision(Vector2 pos, Vector3 actualPos)
    {
        //!this position given has to have the room position subtracted from it
        Vector2Int posInt = pos.ToV2Int();
        if(tiles[posInt].burning){return;}
        Vector3 center = new Vector3(Mathf.RoundToInt(actualPos.x) + 0.5f, Mathf.RoundToInt(actualPos.y) + 0.5f, actualPos.z);
        Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
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
        Debug.Log("Index colored: " + i);
        List<Vector3Int> indices = tiles[i].batchIndices;
        tiles[i].burning = true; //Setting a bool like this prevents negative indices to be read
        
        //int[] Index = GetBatchIndex(new Vector2((int)i%area.x + transform.position.x, (int)i/area.x + transform.position.y)); //Find the index of the batch that just got hit 

        for(int k = 0; k < indices.Count; k++)
        {
            DEBUGbatches.Add(new MeshBatch(batches[indices[k].x].position, new List<ObjectData>(), "grass", batches[indices[k].x].material));
            DEBUGbatches[DEBUGbatches.Count - 1].material.SetColor("_Color", Color.red);
            DEBUGbatches[DEBUGbatches.Count - 1].batches.AddRange(batches[indices[k].x].batches.GetRange(indices[k].y, indices[k].z)); //Add the batch you just hit to the list of burning batches
            
            batches[indices[k].x].batches.RemoveRange(indices[k].y,indices[k].z); 
            //! if removing, then every single index has to be changed in all the tiles whose indices are in index[k].x
            //! I guess thats the fall I have to take since I can't figure out an alternative
            //! it has to do this because otherwise they're references indices in batches that has been removed (RemoveRange)
            for(int l = i; l < tiles.items.Count; l++)
            {
                //Going through all tiles from the point you hit
                //Start from i, since it was the grass of tile i that was removed. Then work your way up and left shift all the indices by indices[k].z
                if(tiles[l].batchIndices[0].x != indices[k].x) //! if the first indices of this tile are from a batch higher up than the one with removed grass, then you can exit this for loop
                {
                    continue;
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
            burningBatches.Add(new BurningMeshBatch(Vector3.zero, new List<ObjectData>(), new Material(batches[indices[k].x].material), new VFXData(), batches[indices[k].x].name));
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
