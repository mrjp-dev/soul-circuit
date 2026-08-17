using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


public class DataManager : MonoBehaviour
{
    [Header("File Storage Settings")]
    [SerializeField] private string fileName;
    [SerializeField] private bool usingEncryption;

    [Header("Settings For Testing")]
    [SerializeField] private bool disableDataPersistence = false;

    private GameData gameData;
    private List<IDataPersistence> dataPersistentObjects;
    private FileDataHandler dataHandler;
    private string currentDataProfileID = "";

    public static DataManager Current { get; private set; }
    public string CurrentDataProfileID { get => currentDataProfileID; }

    public GameData GetGameData()
    {
        return gameData;
    }

    private void Awake()
    {
        if (disableDataPersistence)
            Debug.LogWarning("Data persistence is disabled, no data will be loaded or saved!");

        if (Current == null)
            Current = this;
        else
        {
            Debug.LogWarning("Data Manager instance already exists, newest one will be destroyed");
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, usingEncryption);
        this.currentDataProfileID = dataHandler.GetMostRecentDataProfileID();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void SaveGame()
    {
        if (disableDataPersistence)
            return;

        if (this.gameData == null)
        {
            EventManager.Current.NoSaveData();
            Debug.LogWarning("No data was found, please start a new game before trying to save");
            return;
        }

        foreach (IDataPersistence dataPersistentObj in dataPersistentObjects)
        {
            dataPersistentObj.SaveData(gameData);
        }

        gameData.lastActiveSceneIndex = SceneManager.GetActiveScene().buildIndex;
        gameData.lastActiveSceneName = SceneManager.GetActiveScene().name;

        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        dataHandler.Save(gameData, currentDataProfileID);

        EventManager.Current.GameSaved();
    }

    public void LoadGame()
    {
        if (disableDataPersistence)
        {
            EventManager.Current.NoDataPersistence();
            return;
        }

        this.gameData = dataHandler.Load(currentDataProfileID);

        if(this.gameData == null)
        {
            EventManager.Current.NoLoadData();
            Debug.LogWarning("No data was found, please start a new game before trying to load");
            return;
        }

        foreach(IDataPersistence dataPersistentObj in dataPersistentObjects)
        {
            dataPersistentObj.LoadData(gameData);
        }
        EventManager.Current.GameLoaded();
    }

    public void LoadWithDataProfileID(string dataProfileID)
    {
        currentDataProfileID = dataProfileID;
        LoadGame();
    }

    public List<IDataPersistence> FindAllDataPersistentObjects()
    {
        IEnumerable<IDataPersistence> dataPersistentObjects = FindObjectsByType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistentObjects);
    }

    public Dictionary<string, GameData> GetAllDataProfiles()
    {
        return dataHandler.LoadAllDataProfiles();
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistentObjects = FindAllDataPersistentObjects();

        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        SaveGame();
    }

    private void OnApplicationQuit()
    {
        //SaveGame();
    }
}
