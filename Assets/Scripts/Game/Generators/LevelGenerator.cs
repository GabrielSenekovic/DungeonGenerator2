using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public partial class LevelGenerator : MonoBehaviour
{
    [System.Serializable]public class Section
    {
        public List<Room> rooms = new List<Room>();
    }
    [SerializeField]List<Section> sections = new List<Section>();

    [System.Serializable]public struct RoomGridEntry
    {
        public Vector2Int position;
        public Room room;
        public RoomGridEntry(Vector2Int position_in, Room room_in)
        {
            position = position_in; room = room_in;
        }
    }
    [SerializeField]List<RoomGridEntry> roomGrid = new List<RoomGridEntry> { }; //Separate rooms into rooms and roomGrid. roomGrid is all positions on the grid that are occupied. This is so that I wont have to look through the list of rooms
    
    [System.Serializable]public class debugroomposition
    {
        public Vector2Int pos;
        public string name;
    }
    public List<debugroomposition> roomPositions = new List<debugroomposition>(); //! only for debugging
    List<Tuple<Vector2Int, Room>> surroundingPositions = new List<Tuple<Vector2Int, Room>>();
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

    public Texture2D map;

    public int leftestPoint = 0;
    public int northestPoint = 0;
    public int rightestPoint = 0;
    public int southestPoint = 0;
    public Vector2Int sizeOfMap = Vector2Int.zero;

    public void GenerateStartArea()
    {
        //Called when not in the level

        List<Room.RoomTemplate> templates = new List<Room.RoomTemplate>();
        sections[0].rooms[0].Initialize(new Vector2Int(20,20), false, 0, ref templates, false);
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
    public List<Room.RoomTemplate> GenerateTemplates(LevelData data, Vector2Int RoomSize, Vector2Int amountOfRooms, Vector2Int amountOfSections)
    {
        roomGrid.Clear(); leftestPoint = 0; northestPoint = 0; southestPoint = 0; rightestPoint = 0;
        sections.Clear();
        UnityEngine.Random.InitState(GameData.Instance.levelConstructionSeed);

        List<Room.RoomTemplate> templates = new List<Room.RoomTemplate>();

        sections.Add(new Section()); 
        sections[0].rooms.Add(Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, transform));
        roomGrid.Add(new RoomGridEntry(Vector2Int.zero, sections[0].rooms[0]));
        sections[0].rooms[0].Initialize(RoomSize, true, 0, ref templates, false);

        SpawnRooms(UnityEngine.Random.Range((int)(amountOfRooms.x + sections[0].rooms.Count),
                                (int)(amountOfRooms.y + sections[0].rooms.Count)), UnityEngine.Random.Range((int)(amountOfSections.x),
                                (int)(amountOfSections.y)), RoomSize, data, ref templates);

       // Debug.Log("RoomGrid size " + roomGrid.Count);
        FinishRooms(ref templates); //Touch up, adding entrances and stuff
        GenerateMap(ref templates);
        return templates;
    }
    void GenerateLevel(LevelManager level, ref List<Room.RoomTemplate> templates)
    {
        System.DateTime before = System.DateTime.Now;
        GenerateSurroundings(ref templates);
        BuildRooms(ref templates);

        level.firstRoom = sections[0].rooms[0];
        level.lastRoom = sections[sections.Count-1].rooms[sections[sections.Count-1].rooms.Count - 1];

        //AdjustRoomTypes(level.l_data);
        //AdjustEntrances(RoomSize);

        System.DateTime after = System.DateTime.Now; 
        System.TimeSpan duration = after.Subtract(before);
        Debug.Log("<color=blue>Time to generate: </color>" + duration.TotalMilliseconds + " milliseconds, which is: " + duration.TotalSeconds + " seconds");
    }
    void GenerateMap(ref List<Room.RoomTemplate> templates)
    {
        sizeOfMap = new Vector2Int((rightestPoint+1) - leftestPoint, northestPoint - (southestPoint-1)) * new Vector2Int(20,20);
        //Plus twenty because otherwise its just the upper left corner of that square. I want the lowermost point of it
        int count = 0;
        Debug.Log("Size of map:" + sizeOfMap);
        map = new Texture2D(sizeOfMap.x, sizeOfMap.y, TextureFormat.ARGB32, false);

        for(int i = 0; i < sections.Count; i++)
        {
            for(int j = 0; j < sections[i].rooms.Count; j++)
            {
                DebugLog.AddToMessage("Getting map image", sections[i].rooms[j].name);
                Room.RoomTemplate template = templates[count];
                Texture2D tex = sections[i].rooms[j].CreateMaps(ref template);
                int kStart = (int)sections[i].rooms[j].transform.localPosition.x + Mathf.Abs(leftestPoint * 20);
                int lStart = (int)sections[i].rooms[j].transform.localPosition.y + Mathf.Abs((southestPoint - 1)*20) - 1; //Why the fuck do i have to subtract 1, what (oh maybe cuz southestpoint is 19 off not 20)
                //- Mathf.Abs(southestPoint * 20);
                Debug.Log("Position: " + sections[i].rooms[j].transform.localPosition + " K Start: " + kStart + " L Start: " + lStart);
                for(int k = 0; k < tex.width ; k++)
                {
                    for(int l = 0; l < tex.height ; l++)
                    {
                       // map.SetPixel(k + kStart,l + lStart, tex.GetPixel(k,l));
                        map.SetPixel(k + kStart,lStart-l, tex.GetPixel(k,l));
                    }
                }
                count++;
                DebugLog.PublishMessage();
            }
        }
        map.Apply();
        map.filterMode = FilterMode.Point;
        UIManager.Instance.currentMap = map;
    }
    void GenerateSurroundings(ref List<Room.RoomTemplate> templates)
    {
        GameObject surroundings = new GameObject("Surroundings");
        surroundings.transform.parent = gameObject.transform;
        //Go through every single position saved and spawned, and check next to them. If theres nothing in that specific position, spawn a room there like usual, on a higher level
        for(int i = 0; i < roomGrid.Count; i++)
        {
            for(int x = -1; x < 2; x++)
            {
                for(int y = -1; y < 2; y++)
                {
                    if(!CheckIfCoordinatesOccupied(roomGrid[i].position + new Vector2Int(x,y)) && !surroundingPositions.Any(j => j.Item1 == roomGrid[i].position + new Vector2Int(x,y)))
                    {
                        //Create room here
                        DebugLog.AddToMessage("Generating", "Surroundings #" + surroundings.transform.childCount);
                        Room temp = Instantiate(RoomPrefab, transform);
                        temp.gameObject.name = "Surrounding";
                        temp.transform.parent = surroundings.transform;
                        temp.Initialize(roomGrid[i].position * 20 + new Vector2Int(x,y) * 20, new Vector2Int(20,20), false, -1, ref templates, true);
                        surroundingPositions.Add(new Tuple<Vector2Int, Room>(roomGrid[i].position + new Vector2Int(x,y),temp));
                        DebugLog.PublishMessage();
                    }
                }
            }
        }
        int limit = surroundingPositions.Count;
        for(int i = 0; i < limit; i++)
        {
            for(int x = -1; x < 2; x++)
            {
                for(int y = -1; y < 2; y++)
                {
                    if(!CheckIfCoordinatesOccupied(surroundingPositions[i].Item1 + new Vector2Int(x,y)) && !surroundingPositions.Any(j => j.Item1 == surroundingPositions[i].Item1 + new Vector2Int(x,y)))
                    {
                        //Create room here
                        DebugLog.AddToMessage("Generating", "Surroundings #" + surroundings.transform.childCount);
                        Room temp = Instantiate(RoomPrefab, transform);
                        temp.gameObject.name = "Surrounding";
                        temp.transform.parent = surroundings.transform;
                        temp.Initialize(surroundingPositions[i].Item1 * 20 + new Vector2Int(x,y) * 20, new Vector2Int(20,20), false, -1, ref templates, true);
                        surroundingPositions.Add(new Tuple<Vector2Int, Room>(surroundingPositions[i].Item1 + new Vector2Int(x,y),temp));
                        DebugLog.PublishMessage();
                    }
                }
            }
        }
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
                DebugLog.AddToMessage("Room", j.ToString());
                DebugLog.AddToMessage("Section", i.ToString());
                
                //Vector2Int currentRoomSize = new Vector2Int(UnityEngine.Random.Range(1,4),UnityEngine.Random.Range(1,4)); //in grid space
                Vector2Int currentRoomSize = new Vector2Int(5,5);
                
                Tuple<Room, List<Room.Entrances.Entrance>> originRoom = new Tuple<Room, List<Room.Entrances.Entrance>>(new Room(), new List<Room.Entrances.Entrance>(){});
                try
                {
                    if(j != 0) //If not the first room every section
                    {
                        originRoom = GetRandomRoomOfSection(i); //! If there are no open entrances in any room, the catch will be executed
                        DebugLog.AddToMessage("Spawning from", originRoom.Item1.transform.position.ToString());
                    }
                    else if(i != 0) //And if not the first room of the first section
                    {
                        originRoom = new Tuple<Room, List<Room.Entrances.Entrance>> (sections[i-1].rooms[sections[i-1].rooms.Count - 1], sections[i-1].rooms[sections[i-1].rooms.Count - 1].directions.entrances);
                        OpenAvailableEntrances(originRoom.Item1);
                    }
                }catch{DebugLog.TerminateMessage("Could no longer spawn new rooms");break;}

                sections[i].rooms.Add(Instantiate(RoomPrefab, transform));
                sections[i].rooms[j].name = "Room #" + (numberOfRooms+1); numberOfRooms++;
                DebugLog.AddToMessage("Name", sections[i].rooms[j].name);

                bool indoors = false;

                Vector2Int gridPositionWhereOriginRoomConnects = Vector2Int.zero;
                Vector2Int gridPositionWhereNewRoomConnects = GetNewRoomCoordinates(sections[i].rooms[j], originRoom.Item1.transform.position.ToV2Int(), originRoom.Item2, ref currentRoomSize, ref gridPositionWhereOriginRoomConnects);
                sections[i].rooms[j].Initialize(gridPositionWhereNewRoomConnects, currentRoomSize * 20, indoors, i, ref templates, false);
                sections[i].rooms[j].roomData.stepsAwayFromMainRoom = originRoom.Item1.roomData.stepsAwayFromMainRoom + 1;
                sections[i].rooms[j].originalPosition = gridPositionWhereNewRoomConnects;
                if(sections[i].rooms[j].roomData.stepsAwayFromMainRoom > furthestDistanceFromSpawn)
                {
                    furthestDistanceFromSpawn = sections[i].rooms[j].roomData.stepsAwayFromMainRoom;
                }
                bool locked = j == 0 && i > 0 ? true:false;
                ActivateEntrances(originRoom.Item1, sections[i].rooms[j], gridPositionWhereOriginRoomConnects, gridPositionWhereNewRoomConnects, locked);
                LinkRoom(sections[i].rooms[j], RoomSize);
                OpenRandomEntrances(sections[i].rooms[j], data.openDoorProbability);
                for(int x = 0; x < Mathf.Abs(currentRoomSize.x); x++)
                {
                    for(int y = 0; y < Mathf.Abs(currentRoomSize.y); y++)
                    {
                        roomGrid.Add(new RoomGridEntry(gridPositionWhereNewRoomConnects/20 + 
                            new Vector2Int(x * (int)Mathf.Sign(currentRoomSize.x), y * (int)Mathf.Sign(currentRoomSize.y)), sections[i].rooms[j]));
                        k++;
                        if(roomGrid[roomGrid.Count -1].position.x <= leftestPoint)
                        {
                            leftestPoint = roomGrid[roomGrid.Count -1].position.x;
                        }
                        if(roomGrid[roomGrid.Count -1].position.y >= northestPoint)
                        {
                            northestPoint = roomGrid[roomGrid.Count -1].position.y;
                        }
                        if(roomGrid[roomGrid.Count -1].position.x >= rightestPoint)
                        {
                            rightestPoint = roomGrid[roomGrid.Count -1].position.x;
                        }
                        if(roomGrid[roomGrid.Count -1].position.y <= southestPoint)
                        {
                            southestPoint = roomGrid[roomGrid.Count -1].position.y;
                        }
                    }
                }
                DebugLog.PublishMessage();
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
    void FinishRooms(ref List<Room.RoomTemplate> templates)
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
                count++;
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
                DebugLog.AddToMessage("Generating", sections[i].rooms[j].name);
                Room.RoomTemplate template = templates[count];
                sections[i].rooms[j].CreateRoom(ref template, Resources.Load<Material>("Materials/Wall"), Resources.Load<Material>("Materials/Ground"));
                SaveWallVertices(ref templates, template, sections[i].rooms[j]);
                count++;
                DebugLog.PublishMessage();
            }
        }
        for(int i = 0; i < surroundingPositions.Count; i++)
        {
            Room.RoomTemplate template = templates[count];
            surroundingPositions[i].Item2.CreateRoom(ref template, Resources.Load<Material>("Materials/Wall"), Resources.Load<Material>("Materials/Ground"));
            count++;
        }
        PlantFlora(ref templates);
    }
    void SaveWallVertices(ref List<Room.RoomTemplate> templates, Room.RoomTemplate originTemplate, Room origin)
    {
        //Set the entrance vertices of all adjacent rooms
        List<Tuple<Vector2Int, Vector2Int, Room>> roomList = GetAllAdjacentRooms(origin);
        Debug.Log("The room: " + origin.gameObject.name + " found this many neighbors: " + roomList.Count);
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
                        Debug.Log("Saving wall vertices from: " + roomList[k].Item2 + " to: " + roomList[k].Item1);
                        Room.RoomTemplate adjTemplate = templates[n-1];
                        
                        Vector2 direction = (new Vector2(roomList[k].Item1.x, roomList[k].Item1.y) - roomList[k].Item2).normalized;
                        Tuple<bool, Room.Entrances.Entrance> adjEntrance = roomList[k].Item3.directions.GetEntrance(roomList[k].Item1, -direction.ToV2Int());
                        Tuple<bool, Room.Entrances.Entrance> myEntrance = origin.directions.GetEntrance(roomList[k].Item1 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);

                        roomList[k].Item3.directions.SetEntranceVertices(ref adjTemplate, originTemplate, adjEntrance.Item2, myEntrance.Item2, origin.transform.position, roomList[k].Item3.transform.position);
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

                    Vegetation grass = lawn.AddComponent<Vegetation>();
                    grass.area = templates[j].size;
                    grass.grassPerTile = 3;
                    grass.burningSpeed = 0.001f;
                    grass.fireColor = Color.red;
                    grass.grassRotation = new Vector3(-90, 90, -90);
                    grass.layerMask = ~0;
                    grass.VFX_Burning = Resources.Load<UnityEngine.VFX.VisualEffectAsset>("VFX/Burning");

                    lawn.transform.localPosition = new Vector3(-10, -10, -0.5f);

                    grass.PlantFlora(sections[i].rooms[j]);
                    sections[i].rooms[j].grass = grass;
                }
            }
        }
        for(int i = 0; i < surroundingPositions.Count; i++)
        {
            GameObject lawn = new GameObject("Lawn");
            lawn.transform.parent = surroundingPositions[i].Item2.transform;

            Vegetation grass = lawn.AddComponent<Vegetation>();
            grass.area = surroundingPositions[i].Item2.size;
            grass.grassPerTile = 3;
            grass.burningSpeed = 0.001f;
            grass.fireColor = Color.red;
            grass.grassRotation = new Vector3(-90, 90, -90);
            grass.layerMask = ~0;
            grass.VFX_Burning = Resources.Load<UnityEngine.VFX.VisualEffectAsset>("VFX/Burning");

            lawn.transform.localPosition = new Vector3(-10, -10, -0.5f);

            grass.PlantFlora(surroundingPositions[i].Item2);
            surroundingPositions[i].Item2.grass = grass;
        }
    }

    void ActivateEntrances(Room origin, Room newRoom, Vector2Int gridPositionWhereOriginConnects, Vector2Int gridPositionWhereNewRoomConnects, bool locked)
    {
        //Get entrance in adjacent room that points to this room
        //Get entrance in this room that points to adjacent room
        //!same here as linkrooms, you need to get the origin gridposition that is closest to newroom
        //!the newroom position is already by nature closest, since its spawned from the direction of a door

        //!then you have to make sure that these two rooms connect anywhere else that wasnt intended
        DebugLog.AddToMessage("Step", "Activating Entrances");
        Vector2 direction = (gridPositionWhereOriginConnects - (Vector2)gridPositionWhereNewRoomConnects/20).normalized;
        DebugLog.AddToMessage("Origin pos", origin.transform.position.ToString());
        DebugLog.AddToMessage("Newroom pos", gridPositionWhereNewRoomConnects.ToString());
        //DebugLog.AddToMessage("Grid position where origin connects", gridPositionWhereOriginConnects.ToString());
        DebugLog.AddToMessage("Direction to attach", direction.ToString());
        //Debug.Log(direction);

        Tuple<bool, Room.Entrances.Entrance> adjEntrance = newRoom.directions.GetEntrance(gridPositionWhereNewRoomConnects/20, direction.ToV2Int());

        if(adjEntrance.Item1) //If i found the correct entrance
        {
            Tuple<bool, Room.Entrances.Entrance> myEntrance = origin.directions.GetEntrance(gridPositionWhereNewRoomConnects/20 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);
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
                DebugLog.TerminateMessage("COULDNT FIND THE ORIGIN ENTRANCE DESPITE BEING ADJACENT");
            }
        }
        else
        {
            DebugLog.TerminateMessage("COULDNT FIND THE ADJACENT ENTRANCE DESPITE BEING ADJACENT");
        }
    }
    Vector2Int GetNewRoomCoordinates(Room room, Vector2Int originCoordinates, List<Room.Entrances.Entrance> openEntrances, ref Vector2Int roomSize, ref Vector2Int gridPositonWhereNewRoomConnects) //Roomsize is in grid size
    {
        DebugLog.AddToMessage("Step", "Getting new room coordinates");
        List<Tuple<Vector2Int, Vector2Int>> possibleCoordinates = new List<Tuple<Vector2Int, Vector2Int>> { };
        foreach(Room.Entrances.Entrance entrance in openEntrances)
        {
            if(!CheckIfCoordinatesOccupied(new Vector2Int(entrance.gridPos.x + entrance.dir.x, entrance.gridPos.y + entrance.dir.y)))
            {
                possibleCoordinates.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(entrance.gridPos.x * 20 + entrance.dir.x * 20, entrance.gridPos.y * 20 + entrance.dir.y * 20), entrance.gridPos));
            }
        }
        int index = possibleCoordinates.GetRandomIndex();
        gridPositonWhereNewRoomConnects = possibleCoordinates[index].Item2;
        //!When its a big room, you decide the position first, then you see if you can grow out like intended. If you cant, then change the roomSize. Thats why its passed as a referenc
        AttemptExpansion(room, possibleCoordinates[index].Item1 / 20, ref roomSize);
        return possibleCoordinates[index].Item1;
    }

    void AttemptExpansion(Room room, Vector2Int origin, ref Vector2Int roomSize)
    {
        DebugLog.AddToMessage("Step", "Attempting expansion");

        List<List<RoomGridEntry>> potentialExpansions = new List<List<RoomGridEntry>>();
        List<Vector2Int> potentialSizes = new List<Vector2Int>();

        for(int i = 0; i < 4; i++) //Go through every direction of expansion
        {
            potentialExpansions.Add(new List<RoomGridEntry>());
            potentialSizes.Add(roomSize * Math.diagonals[i]);
            int x = 1, y = 1;
            while (x < Mathf.Abs(potentialSizes[i].x) || y < Mathf.Abs(potentialSizes[i].y))
            { //Run as long as either x or y haven't reach the ends of the room
                OnAttemptExpansion(room, ref x, ref y, i, origin, new Vector2Int(1,0), ref potentialSizes, ref potentialExpansions);
                OnAttemptExpansion(room, ref y, ref x, i, origin, new Vector2Int(0,1), ref potentialSizes, ref potentialExpansions);
            }
        }
        List<RoomGridEntry> choice = new List<RoomGridEntry>();
        roomSize = new Vector2Int(1,1);
        for (int i = 0; i < 4; i++) //Check which room is the biggest
        {
            if(potentialExpansions[i].Count > choice.Count)
            {
                choice = potentialExpansions[i];
                roomSize = potentialSizes[i];
            }
        }
        for(int i = 0; i < choice.Count; i++)
        {
            //Debug.Log("Adding: " + choice[i].position);
        }
       // roomGrid.AddRange(choice);
       // Debug.Log("Added " + choice.Count + " amount of rooms");
        DebugLog.AddToMessage("New Size", roomSize.ToString());
    }
    void OnAttemptExpansion(Room room, ref int currentSide, ref int otherSide, int direction, Vector2Int origin, Vector2Int coordinate, ref List<Vector2Int> potentialSizes, ref List<List<RoomGridEntry>> potentialExpansions)
    {
        //"currentSide" is either x or y and "otherSide" is the other
        //"coordinate" determines if it's going in the x axis or the y axis

        if (currentSide < (coordinate.y == 0 ? Mathf.Abs(potentialSizes[direction].x) : Mathf.Abs(potentialSizes[direction].y)) && 
        !CheckIfCoordinatesOccupied(origin + new Vector2Int(currentSide * Math.diagonals[direction].x * coordinate.x, currentSide * Math.diagonals[direction].y * coordinate.y))) 
        //If there is a free spot to the side, then try to expand to the side
        {
            List<RoomGridEntry> temp = new List<RoomGridEntry>();
            for (int j = 0; j < otherSide; j++) //Go through all to the side. All of them have to be free
            {
                Vector2Int position = origin + new Vector2Int(currentSide * Math.diagonals[direction].x * coordinate.x + j * Math.diagonals[direction].x * coordinate.y, 
                                                              currentSide * Math.diagonals[direction].y * coordinate.y + j * Math.diagonals[direction].y * coordinate.x);
                if (!CheckIfCoordinatesOccupied(position))
                {
                    temp.Add(new RoomGridEntry(position, room));
                }
                else
                {
                    //If it was occupied, set size to what x is
                    //Debug.Log("Clearing x list");
                    temp.Clear();
                    potentialSizes[direction] = new Vector2Int(currentSide * Math.diagonals[direction].x * coordinate.x + potentialSizes[direction].x * coordinate.y, 
                                                               currentSide * Math.diagonals[direction].y * coordinate.y + potentialSizes[direction].y * coordinate.x);
                }
            }
            potentialExpansions[direction].AddRange(temp);
            if(currentSide < (coordinate.y == 0 ? Mathf.Abs(potentialSizes[direction].x) : Mathf.Abs(potentialSizes[direction].y)))
            {
                currentSide++;
            }
        }
        else
        {
            potentialSizes[direction] = new Vector2Int(currentSide * Math.diagonals[direction].x * coordinate.x + potentialSizes[direction].x * coordinate.y, 
                                                       currentSide * Math.diagonals[direction].y * coordinate.y + potentialSizes[direction].y * coordinate.x);
        }
    }

    bool CheckIfCoordinatesOccupied(Vector2Int roomPosition)
    {
        foreach(RoomGridEntry room in roomGrid)
        {
            if(room.position == roomPosition)
            {
                return true;
            } 
        }
        //Debug.Log(roomPosition + " is not occupied");
        return false;
    }

    void LinkRoom(Room room, Vector2 RoomSize)
    {
        //This function checks if this given room has another spawned room in any direction that it must link to, before it decides if it should link anywhere else
        //It does this by checking if a room in any direction has an open but not spawned gate in its own direction, in which case it opens its own gate in that direction
        DebugLog.AddToMessage("Step", "Linking Rooms");
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
                    DebugLog.TerminateMessage("COULDNT FIND THE ORIGIN ENTRANCE DESPITE BEING ADJACENT");
                }
            }
            else
            {
                DebugLog.TerminateMessage("COULDNT FIND THE ADJACENT ENTRANCE DESPITE BEING ADJACENT");
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
            int i = UnityEngine.Random.Range(0, possibleEntrancesToOpen.Count - 1);
            int limit = UnityEngine.Random.Range(i+1, openDoorProbability) + openDoorProbability;
            for (; i < limit; i++)
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
        DebugLog.AddToMessage("Step", "Getting a random origin room of section: " + section);
        List<Tuple<Room, List<Room.Entrances.Entrance>>> roomsWithOpenDoors = new List<Tuple<Room, List<Room.Entrances.Entrance>>>{};
        foreach (Room room in sections[section].rooms)
        {
            List<Room.Entrances.Entrance> openEntrances = room.GetOpenUnspawnedEntrances();
            if (openEntrances.Count > 0)
            {
                roomsWithOpenDoors.Add(new Tuple<Room, List<Room.Entrances.Entrance>>(room, openEntrances));
            }
        }
        DebugLog.AddToMessage("Rooms with open doors", roomsWithOpenDoors.Count.ToString());
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
        //Item1 is the adjacent gridposition, Item2 is the gridposition it connects to in the origin room
    {
        DebugLog.AddToMessage("Substep", "Getting all adjacent rooms");
        List<Tuple<Vector2Int, Vector2Int, Room>> temp = new List<Tuple<Vector2Int,Vector2Int, Room>>();
        
        for(int x = 0; x < Mathf.Abs(room.size.x / 20); x++)
        { //!Looking for rooms vertically
            int _x = x * (int)Mathf.Sign(room.size.x);
            if(Mathf.Sign(room.size.x) == -1)
            {
                _x -= (room.size.x/20) + 1;
            }
            Vector2Int direction = new Vector2Int(0, 1);

            Vector2Int checkPosition = new Vector2Int((int)room.transform.position.x /20 + _x, (int)room.transform.position.y/20 );
            Tuple<Vector2Int,Room> temp2 = FindAdjacentRoom( checkPosition , direction);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, checkPosition,temp2.Item2));DebugLog.AddToMessage("Just added on first X", checkPosition.ToString());}

            checkPosition = new Vector2Int((int)room.transform.position.x/20 + _x, (int)room.transform.position.y/20 - (room.size.y / 20 - 1 * (int)Mathf.Sign(room.size.y))*(int)Mathf.Sign(room.size.y));
            temp2 = FindAdjacentRoom(checkPosition, -direction);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, checkPosition ,temp2.Item2));DebugLog.AddToMessage("Just added on second X", checkPosition.ToString());} 
        }
        for(int y = 0; y < Mathf.Abs(room.size.y / 20); y++)
        { //!Looking for rooms horizontally
            int _y = y * (int)Mathf.Sign(room.size.y);
            if(Mathf.Sign(room.size.y) == 1)
            {
                _y -= (room.size.y/20) - 1;
            }
            Vector2Int direction = new Vector2Int(-1, 0);

            Vector2Int checkPosition = new Vector2Int((int)room.transform.position.x/20, (int)room.transform.position.y/20 + _y);
            Tuple<Vector2Int,Room> temp2 = FindAdjacentRoom(checkPosition, direction);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, checkPosition ,temp2.Item2));DebugLog.AddToMessage("Just added on first Y", checkPosition.ToString());}

            checkPosition = (new Vector2(room.transform.position.x/20 + (room.size.x/20 - 1 * (int)Mathf.Sign(room.size.x))* (int)Mathf.Sign(room.size.x), room.transform.position.y/20 + _y)).ToV2Int();
            temp2 = FindAdjacentRoom(checkPosition, -direction);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, Room>(temp2.Item1, checkPosition ,temp2.Item2));DebugLog.AddToMessage("Just added on second Y", checkPosition.ToString());}
        }
        DebugLog.AddToMessage("Adjacent rooms found", temp.Count.ToString());
        for(int i = 0; i < temp.Count; i++)
        {
            DebugLog.AddToMessage("Adjacent Room " + i, temp[i].Item1.ToString() + " belonging to: " + temp[i].Item3.name);
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
            if(roomGrid[i].position == position)
            {
                return roomGrid[i].room;
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
                    GLFunctions.DrawSquareFromCorner(sections[i].rooms[j].originalPosition.ToV3() - new Vector3(10, 10, 0), sections[i].rooms[j].size, transform, sectionColor);
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