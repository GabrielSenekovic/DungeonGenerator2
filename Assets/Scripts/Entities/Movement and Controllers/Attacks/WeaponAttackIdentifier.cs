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
        SWING = 3,
        DRAW = 4 //Bow
    }
    Animator anim; //It has to trigger an animation of the swinging of the weapon
    [SerializeField] WEAPONATTACK_TYPE type;
    [SerializeField] Equipment.EquipmentType equipmentType;
    [SerializeField] ProjectileAttackIdentifier projectile;
    public override void OnFixedUpdate(Vector3 direction, Vector3 source, Collider castersCollider)
    {
        if (projectile)
        {
            projectile.OnFixedUpdate(direction, source, castersCollider);
            if (projectile.state == CastingState.DONE)
            {
                state = CastingState.DONE;
            }
        }
        else if (UpdateCasting()) //If this attack has started but isn't over
        {
            state = CastingState.DONE;
        }
    }
    public void AddAnimator(Animator anim)
    {
        this.anim = anim;
    }
    public override void Attack()
    {
        if(anim == null)
        {
            anim = Party.instance.GetPartyLeader().gameObject.GetComponent<EquipmentModel>().anim;
        }
        state = CastingState.COMMENCED;
        if (projectile)
        {
            projectile.Attack();
        }
        anim.SetTrigger(type.ToString());
    }
    public Equipment.EquipmentType EquipmentType => equipmentType;
}
