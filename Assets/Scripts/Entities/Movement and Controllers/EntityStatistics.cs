using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EntityStatistics : MonoBehaviour
{
    public struct ElementWeakness
    {
        public DealDamage.Element element;
        public float modifier;

        public Condition source;

        public ElementWeakness(DealDamage.Element element_in, Condition source_in, float modifier_in)
        {
            element = element_in;
            modifier = modifier_in;
            source = source_in;
        }
    }
    public struct DamageWeakness
    {
        public DealDamage.DamageType damageType;
        public float modifier;

        public Condition source;
        public DamageWeakness(DealDamage.DamageType damageType_in, Condition source_in, float modifier_in)
        {
            damageType = damageType_in;
            modifier = modifier_in;
            source = source_in;
        }
    }
    [System.Serializable]public class DamageOverTime
    {
        public DealDamage.Element element;
        public float damage;
        public int damageTimer;
        public int damageTimerMax;
        public Condition source;

        public DamageOverTime(DealDamage.Element element_in, Condition source_in, int damageTimerMax_in, float damage_in)
        {
            element = element_in;
            source = source_in;
            damageTimerMax = damageTimerMax_in;
            damage = damage_in;
            damageTimer = 0;
        }
    }
    public List<ElementWeakness> elementWeaknesses = new List<ElementWeakness>();
    public List<DamageWeakness> damageWeaknesses = new List<DamageWeakness>();
    public List<DamageOverTime> damagesOverTime = new List<DamageOverTime>();

    public float baseSpeed;
    public int moveTimerMax;
    public struct SpeedModifier
    {
        public Condition source;
        public float value;

        public SpeedModifier(Condition source_in, float value_in)
        {
            source = source_in;
            value = value_in;
        }
    }

    public List<SpeedModifier> speedModifiers = new List<SpeedModifier>();

    public float Speed
    {
        get
        {
            return speedModifiers.Count > 0 ? baseSpeed * speedModifiers.Sum(s => s.value) : baseSpeed;
        }
    }

    public int GetDamageOverTime()
    {
        float damage = 0;
        for(int i = 0; i < damagesOverTime.Count; i++)
        {
            damagesOverTime[i].damageTimer++;
            if(damagesOverTime[i].damageTimer >= damagesOverTime[i].damageTimerMax)
            {
                damage += damagesOverTime[i].damage;
                damagesOverTime[i].damageTimer = 0;
            }
        }
        return Mathf.RoundToInt(damage);
    }

    public void AdjustDamage(ref DealDamage.Damage damage)
    {
        float totalModifier = 0;
        for(int i = 0; i < elementWeaknesses.Count; i++)
        {
            if(damage.element == elementWeaknesses[i].element)
            {
                totalModifier += elementWeaknesses[i].modifier;
            }
        }
        for(int i = 0; i < damageWeaknesses.Count; i++)
        {
            if(damage.type == damageWeaknesses[i].damageType)
            {
                totalModifier += damageWeaknesses[i].modifier;
            }
        }
        damage.damage = (int)((float)damage.damage * totalModifier);
    }
    public void RemoveStatisticsChanges(Condition condition)
    {
        for(int i = 0; i < elementWeaknesses.Count; i++)
        {
            if(elementWeaknesses[i].source == condition)
            {
                elementWeaknesses.RemoveAt(i); i--;
            }
        }
        for(int i = 0; i < damageWeaknesses.Count; i++)
        {
            if(damageWeaknesses[i].source == condition)
            {
                damageWeaknesses.RemoveAt(i); i--;
            }
        }
        for(int i = 0; i < speedModifiers.Count; i++)
        {
            if(speedModifiers[i].source == condition)
            {
                speedModifiers.RemoveAt(i); i--;
            }
        }
        for(int i = 0; i < damagesOverTime.Count; i++)
        {
            if(damagesOverTime[i].source == condition)
            {
                damagesOverTime.RemoveAt(i); i--;
            }
        }
        if(condition == Condition.Jolted)
        {
            moveTimerMax = 1;
        }
    }
}
