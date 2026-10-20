using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Moves the player character: a capsule floated above the ground by a damped spring,
// steered by acceleration curves, with jump, attack and dash applied as action forces.
// Interaction, system input and tuning readouts live in sibling components.
public class PlayerControl : MonoBehaviour, IDataPersistence
{
    public static PlayerControl Current { get; private set; }

    // A jump still counts this long after walking off a ledge. It was a serialized field that
    // Awake overwrote, so the scenes hold 0 for it and must not be read.
    private const float CoyoteTime = 0.25f;
    private const float MaxAccelerationForce = 500f; // scaled per turn by the curve below
    private const float FallingSpeed = -0.5f; // below this, spring jitter is not mistaken for a fall

    [SerializeField] private PlayerData data;

    [Header("Floating")]
    [SerializeField] private float floatDistance;
    [SerializeField] private float rayDistance;
    [SerializeField] private float springStrength = 2000f;
    [SerializeField] private float dampForce = 40f;
    [SerializeField] private float groundDeltaThreshold;
    [SerializeField] private LayerMask groundLayer;

    [Header("Horizontal Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;
    [SerializeField] private AnimationCurve maxAccelerationForceFactorFromDot;

    // Attack and dash are data so they can be tuned here. Jump is built in code because its
    // strength comes from PlayerData, next to the rest of the movement tuning.
    [Header("Action Forces")]
    [SerializeField] private ActionForce attack = new ActionForce
    {
        Name = "Attack", Mode = ForceMode.Impulse, Direction = Vector3.forward,
        Magnitude = 5f, Duration = 0.2f, LocksMovement = true, LocksGravity = true,
    };
    [SerializeField] private ActionForce dash = new ActionForce
    {
        Name = "Dash", Mode = ForceMode.Impulse, Direction = Vector3.forward,
        Magnitude = 10f, Duration = 0.5f, LocksMovement = true, LocksGravity = true,
    };

    private readonly ActionForceRunner forces = new ActionForceRunner();
    private Rigidbody body;
    private PlayerAnimation anim;
    private InputAction move, jump, run;
    private InputBindings bindings;

    // Runtime state. Not serialized; the Inspector's Debug mode still shows it while tuning.
    private Vector2 input;
    private Vector3 goalVelocity;
    private float airSpeed, airSpeedDecay, coyoteTimer;
    private bool shouldGround = true;
    private bool isGrounded, isFalling, isJumping, isRunning;

    public PlayerInput InputComponent { get; private set; }
    public string CurrentForceName => forces.Current.Name;

    private void Awake()
    {
        if (Current == null) Current = this;

        body = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<PlayerAnimation>();
        InputComponent = GetComponent<PlayerInput>();

        InputActionAsset actions = InputComponent.actions;
        move = actions["Move"];
        jump = actions["Jump"];
        run = actions["Run"];
        bindings = new InputBindings(actions,
            ("Jump", _ => TriggerJump()),
            ("Act1", _ => ApplyActionForce(attack, anim.SetPrimaryAnim)),
            ("Act2", _ => ApplyActionForce(dash, anim.SetSecondaryAnim)));
    }

    private void OnEnable()
    {
        EventManager.Current.OnPlayerDeath += TriggerDeath;
        EventManager.Current.OnPlayerRestore += data.RestoreStats;
        bindings.Enable();
    }

    private void OnDisable()
    {
        EventManager.Current.OnPlayerDeath -= TriggerDeath;
        EventManager.Current.OnPlayerRestore -= data.RestoreStats;
        bindings.Disable();
    }

    // Otherwise Current keeps pointing at a destroyed player between scene loads.
    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    private void TriggerDeath() => Debug.LogWarning("Player died. Death is not implemented yet.");

    private void FixedUpdate()
    {
        input = move.ReadValue<Vector2>();
        if (forces.Tick(Time.fixedDeltaTime)) anim.SetForceEndTrigger();

        CheckGround();
        CheckFall();

        // Ordering matters: coyote time reads last step's isJumping, as it always has.
        coyoteTimer = isGrounded ? 0f : isJumping ? CoyoteTime : coyoteTimer + Time.fixedDeltaTime;
        isRunning = run.IsPressed() && input != Vector2.zero;
        isJumping = jump.IsPressed() && !isGrounded;

        Move();
        ApplyGravity();
    }

    private void CheckGround()
    {
        Vector3 down = -transform.up;
        bool hit = Physics.Raycast(transform.position, down, out RaycastHit ground, rayDistance, groundLayer);
        float groundDelta = hit ? ground.distance - floatDistance : float.PositiveInfinity;

        // Damped spring toward floatDistance. Released on take-off so it cannot fight the jump.
        if (hit && shouldGround)
            body.AddForce(down * (groundDelta * springStrength - Vector3.Dot(down, body.linearVelocity) * dampForce));

        isGrounded = Mathf.Abs(groundDelta) < groundDeltaThreshold;
        anim.SetGrounded(isGrounded);
        Debug.DrawRay(transform.position, down * rayDistance, isGrounded ? Color.green : Color.cyan);
    }

    private void CheckFall()
    {
        isFalling = body.linearVelocity.y < FallingSpeed;
        if (isFalling) shouldGround = true;
        anim.SetFalling(isFalling);
    }

    private void Move()
    {
        Vector3 direction = new Vector3(input.x, 0f, input.y);
        float dot = Vector3.Dot(direction, goalVelocity);
        float groundSpeed = isRunning ? data.RunSpeed : data.WalkSpeed;

        // In the air, speed starts at take-off speed and decays each step, so jumps commit.
        float speed = isGrounded ? groundSpeed : airSpeed;
        airSpeed = isGrounded ? groundSpeed : Mathf.Max(airSpeed - airSpeedDecay, 0f);

        goalVelocity = Vector3.MoveTowards(goalVelocity, speed * direction,
            acceleration * accelerationFactorFromDot.Evaluate(dot) * Time.fixedDeltaTime);

        bool canMove = !forces.Current.LocksMovement;
        if (canMove) Steer(direction, dot);

        Vector2 shown = canMove ? input : Vector2.zero;
        anim.SetMoveAnimation(shown.x, shown.y, shown != Vector2.zero, canMove && isRunning);
        anim.SetDirection(transform.eulerAngles.y);
    }

    // Steering is a force, never a velocity write, so the float spring and action forces
    // still add on top of it within the same physics step.
    private void Steer(Vector3 direction, float dot)
    {
        Vector3 velocity = body.linearVelocity;
        Vector3 needed = (goalVelocity - new Vector3(velocity.x, 0f, velocity.z)) / Time.fixedDeltaTime;
        needed = Vector3.ClampMagnitude(needed, MaxAccelerationForce * maxAccelerationForceFactorFromDot.Evaluate(dot));
        body.AddForce(needed.x * body.mass, 0f, needed.z * body.mass);

        if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ApplyGravity()
    {
        if (isGrounded || forces.Current.LocksGravity) return;

        // Four phases: rising or falling, jump held or released.
        float factor = isFalling
            ? (isJumping ? data.LatterPressedGravityFactor : data.LatterReleasedGravityFactor)
            : (isJumping ? data.InitialPressedGravityFactor : data.InitialReleasedGravityFactor);
        body.AddForce(factor * Physics.gravity.y * Vector3.up);
    }

    private void TriggerJump()
    {
        if (!isGrounded && coyoteTimer >= CoyoteTime) return;

        shouldGround = false;
        // Air speed lost per step: running jumps start faster but bleed speed quicker.
        airSpeedDecay = (isRunning ? 5.5f : 1.5f) * Time.fixedDeltaTime;
        ApplyActionForce(new ActionForce
        {
            Name = "Jump",
            Mode = ForceMode.Impulse,
            Direction = Vector3.up,
            Magnitude = data.JumpSpeed * (isRunning ? data.JumpBoost : 1f),
        }, anim.SetJumpAnim);
    }

    private void ApplyActionForce(ActionForce force, Action animate)
    {
        // The animation plays even when the force is refused, as it always has: mashing attack
        // restarts the swing without restarting the impulse.
        animate();
        bool interrupting = forces.IsActive;
        if (!forces.TryApply(force)) return;

        // Interrupting a running force kills momentum so the new one starts from rest; a force
        // applied from idle keeps it on purpose, so dashing out of a run carries the run.
        if (interrupting) body.linearVelocity = Vector3.zero;

        body.AddForce(transform.TransformDirection(force.Direction) * force.Magnitude, force.Mode);
    }

#if UNITY_EDITOR
    // Editor-only cheats, excluded from builds so players cannot trigger them.
    // V recovers the Player action map if a menu ever leaves input stuck on UI.
    private void Update()
    {
        Keyboard keys = Keyboard.current;
        if (keys == null) return;

        if (keys.pKey.wasPressedThisFrame) data.Health--;
        if (keys.oKey.wasPressedThisFrame) data.MaxHealth += 4;
        if (keys.iKey.wasPressedThisFrame) data.Health++;
        if (keys.vKey.wasPressedThisFrame) InputComponent.SwitchCurrentActionMap("Player");
    }
#endif

    public void SaveData(GameData saved)
    {
        data.SaveData(saved);
        saved.position = transform.position;
        saved.rotation = transform.rotation;
    }

    public void LoadData(GameData saved)
    {
        data.LoadData(saved);
        transform.SetPositionAndRotation(saved.position, saved.rotation);
    }
}
