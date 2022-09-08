using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public enum Biome
{
    Continental = 0,
    Xeric = 1,
    Desert = 2,
    Savannah = 3,
    Alpine = 4,
    IceCap = 5,
    Tundra = 6,
    Boreal = 7,
    Mediterranean = 8,
    Rainforest = 9,
    Ocean = 10
}

public enum Mood
{
    Creepy = 0, //Grayish, ghostly, bones, discordant, misty
    Mysterious = 1, //Saturated, Sparkly, Misty, Slow, Invisible, Magical
    Calm = 2, //Not many enemies, calm music, desaturated, more safe rooms, maybe villages
    Decrepit = 3, //Ruins, adventurous, full of treasures, curious
    Adventurous = 4, //Higher chance for altitudal elements, higher pace, more enemies
    Dangerous = 5,  //High spawn rate, scary music, no chill, very hazardous, many traps
    Cursed = 6, //Rooms may be cursed, almost no safe zones, ghosts spawn
    Fabulous = 7, //Many flowers, sparkly water, elementals, saturated, crystals
    Plentiful = 8 //Increases rate of forage materials, thick shrubbery, more treasure rooms
}

[System.Serializable]public class LevelData
{
    List<AudioClip> melody = new List<AudioClip>();
    List<AudioClip> baseLine = new List<AudioClip>();

    public Vector2Int amountOfRoomsCap = new Vector2Int(450,500);
    public Vector2Int amountOfSections = new Vector2Int(1,1);

    public Mood[] mood = new Mood[2];
    public Biome biome;

    public List<Room.RoomTemplate> templates;

    public int temperatureLevel; //0 = tepid, 1 = warm, 2 = hot, -1 = cold, -2 freezing
    public uint waterLevel; //0 = no water, 1 = some few lakes, maybe a river, 2 = High chance for lakes, probably a river, 3 = Wetland, its like everything is a lake
    public uint magicLevel; //0 = normal, 1 = may find magical stones, some elementals, 2 = many elementals may spawn, many elemental ores may be found, 3 = magical mist so strong it enhances magic stats and decreases physical stats
    public uint dangerLevel;
    public int altitude; //0 is surface level. Higher levels imply mountainous, or sky. Lower levels imply underground caverns.

    public int normalRoomProbability = 20;
    public int treasureRoomProbability = 1;
    public int ambushRoomProbability = 5;
    public int restingRoomProbability = 5;

    public int openDoorProbability = 10;

    public Vector2 roomOpenness = Vector2.zero;

    public int foragingSpawnProbability;

    public bool cave = false;
    public bool dungeon = false;

    public bool mushroom = false;
    public bool crystal = false;

    [System.Serializable]public struct RoomGridEntry
    {
        public Vector2Int position;
        public Room room;
        public RoomGridEntry(Vector2Int position_in, Room room_in)
        {
            position = position_in; room = room_in;
        }
    }
    [System.Serializable]public class Section
    {
        public List<Room> rooms = new List<Room>();
    }
    public List<Section> sections = new List<Section>();

    public List<RoomGridEntry> roomGrid = new List<RoomGridEntry> { }; //Separate rooms into rooms and roomGrid. roomGrid is all positions on the grid that are occupied. This is so that I wont have to look through the list of rooms


    public Texture2D map;

    //List<Flora> m_flora = new List<Flora>{}
    //List<Enemy> m_enemies = new List<Enemy>{}
    //List<Entity> m_fauna = new List<Entity>{}

    public Mood GetMood(int i)
    {
        return mood[i];
    }

    public float GetFullRoomProbabilityPercentage()
    {
        //Debug.Log("Full probability: " + (m_treasureRoomProbability + m_normalRoomProbability + m_safeRoomProbability + m_ambushRoomProbability));
        return treasureRoomProbability + normalRoomProbability + restingRoomProbability + ambushRoomProbability;
    }

    public float GetRoomProbability(RoomType type)
    {
        switch(type)
        {
            case RoomType.NormalRoom: return normalRoomProbability;
            case RoomType.AmbushRoom: return ambushRoomProbability;
            case RoomType.TreasureRoom: return treasureRoomProbability;
            case RoomType.RestingRoom: return restingRoomProbability;
            default: return 0;
        }
    }
    public float GetRoomPercentage(RoomType type)
    {
        return (int)(GetRoomProbability(type)/GetFullRoomProbabilityPercentage()*100);
    }
}

public class LevelDataGenerator : MonoBehaviour
{
    static public LevelData Initialize(int LevelDataSeed)
    {
        if(DunGenes.Instance != null)
        {
            DunGenes.Instance.gameData.levelDataSeed = LevelDataSeed;
        }
        LevelData data = new LevelData();
        Random.InitState(LevelDataSeed);
        ChooseLocation(data);
        ChooseTemp(data);
        ChooseWaterLevel(data);
        ChooseMagicLevel(data);
        ChooseMood(data);
        ChooseBiome(data);
        ChooseRoomProbabilities(data);
        return data;
    }
    static void ChooseLocation(LevelData data)
    {
        int temp = Random.Range(0, 8);
        if(temp == 7)
        {
            data.dungeon = true;
        }
    }
    static void ChooseTemp(LevelData data)
    {
        data.temperatureLevel += Random.Range(-1, 2);
    }
    static void ChooseWaterLevel(LevelData data)
    {
        data.waterLevel = (uint)Random.Range(0, 4);
    }

    static void ChooseMagicLevel(LevelData data)
    {
        data.magicLevel = (uint)Random.Range(0, 2);
    }
    static void ChooseMood(LevelData data)
    {
        data.mood[0] = (Mood)Random.Range(0, 9);
        data.mood[1] = (Mood)Random.Range(0, 9);
        foreach(Mood mood in data.mood)
        {
            switch(mood)
            {
                case Mood.Adventurous:
                    data.dangerLevel++;
                    break;
                case Mood.Calm:
                    data.restingRoomProbability += 2;
                    break;
                case Mood.Creepy:
                    data.restingRoomProbability--;
                    break;
                case Mood.Cursed:
                    data.restingRoomProbability -= 2;
                    break;
                case Mood.Dangerous:
                    data.dangerLevel += 2;
                    data.ambushRoomProbability += 2;
                    data.restingRoomProbability--;
                    break;
                case Mood.Decrepit:
                    data.treasureRoomProbability += 2;
                    break;
                case Mood.Fabulous:
                    data.magicLevel++;
                    break;
                case Mood.Mysterious:
                    data.magicLevel += 2;
                    break;
                case Mood.Plentiful:
                    data.treasureRoomProbability++;
                    data.foragingSpawnProbability += 3;
                    break;
            }
        }
    } 
    static void ChooseBiome(LevelData data)
    {
        data.biome = (Biome)Random.Range(0, 11);
        switch(data.biome)
        {
            case Biome.Alpine: 
                data.roomOpenness = new Vector2(10, 20);
                data.openDoorProbability = 2;
                break;
            case Biome.Boreal:
                data.temperatureLevel--;
                data.roomOpenness = new Vector2(2, 5);
                data.openDoorProbability = 2;
                break;
            case Biome.Desert:
                data.waterLevel -= 1;
                data.roomOpenness = new Vector2(10, 20);
                data.openDoorProbability = 5;
                break;
            case Biome.Xeric:
                data.waterLevel = 0;
                data.roomOpenness = new Vector2(10, 20);
                data.openDoorProbability = 5;
                break;
            case Biome.IceCap:
                data.temperatureLevel -= 3;
                data.roomOpenness = new Vector2(10, 20);
                data.openDoorProbability = 3;
                break;
            case Biome.Mediterranean:
                data.temperatureLevel++;
                data.roomOpenness = new Vector2(2, 10);
                data.openDoorProbability = 3;
                break;
            case Biome.Ocean: 
                data.openDoorProbability = 5;
                break;
            case Biome.Rainforest:
                data.temperatureLevel += 2;
                data.roomOpenness = new Vector2(0, 10);
                data.openDoorProbability = 1;
                break;
            case Biome.Savannah:
                data.temperatureLevel++;
                data.roomOpenness = new Vector2(5, 20);
                data.openDoorProbability = 2;
                break;
            case Biome.Tundra:
                data.temperatureLevel-=2;
                data.roomOpenness = new Vector2(5, 15);
                data.openDoorProbability = 2;
                break;
            case Biome.Continental: 
                data.roomOpenness = new Vector2(0, 20);
                data.openDoorProbability = 4;
                break;
        }
    }
    static void ChooseRoomProbabilities(LevelData data)
    {
        if (data.restingRoomProbability < 0)
        {
            data.restingRoomProbability = 0;
        }
        if(data.normalRoomProbability < 0)
        {
            data.normalRoomProbability = 0;
        }
        if(data.ambushRoomProbability < 0)
        {
            data.ambushRoomProbability = 0;
        }
        if(data.treasureRoomProbability < 0)
        {
            data.treasureRoomProbability = 0;
        }
    }
}

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
        levelData.dungeon = true;
        questData = DunGenes.Instance.gameData.GetCurrentQuestData();
        generator = FindObjectOfType<LevelGenerator>();

        meshBatchRenderer.Initialise();
        generator.GenerateLevel(this, ref levelData.templates);
        generator.PutDownQuestObjects(this, questData);

        currentRoom = firstRoom; UIManager.Instance.miniMap.SwitchMap(currentRoom.mapTexture);
        CameraMovement.SetCameraAnchor(new Vector2(firstRoom.transform.position.x,firstRoom.transform.position.x + firstRoom.size.x - 20) , new Vector2(firstRoom.transform.position.y - firstRoom.size.y + 20, firstRoom.transform.position.y));
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
                CameraMovement.SetCameraAnchor(new Vector2(currentRoom.transform.position.x,currentRoom.transform.position.x + Mathf.Abs(currentRoom.size.x) - 20) , 
                new Vector2(currentRoom.transform.position.y - Mathf.Abs(currentRoom.size.y) + 20, currentRoom.transform.position.y));
               // previousRoom.gameObject.SetActive(false);
                UIManager.Instance.miniMap.SwitchMap(currentRoom.mapTexture);
            }
        }

    }
    bool CheckIfChangeRoom()
    {
        Vector2Int playerGridPos = (party.GetPartyLeader().transform.position / 20f).ToV2Int();
        if(party.GetPartyLeader().transform.position.x > currentRoom.transform.position.x + (Mathf.Abs(currentRoom.size.x) - 10))
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
        else if (party.GetPartyLeader().transform.position.y < currentRoom.transform.position.y - (Mathf.Abs(currentRoom.size.y) - 10))
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
        switch(questData.missionType)
        {
            case QuestData.MissionType.Recovery:
                //return generator.spawnedEndOfLevel.isInteractedWith;
                return false;
            case QuestData.MissionType.Backup:
                if(questData.GetStatus())
                {
                    return true;
                }
                //If false, update timers about the status of the NPCs youre supposed to help
                return false;
            case QuestData.MissionType.Delivery:
            case QuestData.MissionType.Escort:
            case QuestData.MissionType.Hunt:
            case QuestData.MissionType.Inquiry:
            case QuestData.MissionType.Investigation:
                return questData.GetStatus();
            default: return false;
        }
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
