using UnityEngine;

// Read-only tuning readouts for the Inspector: add it while tuning feel, remove it after.
// Split out of PlayerControl so measuring the player never changes how it moves.
public class PlayerMetrics : MonoBehaviour
{
    [SerializeField] private string currentForce;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float minVerticalSpeed;
    [SerializeField] private float maxVerticalSpeed;
    [SerializeField] private float highestPoint; // jump height check: compare against the take-off y

    private Rigidbody body;
    private PlayerControl player;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        player = GetComponent<PlayerControl>();
    }

    // FixedUpdate, not Update: these are physics values and only change once per step.
    private void FixedUpdate()
    {
        Vector3 velocity = body.linearVelocity;
        currentForce = player.CurrentForceName;
        horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        verticalSpeed = velocity.y;
        minVerticalSpeed = Mathf.Min(minVerticalSpeed, velocity.y);
        maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, velocity.y);
        highestPoint = Mathf.Max(highestPoint, transform.position.y);
    }
}
