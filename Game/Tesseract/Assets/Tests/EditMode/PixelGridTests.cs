using NUnit.Framework;
using UnityEngine;

// The snap math behind the smooth pixel-perfect camera. The tilt matches the game camera,
// because an axis-aligned grid would pass even with the rotation applied the wrong way round.
public class PixelGridTests
{
    private static readonly Quaternion Tilt = Quaternion.Euler(45f, 0f, 0f);
    private static readonly Vector2 Texel = new Vector2(0.0625f, 0.044f);
    private static readonly PixelGrid Grid = new PixelGrid(Tilt, Texel);

    private static Vector3 Local(Vector3 world) => Quaternion.Inverse(Tilt) * world;

    [Test]
    public void Snapped_positions_sit_on_whole_texels_in_camera_space()
    {
        Vector3 local = Local(Grid.Snap(new Vector3(1.234f, 5.678f, -3.21f), out _));

        Assert.AreEqual(Mathf.Round(local.x / Texel.x), local.x / Texel.x, 1e-3f);
        Assert.AreEqual(Mathf.Round(local.y / Texel.y), local.y / Texel.y, 1e-3f);
    }

    // The upscale re-applies the remainder; if snapped + remainder did not rebuild the input,
    // the view would drift away from the real camera.
    [Test]
    public void Snapped_plus_remainder_rebuilds_the_original_position()
    {
        Vector3 world = new Vector3(-2.71f, 0.33f, 9.87f);
        Vector3 snapped = Local(Grid.Snap(world, out Vector2 remainder));

        Vector3 rebuilt = new Vector3(snapped.x + remainder.x * Texel.x, snapped.y + remainder.y * Texel.y, snapped.z);
        Assert.That(Vector3.Distance(rebuilt, Local(world)), Is.LessThan(1e-4f));
    }

    // Half a texel is all the one-texel padding can hide; anything more would expose the edge.
    [Test]
    public void The_remainder_never_exceeds_half_a_texel()
    {
        for (float x = 0f; x < 2f; x += 0.013f)
        {
            Grid.Snap(new Vector3(x, x * 0.7f, -x), out Vector2 remainder);
            Assert.That(Mathf.Abs(remainder.x), Is.LessThanOrEqualTo(0.5f + 1e-4f));
            Assert.That(Mathf.Abs(remainder.y), Is.LessThanOrEqualTo(0.5f + 1e-4f));
        }
    }

    [Test]
    public void Depth_along_the_view_is_left_untouched()
    {
        Vector3 world = new Vector3(0.1f, 0.2f, 0.3f);

        Assert.AreEqual(Local(world).z, Local(Grid.Snap(world, out _)).z, 1e-5f);
    }
}
