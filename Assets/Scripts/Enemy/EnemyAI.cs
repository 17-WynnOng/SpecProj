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
    [SerializeField] protected Transform[] path;
    [SerializeField] protected Transform player;
    [SerializeField] protected float tolerance = 0.3f;
    protected int currentIndex;

    [Header("Drops")]
    [SerializeField] private GameObject dropPrefab;

    protected EnemyState state = EnemyState.Patrol;

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
        path = GameManager.Instance.levelPath.waypoints;
        currentIndex = 0;
        if (path.Length > 0)
            agent.SetDestination(path[0].position);
    }

    protected virtual void HandlePathing()
    {
        if (agent.pathPending || agent.remainingDistance > tolerance) return;

        currentIndex++;
        if (currentIndex < path.Length)
            agent.SetDestination(path[currentIndex].position);
        else
        {
            float currentHealth = GetCurrentHealth();
            GameManager.Instance.playerBase.TakeDamage(currentHealth);
            Die();
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

    protected override void Die()
    {
        DropCollectible();
        GameManager.Instance.enemyCounter++;
        GameManager.Instance.AdvanceWave();
        base.Die();
    }

    private void DropCollectible()
    {
        if (dropPrefab == null)
            return;

        int dropAmount = Random.Range(1, 4); // 1 to 3

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

    protected abstract void Update();// Let children override entirely
}

