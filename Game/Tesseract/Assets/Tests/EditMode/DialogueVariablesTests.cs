using Ink.Runtime;
using NUnit.Framework;

// Compiles small Ink sources at test time, so these run against the real Ink runtime rather
// than hand-written JSON that could drift from what the Ink compiler produces.
public class DialogueVariablesTests
{
    // Every conversation declares the globals, the same way the game's .ink files include them.
    private const string Globals = "VAR met_gauss = false\nVAR coins = 0\n";

    private static string Compile(string ink) => new Ink.Compiler(ink).Compile().ToJson();

    private static Story Conversation(string body) => new Story(Compile(Globals + body));

    private static DialogueVariables Fresh(string saved = null) => new DialogueVariables(Compile(Globals), saved);

    private static void Play(DialogueVariables globals, Story conversation)
    {
        globals.StartListening(conversation);
        conversation.ContinueMaximally();
        globals.StopListening(conversation);
    }

    // The behaviour players notice: an NPC remembering a choice made with a different NPC.
    [Test]
    public void A_global_changed_in_one_conversation_is_seen_by_the_next()
    {
        DialogueVariables globals = Fresh();
        Play(globals, Conversation("~ met_gauss = true\nHello."));

        Story next = Conversation("{met_gauss: Again!|First time.}");
        globals.StartListening(next);

        Assert.AreEqual("Again!", next.Continue().Trim());
    }

    // Saves store this JSON, so a reload must restore exactly what was played.
    [Test]
    public void Saved_state_restores_into_a_fresh_instance()
    {
        DialogueVariables globals = Fresh();
        Play(globals, Conversation("~ coins = 5\nHere."));

        DialogueVariables restored = Fresh(globals.SaveVariables());
        Story next = Conversation("{coins}");
        restored.StartListening(next);

        Assert.AreEqual("5", next.Continue().Trim());
    }

    [Test]
    public void A_conversations_own_variables_are_not_tracked()
    {
        DialogueVariables globals = Fresh();
        Play(globals, new Story(Compile(Globals + "VAR local = 1\n~ local = 2\nHi.")));

        StringAssert.DoesNotContain("local", globals.SaveVariables());
    }

    // Once a conversation ends, later changes to its Story must not reach the globals.
    [Test]
    public void A_conversation_stops_reporting_after_StopListening()
    {
        DialogueVariables globals = Fresh();
        Story talk = Conversation("~ coins = 9\nHi.");
        globals.StartListening(talk);
        globals.StopListening(talk);
        talk.ContinueMaximally();

        Story next = Conversation("{coins}");
        globals.StartListening(next);

        Assert.AreEqual("0", next.Continue().Trim());
    }
}
