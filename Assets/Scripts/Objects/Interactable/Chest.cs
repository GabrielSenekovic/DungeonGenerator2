using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InteractableBase
{
    public Transform lid;
    float angle = 0;

    bool open = false;

    public override void OnInteract()
    {
        base.OnInteract();
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

    public override void OnLeaveInteractable()
    {
        open = false;
    }
}
