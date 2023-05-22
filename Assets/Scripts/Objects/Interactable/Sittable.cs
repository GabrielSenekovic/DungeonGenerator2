using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sittable : MonoBehaviour, IInteractable
{
    bool isInteractable = true;
    bool isInteractedWith = false;
    [SerializeField]Transform sittingPosition;
    public bool GetIsInteractable() => isInteractable;

    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        if(statusConditionModel.IfHasCondition(Condition.Sitting))
        {
            statusConditionModel.RemoveCondition(Condition.Sitting);
        }
        else
        {
            interactionModel.transform.position = sittingPosition.position;
            statusConditionModel.AddCondition(new StatusConditionModel.StatusCondition(Condition.Sitting));
        }
        isInteractedWith = true;
    }

    public void OnLeaveInteractable()
    {
        return;
    }
    public void SetSittingPosition(Transform transform)
    {
        sittingPosition = transform;
    }
}
