using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAttackIdentifier : AttackIdentifier
{
    [SerializeField] NPCController.NPCMovementState transitionState;
    [SerializeField] AttackIdentifier effectOnImpact;
    Animator anim;
    NPCController npcController;

    public override void OnFixedUpdate(Vector3 direction, Vector3 source, Collider castersCollider)
    {
        if (UpdateCasting()) //If this attack has started but isn't over
        {
            state = CastingState.DONE;
            npcController.SwapState(transitionState);
            //anim.SetTrigger(transitionState.ToString());
        }
    }
    public override void Attack()
    {
        if (anim == null)
        {
            anim = Party.instance.GetPartyLeader().gameObject.GetComponent<EquipmentModel>().anim;
        }
        state = CastingState.COMMENCED;
        //OnAttack(direction, source, collider);
    }
    public void SetController(NPCController controller)
    {
        npcController = controller;
    }
}