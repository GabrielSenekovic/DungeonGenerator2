using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unlockable : InteractableBase
{
    bool locked;
    BoxCollider box;

    public Unlockable otherDoor;

    private void Awake() 
    {
        locked = true;
        box = GetComponent<BoxCollider>();
    }
    public override void OnInteract()
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
}