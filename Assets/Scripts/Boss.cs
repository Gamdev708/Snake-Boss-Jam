using UnityEngine;
using UnityEngine.Events;

public class Boss : MonoBehaviour, IAttacker
{
    public Health GetHealth()
    {
        if(TryGetComponent<Health>(out var health))
        {
            return health;
        }
        return null;
    }

//public class Boss : MonoBehaviour, IAttacker
//{
//    [Header("Phase Settings"), Space]
//    [SerializeField] float phase2HealthThreshold = 0.5f; // Triggers at 50% health

//    [Header("Phase 1 Attack Damages"), Space]
//    [SerializeField] float biteDamage = 10f;          // 10% of player health
//    [SerializeField] float venomDamage = 5f;          // 5% over time
//    [SerializeField] float venomTickInterval = 1f;
//    [SerializeField] int venomTickCount = 5;
//    [SerializeField] float slitherDamage = 12f;       // 12% of player health

//    [Header("Phase 2 Attack Damages"), Space]
//    [SerializeField] float multiBiteDamage = 8f;      // Per bite
//    [SerializeField] int multiBiteCount = 3;
//    [SerializeField] float multiBiteDelay = 0.3f;
//    [SerializeField] float tripleVenomDamage = 5f;
//    [SerializeField] float bodySpinDamage = 15f;      // 15% of player health
//    [SerializeField] int slitherRepeatCount = 3;

//    [Header("Phase 1 Attack Cooldowns"), Space]
//    [SerializeField] float biteCooldown = 2f;
//    [SerializeField] float venomCooldown = 4f;
//    [SerializeField] float slitherCooldown = 6f;

//    [Header("Phase 2 Attack Cooldowns"), Space]
//    [SerializeField] float phase2BiteCooldown = 1f;       // Faster
//    [SerializeField] float phase2VenomCooldown = 2.5f;    // Faster
//    [SerializeField] float phase2SlitherCooldown = 4f;    // Faster
//    [SerializeField] float bodySpinCooldown = 8f;

//    [Header("Events"), Space]
//    public UnityEvent onPhase2Enter;
//    public UnityEvent onBiteAttack;
//    public UnityEvent onVenomAttack;
//    public UnityEvent onSlitherAttack;
//    public UnityEvent onBodySpinAttack;

//    Health playerHealth;
//    Health bossHealth;
//    bool isPhase2 = false;

//    // Cooldown flags
//    bool isBiteOnCooldown = false;
//    bool isVenomOnCooldown = false;
//    bool isSlitherOnCooldown = false;
//    bool isBodySpinOnCooldown = false;

//    private void Start()
//    {
//        bossHealth = GetComponent<Health>();
//        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();

//        bossHealth.onDie.AddListener(OnBossDie);
//    }

//    private void Update()
//    {
//        CheckPhaseTransition();
//    }

//    private void CheckPhaseTransition()
//    {
//        if (!isPhase2 && bossHealth.GetFraction() <= phase2HealthThreshold)
//        {
//            EnterPhase2();
//        }
//    }

//    private void EnterPhase2()
//    {
//        isPhase2 = true;
//        onPhase2Enter.Invoke();
//        Debug.Log("Boss entering Phase 2!");
//    }

//    // -------------------------
//    // PHASE 1 ATTACKS
//    // -------------------------

//    public void Bite()
//    {
//        if (isBiteOnCooldown) return;

//        Debug.Log("Boss: Bite!");
//        playerHealth.TakeDamage(gameObject, biteDamage);
//        onBiteAttack.Invoke();

//        isBiteOnCooldown = true;
//        float cooldown = isPhase2 ? phase2BiteCooldown : biteCooldown;
//        StartCoroutine(CooldownRoutine(cooldown, () => isBiteOnCooldown = false));
//    }

//    public void VenomSpit()
//    {
//        if (isVenomOnCooldown) return;

//        Debug.Log("Boss: Venom Spit!");
//        StartCoroutine(VenomDotRoutine(playerHealth, venomDamage, venomTickInterval, venomTickCount));
//        onVenomAttack.Invoke();

//        isVenomOnCooldown = true;
//        float cooldown = isPhase2 ? phase2VenomCooldown : venomCooldown;
//        StartCoroutine(CooldownRoutine(cooldown, () => isVenomOnCooldown = false));
//    }

//    public void Slither()
//    {
//        if (isSlitherOnCooldown) return;

//        Debug.Log("Boss: Slither!");
//        int count = isPhase2 ? slitherRepeatCount : 1;
//        StartCoroutine(SlitherRoutine(count));
//        onSlitherAttack.Invoke();

//        isSlitherOnCooldown = true;
//        float cooldown = isPhase2 ? phase2SlitherCooldown : slitherCooldown;
//        StartCoroutine(CooldownRoutine(cooldown, () => isSlitherOnCooldown = false));
//    }

//    // -------------------------
//    // PHASE 2 ATTACKS
//    // -------------------------

//    public void MultiBite()
//    {
//        if (isBiteOnCooldown || !isPhase2) return;

//        Debug.Log("Boss: Multi Bite!");
//        StartCoroutine(MultiBiteRoutine());
//        onBiteAttack.Invoke();

//        isBiteOnCooldown = true;
//        StartCoroutine(CooldownRoutine(phase2BiteCooldown, () => isBiteOnCooldown = false));
//    }

//    public void TripleVenomShot()
//    {
//        if (isVenomOnCooldown || !isPhase2) return;

//        Debug.Log("Boss: Triple Venom Shot!");
//        StartCoroutine(TripleVenomRoutine());
//        onVenomAttack.Invoke();

//        isVenomOnCooldown = true;
//        StartCoroutine(CooldownRoutine(phase2VenomCooldown, () => isVenomOnCooldown = false));
//    }

//    public void BodySpin()
//    {
//        if (isBodySpinOnCooldown || !isPhase2) return;

//        Debug.Log("Boss: Body Spin!");
//        playerHealth.TakeDamage(gameObject, bodySpinDamage);
//        onBodySpinAttack.Invoke();

//        isBodySpinOnCooldown = true;
//        StartCoroutine(CooldownRoutine(bodySpinCooldown, () => isBodySpinOnCooldown = false));
//    }

//    // -------------------------
//    // COROUTINES
//    // -------------------------

//    // Venom damage over time
//    private IEnumerator VenomDotRoutine(Health target, float damagePerTick, float interval, int ticks)
//    {
//        for (int i = 0; i < ticks; i++)
//        {
//            if (target == null || target.IsDead()) yield break;
//            target.TakeDamage(gameObject, damagePerTick);
//            Debug.Log($"Venom tick {i + 1}/{ticks}");
//            yield return new WaitForSeconds(interval);
//        }
//    }

//    // Slither — repeats in phase 2
//    private IEnumerator SlitherRoutine(int repeatCount)
//    {
//        for (int i = 0; i < repeatCount; i++)
//        {
//            Debug.Log($"Slither pass {i + 1}/{repeatCount}");
//            playerHealth.TakeDamage(gameObject, slitherDamage);
//            yield return new WaitForSeconds(1.5f); // Delay between passes
//        }
//    }

//    // Multi Bite — rapid succession bites
//    private IEnumerator MultiBiteRoutine()
//    {
//        for (int i = 0; i < multiBiteCount; i++)
//        {
//            if (playerHealth == null || playerHealth.IsDead()) yield break;
//            playerHealth.TakeDamage(gameObject, multiBiteDamage);
//            Debug.Log($"Multi Bite {i + 1}/{multiBiteCount}");
//            yield return new WaitForSeconds(multiBiteDelay);
//        }
//    }

//    // Triple Venom — 3 directions (left, center, right)
//    private IEnumerator TripleVenomRoutine()
//    {
//        float[] angles = { -30f, 0f, 30f }; // Spread angles

//        foreach (float angle in angles)
//        {
//            if (playerHealth == null || playerHealth.IsDead()) yield break;

//            // Apply damage per shot (you can replace with projectile spawn here)
//            playerHealth.TakeDamage(gameObject, tripleVenomDamage);
//            Debug.Log($"Triple Venom shot at angle {angle}");
//            yield return new WaitForSeconds(0.15f);
//        }
//    }

//    private IEnumerator CooldownRoutine(float cooldown, System.Action onComplete)
//    {
//        yield return new WaitForSeconds(cooldown);
//        onComplete();
//    }

//    private void OnBossDie()
//    {
//        Debug.Log("Boss Defeated!");
//        StopAllCoroutines();
//    }

//    public Health GetHealth()
//    {
//        if (TryGetComponent<Health>(out var health))
//        {
//            return health;
//        }
//        return null;
//    }

}
