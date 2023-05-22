using UnityEngine;
using System.Collections;

public class PlayerInteractionModel : MonoBehaviour
{
    Collider2D collider;
    [SerializeField]IInteractable interactable;
    StatusConditionModel statusConditionModel;

    private void Start()
    {
        statusConditionModel = GetComponent<StatusConditionModel>();
    }

    public void Initialize(Collider2D collider)
    {
        this.collider = collider;
    }

    public void OnInteract()
    {
        if (interactable != null)
        {
            interactable.OnInteract(this, statusConditionModel);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.TryGetComponent(out IInteractable interactable) && interactable.GetIsInteractable())
        {
            this.interactable = interactable;
        } 
    }
    public void OnCollisionExit(Collision collision)
    {
        if (collision.collider.GetComponent<IInteractable>() != null)
        {
            if (interactable != null)
            {
                interactable.OnLeaveInteractable();
                interactable = null;
            }
        }
    }
}
