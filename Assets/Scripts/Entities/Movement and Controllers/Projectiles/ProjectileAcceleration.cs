using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAcceleration : MonoBehaviour
{
    public enum ProjectileAccelerationMode
    {
        NONE = 0,
        ACCELERATE = 1,
        DEACCELERATE = 2
    }
    [SerializeField]ProjectileAccelerationMode accelerationMode;
    public void CheckAccelerationMode()
    {
        switch(accelerationMode)
        {
            case ProjectileAccelerationMode.ACCELERATE:
                //Speed up projectile overtime
                break;
            case ProjectileAccelerationMode.DEACCELERATE:
                //Slow down projectile overtime
                break;
            default:
                break;
        }
    }
}
