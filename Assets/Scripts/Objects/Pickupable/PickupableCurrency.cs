using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableCurrency : MonoBehaviour, PickupableBase
{
    public int value;
    public void OnPickup()
    {
        UIManager.Instance.moneyCounter.Add(value);
        Destroy(transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            OnPickup();
        }
    }
}
