using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Smooth, pixel-perfect camera. The scene renders at low resolution, so every pixel sits on
// one grid, while the view still glides at screen resolution:
//   1. Before rendering, snap the camera to the texel grid and remember the sub-texel remainder.
//   2. Render into a texture one texel larger on every side than the visible area.
//   3. Show the visible window of that texture shifted by the remainder. The shift happens on
//      the upscaled image, so it moves in screen pixels, not texels.
// The padding texel hides the edge the shift exposes. Moving objects listed in snapTargets are
// snapped the same way so they cannot shimmer against the static scene. Everything is
// restored after rendering, so Cinemachine and gameplay never see a snapped transform.
//
// Replaces UPixelator (Abiogenesis3d) for this scene and absorbs the old CameraController.
[RequireComponent(typeof(Camera))]
public class PixelPerfectCamera : MonoBehaviour
{
    private const int Padding = 1;

    // 1/sin(45 degrees): undoes the foreshortening of the 45-degree tilt so ground texels stay
    // square. Carried over from CameraController.
    private static readonly float VerticalStretch = Mathf.Sqrt(2);

    [SerializeField] private RawImage display; // full-screen image the scene is upscaled into
    [SerializeField] private Vector2Int resolution = new Vector2Int(640, 360); // 3x at 1080p
    [SerializeField] private Transform[] snapTargets = new Transform[0];

    // One colour and opacity for every cast shadow, read by the Hard Shadow Lit shader. A
    // global, so no material can drift to a different shade.
    [SerializeField] private Color shadowColor = new Color(0.08f, 0.06f, 0.2f, 0.55f);

    private Camera cam;
    private RenderTexture target;
    private Vector3 cameraPosition;
    private Vector3[] targetPositions;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        target = new RenderTexture(resolution.x + 2 * Padding, resolution.y + 2 * Padding, 24)
        {
            filterMode = FilterMode.Point,
            name = "Pixel Perfect Render",
        };
        cam.targetTexture = target;
        display.texture = target;
        targetPositions = new Vector3[snapTargets.Length];
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += Snap;
        RenderPipelineManager.endCameraRendering += Restore;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Snap;
        RenderPipelineManager.endCameraRendering -= Restore;
    }

    private void OnDestroy() => Destroy(target);

    private void Snap(ScriptableRenderContext context, Camera rendering)
    {
        if (rendering != cam) return;

        Shader.SetGlobalColor("_HardShadowColor", shadowColor);
        PixelGrid grid = new PixelGrid(transform.rotation, FitProjection());

        cameraPosition = transform.position;
        transform.position = grid.Snap(cameraPosition, out Vector2 remainder);

        for (int i = 0; i < snapTargets.Length; i++)
        {
            targetPositions[i] = snapTargets[i].position;
            snapTargets[i].position = grid.Snap(targetPositions[i], out _);
        }

        // The snapped camera drew everything offset by the remainder, so sample that much
        // further along. Remainder is within half a texel, which the padding covers.
        display.uvRect = new Rect(
            (Padding + remainder.x) / target.width,
            (Padding + remainder.y) / target.height,
            (float)resolution.x / target.width,
            (float)resolution.y / target.height);
    }

    private void Restore(ScriptableRenderContext context, Camera rendering)
    {
        if (rendering != cam) return;

        transform.position = cameraPosition;
        for (int i = 0; i < snapTargets.Length; i++) snapTargets[i].position = targetPositions[i];
    }

    // Widens the view by exactly the padding texels (the lens size is for the visible area),
    // applies the tilt stretch, and returns the world size of one texel along camera x and y.
    private Vector2 FitProjection()
    {
        cam.ResetProjectionMatrix();
        Matrix4x4 projection = cam.projectionMatrix;
        float fit = (float)resolution.y / target.height;
        projection[0, 0] *= fit;
        projection[1, 1] *= fit * VerticalStretch;
        cam.projectionMatrix = projection;

        return new Vector2(2f / (projection[0, 0] * target.width), 2f / (projection[1, 1] * target.height));
    }
}
