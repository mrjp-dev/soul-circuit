using UnityEngine;

// Anything in the world the player can approach but does not control. Each subclass exposes
// its own data asset through Data; this class only holds what every kind shares.
public abstract class NonPlayable : MonoBehaviour
{
    public abstract NPData Data { get; }

    // PlayerInteraction checks Unity's built-in tag, so it must follow the data. Unused so far.
    public void ToggleInteractable(bool state)
    {
        Data.IsInteractable = state;
        gameObject.tag = state ? "Interactable" : "Untagged";
    }
}
