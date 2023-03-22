using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class LevelGenerator : MonoBehaviour
{
    public void GenerateLevel(LevelManager level, ref List<Room.RoomTemplate> templates)
    {
        DateTime before = DateTime.Now;
        //GenerateSurroundings(ref templates, DunGenes.Instance.gameData.CurrentLevel);
        BuildRooms(ref templates);

        LevelData currentLevel = DunGenes.Instance.gameData.CurrentLevel;
        level.firstRoom = currentLevel.sections[0].rooms[0];
        level.lastRoom = currentLevel.sections[currentLevel.sections.Count - 1].rooms[currentLevel.sections[currentLevel.sections.Count - 1].rooms.Count - 1];

        //AdjustRoomTypes(level.l_data);
        //AdjustEntrances(RoomSize);

        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("<color=blue>Time to generate: </color>" + duration.TotalMilliseconds + " milliseconds, which is: " + duration.TotalSeconds + " seconds");
    }
    public void PutDownQuestObjects(LevelManager level, QuestData data)
    {
        switch (data.missionType)
        {
            case QuestData.MissionType.Recovery:
                /*spawnedEndOfLevel = Instantiate(endOfLevel, 
                    new Vector2(level.lastRoom.transform.position.x + 10, level.lastRoom.transform.position.y + 10), 
                    Quaternion.identity, level.lastRoom.transform);*/
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
                for (int i = 0; i < temp2.NPCsToBackup.Count; i++)
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
    void BuildRooms(ref List<Room.RoomTemplate> templates)
    {
        //Now that all entrances have been set, you can put the entrances down on each room template and adjust the templates to make sure there is always space to get to each door
        //Then build the rooms
        //This is where the templates list should end. It is not needed after this
        int count = 0;
        for (int i = 0; i < DunGenes.Instance.gameData.CurrentLevel.sectionData.Count; i++)
        {
            for (int j = 0; j < DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms.Count; j++)
            {
                DebugLog.AddToMessage("Generating", DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms[j].name);
                Room.RoomTemplate template = templates[count];
                RoomTemplateReader reader = new RoomTemplateReader(template, DunGenes.Instance.gameData.CurrentLevel.sections[i].rooms[j].transform);
                reader.CreateRoom(ref template, Resources.Load<Material>("Materials/Wall"), Resources.Load<Material>("Materials/Ground"), DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms[j].GetDirections());
                DunGenes.Instance.gameData.CurrentLevel.sections[i].rooms[j].CreateRoom(ref template, Resources.Load<Material>("Materials/Ground"));
                //SaveWallVertices(ref templates, template, DunGenes.Instance.gameData.CurrentLevel.sectionData[i].rooms[j], DunGenes.Instance.gameData.CurrentLevel);
                count++;
                DebugLog.PublishMessage();
            }
        }
        for (int i = 0; i < surroundingPositions.Count; i++)
        {
            Room.RoomTemplate template = templates[count];
            surroundingPositions[i].Item2.CreateRoom(ref template, Resources.Load<Material>("Materials/Ground"));
            count++;
        }
        //PlantFlora(ref templates);
    }
   /* void SaveWallVertices(ref List<Room.RoomTemplate> templates, Room.RoomTemplate originTemplate, Room origin, LevelData data)
    {
        //Set the entrance vertices of all adjacent rooms
        List<Tuple<Vector2Int, Vector2Int, Room>> roomList = GetAllAdjacentRooms(origin, data);
        Debug.Log("The room: " + origin.gameObject.name + " found this many neighbors: " + roomList.Count);
        //Item1 is the adjacent gridposition, Item2 is the gridposition it connects to in the origin room
        for (int k = 0; k < roomList.Count; k++)
        {
            int n = 0;
            for (int l = 0; l < DunGenes.Instance.gameData.CurrentLevel.sections.Count; l++)
            {
                for (int m = 0; m < DunGenes.Instance.gameData.CurrentLevel.sections[l].rooms.Count; m++)
                {
                    n++;
                    if (roomList[k].Item1 == DunGenes.Instance.gameData.CurrentLevel.sections[l].rooms[m].transform.position.ToV2Int() / 20) //found the index for templates
                    {
                        Debug.Log("Saving wall vertices from: " + roomList[k].Item2 + " to: " + roomList[k].Item1);
                        Room.RoomTemplate adjTemplate = templates[n - 1];

                        Vector2 direction = (new Vector2(roomList[k].Item1.x, roomList[k].Item1.y) - roomList[k].Item2).normalized;
                        Tuple<bool, Room.Entrances.Entrance> adjEntrance = roomList[k].Item3.roomData.GetDirections().GetEntrance(roomList[k].Item1, -direction.ToV2Int());
                        Tuple<bool, Room.Entrances.Entrance> myEntrance = origin.roomData.GetDirections().GetEntrance(roomList[k].Item1 + adjEntrance.Item2.dir, -adjEntrance.Item2.dir);

                        roomList[k].Item3.roomData.GetDirections().SetEntranceVertices(ref adjTemplate, originTemplate, adjEntrance.Item2, myEntrance.Item2, origin.transform.position, roomList[k].Item3.transform.position);
                    }
                }
            }
        }
    }*/
    public void PlantFlora(ref List<Room.RoomTemplate> templates)
    {
        for (int i = 0; i < DunGenes.Instance.gameData.CurrentLevel.sections.Count; i++)
        {
            for (int j = 0; j < DunGenes.Instance.gameData.CurrentLevel.sections[i].rooms.Count; j++)
            {
                if (!templates[j].indoors)
                {
                    GameObject lawn = new GameObject("Lawn");
                    lawn.transform.parent = DunGenes.Instance.gameData.CurrentLevel.sections[i].rooms[j].transform;

                    Vegetation grass = lawn.AddComponent<Vegetation>();
                    grass.area = templates[j].size;
                    grass.grassPerTile = 3;
                    grass.burningSpeed = 0.001f;
                    grass.fireColor = Color.red;
                    grass.grassRotation = new Vector3(-90, 90, -90);
                    grass.layerMask = ~0;
                    grass.VFX_Burning = Resources.Load<UnityEngine.VFX.VisualEffectAsset>("VFX/Burning");

                    lawn.transform.localPosition = new Vector3(-10, -10, -0.5f);

                    grass.PlantFlora(DunGenes.Instance.gameData.CurrentLevel.sections[i].rooms[j]);
                    DunGenes.Instance.gameData.CurrentLevel.sections[i].rooms[j].grass = grass;
                }
            }
        }
        for (int i = 0; i < surroundingPositions.Count; i++)
        {
            GameObject lawn = new GameObject("Lawn");
            lawn.transform.parent = surroundingPositions[i].Item2.transform;

            Vegetation grass = lawn.AddComponent<Vegetation>();
            grass.area = surroundingPositions[i].Item2.roomData.size;
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
    public void DestroyLevel(LevelData data)
    {
        for (int i = 0; i < data.sections.Count; i++)
        {
            for (int j = data.sections[i].rooms.Count - 1; j >= 0; j--)
            {
                Destroy(data.sections[i].rooms[j].gameObject);
            }
            data.sections[i].rooms.Clear();
        }
        data.sections.Clear();
        numberOfRooms = 1;
    }
}
