using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;

//Core code
public partial class Room: MonoBehaviour
{
    public class EntranceData
    {
        public List<Vector3> leftVertices = new List<Vector3>(); //From inside the room looking towards north door. Left vertices are saved at the end of the left wall, right vertices are saved at the beginning of the right wall;
        public List<Vector3> rightVertices = new List<Vector3>();
    }
    public List<EntranceData> entrances = new List<EntranceData>(); //Save vertices for every single door

    public PlacementGrid placementGrid;

    public RoomData roomData = new RoomData();
    public Vegetation grass;

    public Vector2 centerPoint; //DEBUGGING

    public void CreateRoom(ref RoomTemplate template, Material floorMaterial_in, FurnitureDatabase furnitureDatabase)
    {
        Color color = new Color32((byte)UnityEngine.Random.Range(125, 220),(byte)UnityEngine.Random.Range(125, 220),(byte)UnityEngine.Random.Range(125, 220), 255);
        Material furnitureMaterial = new Material(floorMaterial_in.shader);
        furnitureMaterial.CopyPropertiesFromMaterial(floorMaterial_in);
        furnitureMaterial.color = color;
        placementGrid = new PlacementGrid(template);
        Furnish(furnitureMaterial, furnitureDatabase);
    }

    private void Update() 
    {
        if(!grass){return;}
        Vector2 radius = new Vector2(Mathf.Abs(roomData.size.x) / 2, -Mathf.Abs(roomData.size.y) / 2);
        centerPoint = transform.position - new Vector3(10,-10, 0) + (Vector3)radius; //10,10 to make the position the corner and then push it to the middle'
        //These variables are for the frustum culling. But I havent gotten those to work yet

        grass.UpdateVegetation();
    }
    private void FixedUpdate() 
    {
        if(!grass){return;}
        grass.FixedUpdateVegetation();
    }
    public void Furnish(string furnitureName, LevelManager levelManager, FurnitureDatabase furnitureDatabase, Transform placement = null)
    {
        DebugLog.AddToMessage("Substep", "Furnishing");
        FurnitureDatabase.DatabaseEntry furnitureData = furnitureDatabase.GetDatabaseEntry(furnitureName);
        Quaternion rotation = placement == null ? Quaternion.Euler(furnitureData.rotation) : placement.rotation;
        GameObject furniture = Instantiate(furnitureData.prefab, Vector2.zero, rotation, placement == null ? transform : placement); //x is the rotation
        furniture.AddComponent<BoxCollider>();
        Vector3 position = placement == null ? FindRandomPlacementPositionOfSize(furniture, furnitureData.dimensions) + new Vector3(0.5f, -0.5f, 0) : Vector3.zero;
        furniture.transform.localPosition = position;
        if(furniture.TryGetComponent(out IInteractable interactable))
        {
            interactable.OnCreate(levelManager, furnitureDatabase);
        }
    }
    public void Furnish(Material mat, FurnitureDatabase furnitureDatabase)
    {
        DebugLog.AddToMessage("Substep", "Furnishing");

        GameObject rock = new GameObject("Rock");
        rock.transform.parent = transform;
        MeshRenderer rend = rock.AddComponent<MeshRenderer>();
        MeshFilter filt = rock.AddComponent<MeshFilter>();
        rock.AddComponent<Rigidbody>();
        filt.mesh = MeshMaker.CreateRock();
        Material matz = Resources.Load<Material>("Materials/Stone");
        rend.material = matz;
        rock.AddComponent<SphereCollider>();
        rock.transform.localPosition = FindRandomPlacementPositionOfSize(rock, new Vector2Int(1,1));
        rock.AddComponent<Carryable>();
        HealthModel health = rock.AddComponent<HealthModel>();
        health.maxHealth = 1;
        health.currentHealth = 1;
        EntityStatistics entityStatistics = rock.AddComponent<EntityStatistics>();
        entityStatistics.physiology = EntityStatistics.Physiology.ROCKEN;
        entityStatistics.SetPhysiology();

        //GameObject chest = new GameObject("Chest");
        //MeshMaker.CreateChest(chest, 0);
    }

    Vector3 FindRandomPlacementPositionOfSize(GameObject obj, Vector2Int size)
    {
        return placementGrid.FindRandomPlacementPositionOfSize(obj, size);
    }

    public bool RequestPosition(Vector2 pos, Vector2Int size)
    {
        return placementGrid.RequestPosition(pos, size);
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
        return placementGrid.RequestPositionFromWorldSpace(pos, size);
    }
    public void DisplayDistance()
    {
        //GetComponentInChildren<Number>().OnDisplayNumber(roomData.stepsAwayFromMainRoom);
    }
    public void RenderPlacementGrid(Mesh placementSpot, Material mat, LevelManager.PlacementRenderMode mode)
    {
        placementGrid.RenderPlacementGrid(placementSpot, mat, mode, roomData);
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