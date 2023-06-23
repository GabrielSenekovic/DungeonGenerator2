using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    Manuscript.Dialog dialog;
    bool isInteractable = true;
    bool isInteractedWith = false;

    DialogManager dialogManager;
    public void Initialize(DialogManager dialogManager, Manuscript.Dialog dialog)
    {
        this.dialogManager = dialogManager;
        this.dialog = dialog;
    }
    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        if (isInteractable)
        {
            isInteractedWith = true;
            if(!dialogManager.DialogActive())
            {
                dialogManager.StartDialog(dialog);
            }
            else if(!dialogManager.DialogDone())
            {
                dialogManager.ContinueDialog();
            }
            else
            {
                dialogManager.EndDialog();
            }
        }
    }

    public bool GetIsInteractable() => isInteractable;

    public void OnLeaveInteractable()
    {
        return;
    }

    public void OnCreate(LevelManager levelManager, FurnitureDatabase furnitureDatabase)
    {
    }
}
