using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthModel : MonoBehaviour
{
    public float currentHealth;
    public int maxHealth;

    public string deathSound;

    EntityStatistics statistics;
    private void Start() 
    {
        currentHealth = maxHealth;
        statistics = GetComponent<EntityStatistics>();
    }

    private void FixedUpdate() 
    {
        if(statistics)
        {
            TakeDamage(statistics.GetDamageOverTime());
        }
    }
    public void TakeDamage(int damage)
    {
        if(currentHealth - damage <= 0)
        {
            AudioManager.PlaySFX(deathSound);
            gameObject.SetActive(false);
            if(GetComponent<DropItems>())
            {
                GetComponent<DropItems>().Drop(3, Vector3.zero);
            }
        }
        else
        {
            currentHealth -= damage;
        }
    }
    public void TakeDamage(DealDamage.Damage damage)
    {
        if(statistics)
        {
            statistics.AdjustDamage(ref damage);
        }
        if(GetComponent<StatusConditionModel>())
        {
            GetComponent<StatusConditionModel>().ReactToDamage(ref damage);
        }

        if(currentHealth - damage.damage <= 0)
        {
            AudioManager.PlaySFX(deathSound);
            gameObject.SetActive(false);
            if(GetComponent<DropItems>())
            {
                GetComponent<DropItems>().Drop(3, Vector3.zero);
            }
        }
        else
        {
            currentHealth -= damage.damage;
        }
    }
    public float GetHealthPercentage()
    {
        return currentHealth / (float)maxHealth;
    }
    public float GetHealthPercentage(float modifier)
    {
        return (currentHealth + modifier) / (float)maxHealth;
    }

    public bool isDead()
    {
        return currentHealth <= 0;
    }
}
