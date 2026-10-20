using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Reads and writes one GameData JSON file per save profile at <root>/<profileId>/<fileName>.
public class FileDataHandler
{
    // Not encryption. XOR against a key compiled into the binary, which deters casual
    // save-file editing and nothing else. Do not store anything sensitive here.
    private const string ObfuscationKey = "among_us";

    private readonly string root;
    private readonly string fileName;
    private readonly bool useObfuscation;

    public FileDataHandler(string root, string fileName, bool useObfuscation)
    {
        this.root = root;
        this.fileName = fileName;
        this.useObfuscation = useObfuscation;
    }

    public GameData Load(string profileId)
    {
        string path = PathFor(profileId);
        if (path == null || !File.Exists(path)) return null;

        try
        {
            return JsonUtility.FromJson<GameData>(Obfuscate(File.ReadAllText(path)));
        }
        catch (Exception e)
        {
            // A corrupt save must not take the game down: the profile reads as empty and
            // the error says which file to inspect.
            Debug.LogError($"Could not load save {path}\n{e}");
            return null;
        }
    }

    public void Save(GameData data, string profileId)
    {
        string path = PathFor(profileId);
        if (path == null) return;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Obfuscate(JsonUtility.ToJson(data, true)));
        }
        catch (Exception e)
        {
            // Same reasoning as Load: losing one save is bad, crashing mid-scene-change is worse.
            Debug.LogError($"Could not write save {path}\n{e}");
        }
    }

    public Dictionary<string, GameData> LoadAllDataProfiles()
    {
        // No directory means no saves yet, which is the normal first-run state.
        // DataManager.Awake reaches here before anything is written, so this must not throw.
        if (!Directory.Exists(root)) return new Dictionary<string, GameData>();

        // Folders without a readable save are skipped silently: persistentDataPath also
        // holds Unity's own folders, which used to log a warning on every launch.
        return new DirectoryInfo(root).EnumerateDirectories()
            .Select(dir => (id: dir.Name, data: Load(dir.Name)))
            .Where(profile => profile.data != null)
            .ToDictionary(profile => profile.id, profile => profile.data);
    }

    public string GetMostRecentDataProfileID() =>
        LoadAllDataProfiles()
            .OrderByDescending(profile => DateTime.FromBinary(profile.Value.lastUpdated))
            .Select(profile => profile.Key)
            .FirstOrDefault();

    private string PathFor(string profileId) =>
        profileId == null ? null : Path.Combine(root, profileId, fileName);

    // XOR is its own inverse, so the same call obfuscates on save and restores on load.
    private string Obfuscate(string text) =>
        useObfuscation
            ? new string(text.Select((c, i) => (char)(c ^ ObfuscationKey[i % ObfuscationKey.Length])).ToArray())
            : text;
}
