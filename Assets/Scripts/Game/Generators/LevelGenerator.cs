using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class LevelGenerator : MonoBehaviour
{
    [System.Serializable]public class Section
    {
        public List<Room> rooms = new List<Room>();
    }
    [SerializeField]List<Section> sections = new List<Section>();
    [SerializeField]List<Tuple<Vector2Int, Room>> roomGrid = new List<Tuple<Vector2Int, Room>> { }; //Separate rooms into rooms and roomGrid. roomGrid is all positions on the grid that are occupied. This is so that I wont have to look through the list of rooms
    List<MeshRenderer> surroundings = new List<MeshRenderer>{};
    
    [SerializeField]protected Room RoomPrefab;

    int numberOfRooms = 1;

    bool renderSections = false;

    bool bossSpawned = false;
    Room bossRoom;

    int furthestDistanceFromSpawn = 0;

    int amountOfRandomOpenEntrances = 0;

    public bool levelGenerated = false;

    public Material debugMaterial;

    public InteractableBase endOfLevel; //Debugging object for Recovery Quest
    [System.NonSerialized]public InteractableBase spawnedEndOfLevel; // spawned version

    public void GenerateStartArea()
    {
        //Called when not in the level

        List<Room.RoomTemplate> templates = new List<Room.RoomTemplate>();
        sections[0].rooms[0].Initialize(new Vector2Int(20,20), false, 0, ref templates);
        templates[0].AddEntrancesToRoom(sections[0].rooms[0].directions);
        Room.RoomTemplate template = templates[0];
        sections[0].rooms[0].CreateRoom(ref template, Resources.Load<Material>("Materials/Wall"), Resources.Load<Material>("Materials/Ground"));
        //Surround this one room with floors
        GameObject surroundings = new GameObject("Surroundings");
        surroundings.transform.parent = this.gameObject.transform;
        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                if(x == 1 && y == 1){continue;}
                GameObject surroundingObject = new GameObject("Surroundings");
                surroundingObject.transform.parent = surroundings.gameObject.transform;
                surroundingObject.AddComponent<MeshFilter>();
                MeshMaker.CreateSurface(surroundingObject.GetComponent<MeshFilter>().mesh, 4);
                surroundingObject.AddComponent<MeshRenderer>();
                surroundingObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Ground");
                surroundingObject.transform.localPosition = new Vector3(-10 - 20 + (x * 20), 10 + 20 - (y * 20), 0);
            }
        }
    }
    public void GenerateLevel(LevelManager level, Vector2Int RoomSize, Vector2Int amountOfRooms, Vector2Int amountOfSections)
    {
        System.DateTime before = System.DateTime.Now;

        UnityEngine.Random.InitState(GameData.m_LevelConstructionSeed);

        List<Room.RoomTemplate> templates = new List<Room.RoomTemplate>();

        sections.Add(new Section()); 
        sections[0].rooms.Add(Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, transform));
        roomGrid.Add(new Tuple<Vector2Int, Room>(Vector2Int.zero, sections[0].rooms[0]));
        sections[0].rooms[0].Initialize(RoomSize, true, 0, ref templates);

        SpawnRooms(UnityEngine.Random.Range((int)(amountOfRooms.x + sections[0].rooms.Count),
                                (int)(amountOfRooms.y + sections[0].rooms.Count)), UnityEngine.Random.Range((int)(amountOfSections.x),
                                (int)(amountOfSections.y)), RoomSize, level.l_data, ref templates);

       // Debug.Log("RoomGrid size " + roomGrid.Count);

        BuildRooms(ref templates);

        level.firstRoom = sections[0].rooms[0];
        level.lastRoom = sections[sections.Count-1].rooms[sections[sections.Count-1].rooms.Count - 1];

        //AdjustRoomTypes(level.l_data);
        //AdjustEntrances(RoomSize);

        System.DateTime after = System.DateTime.Now; 
        System.TimeSpan duration = after.Subtract(before);
        Debug.Log("<color=blue>Time to generate: </color>" + duration.TotalMilliseconds + " milliseconds, which is: " + duration.TotalSeconds + " seconds");
        if(DebuggingTools.spawnOnlyBasicRooms){FindObjectOfType<DebugText>().Display(level.l_data);}
    }
   
    public void PutDownQuestObjects(LevelManager level, QuestData data)
    {
        switch(data.missionType)
        {
            case QuestData.MissionType.Recovery: 
                spawnedEndOfLevel = Instantiate(endOfLevel, 
                    new Vector2(level.lastRoom.transform.position.x + 10, level.lastRoom.transform.position.y + 10), 
                    Quaternion.identity, level.lastRoom.transform);
                break;
            case QuestData.MissionType.Inquiry:
                InquiryQuestData temp = data as InquiryQuestData;
               /* Instantiate(temp.Target.NPC,
                    new Vector2(temp.Target.Room.x + 10, temp.Target.Room.y + 10),
                    Quaternion.identity, level.lastRoom.transform);*/
                break;
            case QuestData.MissionType.Delivery:
                break;
            case QuestData.MissionType.Backup:
                BackupQuestData temp2 = data as BackupQuestData;
                for(int i = 0; i < temp2.NPCsToBackup.Count; i++)
                {
                    Instantiate(temp2.NPCsToBackup[i].NPC,
                    new Vector2(level.lastRoom.transform.position.x + 10, level.lastRoom.transform.position.y + 10),
                    Quaternion.identity, level.lastRoom.transform);
                }
                break;
            case QuestData.MissionType.Escort:
                break;
            case QuestData.MissionType.Hunt:
                break;
            case QuestData.MissionType.Investigation:
                break;
            default: break;
        }
    }
    public void BuildLevel(LevelData data, Room currentRoom)
    {
        Debug.LogWarning("<color=blue>Time to build rooms!</color>");
        levelGenerated = true;
        /*foreach(Room room in rooms)
        {
            if(room != currentRoom && !DebuggingTools.spawnOnlyBasicRooms)
            {
               // room.gameObject.SetActive(false);
            }
        }*/
    }
    void SpawnRooms(int amountOfRooms, int amountOfSections, Vector2Int RoomSize, LevelData data, ref List<Room.RoomTemplate> templates)
    {
        System.DateTime before = System.DateTime.Now;
        //this spawns all rooms
        for(int i = 0; i < amountOfSections; i++)
        {
            if(i != 0)
            {
                sections.Add(new Section());
            }
            int k = 0;
            for (int j = sections[i].rooms.Count; k < amountOfRooms; j++)
            {
                Debug.Log("<color=yellow>Spawning room: " + j + " in section: " + i + "</color>");
                
                //Vector2Int currentRoomSize = new Vector2Int(UnityEngine.Random.Range(1,4),UnityEngine.Random.Range(1,4)); //in grid space
                Vector2Int currentRoomSize = new Vector2Int(5,5);
                
                Tuple<Room, List<Room.Entrances.Entrance>> originRoom = new Tuple<Room, List<Room.Entrances.Entrance>>(new Room(), new List<Room.Entrances.Entrance>(){});
                try
                {
                    if(j != 0) //If not the first room every section
                    {
                        originRoom = GetRandomRoomOfSection(i); //! If there are no open entrances in any room, the catch will be executed
                        Debug.Log("Origin room to spawn new from: " + originRoom.Item1.transform.position);
                    }
                    else if(i != 0) //And if not the first room of the first section
                    {
                        originRoom = new Tuple<Room, List<Room.Entrances.Entrance>> (sections[i-1].rooms[sections[i-1].rooms.Count - 1], sections[i-1].rooms[sections[i-1].rooms.Count - 1].directions.entrances);
                        OpenAvailableEntrances(originRoom.Item1);
                        Debug.Log("I ended up here");
                    }
                }
                catch
                {
                    Debug.Log("<color=red>Could no longer spawn new rooms</color>");
                    break;
                }
                sections[i].rooms.Add(Instantiate(RoomPrefab, transform));
                sections[i].rooms[j].name = "Room #" + (numberOfRooms+1); numberOfRooms++;
                Debug.Log(sections[i].rooms[j].name);

                bool indoors = false;
                //data.dungeon ? UnityEngine.Random.Range(0, 100) < 80 : false;

                Vector2Int gridPositionWhereTheNewRoomConnects = Vector2Int.zero;
                sections[i].rooms[j].Initialize(GetNewRoomCoordinates(sections[i].rooms[j], originRoom.Item1.transform.position.ToV2Int(), originRoom.Item2, ref currentRoomSize, ref gridPositionWhereTheNewRoomConnects), currentRoomSize * 20, indoors, i, ref templates);
                sections[i].rooms[j].roomData.stepsAwayFromMainRoom = originRoom.Item1.roomData.stepsAwayFromMainRoom + 1;
                if(sections[i].rooms[j].roomData.stepsAwayFromMainRoom > furthestDistanceFromSpawn)
                {
                    furthestDistanceFromSpawn = sections[i].rooms[j].roomData.stepsAwayFromMainRoom;
                }
                bool locked = j == 0 && i > 0 ? true:false;
                ActivateEntrances(originRoom.Item1, sections[i].rooms[j], gridPositionWhereTheNewRoomConnects, locked);
                LinkRoom(sections[i].rooms[j], RoomSize);
                OpenRandomEntrances(sections[i].rooms[j], data.openDoorProbability);
                for(int x = 0; x < currentRoomSize.x; x++)
                {
                    for(int y = 0; y < currentRoomSize.y; y++)
                    {
                        roomGrid.Add(new Tuple<Vector2Int, Room>(sections[i].rooms[j].transform.position.ToV2Int()/20 + new Vector2Int(x,-y), sections[i].rooms[j]));
                        k++;
                    }
                }
            }
            CloseAllEntrances();
        }
        System.DateTime after = System.DateTime.Now; 
        System.TimeSpan duration = after.Subtract(before);
        Debug.Log("<color=blue>Time to spawn rooms: </color>" + duration.TotalMilliseconds + " milliseconds, which is: " + duration.TotalSeconds + " seconds");
    }
    void OpenAvailableEntrances(Room origin)
    {
        for(int i = 0; i < origin.directions.entrances.Count; i++)
        {
            if(!CheckIfCoordinatesOccupied(origin.transform.position.ToV2Int() / 20 + origin.directions.entrances[i].dir))
            {
                origin.directions.entrances[i].SetOpen(true);
            }
        }
    }
    void CloseAllEntrances()
    {
        for(int i = 0; i < sections.Count; i++)
        {
            for(int j = 0; j < sections[i].rooms.Count; j++)
            {
                for(int k = 0; k < sections[i].rooms[j].directions.entrances.Count; k++)
                {
                    if(sections[i].rooms[j].directions.entrances[k].open && !sections[i].rooms[j].directions.entrances[k].spawned)
                    {
                        sections[i].rooms[j].directions.entrances[k].Deactivate();
                    }
                }
            }
        }
    }
    void BuildRooms(ref List<Room.RoomTemplate> templates)
    {
        //Now that all entrances have been set, you can put the entrances down on each room template and adjust the templates to make sure there is always space to get to each door
        //Then build the rooms
        //This is where the templates list should end. It is not needed after this
        int count = 0;
        for(int i = 0; i < sections.Count; i++)
        {
            for(int j = 0; j < sections[i].rooms.Count; j++)
            {
                templates[count].AddEntrancesToRoom(sections[i].rooms[j].directions);
                Room.RoomTemplate template = templates[count];
                sections[i].rooms[j].CreateRoom(ref template, Resources.Load<Material>("Materials/Wall"), Resources.Load<Material>("Materials/Ground"));
                SaveWallVertices(ref templates, template, sections[i].rooms[j]);
                count++;
            }
        }
        //PlantFlora(ref templates);
    }
    void SaveWallVertices(ref List<Room.RoomTemplate> templates, Room.RoomTemplate originTemplate, Room origin)
    {
        //Set the entrance vertices of all adjacent rooms
        List<Tuple<Vector2Int, Vector2Int, Room>> roomList = GetAllAdjacentRooms(origin);
        //Item1 is the adjacent gridposition, Item2 is the gridposition it connects to in the origin room
        for(int k = 0; k < roomList.Count; k++)
        {
            int n = 0;
            for(int l = 0; l < sections.Count; l++)
            {
                for(int m = 0; m < sections[l].rooms.Count; m++)
                {
                    n++;
                    if(roomList[k].Item1 == sections[l].rooms[m].transform.position.ToV2Int()/20) //found the index for templates
                    {
                       // Debug.Log("Saving wall vertices from: " + roomList[k].Item2 + " to: " + roomList[k].Item1);
                        Room.RoomTemplate adjTemplate = templates[n-1];
                        
                        Vector2 direction = (new Vector2(roomList[k].Item1.x, roomList[k].Item1.y) - roomList[k].Item2).normalized;
                        Tuple<bool, Room.Entrances.Entrance> adjEntrance = roomList[k].Item3.directions.GetEntrance(roomList[k].Item1, -direction.ToV2Int());
                        Tuple<bool, Room.Entrances.Entrance> myEntrance = origin.directions.GetEntrance(roomList[k].Item1 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);

                        roomList[k].Item3.directions.SetEntranceVertices(ref adjTemplate, originTemplate, adjEntrance.Item2, myEntrance.Item2);
                    }
                }
            }
        }
    }
     public void PlantFlora(ref List<Room.RoomTemplate> templates)
    {
        for(int i = 0; i < sections.Count; i++)
        {
            for(int j = 0; j < sections[i].rooms.Count; j++)
            {
                if(!templates[j].indoors)
                {
                    GameObject lawn = new GameObject("Lawn");
                    lawn.transform.parent = sections[i].rooms[j].transform;

                    Grass grass = lawn.AddComponent<Grass>();
                    grass.grassMaterial = Resources.Load<Material>("Materials/Grass");
                    grass.area = templates[j].size;
                    grass.grassPerTile = 3;
                    grass.burningSpeed = 0.001f;
                    grass.fireColor = Color.red;
                    grass.grassRotation = new Vector3(-90, 90, -90);
                    grass.layerMask = ~0;
                    grass.VFX_Burning = Resources.Load<UnityEngine.VFX.VisualEffectAsset>("VFX/Burning");

                    lawn.transform.localPosition = new Vector3(-10, -10, -0.5f);

                    grass.PlantFlora(templates[j+i]);
                }
            }
        }
    }

    void ActivateEntrances(Room origin, Room newRoom, Vector2Int gridPositionWhereOriginConnects, bool locked)
    {
        //Get entrance in adjacent room that points to this room
        //Get entrance in this room that points to adjacent room
        //!same here as linkrooms, you need to get the origin gridposition that is closest to newroom
        //!the newroom position is already by nature closest, since its spawned from the direction of a door

        //!then you have to make sure that these two rooms connect anywhere else that wasnt intended

        Vector2 direction = ((Vector2)gridPositionWhereOriginConnects - (Vector2)newRoom.transform.position/20).normalized;
        //Debug.Log(direction);

        Tuple<bool, Room.Entrances.Entrance> adjEntrance = newRoom.directions.GetEntrance(newRoom.transform.position.ToV2Int()/20, direction.ToV2Int());

        if(adjEntrance.Item1) //If i found the correct entrance
        {
            Tuple<bool, Room.Entrances.Entrance> myEntrance = origin.directions.GetEntrance(newRoom.transform.position.ToV2Int()/20 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);
            if(myEntrance.Item1)
            {
               // Debug.Log("<color=magenta>Linking " + myEntrance.Item2.dir + "from " + (origin.transform.position / 20) + " to " + adjEntrance.Item2.dir + " from " + (newRoom.transform.position/20) + "</color>");
                myEntrance.Item2.Activate();
                adjEntrance.Item2.Activate();

                if(locked)
                {
                    GameObject myDoor = new GameObject("Locked Door");
                    myDoor.transform.parent = origin.transform;
                    myDoor.transform.localPosition = new Vector3(myEntrance.Item2.dir.x * 9.5f, myEntrance.Item2.dir.y * 9.5f); //positions are in grid space, while theyre not in the real world
                    BoxCollider myCol = myDoor.AddComponent<BoxCollider>();
                    myCol.size = new Vector3(1 + 3 * Mathf.Abs(myEntrance.Item2.dir.y), 1 + 3 * Mathf.Abs(myEntrance.Item2.dir.x), 5);
                    Unlockable myUnl = myDoor.AddComponent<Unlockable>();

                    GameObject adjDoor = new GameObject("Locked Door");
                    adjDoor.transform.parent = newRoom.transform;
                    adjDoor.transform.localPosition = new Vector3(adjEntrance.Item2.dir.x * 9.5f, adjEntrance.Item2.dir.y * 9.5f);
                    BoxCollider adjCol = adjDoor.AddComponent<BoxCollider>();
                    adjCol.size = new Vector3(1 + 3 * Mathf.Abs(adjEntrance.Item2.dir.y), 1 + 3 * Mathf.Abs(adjEntrance.Item2.dir.x), 5);
                    Unlockable adjUnl = adjDoor.AddComponent<Unlockable>();

                    adjUnl.otherDoor = myUnl;
                    myUnl.otherDoor = adjUnl;

                    adjEntrance.Item2.SetEntranceType(Room.Entrances.Entrance.EntranceType.LockedDoor);
                    myEntrance.Item2.SetEntranceType(Room.Entrances.Entrance.EntranceType.LockedDoor);
                }
            }
            else
            {
                Debug.LogError("COULDNT FIND THE ORIGIN ENTRANCE DESPITE BEING ADJACENT");
            }
        }
        else
        {
            Debug.LogError("COULDNT FIND THE ADJACENT ENTRANCE DESPITE BEING ADJACENT");
        }
    }
    Vector2Int GetNewRoomCoordinates(Room room, Vector2Int originCoordinates, List<Room.Entrances.Entrance> openEntrances, ref Vector2Int roomSize, ref Vector2Int gridPositonWhereNewRoomConnects) //Roomsize is in grid size
    {
        List<Tuple<Vector2Int, Vector2Int>> possibleCoordinates = new List<Tuple<Vector2Int, Vector2Int>> { };
        //Debug.Log("The count of open entrances is: " + openEntrances.Count + " and value 0 is " + openEntrances[0].dir);
        foreach(Room.Entrances.Entrance entrance in openEntrances)
        {
            //Debug.Log("Get new room coordinates " + new Vector2Int(entrance.gridPos.x + entrance.dir.x, entrance.gridPos.y + entrance.dir.y));
            if(!CheckIfCoordinatesOccupied(new Vector2Int(entrance.gridPos.x + entrance.dir.x, entrance.gridPos.y + entrance.dir.y)))
            {
                possibleCoordinates.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(entrance.gridPos.x * 20 + entrance.dir.x * 20, entrance.gridPos.y * 20 + entrance.dir.y * 20), entrance.gridPos));
            }
        }
        int index = possibleCoordinates.GetRandomIndex();
        //Debug.Log("Amount of possible coordinates: " + possibleCoordinates.Count + " and amount of openEntrances: " + openEntrances.Count + " index became: " + index);
        gridPositonWhereNewRoomConnects = possibleCoordinates[index].Item2;
        //!When its a big room, you decide the position first, then you see if you can grow out like intended. If you cant, then change the roomSize. Thats why its passed as a reference
        AttemptExpansion(room, possibleCoordinates[index].Item1 / 20, ref roomSize);
        //Debug.Log("Roomsize became " + roomSize);
        return possibleCoordinates[index].Item1;
        //return possibleCoordinates[index].Item1 + new Vector2Int(0, roomSize.y * 20 - 20); //Because it expands upwards in y, the start position should also be pushed up
    }

    void AttemptExpansion(Room room, Vector2Int origin, ref Vector2Int roomSize)
    {
        //Debug.Log("Roomsize before attempting expansion: " + roomSize);
        int x = 1, y = 1;
        
        while(x < roomSize.x || y < roomSize.y)
        {
            if(x < roomSize.x && !CheckIfCoordinatesOccupied(origin + new Vector2Int(x,0))) //If there is a free spot to the side, then try to expand to the side
            {
                List<Tuple<Vector2Int, Room>> temp = new List<Tuple<Vector2Int, Room>>();
                for(int i = 0; i < y; i++) //Go through all to the side. All of them have to be free
                {
                    if(!CheckIfCoordinatesOccupied(origin + new Vector2Int(x,-i)))
                    {
                        temp.Add(new Tuple<Vector2Int, Room>(origin + new Vector2Int(x,-i), room));
                    }
                    else
                    {
                        //Debug.Log("Clearing x list");
                        temp.Clear();
                        roomSize = new Vector2Int(x, roomSize.y);
                    }
                }
                roomGrid.AddRange(temp);
                x++;
            }
            else
            {
                roomSize = new Vector2Int(x, roomSize.y);
            }
            if(y < roomSize.y && !CheckIfCoordinatesOccupied(origin + new Vector2Int(0,-y))) //If there is a free spot vertically, then try to expand vertically
            {
                List<Tuple<Vector2Int, Room>> temp = new List<Tuple<Vector2Int, Room>>();
                for(int i = 0; i < x; i++)
                {
                    if(!CheckIfCoordinatesOccupied(origin + new Vector2Int(i,-y)))
                    {
                        temp.Add(new Tuple<Vector2Int, Room>(origin + new Vector2Int(i,-y), room));
                    }
                    else
                    {
                        //Debug.Log("Clearing y list");
                        temp.Clear();
                        roomSize = new Vector2Int(roomSize.x, y);
                    }
                }
                roomGrid.AddRange(temp);
                y++;
            }
            else
            {
                roomSize = new Vector2Int(roomSize.x, y);
            }
        }
    }

    bool CheckIfCoordinatesOccupied(Vector2Int roomPosition)
    {
        foreach(Tuple<Vector2Int, Room> room in roomGrid)
        {
            if(room.Item1 == roomPosition)
            {
                return true;
            } 
        }
        return false;
    }

    void LinkRoom(Room room, Vector2 RoomSize)
    {
        //This function checks if this given room has another spawned room in any direction that it must link to, before it decides if it should link anywhere else
        //It does this by checking if a room in any direction has an open but not spawned gate in its own direction, in which case it opens its own gate in that direction

        //Debug.Log("Getting adjacent rooms of: " + (room.transform.position.ToV2Int() / 20) + " with the size of: " + RoomSize);
        List<Tuple<Vector2Int, Vector2Int, Room>> roomList = GetAllAdjacentRooms(room);
        //Item1 is the adjacent gridposition, Item2 is the gridposition it connects to in the origin room
        for(int i = 0; i < roomList.Count; i++)
        {
            //Get entrance in adjacent room that points to this room
            //Get entrance in this room that points to adjacent room

            //It cant just be any grid position of the room, it has to be the grid position where the entrances are. Roomlist returns the correct position, but the correct position in the origin room has to be used
            
            Vector2 direction = (new Vector2(roomList[i].Item1.x, roomList[i].Item1.y) - roomList[i].Item2).normalized;

            Tuple<bool, Room.Entrances.Entrance> adjEntrance = roomList[i].Item3.directions.GetEntrance(roomList[i].Item1, -direction.ToV2Int());

            if(adjEntrance.Item1) //If i found the correct entrance
            {
                Tuple<bool, Room.Entrances.Entrance> myEntrance = room.directions.GetEntrance(roomList[i].Item1 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);
                if(myEntrance.Item1)
                {
                    if(adjEntrance.Item2.open)
                    {
                        //Debug.Log("<color=cyan>Linking " + myEntrance.Item2.dir + "from " + (room.transform.position / 20) + " to " + adjEntrance.Item2.dir + " from " + roomList[i].Item1 + "</color>");
                        myEntrance.Item2.Activate();
                        adjEntrance.Item2.Activate();
                    }
                    else
                    {//!if an adjacent rooms entrance is not Open, then close the origin rooms entrance and set it to spawned
                        myEntrance.Item2.Close();
                    }
                }
                else
                {
                    Debug.LogError("COULDNT FIND THE ORIGIN ENTRANCE DESPITE BEING ADJACENT");
                }
            }
            else
            {
                Debug.LogError("COULDNT FIND THE ADJACENT ENTRANCE DESPITE BEING ADJACENT");
            }
        }
    }

    void OpenRandomEntrances(Room room, int openDoorProbability)
    {
        //This will open a random amount of doors in the newly spawned room
        List<Room.Entrances.Entrance> possibleEntrancesToOpen = new List<Room.Entrances.Entrance> { };

        foreach(Room.Entrances.Entrance entrance in room.GetDirections().entrances)
        {
            if(entrance.open == false && entrance.spawned == false)
            {
                possibleEntrancesToOpen.Add(entrance);
            }
        }
        if (possibleEntrancesToOpen.Count > 0)
        {
            for (int i = UnityEngine.Random.Range(0, possibleEntrancesToOpen.Count - 1); i < UnityEngine.Random.Range(i+1, openDoorProbability); i++)
            {
                amountOfRandomOpenEntrances++;
                possibleEntrancesToOpen[UnityEngine.Random.Range(0, possibleEntrancesToOpen.Count)].SetOpen(true);
            }
        }
    }
}
//The following part of the class finds and returns rooms
public partial class LevelGenerator : MonoBehaviour
{
    Tuple<Room, List<Room.Entrances.Entrance>> GetRandomRoom()
    {
        //This functions gets any of the rooms that are already spawned
        //It should make sure that it doesnt have something spawned in each direction

        List<Tuple<Room, List<Room.Entrances.Entrance>>> roomsWithOpenDoors = new List<Tuple<Room, List<Room.Entrances.Entrance>>>{};

        foreach(Section section in sections)
        {
            foreach (Room room in section.rooms)
            {
                List<Room.Entrances.Entrance> openEntrances = room.GetOpenUnspawnedEntrances();
                if (openEntrances.Count > 0)
                {
                    roomsWithOpenDoors.Add(new Tuple<Room, List<Room.Entrances.Entrance>>(room, openEntrances));
                }
            }
        }
        //! if rooms with open doors is empty, this will cause an error
        //! this will only happen if no rooms have open doors
        return roomsWithOpenDoors[UnityEngine.Random.Range(0, roomsWithOpenDoors.Count - 1)];
    }
    Tuple<Room, List<Room.Entrances.Entrance>> GetRandomRoomOfSection(int section)
    {
        //This functions gets any of the rooms that are already spawned
        //It should make sure that it doesnt have something spawned in each direction

        List<Tuple<Room, List<Room.Entrances.Entrance>>> roomsWithOpenDoors = new List<Tuple<Room, List<Room.Entrances.Entrance>>>{};
        foreach (Room room in sections[section].rooms)
        {
            List<Room.Entrances.Entrance> openEntrances = room.GetOpenUnspawnedEntrances();
            if (openEntrances.Count > 0)
            {
                roomsWithOpenDoors.Add(new Tuple<Room, List<Room.Entrances.Entrance>>(room, openEntrances));
            }
        }
        //! if rooms with open doors is empty, this will cause an error
        //! this will only happen if no rooms have open doors
        return roomsWithOpenDoors[UnityEngine.Random.Range(0, roomsWithOpenDoors.Count - 1)];
    }

    public List<Tuple<Vector2Int, Room>> GetAllConnectingRooms(Room origin)
    {
        List<Tuple<Vector2Int, Room>> temp = new List<Tuple<Vector2Int, Room>>();
        foreach (Room.Entrances.Entrance entrance in origin.GetDirections().entrances)
        {
            if (entrance.open && entrance.spawned)
            {
                temp.Add(FindAdjacentRoom(entrance.gridPos, entrance.dir));
            }
        }
        return temp;
    }
    public List<Tuple<Vector2Int, Vector2Int, Room>> GetAllAdjacentRooms(Room room)
    {
        List<Tuple<Vector2Int, Vector2Int, Room>> temp = new List<Tuple<Vector2Int,Vector2Int, Room>>();
        for(int x = 0; x < room.size.x / 20; x++)
        {
            Tuple<Vector2Int,Room> temp2 = FindAdjacentRoom(new Vector2Int((int)room.transform.position.x /20 + x, (int)room.transform.position.y/20 ) , Vector2Int.up);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, new Vector2Int((int)room.transform.position.x /20 + x, (int)room.transform.position.y/20) ,temp2.Item2));}
            temp2 = FindAdjacentRoom(new Vector2Int((int)room.transform.position.x/20 + x, (int)room.transform.position.y/20- (room.size.y / 20 - 1)), Vector2Int.down);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, new Vector2Int((int)room.transform.position.x/20 + x, (int)room.transform.position.y/20- (room.size.y / 20 - 1)) ,temp2.Item2));}
        }
        for(int y = 0; y < room.size.y / 20; y++)
        {
            Tuple<Vector2Int,Room> temp2 = FindAdjacentRoom(new Vector2Int((int)room.transform.position.x/20, (int)room.transform.position.y/20 - y), Vector2Int.left);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, new Vector2Int((int)room.transform.position.x/20, (int)room.transform.position.y/20 - y) ,temp2.Item2));}
            temp2 = FindAdjacentRoom((new Vector2(room.transform.position.x/20 + (room.size.x/20 - 1), room.transform.position.y/20 - y)).ToV2Int(), Vector2Int.right);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, (new Vector2(room.transform.position.x/20 + (room.size.x/20 - 1), room.transform.position.y/20 - y)).ToV2Int() ,temp2.Item2));}
        }
        return temp;
    }
    public List<Tuple<Vector2Int, Vector2Int, Room>> GetAllAdjacentRooms(Room room, Section section)
    {
        List<Tuple<Vector2Int, Vector2Int, Room>> temp = new List<Tuple<Vector2Int,Vector2Int, Room>>();
        for(int x = 0; x < room.size.x / 20; x++)
        {
            Tuple<Vector2Int,Room> temp2 = FindAdjacentRoom(new Vector2Int((int)room.transform.position.x /20 + x, (int)room.transform.position.y/20), Vector2Int.up, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, new Vector2Int((int)room.transform.position.x /20 + x, (int)room.transform.position.y/20) ,temp2.Item2));}
            temp2 = FindAdjacentRoom(new Vector2Int((int)room.transform.position.x/20 + x, (int)room.transform.position.y/20 + (room.size.y / 20 - 1)), Vector2Int.down, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, new Vector2Int((int)room.transform.position.x/20 + x, (int)room.transform.position.y/20 + (room.size.y / 20 - 1)) ,temp2.Item2));}
        }
        for(int y = 0; y < room.size.y / 20; y++)
        {
            Tuple<Vector2Int,Room> temp2 = FindAdjacentRoom(new Vector2Int((int)room.transform.position.x/20, (int)room.transform.position.y/20 + y), Vector2Int.left, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, new Vector2Int((int)room.transform.position.x/20, (int)room.transform.position.y/20 + y) ,temp2.Item2));}
            temp2 = FindAdjacentRoom((new Vector2(room.transform.position.x/20 + (room.size.x/20 - 1), room.transform.position.y/20 + y)).ToV2Int(), Vector2Int.right, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, (new Vector2(room.transform.position.x/20 + (room.size.x/20 - 1), room.transform.position.y/20 + y)).ToV2Int() ,temp2.Item2));}

        }
        return temp;
    }
    public Tuple<Vector2Int, Room> FindAdjacentRoom(Vector2Int origin, Vector2Int direction)
    {
        return new Tuple<Vector2Int, Room> (origin + direction, FindRoomOfPosition(origin + direction));
    }

    public Tuple<Vector2Int, Room> FindAdjacentRoom(Vector2Int origin, Vector2Int direction, Section section)
    {
        return new Tuple<Vector2Int, Room> (origin + direction, FindRoomOfPosition(origin + direction, section));
    }
    Room FindRoomOfPosition(Vector2Int position, Section section)
    {
        for(int i = 0; i < section.rooms.Count; i++)
        {
            if(section.rooms[i].transform.position.ToV2Int()/20 == position)
            {
                return section.rooms[i];
            }
        }
        return null;
    }
    public Room FindRoomOfPosition(Vector2Int position)
    {
        for(int i = 0; i < roomGrid.Count; i++)
        {
            if(roomGrid[i].Item1 == position)
            {
                return roomGrid[i].Item2;
            }
        }
        return null;
    }
    public void DestroyLevel()
    {
        for(int i = 0; i < sections.Count; i++)
        {
            for(int j = sections[i].rooms.Count -1; j >= 0; j--)
            {
                Destroy(sections[i].rooms[j].gameObject);
            }
            sections[i].rooms.Clear();
        }
        sections.Clear();
        numberOfRooms = 1;
    }
    public void ToggleRenderSections()
    {
        renderSections = true;
    }
    private void OnRenderObject() 
    {
        if(!renderSections)
        {
            for(int i = 0; i < sections.Count; i++)
            {
                float percentageThroughSections = (float)i / sections.Count;
                Color sectionColor = Color.HSVToRGB(percentageThroughSections, 1, 1);

                for(int j = 0; j < sections[i].rooms.Count; j++)
                {
                    GLFunctions.DrawSquare(sections[i].rooms[j].transform.position - new Vector3(10, 10, 0), sections[i].rooms[j].size, transform, sectionColor);
                }
            }
        }
    }
    private void OnDrawGizmos() 
    {
        foreach(Section section in sections)
        {
            foreach(Room room in section.rooms)
            {
                Gizmos.color = Color.red;
                Vector3 centeredPosition = room.transform.position - new Vector3(10,10,0);
                Gizmos.DrawLine(centeredPosition, centeredPosition + new Vector3(room.size.x, 0, 0));
                Gizmos.DrawLine(centeredPosition + new Vector3(room.size.x, 0, 0), centeredPosition + new Vector3(room.size.x, -room.size.y + 20, 0));
                Gizmos.DrawLine(centeredPosition + new Vector3(room.size.x, -room.size.y +20, 0), centeredPosition + new Vector3(0, -room.size.y + 20 , 0));
                Gizmos.DrawLine(centeredPosition + new Vector3(0, -room.size.y +20, 0), centeredPosition);
                foreach(Room.Entrances.Entrance entrance in room.directions.entrances)
                {
                    if(entrance.open && entrance.spawned) //This is a finished door
                    {
                        Gizmos.color = Color.green;
                    }
                    else if(entrance.open && !entrance.spawned) //This is a door that is waiting to spawn another
                    {
                        Gizmos.color = Color.blue;
                    }
                    else if(!entrance.open && entrance.spawned) //This room was linked, can be black as well
                    {
                        //Gizmos.color = Color.magenta;
                        Gizmos.color = Color.black;
                    }
                    else //This door is completely locked
                    {
                        Gizmos.color = Color.black;
                    }
                    Gizmos.DrawLine(entrance.gridPos.ToV3() * 20 + entrance.dir.ToV3() * 5, entrance.gridPos.ToV3() * 20 + entrance.dir.ToV3() * 10);
                }
            }
        }
    }
}