using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetDirection(float direction)
    {
        Vector3 _v3 = transform.localScale;

        animator.SetFloat("horizontalDirection", (direction % 360f) / 360f);

        if(animator.GetFloat("horizontalDirection") is > 0.625f and < 0.875f){
            _v3.x *= -1f;
        }
        else{
            _v3.x = 1f;
        }
        
        transform.localScale = _v3;
    }

    public void SetMoveAnimation(float directionX, float directionZ, bool walking, bool running)
    {
        animator.SetFloat("horizontalDirectionX", directionX);
        animator.SetFloat("horizontalDirectionZ", directionZ);

        animator.SetBool("isWalking", walking);
        animator.SetBool("isRunning", running);
    }

    public void SetGrounded(bool state)
    {
        animator.SetBool("isGrounded", state);
    }

    public void SetFalling(bool state, float direction)
    {
        SetDirection(direction);
        animator.SetBool("isFalling", state);
    }

    public void SetJumpAnim()
    {
        animator.SetTrigger("JumpTrigger");
    }

    internal void SetPrimaryAnim()
    {
        animator.SetTrigger("PrimaryTrigger");
    }

    internal void SetSecondaryAnim()
    {
        animator.SetTrigger("SecondaryTrigger");
    }

    internal void SetForceEndTrigger()
    {
        animator.SetTrigger("ForceEndTrigger");
    }
}