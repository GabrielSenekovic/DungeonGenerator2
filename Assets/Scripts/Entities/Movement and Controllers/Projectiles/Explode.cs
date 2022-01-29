using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour
{
    public float blastRadius;
    public float explosionPower;
    public void OnExplode(ref List<ProjectileController.targetData> targets)
    {//? If this projectile is supposed to explode, explode
        foreach(ProjectileController.targetData t in targets)
        {
            if(t.target != null && t.target.GetComponent<Rigidbody>())
            {
                Vector2 vectorToTarget = (Vector2)(transform.position - t.target.transform.position);
                float distanceModifier = vectorToTarget.magnitude <= blastRadius ? (blastRadius - vectorToTarget.magnitude) / blastRadius : 0;
                Vector2 value = vectorToTarget.normalized * explosionPower * distanceModifier;
                t.target.GetComponent<Rigidbody>().AddForce(-value, ForceMode.Impulse);
                Debug.Log("Exploding with value: " + value);
                //t.target.GetComponent<EntityMovementModel>().push[t.pushIndex] = -value;
            }
        }
    }
}
