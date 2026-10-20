using UnityEngine;

// Data for non-playable characters. Allegiance is three flags because that is how the NPC
// assets already store it; NonPlayableCharacter keeps them exclusive. Health is combat
// scaffolding and runtime-only, so it is not serialized.
[CreateAssetMenu(fileName = "NPCData", menuName = "Scriptable Object/NPC Data")]
public class NPCData : NPData
{
    [SerializeField] private bool isHostile;
    [SerializeField] private bool isFriendly;
    [SerializeField] private bool isNeutral;

    private float health;
    private float maxHealth;

    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            if (health == 0) Debug.LogWarning($"{NpName} died. NPC death is not implemented yet.");
        }
    }

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public bool IsHostile { get => isHostile; set => isHostile = value; }
    public bool IsFriendly { get => isFriendly; set => isFriendly = value; }
    public bool IsNeutral { get => isNeutral; set => isNeutral = value; }
}
