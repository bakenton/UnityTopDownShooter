using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Wander, Chase, Attack, ReturnToWander }

    [Header("Wander")]
    public float patrolSpeed = 1.5f;
    public float wanderRadius = 5f;
    public float wanderPointReachDistance = 0.5f;
    public float timeBetweenWanderPoints = 2f;

    [Header("Detection")]
    public float detectionRadius = 8f;
    public float attackRadius = 1.5f;
    public float chaseSpeed = 3.5f;

    [Header("Navigation")]
    public LayerMask obstacleMask = ~0;
    public float obstacleCheckDistance = 0.4f;

    [Header("Attack")]
    public float attackInterval = 3f;
    public int attackDamage = 10;
    public int contactDamage = 10;
    public float contactDamageCooldown = 1f;

    [Header("Health")]
    public int maxHealth = 50;
    public int currentHealth;
    public GameObject deathEffectPrefab;
    public AudioClip[] hitSounds;
    public float hitVolume = 1f;
    public AudioClip[] deathSounds;
    public float deathVolume = 1f;

    [Header("Audio")]
    public AudioClip chaseSound;
    public float chaseSoundVolume = 0.7f;
    public AudioClip[] attackSounds;
    public float attackVolume = 0.8f;

    [Header("Animation")]
    private Animator animator;
    public string walkParameter = "IsWalk?";
    public string attackParameter = "IsAttack?";
    public string deadParameter = "IsDead?";

    [Header("Debug")]
    public bool drawDebugGizmos = true;

    Transform player;
    EnemyState currentState = EnemyState.Wander;
    EnemyState previousState = EnemyState.Wander;
    float attackTimer;
    bool attackReady;
    bool attackPerformed;
    float contactDamageTimer;
    Rigidbody2D rb;
    Vector2 wanderCenter;
    Vector2 wanderTarget;
    float wanderTimer;
    bool isDead;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        ValidateAnimatorParameters();
    }

    void Start()
    {
        player = FindPlayer();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        wanderCenter = transform.position;
        SetNewWanderTarget();

        currentHealth = maxHealth;
        contactDamageTimer = 0f;
        isDead = false;
        SetAnimatorBool(deadParameter, false);
        SetAnimatorBool(walkParameter, false);
        SetAnimatorBool(attackParameter, false);
    }

    void Update()
    {
        if (player == null)
        {
            player = FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        bool playerVisible = distanceToPlayer <= detectionRadius;

        if (currentHealth <= 0)
            return;

        contactDamageTimer -= Time.deltaTime;
        if (contactDamageTimer < 0f) contactDamageTimer = 0f;

        switch (currentState)
        {
            case EnemyState.Wander:
                if (playerVisible)
                {
                    currentState = EnemyState.Chase;
                    PlaySound(chaseSound, chaseSoundVolume);
                }
                else
                {
                    Wander();
                }
                break;

            case EnemyState.Chase:
                if (!playerVisible)
                {
                    currentState = EnemyState.ReturnToWander;
                }
                else if (distanceToPlayer <= attackRadius)
                {
                    currentState = EnemyState.Attack;
                }
                else
                {
                    ChasePlayer();
                }
                break;

            case EnemyState.Attack:
                if (!playerVisible)
                {
                    currentState = EnemyState.ReturnToWander;
                }
                else if (distanceToPlayer > attackRadius)
                {
                    currentState = EnemyState.Chase;
                }
                else
                {
                    AttackPlayer();
                }
                break;

            case EnemyState.ReturnToWander:
                if (playerVisible)
                {
                    currentState = EnemyState.Chase;
                    PlaySound(chaseSound, chaseSoundVolume);
                }
                else
                {
                    ReturnToWander();
                }
                break;
        }

        previousState = currentState;
        UpdateAnimationState();
    }

    Transform FindPlayer()
    {
        var playerObject = GameObject.FindWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (ReachedDestination(wanderTarget) || wanderTimer <= 0f)
        {
            SetNewWanderTarget();
        }

        MoveToward(wanderTarget, patrolSpeed);
    }

    void ChasePlayer()
    {
        MoveToward(player.position, chaseSpeed);
    }

    void ReturnToWander()
    {
        if (ReachedDestination(wanderTarget))
        {
            currentState = EnemyState.Wander;
            SetNewWanderTarget();
            return;
        }

        MoveToward(wanderTarget, patrolSpeed);
    }

    void AttackPlayer()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            attackReady = true;
            attackPerformed = false;
        }
    }

    public void OnAttackAnimationHit()
    {
        if (!attackReady || attackPerformed || player == null || currentHealth <= 0)
            return;

        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(attackDamage);
            PlayRandomAttackSound();
        }

        attackPerformed = true;
        attackReady = false;
    }

    void MoveToward(Vector2 target, float speed)
    {
        Vector2 desiredDirection = (target - rb.position).normalized;
        bool moving = desiredDirection.sqrMagnitude > 0.001f;

        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        SetAnimatorBool(walkParameter, moving && !isDead);

        Vector2 movementDirection = GetMovementDirection(desiredDirection);
        Vector2 nextPosition = rb.position + movementDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    Vector2 GetMovementDirection(Vector2 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude <= 0.001f)
            return Vector2.zero;

        if (!IsBlocked(desiredDirection))
            return desiredDirection;

        Vector2 left = Vector2.Perpendicular(desiredDirection).normalized;
        Vector2 right = -left;

        bool leftClear = !IsBlocked(left);
        bool rightClear = !IsBlocked(right);

        if (leftClear && rightClear)
            return leftClear ? left : right;

        if (leftClear)
            return left;
        if (rightClear)
            return right;

        return Vector2.zero;
    }

    bool IsBlocked(Vector2 direction)
    {
        if (rb == null || direction.sqrMagnitude <= 0.001f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, obstacleCheckDistance, obstacleMask);
        if (hit.collider == null)
            return false;

        if (hit.collider.transform == transform)
            return false;

        if (hit.collider.CompareTag("Player"))
            return false;

        return true;
    }

    bool ReachedDestination(Vector2 target)
    {
        return Vector2.Distance(rb.position, target) <= wanderPointReachDistance;
    }

    void SetNewWanderTarget()
    {
        wanderTimer = timeBetweenWanderPoints;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0f, wanderRadius);
        wanderTarget = wanderCenter + offset;
    }

    void SetAnimatorBool(string parameter, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(parameter))
            return;

        animator.SetBool(parameter, value);
    }

    void UpdateAnimationState()
    {
        if (animator == null)
            return;

        SetAnimatorBool(deadParameter, isDead);
        bool isWalking = !isDead && (currentState == EnemyState.Wander || currentState == EnemyState.Chase || currentState == EnemyState.ReturnToWander);
        SetAnimatorBool(walkParameter, isWalking);

        bool isAttacking = !isDead && currentState == EnemyState.Attack;
        SetAnimatorBool(attackParameter, isAttacking);
    }

    void ValidateAnimatorParameters()
    {
        if (animator == null)
            return;

        if (!AnimatorHasParameter(walkParameter))
            Debug.LogWarning($"Enemy: Animator parameter '{walkParameter}' not found on {name}.");
        if (!AnimatorHasParameter(attackParameter))
            Debug.LogWarning($"Enemy: Animator parameter '{attackParameter}' not found on {name}.");
        if (!AnimatorHasParameter(deadParameter))
            Debug.LogWarning($"Enemy: Animator parameter '{deadParameter}' not found on {name}.");
    }

    bool AnimatorHasParameter(string parameter)
    {
        if (animator == null || string.IsNullOrEmpty(parameter))
            return false;

        foreach (var p in animator.parameters)
        {
            if (p.name == parameter)
                return true;
        }
        return false;
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, transform.position, Mathf.Clamp01(volume));
    }

    void PlayRandomHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, hitSounds.Length);
        PlaySound(hitSounds[randomIndex], hitVolume);
    }

    void PlayRandomDeathSound()
    {
        if (deathSounds == null || deathSounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, deathSounds.Length);
        PlaySound(deathSounds[randomIndex], deathVolume);
    }

    void PlayRandomAttackSound()
    {
        if (attackSounds == null || attackSounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, attackSounds.Length);
        PlaySound(attackSounds[randomIndex], attackVolume);
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayRandomHitSound();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        SetAnimatorBool(deadParameter, true);
        SetAnimatorBool(walkParameter, false);
        PlayRandomDeathSound();

        GameManager.Instance.AddKill();

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject, 1.5f);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == EnemyState.Attack)
            return;

        if (contactDamageTimer > 0f) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            var health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(contactDamage);
                contactDamageTimer = contactDamageCooldown;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wanderCenter, wanderRadius);
        Gizmos.DrawSphere(wanderTarget, 0.15f);
    }
}
