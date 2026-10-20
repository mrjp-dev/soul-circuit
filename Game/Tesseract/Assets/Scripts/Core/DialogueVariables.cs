using System.Collections.Generic;
using Ink.Runtime;

// Ink global variables shared by every conversation. Each conversation is its own Story with
// its own copy of the globals, so listening copies its changes back here; that is what lets a
// choice made talking to one NPC show up when talking to the next, and in saves.
public class DialogueVariables
{
    private readonly Dictionary<string, Object> variables = new Dictionary<string, Object>();
    private readonly Story globals;

    public DialogueVariables(string globalsJson, string savedState)
    {
        globals = new Story(globalsJson);
        if (!string.IsNullOrEmpty(savedState)) globals.state.LoadJson(savedState);

        foreach (string name in globals.variablesState)
            variables.Add(name, globals.variablesState.GetVariableWithName(name));
    }

    public void StartListening(Story conversation)
    {
        CopyInto(conversation);
        conversation.variablesState.variableChangedEvent += OnChanged;
    }

    public void StopListening(Story conversation) =>
        conversation.variablesState.variableChangedEvent -= OnChanged;

    public string SaveVariables()
    {
        CopyInto(globals);
        return globals.state.ToJson();
    }

    // Only declared globals are tracked, so a conversation's own variables never leak into saves.
    private void OnChanged(string name, Object value)
    {
        if (variables.ContainsKey(name)) variables[name] = value;
    }

    private void CopyInto(Story story)
    {
        foreach (KeyValuePair<string, Object> variable in variables)
            story.variablesState.SetGlobal(variable.Key, variable.Value);
    }
}
