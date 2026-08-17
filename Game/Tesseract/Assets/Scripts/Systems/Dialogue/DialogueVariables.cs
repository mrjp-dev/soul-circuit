using Ink.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueVariables
{
    public Dictionary<string, Ink.Runtime.Object> variables { get; private set; }
    private Story globalVariables;

    public DialogueVariables(TextAsset loadGVariablesJSON, string storedVariables)
    {
        globalVariables = new Story(loadGVariablesJSON.text);

        if (!string.IsNullOrEmpty(storedVariables))
            globalVariables.state.LoadJson(storedVariables);
        else
            Debug.Log("No stored global variables found, using defaults!");

        variables = new Dictionary<string, Ink.Runtime.Object>();

        foreach (string name in globalVariables.variablesState)
        {
            Ink.Runtime.Object value = globalVariables.variablesState.GetVariableWithName(name);
            variables.Add(name, value);
            //Debug.Log("Initialized global dialogue variable: " + name + " = " + value);
        }
    }

    public void StartListening(Story dialogue)
    {
        VariablesToStory(dialogue);
        dialogue.variablesState.variableChangedEvent += GlobalVariableChanged;
    }
    public void StopListening(Story dialogue)
    {
        dialogue.variablesState.variableChangedEvent -= GlobalVariableChanged;
    }
    private void GlobalVariableChanged(string name, Ink.Runtime.Object value)
    {
        if (variables.ContainsKey(name))
        {
            variables.Remove(name);
            variables.Add(name, value);
        }
    }

    public void VariablesToStory(Story dialogue)
    {
        foreach (KeyValuePair<string, Ink.Runtime.Object> variable in variables)
        {
            dialogue.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }

    public string SaveVariables()
    {
        if (globalVariables != null)
        {
            VariablesToStory(globalVariables);
            return globalVariables.state.ToJson();
        }
        else
            return null;
    }

    public void LoadVariables(string inkVariables)
    {
        globalVariables.state.LoadJson(inkVariables);
    }
}
