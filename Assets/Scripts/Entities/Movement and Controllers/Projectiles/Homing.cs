using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homing : MonoBehaviour
{
    public enum HomingMode
    {
        NONE = 0,
        HOMING = 1,
        REPULSED = 2, //goes in the opposite direction of the target, may hit allies instead because of it, or bounce off walls in unexpected manners
        HOMING_REPULSED = 3 //goes after player and tries to stay at a distance from them
    }
    [SerializeField]HomingMode homingMode;
    public void CheckHomingMode()
    {
        switch(homingMode)
        {
            case HomingMode.HOMING:
                break;
            case HomingMode.REPULSED:
                break;
            case HomingMode.HOMING_REPULSED:
                break;
            default: 
                break;
        }
    }
}
