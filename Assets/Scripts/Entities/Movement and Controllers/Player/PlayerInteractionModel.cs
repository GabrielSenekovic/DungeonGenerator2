using UnityEngine;
using System.Collections;

public class PlayerInteractionModel : MonoBehaviour
{
    Collider2D collider;
    [SerializeField]InteractableBase interactable;

    public void Initialize(Collider2D collider)
    {
        this.collider = collider;
    }

    public void OnInteract()
    {
        if (interactable != null)
        {
            interactable.OnInteract();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<InteractableBase>() && collision.collider.GetComponent<InteractableBase>().GetIsInteractable() == true)
        {
            interactable = collision.collider.GetComponent<InteractableBase>();
        }     
    }
    public void OnCollisionExit(Collision collision)
    {
        if (collision.collider.GetComponent<InteractableBase>())
        {
            if (interactable != null)
            {
                interactable.OnLeaveInteractable();
                interactable = null;
            }
        }
    }
}
