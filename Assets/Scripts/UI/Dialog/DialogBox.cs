using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class DialogBox : MonoBehaviour
{
    [SerializeField]float speechDelay;
    public bool messageDone = false;

    public bool dialogDone = false;
    public bool breakPrint = false;

    Manuscript.Dialog currentDialog;

    public Sprite[] mugshotSprites;

    public int lineIndex = 0;
    public int lineNumber = 0;

    public CanvasGroup promptWindow;
    public Transform promptButtonTransform;

    SpriteText dialogText;
    SpriteText nameText;

    private void Start() 
    {
        GridLayoutGroup gridLayout = promptButtonTransform.gameObject.AddComponent<GridLayoutGroup>();
        Sprite buttonSprite = Resources.Load<Sprite>("Art/UI/MenuButton");
        gridLayout.cellSize = new Vector2(buttonSprite.texture.width, buttonSprite.texture.height);
        gridLayout.spacing = new Vector2(0, -13);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

        GameObject dialogTextObj = new GameObject("Dialog");
        dialogTextObj.transform.parent = transform;
        dialogTextObj.AddComponent<RectTransform>();
        dialogTextObj.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
        dialogTextObj.GetComponent<RectTransform>().localPosition = new Vector3(-270, 70, 0);
        dialogText = dialogTextObj.AddComponent<SpriteText>();
        dialogText.Initialize(UIManager.Instance.graphemeDatabase.fonts[0], false);
        dialogText.spaceSize = 8;
        dialogText.Write("");

        GameObject nameTextObj = new GameObject("Name");
        nameTextObj.transform.parent = transform;
        nameTextObj.AddComponent<RectTransform>();
        nameTextObj.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
        nameTextObj.GetComponent<RectTransform>().localPosition = new Vector3(-290, 90, 0);
        nameText = nameTextObj.AddComponent<SpriteText>();
        nameText.Initialize(UIManager.Instance.graphemeDatabase.fonts[0], false);
        nameText.spaceSize = 8;
        nameText.Write("");

        gameObject.SetActive(false);
    }

    public void InitiateDialog(Manuscript.Dialog dialog)
    {
        lineIndex = 0;
        dialogDone = false;
        currentDialog = dialog;
        messageDone  = true;
        lineNumber = dialog.currentNode.lines.Count;
        ContinueDialog();
    }
    public void Say(Manuscript.Dialog.DialogNode.Line line)
    {
        lineIndex++;
        dialogDone = true;
        StartCoroutine(PrintMessage(line.myLine));
    }

    public void Say(Manuscript.Dialog lines, int amountOfLines)
    {
        dialogDone = false;
        messageDone = true;
        lineNumber = amountOfLines;
        currentDialog = lines;
        ContinueDialog();
    }

    public void ContinueDialog()
    {
        if(promptWindow.alpha == 1) { return; }
        if(messageDone)
        {
            dialogText.Write("");
            nameText.Write(currentDialog.currentNode.lines[lineIndex].myIdentity.name);
            StartCoroutine(PrintMessage(currentDialog.currentNode.lines[lineIndex].myLine));
            lineIndex++;
            lineNumber--;
            if(lineNumber == 0)
            {
                if(currentDialog.currentNode.options.Count > 0)
                {
                    //show prompt
                    promptWindow.alpha = 1;
                    for(int i = 0; i < currentDialog.currentNode.options.Count; i++)
                    {
                        GameObject button = new GameObject("Button: " + currentDialog.currentNode.options[i].promptName); button.transform.parent = promptButtonTransform; 
                        button.AddComponent<Button>();
                        button.AddComponent<EventTrigger>();
                        button.AddComponent<RectTransform>();
                        button.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);

                        button.AddComponent<Image>();
                        button.GetComponent<Image>().sprite = Resources.Load<Sprite>("Art/UI/MenuButton");

                        button.GetComponent<Button>().image = button.GetComponent<Image>();

                        EventTrigger.Entry entry = new EventTrigger.Entry();
                        entry.eventID = EventTriggerType.PointerEnter;
                        entry.callback.AddListener( (data) => AudioManager.PlaySFX("button_hover") );

                        Manuscript.Dialog.DialogNode node = currentDialog.currentNode.options[i].destinationDialog;
                        Action action = currentDialog.currentNode.options[i].action;

                        button.GetComponent<Button>().onClick.AddListener(() => promptWindow.alpha = 0);
                        if (node != null)
                        {
                            button.GetComponent<Button>().onClick.AddListener(() => { currentDialog.currentNode = node; InitiateDialog(currentDialog); });
                        }
                        else
                        {
                            button.GetComponent<Button>().onClick.AddListener(() =>
                            {
                                UIManager.ToggleHUD();
                                gameObject.SetActive(false);
                                Time.timeScale = 1;
                            });
                        }
                        if(action != null)
                        {
                            button.GetComponent<Button>().onClick.AddListener(() => action());
                        }
                        
                        button.GetComponent<Button>().onClick.AddListener(() => DestroyPromptButtons());

                        GameObject textObject = new GameObject("Text"); textObject.transform.parent = button.transform;
                        textObject.AddComponent<RectTransform>();
                        textObject.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
                        textObject.GetComponent<RectTransform>().localPosition = new Vector3(-56, 46, 0);
                        textObject.AddComponent<SpriteText>();
                        textObject.GetComponent<SpriteText>().Initialize(UIManager.Instance.graphemeDatabase.fonts[0], false);
                        textObject.GetComponent<SpriteText>().spaceSize = 8;
                        textObject.GetComponent<SpriteText>().Write(currentDialog.currentNode.options[i].promptName);
                    }
                }
                else
                {
                    dialogDone = true;
                }
            }
        }
        else
        {
            breakPrint = true;
        }
    }
    void DestroyPromptButtons()
    {
        for(int i = promptButtonTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(promptButtonTransform.GetChild(i).gameObject);
        }
    }
    public IEnumerator PrintMessage(string text)
    {
        messageDone = false;
        foreach(char c in text)
        {
            if(breakPrint)
            {
                breakPrint = false;
                dialogText.Write(text);
                break;
            }
            if(c != ' ')
            {
                yield return new WaitForSecondsRealtime(speechDelay);
            }
            dialogText.Write(c);
           // GetComponentInChildren<Text>().text += c;
        }
        messageDone = true;
    }
}