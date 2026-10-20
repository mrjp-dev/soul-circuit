using Unity.Cinemachine;
using UnityEngine;

// Steps the player camera through fixed zoom levels. The sizes are hand-picked, not computed:
// at 640x360, an orthographic size of 11.24 is pixel-perfect at zoom 1, and each other level
// is that divided by its zoom factor (1, 2, 4, 5, 8, 10, 20, 40).
public class PlayerCameraController : MonoBehaviour
{
    private static readonly float[] OrthographicSizes = { 11.24f, 5.62f, 2.81f, 2.248f, 1.405f, 1.124f, 0.562f, 0.281f };
    private static readonly float[] PerspectiveDistances = { 20f, 10f, 5f, 4f, 2.5f, 2f, 1f, 0.5f };

    private CinemachineCamera cmCamera;
    private CinemachineThirdPersonFollow follow;
    private CinemachineHardLookAt lookAt;
    private int zoomLevel;

    private void Awake()
    {
        cmCamera = GetComponent<CinemachineCamera>();
        follow = GetComponent<CinemachineThirdPersonFollow>();
        lookAt = GetComponent<CinemachineHardLookAt>();
    }

    private void OnEnable()
    {
        EventManager.Current.OnCameraZoomIn += ZoomIn;
        EventManager.Current.OnCameraZoomOut += ZoomOut;
    }

    private void OnDisable()
    {
        EventManager.Current.OnCameraZoomIn -= ZoomIn;
        EventManager.Current.OnCameraZoomOut -= ZoomOut;
    }

    private void ZoomIn() => Zoom(1);

    private void ZoomOut() => Zoom(-1);

    private void Zoom(int step)
    {
        zoomLevel = Mathf.Clamp(zoomLevel + step, 0, OrthographicSizes.Length - 1);
        if (cmCamera.Lens.Orthographic) cmCamera.Lens.OrthographicSize = OrthographicSizes[zoomLevel];
        else follow.CameraDistance = PerspectiveDistances[zoomLevel];

        // At 8x and 40x the view is an odd number of pixels tall (360/8 = 45, 360/40 = 9), so
        // its centre falls mid-pixel; a 1/32 unit nudge on the look-at target realigns it.
        lookAt.LookAtOffset.y = zoomLevel is 4 or 7 ? 0.03125f : 0f;
    }
}
