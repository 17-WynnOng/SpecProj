 using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Attract Settings")]
    [SerializeField] private float attractSpeed = 5f;

    private Transform playerTransform;
    private Rigidbody rb;
    private Collider col;

    private bool isFlyingToPlayer = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = true;
        col.isTrigger = false; // default collider is solid

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void FixedUpdate()
    {
        Rotate();

        if (isFlyingToPlayer)
        {
            FlyToPlayer();
        }
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.fixedDeltaTime, Space.World);
    }


    public void StartFlyingToPlayer(Transform player)
    {
        if (isFlyingToPlayer) return;

        playerTransform = player;
        isFlyingToPlayer = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        col.isTrigger = true;
    }

    private void FlyToPlayer()
    {
        if (playerTransform == null) return;

        Vector3 dir = (playerTransform.position - transform.position).normalized;
        rb.MovePosition(transform.position + dir * attractSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFlyingToPlayer) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Collect(player);
                Destroy(gameObject);
            }
        }
    }

    protected abstract void Collect(PlayerController player);
}
