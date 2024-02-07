using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    [SerializeField] GameObject enemyBluePrint;
    [SerializeField] AttackIdentifier swoop;
    [SerializeField] WeaponAttackIdentifier maceAttack;
    [SerializeField] RuntimeAnimatorController animationController;
    [SerializeField] GameObject maceWeapon;
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

        NPCAttackModel attackModel = newEntity.GetComponent<NPCAttackModel>();
        attackModel.AddAttack(swoop);

        return newEntity;
    }
    public GameObject SpawnMeeleeEntity()
    {
        GameObject newEntity = Instantiate(enemyBluePrint, new Vector3(0, 0, -1), Quaternion.identity, transform);
        EquipmentModel equipmentModel = newEntity.AddComponent<EquipmentModel>();

        NPCAttackModel attackModel = newEntity.GetComponent<NPCAttackModel>();
        attackModel.AddAttack(maceAttack);

        GameObject weaponTransform = new GameObject("Weapon Transform");
        weaponTransform.transform.parent = newEntity.transform;
        Animator animator = weaponTransform.AddComponent<Animator>();
        animator.runtimeAnimatorController = animationController;
        WeaponAnimator weaponAnimator = weaponTransform.AddComponent<WeaponAnimator>();
        weaponAnimator.Initialize(equipmentModel);
        maceAttack.AddAnimator(animator);

        GameObject mace = Instantiate(maceWeapon, maceWeapon.transform.position, maceWeapon.transform.rotation, weaponTransform.transform);
        mace.transform.localScale = maceWeapon.transform.localScale;
        equipmentModel.leftHandItem = mace.GetComponent<Equipment>();

        return newEntity;
    }
}
