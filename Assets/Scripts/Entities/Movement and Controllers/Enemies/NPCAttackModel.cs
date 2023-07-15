using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAttackModel : AttackModel
{
    List<AttackIdentifier> attacks = new List<AttackIdentifier>();
    public int attackTimer = 0;
    public int attackTimerFrequency; //Also known as Agression. How often will you try to attack
    float brutality = 0.5f; //Likelihood of attack
    MovementModel movementModel;

    private void Start() 
    {
        movementModel = GetComponent<MovementModel>();
    }
    private void FixedUpdate()
    {
        if (currentAttack == null || currentAttack.state == AttackIdentifier.CastingState.DONE) { return; }
        currentAttack.OnFixedUpdate(movementModel.GetFacingDirection(), new Vector3(transform.position.x, transform.position.y, transform.position.z - castingHeight), GetComponent<Collider>());
    }

    public void Attack(Vector2 direction)
    {
        if(attacks.Count == 0){return;}
        attackTimer++;
        if(attackTimer >= attackTimerFrequency && Random.Range(0.0f, 1.0f) <= brutality)
        {
            attackTimer = 0;
            int i = Random.Range(0, attacks.Count);
            currentAttack = attacks[i];
            attacks[i].Attack();
        }
    }
    public void AddAttack(AttackIdentifier attack)
    {
        attacks.Add(attack);
        if(attack is MovementAttackIdentifier)
        {
            (attack as MovementAttackIdentifier).SetController(GetComponent<NPCController>());
        }
    }
}
