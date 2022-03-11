using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : InteractableBase
{
    Manuscript.Dialog dialog;
    public Manuscript.Dialog tutorialDialog = new Manuscript.Dialog(new Manuscript.Dialog.DialogNode
    (
        new List<Manuscript.Dialog.DialogNode.Line>()
        {
            new Manuscript.Dialog.DialogNode.Line("Hi!", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P1),
            new Manuscript.Dialog.DialogNode.Line("Oh hello, I'm an NPC", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P2),
            new Manuscript.Dialog.DialogNode.Line("Nice to meet you, what are you doing?", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P2),
            new Manuscript.Dialog.DialogNode.Line("I'm just standing here", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P1),
            new Manuscript.Dialog.DialogNode.Line("Do you wanna eat an ice cream?", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P2)
        },
        new List<Manuscript.Dialog.DialogNode.PromptOption>()
        {
            new Manuscript.Dialog.DialogNode.PromptOption("Yes", new Manuscript.Dialog.DialogNode(
                new List<Manuscript.Dialog.DialogNode.Line>()
                {
                    new Manuscript.Dialog.DialogNode.Line("Okay lets go!", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P1),
                },new List<Manuscript.Dialog.DialogNode.PromptOption>(){}
            )),
            new Manuscript.Dialog.DialogNode.PromptOption("No", new Manuscript.Dialog.DialogNode(
                new List<Manuscript.Dialog.DialogNode.Line>()
                {
                    new Manuscript.Dialog.DialogNode.Line("Awww...", Manuscript.Dialog.DialogNode.Line.CharacterIdentity.P1),
                },new List<Manuscript.Dialog.DialogNode.PromptOption>(){}
            ))
        }
    ));
    public override void OnInteract()
    {
        if (isInteractable)
        {
            //isInteractedWith = true;
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
}
