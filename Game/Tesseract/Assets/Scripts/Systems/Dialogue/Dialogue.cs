using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [SerializeField] private TextAsset dialogueJSON;
    public TextAsset DialogueJSON { get => dialogueJSON; }
}