using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unlockable : MonoBehaviour, IInteractable
{
    bool locked;
    BoxCollider box;

    public Unlockable otherDoor;
    bool isInteractable = true;
    bool isInteractedWith = false;

    private void Awake() 
    {
        locked = true;
        box = GetComponent<BoxCollider>();
    }
    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        if(Party.Unlock())
        {
            Unlock();
        }
    }
    public void Unlock()
    {
        locked = false;
        box.enabled = false;
        otherDoor.locked = false;
        otherDoor.box.enabled = false;
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