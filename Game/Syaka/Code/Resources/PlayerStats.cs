using Godot;
using System;


public partial class PlayerData : Resource
{
    #region Variables

    [ExportCategory("Player Attributes")]
    [Export] private float health; // Current health of the player clamped between 0 and maximum health, if it's 0, the player dies
    [Export] private float maxHealth; // Maximum health of the player

    [ExportCategory("Player Horizontal Movement")]
    // Walking and running speeds, on which move speed depends upon as player switches between walking and running
    [Export] private float walkSpeed;
    [Export] private float runSpeed;

    [Export] private float runBoost; // Run speed modifier, how much the run speed increases in relation to the walk speed

    [ExportCategory("Player Vertical Movement")]
    [Export] private float jumpSpeed; // Initial jump speed applied with AddForce() when a jump is first triggered
    [Export] private float jumpBoost; // Jump speed modifier applied when the player jumps while running

    [Export] private float initialPressedGravityFactor; // Gravity modifier in the first half of the jump while the jump button is being pressed
    [Export] private float initialReleasedGravityFactor; // Gravity modifier in the first half of the jump after the jump button has been released
    [Export] private float latterPressedGravityFactor; // Gravity modifier in the second half of the jump while the jump button is being pressed
    [Export] private float latterReleasedGravityFactor; // Gravity modifier in the second half of the jump after the jump button has been released

    /* Original values for the "perfect jump"
     * jumpSpeed = 20f;
     * initialPressedFallFactor = 2f;
     * initialReleasedFallFactor = 5.5f * initialPressedFallFactor;
     * latterPressedFallFactor = 2.5f * initialPressedFallFactor;
     * latterReleasedFallFactor = 3.5f * initialPressedFallFactor;
     */

    #endregion

    #region Variable Properties

    public float WalkSpeed // Everytime the walk speed is changed, the run speed is affected as well
    {
        get => walkSpeed;
        set
        {
            walkSpeed = value;
            runSpeed = runBoost * value;
        }
    }
    public float RunSpeed { get => runSpeed; }
    public float JumpSpeed { get => jumpSpeed; set => jumpSpeed = value; }
    public float InitialPressedGravityFactor // all the gravity factors depend on initialPressedGravityFactor by a fixed proportion
    {
        get => initialPressedGravityFactor;
        set
        {
            initialPressedGravityFactor = value;
            initialReleasedGravityFactor = 5.5f * value;
            latterPressedGravityFactor = 2.5f * value;
            latterReleasedGravityFactor = 3.5f * value;
        }
    }
    public float InitialReleasedGravityFactor { get => initialReleasedGravityFactor; }
    public float LatterPressedGravityFactor { get => latterPressedGravityFactor; }
    public float LatterReleasedGravityFactor { get => latterReleasedGravityFactor; }

    // health setter clamps the health between and including 0 and maxHealth
    public float Health
    {
        get => health;

        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            //EventManager.current.PlayerDrawHealth(); // Draw health UI every time health is modified
            //if (health <= 0f);
                //EventManager.current.PlayerDeath(); // Trigger player's death if health's value is 0
        }
    }
    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            //EventManager.current.PlayerDrawHealth(); // Draw health UI every time maximum health is modified
        }
    }
    public float JumpBoost { get => jumpBoost; set => jumpBoost = value; }
    public float RunBoost { get => runBoost; set => runBoost = value; }

    #endregion

    #region Initialization

	public PlayerData() : this(0, 0, 0, 0, 0, 0) {}
	public PlayerData(
		float health,
		float maxHealth,
		float walkSpeed,
		float runBoost,
		float jumpBoost,
		float initialPressedGravityFactor
	)
	{
		Health = health;
		MaxHealth = maxHealth;
		WalkSpeed = walkSpeed;
		RunBoost = runBoost;
		JumpBoost = jumpBoost;
		InitialPressedGravityFactor = initialPressedGravityFactor;
	}
    public void InitializeDataIfNull() // Default values
    {
        if(walkSpeed == 0)
        {
            //Debug.LogWarning("Data Object is null, initializing with default values");
            RunBoost = 1.5f;
            WalkSpeed = 7f;

            JumpSpeed = 14f;
            JumpBoost = 1.3f;

            InitialPressedGravityFactor = 2f;
        }
    }

    public void RestoreStats()
    {
        Health = maxHealth;
    }

    #endregion

    #region Saving & Loading
	/*
    public void SaveData(GameData data)
    {
        data.maxHealth = maxHealth;
        data.health = health;

        data.walkSpeed = walkSpeed;
        data.jumpSpeed = JumpSpeed;
        data.jumpBoost = jumpBoost;

        data.initialPressedGravityFactor = initialPressedGravityFactor;
    }

    public void LoadData(GameData data)
    {
        maxHealth = data.maxHealth;
        health = data.health;

        runBoost = data.runBoost;
        WalkSpeed = data.walkSpeed;

        jumpSpeed = data.jumpSpeed;
        jumpBoost = data.jumpBoost;

        InitialPressedGravityFactor = data.initialPressedGravityFactor;
    }
	*/
    #endregion
}