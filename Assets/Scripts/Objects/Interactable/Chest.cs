using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public Transform lid;
    float angle = 0;

    bool open = false;
    bool isInteractable = true;
    bool isInteractedWith = false;

    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        isInteractedWith = true;
        open = true;
    }
    void Update()
    {
        if(open && angle < 90)
        {
            lid.RotateAround(new Vector3(0.5f, 0.5f, -0.6f), Vector3.left, -5);
            angle +=5;
        }
        else if(!open && angle > 0)
        {
            lid.RotateAround(new Vector3(0.5f, 0.5f, -0.6f), Vector3.left, 5);
            angle -=5;
        }
    }

    public void OnLeaveInteractable()
    {
        open = false;
    }

    public bool GetIsInteractable() => isInteractable;
}
