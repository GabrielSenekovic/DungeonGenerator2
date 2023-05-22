using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.VFX;

public enum Condition
{
    Burning,
    Chilled,
    Frozen,
    Jolted,
    Rigid,
    Wet,

    Sitting = 5000,
    Cutscene = 5001
}
public class StatusConditionModel : MonoBehaviour
{
    [System.Serializable]public class StatusCondition
    {
        public Condition value;
        public float duration;

        public StatusCondition(Condition value_in)
        {
            value = value_in;
            duration = Mathf.Infinity;
        }
        public StatusCondition(Condition value_in, float duration_in)
        {
            value = value_in;
            duration = duration_in;
        }

        public void Decrement()
        {
            duration--;
        }
    }
    [System.Serializable]public struct StatusConditionHUD
    {
        [System.Serializable]public struct ConditionIcon
        {
            public Condition condition;
            public Image image;

            public ConditionIcon(Condition condition_in, Transform trans)
            {
                condition = condition_in;

                Sprite sprite = Resources.Load<Sprite>("Art/ConditionIcons/" + condition.ToString());

                GameObject temp = new GameObject(condition.ToString()); temp.transform.parent = trans;

                RectTransform rect = temp.AddComponent<RectTransform>();
                rect.localScale = new Vector3(1, 1, 1);
                image = temp.AddComponent<Image>();

                image.sprite = sprite;
            }
        }
        public Transform trans;
        public List<ConditionIcon> icons;

        public void AddCondition(Condition condition)
        {
            if(!icons.Any(c => c.condition == condition))
            {
                icons.Add(new ConditionIcon(condition, trans));
            }
        }
        public void RemoveCondition(Condition condition)
        {
            for(int i = 0; i < icons.Count; i++)
            {
                if(icons[i].condition == condition)
                {
                    ConditionIcon icon = icons[i];
                    icons.RemoveAt(i);
                    Destroy(icon.image.gameObject);
                }
            }    
        }
    }
    public StatusConditionHUD HUD;

    EntityStatistics statistics;

    public VisualEffect burning;

    public List<StatusCondition> conditions = new List<StatusCondition>();
    private void Start() 
    {
        statistics = GetComponent<EntityStatistics>();
    }

    private void FixedUpdate() 
    {
        for(int i = 0; i < conditions.Count; i++)
        {
            //Go through all currently had conditions and update the duration until it's over
            conditions[i].Decrement();
            if(conditions[i].duration <= 0)
            {
                HUD.RemoveCondition(conditions[i].value);
                RemoveStatisticsChanges(conditions[i].value);
                if(conditions[i].value == Condition.Burning)
                {
                    burning.gameObject.SetActive(false);
                }
                conditions.RemoveAt(i);
            }
        }
    }

    public bool IfHasCondition(Condition condition)
    {
        if(conditions.Any(c => c.value == condition))
        {
            return true;
        }
        return false;
    }

    public void AddCondition(StatusCondition condition)
    {
        conditions.Add(condition);
        if(((int)condition.value) < 5000)
        {
            HUD.AddCondition(condition.value);
        }

        switch(condition.value)
        {
            case Condition.Burning:
                statistics.damagesOverTime.Add(new EntityStatistics.DamageOverTime(Element.FIRE, condition.value, 50, 1));
                statistics.speedModifiers.Add(new EntityStatistics.SpeedModifier(condition.value, 3.0f));
                burning.gameObject.SetActive(true);
            break;
            case Condition.Chilled: 
                statistics.elementWeaknesses.Add(new EntityStatistics.ElementWeakness(Element.FIRE, condition.value, 0.5f));
                statistics.speedModifiers.Add(new EntityStatistics.SpeedModifier(condition.value, 0.75f));
            break;
            case Condition.Frozen: 
                statistics.elementWeaknesses.Add(new EntityStatistics.ElementWeakness(Element.FIRE, condition.value, 0));
                statistics.damageWeaknesses.Add(new EntityStatistics.DamageWeakness(DealDamage.DamageType.BLUDGEONING, condition.value, 2f));
                statistics.speedModifiers.Add(new EntityStatistics.SpeedModifier(condition.value, 0));
            break;
            case Condition.Jolted:
                statistics.moveTimerMax = UnityEngine.Random.Range(20, 40);
            break;
            case Condition.Wet: 
                statistics.elementWeaknesses.Add(new EntityStatistics.ElementWeakness(Element.FIRE, condition.value, 0.5f));
            break;
            case Condition.Sitting:
                statistics.speedModifiers.Add(new EntityStatistics.SpeedModifier(condition.value, 0));
            break;
            case Condition.Cutscene:
                statistics.speedModifiers.Add(new EntityStatistics.SpeedModifier(condition.value, 0));
            break;
        }
    }
    public void RemoveCondition(Condition condition)
    {
        for(int i = 0; i < conditions.Count; i++)
        {
            if(conditions[i].value == condition)
            {
                HUD.RemoveCondition(conditions[i].value);
                RemoveStatisticsChanges(conditions[i].value);
                conditions.RemoveAt(i);
            }
        }
    }
    public void RemoveStatisticsChanges(Condition condition)
    {
        statistics.RemoveStatisticsChanges(condition);
    }

    public void ReactToDamage(ref DealDamage.Damage damage)
    {
        if (damage.element == Element.FIRE && IfHasCondition(Condition.Frozen))
        {
            if (Random.Range(0, 5) == 0) //1 in 5 chance for fire to melt ice
            {
                RemoveCondition(Condition.Frozen);
                AddCondition(new StatusCondition(Condition.Chilled));
            }
        }
        if (damage.element == Element.WATER && IfHasCondition(Condition.Chilled))
        {
            damage.element = Element.ICE;
            if (Random.Range(0, 3) > 0) //High likelihood of being frozen if being chilled. 2 in 3
            {
                RemoveCondition(Condition.Chilled);
                AddCondition(new StatusCondition(Condition.Frozen));
            }
        }
        if (damage.element == Element.FIRE)
        {
            //Set on fire based on current flammability
            //If wet, flammability is lower
            if (Random.Range(0, 3) > 0) //High likelihood of being frozen if being chilled. 2 in 3
            {
                AddCondition(new StatusCondition(Condition.Burning));
            }
        }
        if (damage.element == Element.LIGHTNING)
        {
            AddCondition(new StatusCondition(Condition.Jolted));
        }
    }
}
