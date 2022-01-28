using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackModel : AttackModel
{
    [System.Serializable]public class Attack
    {
        public AttackIdentifier attack;
        public KeyCode key;

    }
    public Attack[] attacks;

    private void Start() 
    {
        currentAttack = attacks[0].attack;
        currentAttack.state = AttackIdentifier.CastingState.DONE;
        for(int i = 0; i < attacks.Length; i++)
        {
            attacks[i].attack.Initialize();
        }
    }
    private void FixedUpdate()
    {
        if(currentAttack == null || currentAttack.state == AttackIdentifier.CastingState.DONE){ return; }
        currentAttack.OnFixedUpdate(GetComponent<MovementModel>().GetFacingDirection(),new Vector3(transform.position.x, transform.position.y, transform.position.z - castingHeight), GetComponent<Collider>());
    }

    public void UpdateAttack() //This is called from Playercontroller
    {
        if(currentAttack.state == AttackIdentifier.CastingState.DONE) //Only attack if you are done with previous attack
        {
            for(int i = 0; i < 4; i++) //Go through all skillslots
            {
                if(attacks[i].attack == null) { continue; } //If this skillslot is empty, continue
                if(Input.GetKeyDown(attacks[i].key)) //This is where it checks the button to cause the attack
                {
                    currentAttack = attacks[i].attack; //Set current attack to the skill in this slot
                    currentAttack.Attack(); //Activate it
                }
            }
        }
    }
}