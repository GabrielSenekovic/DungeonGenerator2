﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class LevelManager : MonoBehaviour
{
    public Vector2Int RoomSize = new Vector2Int(20,20);

    public LevelData levelData;
    public QuestData questData;

    public Room firstRoom;
    public Room lastRoom;

    public Room currentRoom;
    public Room previousRoom = null;

    public Party party;

    LevelGenerator generator;

    bool renderGrassChunks;

    public Mesh placementQuad;
    public Material placementMat;

    public PlacementRenderMode placementRenderMode;

    public enum PlacementRenderMode
    {
        NONE,
        BUILD,
        POSITION
    }

    EntityManager entityManager;
    public MeshBatchRenderer meshBatchRenderer;

    private void Awake() 
    {
        Debug.Log("Awake");
        //GameData.m_LevelConstructionSeed = Random.Range(0, int.MaxValue);
        //GameData.m_LevelDataSeed = Random.Range(0, int.MaxValue);
        if(DunGenes.Instance.gameData != null)
        {
            Party.instance.GetPartyLeader().transform.position = Vector2.zero;
           // DunGenes.Instance.gameData.SetPlayerPosition(new Vector2(-RoomSize.x/2, -RoomSize.y/2));
        }
        renderGrassChunks = true;
        placementQuad = MeshMaker.GetQuad();
        placementMat = Resources.Load<Material>("Materials/Placement");
        placementRenderMode = PlacementRenderMode.NONE;
        entityManager = GetComponent<EntityManager>();
    }
    private void Start() 
    {
        Debug.Log("Start");
        party = Party.instance;
        levelData = DunGenes.Instance.gameData.GetCurrentLevelData();
        questData = DunGenes.Instance.gameData.GetCurrentQuestData();
        generator = FindObjectOfType<LevelGenerator>();

        meshBatchRenderer.Initialise();
        generator.GenerateLevel(this, ref levelData.templates, ref levelData.bigTemplate);
        generator.PutDownQuestObjects(this, questData);

        currentRoom = firstRoom; UIManager.Instance.miniMap.SwitchMap(currentRoom.roomData.mapTexture);
        CameraMovement.SetCameraAnchor(new Vector2(firstRoom.transform.position.x,firstRoom.transform.position.x + firstRoom.roomData.size.x - 20) , new Vector2(firstRoom.transform.position.y - firstRoom.roomData.size.y + 20, firstRoom.transform.position.y));
        CameraMovement.movementMode = CameraMovement.CameraMovementMode.SingleRoom;
    }

    private void Update()
    {
        if(!generator.levelGenerated){generator.BuildLevel(levelData, currentRoom);}
        if(party == null){return;}
        if(UpdateQuest())
        {
            //Level is ended
            //Load HQ scene
            SceneManager.LoadSceneAsync("HQ");
        }
        if(CheckIfChangeRoom())
        {
            party.GetPartyLeader().GetPMM().SetCanMove(false);
            CameraMovement.SetMovingRoom(true);
        }
        if(currentRoom.grass != null)
        {
            entityManager.CheckProjectileGrassCollision(currentRoom);
        }
    }
    private void LateUpdate()
    {
        if (CameraMovement.GetMovingRoom())
        {
            Vector2Int newPos = (Party.instance.GetPartyLeader().transform.position / 20f).ToV2Int() * 20;
            Vector2Int prevPos = (CameraMovement.Instance.prevCameraPosition / 20f).ToV2Int() * 20;

            if (CameraMovement.Instance.MoveCamera(new Vector3(newPos.x, newPos.y, CameraMovement.GetRotationObject().transform.position.z), prevPos.ToV3()))
            {
                CameraMovement.SetCameraAnchor(new Vector2(currentRoom.transform.position.x,currentRoom.transform.position.x + Mathf.Abs(currentRoom.roomData.size.x) - 20) , 
                new Vector2(currentRoom.transform.position.y - Mathf.Abs(currentRoom.roomData.size.y) + 20, currentRoom.transform.position.y));
               // previousRoom.gameObject.SetActive(false);
                UIManager.Instance.miniMap.SwitchMap(currentRoom.roomData.mapTexture);
            }
        }

    }
    bool CheckIfChangeRoom()
    {
        Vector2Int playerGridPos = (party.GetPartyLeader().transform.position / 20f).ToV2Int();
        if(party.GetPartyLeader().transform.position.x > currentRoom.transform.position.x + (Mathf.Abs(currentRoom.roomData.size.x) - 10))
        {
            previousRoom = currentRoom;
            currentRoom = generator.FindRoomOfPosition(playerGridPos, DunGenes.Instance.gameData.CurrentLevel);
            currentRoom.gameObject.SetActive(true);
            return true;
        }
        else if(party.GetPartyLeader().transform.position.x < currentRoom.transform.position.x - 10)
        {
            previousRoom = currentRoom;
            currentRoom = generator.FindRoomOfPosition(playerGridPos, DunGenes.Instance.gameData.CurrentLevel);
            currentRoom.gameObject.SetActive(true);
            return true;
        }
        else if (party.GetPartyLeader().transform.position.y > currentRoom.transform.position.y + 10)
        {
            previousRoom = currentRoom;
            currentRoom = generator.FindRoomOfPosition(playerGridPos, DunGenes.Instance.gameData.CurrentLevel);
            currentRoom.gameObject.SetActive(true);
            return true;
        }
        else if (party.GetPartyLeader().transform.position.y < currentRoom.transform.position.y - (Mathf.Abs(currentRoom.roomData.size.y) - 10))
        {
            previousRoom = currentRoom;
            currentRoom = generator.FindRoomOfPosition(playerGridPos, DunGenes.Instance.gameData.CurrentLevel);
            currentRoom.gameObject.SetActive(true);
            return true;
        }
        return false;
    }
    bool UpdateQuest()
    {
        return false;
    }
    public void SetPlacementRenderMode(PlacementRenderMode mode)
    {
        placementRenderMode = mode;
    }

    private void OnRenderObject() 
    {
        if(renderGrassChunks && currentRoom.grass != null)
        {
            currentRoom.grass.RenderGrassChunkCenters(transform);
        }
        if(placementRenderMode != PlacementRenderMode.NONE)
        {
            currentRoom.RenderPlacementGrid(placementQuad, placementMat, placementRenderMode);
        }
    }
}
