using UnityEngine;

// The world-anchored pixel grid a camera renders on, in the camera's own x/y axes so snapped
// positions land on whole texels at any tilt. Depth is not snapped: it never moves an
// orthographic pixel, and snapping it made UPixelator's shadows flicker.
public readonly struct PixelGrid
{
    private readonly Quaternion rotation;
    private readonly Vector2 texel; // camera-local world units covered by one render texel

    public PixelGrid(Quaternion rotation, Vector2 texel)
    {
        this.rotation = rotation;
        this.texel = texel;
    }

    // Returns the nearest on-grid position. remainder is how far the input was from it, in
    // texels, along camera x and y: the sub-pixel offset the upscale must re-apply.
    public Vector3 Snap(Vector3 world, out Vector2 remainder)
    {
        Vector3 local = Quaternion.Inverse(rotation) * world;
        Vector3 snapped = new Vector3(Round(local.x, texel.x), Round(local.y, texel.y), local.z);
        remainder = new Vector2((local.x - snapped.x) / texel.x, (local.y - snapped.y) / texel.y);
        return rotation * snapped;
    }

    private static float Round(float value, float step) => Mathf.Round(value / step) * step;
}
