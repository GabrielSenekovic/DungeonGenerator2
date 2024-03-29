﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [System.Serializable]struct ButtonData
    {
        public string name;
        public int destination;

        public string tooltip;

        public GameObject menu;
    }
    [System.Serializable]struct ButtonLayout
    {
        public string name;
        public List<ButtonData> buttons;
        public int parentMenu;
    }
    [SerializeField] List<ButtonLayout> buttonLayouts;
    //0 == Main Menu
    //1 == Party
    //2 == Equipment
    //3 == Inventory
    //4 == Crafting
    //5 == Abilities
    //6 == Quests
    //7 == Map
    //8 == Bestiary
    //9 == Jukebox
    //10 == Trophies
    //11 == Tutorial
    //12 == Config

    GameObject[] buttons = new GameObject[12];

    UIManager UI;
    Image frame_Image;
    Image background_Image;

    [System.NonSerialized]public CanvasGroup canvas;

    public bool central;

    private void Awake() 
    {
        background_Image = transform.GetChild(0).GetComponentInChildren<Image>();
        canvas = GetComponent<CanvasGroup>();
    }

    public void SwitchMenu(int i)
    {
        for(int j = 0; j < buttons.Length; j++)
        {
            buttons[j].GetComponent<EventTrigger>().triggers.Clear();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener( (data) => AudioManager.PlaySFX("button_hover") );

            if(j < buttonLayouts[i].buttons.Count)
            {
                buttons[j].GetComponent<Image>().color = UIManager.Instance.UIColor.primary;
                buttons[j].GetComponent<Image>().raycastTarget = true;
                int index_1 = i; int index_2 = j;
                buttons[j].GetComponentInChildren<SpriteText>().Write(buttonLayouts[i].buttons[j].name);
                buttons[j].GetComponent<Button>().onClick.RemoveAllListeners();

                if(buttonLayouts[index_1].buttons[index_2].destination >= 0)
                {
                    buttons[j].GetComponent<Button>().onClick.AddListener(() => SwitchMenu(buttonLayouts[index_1].buttons[index_2].destination) );
                    buttons[j].GetComponent<Button>().onClick.AddListener(() => MenuTooltip.UpdateUpperTooltip(" "));
                    
                    entry.callback.AddListener( (data) => MenuTooltip.UpdateUpperTooltip(buttonLayouts[index_1].buttons[index_2].tooltip));

                    EventTrigger.Entry exit_entry = new EventTrigger.Entry();
                    exit_entry.eventID = EventTriggerType.PointerExit;
                    exit_entry.callback.AddListener( (data) => MenuTooltip.UpdateUpperTooltip(" "));
                    buttons[j].GetComponent<EventTrigger>().triggers.Add(exit_entry);
                }
                else if(buttonLayouts[index_1].buttons[index_2].destination == -1)
                {
                    buttons[j].GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(1));
                }
                else
                {
                    buttons[j].GetComponent<Button>().onClick.AddListener(() => {Application.Quit(); Debug.Log("Quit the game from Menu");});
                }
                buttons[j].GetComponent<Button>().onClick.AddListener(() => AudioManager.PlaySFX("button_click"));
                if(buttonLayouts[i].buttons[j].menu != null)
                {
                    UnityEngine.Events.UnityAction temp = () => UIManager.OpenOrClose(buttonLayouts[index_1].buttons[index_2].menu.GetComponent<IMenu>().GetCanvas());
                    buttons[j].GetComponent<Button>().onClick.AddListener(temp);
                    temp = () => UI.AddMenu(buttonLayouts[index_1].buttons[index_2].menu.GetComponent<IMenu>().GetCanvas());
                    buttons[j].GetComponent<Button>().onClick.AddListener(temp);
                    temp = () => buttonLayouts[index_1].buttons[index_2].menu.GetComponent<IMenu>().OnOpen();
                    buttons[j].GetComponent<Button>().onClick.AddListener(temp);
                }
            }
            else if(j < buttons.Length - 1 || buttonLayouts[i].parentMenu < 0)
            {
                buttons[j].GetComponent<Image>().color = Color.clear;
                buttons[j].GetComponentInChildren<SpriteText>().Write("");
                buttons[j].GetComponent<Image>().raycastTarget = false;
            }
            else if(buttonLayouts[i].parentMenu >= 0)
            {
                //Make return button
                buttons[j].GetComponent<Image>().color = UIManager.Instance.UIColor.primary;
                buttons[j].GetComponent<Image>().raycastTarget = true;
                buttons[j].GetComponentInChildren<SpriteText>().Write("Return");
                Button button = buttons[j].GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SwitchMenu(buttonLayouts[i].parentMenu) );
                button.onClick.AddListener(() => AudioManager.PlaySFX("button_return"));
                button.onClick.AddListener(() => UI.EmptyMenus());
                entry.callback.AddListener( (data) => MenuTooltip.UpdateUpperTooltip("return"));
                EventTrigger.Entry exit_entry = new EventTrigger.Entry();
                exit_entry.eventID = EventTriggerType.PointerExit;
                exit_entry.callback.AddListener( (data) => MenuTooltip.UpdateUpperTooltip(" "));
                buttons[j].GetComponent<EventTrigger>().triggers.Add(exit_entry);
            }
            buttons[j].GetComponent<EventTrigger>().triggers.Add(entry);
        }
    }
    public void Initialize(UIManager UI_in, AudioSource audio)
    {
        UI = UI_in;

        GameObject buttonTransform = new GameObject("Buttons"); buttonTransform.transform.parent = transform;

        RectTransform rect = buttonTransform.AddComponent<RectTransform>(); 
        rect.localScale = new Vector3(1,1,1);
        rect.anchorMax = new Vector2(0,1);
        rect.anchorMin = new Vector2(0,1);
        if(central)
        {
            rect.localPosition = new Vector3(-30, -80, 0);
        }
        else
        {
            rect.localPosition = new Vector3(-254, 77, 0);
        }

        GridLayoutGroup gridLayout = buttonTransform.AddComponent<GridLayoutGroup>();
        Sprite buttonSprite = Resources.Load<Sprite>("Art/UI/MenuButton");
        gridLayout.cellSize = new Vector2(buttonSprite.texture.width, buttonSprite.texture.height);
        gridLayout.spacing = new Vector2(0, -13);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = new GameObject("Button: " + (i+1)); buttons[i].transform.parent = buttonTransform.transform; 
            buttons[i].AddComponent<RectTransform>();
            buttons[i].GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
            buttons[i].AddComponent<Image>();
            buttons[i].GetComponent<Image>().sprite = buttonSprite;
            buttons[i].AddComponent<Button>();
            buttons[i].GetComponent<Button>().image = buttons[i].GetComponent<Image>();
            buttons[i].AddComponent<EventTrigger>();

            GameObject textObject = new GameObject("Text"); textObject.transform.parent = buttons[i].transform;
            textObject.AddComponent<SpriteText>();
            textObject.GetComponent<SpriteText>().Initialize(UI.graphemeDatabase.fonts[0], false);
            textObject.GetComponent<SpriteText>().text = "";
            textObject.GetComponent<SpriteText>().spaceSize = 8;
            textObject.AddComponent<RectTransform>();
            textObject.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
            textObject.GetComponent<RectTransform>().localPosition = new Vector3(-56, 46, 0);
        }
        GameObject frame_temp = new GameObject("Frame"); frame_temp.transform.parent = this.transform;
        RectTransform frame_rect = frame_temp.AddComponent<RectTransform>();
        frame_rect.localPosition = Vector3.zero;
        frame_rect.localScale = new Vector3(1,1,1);
        GameObject frame_visuals = new GameObject("Visuals"); frame_visuals.transform.parent = frame_temp.transform;
        frame_Image = frame_visuals.AddComponent<Image>();
        frame_Image.sprite = Resources.Load<Sprite>("Art/UI/Frame");
        frame_Image.transform.localPosition = Vector2.zero;
        frame_Image.SetNativeSize();
        frame_Image.raycastTarget = false;
        frame_Image.transform.localScale = new Vector3(1,1,1);

        UIManager.Instance.UIColor.SetPrimaryColor(Color.red, UIManager.Instance);
    }
    public void ChangeColor(Color newColor)
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<Image>().color = new Color(newColor.r, newColor.g, newColor.b, buttons[i].GetComponent<Image>().color.a);
        }
        frame_Image.color = newColor;
        background_Image.color = newColor;
    }
}
