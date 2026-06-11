using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 1;
    [SerializeField] bool isHoming = true;
    [SerializeField] GameObject hitEffect = null;
    [SerializeField] GameObject[] destroyOnHit = null;
    [SerializeField] float maxLifeTime = 10;
    [SerializeField] float lifeAfterImpact = 2;
    [SerializeField] UnityEvent onHit;


    Health target = null;
    Vector3 targetPoint;
    GameObject instigator = null;
    float damage = 0;

    //private void Start()
    //{
    //    RotateToward(GetAimLocation());
    //}

    void Update()
    {
        if (target != null && isHoming && !target.IsDead())
        {
            RotateToward(GetAimLocation());
        }
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void RotateToward(Vector3 aimLocation)
    {
        Vector2 direction = (aimLocation - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetTarget(Health target, GameObject instigator, float damage)
    {
        SetTarget(instigator, damage, target);
    }

    public void SetTarget(Vector3 targetPoint, GameObject instigator, float damage)
    {
        SetTarget(instigator, damage, null, targetPoint);
    }

    public void SetTarget(GameObject instigator, float damage, Health target = null, Vector3 targetPoint = default)
    {
        this.target = target;
        this.targetPoint = targetPoint;
        this.damage = damage;
        this.instigator = instigator;
        RotateToward(GetAimLocation());
        Destroy(gameObject, maxLifeTime);
    }

    private Vector3 GetAimLocation()
    {
        if (target == null) { return targetPoint; }

        CapsuleCollider2D targetCapsule = target.GetComponent<CapsuleCollider2D>(); // 2D collider
        if (targetCapsule == null) { return target.transform.position; }

        // Use the collider's size.y to approximate center height in 2D
        return target.transform.position + Vector3.up * targetCapsule.size.y / 2;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponent<Health>();
        if (target != null && health != target) return;
        if (health == null || health.IsDead()) return;
        if (other.gameObject == instigator) return;
        health.TakeDamage(instigator, damage);

        speed = 0;

        onHit.Invoke();

        if (hitEffect != null)
        {
            Instantiate(hitEffect, GetAimLocation(), transform.rotation);
        }

        foreach (GameObject toDestroy in destroyOnHit)
        {
            Destroy(toDestroy);
        }

        Destroy(gameObject, lifeAfterImpact);
    }
}
