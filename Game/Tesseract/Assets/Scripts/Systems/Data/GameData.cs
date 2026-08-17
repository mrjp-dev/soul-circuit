using UnityEngine;


[System.Serializable]
public class GameData
{
    // PlayerData
    public int maxHealth;
    public int health;

    public float walkSpeed;
    public float runBoost;
    public float jumpSpeed;
    public float jumpBoost;
    public float initialPressedGravityFactor;

    public string lastActiveSceneName;
    public int lastActiveSceneIndex;

    // GameStatistics
    public long lastUpdated;
    public float totalPlaytime;

    public string inkVariables;

    // PlayerControl
    public Vector3 position;
    public Quaternion rotation;

    public GameData()
    {
        totalPlaytime = 0f;

        maxHealth = 12;
        health = maxHealth;

        walkSpeed = 7.0f;
        runBoost = 1.5f;
        jumpSpeed = 14f;
        jumpBoost = 1.3f;
        initialPressedGravityFactor = 2f;

        lastActiveSceneName = "Main";
        lastActiveSceneIndex = 1;

        position = Vector3.zero;
        rotation = Quaternion.identity;

        inkVariables = null;
    }
}
