using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    List<AudioClip> melody = new List<AudioClip>();
    List<AudioClip> baseLine = new List<AudioClip>();

    public Vector2Int amountOfRoomsCap = new Vector2Int(10, 20);
    public Vector2Int amountOfSections = new Vector2Int(1, 1);

    public List<Room.RoomTemplate> templates;
    public Room.RoomTemplate bigTemplate;

    public int openDoorProbability = 10;

    public Vector2 roomOpenness = Vector2.zero;

    public int foragingSpawnProbability;

    [System.Serializable]
    public class RoomGridEntry
    {
        public Vector2Int position;
        public RoomData roomData;
        Room room;
        public RoomGridEntry(Vector2Int position, RoomData roomData)
        {
            this.position = position;
            this.roomData = roomData;
            room = null;
        }
        public void SetRoom(Room room)
        {
            this.room = room;
        }
        public Room GetRoom() => room;
    }
    [System.Serializable]
    public class SectionData //Used only for generation
    {
        public List<RoomData> rooms = new List<RoomData>();
    }
    public List<SectionData> sectionData = new List<SectionData>();
    [System.Serializable]
    public class Section //For the instantiated version
    {
        public List<Room> rooms = new List<Room>();
    }
    public List<Section> sections = new List<Section>();

    public List<RoomGridEntry> roomGrid = new List<RoomGridEntry> { }; //Separate rooms into rooms and roomGrid. roomGrid is all positions on the grid that are occupied. This is so that I wont have to look through the list of rooms


    public Texture2D map;

    //List<Flora> m_flora = new List<Flora>{}
    //List<Enemy> m_enemies = new List<Enemy>{}
    //List<Entity> m_fauna = new List<Entity>{}
}

public class LevelDataGenerator : MonoBehaviour
{
    static public LevelData Initialize(int LevelDataSeed)
    {
        if (DunGenes.Instance != null)
        {
            DunGenes.Instance.gameData.levelDataSeed = LevelDataSeed;
        }
        LevelData data = new LevelData();
        Random.InitState(LevelDataSeed);
        return data;
    }
}
