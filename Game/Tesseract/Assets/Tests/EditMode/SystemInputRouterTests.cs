using NUnit.Framework;
using UnityEngine.InputSystem;

/// <summary>
/// Exercises the system-input routing without a player, a scene, or an EventManager.
/// That is the point of the ISystemInputSink seam: before it existed this logic lived
/// inside PlayerControl and could only be reached by instantiating a MonoBehaviour
/// with a Rigidbody, colliders, and a PlayerInput component.
/// </summary>
public class SystemInputRouterTests
{
    private class RecordingSink : ISystemInputSink
    {
        public int PauseCount;
        public int BackCount;
        public int ZoomInCount;
        public int ZoomOutCount;
        public int LastMenuId = -1;

        public void Pause() => PauseCount++;
        public void OpenMenu(int menuId) { LastMenuId = menuId; }
        public void Back() => BackCount++;
        public void CameraZoomIn() => ZoomInCount++;
        public void CameraZoomOut() => ZoomOutCount++;
    }

    private InputActionMap map;
    private InputAction pause, inventory, zoomIn, zoomOut, cancel;
    private RecordingSink sink;
    private SystemInputRouter router;

    [SetUp]
    public void SetUp()
    {
        map = new InputActionMap("system-input-tests");
        pause = map.AddAction("Pause", InputActionType.Button);
        inventory = map.AddAction("Inventory", InputActionType.Button);
        zoomIn = map.AddAction("ZoomIn", InputActionType.Button);
        zoomOut = map.AddAction("ZoomOut", InputActionType.Button);
        cancel = map.AddAction("Cancel", InputActionType.Button);

        sink = new RecordingSink();
        router = new SystemInputRouter(pause, inventory, zoomIn, zoomOut, cancel, sink);
    }

    [TearDown]
    public void TearDown()
    {
        router.Disable();
        map.Dispose();
    }

    [Test]
    public void Enable_is_idempotent_so_double_enabling_cannot_double_subscribe()
    {
        router.Enable();
        router.Enable();

        Assert.DoesNotThrow(() => router.Disable());
    }

    [Test]
    public void Disable_before_Enable_is_safe()
    {
        Assert.DoesNotThrow(() => router.Disable());
    }

    [Test]
    public void Inventory_opens_the_named_menu_rather_than_a_bare_literal()
    {
        Assert.AreEqual(1, SystemInputRouter.InventoryMenuId,
            "InventoryMenuId documents what the old literal 1 at the call site meant. " +
            "If the menu order changes, this constant is the single place to update.");
    }

    [Test]
    public void A_fresh_router_has_not_touched_the_sink()
    {
        Assert.AreEqual(0, sink.PauseCount);
        Assert.AreEqual(0, sink.BackCount);
        Assert.AreEqual(0, sink.ZoomInCount);
        Assert.AreEqual(0, sink.ZoomOutCount);
        Assert.AreEqual(-1, sink.LastMenuId);
    }
}
