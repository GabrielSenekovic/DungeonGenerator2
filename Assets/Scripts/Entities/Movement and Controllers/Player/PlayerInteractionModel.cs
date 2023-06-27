using UnityEngine;
using System.Collections;

public class PlayerInteractionModel : MonoBehaviour
{
    Collider2D collider;
    [SerializeField]IInteractable interactable;
    Carryable carryable;
    StatusConditionModel statusConditionModel;
    EntityStatistics entityStatistics;

    private void Start()
    {
        statusConditionModel = GetComponent<StatusConditionModel>();
        entityStatistics = GetComponent<EntityStatistics>();
    }

    public void Initialize(Collider2D collider)
    {
        this.collider = collider;
    }

    public void OnInteract()
    {
        if (interactable != null && carryable == null)
        {
            interactable.OnInteract(this, statusConditionModel);
        }
        else if(carryable != null)
        {
            carryable.OnInteract(this, statusConditionModel);
            carryable = null;
        }
    }
    public void Release()
    {
        interactable.OnLeaveInteractable();
        interactable = null;
    }
    public void Carry(Carryable carryable)
    {
        this.carryable = carryable;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(entityStatistics.canInteract && collision.collider.TryGetComponent(out IInteractable interactable) && interactable.GetIsInteractable())
        {
            this.interactable = interactable;
        } 
    }
    public void OnCollisionExit(Collision collision)
    {
        if (entityStatistics.canInteract && collision.collider.GetComponent<IInteractable>() != null)
        {
            if (interactable != null)
            {
                Release();
            }
        }
    }
}
