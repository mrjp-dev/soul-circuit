using UnityEngine;

// Shared data for every non-playable: name, Ink dialogue, interactability. An asset rather
// than component fields so several scene objects can share one definition.
public class NPData : ScriptableObject
{
    [SerializeField] private string npName;
    [SerializeField] private TextAsset dialogue; // compiled Ink JSON, played on interaction
    [SerializeField] private bool isInteractable;

    public string NpName => npName;
    public TextAsset Dialogue => dialogue;
    public bool IsInteractable { get => isInteractable; set => isInteractable = value; }
}
