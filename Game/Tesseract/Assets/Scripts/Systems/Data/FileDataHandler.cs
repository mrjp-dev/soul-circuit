using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class FileDataHandler
{
    private string dataFileDirectoryPath = "";
    private string dataFileName = "";

    private bool useEncryption = false;
    private DateTime mostRecentDate;
    private readonly string encryptionCodeWord = "among_us";

    public FileDataHandler(string fileDir, string fileName, bool encrypt)
    {
        dataFileDirectoryPath = fileDir;
        dataFileName = fileName;
        useEncryption = encrypt;
    }

    public GameData Load(string dataProfileID)
    {
        if (dataProfileID == null)
            return null;

        string fullPath = Path.Combine(dataFileDirectoryPath, dataProfileID, dataFileName);
        GameData storedData = null;
        if(File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                using FileStream stream = new FileStream(fullPath, FileMode.Open);
                using StreamReader reader = new StreamReader(stream);

                dataToLoad = reader.ReadToEnd();

                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                storedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return storedData;
    }

    public void Save(GameData data, string dataProfileID)
    {
        if (dataProfileID == null)
            return;

        string fullPath = Path.Combine(dataFileDirectoryPath, dataProfileID, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            using FileStream stream = new FileStream(fullPath, FileMode.Create);
            using StreamWriter writer = new StreamWriter(stream);

            writer.Write(dataToStore);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occurred when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public Dictionary<string, GameData> LoadAllDataProfiles()
    {
        Dictionary<string, GameData> dataProfileDict = new Dictionary<string, GameData>();

        // No directory means no saves yet, which is the normal first-run state.
        // DataManager.Awake reaches here via GetMostRecentDataProfileID before
        // anything has been written, so this must not throw.
        if (!Directory.Exists(dataFileDirectoryPath))
            return dataProfileDict;

        IEnumerable<DirectoryInfo> directoriesInformation = new DirectoryInfo(dataFileDirectoryPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in directoriesInformation)
        {
            string profileDataID = dirInfo.Name;

            string fullPath = Path.Combine(dataFileDirectoryPath, profileDataID, dataFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("This directory does not contain save data profile " + profileDataID + " and will thus be skipped");
                continue;
            }

            GameData profileData = Load(profileDataID);

            if (profileData != null)
            {
                dataProfileDict.Add(profileDataID, profileData);
            }
            else
            {
                Debug.LogError("Error occurred while trying to load data with id: " + profileDataID);
            }

        }

        return dataProfileDict;
    }

    public string GetMostRecentDataProfileID()
    {
        string mostRecentProfileID = null;

        Dictionary<string, GameData> dataProfiles = LoadAllDataProfiles();
        foreach(KeyValuePair<string, GameData> pair in dataProfiles)
        {
            string profileID = pair.Key;
            GameData data = pair.Value;

            if (data == null)
                continue;

            if (mostRecentProfileID == null)
                mostRecentProfileID = profileID;
            else
            {
                DateTime mostRecentDate = DateTime.FromBinary(dataProfiles[mostRecentProfileID].lastUpdated);
                DateTime newDate = DateTime.FromBinary(data.lastUpdated);

                if (newDate > mostRecentDate)
                    mostRecentProfileID = profileID;
            }
        }
        return mostRecentProfileID;
    }

    private string EncryptDecrypt(string dataToProcess)
    {
        string modifiedData = "";
        for(int i = 0; i < dataToProcess.Length; i++)
        {
            modifiedData += (char)(dataToProcess[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }
}
