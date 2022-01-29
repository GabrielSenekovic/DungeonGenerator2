using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public float gravitySpeed;
    public float gravityRadius;

    public void OnAttackStay(GameObject vic, ref List<ProjectileController.targetData> targets)
    {
        bool isNew = true;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].target != vic)
            {
                isNew = true;
            }
            else
            {
                isNew = false;
                break;
            }
        }
        if (isNew) //Check if this target is a new target
        {
            Vector2 vectorToTarget = (Vector2)(transform.position - vic.transform.position);
            float distanceModifier = vectorToTarget.magnitude <= gravityRadius ? (gravityRadius - vectorToTarget.magnitude) / gravityRadius : 0;
            Vector2 pushV2 = vectorToTarget.normalized * gravitySpeed * distanceModifier;
            //! Add gravity to target

            targets.Add(new ProjectileController.targetData(vic, 0));
        }
        else
        {
            if(gravitySpeed != 0){GravityEffect(vic, ref targets);}
        }
    }
    public void GravityEffect(GameObject vic, ref List<ProjectileController.targetData> targets)
    {
        if (targets.Count > 0)
        {
            ProjectileController.targetData targetVic = targets[0]; //If the list isnt empty, take the first element
            foreach (ProjectileController.targetData t in targets)
            {
                if (t.target == vic) //Check if you already have the new element
                {
                    targetVic = t; //If you do, then your target is it
                    break;
                }
            }
            if (targetVic.target != null)
            {
                Vector2 vectorToTarget = (Vector2)(transform.position - vic.transform.position);
                float distanceModifier = vectorToTarget.magnitude <= gravityRadius ? (gravityRadius - vectorToTarget.magnitude) / gravityRadius : 0;
                Vector2 value = vectorToTarget.normalized * gravitySpeed * distanceModifier;
                //! Apply gravity to all the targets

                targetVic.target.GetComponent<Rigidbody>().AddForce(value, ForceMode.Impulse);
            }
        }
    }
}
