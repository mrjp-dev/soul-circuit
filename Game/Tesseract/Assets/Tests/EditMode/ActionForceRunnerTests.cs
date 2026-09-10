using NUnit.Framework;
using UnityEngine;

// Characterises the interrupt rules lifted out of PlayerControl. Before the
// extraction none of this could be reached without a Rigidbody, an Animator and
// a scene, which is why it had no coverage at all.
public class ActionForceRunnerTests
{
    private ActionForceRunner runner;

    [SetUp]
    public void SetUp() => runner = new ActionForceRunner();

    private static ActionForce Dash(bool selfInterruptable = false,
                                    bool interruptable = true,
                                    float duration = 0.5f) =>
        new ActionForce("Dash",
                        ForceMode.Impulse,
                        Vector3.forward,
                        magnitude: 10f,
                        duration: duration,
                        isInterruptable: interruptable,
                        isSelfInterruptable: selfInterruptable,
                        allowsMovement: false,
                        allowsGravity: false);

    [Test]
    public void Idle_state_permits_movement_and_gravity()
    {
        // The trap this guards: default(ActionForce) zeroes both flags, so an
        // idle state built that way would leave the player unable to move.
        Assert.IsTrue(runner.Current.AllowsMovement);
        Assert.IsTrue(runner.Current.AllowsGravity);
        Assert.IsFalse(runner.IsActive);
    }

    [Test]
    public void Applying_from_idle_succeeds_and_starts_the_timer()
    {
        Assert.IsTrue(runner.TryApply(Dash()));
        Assert.IsTrue(runner.IsActive);
        Assert.AreEqual("Dash", runner.Current.Name);
    }

    [Test]
    public void Retriggering_a_non_self_interruptable_force_is_rejected()
    {
        runner.TryApply(Dash(selfInterruptable: false));

        Assert.IsFalse(runner.TryApply(Dash(selfInterruptable: false)),
            "Attack and dash rely on this to stop button mashing re-firing the impulse.");
    }

    // No production force sets this true today. The rule is covered anyway
    // because it is the only reason IsSelfInterruptable is a field rather than
    // a hardcoded false, and a future parry or combo step will need it.
    [Test]
    public void Retriggering_a_self_interruptable_force_restarts_it()
    {
        runner.TryApply(Dash(selfInterruptable: true));
        runner.Tick(0.4f);

        Assert.IsTrue(runner.TryApply(Dash(selfInterruptable: true)));
        Assert.IsTrue(runner.IsActive, "the timer should have restarted from zero");
    }

    [Test]
    public void A_different_force_is_admitted_only_when_the_running_one_allows_it()
    {
        runner.TryApply(Dash(interruptable: true));
        Assert.IsTrue(runner.TryApply(new ActionForce("Attack", duration: 0.2f)));

        // Fresh runner: the first half already consumed the dash by
        // interrupting it, and Previous now names Attack.
        runner = new ActionForceRunner();
        runner.TryApply(Dash(interruptable: false));
        Assert.IsFalse(runner.TryApply(new ActionForce("Attack", duration: 0.2f)));
    }

    [Test]
    public void Tick_reports_the_end_exactly_once()
    {
        runner.TryApply(Dash(duration: 0.5f));

        // Tick is edge-triggered so SetForceEndTrigger fires once per force.
        Assert.IsFalse(runner.Tick(0.4f));
        Assert.IsTrue(runner.Tick(0.2f), "0.6s of a 0.5s force must end it");
        Assert.IsFalse(runner.Tick(0.2f),
            "a second report would re-fire the animation trigger every frame");
    }

    [Test]
    public void An_ended_force_returns_the_runner_to_the_permissive_idle_state()
    {
        runner.TryApply(Dash(duration: 0.1f));
        runner.Tick(0.2f);

        Assert.IsTrue(runner.Current.AllowsMovement);
        Assert.IsTrue(runner.Current.AllowsGravity);
    }

    [Test]
    public void A_zero_duration_force_applies_without_ever_becoming_active()
    {
        // Jump is built with duration 0, so it must never block a later force.
        Assert.IsTrue(runner.TryApply(new ActionForce("Jump", duration: 0f)));
        Assert.IsFalse(runner.IsActive);
    }
}
