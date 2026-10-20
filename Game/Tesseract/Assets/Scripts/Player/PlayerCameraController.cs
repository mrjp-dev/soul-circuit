using Unity.Cinemachine;
using UnityEngine;

// Steps the player camera through fixed zoom levels. 11.25 is half of 360 texels at 16 texels
// per unit, so zoom 1 maps one texture pixel to one render texel; the other levels divide it
// by their zoom factor (1, 2, 4, 5, 8, 10, 20, 40). They were 11.24 and its divisions, a
// workaround for UPixelator's grid that PixelPerfectCamera does not need.
public class PlayerCameraController : MonoBehaviour
{
    private static readonly float[] OrthographicSizes = { 11.25f, 5.625f, 2.8125f, 2.25f, 1.40625f, 1.125f, 0.5625f, 0.28125f };
    private static readonly float[] PerspectiveDistances = { 20f, 10f, 5f, 4f, 2.5f, 2f, 1f, 0.5f };

    private CinemachineCamera cmCamera;
    private CinemachineThirdPersonFollow follow; // perspective only; Testing.unity has none
    private int zoomLevel;

    private void Awake()
    {
        cmCamera = GetComponent<CinemachineCamera>();
        follow = GetComponent<CinemachineThirdPersonFollow>();
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
    }
}
