﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuestSelect : MonoBehaviour
{
    public struct SeedBox
    {
        public SeedBox(int constSeed_in, int dataSeed_in, int questSeed_in)
        {
            constructionSeed = constSeed_in;
            dataSeed = dataSeed_in;
            questSeed = questSeed_in;
        }
        public int constructionSeed;
        public int dataSeed;
        public int questSeed;
    }
    List<SeedBox> seeds = new List<SeedBox>(){};

    public List<LevelData> levels = new List<LevelData>(){};

    List<QuestData> quests = new List<QuestData>(){};

    bool selectedByButton = false;

    public List<Button> buttons = new List<Button>(){};
    [System.NonSerialized] public int index = 0;

    [SerializeField]Button buttonPrefab;
    [SerializeField]Transform buttonParent;
    [SerializeField]SpriteText detailText;
    [SerializeField] MapMenu mapMenu;

    [SerializeField]GraphemeDatabase graphemeDatabase; //Remove later

    BulletinBoard board = null;

    public LevelGenerator generator;
    public LevelBuilder builder;

    [SerializeField] LevelManager levelManager;

    public void Initialize(Tuple<int[], int[], int[]> seeds_in, BulletinBoard board_in)
    {
        board = board_in;
        for(int i = 0; i < seeds_in.Item1.Length; i++)
        {
            seeds.Add(new SeedBox(seeds_in.Item1[i], seeds_in.Item2[i], seeds_in.Item3[i]));
            levels.Add(LevelDataGenerator.Initialize(seeds[i].dataSeed));
            buttons.Add(Instantiate(buttonPrefab, buttonPrefab.transform.position, Quaternion.identity, buttonParent));
            buttons[i].GetComponent<QuestButton>().select = this;
            buttons[i].GetComponent<QuestButton>().index = i;
            generator.GenerateTemplates(levels[levels.Count - 1], new Vector2Int(20,20), levels[levels.Count - 1].amountOfRoomsCap, levels[levels.Count - 1].amountOfSections);
            levels[levels.Count - 1].templates = levels[levels.Count - 1].templates;
            levels[levels.Count - 1].roomGrid = new List<LevelData.RoomGridEntry>(levels[levels.Count - 1].roomGrid);
            levels[levels.Count - 1].sectionData = new List<LevelData.SectionData>(levels[levels.Count - 1].sectionData);
            levels[levels.Count - 1].map = generator.map;
            //quests.Add(QuestDataGenerator.Initialize(seeds[i].questSeed));
        }
        detailText.Initialize(graphemeDatabase.fonts[0], true);
    }

    public void OnClose()
    {
        for(int i = buttons.Count-1; i >= 0; i--)
        {
            Button temp = buttons[i];
            buttons.RemoveAt(i);
            Destroy(temp.gameObject);
        }
        board.OnClose();
        board = null;
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void OnLoadLevel() //This is called from the start button of the Quest Select
    {
        Time.timeScale = 1;
        DunGenes.Instance.gameData.CurrentLevel = levels[index];
        //GameData.currentQuest = QuestDataGenerator.Initialize(seeds[index].questSeed);
        DontDestroyOnLoad(DunGenes.Instance);
        DunGenes.Instance.isStartArea = false;
        SceneManager.LoadSceneAsync("Level");
        OnClose();
        Time.timeScale = 1;
        mapMenu.AddMap(new MapContainer(Sprite.Create(levels[index].map, new Rect(0, 0, levels[index].map.width, levels[index].map.height), new Vector2(0.5f, 0.5f), 16)));
        builder.BuildLevel(levelManager, ref levels[index].templates, ref levels[index].bigTemplate, null);
    }

    private void Update() 
    {
        if(GetComponent<CanvasGroup>().alpha == 0)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(index ==0)
            {
                index = buttons.Count-1;
            }
            else
            {
                index--;
            }
            buttons[index].Select();
            RevealDetails(index);
            selectedByButton = true;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(index == buttons.Count-1)
            {
                index = 0;
            }
            else
            {
                index++;
            }
            buttons[index].Select();
            RevealDetails(index);
            selectedByButton = true;
        }
        if(HasMouseMoved())
        {
            if(EventSystem.current.currentSelectedGameObject == buttons[index].gameObject && selectedByButton)
            {
                EventSystem.current.SetSelectedGameObject(null);
                selectedByButton = false;
                HideDetails();
            }
        }
    }
    bool HasMouseMoved()
    {
        return (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0);
    }
    public void RevealDetails(int index_in)
    {
        detailText.text = "";
        detailText.Write();
        detailText.PlaceSprite(Sprite.Create(levels[index_in].map, new Rect(0, 0, levels[index_in].map.width, levels[index_in].map.height), new Vector2(0.5f, 0.5f), 16));
        /*detailText.text = "Information about the quest: \n";
        detailText.text += quests[index_in].GetQuestDescription();
        detailText.Write();
        detailText.text = "\nQuestgiver: " + NameDatabase.GetRandomName();
        detailText.text += "\nObjective: " + "\nDifficulty level: \nReward: \n";
        detailText.text += "\nInformation about the destination: \n";
        detailText.PlaceSprite(Sprite.Create(levels[index_in].map, new Rect(0, 0, levels[index_in].map.width, levels[index_in].map.height), new Vector2(0.5f, 0.5f), 16));
        detailText.text += "\nSeeds: \nData seed: " + seeds[index_in].dataSeed + "\n";
        detailText.text += "Construction seed: " + seeds[index_in].constructionSeed + "\n";
        detailText.WriteAppend();*/
    }
    public void HideDetails()
    {
        detailText.text = "";
        detailText.Write();
    }
    public string GetBiomeDescription(bool same)
    {
        return "description";
    }
}
