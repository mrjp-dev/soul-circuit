using UnityEngine;

// Drives the player sprite's Animator from movement state.
public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake() => animator = GetComponentInChildren<Animator>();

    public void SetDirection(float degrees)
    {
        float direction = degrees % 360f / 360f;
        animator.SetFloat("horizontalDirection", direction);

        // Blend values 0.625 to 0.875 (225 to 315 degrees) are drawn mirrored. The original
        // multiplied x by -1 on each of two calls per physics step, so the flip only held when
        // that range was entered by turning; setting it outright gives the same picture always.
        Vector3 scale = transform.localScale;
        scale.x = direction is > 0.625f and < 0.875f ? -1f : 1f;
        transform.localScale = scale;
    }

    public void SetMoveAnimation(float directionX, float directionZ, bool walking, bool running)
    {
        animator.SetFloat("horizontalDirectionX", directionX);
        animator.SetFloat("horizontalDirectionZ", directionZ);
        animator.SetBool("isWalking", walking);
        animator.SetBool("isRunning", running);
    }

    public void SetGrounded(bool state) => animator.SetBool("isGrounded", state);

    public void SetFalling(bool state) => animator.SetBool("isFalling", state);

    public void SetJumpAnim() => animator.SetTrigger("JumpTrigger");

    public void SetPrimaryAnim() => animator.SetTrigger("PrimaryTrigger");

    public void SetSecondaryAnim() => animator.SetTrigger("SecondaryTrigger");

    public void SetForceEndTrigger() => animator.SetTrigger("ForceEndTrigger");
}
