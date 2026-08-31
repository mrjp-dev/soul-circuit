using System.IO;
using NUnit.Framework;

public class FileDataHandlerTests
{
    private string dir;

    [SetUp]
    public void SetUp() =>
        dir = Path.Combine(Path.GetTempPath(), "soul-circuit-tests", Path.GetRandomFileName());

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
    }

    // DataManager.Awake calls GetMostRecentDataProfileID before anything has been saved,
    // so on a first run the save directory does not exist yet. Load guards with
    // File.Exists and Save creates the directory, but LoadAllDataProfiles enumerated
    // the directory unguarded, which threw out of Awake.
    [Test]
    public void Most_recent_profile_is_null_when_no_saves_exist()
    {
        var handler = new FileDataHandler(dir, "save.json", false);

        Assert.IsNull(handler.GetMostRecentDataProfileID());
    }
}
