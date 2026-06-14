using System.Collections;
using UnityEditor;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    [SerializeField] Transform attackPoint;
    [SerializeField,Range(0.1f,0.9f)] float meleeAttackRange = 0.5f;
    [SerializeField] float meleeAttackDamage = 10f;
    [SerializeField] float rangedAttackDamage = 5f;
    [SerializeField] float homingAttackDamage = 7f;
    [SerializeField] float rangedAttackCooldown = 0.5f; 
    [SerializeField] float homingAttackCooldown = 2f;
    [SerializeField] float meleeAttackCooldown = 0.3f;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] Projectile homingProjectilePrefab;
    [SerializeField] LayerMask enemyLayerMask;

    bool isMeleeOnCooldown = false;
    bool isRangedOnCooldown = false;
    bool isHomingOnCooldown = false;

   

    public void MeleeAttack()
    {
        if (isMeleeOnCooldown) { return; }
        Collider2D overlapedCollider = Physics2D.OverlapCircle(attackPoint.position, meleeAttackRange, enemyLayerMask);
        
        //DrawWireCapsule(transform.position, transform.position, 1f);

        if (overlapedCollider != null)
        {
            Debug.Log("Hit " + overlapedCollider.name);
            if (overlapedCollider.TryGetComponent(out Boss boss))
            {
                boss.GetComponent<Health>().TakeDamage(gameObject,meleeAttackDamage);
            }
        }
        StartCoroutine(CooldownRoutine(meleeAttackCooldown, () => isMeleeOnCooldown = false));
        isMeleeOnCooldown = true;
    }
    public void RangedAttackManual(Vector2 direction, float angle)
    {
        if (isRangedOnCooldown) { return; }
        Debug.Log("Ranged Attack");
        // Under Certain condition the launch projectile method should be called here
        Projectile projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.Euler(0, 0, angle));

        // Project far along the direction to get an actual world point
        Vector3 targetPoint = attackPoint.position + (Vector3)direction * 100f;

        projectile.SetTarget(gameObject, rangedAttackDamage, null, targetPoint);

        StartCoroutine(CooldownRoutine(rangedAttackCooldown, () => isRangedOnCooldown = false));
        isRangedOnCooldown = true;

    }
    public void RangedAttack(Vector2 direction, float angle, IAttacker target)
    {
        if (isRangedOnCooldown) { return; }
        Debug.Log("Ranged Attack");
        // Under Certain condition the launch projectile method should be called here
        Projectile projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.Euler(0, 0, angle));
        Health health = target.GetHealth();
        Vector3 enemyPosition = health != null ? health.transform.position : (Vector3)(direction * 100f);

        projectile.SetTarget(enemyPosition, gameObject, rangedAttackDamage); // Use point overload

        StartCoroutine(CooldownRoutine(rangedAttackCooldown, () => isRangedOnCooldown = false));
        isRangedOnCooldown = true;  

    }
    public void RangedAttackHoming(Vector2 direction, float angle, IAttacker target)
    {
        if (isHomingOnCooldown) { return; }
        Debug.Log("Ranged Attack");
        // Under Certain condition the launch projectile method should be called here
        Projectile projectile = Instantiate(homingProjectilePrefab, attackPoint.position, Quaternion.Euler(0, 0, angle));
        Health health = target.GetHealth();
        Vector3 enemyPosition = health != null ? health.transform.position : (Vector3)(direction * 100f);

        projectile.SetTarget(enemyPosition, gameObject, homingAttackDamage); // Use point overload

        StartCoroutine(CooldownRoutine(homingAttackCooldown, () => isHomingOnCooldown = false));
        isHomingOnCooldown = true;

    }
    // Single reusable coroutine for all cooldowns
    private IEnumerator CooldownRoutine(float cooldown, System.Action onComplete)
    {
        yield return new WaitForSeconds(cooldown);
        onComplete();
    }

    public Transform GetAttackPoint()
    {
        return attackPoint;
    }
    private void OnDrawGizmos()
    {
        if (attackPoint is not null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, meleeAttackRange);
        }
    }
}
