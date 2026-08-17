using UnityEngine.InputSystem;

/// <summary>
/// Routes input that acts on the game rather than on the player character:
/// pausing, opening the inventory, camera zoom, and cancelling a menu.
///
/// These share the player's PlayerInput component because that is where the
/// bindings live, but none of them are player abilities. Keeping them here means
/// PlayerControl only handles things the character itself does, and it means this
/// routing can be exercised without a player in the scene.
///
/// Deliberately a plain class rather than a MonoBehaviour: it holds no serialized
/// state, so making it a component would add a prefab entry and a lifecycle for no
/// benefit. PlayerControl owns its lifetime and pumps Enable/Disable.
/// </summary>
public class SystemInputRouter
{
    private readonly InputAction pause;
    private readonly InputAction inventory;
    private readonly InputAction cameraZoomIn;
    private readonly InputAction cameraZoomOut;
    private readonly InputAction menuCancel;

    private readonly ISystemInputSink sink;

    private bool subscribed;

    public SystemInputRouter(
        InputAction pause,
        InputAction inventory,
        InputAction cameraZoomIn,
        InputAction cameraZoomOut,
        InputAction menuCancel,
        ISystemInputSink sink)
    {
        this.pause = pause;
        this.inventory = inventory;
        this.cameraZoomIn = cameraZoomIn;
        this.cameraZoomOut = cameraZoomOut;
        this.menuCancel = menuCancel;
        this.sink = sink;
    }

    public void Enable()
    {
        if (subscribed) return;
        subscribed = true;

        pause.performed += OnPause;
        inventory.performed += OnInventory;
        cameraZoomIn.performed += OnCameraZoomIn;
        cameraZoomOut.performed += OnCameraZoomOut;
        menuCancel.performed += OnMenuCancel;
    }

    public void Disable()
    {
        if (!subscribed) return;
        subscribed = false;

        pause.performed -= OnPause;
        inventory.performed -= OnInventory;
        cameraZoomIn.performed -= OnCameraZoomIn;
        cameraZoomOut.performed -= OnCameraZoomOut;
        menuCancel.performed -= OnMenuCancel;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed) sink.Pause();
    }

    private void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed) sink.OpenMenu(InventoryMenuId);
    }

    private void OnCameraZoomIn(InputAction.CallbackContext context)
    {
        if (context.performed) sink.CameraZoomIn();
    }

    private void OnCameraZoomOut(InputAction.CallbackContext context)
    {
        if (context.performed) sink.CameraZoomOut();
    }

    private void OnMenuCancel(InputAction.CallbackContext context)
    {
        if (context.performed) sink.Back();
    }

    /// <summary>
    /// Menu index the inventory lives at. Was an unexplained literal 1 at the call site.
    /// </summary>
    public const int InventoryMenuId = 1;
}

/// <summary>
/// What system input does once it has been recognised. EventManager is the real
/// implementation; the seam exists so the routing above can be tested without one.
/// </summary>
public interface ISystemInputSink
{
    void Pause();
    void OpenMenu(int menuId);
    void Back();
    void CameraZoomIn();
    void CameraZoomOut();
}

/// <summary>
/// Adapts the EventManager singleton to ISystemInputSink.
/// </summary>
public class EventManagerInputSink : ISystemInputSink
{
    public void Pause() => EventManager.Current.Pause();
    public void OpenMenu(int menuId) => EventManager.Current.OpenMenu(menuId);
    public void Back() => EventManager.Current.Back();
    public void CameraZoomIn() => EventManager.Current.CameraZoomIn();
    public void CameraZoomOut() => EventManager.Current.CameraZoomOut();
}
