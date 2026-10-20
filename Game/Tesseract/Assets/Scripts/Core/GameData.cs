using UnityEngine;

// Everything one save profile stores. Stats are saved, not just health, because equipment,
// items and permanent upgrades will raise them. Field initializers are the new-game defaults.
[System.Serializable]
public class GameData
{
    public int maxHealth = 12;
    public int health = 12;

    public float walkSpeed = 7f;
    public float runBoost = 1.5f;
    public float jumpSpeed = 14f;
    public float jumpBoost = 1.3f;
    public float initialPressedGravityFactor = 2f;

    public string lastActiveSceneName = "Main"; // no "Main" scene yet: start menu is unbuilt

    public long lastUpdated; // DateTime.ToBinary of the last save
    public float totalPlaytime; // seconds

    public string inkVariables; // Ink global variable state as JSON, null on a new game

    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
}
