using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carryable : MonoBehaviour, IInteractable
{
    Rigidbody body;
    Transform parent;

    public void Awake()
    {
        body = GetComponent<Rigidbody>();
    }
    public bool GetIsInteractable() => true;

    public void OnCreate(LevelManager levelManager, FurnitureDatabase furnitureDatabase)
    {
        parent = transform.parent;
    }

    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        body.useGravity = !body.useGravity;
        if (!body.useGravity)
        {
            transform.parent = interactionModel.transform;
            transform.position = interactionModel.transform.position + new Vector3(0, 0, -2);
            body.velocity = Vector2.zero;
            body.isKinematic = true;
            interactionModel.Carry(this);
        }
        else
        {
            transform.parent = parent;
            body.isKinematic = false;
        }
    }

    public void OnLeaveInteractable()
    {
       
    }
}
