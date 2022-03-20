using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    public static GameObject SpawnRandomEntity()
    {
        return new GameObject("New Entity");
    }
}
