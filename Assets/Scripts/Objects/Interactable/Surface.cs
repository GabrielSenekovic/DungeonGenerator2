using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour, IInteractable
{
    [SerializeField] List<Transform> chairPlacements = new List<Transform>();
    public bool GetIsInteractable()
    {
        return true;
    }

    public void OnCreate(LevelManager levelManager, FurnitureDatabase furnitureDatabase)
    {
        //Create chairs?
        for(int i = 0; i < chairPlacements.Count; i++)
        {
            levelManager.currentRoom.Furnish("Chair", levelManager, furnitureDatabase, chairPlacements[i]);
        }
    }

    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        //Pick things off surface, or interact with things on surface
    }

    public void OnLeaveInteractable()
    {
    }
}