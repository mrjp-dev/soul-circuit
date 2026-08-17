using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour, IDataPersistence
{
    public static PlayerControl Current { get; private set; }
    [SerializeField] private PlayerData data;
    private PlayerInput inputComponent;
    private PlayerAnimation animationComponent;
    [SerializeField] private Rigidbody rbComponent;
    [SerializeField] private CapsuleCollider rbColliderComponent;
    [SerializeField] private BoxCollider[] boxColliders;
    [SerializeField] private BoxCollider interactCollider;
    [SerializeField] private BoxCollider hurtboxCollider;
    [SerializeField] private BoxCollider hitboxCollider;

    [SerializeField] private GameObject currentInteractable;
    [SerializeField] private CustomTag currentInteractableTag;

    private struct Force
    {
        public string Name;
        public ForceMode Mode;
        public Vector3 Direction;
        public float Magnitude;
        public float Duration;
        public bool IsInterruptable;
        public bool IsSelfInterruptable;
        public bool AllowsMovement;
        public bool AllowsGravity;

        public Force(string name = "None",
                     ForceMode mode = ForceMode.Force,
                     Vector3 direction = default,
                     float magnitude = 0f,
                     float duration = 0f,
                     bool isInterruptable = true,
                     bool isSelfInterruptable = true,
                     bool allowsMovement = true,
                     bool allowsGravity = true)
        {
            Name = name;
            Mode = mode;
            Direction = direction;
            Magnitude = magnitude;
            Duration = duration;
            IsInterruptable = isInterruptable;
            IsSelfInterruptable = isSelfInterruptable;
            AllowsMovement = allowsMovement;
            AllowsGravity = allowsGravity;
        }
    }
    
    private Force previousForce = new Force();
    private Force currentForce = new Force();
    private Force nullForce = new Force("None");

    private Force jumpForce;
    private Force attackForce;
    private Force dodgeForce;
    private Force dashForce;

    [SerializeField] private string previousForceName;
    [SerializeField] private string currentForceName;

    // Floating

    [SerializeField] private float floatDistance;
    [SerializeField] private float rayDistance;

    public RaycastHit hitInfo;
    public LayerMask interactLayer;
    public LayerMask groundLayer;
    public LayerMask damageLayer;

    // Player character inputs
    private InputAction inputMove;
    private InputAction inputAct1;
    private InputAction inputAct2;
    private InputAction inputAct3;
    private InputAction inputAct4;
    private InputAction inputJump;
    private InputAction inputInteraction;
    private InputAction inputRun;
    private InputAction inputMenuOptions;
    private InputAction inputInventory;
    private InputAction inputMap;
    private InputAction inputCameraZoomIn;
    private InputAction inputCameraZoomOut;

    // Menu inputs
    private InputAction inputMenuCancel;

    #region Movement Variables

    [Header("Horizontal Movement Factors")]
    [SerializeField] private float maxMoveSpeed; // maximum movement speed defined by input
    [SerializeField] private float acceleration; // defines how fast maxMoveSpeed will be reached

    // Move speed while falling, and its decreasing factor
    [SerializeField] private float onAirMaxMoveSpeed;
    [SerializeField] private float onAirMoveFactor;

    private float maxAccelerationForce = 500f; // maximum force that can be applied when accelerating

    [Header("Factor Curves")]
    [SerializeField] private AnimationCurve accelerationFactorFromDot; // how fast the acceleration will change when switching directions
    [SerializeField] private AnimationCurve maxAccelerationForceFactorFromDot; // how fast the maximum force will change when switching directions

    private Vector3 goalHorizontalVelocity = Vector3.zero; // desired horizontal velocity
    private Vector3 horizontalForceScale = new Vector3(1f, 0, 1f); // scaling vector used in AddForce() for scaling the force to be applied

    [Header("Horizontal Movement Attributes")]
    [SerializeField] private Vector2 horizontalDirection; // Player's direction of movement as dictated by inputMove
    [SerializeField] private Vector3 maxHorizontalVelocity; // Player's force of movement after transforming the 2D vector of movement direction and applying the corresponding forces

    [Header("Vertical Movement Attributes")]
    [SerializeField] private float jumpDelay; //
    [SerializeField] private float jumpDelayTimer; //

    [SerializeField] private float springStrength = 2000f;
    [SerializeField] private float dampForce = 40f;

    [SerializeField] private float groundDeltaThreshold; // The delta threshold for the player to be considered grounded
    [SerializeField] private float groundDelta = 0; // The delta between the floating distance and the distance at which the raycast hit the ground collider

    [Header("Metrics")]
    [SerializeField] private Vector3 lastPosition; // Player's position in the last frame
    [Space(20)]

    [SerializeField] private Vector3 desiredHorizontalVelocity; // Desired horizontal velocity speed as per input
    [SerializeField] private float desiredHorizontalSpeed; // Desired horizontal speed as per input
    [Space(20)]

    [SerializeField] private Vector3 horizontalVelocity; // observed horizontal velocity after physics
    [SerializeField] private float horizontalSpeed; // observed horizontal speed after physics
    [Space(20)]

    [SerializeField] private float highestPoint; // highest position reached in y axis
    [SerializeField] private float verticalSpeed; // observed vertical movement speed after physics
    [SerializeField] private float minVerticalSpeed = 0; // Actual observed vertical movement speed after physics
    [SerializeField] private float maxVerticalSpeed = 0; // Actual observed vertical movement speed after physics
    [Space(20)]

    [SerializeField] private float fixedFrameTime; // 
    [SerializeField] private float frameTime; // 

    [Header("Other Forces")]
    [SerializeField] private float otherForceDuration; //
    [SerializeField] private float otherForceTimer = 0f; //

    [Header("States")]
    [SerializeField] private bool otherForce = false; // true

    [SerializeField] private bool isRunning; // True if the player is pressing the run button
    [SerializeField] private bool isPressingJump; // True if the player is pressing the jump button
    [SerializeField] private bool canJump; // True if the player is able to jump
    [SerializeField] private bool shouldGround = true; // True if the player isn't jumping
    [SerializeField] private bool isJumping; // True if the player is pressing the jump button and is not grounded
    [SerializeField] private bool isFalling; // True if verticalSpeed < 0
    [SerializeField] private bool isGrounded; // True if the player's grounding checkbox is in contact with an object with ground LayerMask
    [SerializeField] private bool couldInteract; // True if the player's trigger collider is in contact with an object with the "Interactable" tag

    #endregion

    public PlayerInput InputComponent { get => inputComponent; private set => inputComponent = value; }

    #region Initialization

    private void Awake()
    {
        // Singleton instance
        if (Current == null)
            Current = this;

        // Get PlayerInput component and map the corresponding InputActions
        inputComponent = GetComponent<PlayerInput>();

        // Player character control inputs
        inputMove = inputComponent.actions["Move"];
        inputAct1 = inputComponent.actions["Act1"];
        inputAct2 = inputComponent.actions["Act2"];
        inputAct3 = inputComponent.actions["Act3"];
        inputAct4 = inputComponent.actions["Act4"];
        inputJump = inputComponent.actions["Jump"];
        inputInteraction = inputComponent.actions["Interact"];
        inputRun = inputComponent.actions["Run"];
        inputMenuOptions = inputComponent.actions["MenuOptions"];
        inputInventory = inputComponent.actions["MenuInventory"];
        inputMap = inputComponent.actions["MenuMap"];
        inputCameraZoomIn = inputComponent.actions["ZoomIn"];
        inputCameraZoomOut = inputComponent.actions["ZoomOut"];

        // Menu control inputs
        inputMenuCancel = inputComponent.actions["Cancel"];

        animationComponent = GetComponentInChildren<PlayerAnimation>();

        rbComponent = GetComponent<Rigidbody>();

        // Get collider components
        rbColliderComponent = transform.GetComponent<CapsuleCollider>();

        boxColliders = transform.GetComponentsInChildren<BoxCollider>();
        
        interactCollider = boxColliders[0];
        //hurtboxCollider = boxColliders[1];
        //hitboxCollider = boxColliders[2];
        
        //hurtboxCollider.gameObject.SetActive(false);
        //hitboxCollider.gameObject.SetActive(false);

        jumpDelay = 0.25f;
        jumpDelayTimer = 0f;

        Physics.IgnoreCollision(interactCollider, rbColliderComponent, true);
        Physics.IgnoreCollision(interactCollider, rbColliderComponent, true);

        data.InitializeDataIfNull();
    }

    private void OnEnable()
    {
        EventManager.current.OnPlayerDeath += TriggerDeath;
        EventManager.current.OnPlayerRestore += data.RestoreStats;
    }

    private void OnDisable()
    {
        EventManager.current.OnPlayerDeath -= TriggerDeath;
        EventManager.current.OnPlayerRestore -= data.RestoreStats;
    }

    public void TriggerDeath()
    {
        Debug.Log("Player Died!");
    }

    private void Start()
    {

        // Subscribe corresponding methods to the input that triggers them
        inputJump.performed += TriggerJump;
        inputAct1.performed += TriggerPrimaryAction;
        inputAct2.performed += TriggerSecondaryAction;
        inputInteraction.performed += TriggerInteraction;
        inputMenuOptions.performed += TriggerPause;
        inputInventory.performed += TriggerInventory;
        inputCameraZoomIn.performed += TriggerCameraZoomIn;
        inputCameraZoomOut.performed += TriggerCameraZoomOut;

        inputMenuCancel.performed += TriggerMenuCancel;
    }

    #endregion

    #region Updates

    private void Update()
    {
        // Get continuous player input
        horizontalDirection = inputMove.ReadValue<Vector2>();
        isPressingJump = inputJump.IsPressed();

        // DEBUG: drain/add health
        if (Keyboard.current.pKey.wasPressedThisFrame)
            ApplyDamage();
        if (Keyboard.current.oKey.wasPressedThisFrame)
            IncreaseMaxHealth();
        if (Keyboard.current.iKey.wasPressedThisFrame)
            IncreaseCurrentHealth();
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            inputComponent.SwitchCurrentActionMap("Player");
            Debug.Log("input changed");
        }
            

        frameTime = Time.deltaTime;
    }

    private void FixedUpdate()
    {
        previousForceName = previousForce.Name;
        currentForceName = currentForce.Name;

        CheckOtherForceTimer();
        CheckGround();
        CheckFall(); // Check if player's vertical speed is less than the falling speed threshold
        CoyoteTimeCheck();

        //shouldGround = (canJump && isGrounded);

        minVerticalSpeed = Mathf.Min(minVerticalSpeed, rbComponent.linearVelocity.y);
        maxVerticalSpeed = Mathf.Max(maxVerticalSpeed, rbComponent.linearVelocity.y);

        desiredHorizontalVelocity = maxHorizontalVelocity;
        desiredHorizontalSpeed = maxHorizontalVelocity.magnitude;

        // Calculate player's horizontal speed
        Vector3 positionXZ = transform.position - new Vector3(0f, transform.position.y, 0f);
        horizontalVelocity = (positionXZ - lastPosition) / Time.fixedDeltaTime;
        horizontalSpeed = horizontalVelocity.magnitude;
        lastPosition = positionXZ;

        // Calculate player's highest vertical position
        highestPoint = Mathf.Max(highestPoint, transform.position.y);

        // Continuously check if the player is running and jumping
        canJump = jumpDelay > jumpDelayTimer;
        isRunning = inputRun.IsPressed() && horizontalDirection.magnitude > 0;
        isJumping = inputJump.IsPressed() && !isGrounded;
        

        //Apply movement
        BetterMovePlayer();

        fixedFrameTime = Time.fixedDeltaTime;
    }

    #endregion

    #region Player State Checks

    /* GroundCheck() creates a physics checkBox with checkBoxSize, and the checkBox returns if it's colliding with
     * an object with the ground LayerMask, currently the only layer considered ground is "Environment"
     */

    private void CoyoteTimeCheck()
    {
        if (!isGrounded)
            jumpDelayTimer = isJumping ? jumpDelay : jumpDelayTimer + Time.fixedDeltaTime;
        else
            jumpDelayTimer = 0f;
    }

    private void CheckGround()
    {
        Vector3 rayOrigin = transform.position; //transform.TransformPoint(colliderComponent.center);
        Vector3 rayDir = transform.TransformDirection(Vector3.down);
        

        Debug.DrawRay(rayOrigin, rayDir * floatDistance, Color.blue); // Draw raycast color
        Debug.DrawRay(new Vector3(rayOrigin.x, rayOrigin.y - floatDistance, rayOrigin.z), rayDir * rayDistance, Color.cyan); // Draw raycast color

        bool rayHit = Physics.Raycast(rayOrigin, rayDir, out hitInfo, rayDistance, groundLayer);

        if (rayHit)
        {
            Vector3 rbVelocity = rbComponent.linearVelocity;
            float rayDirVelocity = Vector3.Dot(rayDir, rbVelocity);

            groundDelta = hitInfo.distance - floatDistance;

            float springForce = (groundDelta * springStrength) - (rayDirVelocity * dampForce);

            if(shouldGround)
                rbComponent.AddForce(rayDir * springForce);

            isGrounded = Mathf.Abs(groundDelta) < groundDeltaThreshold;
        }
        else
        {
            groundDelta = Mathf.Infinity;
            isGrounded = false;
        }
            
        
        animationComponent.SetGrounded(isGrounded);
    }

    // FallingCheck() takes in the rigidbody velocity in its Y axis, and returns if its value is less than 0, or negative

    private void CheckFall()
    {
        verticalSpeed = rbComponent.linearVelocity.y;
        isFalling = rbComponent.linearVelocity.y < -0.5f;

        if (isFalling)
            shouldGround = true;

        animationComponent.SetFalling(isFalling, transform.eulerAngles.y);
    }

    private void CheckOtherForceTimer(bool otherCondition = false)
    {
        bool baseCondition = otherForceDuration > otherForceTimer;
        otherForce = baseCondition || otherCondition;

        if (otherForce)
            otherForceTimer += Time.fixedDeltaTime;
        else
        {
            otherForceTimer = otherForceDuration;
            currentForce = nullForce;
            previousForce = currentForce;

            animationComponent.SetForceEndTrigger();
        }
    }

    #endregion

    #region Player Triggers & Event Triggers

    private void StartOtherForceTimer(float forceDuration)
    {
        otherForce = true;
        otherForceDuration = forceDuration;
        otherForceTimer = 0f;
    }

    /* OnTriggerEnter, OnTriggerStay, and OnTriggerExit check whether the player has come in contact, or lost it with an interactable object,
     * and then toggles interactivity with said object(s)
     */

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
            ToggleInteract(true, other.gameObject);
        else
            ToggleInteract(false);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
            ToggleInteract(true, other.gameObject);
        else
            ToggleInteract(false);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
            ToggleInteract(false);
    }

    /* ToggleInteract toggles interactivity between the player and interactable objects, as it reads the player's ability to interact
     * and sets the currentInteractable object to the latest interactable the player has collided with
     */

    public void ToggleInteract(bool state, GameObject interactableObject = null)
    {
        couldInteract = state;
        currentInteractable = interactableObject;

        if (couldInteract)
        {
            currentInteractableTag = currentInteractable.GetComponent<CustomTag>();
            //Debug.Log("Can Interact!");
        }
        else
        {
            currentInteractableTag = null;
            //Debug.Log("Can No Longer Interact!");
        }
    }

    /* TriggerInteraction triggers an interaction between the player and the currentInteractable
     * if the player can interact and the object is interactable and has an interactable tag
     */

    private void TriggerInteraction(InputAction.CallbackContext context)
    {
        if (context.performed && couldInteract)
        {
            //switch(currentInteractableTag.);
            if (currentInteractableTag.HasTag("Item"))
                Debug.Log("Interacted with item!");// Do stuff
            else
                DialogueManager.current.StartDialogue(currentInteractable);
        }
    }

    /* TriggerJump makes the player jump if the jump button has been pressed and the player is grounded, using AddForce and making the
     * player jump a bit higher if they're running
     */

    private void TriggerJump(InputAction.CallbackContext context)
    {
        if (context.performed && (isGrounded || canJump))
        {
            shouldGround = false;
            onAirMoveFactor = isRunning ? 5.5f * Time.fixedDeltaTime : 1.5f * Time.fixedDeltaTime;

            jumpForce = new Force("Jump", ForceMode.Impulse, Vector3.up, isRunning ? data.JumpBoost * data.JumpSpeed : data.JumpSpeed, 0f, true, false, true, true);
            ApplyActionForce(jumpForce);

            //rbComponent.AddForce(isRunning ? dataComponent.JumpBoost * dataComponent.JumpSpeed * Vector3.up : dataComponent.JumpSpeed * Vector3.up, ForceMode.Impulse);


            animationComponent.SetJumpAnim();
        }
    }

    private void TriggerPrimaryAction(InputAction.CallbackContext context)
    {
        attackForce = new Force(name: "Attack",
                                mode: ForceMode.Impulse,
                                direction: transform.forward,
                                magnitude: 5f,
                                duration: .2f,
                                isInterruptable: true,
                                isSelfInterruptable: false,
                                allowsMovement: false,
                                allowsGravity: false);

        ApplyActionForce(attackForce);

        animationComponent.SetPrimaryAnim();
    }

    private void TriggerSecondaryAction(InputAction.CallbackContext context)
    {
        // dodgeForce = new Force("Dodge", ForceMode.Impulse, transform.forward, 20f, .25f, false, false, false, true);
        dashForce = new Force(name: "Dash",
                              mode: ForceMode.Impulse,
                              direction: transform.forward,
                              magnitude: 10f,
                              duration: .5f,
                              isInterruptable: true,
                              isSelfInterruptable: false,
                              allowsMovement: false,
                              allowsGravity: false);

        ApplyActionForce(dashForce);

        animationComponent.SetSecondaryAnim();
    }

    private void ApplyActionForce(Force actionForce)
    {
        if (otherForce)
        {
            if (actionForce.Name == previousForce.Name)
            {
                if(actionForce.IsSelfInterruptable)
                {
                    Debug.Log("Interrupting previous force");

                    currentForce = actionForce;


                    rbComponent.linearVelocity = Vector3.zero;
                    StartOtherForceTimer(actionForce.Duration);
                    rbComponent.AddForce(actionForce.Direction * actionForce.Magnitude, actionForce.Mode);
                }
                else
                {
                    Debug.LogWarning("Can't apply force in this moment!");
                    currentForce = nullForce;
                    return;
                }
            }
            else if (previousForce.IsInterruptable)
            {
                Debug.Log("Interrupting previous force");

                currentForce = actionForce;

                rbComponent.linearVelocity = Vector3.zero;
                StartOtherForceTimer(actionForce.Duration);
                rbComponent.AddForce(actionForce.Direction * actionForce.Magnitude, actionForce.Mode);

                Debug.Log(isGrounded ? "Performed " + actionForce.Name + " on the ground" : "Performed " + actionForce.Name + " on the air");

                previousForce = actionForce;
            }

            else
            {
                Debug.LogWarning("Can't apply force in this moment!");
                currentForce = nullForce;
                return;
            }
        }
        else
        {
            currentForce = actionForce;

            StartOtherForceTimer(actionForce.Duration);
            rbComponent.AddForce(actionForce.Direction * actionForce.Magnitude, actionForce.Mode);

            Debug.Log(isGrounded ? "Performed " + actionForce.Name + " on the ground" : "Performed " + currentForce.Name + " on the air");
        }
        previousForce = currentForce;
    }

    /* TriggerPause pauses the game if the pause button has been pressed
     */

    private void TriggerPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            EventManager.current.Pause();
        }
    }

    // TriggerInventory opens the inventory if the inventory button has been pressed
    private void TriggerInventory(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            EventManager.current.OpenMenu(1);
        }
    }

    // TriggerZoomIn zooms in to the next level of zoom is possible
    private void TriggerCameraZoomIn(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            EventManager.current.CameraZoomIn();
        }
    }

    // TriggerZoomOut zooms out to the next level of zoom is possible
    private void TriggerCameraZoomOut(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            EventManager.current.CameraZoomOut();
        }
    }

        // TriggerMenu Cancel cancels current menu if possible
    private void TriggerMenuCancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            EventManager.current.Back();
        }
    }

    #endregion

    #region Player Movement
    private void BetterMovePlayer()
    {
        Vector3 newMoveDirection = new Vector3(horizontalDirection.x, 0f, horizontalDirection.y);

        float velocityDot = Vector3.Dot(newMoveDirection, goalHorizontalVelocity);
        float accel = acceleration * accelerationFactorFromDot.Evaluate(velocityDot);

        maxMoveSpeed = isRunning ? data.RunSpeed : data.WalkSpeed;

        if (isGrounded)
        {
            maxHorizontalVelocity = maxMoveSpeed * newMoveDirection;
            onAirMaxMoveSpeed = maxMoveSpeed;
        }
        else
        {
            maxHorizontalVelocity = onAirMaxMoveSpeed * newMoveDirection;
            onAirMaxMoveSpeed = Mathf.Max(onAirMaxMoveSpeed - onAirMoveFactor, 0);
        }

        goalHorizontalVelocity = Vector3.MoveTowards(goalHorizontalVelocity, maxHorizontalVelocity, accel * Time.fixedDeltaTime);

        Vector3 neededAccel = (goalHorizontalVelocity - new Vector3(rbComponent.linearVelocity.x, 0f, rbComponent.linearVelocity.z)) / Time.fixedDeltaTime;
        float maxAccel = maxAccelerationForce * maxAccelerationForceFactorFromDot.Evaluate(velocityDot);
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        if (currentForce.AllowsMovement)
        {
            rbComponent.AddForce(Vector3.Scale(neededAccel * rbComponent.mass, horizontalForceScale));

            if (horizontalDirection.magnitude > 0)
                transform.rotation = Quaternion.LookRotation(newMoveDirection);

            animationComponent.SetMoveAnimation
            (   
                horizontalDirection.x,
                horizontalDirection.y,
                horizontalDirection.magnitude > 0,
                isRunning
            );
        }
        else
            animationComponent.SetMoveAnimation(0f, 0f, false, false);

        animationComponent.SetDirection(transform.eulerAngles.y);

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if(!isGrounded && currentForce.AllowsGravity) // Apply gravity only if the player isn't grounded and if the current force allows it
        {
            if (isJumping && !isFalling)
                rbComponent.AddForce(data.InitialPressedGravityFactor * Physics.gravity.y * Vector3.up); // First half of the jump and still pressing jump

            else if (!isJumping && !isFalling)
                rbComponent.AddForce(data.InitialReleasedGravityFactor * Physics.gravity.y * Vector3.up); // First half of the jump and stopped pressing jump

            else if (isJumping && isFalling)
                rbComponent.AddForce(data.LatterPressedGravityFactor * Physics.gravity.y * Vector3.up); // Second half of the jump and still pressing jump

            else if (!isJumping && isFalling)
                rbComponent.AddForce(data.LatterReleasedGravityFactor * Physics.gravity.y * Vector3.up); // Second half of the jump and stopped pressing jump
        }
    }
    #endregion

    #region Testing

    private void ApplyDamage()
    {
        data.Health--;
    }

    private void IncreaseMaxHealth()
    {
        data.MaxHealth += 4;
    }
    private void IncreaseCurrentHealth()
    {
        data.Health++;
    }

    #endregion

    #region Saving & Loading

    public void SaveData(GameData data)
    {
        this.data.SaveData(data);

        data.position = transform.position;
        data.rotation = transform.rotation;
    }

    public void LoadData(GameData data)
    {
        this.data.LoadData(data);

        transform.position = data.position;
        transform.rotation = data.rotation;
    }

    #endregion

    private void OnDrawGizmos()
    {
        
    }
}