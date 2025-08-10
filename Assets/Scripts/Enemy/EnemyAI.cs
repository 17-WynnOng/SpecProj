using UnityEngine.AI;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Search,
    Attack
}

public abstract class EnemyAI : Damageable
{
    protected NavMeshAgent agent;

    [Header("Path Finding")]
    [SerializeField] public Transform[] path;
    [SerializeField] protected Transform player;
    [SerializeField] protected float tolerance = 0.3f;
    protected int currentIndex;

    [Header("Drops")]
    [SerializeField] private GameObject dropPrefab;
    [Tooltip("minimum amount of drops")]
    [SerializeField] private int minDrops;
    [Tooltip("minimum amount of drops")]
    [SerializeField] private int maxDrops;

    [Header("Animator")]
    [SerializeField] protected Animator animator;

    [Header("Health Bar")]
    [SerializeField] protected EnemyHealthBar healthBar;

    protected EnemyState state = EnemyState.Patrol;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer);
    }

    protected override void Start()
    {
        base.Start();
        currentIndex = 0;
        if (path.Length > 0)
            agent.SetDestination(path[0].position);
    }

    protected virtual void HandlePathing()
    {
        if (agent.pathPending || agent.remainingDistance > tolerance) return;

        currentIndex++;
        if (currentIndex < path.Length)
        {
            Vector3 baseTarget = path[currentIndex].position;

            // Offset radius (tweakable)
            float radius = 2;

            // Generate a random point in a circle around the waypoint (on XZ plane)
            Vector2 offset2D = Random.insideUnitCircle * radius;
            Vector3 offset = new Vector3(offset2D.x, 0f, offset2D.y);

            Vector3 randomizedTarget = baseTarget + offset;

            // Check if point is on NavMesh
            if (NavMesh.SamplePosition(randomizedTarget, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                agent.SetDestination(baseTarget); // fallback to exact waypoint if failed
            }
        }
        else
        {
            float currentHealth = GetCurrentHealth();
            GameManager.Instance.playerBase.TakeDamage(currentHealth);
            DieWithNoDrops();
        }
    }

    protected int GetClosestWaypointIndex()
    {
        int closestIndex = 0;
        float closestDist = float.MaxValue;

        Vector3 pos = transform.position;
        for (int i = 0; i < path.Length; i++)
        {
            float dist = Vector3.SqrMagnitude(path[i].position - pos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void DieWithNoDrops()
    {
        base.Die();
        onDeath?.Invoke(); // Notify listeners
    }

    protected override void Die()
    {
        DropCollectible();
        onDeath?.Invoke(); // Notify listeners
        base.Die();
    }

    private void DropCollectible()
    {
        if (dropPrefab == null)
            return;

        int dropAmount = Random.Range(minDrops, maxDrops + 1); // 1 to 3

        for (int i = 0; i < dropAmount; i++)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 1f;
            GameObject scrap = Instantiate(dropPrefab, spawnPosition, Quaternion.identity);

            if (scrap.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                // Launch in a random horizontal direction with some upward force
                Vector3 randomDir = Quaternion.Euler(0, Random.Range(0f, 360f), 0) * Vector3.forward;
                Vector3 launchForce = (randomDir + Vector3.up * 0.75f) * Random.Range(1f, 2f);
                rb.AddForce(launchForce, ForceMode.Impulse);
            }
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (healthBar != null)
            healthBar.UpdateEnemyHealthBar(currentHealth, maxHealth);
    }

    protected abstract void Update();// Let children override entirely
}

