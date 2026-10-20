using UnityEngine;
using UnityEngine.InputSystem;

// Pause, inventory, camera zoom and menu cancel. They share the player's PlayerInput because
// that is where the bindings live, but they drive menus and the camera, not the character,
// so they stay out of PlayerControl. One row per action: adding a system action is one line.
public class SystemInput : MonoBehaviour
{
    private InputBindings bindings;

    private void Awake() =>
        bindings = new InputBindings(GetComponent<PlayerInput>().actions,
            ("MenuOptions", _ => EventManager.Current.Pause()),
            ("MenuInventory", _ => EventManager.Current.OpenMenu((int)MenuId.Inventory)),
            ("ZoomIn", _ => EventManager.Current.CameraZoomIn()),
            ("ZoomOut", _ => EventManager.Current.CameraZoomOut()),
            ("Cancel", _ => EventManager.Current.Back()));

    private void OnEnable() => bindings.Enable();

    private void OnDisable() => bindings.Disable();
}
