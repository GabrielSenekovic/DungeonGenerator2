using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sleepable : MonoBehaviour, IInteractable
{
    bool isInteractable = true;
    bool isInteractedWith = false;
    public bool GetIsInteractable() => isInteractable;

    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        DunGenes.Instance.GetDayNightCycle().Sleep();
    }

    public void OnLeaveInteractable()
    {
        return;
    }
}