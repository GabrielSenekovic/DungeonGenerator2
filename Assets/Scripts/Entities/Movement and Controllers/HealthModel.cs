using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthModel : MonoBehaviour
{
    public delegate void OnDeath(GameObject entity);
    public OnDeath onDeath;

    public float currentHealth;
    public int maxHealth;

    public string deathSound;

    EntityStatistics statistics;
    DropItems dropItems;
    private void Start() 
    {
        currentHealth = maxHealth;
        statistics = GetComponent<EntityStatistics>();
        TryGetComponent(out dropItems);
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
            Die();
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
            currentHealth = 0;
            Die();
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

    public void Die()
    {
        AudioManager.PlaySFX(deathSound);
        gameObject.SetActive(false);
        onDeath?.Invoke(gameObject);

        dropItems?.Drop(3, Vector3.zero);
    }

    public bool isDead()
    {
        return currentHealth <= 0;
    }
}
