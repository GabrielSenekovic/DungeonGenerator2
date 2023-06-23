using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DunGenes
    : MonoBehaviour
{
    static DunGenes instance;

    public static DunGenes Instance
    {
        get
        {
            return instance;
        }
    }
    public GameData gameData;

    public bool isStartArea; //Only for debug
    public int wallCount = 0;

    DayNightCycle dayNightCycle;

    FurnitureDatabase furnitureDatabase;
    public LevelManager levelManager;

    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
            dayNightCycle = GetComponent<DayNightCycle>();
            furnitureDatabase = Resources.Load<FurnitureDatabase>("FurnitureDatabase");
            TextAsset reader = Resources.Load<TextAsset>("FurnitureDatabase");
            furnitureDatabase.Initialise(reader.text);
        }
        else
        {
            Debug.Log("DESTROYING SELF");
            Destroy(gameObject);
            isStartArea = false;
        }
        if(isStartArea)
        {
            FindObjectOfType<LevelGenerator>().GenerateStartArea(levelManager.settlementData);
        }
    }

    private void Start() 
    {
        /*if(isStartArea)
        {
            FindObjectOfType<LevelGenerator>().GenerateStartArea();
        }*/
    }
    public DayNightCycle GetDayNightCycle()
    {
        return dayNightCycle;
    }
}
[System.Serializable]public class GameData
{
    public PlayerController player;

    public LevelData currentLevel;
    public static QuestData currentQuest;

    public int levelConstructionSeed; //Used by the room generator to generate the room
    public LevelData CurrentLevel
    {
        get
        {
            return currentLevel;
        }
        set
        {
            currentLevel = value;
        }
    }
    public int levelDataSeed;
    public int questDataSeed;

    public GameData()
    {
    }

    public void SetSeed(int levelConstructionSeed_in, int levelDataSeed_in, int questDataSeed_in)
    {
        Debug.Log("The construction seed is: " + levelConstructionSeed_in);
        Debug.Log("The data seed is: " + levelDataSeed);
        levelConstructionSeed = levelConstructionSeed_in;
        levelDataSeed = levelDataSeed_in;
        questDataSeed = questDataSeed_in;
    }
    public Vector2 GetPlayerPosition()
    {
        return player.transform.position;
    }
    public void SetPlayerPosition(Vector2 newPosition)
    {
        player.transform.position = newPosition;
    }
    public LevelData GetCurrentLevelData()
    {
        if(currentLevel != null)
        {
            return currentLevel;
        }
        else
        {
            return LevelDataGenerator.Initialize(levelDataSeed);
        }
    }
    public QuestData GetCurrentQuestData()
    {
        if(currentQuest != null)
        {
            return currentQuest;
        }
        else
        {
            //Make new
            return null;
        }
    }
}