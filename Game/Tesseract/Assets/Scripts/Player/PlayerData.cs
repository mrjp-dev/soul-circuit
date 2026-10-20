using UnityEngine;

// The player's tuning and health, as an asset so values survive play mode and are shared
// by every scene. Initializers are the defaults for a newly created asset.
[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Player Attributes")]
    [SerializeField] private int health = 12; // clamped to 0..maxHealth; 0 means dead
    [SerializeField] private int maxHealth = 12;

    [Header("Player Horizontal Movement")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float runBoost = 1.5f; // run speed = walk speed x this

    [Header("Player Vertical Movement")]
    [SerializeField] private float jumpSpeed = 14f; // impulse applied on take-off
    [SerializeField] private float jumpBoost = 1.3f; // jump impulse multiplier while running

    // Gravity multiplier for the rising half of a jump while jump is held. The other three
    // phases scale from it by fixed ratios, so this one number tunes the whole arc.
    // The original "perfect jump" used jumpSpeed 20 with this at 2.
    [SerializeField] private float initialPressedGravityFactor = 2f;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runBoost * walkSpeed;
    public float JumpSpeed => jumpSpeed;
    public float JumpBoost => jumpBoost;

    // Releasing jump early pulls the arc down hardest (5.5x), which is what turns a short
    // tap into a short hop. These were stored fields that only setters updated, so editing
    // the base factor in the Inspector silently left them stale.
    public float InitialPressedGravityFactor => initialPressedGravityFactor;
    public float InitialReleasedGravityFactor => 5.5f * initialPressedGravityFactor;
    public float LatterPressedGravityFactor => 2.5f * initialPressedGravityFactor;
    public float LatterReleasedGravityFactor => 3.5f * initialPressedGravityFactor;

    public int Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            EventManager.Current.PlayerDrawHealth();
            if (health == 0) EventManager.Current.PlayerDeath();
        }
    }

    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            EventManager.Current.PlayerDrawHealth();
        }
    }

    public void RestoreStats() => Health = maxHealth;

    // Stats travel with the save so upgrades persist. In the editor this writes into the asset
    // itself (ScriptableObjects keep play-mode changes), so loading a save also changes tuning.
    // The original loaded runBoost but never saved it; all five now go both ways.
    public void SaveData(GameData data)
    {
        data.maxHealth = maxHealth;
        data.health = health;
        data.walkSpeed = walkSpeed;
        data.runBoost = runBoost;
        data.jumpSpeed = jumpSpeed;
        data.jumpBoost = jumpBoost;
        data.initialPressedGravityFactor = initialPressedGravityFactor;
    }

    public void LoadData(GameData data)
    {
        maxHealth = data.maxHealth;
        health = data.health;
        walkSpeed = data.walkSpeed;
        runBoost = data.runBoost;
        jumpSpeed = data.jumpSpeed;
        jumpBoost = data.jumpBoost;
        initialPressedGravityFactor = data.initialPressedGravityFactor;
    }
}
