using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    [SerializeField] EquipmentModel equipmentModel;

    public void ActivateCollider()
    {
        equipmentModel.ActivateCollider();
    }
    public void DeactivateCollider()
    {
        equipmentModel.DeactivateCollider();
    }
}
