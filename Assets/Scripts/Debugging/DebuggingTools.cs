using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggingTools : MonoBehaviour
{
    public bool checkForBrokenSeeds_in;
    public static bool checkForBrokenSeeds;

    private void Awake() 
    {
        checkForBrokenSeeds = checkForBrokenSeeds_in;
    }
}
