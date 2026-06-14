using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public enum BossPhase
{
    Phase1,
    Phase2
}

public class Boss : MonoBehaviour, IAttacker
{
    [Header("Phase Settings"), Space]
    [SerializeField] float phase2HealthThreshold = 0.5f; // Triggers at 50% health

    [Header("Phase 1 Attack Damages"), Space]
    [SerializeField] float biteDamage = 10f;          // 10% of player health
    [SerializeField] float venomDamage = 5f;          // 5% over time
    [SerializeField] float slitherDamage = 12f;       // 12% of player health

    [Header("Phase 1 Attack Cooldowns"), Space]
    [SerializeField] float biteCooldown = 2f;
    [SerializeField] float venomCooldown = 4f;
    [SerializeField] float slitherCooldown = 6f;

    [Header("Phase 2 Attack Damages"), Space]
    [SerializeField] float multiBiteDamage = 8f;      // Per bite
    [SerializeField] int multiBiteCount = 3;
    [SerializeField] float multiBiteDelay = 0.3f;
    [SerializeField] float tripleVenomDamage = 5f;
    [SerializeField] float bodySpinDamage = 15f;      // 15% of player health
    [SerializeField] int slitherRepeatCount = 3;

    [Header("Phase 2 Attack Cooldowns"), Space]
    [SerializeField] float phase2BiteCooldown = 1f;       // Faster
    [SerializeField] float phase2VenomCooldown = 2.5f;    // Faster
    [SerializeField] float phase2SlitherCooldown = 4f;    // Faster
    [SerializeField] float bodySpinCooldown = 8f;

    [Header("Attack Range Settings"), Space]
    [SerializeField] Tilemap tilemap;
    [SerializeField] float venomProjectileSpeed = 5f;

    [Header("Attack Range Settings (in cells)"), Space]
    [SerializeField] int biteRangeCells = 2;
    [SerializeField] int tailSwipeRangeCells = 5;


    [Header("Movement Settings"), Space]
    [SerializeField] float lungeMoveSpeed = 12f;      // How fast the boss lunges forward
    [SerializeField] float returnMoveSpeed = 5f;      // How fast it returns home (slower = snakier)
    [SerializeField] float slitherMoveSpeed = 8f;     // Speed of tail swipe sweep
    [SerializeField] float arrivalThreshold = 0.1f;   // How close = "arrived"


    [Header("Events"), Space]
    public UnityEvent onPhase2Enter;
    public UnityEvent onBiteAttack;
    public UnityEvent onVenomAttack;
    public UnityEvent onSlitherAttack;
    public UnityEvent onBodySpinAttack;

    Health playerHealth;
    Health bossHealth;
    Rigidbody2D rb;
    Fighter fighter; // For ranged attacks
    BossPhase currentPhase = BossPhase.Phase1;
    bool isAttacking = false;

    // Home position � set once at Start, always return here after attacks
    Vector2 returnPosition;

    // Cooldown flags
    bool isBiteOnCooldown = false;
    bool isVenomOnCooldown = false;
    bool isSlitherOnCooldown = false;
    bool isBodySpinOnCooldown = false;
    public Health GetHealth()
    {
        if (TryGetComponent<Health>(out var health))
        {
            return health;
        }
        return null;
    }


    private void Awake()
    {
        bossHealth = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        fighter = GetComponent<Fighter>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();

        bossHealth.onDie.AddListener(OnBossDie);
    }

    private void Start()
    {
        returnPosition = transform.position;
        StartCoroutine(AttackDecisionLoop());
    }

    IEnumerator AttackDecisionLoop()
    {
        while (bossHealth != null && !bossHealth.IsDead())
        {
            yield return new WaitForSeconds(0.5f);

            if (playerHealth.IsDead()) yield break;


            if (currentPhase == BossPhase.Phase1)
            {
                int attackChoice = Random.Range(0, 3);
                switch (attackChoice)
                {
                    case 0: Bite(); break;
                    case 1: VenomSpit(); break;
                    case 2: Slither(); break;
                }
            }
            else
            {
                int attackChoice = Random.Range(0, 4);
                switch (attackChoice)
                {
                    case 0: Bite(); break;
                    case 1: VenomSpit(); break;
                    case 2: Slither(); break;
                    case 3: BodySpin(); break;
                }
            }
            yield return new WaitForSeconds(1f); // Decision every second
        }
    }

    // -------------------------
    // CORE MOVEMENT HELPERS
    // -------------------------

    // Drives the Rigidbody2D toward a world position.
    // Yields until arrived. Speed is tunable per attack.
    private IEnumerator MoveToPosition(Vector2 target, float speed)
    {
        while (Vector2.Distance(rb.position, target) > arrivalThreshold)
        {
            Vector2 dir = (target - rb.position).normalized;
            rb.linearVelocity = dir * speed;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        rb.position = target; // snap to avoid floating point drift
    }

    // Slides boss back to homePosition at returnMoveSpeed
    private IEnumerator ReturnHome()
    {
        yield return StartCoroutine(MoveToPosition(returnPosition, returnMoveSpeed));
        isAttacking = false;
    }



    // -------------------------
    // GRID HELPERS
    // -------------------------

    private int GetCellDistance()
    {
        if (tilemap == null || playerHealth.transform == null) return 999;

        Vector3Int bossCell = tilemap.WorldToCell(transform.position);
        Vector3Int playerCell = tilemap.WorldToCell(playerHealth.transform.position);

        // Chebyshev distance � accounts for both X and Y
        return Mathf.Max(
            Mathf.Abs(playerCell.x - bossCell.x),
            Mathf.Abs(playerCell.y - bossCell.y)
        );
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }

    private void Update()
    {
        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        if (currentPhase == BossPhase.Phase1 && bossHealth.GetFraction() <= phase2HealthThreshold)
        {
            EnterPhase2();
            AudioManager.instance.PhaseChange(1);
        }
    }

    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;
        onPhase2Enter.Invoke();
        Debug.Log("Boss entering Phase 2!");
    }

    // -------------------------
    // PHASE 1 ATTACKS
    // -------------------------

    public void Bite()
    {
        if (isBiteOnCooldown) return;

        if (currentPhase == BossPhase.Phase1)
        {
            Debug.Log("Boss: Bite!");
            onBiteAttack.Invoke();
            StartCoroutine(BiteRoutine(biteDamage));
            isBiteOnCooldown = true;
            StartCoroutine(CooldownRoutine(biteCooldown, () => isBiteOnCooldown = false));
        }
        else
        {
            Debug.Log("Boss: Multi Bite!");
            onBiteAttack.Invoke();
            StartCoroutine(MultiBiteRoutine());
            isBiteOnCooldown = true;
            StartCoroutine(CooldownRoutine(phase2BiteCooldown, () => isBiteOnCooldown = false));
        }
    }

    public void VenomSpit()
    {
        if (isVenomOnCooldown) return;

        if (currentPhase == BossPhase.Phase1)
        {
            Debug.Log("Boss: Venom Spit!");
            SingleVenomProjectile();
            onVenomAttack.Invoke();

            isVenomOnCooldown = true;
            StartCoroutine(CooldownRoutine(venomCooldown, () => isVenomOnCooldown = false));
        }
        else
        {
            Debug.Log("Boss: Triple Venom Shot!");
            StartCoroutine(TripleVenomRoutine());
            onVenomAttack.Invoke();

            isVenomOnCooldown = true;
            StartCoroutine(CooldownRoutine(phase2VenomCooldown, () => isVenomOnCooldown = false));
        }
    }

    public void Slither()
    {
        if (isSlitherOnCooldown) return;

        Debug.Log("Boss: Slither!");
        int count = currentPhase == BossPhase.Phase2 ? slitherRepeatCount : 1;
        StartCoroutine(SlitherRoutine(count));
        onSlitherAttack.Invoke();

        isSlitherOnCooldown = true;
        float cooldown = currentPhase == BossPhase.Phase2 ? phase2SlitherCooldown : slitherCooldown;
        StartCoroutine(CooldownRoutine(cooldown, () => isSlitherOnCooldown = false));
    }

    public void BodySpin()
    {
        if (isBodySpinOnCooldown || currentPhase != BossPhase.Phase2) return;

        Debug.Log("Boss: Body Spin!");
        playerHealth.TakeDamage(gameObject, bodySpinDamage);
        onBodySpinAttack.Invoke();

        isBodySpinOnCooldown = true;
        StartCoroutine(CooldownRoutine(bodySpinCooldown, () => isBodySpinOnCooldown = false));
    }

    // -------------------------
    // COROUTINES
    // -------------------------

    // BITE � lunge toward player, deal damage on contact, return home
    private IEnumerator BiteRoutine(float damage)
    {
        isAttacking = true;

        // Snapshot player position at moment of attack
        Vector2 targetPos = playerHealth.transform.position;

        // --- LUNGE FORWARD ---
        yield return StartCoroutine(MoveToPosition(targetPos, lungeMoveSpeed));

        // Deal damage if still close enough
        //if (GetCellDistance() <= biteRangeCells + 1)
        //    playerHealth.TakeDamage(gameObject, damage);

        // Brief pause at the player � feels more impactful
        yield return new WaitForSeconds(0.1f);

        // --- RETURN HOME ---
        yield return StartCoroutine(ReturnHome());
        // ReturnHome sets isAttacking = false when done
    }


    // Multi Bite � rapid succession bites
    private IEnumerator MultiBiteRoutine()
    {
        isAttacking = true;
        Vector2 targetPos = playerHealth.transform.position;
        yield return StartCoroutine(MoveToPosition(targetPos, lungeMoveSpeed));

        for (int i = 0; i < multiBiteCount; i++)
        {
            if (playerHealth == null || playerHealth.IsDead()) yield break;
            //playerHealth.TakeDamage(gameObject, multiBiteDamage);
            Debug.Log($"Multi Bite {i + 1}/{multiBiteCount}");
            yield return new WaitForSeconds(multiBiteDelay);
        }
        // --- RETURN HOME ---
        yield return StartCoroutine(ReturnHome());
    }

    // Slither � repeats in phase 2
    private IEnumerator SlitherRoutine(int repeatCount)
    {
        isAttacking = true;
        for (int i = 0; i < repeatCount; i++)
        {
            // Sweep toward the player's X position but stay at home Y
            // This keeps the boss grounded while sweeping like a tail swipe
            Vector2 sweepTarget = new Vector2(playerHealth.transform.position.x, returnPosition.y);

            Debug.Log($"Slither pass {i + 1}/{repeatCount}");
            yield return StartCoroutine(MoveToPosition(sweepTarget, slitherMoveSpeed));

            // Deal damage if the player is at roughly the same height
            if (GetCellDistance() <= tailSwipeRangeCells)
            { playerHealth.TakeDamage(gameObject, slitherDamage); }

            // Short pause between passes in Phase 2
            if (i < repeatCount - 1) { yield return new WaitForSeconds(0.4f); }
        }

        // Return home after all passes
        yield return StartCoroutine(ReturnHome());

    }

    private void SingleVenomProjectile()
    {
        Vector2 dir = ((Vector2)playerHealth.transform.position - rb.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // RangedAttack(direction, angle, target) � target is the player IAttacker
        IAttacker playerAttacker = playerHealth.transform.GetComponent<IAttacker>();
        if (playerAttacker != null)
        {
            fighter.RangedAttackHoming(dir, angle, playerAttacker);
        }
        else
        {
            fighter.RangedAttackManual(dir, angle);
        }// fallback if player has no IAttacker
    }

    // Triple Venom � 3 directions (left, center, right)
    private IEnumerator TripleVenomRoutine()
    {

        if (playerHealth == null || playerHealth.IsDead()) { yield break; }
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"Triple Venom shot "); 
            SingleVenomProjectile();
            yield return new WaitForSeconds(0.5f); // Short delay between shots
        }
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator CooldownRoutine(float cooldown, System.Action onComplete)
    {
        yield return new WaitForSeconds(cooldown);
        onComplete();
    }

    private void OnBossDie()
    {
        Debug.Log("Boss Defeated!");
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        isAttacking = false;
    }

    //    public Health GetHealth()
    //    {
    //        if (TryGetComponent<Health>(out var health))
    //        {
    //            return health;
    //        }
    //        return null;
    //     }
    //    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isAttacking)
        {
            playerHealth.TakeDamage(gameObject, biteDamage);
        }
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (tilemap == null) return;

        Vector3Int bossCell = tilemap.WorldToCell(transform.position);
        DrawCellRange(bossCell, biteRangeCells, Color.red);
        DrawCellRange(bossCell, tailSwipeRangeCells, Color.yellow);

        // Show home position as a green cross
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(returnPosition == Vector2.zero ? (Vector2)transform.position : returnPosition, 0.2f);
    }

    private void DrawCellRange(Vector3Int centerCell, int radius, Color color)
    {
        Gizmos.color = color;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3 worldPos = tilemap.CellToWorld(
                    new Vector3Int(centerCell.x + x, centerCell.y + y, 0)
                );
                worldPos += tilemap.cellSize * 0.5f;
                Gizmos.DrawWireCube(worldPos, tilemap.cellSize * 0.9f);
            }
        }
    }

    private void OnGUI()
    {
        if (tilemap == null || playerHealth == null) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        int dist = GetCellDistance();
        string phase = currentPhase.ToString();
        string nextAtk = dist <= biteRangeCells ? "BITE"
                       : dist <= tailSwipeRangeCells ? "TAIL SWIPE"
                       : "VENOM SPIT";

        GUI.Label(new Rect(10, 10, 500, 30), $"Phase: {phase}", style);
        GUI.Label(new Rect(10, 35, 500, 30), $"Cell Distance: {dist}", style);
        GUI.Label(new Rect(10, 60, 500, 30), $"Next Attack: {nextAtk}", style);
        GUI.Label(new Rect(10, 85, 500, 30), $"Is Attacking: {isAttacking}", style);
        GUI.Label(new Rect(10, 110, 500, 30),
            $"Cooldowns � Bite:{isBiteOnCooldown} Venom:{isVenomOnCooldown} Slither:{isSlitherOnCooldown} Spin:{isBodySpinOnCooldown}",
            style);
    }
#endif

}
