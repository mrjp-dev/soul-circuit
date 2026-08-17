using UnityEngine;

public class NPAnimation : MonoBehaviour
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetDirection(float direction)
    {
        animator.SetFloat("horizontalDirection", (direction % 360f) / 360f);
    }

    public void SetMoveAnimation(bool walking, bool running)
    {
        animator.SetBool("isWalking", walking);
        animator.SetBool("isRunning", running);
    }

    public void SetGrounded(bool groundedVal)
    {
        animator.SetBool("isGrounded", groundedVal);
    }

    public void SetJumpAnim(float direction)
    {
        animator.SetTrigger("JumpTrigger");
        SetDirection(direction);
    }
}
