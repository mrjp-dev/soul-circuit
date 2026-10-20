using UnityEngine;

// Everything one save profile stores. Progress only: feel-tuning values (speeds, jump,
// gravity) live in the PlayerData asset and stay out of saves, otherwise loading an old save
// would silently overwrite the tuning. Field initializers are the new-game defaults.
[System.Serializable]
public class GameData
{
    public int maxHealth = 12;
    public int health = 12;

    public string lastActiveSceneName = "Main"; // no "Main" scene yet: start menu is unbuilt

    public long lastUpdated; // DateTime.ToBinary of the last save
    public float totalPlaytime; // seconds

    public string inkVariables; // Ink global variable state as JSON, null on a new game

    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
}
