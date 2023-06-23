using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    [SerializeField] GameObject enemyBluePrint;
    public GameObject SpawnRandomEntity()
    {
        GameObject newEntity = Instantiate(enemyBluePrint, Vector2.zero, Quaternion.identity, transform);
        return newEntity;
    }
    public GameObject SpawnFlyingEntity()
    {
        GameObject newEntity = Instantiate(enemyBluePrint, Vector2.zero, Quaternion.identity, transform);
        Rigidbody body = newEntity.GetComponent<Rigidbody>();
        body.useGravity = false;
        newEntity.transform.position += new Vector3(0, 0, -2);
        return newEntity;
    }
    public GameObject SpawnMeeleeEntity()
    {
        GameObject newEntity = Instantiate(enemyBluePrint, Vector2.zero, Quaternion.identity, transform);
        NPCAttackModel attackModel = newEntity.GetComponent<NPCAttackModel>();
        //add a meelee attack to the attackmodel
        return newEntity;
    }
}
