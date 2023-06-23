using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void OnCreate(LevelManager levelManager, FurnitureDatabase furnitureDatabase);
    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel);

    public bool GetIsInteractable();

    public void OnLeaveInteractable();
}
