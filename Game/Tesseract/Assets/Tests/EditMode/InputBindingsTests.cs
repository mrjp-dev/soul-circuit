using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

// InputTestFixture swaps in an isolated input system, so the virtual keyboard pressed here
// cannot leak into the editor's real devices or into other tests.
public class InputBindingsTests : InputTestFixture
{
    private InputActionAsset asset;
    private Keyboard keyboard;
    private InputBindings bindings;
    private int presses;

    public override void Setup()
    {
        base.Setup();
        keyboard = InputSystem.AddDevice<Keyboard>();
        asset = ScriptableObject.CreateInstance<InputActionAsset>();
        asset.AddActionMap("Player").AddAction("Jump", binding: "<Keyboard>/space");
        asset.Enable();
        presses = 0;
        bindings = new InputBindings(asset, ("Jump", _ => presses++));
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(asset);
        base.TearDown();
    }

    // Exactly once: a duplicate subscription is what made handlers stack after scene reloads.
    [Test]
    public void An_enabled_binding_runs_its_handler_once_per_press()
    {
        bindings.Enable();
        Press(keyboard.spaceKey);

        Assert.AreEqual(1, presses);
    }

    // Disable must remove the same delegate Enable added, or the handler outlives its component.
    [Test]
    public void A_disabled_binding_no_longer_runs()
    {
        bindings.Enable();
        bindings.Disable();
        Press(keyboard.spaceKey);

        Assert.AreEqual(0, presses);
    }
}
