using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    Manuscript.Dialog dialog;
    public Manuscript.Dialog tutorialDialog;
    bool isInteractable = true;
    bool isInteractedWith = false;
    private void Start() 
    {
        LoadDialog();
    }
    public void LoadDialog()
    {
        string myName = GetComponent<CharacterData>().GetName();
        tutorialDialog = new Manuscript.Dialog(new Manuscript.Dialog.DialogNode
        (
            new List<Manuscript.Dialog.DialogNode.Line>()
            {
                new Manuscript.Dialog.DialogNode.Line("Hi!", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity("Player")),
                new Manuscript.Dialog.DialogNode.Line("Oh hello, I'm an NPC", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity(myName)),
                new Manuscript.Dialog.DialogNode.Line("Nice to meet you, what are you doing?", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity("Player")),
                new Manuscript.Dialog.DialogNode.Line("I'm just standing here", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity(myName)),
                new Manuscript.Dialog.DialogNode.Line("Do you wanna eat an ice cream?", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity("Player"))
            },
            new List<Manuscript.Dialog.DialogNode.PromptOption>()
            {
                new Manuscript.Dialog.DialogNode.PromptOption("Yes", new Manuscript.Dialog.DialogNode(
                    new List<Manuscript.Dialog.DialogNode.Line>()
                    {
                        new Manuscript.Dialog.DialogNode.Line("Okay lets go!", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity(myName)),
                    },new List<Manuscript.Dialog.DialogNode.PromptOption>(){}
                )),
                new Manuscript.Dialog.DialogNode.PromptOption("No", new Manuscript.Dialog.DialogNode(
                    new List<Manuscript.Dialog.DialogNode.Line>()
                    {
                        new Manuscript.Dialog.DialogNode.Line("Awww...", new Manuscript.Dialog.DialogNode.Line.CharacterIdentity(myName)),
                    },new List<Manuscript.Dialog.DialogNode.PromptOption>(){}
                ))
            }
        ));
    }
    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        if (isInteractable)
        {
            isInteractedWith = true;
            if(!UIManager.Instance.dialogBox.gameObject.activeSelf)
            {
                UIManager.StartDialog(tutorialDialog);
            }
            else if(!UIManager.Instance.dialogBox.dialogDone)
            {
                UIManager.Instance.dialogBox.ContinueDialog();
            }
            else
            {
                UIManager.EndDialog();
            }
        }
    }

    public bool GetIsInteractable() => isInteractable;

    public void OnLeaveInteractable()
    {
        return;
    }
}
