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
        int w = source.width;
        int h = source.height;

        if (w != currentWidth || h != currentHeight)
            Reallocate(w, h);

        Graphics.Blit(source, rt);

        RenderTexture.active = rt;
        readback.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        readback.Apply();
        RenderTexture.active = null;

        // GetPixels32 returns rows bottom-to-top (OpenGL convention).
        // We reverse row order so C++ receives pixels top-to-bottom,
        // matching what Python sees via JPEG decode.
        pixels = readback.GetPixels32();
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

        rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
        readback = new Texture2D(w, h, TextureFormat.RGB24, false);
        buffer = new byte[w * h * 3];
        pixels = new Color32[w * h];

        currentWidth = w;
        currentHeight = h;
    }

    void Release()
    {
        if (rt != null) { rt.Release(); Object.Destroy(rt); rt = null; }
        if (readback != null) { Object.Destroy(readback); readback = null; }
        buffer = null;
    }
}