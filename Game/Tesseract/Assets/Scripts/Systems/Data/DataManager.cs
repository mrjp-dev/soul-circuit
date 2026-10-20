using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

// Owns the current save profile. When a scene loads, every IDataPersistence in it is filled
// from the profile; SaveGame collects them back. Survives scene loads, and every scene carries
// one so it can be played directly: the first instance wins and later ones remove themselves.
public class DataManager : MonoBehaviour
{
    [Header("File Storage Settings")]
    [SerializeField] private string fileName;
    [FormerlySerializedAs("usingEncryption")]
    [SerializeField] private bool useObfuscation; // see FileDataHandler: not encryption

    [Header("Settings For Testing")]
    [SerializeField] private bool disableDataPersistence;

    private FileDataHandler files;
    private GameData gameData;
    private List<IDataPersistence> persistent = new List<IDataPersistence>();
    private float playtimeMark; // Time.unscaledTime when playtime was last banked

    public static DataManager Current { get; private set; }
    public string CurrentDataProfileID { get; private set; }

    public GameData GetGameData() => gameData;

    private void Awake()
    {
        if (Current != null)
        {
            Destroy(gameObject);
            return;
        }

        Current = this;
        DontDestroyOnLoad(gameObject);
        if (disableDataPersistence) Debug.LogWarning("Data persistence is disabled: nothing will be loaded or saved.");

        files = new FileDataHandler(Application.persistentDataPath, fileName, useObfuscation);
        CurrentDataProfileID = files.GetMostRecentDataProfileID();
    }

    // There is deliberately no save on sceneUnloaded: by then the scene's objects are
    // destroyed, so saving there threw. MenuManager.ChangeScene saves before leaving instead.
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    public void NewGame()
    {
        gameData = new GameData();
        playtimeMark = Time.unscaledTime;
    }

    public void SaveGame()
    {
        if (disableDataPersistence) return;
        if (gameData == null)
        {
            Debug.LogWarning("Nothing to save: start a new game first.");
            return;
        }

        foreach (IDataPersistence target in persistent) target.SaveData(gameData);
        gameData.lastActiveSceneName = SceneManager.GetActiveScene().name;
        gameData.lastUpdated = DateTime.Now.ToBinary();

        // Playtime is banked here. GameStatistics used to track it but was never attached
        // to anything, so every save slot showed zero.
        gameData.totalPlaytime += Time.unscaledTime - playtimeMark;
        playtimeMark = Time.unscaledTime;

        files.Save(gameData, CurrentDataProfileID);
    }

    public void LoadGame()
    {
        if (disableDataPersistence) return;

        gameData = files.Load(CurrentDataProfileID);
        playtimeMark = Time.unscaledTime;
        // No profile yet, or an unreadable one: the start menu hides Continue and Load on this.
        if (gameData == null)
        {
            EventManager.Current.NoLoadData();
            return;
        }

        foreach (IDataPersistence target in persistent) target.LoadData(gameData);
        EventManager.Current.GameLoaded();
    }

    public void LoadWithDataProfileID(string dataProfileID)
    {
        CurrentDataProfileID = dataProfileID;
        LoadGame();
    }

    public Dictionary<string, GameData> GetAllDataProfiles() => files.LoadAllDataProfiles();

    // Searched per scene: each scene brings its own player, dialogue and HUD objects.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        persistent = FindObjectsByType<MonoBehaviour>().OfType<IDataPersistence>().ToList();
        LoadGame();
    }
}
