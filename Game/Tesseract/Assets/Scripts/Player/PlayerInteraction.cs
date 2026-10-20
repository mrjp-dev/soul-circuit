using UnityEngine;
using UnityEngine.InputSystem;

// Remembers the interactable the player is standing next to and acts on it when Interact is
// pressed. Unity's own tag says whether something can be approached ("Interactable"); its
// CustomTag says what kind of thing it is ("Item" is picked up, anything else talks).
//
// Collider_Hurtbox and Collider_Hitbox on the player are combat scaffolding. They stay
// unbound until combat lands; resolve them by child name, never by index.
public class PlayerInteraction : MonoBehaviour
{
    private GameObject current;
    private InputBindings bindings;

    private void Awake() =>
        bindings = new InputBindings(GetComponent<PlayerInput>().actions, ("Interact", _ => Interact()));

    private void OnEnable() => bindings.Enable();

    private void OnDisable() => bindings.Disable();

    private void OnTriggerEnter(Collider other) => OnTriggerStay(other);

    // Only interactables are remembered. The original also cleared the target on contact with
    // any other collider, so interaction depended on the order Unity reported contacts in.
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Interactable")) current = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == current) current = null;
    }

    private void Interact()
    {
        if (current == null) return;

        if (current.GetComponent<CustomTag>().HasTag("Item"))
            Debug.LogWarning("Item pickup is not implemented yet.");
        else
            DialogueManager.Current.StartDialogue(current);
    }
}
