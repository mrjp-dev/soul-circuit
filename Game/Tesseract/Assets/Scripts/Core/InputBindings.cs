using System;
using UnityEngine.InputSystem;

// Subscribes a component's input actions as one set, so OnEnable and OnDisable cannot drift
// apart. InputActions live on the PlayerInput asset, not on the GameObject, so a handler left
// attached survives the component's destruction and fires again after the next scene load.
public sealed class InputBindings
{
    private readonly (InputAction action, Action<InputAction.CallbackContext> handler)[] pairs;

    public InputBindings(InputActionAsset actions,
                         params (string name, Action<InputAction.CallbackContext> handler)[] map)
    {
        pairs = Array.ConvertAll(map, m => (actions[m.name], m.handler));
    }

    public void Enable()
    {
        foreach (var (action, handler) in pairs) action.performed += handler;
    }

    public void Disable()
    {
        foreach (var (action, handler) in pairs) action.performed -= handler;
    }
}
