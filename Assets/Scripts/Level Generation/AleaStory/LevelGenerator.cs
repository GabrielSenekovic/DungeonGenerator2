﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Random = UnityEngine.Random;
using SectionData = LevelData.SectionData;
using Color = UnityEngine.Color;

public partial class LevelGenerator : MonoBehaviour, ILevelGenerator
{
    int numberOfRooms = 1;

    int furthestDistanceFromSpawn = 0;

    int amountOfRandomOpenEntrances;

    bool renderSections;

    public Texture2D map;

    public int leftestPoint = 0;
    public int northestPoint = 0;
    public int rightestPoint = 0;
    public int southestPoint = 0;
    public Vector2Int sizeOfMap = Vector2Int.zero;

    [SerializeField] FurnitureDatabase furnitureDatabase;
    [SerializeField] MaterialDatabase materialDatabase;

    RoomTemplate fullTemplate; //Saved in case of build house. Only to be used now when we dont generate HQ from a seed yet.

    public void OnGenerateOneRoom(bool withEntrances, SettlementData settlementData, string outsideInstructions = "", string houseInstructions = "", Vector2Int? size = null)
    {
        LevelData currentLevel = DunGenes.Instance.gameData.CurrentLevel;
        Room currentRoom = currentLevel.sections[0].rooms[0];
        currentRoom.OnReset();
        List<RoomTemplate> templates = new List<RoomTemplate>();
        currentLevel.sectionData.Add(new SectionData());
        currentLevel.sectionData[0].rooms.Add(new RoomData());
        RoomData currentRoomData = currentLevel.sectionData[0].rooms[0];
        currentRoomData.Initialise(size != null? size.Value : new Vector2Int(20,20), 0, ref templates, outsideInstructions, houseInstructions);
        if(withEntrances)
        {
            currentRoomData.GetDirections().ActivateAllEntrances();
        }
        templates[0].AddEntrancesToRoom(currentRoomData.GetDirections());
        templates[0].IdentifyWalls();
        RoomTemplate template = templates[0];
        RoomTemplateReader reader = new RoomTemplateReader(template, currentRoom.transform);
        reader.CreateLevel(ref template, Resources.Load<Material>("Materials/Ground"), materialDatabase, settlementData,
            currentRoomData.GetDirections());
        currentRoom.CreateRoom(ref template, Resources.Load<Material>("Materials/Ground"), furnitureDatabase);
        currentRoom.roomData = currentRoomData;
        currentRoomData.CreateMaps(template);

        CameraMovement.SetCameraAnchor(new Vector2(currentRoom.transform.position.x, currentRoom.transform.position.x + Mathf.Abs(currentRoom.roomData.size.x) - 20),
                new Vector2(-currentRoom.transform.position.y, -(currentRoom.transform.position.y - Mathf.Abs(currentRoom.roomData.size.y) + 20)));

        fullTemplate = template;
    }
    public void GenerateHouse(string instructions, SettlementData settlementData)
    {
        if(fullTemplate == null) { return; }
        LevelData currentLevel = DunGenes.Instance.gameData.CurrentLevel;
        Room currentRoom = currentLevel.sections[0].rooms[0];
        currentRoom.OnReset();
        List<RoomTemplate> templates = new List<RoomTemplate>();
        RoomData currentRoomData = currentLevel.sectionData[0].rooms[0];

        fullTemplate.AddHouse(instructions);
        fullTemplate.IdentifyWalls();
        RoomTemplateReader reader = new RoomTemplateReader(fullTemplate, currentRoom.transform);
        reader.CreateLevel(ref fullTemplate, Resources.Load<Material>("Materials/Ground"), materialDatabase, settlementData,
            currentRoomData.GetDirections());
        currentRoom.CreateRoom(ref fullTemplate, Resources.Load<Material>("Materials/Ground"), furnitureDatabase);
        currentRoom.roomData = currentRoomData;
        currentRoomData.CreateMaps(fullTemplate);
    }

    public void GenerateStartArea(SettlementData settlementData)
    {
        OnGenerateOneRoom(false, settlementData);
    }
    public void GenerateTemplates(LevelData data, Vector2Int RoomSize, Vector2Int amountOfRooms, Vector2Int amountOfSections)
    {
        data.roomGrid.Clear(); leftestPoint = 0; northestPoint = 0; southestPoint = 0; rightestPoint = 0;
        data.sectionData.Clear();
        
        Random.InitState(DunGenes.Instance.gameData.levelConstructionSeed);

        List<RoomTemplate> templates = new List<RoomTemplate>();

        data.sectionData.Add(new SectionData());
        data.sectionData[0].rooms.Add(new RoomData());
        //data.sections[0].rooms.Add(Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, transform));
        data.roomGrid.Add(new LevelData.RoomGridEntry(Vector2Int.zero, data.sectionData[0].rooms[0]));
        data.sectionData[0].rooms[0].Initialise(RoomSize, 0, ref templates);

        SpawnRooms(Random.Range((int)(amountOfRooms.x + data.sectionData[0].rooms.Count),
                                (int)(amountOfRooms.y + data.sectionData[0].rooms.Count)), Random.Range((int)(amountOfSections.x),
                                (int)(amountOfSections.y)), RoomSize, data, ref templates);
        // Debug.Log("RoomGrid size " + roomGrid.Count);
        FinishRooms(ref templates, data); //Touch up, adding entrances and stuff
        GenerateTerrain(ref templates, data, out RoomTemplate bigTemplate);
        GenerateFullTemplate(ref templates, data, ref bigTemplate);
        data.bigTemplate = bigTemplate;
        GenerateMap(data.bigTemplate);
        data.templates = templates;
    }
    void GenerateFullTemplate(ref List<RoomTemplate> templates, LevelData data, ref RoomTemplate bigTemplate)
    {
        int count = 0;

        for (int i = 0; i < data.sectionData.Count; i++)
        {
            for (int j = 0; j < data.sectionData[i].rooms.Count; j++)
            {
                DebugLog.AddToMessage("Getting map image", data.sectionData[i].rooms[j].name);
                RoomTemplate template = templates[count];
                int kStart = (int)data.sectionData[i].rooms[j].position.x + Mathf.Abs(leftestPoint * 20);
                int lStart = (int)data.sectionData[i].rooms[j].position.y + Mathf.Abs((southestPoint - 1) * 20) - 1; //Why the fuck do i have to subtract 1, what (oh maybe cuz southestpoint is 19 off not 20)
                for (int l = 0; l < template.size.y; l++)
                {
                    for (int k = 0; k < template.size.x; k++)
                    {
                        //Debug.Log("x: " + (k + kStart) + " and y: " + (l + lStart) + " Width: " + bigTemplate.size.x + " Count is: " + bigTemplate.positions.Count());
                        //Debug.Log("x: " + k + " and y: " + l + "Width: " + template.size.x + " Count is: " + template.positions.Count());
                        bigTemplate.positions[k + kStart, lStart - l] = template.positions[k, l];
                    }
                }
                count++;
                DebugLog.PublishMessage();
            }
        }
    }
    void GenerateMap(RoomTemplate template)
    {
        sizeOfMap = new Vector2Int((rightestPoint+1) - leftestPoint, northestPoint - (southestPoint-1)) * new Vector2Int(20,20);
        //Plus twenty because otherwise its just the upper left corner of that square. I want the lowermost point of it
        //Debug.Log("Size of map:" + sizeOfMap);
        Color[] colors = new Color[sizeOfMap.x * sizeOfMap.y];
        map = new Texture2D(sizeOfMap.x, sizeOfMap.y, TextureFormat.ARGB32, false);
        Grid<TileTemplate> grid = template.positions.FlipVertically();
        for (int i = 0; i < sizeOfMap.x * sizeOfMap.y; i++)
        {
            float lumValue = (float)grid[i].elevation / 20f + 0.5f;
            colors[i] = Color.HSVToRGB(0.3f, 1, lumValue);
        }
        map.Finish(colors);
        map.Apply();
        map.filterMode = FilterMode.Point;
        if(UIManager.Instance != null)
        {
            UIManager.Instance.currentMap = map;
        }
    }
    void GenerateTerrain(ref List<RoomTemplate> templates, LevelData data, out RoomTemplate bigTemplate)
    {
        //Keep spawning rooms within the bounds :)
        sizeOfMap = new Vector2Int((rightestPoint + 1) - leftestPoint, northestPoint - (southestPoint - 1)) * new Vector2Int(20, 20);
        bigTemplate = new RoomTemplate(sizeOfMap, "B[4], N");
    }
   
    void SpawnRooms(int amountOfRooms, int amountOfSections, Vector2Int RoomSize, LevelData data, ref List<RoomTemplate> templates)
    {
        DateTime before = DateTime.Now;
        numberOfRooms = 1;
        //this spawns all rooms
        for(int i = 0; i < amountOfSections; i++)
        {
            if(i != 0)
            {
                data.sectionData.Add(new SectionData());
            }
            int k = 0;
            for (int j = data.sectionData[i].rooms.Count; k < amountOfRooms; j++)
            {
                DebugLog.AddToMessage("Room", j.ToString());
                DebugLog.AddToMessage("Section", i.ToString());
                
                Vector2Int currentRoomSize = new Vector2Int(Random.Range(1, 5), Random.Range(1, 5));
                
                Tuple<RoomData, List<Entrances.Entrance>> originRoom = new Tuple<RoomData, List<Entrances.Entrance>>(new RoomData(), new List<Entrances.Entrance>(){});

                if(j != 0) //If not the first room every section, find a room to originate from
                {
                    originRoom = GetRandomRoomOfSection(i, data, true, false); 
                    if(originRoom == null) //! If there are no open entrances in any room, break and start the next section
                    {
                        break;
                    }
                    DebugLog.AddToMessage("Spawning from", originRoom.Item1.position.ToString());
                }
                else if(i != 0) //And if not the first room of the first section, its the first room of the current section. Get a room to originate from
                {
                    //Gets any random room to spawn from. This is good for optional routes or for key item routes
                    List<Tuple<RoomData, List<Entrances.Entrance>>> roomsWithAvailableDoors = GetRooms(data, false, false);
                    originRoom = roomsWithAvailableDoors[Random.Range(0, roomsWithAvailableDoors.Count)];
                    //Gets the last room of last section. This is good for linear progression through dungeon
                    // originRoom = new Tuple<Room, List<Room.Entrances.Entrance>> (data.sections[i-1].rooms[data.sections[i-1].rooms.Count - 1], 
                    //data.sections[i-1].rooms[data.sections[i-1].rooms.Count - 1].directions.entrances);
                    OpenAvailableEntrances(originRoom.Item1, data);
                }

                data.sectionData[i].rooms.Add(new RoomData());
                //data.sections[i].rooms.Add(Instantiate(RoomPrefab, transform));
                data.sectionData[i].rooms[j].name = "Room #" + (numberOfRooms+1); numberOfRooms++;
                DebugLog.AddToMessage("Name", data.sectionData[i].rooms[j].name);

                Vector2Int gridPositionWhereOriginRoomConnects = Vector2Int.zero;
                Vector2Int gridPositionWhereNewRoomConnects = GetNewRoomCoordinates(data.sectionData[i].rooms[j], originRoom.Item1.position.ToV2Int(), originRoom.Item2, ref currentRoomSize, ref gridPositionWhereOriginRoomConnects, data);
                data.sectionData[i].rooms[j].Initialize(gridPositionWhereNewRoomConnects, currentRoomSize * 20, i, ref templates);
                data.sectionData[i].rooms[j].stepsAwayFromMainRoom = originRoom.Item1.stepsAwayFromMainRoom + 1;
                data.sectionData[i].rooms[j].originalPosition = gridPositionWhereNewRoomConnects;
                if(data.sectionData[i].rooms[j].stepsAwayFromMainRoom > furthestDistanceFromSpawn)
                {
                    furthestDistanceFromSpawn = data.sectionData[i].rooms[j].stepsAwayFromMainRoom;
                }
                bool locked = j == 0 && i > 0 ? true:false;
                ActivateEntrances(originRoom.Item1, data.sectionData[i].rooms[j], gridPositionWhereOriginRoomConnects, gridPositionWhereNewRoomConnects, locked);
                LinkRoom(data.sectionData[i].rooms[j], RoomSize, data);
                OpenRandomEntrances(data.sectionData[i].rooms[j], data.openDoorProbability);
                for(int x = 0; x < Mathf.Abs(currentRoomSize.x); x++)
                {
                    for(int y = 0; y < Mathf.Abs(currentRoomSize.y); y++)
                    {
                        data.roomGrid.Add(new LevelData.RoomGridEntry(gridPositionWhereNewRoomConnects/20 + 
                            new Vector2Int(x * (int)Mathf.Sign(currentRoomSize.x), y * (int)Mathf.Sign(currentRoomSize.y)), data.sectionData[i].rooms[j]));
                        k++;
                        if(data.roomGrid[data.roomGrid.Count -1].position.x <= leftestPoint)
                        {
                            leftestPoint = data.roomGrid[data.roomGrid.Count -1].position.x;
                        }
                        if(data.roomGrid[data.roomGrid.Count -1].position.y >= northestPoint)
                        {
                            northestPoint = data.roomGrid[data.roomGrid.Count -1].position.y;
                        }
                        if(data.roomGrid[data.roomGrid.Count -1].position.x >= rightestPoint)
                        {
                            rightestPoint = data.roomGrid[data.roomGrid.Count -1].position.x;
                        }
                        if(data.roomGrid[data.roomGrid.Count -1].position.y <= southestPoint)
                        {
                            southestPoint = data.roomGrid[data.roomGrid.Count -1].position.y;
                        }
                    }
                }
                DebugLog.PublishMessage();
            }
            CloseAllEntrances(data);
        }
        DateTime after = DateTime.Now; 
        TimeSpan duration = after.Subtract(before);
        Debug.Log("<color=blue>Time to spawn rooms: </color>" + duration.TotalMilliseconds + " milliseconds, which is: " + duration.TotalSeconds + " seconds");
    }
    void OpenAvailableEntrances(RoomData origin, LevelData data)
    {
        for(int i = 0; i < origin.GetDirections().entrances.Count; i++)
        {
            if(!CheckIfCoordinatesOccupied(origin.position.ToV2Int() / 20 + origin.GetDirections().entrances[i].dir, data))
            {
                origin.GetDirections().entrances[i].SetOpen(true);
            }
        }
    }
    void CloseAllEntrances(LevelData data)
    {
        for(int i = 0; i < data.sectionData.Count; i++)
        {
            for(int j = 0; j < data.sectionData[i].rooms.Count; j++)
            {
                Entrances entrances = data.sectionData[i].rooms[j].GetDirections();
                for (int k = 0; k < entrances.entrances.Count; k++)
                {
                    if(entrances.entrances[k].open && !entrances.entrances[k].spawned)
                    {
                        entrances.entrances[k].Deactivate();
                    }
                }
            }
        }
    }
    void FinishRooms(ref List<RoomTemplate> templates, LevelData data)
    {
        //Now that all entrances have been set, you can put the entrances down on each room template and adjust the templates to make sure there is always space to get to each door
        //Then build the rooms
        //This is where the templates list should end. It is not needed after this
        int count = 0;
        for(int i = 0; i < data.sectionData.Count; i++)
        {
            for(int j = 0; j < data.sectionData[i].rooms.Count; j++)
            {
                templates[count].AddEntrancesToRoom(data.sectionData[i].rooms[j].GetDirections());
                count++;
            }
        }
    }
    
    

    void ActivateEntrances(RoomData origin, RoomData newRoom, Vector2Int gridPositionWhereOriginConnects, Vector2Int gridPositionWhereNewRoomConnects, bool locked)
    {
        //Get entrance in adjacent room that points to this room
        //Get entrance in this room that points to adjacent room
        //!same here as linkrooms, you need to get the origin gridposition that is closest to newroom
        //!the newroom position is already by nature closest, since its spawned from the direction of a door

        //!then you have to make sure that these two rooms connect anywhere else that wasnt intended
        DebugLog.AddToMessage("Step", "Activating Entrances");
        Vector2 direction = (gridPositionWhereOriginConnects - (Vector2)gridPositionWhereNewRoomConnects/20).normalized;
        DebugLog.AddToMessage("Origin pos", origin.position.ToString());
        DebugLog.AddToMessage("Newroom pos", gridPositionWhereNewRoomConnects.ToString());
        //DebugLog.AddToMessage("Grid position where origin connects", gridPositionWhereOriginConnects.ToString());
        DebugLog.AddToMessage("Direction to attach", direction.ToString());
        //Debug.Log(direction);

        Tuple<bool, Entrances.Entrance> adjEntrance = newRoom.GetDirections().GetEntrance(gridPositionWhereNewRoomConnects/20, direction.ToV2Int());

        if(adjEntrance.Item1) //If i found the correct entrance
        {
            Tuple<bool, Entrances.Entrance> myEntrance = origin.GetDirections().GetEntrance(gridPositionWhereNewRoomConnects/20 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);
            if(myEntrance.Item1)
            {
               // Debug.Log("<color=magenta>Linking " + myEntrance.Item2.dir + "from " + (origin.transform.position / 20) + " to " + adjEntrance.Item2.dir + " from " + (newRoom.transform.position/20) + "</color>");
                myEntrance.Item2.Activate();
                adjEntrance.Item2.Activate();

                if(locked)
                {
                    /*GameObject myDoor = new GameObject("Locked Door");
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
                    */
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
    Vector2Int GetNewRoomCoordinates(RoomData room, Vector2Int originCoordinates, List<Entrances.Entrance> openEntrances, ref Vector2Int roomSize, ref Vector2Int gridPositonWhereNewRoomConnects, LevelData data) //Roomsize is in grid size
    {
        DebugLog.AddToMessage("Step", "Getting new room coordinates");
        List<Tuple<Vector2Int, Vector2Int>> possibleCoordinates = new List<Tuple<Vector2Int, Vector2Int>> { };
        foreach(Entrances.Entrance entrance in openEntrances)
        {
            if(!CheckIfCoordinatesOccupied(new Vector2Int(entrance.gridPos.x + entrance.dir.x, entrance.gridPos.y + entrance.dir.y), data))
            {
                possibleCoordinates.Add(new Tuple<Vector2Int, Vector2Int>(new Vector2Int(entrance.gridPos.x * 20 + entrance.dir.x * 20, entrance.gridPos.y * 20 + entrance.dir.y * 20), entrance.gridPos));
            }
        }
        int index = possibleCoordinates.GetRandomIndex();
        gridPositonWhereNewRoomConnects = possibleCoordinates[index].Item2;
        //!When its a big room, you decide the position first, then you see if you can grow out like intended. If you cant, then change the roomSize. Thats why its passed as a referenc
        AttemptExpansion(room, possibleCoordinates[index].Item1 / 20, ref roomSize, data);
        return possibleCoordinates[index].Item1;
    }

    void AttemptExpansion(RoomData room, Vector2Int origin, ref Vector2Int roomSize, LevelData data)
    {
        DebugLog.AddToMessage("Step", "Attempting expansion");

        List<List<LevelData.RoomGridEntry>> potentialExpansions = new List<List<LevelData.RoomGridEntry>>();
        List<Vector2Int> potentialSizes = new List<Vector2Int>();

        for(int i = 0; i < 4; i++) //Go through every direction of expansion
        {
            potentialExpansions.Add(new List<LevelData.RoomGridEntry>());
            potentialSizes.Add(roomSize * Math.diagonals[i]);
            int x = 1, y = 1;
            while (x < Mathf.Abs(potentialSizes[i].x) || y < Mathf.Abs(potentialSizes[i].y))
            { //Run as long as either x or y haven't reach the ends of the room
                OnAttemptExpansion(room, ref x, ref y, i, origin, new Vector2Int(1,0), ref potentialSizes, ref potentialExpansions, data);
                OnAttemptExpansion(room, ref y, ref x, i, origin, new Vector2Int(0,1), ref potentialSizes, ref potentialExpansions, data);
            }
        }
        List<LevelData.RoomGridEntry> choice = new List<LevelData.RoomGridEntry>();
        roomSize = new Vector2Int(1,1);
        for (int i = 0; i < 4; i++) //Check which room is the biggest
        {
            if(potentialExpansions[i].Count > choice.Count)
            {
                choice = potentialExpansions[i];
                roomSize = potentialSizes[i];
            }
        }
        DebugLog.AddToMessage("New Size", roomSize.ToString());
    }
    void OnAttemptExpansion(RoomData room, ref int currentSide, ref int otherSide, int direction, Vector2Int origin, Vector2Int coordinate, ref List<Vector2Int> potentialSizes, ref List<List<LevelData.RoomGridEntry>> potentialExpansions, LevelData data)
    {
        //"currentSide" is either x or y and "otherSide" is the other
        //"coordinate" determines if it's going in the x axis or the y axis

        if (currentSide < (coordinate.y == 0 ? Mathf.Abs(potentialSizes[direction].x) : Mathf.Abs(potentialSizes[direction].y)) && 
        !CheckIfCoordinatesOccupied(origin + new Vector2Int(currentSide * Math.diagonals[direction].x * coordinate.x, currentSide * Math.diagonals[direction].y * coordinate.y), data)) 
        //If there is a free spot to the side, then try to expand to the side
        {
            List<LevelData.RoomGridEntry> temp = new List<LevelData.RoomGridEntry>();
            for (int j = 0; j < otherSide; j++) //Go through all to the side. All of them have to be free
            {
                Vector2Int position = origin + new Vector2Int(currentSide * Math.diagonals[direction].x * coordinate.x + j * Math.diagonals[direction].x * coordinate.y, 
                                                              currentSide * Math.diagonals[direction].y * coordinate.y + j * Math.diagonals[direction].y * coordinate.x);
                if (!CheckIfCoordinatesOccupied(position, data))
                {
                    temp.Add(new LevelData.RoomGridEntry(position, room));
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

    bool CheckIfCoordinatesOccupied(Vector2Int roomPosition, LevelData data)
    {
        foreach(LevelData.RoomGridEntry room in data.roomGrid)
        {
            if(room.position == roomPosition)
            {
                return true;
            } 
        }
        //Debug.Log(roomPosition + " is not occupied");
        return false;
    }

    void LinkRoom(RoomData room, Vector2 RoomSize, LevelData data)
    {
        //This function checks if this given room has another spawned room in any direction that it must link to, before it decides if it should link anywhere else
        //It does this by checking if a room in any direction has an open but not spawned gate in its own direction, in which case it opens its own gate in that direction
        DebugLog.AddToMessage("Step", "Linking Rooms");
        List<Tuple<Vector2Int, Vector2Int, RoomData>> roomList = GetAllAdjacentRooms(room, data);
        //Item1 is the adjacent gridposition, Item2 is the gridposition it connects to in the origin room
        for(int i = 0; i < roomList.Count; i++)
        {
            //Get entrance in adjacent room that points to this room
            //Get entrance in this room that points to adjacent room

            //It cant just be any grid position of the room, it has to be the grid position where the entrances are. Roomlist returns the correct position, but the correct position in the origin room has to be used
            
            Vector2 direction = (new Vector2(roomList[i].Item1.x, roomList[i].Item1.y) - roomList[i].Item2).normalized;

            Tuple<bool, Entrances.Entrance> adjEntrance = roomList[i].Item3.GetDirections().GetEntrance(roomList[i].Item1, -direction.ToV2Int());

            if(adjEntrance.Item1) //If i found the correct entrance
            {
                Tuple<bool, Entrances.Entrance> myEntrance = room.GetDirections().GetEntrance(roomList[i].Item1 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);
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

    void OpenRandomEntrances(RoomData room, int openDoorProbability)
    {
        //This will open a random amount of doors in the newly spawned room
        List<Entrances.Entrance> possibleEntrancesToOpen = new List<Entrances.Entrance> { };

        foreach(Entrances.Entrance entrance in room.GetDirections().entrances)
        {
            if(entrance.open == false && entrance.spawned == false)
            {
                possibleEntrancesToOpen.Add(entrance);
            }
        }
        if (possibleEntrancesToOpen.Count > 0)
        {
            int i = Random.Range(0, possibleEntrancesToOpen.Count - 1);
            int limit = Random.Range(i+1, openDoorProbability) + openDoorProbability;
            for (; i < limit; i++)
            {
                amountOfRandomOpenEntrances++;
                possibleEntrancesToOpen[Random.Range(0, possibleEntrancesToOpen.Count)].SetOpen(true);
            }
        }
    }
}
//The following part of the class finds and returns rooms
public partial class LevelGenerator : MonoBehaviour
{
    List<Tuple<RoomData, List<Entrances.Entrance>>> GetRooms(LevelData data, bool openDoors, bool spawnedDoors)
    {
        //Gets and returns all rooms with the bool conditions
        List<Tuple<RoomData, List<Entrances.Entrance>>> roomsWithOpenDoors = new List<Tuple<RoomData, List<Entrances.Entrance>>> { };

        foreach (LevelData.SectionData section in data.sectionData)
        {
            foreach (RoomData room in section.rooms)
            {
                List<Entrances.Entrance> openEntrances = room.GetEntrances(openDoors, spawnedDoors);
                if (openEntrances.Count > 0)
                {
                    roomsWithOpenDoors.Add(new Tuple<RoomData, List<Entrances.Entrance>>(room, openEntrances));
                }
            }
        }
        return roomsWithOpenDoors;
    }
    Tuple<RoomData, List<Entrances.Entrance>> GetRandomRoom(LevelData data, bool openDoors, bool spawnedDoors)
    {
        //This functions gets any of the rooms that are already spawned
        //It should make sure that it doesnt have something spawned in each direction

        List<Tuple<RoomData, List<Entrances.Entrance>>> roomsWithOpenDoors = GetRooms(data, openDoors, spawnedDoors);
        return roomsWithOpenDoors[Random.Range(0, roomsWithOpenDoors.Count - 1)];
    }
    Tuple<RoomData, List<Entrances.Entrance>> GetRandomRoomOfSection(int section, LevelData data, bool openDoors, bool spawnedDoors)
    {
        //This functions gets any of the rooms that are already spawned
        //It should make sure that it doesnt have something spawned in each direction
        DebugLog.AddToMessage("Step", "Getting a random origin room of section: " + section);
        List<Tuple<RoomData, List<Entrances.Entrance>>> roomsWithOpenDoors = new List<Tuple<RoomData, List<Entrances.Entrance>>>{};
        foreach (RoomData room in data.sectionData[section].rooms)
        {
            List<Entrances.Entrance> openEntrances = room.GetEntrances(openDoors, spawnedDoors);
            if (openEntrances.Count > 0)
            {
                roomsWithOpenDoors.Add(new Tuple<RoomData, List<Entrances.Entrance>>(room, openEntrances));
            }
        }
        DebugLog.AddToMessage("Rooms with open doors", roomsWithOpenDoors.Count.ToString());
        //! if rooms with open doors is empty, this will cause an error
        //! this will only happen if no rooms have open doors
        if(roomsWithOpenDoors.Count > 0)
        {
            return roomsWithOpenDoors[Random.Range(0, roomsWithOpenDoors.Count - 1)];
        }
        return null;
    }

    public List<Tuple<Vector2Int, RoomData>> GetAllConnectingRooms(RoomData origin, LevelData data)
    {
        List<Tuple<Vector2Int, RoomData>> temp = new List<Tuple<Vector2Int, RoomData>>();
        foreach (Entrances.Entrance entrance in origin.GetDirections().entrances)
        {
            if (entrance.open && entrance.spawned)
            {
                temp.Add(FindAdjacentRoom(entrance.gridPos, entrance.dir, data));
            }
        }
        return temp;
    }
    public List<Tuple<Vector2Int, Vector2Int, RoomData>> GetAllAdjacentRooms(RoomData room, LevelData data) 
        //Item1 is the adjacent gridposition, Item2 is the gridposition it connects to in the origin room
    {
        DebugLog.AddToMessage("Substep", "Getting all adjacent rooms");
        List<Tuple<Vector2Int, Vector2Int, RoomData>> temp = new List<Tuple<Vector2Int,Vector2Int, RoomData>>();
        
        for(int x = 0; x < Mathf.Abs(room.size.x / 20); x++)
        { //!Looking for rooms vertically
            int _x = x * (int)Mathf.Sign(room.size.x);
            if(Mathf.Sign(room.size.x) == -1)
            {
                _x -= (room.size.x/20) + 1;
            }
            Vector2Int direction = new Vector2Int(0, 1);

            Vector2Int checkPosition = new Vector2Int((int)room.position.x /20 + _x, (int)room.position.y/20 );
            Tuple<Vector2Int,RoomData> temp2 = FindAdjacentRoom( checkPosition , direction, data);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, checkPosition,temp2.Item2));DebugLog.AddToMessage("Just added on first X", checkPosition.ToString());}

            checkPosition = new Vector2Int((int)room.position.x/20 + _x, (int)room.position.y/20 - (room.size.y / 20 - 1 * (int)Mathf.Sign(room.size.y))*(int)Mathf.Sign(room.size.y));
            temp2 = FindAdjacentRoom(checkPosition, -direction, data);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, checkPosition ,temp2.Item2));DebugLog.AddToMessage("Just added on second X", checkPosition.ToString());} 
        }
        for(int y = 0; y < Mathf.Abs(room.size.y / 20); y++)
        { //!Looking for rooms horizontally
            int _y = y * (int)Mathf.Sign(room.size.y);
            if(Mathf.Sign(room.size.y) == 1)
            {
                _y -= (room.size.y/20) - 1;
            }
            Vector2Int direction = new Vector2Int(-1, 0);

            Vector2Int checkPosition = new Vector2Int((int)room.position.x/20, (int)room.position.y/20 + _y);
            Tuple<Vector2Int,RoomData> temp2 = FindAdjacentRoom(checkPosition, direction, data);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, checkPosition ,temp2.Item2));DebugLog.AddToMessage("Just added on first Y", checkPosition.ToString());}

            checkPosition = (new Vector2(room.position.x/20 + (room.size.x/20 - 1 * (int)Mathf.Sign(room.size.x))* (int)Mathf.Sign(room.size.x), room.position.y/20 + _y)).ToV2Int();
            temp2 = FindAdjacentRoom(checkPosition, -direction, data);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, checkPosition ,temp2.Item2));DebugLog.AddToMessage("Just added on second Y", checkPosition.ToString());}
        }
        DebugLog.AddToMessage("Adjacent rooms found", temp.Count.ToString());
        for(int i = 0; i < temp.Count; i++)
        {
            DebugLog.AddToMessage("Adjacent Room " + i, temp[i].Item1.ToString() + " belonging to: " + temp[i].Item3.name);
        }
        return temp;
    }
    public List<Tuple<Vector2Int, Vector2Int, RoomData>> GetAllAdjacentRooms(RoomData room, LevelData.SectionData section)
    {
        List<Tuple<Vector2Int, Vector2Int, RoomData>> temp = new List<Tuple<Vector2Int,Vector2Int, RoomData>>();
        for(int x = 0; x < room.size.x / 20; x++)
        {
            Tuple<Vector2Int,RoomData> temp2 = FindAdjacentRoom(new Vector2Int((int)room.position.x /20 + x, (int)room.position.y/20), Vector2Int.up, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, new Vector2Int((int)room.position.x /20 + x, (int)room.position.y/20) ,temp2.Item2));}
            temp2 = FindAdjacentRoom(new Vector2Int((int)room.position.x/20 + x, (int)room.position.y/20 + (room.size.y / 20 - 1)), Vector2Int.down, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, new Vector2Int((int)room.position.x/20 + x, (int)room.position.y/20 + (room.size.y / 20 - 1)) ,temp2.Item2));}
        }
        for(int y = 0; y < room.size.y / 20; y++)
        {
            Tuple<Vector2Int,RoomData> temp2 = FindAdjacentRoom(new Vector2Int((int)room.position.x/20, (int)room.position.y/20 + y), Vector2Int.left, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, new Vector2Int((int)room.position.x/20, (int)room.position.y/20 + y) ,temp2.Item2));}
            temp2 = FindAdjacentRoom((new Vector2(room.position.x/20 + (room.size.x/20 - 1), room.position.y/20 + y)).ToV2Int(), Vector2Int.right, section);
            if(temp2.Item2 != null){temp.Add(new Tuple<Vector2Int, Vector2Int, RoomData>(temp2.Item1, (new Vector2(room.position.x/20 + (room.size.x/20 - 1), room.position.y/20 + y)).ToV2Int() ,temp2.Item2));}

        }
        return temp;
    }
    public Tuple<Vector2Int, RoomData> FindAdjacentRoom(Vector2Int origin, Vector2Int direction, LevelData data)
    {
        return new Tuple<Vector2Int, RoomData> (origin + direction, FindRoomDataOfPosition(origin + direction, data));
    }

    public Tuple<Vector2Int, RoomData> FindAdjacentRoom(Vector2Int origin, Vector2Int direction, LevelData.SectionData section)
    {
        return new Tuple<Vector2Int, RoomData> (origin + direction, FindRoomOfPosition(origin + direction, section));
    }
    RoomData FindRoomOfPosition(Vector2Int position, LevelData.SectionData section)
    {
        for(int i = 0; i < section.rooms.Count; i++)
        {
            if(section.rooms[i].position.ToV2Int()/20 == position)
            {
                return section.rooms[i];
            }
        }
        return null;
    }
    public RoomData FindRoomDataOfPosition(Vector2Int position, LevelData data)
    {
        for(int i = 0; i < data.roomGrid.Count; i++)
        {
            if(data.roomGrid[i].position == position)
            {
                return data.roomGrid[i].roomData;
            }
        }
        return null;
    }
    public Room FindRoomOfPosition(Vector2Int position, LevelData data)
    {
        for (int i = 0; i < data.roomGrid.Count; i++)
        {
            if (data.roomGrid[i].position == position)
            {
                return data.roomGrid[i].GetRoom();
            }
        }
        return null;
    }

    public void ToggleRenderSections()
    {
        renderSections = true;
    }
    private void OnRenderObject() 
    {
       /* if(!renderSections && DunGenes.Instance != null)
        {
            for(int i = 0; i < DunGenes.Instance.gameData.CurrentLevel.sectionData.Count; i++)
            {
                float percentageThroughSections = (float)i / DunGenes.Instance.gameData.CurrentLevel.sectionData.Count;
                Color sectionColor = Color.HSVToRGB(percentageThroughSections, 1, 1);

                for(int j = 0; j < DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms.Count; j++)
                {
                    GLFunctions.DrawSquareFromCorner(DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms[j].originalPosition.ToV3() - new Vector3(10, 10, 0), DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms[j].size, transform, sectionColor);
                }
            }
        }*/
    }
    private void OnDrawGizmos() 
    {
        if (DunGenes.Instance == null) { return; }
        foreach(LevelData.SectionData section in DunGenes.Instance.gameData.CurrentLevel.sectionData)
        {
            foreach(var room in section.rooms)
            {
                Gizmos.color = Color.red;
                Vector3 centeredPosition = room.position - new Vector3(10,10,0);
                Gizmos.DrawLine(centeredPosition, centeredPosition + new Vector3(room.size.x, 0, 0));
                Gizmos.DrawLine(centeredPosition + new Vector3(room.size.x, 0, 0), centeredPosition + new Vector3(room.size.x, -room.size.y + 20, 0));
                Gizmos.DrawLine(centeredPosition + new Vector3(room.size.x, -room.size.y +20, 0), centeredPosition + new Vector3(0, -room.size.y + 20 , 0));
                Gizmos.DrawLine(centeredPosition + new Vector3(0, -room.size.y +20, 0), centeredPosition);
                foreach(Entrances.Entrance entrance in room.GetDirections().entrances)
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