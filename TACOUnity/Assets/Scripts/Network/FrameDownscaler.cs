using UnityEngine;

/// <summary>
/// Facade pattern — hides Graphics.Blit + RenderTexture + ReadPixels
/// + EncodeToJPG behind a single Encode() call.
/// </summary>
public class FrameDownscaler : System.IDisposable
{
    readonly RenderTexture rt;
    readonly Texture2D readback;
    readonly int quality;

    public FrameDownscaler(int width = 320, int height = 180, int jpegQuality = 40)
    {
        quality = jpegQuality;
        rt = new RenderTexture(width, height, 0);
        readback = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public byte[] Encode(Texture source)
    {
        Graphics.Blit(source, rt);

        RenderTexture.active = rt;
        readback.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readback.Apply();
        RenderTexture.active = null;

        return readback.EncodeToJPG(quality);
    }

    public void Dispose()
    {
        Object.Destroy(rt);
        Object.Destroy(readback);
    }
}