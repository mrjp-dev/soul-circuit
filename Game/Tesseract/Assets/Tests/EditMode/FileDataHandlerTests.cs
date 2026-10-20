using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Real files in a throwaway folder: the handler's whole job is the filesystem, so faking it
// would test nothing.
public class FileDataHandlerTests
{
    private const string FileName = "save.json";
    private string root;

    [SetUp]
    public void SetUp() => root = Path.Combine(Path.GetTempPath(), "SoulCircuitTests", Path.GetRandomFileName());

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private FileDataHandler Handler(bool obfuscate = false) => new FileDataHandler(root, FileName, obfuscate);

    private string SavePath(string profileId) => Path.Combine(root, profileId, FileName);

    [Test]
    public void Most_recent_profile_is_null_when_no_saves_exist()
    {
        Assert.IsNull(Handler().GetMostRecentDataProfileID());
    }

    // Both modes: XOR is symmetric, so a broken key index would still round-trip, and plain
    // mode is what the shipped DataManager prefab actually uses.
    [TestCase(false)]
    [TestCase(true)]
    public void A_saved_profile_loads_back_unchanged(bool obfuscate)
    {
        Handler(obfuscate).Save(new GameData { health = 7, lastActiveSceneName = "Testing" }, "slot1");

        GameData loaded = Handler(obfuscate).Load("slot1");

        Assert.AreEqual(7, loaded.health);
        Assert.AreEqual("Testing", loaded.lastActiveSceneName);
    }

    // Round-tripping alone would pass if Obfuscate were a no-op, so check the bytes on disk.
    [Test]
    public void An_obfuscated_save_is_not_plain_json_on_disk()
    {
        Handler(true).Save(new GameData(), "slot1");

        StringAssert.DoesNotContain("health", File.ReadAllText(SavePath("slot1")));
    }

    // DataManager passes null before any profile exists; it must not create a junk folder.
    [Test]
    public void A_null_profile_id_neither_loads_nor_writes()
    {
        Handler().Save(new GameData(), null);

        Assert.IsNull(Handler().Load(null));
        Assert.IsFalse(Directory.Exists(root));
    }

    [Test]
    public void A_missing_profile_loads_as_null()
    {
        Assert.IsNull(Handler().Load("nope"));
    }

    [Test]
    public void A_corrupt_save_loads_as_null_and_logs_an_error()
    {
        Directory.CreateDirectory(Path.Combine(root, "slot1"));
        File.WriteAllText(SavePath("slot1"), "{ not json");
        LogAssert.Expect(LogType.Error, new Regex("Could not load save"));

        Assert.IsNull(Handler().Load("slot1"));
    }

    [Test]
    public void An_unwritable_save_logs_an_error_instead_of_throwing()
    {
        // A plain file where the profile folder belongs makes CreateDirectory fail.
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "slot1"), "");
        LogAssert.Expect(LogType.Error, new Regex("Could not write save"));

        Handler().Save(new GameData(), "slot1");
    }

    // persistentDataPath also holds Unity's own folders, which must not appear as save slots.
    [Test]
    public void Folders_without_a_save_are_skipped()
    {
        Directory.CreateDirectory(Path.Combine(root, "Unity"));
        Handler().Save(new GameData(), "slot1");

        CollectionAssert.AreEquivalent(new[] { "slot1" }, Handler().LoadAllDataProfiles().Keys);
    }

    // Continue on the start menu resumes this profile, so order by timestamp, not folder name.
    [Test]
    public void The_most_recently_saved_profile_wins()
    {
        Handler().Save(new GameData { lastUpdated = new DateTime(2026, 1, 1).ToBinary() }, "old");
        Handler().Save(new GameData { lastUpdated = new DateTime(2026, 6, 1).ToBinary() }, "new");

        Assert.AreEqual("new", Handler().GetMostRecentDataProfileID());
    }
}
