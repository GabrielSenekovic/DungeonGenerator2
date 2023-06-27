using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentModel : MonoBehaviour
{
    //TODO

    //Prompt to equip skill in which slot when equipping weapon. Prompt to equip weapon when equipping skill
    //When weapons are visually there, can't interact
    //Weapons automatically go away when no enemies are nearby
    //Weapons visually appear when attacking
    [SerializeField] GameObject leftHandItem;
    [SerializeField] GameObject rightHandItem;
    public Animator anim;

    public void TakeOutWeapons()
    {
        if(leftHandItem != null)
        {
            leftHandItem.SetActive(true);
        }
        if (rightHandItem != null)
        {
            rightHandItem.SetActive(true);
        }
    }
    public void PutAwayWeapons()
    {
        if (leftHandItem != null)
        {
            leftHandItem.SetActive(false);
        }
        if (rightHandItem != null)
        {
            rightHandItem.SetActive(false);
        }
    }
}
