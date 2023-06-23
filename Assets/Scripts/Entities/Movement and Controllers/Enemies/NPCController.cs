﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public int directionShiftTimer = 0;
    public int directionShiftFrequency;
    public enum NPCMovementState
    {
        CHASING = 0,
        ESCAPING = 1,
        IDLE = 2,
        WANDERING = 3,
        WORKING = 4 //Work if not finished instead of going idle
    }
    NPCMovementState movementState = NPCMovementState.IDLE;

    MovementModel movementModel;
    StatusConditionModel statusConditionModel;
    NPCAttackModel attackModel;
    SphereCollider visionCollider;
    Transform target;
    [SerializeField] CharacterData characterData;

    public float gizmoWidth;


    private void Awake()
    {
        movementModel = GetComponent<MovementModel>();
        statusConditionModel = GetComponent<StatusConditionModel>();
        attackModel = GetComponent<NPCAttackModel>();

        visionCollider = gameObject.AddComponent<SphereCollider>();
        visionCollider.radius = 6;
        visionCollider.isTrigger = true;
        
        VisualsRotator.Add(GetComponentInChildren<MeshRenderer>());

        MeshRenderer rend = GetComponentInChildren<MeshRenderer>();
        rend.material = new Material(rend.material);
    }
    private void Start()
    {
    }
    private void FixedUpdate()
    {
        if(movementState == NPCMovementState.WANDERING)
        {
            if(statusConditionModel.IfHasCondition(Condition.Rigid)){RigidMovement();}
            else
            {
                Wander();
            }
        }
        else if(movementState == NPCMovementState.CHASING && target)
        {
            movementModel.SetMovementDirection((target.position - transform.position).normalized);
            //Attack();
        }
        else if(movementState == NPCMovementState.ESCAPING)
        {
            movementModel.SetMovementDirection((transform.position - target.position).normalized);
        }
        else if(movementState == NPCMovementState.WORKING)
        {
            //Perform profession
            bool finished = characterData.profession.Work();
            if(finished)
            {
                movementState = NPCMovementState.IDLE;
            }
        }
    }

    void Attack()
    {
        attackModel.Attack(movementModel.GetFacingDirection());
    }

    void Wander()
    {
        //Walk in one direction, stop, walk in another, stop
        directionShiftTimer++;
        if (directionShiftTimer >= directionShiftFrequency)
        {
            directionShiftTimer = 0;
            movementModel.SetMovementDirection(new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized);
        }
        else
        {
            movementModel.SetMovementDirection(movementModel.GetFacingDirection());
        }
    }

    void RigidMovement()
    {
        Vector2[] directions = 
        new Vector2[4] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };

        directionShiftTimer++;
        if (directionShiftTimer >= directionShiftFrequency)
        {
            directionShiftTimer = 0;
            movementModel.SetMovementDirection(directions[Random.Range(0, 4)]);
        }
        else
        {
            movementModel.SetMovementDirection(movementModel.GetFacingDirection());
        }
    }
    void Sleep()
    {
        //Reset daily mission
        characterData.profession.Reset();
    }

    private void OnTriggerStay(Collider other) 
    {
        if(!target && other.CompareTag("Player"))
        {
            movementState = NPCMovementState.CHASING;
            target = other.transform;
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            movementState = NPCMovementState.WANDERING;
            target = null;
        }
    }

    public CharacterData GetData()
    {
        return characterData;
    }

    private void OnDrawGizmos() 
    {
        switch(movementState)
        {
            case NPCMovementState.IDLE: Gizmos.color = Color.green; break;
            case NPCMovementState.CHASING: Gizmos.color = Color.red; break;
            case NPCMovementState.ESCAPING: Gizmos.color = Color.yellow; break;
            case NPCMovementState.WANDERING: Gizmos.color = Color.blue; break;
            case NPCMovementState.WORKING: Gizmos.color = Color.cyan; break;
        }
        Gizmos.DrawSphere(transform.position, gizmoWidth);
    }
}
