using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DunGenes : MonoBehaviour
{
    static DunGenes instance;

    public static DunGenes Instance
    {
        get
        {
            return instance;
        }
    }
    public static GameData gameData;

    public bool isStartArea; //Only for debug

    private void Awake() 
    {
        if(isStartArea)
        {
            FindObjectOfType<LevelGenerator>().GenerateStartArea();
        }
        if(instance == null)
        {
            instance = this;
            gameData = new GameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start() 
    {
        /*if(isStartArea)
        {
            FindObjectOfType<LevelGenerator>().GenerateStartArea();
        }*/
    }
}
public class GameData : MonoBehaviour
{
    public static GameData Instance;
    static PlayerController Player;

    static LevelData currentLevel;
    public static QuestData currentQuest;

    public int levelConstructionSeed; //Used by the room generator to generate the room
    public int levelDataSeed;
    public int questDataSeed;

    public GameData()
    {
        Initialize();
    }
    public void Initialize()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void SetSeed(int constructionSeed, int levelDataSeed, int questDataSeed)
    {
        Debug.Log("The construction seed is: " + constructionSeed);
        Debug.Log("The data seed is: " + levelDataSeed);
        Instance.levelConstructionSeed = constructionSeed;
        Instance.levelDataSeed = levelDataSeed;
        Instance.questDataSeed = questDataSeed;
    }
    public static Vector2 GetPlayerPosition()
    {
        return Player.transform.position;
    }
    public static void SetPlayerPosition(Vector2 newPosition)
    {
        Player.transform.position = newPosition;
    }
    public static LevelData GetCurrentLevelData()
    {
        if(currentLevel != null)
        {
            return currentLevel;
        }
        else
        {
            return LevelDataGenerator.Initialize(Instance.levelDataSeed);
        }
    }
    public static QuestData GetCurrentQuestData()
    {
        if(currentQuest != null)
        {
            return currentQuest;
        }
        else
        {
            return QuestDataGenerator.Initialize(Instance.questDataSeed);
        }
    }
}