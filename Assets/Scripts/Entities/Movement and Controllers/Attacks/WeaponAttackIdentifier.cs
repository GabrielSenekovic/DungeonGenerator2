using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttackIdentifier : AttackIdentifier
{
    public enum WEAPONATTACK_TYPE
    {
        NONE = 0,
        SLASH = 1,
        THRUST = 2,
        SWING = 3
    }
    Animator anim; //It has to trigger an animation of the swinging of the weapon
    [SerializeField] WEAPONATTACK_TYPE type;
    public override void OnFixedUpdate(Vector3 direction, Vector3 source, Collider castersCollider)
    {
        if (UpdateCasting()) //If this attack has started but isn't over
        {
            state = CastingState.DONE;
            anim.SetTrigger(type.ToString());
        }
    }
    public override void Attack()
    {
        if(anim == null)
        {
            anim = Party.instance.GetPartyLeader().gameObject.GetComponent<EquipmentModel>().anim;
        }
        state = CastingState.COMMENCED;
        //OnAttack(direction, source, collider);
    }
}
