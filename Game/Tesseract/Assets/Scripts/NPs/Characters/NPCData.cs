using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Scriptable Object/NPC Data")]
public class NPCData : NPData
{
    [SerializeField] protected bool isHostile; // toggle for aggressiveness
    [SerializeField] protected bool isFriendly; // toggle for allegiance
    [SerializeField] protected bool isNeutral; // toggle for non-aggressiveness

    private float health; // NPC's health, if it reaches 0 the NPC dies
    private float maxHealth; // NPC's maximum health, dictates how much health the npc can have
    public float Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            if (health <= 0)
                Debug.Log(npName + " died!");
        }
    }
    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
        }
    }

    public bool IsHostile { get => isHostile; set => isHostile = value; }
    public bool IsFriendly { get => isFriendly; set => isFriendly = value; }
    public bool IsNeutral { get => isNeutral; set => isNeutral = value; }
}
