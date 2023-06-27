using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour
{
    public float blastRadius;
    public float explosionPower;

    [SerializeField] List<StatusConditionModel.StatusCondition> conditionsInflicted = new List<StatusConditionModel.StatusCondition>();
    public void OnExplode(Collider[] hits)
    {//? If this projectile is supposed to explode, explode
        foreach(Collider hit in hits)
        {
            if(hit.TryGetComponent(out Rigidbody body))
            {
                Vector2 vectorToTarget = (Vector2)(transform.position - hit.transform.position);
                float distanceModifier = vectorToTarget.magnitude <= blastRadius ? (blastRadius - vectorToTarget.magnitude) / blastRadius : 0;
                Vector2 value = vectorToTarget.normalized * explosionPower * distanceModifier;
                body.AddForce(-value, ForceMode.Impulse);
                Debug.Log("Exploding with value: " + value);
                //t.target.GetComponent<EntityMovementModel>().push[t.pushIndex] = -value;
                if(hit.TryGetComponent(out StatusConditionModel statusConditionModel))
                {
                    for(int i = 0; i < conditionsInflicted.Count; i++)
                    {
                        statusConditionModel.AddCondition(conditionsInflicted[i]);
                    }
                }
            }
        }
    }
}
