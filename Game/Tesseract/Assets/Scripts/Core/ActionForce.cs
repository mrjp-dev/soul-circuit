using System;
using UnityEngine;

// One self-applied impulse: jump, dash, or attack. Serializable so attack and dash are
// tuned in the Inspector instead of in code.
//
// Both locks default to false on purpose: default(ActionForce) is the idle state, and
// idle must leave movement and gravity alone.
[Serializable]
public struct ActionForce
{
    public string Name;
    public ForceMode Mode;
    public Vector3 Direction; // local space, so "forward" follows the character's facing
    public float Magnitude;
    public float Duration;
    public bool LocksMovement;
    public bool LocksGravity;
}

// Owns which action force runs and for how long. Retriggering the running force is
// ignored, so button mashing cannot re-fire an impulse every frame; any other force
// interrupts it. Does no physics: it decides, PlayerControl applies.
public class ActionForceRunner
{
    public ActionForce Current { get; private set; }
    public bool IsActive => elapsed < Current.Duration;

    private float elapsed;

    public bool TryApply(ActionForce force)
    {
        if (IsActive && force.Name == Current.Name) return false;

        Current = force;
        elapsed = 0f;
        return true;
    }

    // True only on the frame the force ends, so the caller fires its animation trigger
    // once instead of on every fixed step.
    public bool Tick(float deltaTime)
    {
        if (!IsActive) return false;

        elapsed += deltaTime;
        if (IsActive) return false;

        Current = default;
        return true;
    }
}
