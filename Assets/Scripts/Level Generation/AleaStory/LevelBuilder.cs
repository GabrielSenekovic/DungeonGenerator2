using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Section = LevelData.Section;

public class LevelBuilder : MonoBehaviour, ILevelBuilder
{
    [SerializeField] MaterialDatabase materialDatabase;
    [SerializeField] FurnitureDatabase furnitureDatabase;
    [SerializeField] protected Room RoomPrefab;

    public int leftestPoint = 0;
    public int northestPoint = 0;
    public int rightestPoint = 0;
    public int southestPoint = 0;

    List<Tuple<Vector2Int, Room>> surroundingPositions = new List<Tuple<Vector2Int, Room>>();

    bool levelGenerated = false;
    public void BuildLevel(LevelManager level, ref List<RoomTemplate> templates, ref RoomTemplate bigTemplate, SettlementData settlementData)
    {
        DateTime before = DateTime.Now;
        //GenerateSurroundings(ref templates, DunGenes.Instance.gameData.CurrentLevel);
        BuildRooms(ref templates, ref bigTemplate, settlementData);

        LevelData currentLevel = DunGenes.Instance.gameData.CurrentLevel;
        level.firstRoom = currentLevel.sections[0].rooms[0];
        level.lastRoom = currentLevel.sections[currentLevel.sections.Count - 1].rooms[currentLevel.sections[currentLevel.sections.Count - 1].rooms.Count - 1];

        //AdjustRoomTypes(level.l_data);
        //AdjustEntrances(RoomSize);

        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("<color=blue>Time to generate: </color>" + duration.TotalMilliseconds + " milliseconds, which is: " + duration.TotalSeconds + " seconds");
    }
    public void BuildRooms(ref List<RoomTemplate> templates, ref RoomTemplate bigTemplate, SettlementData settlementData)
    {
        //Now that all entrances have been set, you can put the entrances down on each room template and adjust the templates to make sure there is always space to get to each door
        //Then build the rooms
        //This is where the templates list should end. It is not needed after this
        int count = 0;
        LevelData currentLevel = DunGenes.Instance.gameData.CurrentLevel;
        GameObject levelMesh = new GameObject("Level Mesh");
        RoomTemplateReader reader = new RoomTemplateReader(bigTemplate, levelMesh.transform);
        levelMesh.transform.position = new Vector2(leftestPoint * 20, -southestPoint * 20);
        reader.CreateLevel(ref bigTemplate, Resources.Load<Material>("Materials/Ground"), materialDatabase, settlementData);
        for (int i = 0; i < currentLevel.sectionData.Count; i++)
        {
            currentLevel.sections.Add(new Section());
            for (int j = 0; j < currentLevel.sectionData[i].rooms.Count; j++)
            {
                Room newRoom = Instantiate(RoomPrefab, transform);
                RoomData roomData = currentLevel.sectionData[i].rooms[j];

                newRoom.name = roomData.name;
                newRoom.roomData = roomData;
                newRoom.transform.position = roomData.position;
                currentLevel.sections[i].rooms.Add(newRoom);
                DebugLog.AddToMessage("Generating", roomData.name);

                List<LevelData.RoomGridEntry> entries = currentLevel.roomGrid.Where(e => e.roomData.originalPosition == roomData.originalPosition).ToList();
                entries.ForEach(e => e.SetRoom(newRoom));
                RoomTemplate template = templates[count];
                newRoom.CreateRoom(ref template, Resources.Load<Material>("Materials/Ground"), furnitureDatabase);
                count++;
                DebugLog.PublishMessage();
            }
        }
        PlantFlora(ref templates);
    }
    void PlantFlora(ref List<RoomTemplate> templates)
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
        //numberOfRooms = 1;
    }

    public bool HasGenerated() => levelGenerated;
}
