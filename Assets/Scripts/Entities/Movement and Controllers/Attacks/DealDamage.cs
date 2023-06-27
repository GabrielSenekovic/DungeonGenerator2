using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    public enum DamageType
    {
        NONE = 0,
        BLUDGEONING = 1,
        SLASHING = 2,
        PIERCING = 3,
        MAGIC = 4,
        CLEAVING = 5
    }
    [System.Serializable]public struct Damage
    {
        public DamageType type;
        public Element element;
        public int damage;
    }
    public List<Damage> damageToDeal;

    void OnTriggerEnter(Collider other) 
    {
        if(other.GetComponent<HealthModel>())
        {
            Hit(other.gameObject);
        }
        OnImpact();
    }
    void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.GetComponent<HealthModel>())
        {
            Hit(other.gameObject);
        }
        OnImpact();
    }
    protected virtual void Hit(GameObject target)
    {
        foreach(Damage damage in damageToDeal)
        {
            target.GetComponent<HealthModel>().TakeDamage(damage);
        }
    }
    protected virtual void OnImpact()
    {
    }
}
