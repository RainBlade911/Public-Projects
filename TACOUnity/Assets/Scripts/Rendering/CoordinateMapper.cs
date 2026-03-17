using UnityEngine;

/// <summary>
/// Single responsibility: convert normalised pose coordinates (0..1)
/// to overlay-local anchoredPosition pixels, accounting for
/// letterbox / pillarbox based on the source video aspect ratio.
///
/// Y coordinates are clamped to 0..1 — the model can return values
/// above 1.0 when limbs extend below the camera frame.
/// </summary>
public class CoordinateMapper
{
    readonly float videoAspect;

    public CoordinateMapper(float sourceWidth = 1280f, float sourceHeight = 720f)
    {
        videoAspect = sourceWidth / Mathf.Max(sourceHeight, 1f);
    }

    public Vector2 Map(Vector2 normalized, RectTransform overlayArea)
    {
        // Clamp both axes — model can return values outside 0..1
        float x = Mathf.Clamp01(normalized.x);
        float y = Mathf.Clamp01(normalized.y);

        Rect vr = ComputeVideoRect(overlayArea);

        float px = vr.x + x * vr.width;
        float py = vr.y + (1f - y) * vr.height;

        return new Vector2(
            px - overlayArea.rect.width * 0.5f,
            py - overlayArea.rect.height * 0.5f);
    }

    Rect ComputeVideoRect(RectTransform overlayArea)
    {
        float w = overlayArea.rect.width;
        float h = overlayArea.rect.height;
        float overlayAspect = w / Mathf.Max(h, 1f);

        float drawW, drawH, offsetX = 0f, offsetY = 0f;

        if (overlayAspect > videoAspect)
        {
            // Pillarbox
            drawH = h;
            drawW = drawH * videoAspect;
            offsetX = (w - drawW) * 0.5f;
        }
        else
        {
            // Letterbox
            drawW = w;
            drawH = drawW / videoAspect;
            offsetY = (h - drawH) * 0.5f;
        }

        return new Rect(offsetX, offsetY, drawW, drawH);
    }
}