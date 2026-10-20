using UnityEngine;

// Drives the player sprite's Animator from movement state. Parameter names are hashed once:
// the string overloads hash on every call, and these run every physics step.
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int Direction = Animator.StringToHash("horizontalDirection");
    private static readonly int DirectionX = Animator.StringToHash("horizontalDirectionX");
    private static readonly int DirectionZ = Animator.StringToHash("horizontalDirectionZ");
    private static readonly int Walking = Animator.StringToHash("isWalking");
    private static readonly int Running = Animator.StringToHash("isRunning");
    private static readonly int Grounded = Animator.StringToHash("isGrounded");
    private static readonly int Falling = Animator.StringToHash("isFalling");
    private static readonly int Jump = Animator.StringToHash("JumpTrigger");
    private static readonly int Primary = Animator.StringToHash("PrimaryTrigger");
    private static readonly int Secondary = Animator.StringToHash("SecondaryTrigger");
    private static readonly int ForceEnd = Animator.StringToHash("ForceEndTrigger");

    private Animator animator;

    private void Awake() => animator = GetComponentInChildren<Animator>();

    public void SetDirection(float degrees)
    {
        float direction = degrees % 360f / 360f;
        animator.SetFloat(Direction, direction);

        // Blend values 0.625 to 0.875 (225 to 315 degrees) are drawn mirrored. The original
        // multiplied x by -1 on each of two calls per physics step, so the flip only held when
        // that range was entered by turning; setting it outright gives the same picture always.
        Vector3 scale = transform.localScale;
        scale.x = direction is > 0.625f and < 0.875f ? -1f : 1f;
        transform.localScale = scale;
    }

    public void SetMoveAnimation(float directionX, float directionZ, bool walking, bool running)
    {
        animator.SetFloat(DirectionX, directionX);
        animator.SetFloat(DirectionZ, directionZ);
        animator.SetBool(Walking, walking);
        animator.SetBool(Running, running);
    }

    public void SetGrounded(bool state) => animator.SetBool(Grounded, state);

    public void SetFalling(bool state) => animator.SetBool(Falling, state);

    public void SetJumpAnim() => animator.SetTrigger(Jump);

    public void SetPrimaryAnim() => animator.SetTrigger(Primary);

    public void SetSecondaryAnim() => animator.SetTrigger(Secondary);

    public void SetForceEndTrigger() => animator.SetTrigger(ForceEnd);
}
