using UnityEngine;

// Keeps a sprite upright while its parent turns: the parent's rotation is for physics and
// steering, and the sprite must never show it. LateUpdate, after every parent has moved,
// so no frame renders the sprite mid-turn; it ran in FixedUpdate before.
public class SpriteController : MonoBehaviour
{
    private void LateUpdate() => transform.rotation = Quaternion.identity;
}
