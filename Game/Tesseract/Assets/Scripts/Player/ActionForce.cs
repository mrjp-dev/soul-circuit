using UnityEngine;

// One self-applied impulse: jump, dash, or attack. The four flags are the whole
// difference between them. A dash forbids steering and gravity so it travels a
// straight line; a jump allows both so the player keeps air control.
public struct ActionForce
{
    public string Name;
    public ForceMode Mode;
    public Vector3 Direction;
    public float Magnitude;
    public float Duration;

    public bool IsInterruptable; // a different force may cut this one short

    // False on attack and dash, so mashing cannot re-fire the impulse each frame.
    public bool IsSelfInterruptable;

    public bool AllowsMovement;
    public bool AllowsGravity;

    // Idle state, built through the constructor deliberately. C# 9 structs
    // cannot declare a parameterless constructor, so `new ActionForce()` and
    // `default(ActionForce)` zero every bool, leaving the player unable to
    // move or fall whenever nothing else is happening.
    public static readonly ActionForce None = new ActionForce("None");

    public ActionForce(string name = "None",
                       ForceMode mode = ForceMode.Force,
                       Vector3 direction = default,
                       float magnitude = 0f,
                       float duration = 0f,
                       bool isInterruptable = true,
                       bool isSelfInterruptable = true,
                       bool allowsMovement = true,
                       bool allowsGravity = true)
    {
        Name = name;
        Mode = mode;
        Direction = direction;
        Magnitude = magnitude;
        Duration = duration;
        IsInterruptable = isInterruptable;
        IsSelfInterruptable = isSelfInterruptable;
        AllowsMovement = allowsMovement;
        AllowsGravity = allowsGravity;
    }
}

// Owns which action force runs, for how long, and when one may interrupt
// another. Does no physics: it decides, PlayerControl applies. That split is why
// the rules can now be tested without a Rigidbody, an Animator, or a scene.
public class ActionForceRunner
{
    public ActionForce Current { get; private set; } = ActionForce.None;
    public ActionForce Previous { get; private set; } = ActionForce.None;

    public bool IsActive => elapsed < duration;

    private float duration;
    private float elapsed;

    public bool TryApply(ActionForce force)
    {
        bool allowed = !IsActive
                    || (force.Name == Previous.Name ? force.IsSelfInterruptable
                                                    : Previous.IsInterruptable);
        if (!allowed)
        {
            // Preserved from the original: a rejected input still clears Current,
            // so the force still running silently regains movement and gravity.
            // Almost certainly a bug, kept so this extraction changes no feel.
            Current = ActionForce.None;
            return false;
        }

        Current = force;
        Previous = force;
        duration = force.Duration;
        elapsed = 0f;
        return true;
    }

    // Returns true only on the frame the force ends, so the caller fires its
    // animation trigger once. The original set that trigger on every fixed
    // step while idle, which left it permanently raised.
    public bool Tick(float deltaTime)
    {
        if (!IsActive) return false;

        elapsed += deltaTime;
        if (IsActive) return false;

        Current = ActionForce.None;
        Previous = ActionForce.None;
        return true;
    }
}
