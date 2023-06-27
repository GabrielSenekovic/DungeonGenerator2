using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.Networking.UnityWebRequest;

public class EntityStatistics : MonoBehaviour
{
    public enum Physiology
    {
        NONE = 0,
        FLESH = 1,
        WOODEN = 2,
        WATER = 3,
        METALLIC = 4,
        EARTHEN = 5,
        FIRE = 6,
        FAE = 7,
        ICE = 8,
        ROCKEN = 9
    }
    public struct ElementWeakness
    {
        public Element element;
        public float modifier;

        public Condition source;
        public Condition result;

        public ElementWeakness(Element element_in, Condition source_in, float modifier_in, Condition result = Condition.NONE)
        {
            element = element_in;
            modifier = modifier_in;
            source = source_in;
            this.result = result;
        }
    }
    public struct DamageWeakness
    {
        public DealDamage.DamageType damageType;
        public float modifier;

        public Condition source;
        public Condition result;
        public DamageWeakness(DealDamage.DamageType damageType_in, Condition source_in, float modifier_in, Condition result = Condition.NONE)
        {
            damageType = damageType_in;
            modifier = modifier_in;
            source = source_in;
            this.result = result;
        }
    }
    [System.Serializable]public class DamageOverTime
    {
        public Element element;
        public float damage;
        public int damageTimer;
        public int damageTimerMax;
        public Condition source;

        public DamageOverTime(Element element_in, Condition source_in, int damageTimerMax_in, float damage_in)
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
    public bool canInteract;
    public Physiology physiology;
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

    private void Awake()
    {
        SetPhysiology();
    }
    public void SetPhysiology()
    {
        switch (physiology)
        {
            case Physiology.FLESH:
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.CLEAVING, Condition.NONE, 4, Condition.Bleeding));
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.SLASHING, Condition.NONE, 1, Condition.Bleeding));
                elementWeaknesses.Add(new ElementWeakness(Element.ICE, Condition.NONE, 3, Condition.Frozen));
                break;
            case Physiology.WOODEN:
                elementWeaknesses.Add(new ElementWeakness(Element.FIRE, Condition.NONE, 3, Condition.Burning));
                elementWeaknesses.Add(new ElementWeakness(Element.ICE, Condition.NONE, 3, Condition.Frozen));
                elementWeaknesses.Add(new ElementWeakness(Element.AIR, Condition.NONE, 0.25f));
                break;
            case Physiology.WATER:
                elementWeaknesses.Add(new ElementWeakness(Element.LIGHTNING, Condition.NONE, 2, Condition.Jolted));
                elementWeaknesses.Add(new ElementWeakness(Element.ICE, Condition.NONE, 3, Condition.Frozen));
                elementWeaknesses.Add(new ElementWeakness(Element.AETHER, Condition.NONE, 1.2f, Condition.Frozen));
                break;
            case Physiology.FIRE:
                elementWeaknesses.Add(new ElementWeakness(Element.WATER, Condition.NONE, 2));
                elementWeaknesses.Add(new ElementWeakness(Element.AIR, Condition.NONE, 1.5f));
                break;
            case Physiology.METALLIC:
                elementWeaknesses.Add(new ElementWeakness(Element.FIRE, Condition.NONE, 2));
                break;
            case Physiology.EARTHEN:
                elementWeaknesses.Add(new ElementWeakness(Element.WATER, Condition.NONE, 1.2f));
                elementWeaknesses.Add(new ElementWeakness(Element.WOOD, Condition.NONE, 2));
                elementWeaknesses.Add(new ElementWeakness(Element.FIRE, Condition.NONE, 0));
                elementWeaknesses.Add(new ElementWeakness(Element.LIGHTNING, Condition.NONE, 0));
                break;
            case Physiology.FAE:
                elementWeaknesses.Add(new ElementWeakness(Element.METAL, Condition.NONE, 4));
                break;
            case Physiology.ICE:
                elementWeaknesses.Add(new ElementWeakness(Element.FIRE, Condition.NONE, 4));
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.BLUDGEONING, Condition.NONE, 4));
                break;
            case Physiology.ROCKEN:
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.PIERCING, Condition.NONE, 0));
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.SLASHING, Condition.NONE, 0));
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.CLEAVING, Condition.NONE, 0));
                damageWeaknesses.Add(new DamageWeakness(DealDamage.DamageType.BLUDGEONING, Condition.NONE, 4));
                elementWeaknesses.Add(new ElementWeakness(Element.ICE, Condition.NONE, 2));
                elementWeaknesses.Add(new ElementWeakness(Element.FIRE, Condition.NONE, 0));
                elementWeaknesses.Add(new ElementWeakness(Element.AIR, Condition.NONE, 0));
                elementWeaknesses.Add(new ElementWeakness(Element.LIGHTNING, Condition.NONE, 1.5f));
                elementWeaknesses.Add(new ElementWeakness(Element.AETHER, Condition.NONE, 0));
                elementWeaknesses.Add(new ElementWeakness(Element.WOOD, Condition.NONE, 4));
                elementWeaknesses.Add(new ElementWeakness(Element.WATER, Condition.NONE, 0));
                break;
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
        if(condition == Condition.InCombat)
        {
            canInteract = true;
        }
    }
}
