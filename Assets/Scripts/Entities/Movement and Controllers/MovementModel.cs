using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

public class MovementModel : MonoBehaviour
{
    Vector3 movementDirection;
    Vector2 facingDirection;
    bool canMove = true;

    public Vector2 orbitPoint; //Anything that can move could orbit around something at some point
    public float orbitSpeed;

    Animator anim;

    Rigidbody body;

    EntityStatistics statistics;

    public int moveTimer = 0;
    public int moveTimerMax = 1;
    void Awake()
    {
        movementDirection = Vector2.zero; facingDirection = new Vector2(0, -1);
        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Rigidbody>();
        statistics = GetComponent<EntityStatistics>();
    }
    private void Update() 
    {
        Vector2 facingDirection = GetRelativeFacingDirection();
        if(Mathf.Abs(facingDirection.x) > Mathf.Abs(facingDirection.y))
        {
            facingDirection.y = 0;
        }
        else
        {
            facingDirection.x = 0;
        };
        facingDirection.x = Mathf.RoundToInt(facingDirection.x); facingDirection.y = Mathf.RoundToInt(facingDirection.y);
        if(anim != null)
        {
            anim.SetFloat("DirectionX", facingDirection.x);
            anim.SetFloat("DirectionY", facingDirection.y);
        }
    }
    public void FixedUpdate()
    {
        if(moveTimerMax > 1)
        {
            moveTimer++; 
            if(moveTimer <= moveTimerMax)
            {
                Move();
                if(GetComponent<StatusConditionModel>().IfHasCondition(Condition.Jolted))
                {
                    moveTimerMax = UnityEngine.Random.Range(20, 40);
                }
            }
            else if(moveTimer >= moveTimerMax + 10)
            {
                moveTimer%=(moveTimerMax + 10); //10 for the amount youll be frozen due to jolted
            }
        }
        else
        {
            Move();
        }
    }

    public void Move()
    {
        if(statistics != null)
        {
            body.MovePosition(transform.position + movementDirection.normalized * statistics.Speed * 0.1f); //0.1 is because on a scale of 1, the player moves 1 tile per Move()
        }
        else
        {
            body.MovePosition(transform.position + movementDirection.normalized); //0.1 is because on a scale of 1, the player moves 1 tile per Move()
        }
        movementDirection = Vector3.zero;
    }

    public Vector2 GetFacingDirection()
    {
        return facingDirection;
    }
    public void SetFacingDirection(Vector2 facingDirection_in)
    {
        facingDirection = facingDirection_in;
    }
    public void SetMovementDirection(Vector2 movementDirection_in)
    {
        movementDirection = movementDirection_in;
        facingDirection = movementDirection_in;
    }
    public void SetConstantVelocity(Vector3 velocity_in)
    {
        if(statistics != null)
        {
            body.velocity = velocity_in.normalized * statistics.Speed;
        }
        else
        {
            body.velocity = velocity_in.normalized;
        }
    }
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
    public Vector2 GetRelativeFacingDirection()
    {
        return Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * facingDirection;
    }
    public void OnDeath()
    {
        Destroy(gameObject);
    }
}
