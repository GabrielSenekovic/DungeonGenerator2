using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Gravity : MonoBehaviour
{
    public enum GravityBehavior
    {
        ATTRACT = 0,
        REPEL = 1,
        TRAP = 2
    }
    [SerializeField] float gravitySpeed;
    [SerializeField] float gravityRadius;
    [SerializeField] GravityBehavior gravityBehavior;
    public void OnAttackStay()
    {
        Rigidbody[] hits = Physics.OverlapSphere(transform.position, gravityRadius).Where(c => c.GetComponent<Rigidbody>()).Select(c => c.GetComponent<Rigidbody>()).ToArray();

        if(gravitySpeed != 0)
        {
            foreach (Rigidbody body in hits)
            {
                switch(gravityBehavior)
                {
                    case GravityBehavior.ATTRACT: Attract(body);
                        break;
                    case GravityBehavior.REPEL: Repel(body);
                        break;
                    case GravityBehavior.TRAP: Trap(body);
                        break;
                }
            }
        }
    }
    void Attract(Rigidbody body)
    {
        Vector2 vectorToTarget = (Vector2)(transform.position - body.transform.position);
        float distanceModifier = vectorToTarget.magnitude <= gravityRadius ? (gravityRadius - vectorToTarget.magnitude) / gravityRadius : 0;
        Vector2 speedOfGravity = vectorToTarget.normalized * gravitySpeed * distanceModifier;
        body.AddForce(speedOfGravity, ForceMode.Impulse);
    }
    void Repel(Rigidbody body)
    {
        Vector2 vectorToTarget = (Vector2)(body.transform.position - transform.position);
        float distanceModifier = vectorToTarget.magnitude <= gravityRadius ? (gravityRadius - vectorToTarget.magnitude) / gravityRadius : 0;
        Vector2 speedOfGravity = vectorToTarget.normalized * gravitySpeed * distanceModifier;
        body.AddForce(speedOfGravity, ForceMode.Impulse);
    }
    void Trap(Rigidbody body)
    {
        Vector2 vectorToTarget = (Vector2)(transform.position - body.transform.position); //pulling force
        //But only when trying to exit the radius
        float distanceModifier = vectorToTarget.magnitude >= gravityRadius - 0.5f ? 1 : 0;
        Vector2 speedOfGravity = vectorToTarget.normalized * gravitySpeed * distanceModifier;
        body.AddForce(speedOfGravity, ForceMode.Impulse);
    }
}
