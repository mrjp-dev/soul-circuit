using UnityEngine;

// Points an NPC's sprite in a direction, for example toward the player when spoken to.
public class NPAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake() => animator = GetComponentInChildren<Animator>();

    // The blend tree takes the angle as a 0..1 fraction of a turn.
    public void SetDirection(float degrees) => animator.SetFloat("horizontalDirection", degrees % 360f / 360f);
}
