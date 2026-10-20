using NUnit.Framework;
using UnityEngine;

// The interrupt rules behind jump, attack and dash. PlayerControl only applies what the
// runner decides, so this covers the feel-relevant logic without a Rigidbody or a scene.
public class ActionForceRunnerTests
{
    private static readonly ActionForce Dash = new ActionForce
    {
        Name = "Dash",
        Mode = ForceMode.Impulse,
        Direction = Vector3.forward,
        Magnitude = 10f,
        Duration = 0.5f,
        LocksMovement = true,
        LocksGravity = true,
    };

    // No locks on these two: the interrupt rule keys on Name only, so the tests below must
    // pass whatever the lock values are.
    private static readonly ActionForce Attack = new ActionForce { Name = "Attack", Duration = 0.2f };
    private static readonly ActionForce Jump = new ActionForce { Name = "Jump" }; // duration 0, like the real jump

    private ActionForceRunner runner;

    [SetUp]
    public void SetUp() => runner = new ActionForceRunner();

    [Test]
    public void Idle_state_leaves_movement_and_gravity_alone()
    {
        Assert.IsFalse(runner.IsActive);
        Assert.IsFalse(runner.Current.LocksMovement);
        Assert.IsFalse(runner.Current.LocksGravity);
    }

    [Test]
    public void A_force_applied_from_idle_starts_running()
    {
        Assert.IsTrue(runner.TryApply(Dash));
        Assert.IsTrue(runner.IsActive);
        Assert.AreEqual("Dash", runner.Current.Name);
    }

    [Test]
    public void Retriggering_the_running_force_is_ignored_and_keeps_its_locks()
    {
        runner.TryApply(Dash);

        Assert.IsFalse(runner.TryApply(Dash));
        // The previous runner cleared Current here, so mashing dash mid-dash regained steering and gravity.
        Assert.IsTrue(runner.Current.LocksMovement);
        Assert.IsTrue(runner.Current.LocksGravity);
    }

    // Jumping or attacking out of a dash relies on this; PlayerControl zeroes velocity when it happens.
    [Test]
    public void A_different_force_interrupts_the_running_one()
    {
        runner.TryApply(Dash);

        Assert.IsTrue(runner.TryApply(Attack));
        Assert.AreEqual("Attack", runner.Current.Name);
    }

    // PlayerControl fires SetForceEndTrigger on this edge. The original code fired it on every
    // fixed step while idle, which left the animator parameter permanently raised.
    [Test]
    public void Tick_reports_the_end_exactly_once_then_returns_to_idle()
    {
        runner.TryApply(Dash);

        Assert.IsFalse(runner.Tick(0.4f));
        Assert.IsTrue(runner.Tick(0.2f), "0.6s of a 0.5s force must end it");
        Assert.IsFalse(runner.Tick(0.2f), "a second report would refire the animation trigger");
        Assert.IsFalse(runner.Current.LocksMovement);
    }

    [Test]
    public void The_same_force_is_accepted_again_once_it_has_ended()
    {
        runner.TryApply(Dash);
        runner.Tick(0.6f);

        Assert.IsTrue(runner.TryApply(Dash));
    }

    [Test]
    public void A_zero_duration_force_never_blocks_the_next_one()
    {
        // Jump is built with duration 0, so jumping again or dashing straight after must work.
        Assert.IsTrue(runner.TryApply(Jump));
        Assert.IsFalse(runner.IsActive);
        Assert.IsTrue(runner.TryApply(Jump));
    }
}
