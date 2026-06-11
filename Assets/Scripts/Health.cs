using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] TakeDamageEvent takeDamage;
    public UnityEvent onDie;

    [System.Serializable]
    public class TakeDamageEvent : UnityEvent<float>
    {
    }

    float health = 100f;
    float maxHealth = 100f;

    bool wasDeadLastFrame = false;
    public bool IsDead()
    {
        return health <= 0;
    }

    public void TakeDamage(GameObject instigator, float damage)
    {
        health = Mathf.Max(health - damage, 0);

        if (IsDead())
        {
            onDie.Invoke();
            //AwardExperience(instigator);
        }
        else
        {
            takeDamage.Invoke(damage);
        }
        UpdateState();
    }
    public void Heal(float healthToRestore)
    {
        health = Mathf.Min(health + healthToRestore, maxHealth);
        UpdateState();
    }
    public float GetHealthPoints()
    {
        return health;
    }
    public float GetPercentage()
    {
        return 100 * GetFraction();
    }

    public float GetFraction()
    {
        return health / maxHealth;
    }

    private void UpdateState()
    {
        if (TryGetComponent(out Animator animator))
        {
            if (!wasDeadLastFrame && IsDead())
            {
                animator.SetTrigger("die");
                //GetComponent<ActionScheduler>().CancelCurrentAction();
            }

            if (wasDeadLastFrame && !IsDead())
            {
                animator.Rebind();
            }

            wasDeadLastFrame = IsDead(); 
        }
    }

}
