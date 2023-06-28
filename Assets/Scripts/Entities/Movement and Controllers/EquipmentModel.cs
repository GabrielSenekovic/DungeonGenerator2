using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentModel : MonoBehaviour
{
    //TODO

    //Prompt to equip skill in which slot when equipping weapon. Prompt to equip weapon when equipping skill
    //When weapons are visually there, can't interact
    //Weapons automatically go away when no enemies are nearby
    //Weapons visually appear when attacking
    [SerializeField] public Equipment leftHandItem;
    [SerializeField] GameObject rightHandItem;
    public Animator anim;

    bool weaponsOut;

    [SerializeField] int putAwayWeaponTimerMax;
    int putAwayWeaponTimer;

    private void Awake()
    {
        putAwayWeaponTimer = 0;
        weaponsOut = false;
    }

    private void FixedUpdate()
    {
        if(weaponsOut)
        {
            putAwayWeaponTimer++;
            if (putAwayWeaponTimer >= putAwayWeaponTimerMax)
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position, 10, transform.forward);
                if (!hits.Any(h => h.collider.GetComponent<EnemyHPBar>()))
                {
                    PutAwayWeapons();
                }
            }
        }
    }

    public void TakeOutWeapons()
    {
        if(leftHandItem != null)
        {
            leftHandItem.gameObject.SetActive(true);
        }
        if (rightHandItem != null)
        {
            rightHandItem.SetActive(true);
        }
        weaponsOut = true;
        putAwayWeaponTimer = 0;
    }
    public void PutAwayWeapons()
    {
        if (leftHandItem != null)
        {
            leftHandItem.gameObject.SetActive(false);
        }
        if (rightHandItem != null)
        {
            rightHandItem.SetActive(false);
        }
        weaponsOut = false;
    }
    public void ActivateCollider()
    {
        if(leftHandItem)
        {
            leftHandItem.GetComponent<SphereCollider>().enabled = true;
        }
    }
    public void DeactivateCollider()
    {
        if(leftHandItem)
        {
            leftHandItem.GetComponent<SphereCollider>().enabled = false;
        }
    }
}
