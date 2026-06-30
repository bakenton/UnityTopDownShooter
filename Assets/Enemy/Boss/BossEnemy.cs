using UnityEngine;

public class BossEnemy : MonoBehaviour, IDamageable
{
    public enum BossState { Idle, Pursuit, Cooldown }

    [Header("Stats")]
    public int maxHealth = 200;
    public int currentHealth;
    public int handDamage = 20;

    [Header("Movement")]
    public float detectionRadius = 10f;
    public float pursuitSpeed = 6f;
    public float dashSpeed = 9f;
    public float dashCooldown = 1.5f;
    public float dashDuration = 0.2f;
    public float desiredAttackDistance = 2.2f;
    public float stopDistance = 0.3f;

    [Header("Attack")]
    public float swingInterval = 0.8f;
    public int maxPursuitSwings = 3;
    public float pursuitDuration = 5f;
    public float pursuitCooldown = 2f;
    public float attackHitDistance = 2.4f;
    public float attackApproachDistance = 3.2f;
    public float attackWindup = 0.15f;
    public float attackRecovery = 0.25f;
    public float retreatAfterHitTime = 0.25f;
    public float strafeSpeed = 3.5f;

    [Header("Animation")]
    public Animator animator;
    public string walkParameter = "IsWalk?";
    public string deadParameter = "IsDead?";
    public string attackParameter = "IsAttack?";

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip attackSound;
    public float soundVolume = 0.8f;

    [Header("Effects")]
    public GameObject deathEffectPrefab;

    private Rigidbody2D rb;
    private Transform player;
    private BossState currentState = BossState.Idle;
    private float pursuitTimer;
    private float swingTimer;
    private float dashTimer;
    private float dashDurationTimer;
    private float attackWindupTimer;
    private float cooldownTimer;
    private float retreatTimer;
    private int swingsUsed;
    private bool isPerformingAttack;
    private bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
        isDead = false;
        player = FindPlayer();
        UpdateAnimationState();
    }

    void Update()
    {
        if (isDead || currentHealth <= 0)
            return;

        if (player == null)
        {
            player = FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        bool canSeePlayer = distanceToPlayer <= detectionRadius;

        switch (currentState)
        {
            case BossState.Idle:
                if (canSeePlayer)
                    StartPursuit();
                break;

            case BossState.Pursuit:
                pursuitTimer += Time.deltaTime;
                swingTimer -= Time.deltaTime;
                dashTimer -= Time.deltaTime;

                if (dashDurationTimer > 0f)
                    dashDurationTimer -= Time.deltaTime;

                if (retreatTimer > 0f)
                {
                    retreatTimer -= Time.deltaTime;
                    MoveAwayFrom(player.position, pursuitSpeed * 0.8f);
                    break;
                }

                if (attackWindupTimer > 0f)
                {
                    attackWindupTimer -= Time.deltaTime;
                    if (attackWindupTimer <= 0f)
                        PerformAttack(distanceToPlayer);
                    break;
                }

                if (distanceToPlayer > detectionRadius * 1.2f)
                {
                    currentState = BossState.Idle;
                    break;
                }

                HandlePursuit(distanceToPlayer);
                break;

            case BossState.Cooldown:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                    currentState = BossState.Idle;
                break;
        }

        UpdateAnimationState();
    }

    private void HandlePursuit(float distanceToPlayer)
    {
        float currentSpeed = dashDurationTimer > 0f ? dashSpeed : pursuitSpeed;

        if (dashDurationTimer <= 0f && dashTimer <= 0f && distanceToPlayer > attackApproachDistance)
        {
            dashTimer = dashCooldown;
            dashDurationTimer = dashDuration;
            PlaySound(attackSound);
        }

        if (distanceToPlayer > attackApproachDistance)
        {
            MoveToward(player.position, currentSpeed);
        }
        else if (distanceToPlayer > attackHitDistance)
        {
            // приближаемся для атаки, но не впритык
            MoveToward(player.position, pursuitSpeed * 0.8f);
        }
        else if (distanceToPlayer < desiredAttackDistance - stopDistance)
        {
            MoveAwayFrom(player.position, pursuitSpeed * 0.6f);
        }
        else
        {
            // держим дистанцию и немного шатаемся в стороны
            Vector2 strafeDirection = Vector2.Perpendicular((player.position - transform.position).normalized);
            float strafeSign = Mathf.Sign(Vector2.Dot(strafeDirection, transform.right));
            MoveAlong(strafeDirection * strafeSign, strafeSpeed * 0.7f);
        }

        if (swingTimer <= 0f && distanceToPlayer <= attackHitDistance)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        if (player == null || isPerformingAttack)
            return;

        isPerformingAttack = true;
        swingTimer = swingInterval;
        attackWindupTimer = attackWindup;
        SetAnimatorBool(attackParameter, true);
    }

    private void PerformAttack(float distanceToPlayer)
    {
        isPerformingAttack = false;
        SetAnimatorBool(attackParameter, false);

        if (player == null)
            return;

        swingsUsed++;
        PlaySound(attackSound);

        if (distanceToPlayer <= attackHitDistance)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(handDamage);

            retreatTimer = retreatAfterHitTime;
        }
        else
        {
            // если бьёт мимо, он не отступает так сильно
            retreatTimer = attackRecovery;
        }
    }

    private void StartPursuit()
    {
        currentState = BossState.Pursuit;
        pursuitTimer = 0f;
        swingTimer = 0f;
        dashTimer = 0f;
        dashDurationTimer = 0f;
        swingsUsed = 0;
    }

    private void StartCooldown()
    {
        currentState = BossState.Cooldown;
        cooldownTimer = pursuitCooldown;
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 direction = (target - rb.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
            rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
    }

    private void MoveAwayFrom(Vector2 target, float speed)
    {
        Vector2 direction = (rb.position - target).normalized;
        if (direction.sqrMagnitude > 0.001f)
            rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
    }

    private void MoveAlong(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude > 0.001f)
            rb.MovePosition(rb.position + direction.normalized * speed * Time.deltaTime);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
            return;

        PlaySound(hitSound);
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        PlaySound(deathSound);
        SetAnimatorBool(deadParameter, true);

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        GameManager.Instance.AddKill();
        Destroy(gameObject, 0.1f);
    }

    private Transform FindPlayer()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }

    private void UpdateAnimationState()
    {
        bool isWalking = !isDead && currentState == BossState.Pursuit && !isPerformingAttack && retreatTimer <= 0f;
        bool isAttacking = !isDead && isPerformingAttack;

        SetAnimatorBool(walkParameter, isWalking);
        SetAnimatorBool(attackParameter, isAttacking);
        SetAnimatorBool(deadParameter, isDead);
    }

    private void SetAnimatorBool(string parameter, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(parameter))
            return;

        animator.SetBool(parameter, value);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, transform.position, Mathf.Clamp01(soundVolume));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, desiredAttackDistance);
    }
}
