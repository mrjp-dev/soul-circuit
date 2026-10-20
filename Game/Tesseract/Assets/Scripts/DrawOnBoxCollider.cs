using UnityEngine;

// Draws this object's BoxCollider as a coloured gizmo, solid or wireframe, so trigger
// volumes show in the Scene view without selecting them.
public class DrawOnBoxCollider : MonoBehaviour
{
    public enum DrawStates { DrawCube, DrawWireCube }

    [Range(0f, 1f)]
    [SerializeField] private float colorTransparency = 1f;
    [SerializeField] private bool draw = true;
    [SerializeField] private DrawStates drawStates = DrawStates.DrawWireCube;
    [SerializeField] private Color colliderColor = Color.white;

    // Looked up per draw rather than cached in Start, so it also works in edit mode.
    private void OnDrawGizmos()
    {
        if (!draw || !TryGetComponent(out BoxCollider box) || !box.enabled) return;

        Gizmos.color = new Color(colliderColor.r, colliderColor.g, colliderColor.b, colorTransparency);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        if (drawStates == DrawStates.DrawCube) Gizmos.DrawCube(box.center, box.size);
        else Gizmos.DrawWireCube(box.center, box.size);
    }
}
