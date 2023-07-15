using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public int directionShiftTimer = 0;
    public int directionShiftFrequency;
    public enum NPCMovementState
    {
        NONE = 0,
        CHASING = 1,
        ESCAPING = 2,
        IDLE = 3,
        WANDERING = 4,
        WORKING = 5, //Work if not finished instead of going idle

        //Special attack states
        SWOOP = 100, //Swoop attack
    }
    [SerializeField] NPCMovementState movementState = NPCMovementState.IDLE;

    MovementModel movementModel;
    StatusConditionModel statusConditionModel;
    NPCAttackModel attackModel;
    SphereCollider visionCollider;
    CapsuleCollider massCollider;
    Transform target;
    [SerializeField] CharacterData characterData;

    Vector3 preSwoopPosition;
    Vector2 vectorOfAttack;
    Vector3 lastPositionOfTarget;
    [SerializeField] float swoopTimerMax;
    float swoopTimer;

    public float gizmoWidth;


    private void Awake()
    {
        movementModel = GetComponent<MovementModel>();
        statusConditionModel = GetComponent<StatusConditionModel>();
        attackModel = GetComponent<NPCAttackModel>();

        visionCollider = gameObject.AddComponent<SphereCollider>();
        visionCollider.radius = 6;
        visionCollider.isTrigger = true;

        massCollider = GetComponent<CapsuleCollider>();
        
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
            Attack();
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
        else if(movementState == NPCMovementState.SWOOP)
        {
            swoopTimer++;
            float percentage = swoopTimer / swoopTimerMax;
            float currentAngle = percentage * 180f * Mathf.Deg2Rad;
            Vector2 vectorBetween = (Vector2)preSwoopPosition - (Vector2)lastPositionOfTarget;
            float angleToTarget = -Mathf.Atan2(vectorBetween.x, vectorBetween.y) * Mathf.Rad2Deg + 90;

            float horizontalRadius = vectorBetween.magnitude;
            float verticalRadius = lastPositionOfTarget.z - preSwoopPosition.z;
            float radius = (horizontalRadius * verticalRadius) / Mathf.Sqrt(
                Mathf.Pow(horizontalRadius, 2) * Mathf.Pow(Mathf.Sin(currentAngle), 2) +
                Mathf.Pow(verticalRadius, 2) * Mathf.Pow(Mathf.Cos(currentAngle), 2)
                );

            float x = Mathf.Cos(currentAngle) * radius - radius;
            float z = Mathf.Sin(currentAngle) * radius;
            Vector3 newPosition = new Vector3(x, 0, z) + preSwoopPosition;
            newPosition = Quaternion.Euler(0, 0, angleToTarget) * newPosition;
            transform.position = newPosition;

            if(swoopTimer >= swoopTimerMax)
            {
                movementState = NPCMovementState.NONE;
                massCollider.isTrigger = false;
                visionCollider.enabled = true;
            }
        }
    }
    public void SwapState(NPCMovementState newState)
    {
        movementState = newState;
        directionShiftTimer = 0;
        if(newState == NPCMovementState.SWOOP)
        {
            massCollider.isTrigger = true;
            visionCollider.enabled = false;
            preSwoopPosition = transform.position;
            vectorOfAttack = (target.position - transform.position).normalized;
            lastPositionOfTarget = target.position;
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(movementState == NPCMovementState.SWOOP)
            {
                if (other.GetComponent<HealthModel>())
                {
                    target.GetComponent<HealthModel>().TakeDamage(3);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        if(!target && other.CompareTag("Player") && (int)movementState < 100)
        {
            movementState = NPCMovementState.CHASING;
            target = other.transform;
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if(other.CompareTag("Player") && movementState != NPCMovementState.SWOOP)
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
