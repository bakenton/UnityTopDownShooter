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

    [Header("Attack")]
    public float attackInterval = 1.2f;
    public int attackDamage = 10;
    public int contactDamage = 10;
    public float contactDamageCooldown = 1f;

    [Header("Health")]
    public int maxHealth = 50;
    public int currentHealth;
    public GameObject deathEffectPrefab;

    [Header("Debug")]
    public bool drawDebugGizmos = true;

    Transform player;
    EnemyState currentState = EnemyState.Wander;
    float attackTimer;
    float contactDamageTimer;
    Rigidbody2D rb;
    Vector2 wanderCenter;
    Vector2 wanderTarget;
    float wanderTimer;

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
                }
                else
                {
                    ReturnToWander();
                }
                break;
        }
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
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }
    }

    void MoveToward(Vector2 target, float speed)
    {
        Vector2 direction = (target - rb.position).normalized;
        rb.MovePosition(Vector2.MoveTowards(rb.position, target, speed * Time.deltaTime));

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
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

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
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
