using UnityEngine;

// Stretches the camera's vertical axis by sqrt(2), which is 1/sin(45 degrees): it undoes the
// foreshortening of a camera tilted 45 degrees, so the pixel art keeps square pixels.
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField, HideInInspector] private Camera _camera;

    private void OnValidate() => TryGetComponent(out _camera);

    // Every frame because Unity has no projection-changed event, and it costs next to nothing.
    private void LateUpdate()
    {
        _camera.ResetProjectionMatrix();
        Matrix4x4 projection = _camera.projectionMatrix;
        projection[1, 1] *= Mathf.Sqrt(2);
        _camera.projectionMatrix = projection;
    }
}
