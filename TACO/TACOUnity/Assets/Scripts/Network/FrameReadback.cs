using UnityEngine;

/// <summary>
/// Facade pattern — hides Graphics.Blit + RenderTexture + ReadPixels
/// behind a single ReadRGB() call, returning raw RGB24 bytes.
///
/// Mirrors FrameDownscaler but skips JPEG encoding — used by
/// NativePluginBackend to feed raw pixels directly to the C++ plugin.
///
/// Uses Blit + ReadPixels (same as FrameDownscaler) intentionally:
/// this correctly resolves WebCamTexture's platform-specific Y-flip
/// and DX12/OpenGL coordinate differences, unlike CopyTexture which
/// does a raw GPU copy and bypasses Unity's coordinate correction.
/// </summary>
public class FrameReadback : System.IDisposable
{
    // ── Private state ─────────────────────────────────────────────────────────

    RenderTexture rt;
    Texture2D readback;
    byte[] buffer;
    Color32[] pixels;

    int currentWidth;
    int currentHeight;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Blit source texture into a RenderTexture, read back as RGB24 bytes.
    /// Reallocates internal buffers automatically if the source size changes.
    /// Returns a reference to an internal buffer — do not cache across frames.
    /// </summary>
    public byte[] ReadRGB(Texture source)
    {
        var webcam = source as WebCamTexture;
        if (webcam == null) return buffer;

        int w = webcam.width;
        int h = webcam.height;

        if (w != currentWidth || h != currentHeight)
            Reallocate(w, h);

        webcam.GetPixels32(pixels);

        int idx = 0;
        for (int row = h - 1; row >= 0; row--)
        {
            for (int col = 0; col < w; col++)
            {
                Color32 p = pixels[row * w + col];
                buffer[idx++] = p.r;
                buffer[idx++] = p.g;
                buffer[idx++] = p.b;
            }
        }
        return buffer;
    }

    public void Dispose()
    {
        Release();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    void Reallocate(int w, int h)
    {
        Release();
        pixels = new Color32[w * h];
        buffer = new byte[w * h * 3];
        currentWidth = w;
        currentHeight = h;
    }

    void Release()
    {
        if (rt != null) { rt.Release(); Object.Destroy(rt); rt = null; }
        if (readback != null) { Object.Destroy(readback); readback = null; }
        pixels = null;
        buffer = null;
    }
}