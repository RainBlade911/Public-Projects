using UnityEngine;

/// <summary>
/// One Euro Filter — adaptive low-pass filter for real-time 2D signals.
///
///   cutoff = minCutoff + beta * |velocity|
///
/// Slow motion  → small cutoff → heavy smoothing (kills jitter)
/// Fast motion  → large cutoff → light smoothing (kills lag)
///
/// Use FilterFactory to get per-joint tuned instances.
/// </summary>
public class OneEuroFilter
{
    readonly float minCutoff;
    readonly float beta;
    readonly float dCutoff;

    bool initialized;
    Vector2 xPrev;
    Vector2 dxPrev;
    float lastTime;

    public OneEuroFilter(float minCutoff = 1.0f, float beta = 0.02f, float dCutoff = 1.0f)
    {
        this.minCutoff = minCutoff;
        this.beta = beta;
        this.dCutoff = dCutoff;
    }

    public Vector2 Filter(Vector2 x)
    {
        float now = Time.time;

        if (!initialized)
        {
            initialized = true;
            xPrev = x;
            dxPrev = Vector2.zero;
            lastTime = now;
            return x;
        }

        float dt = now - lastTime;
        if (dt <= 0f) dt = 0.0001f;

        Vector2 dx = (x - xPrev) / dt;
        float alphaD = Alpha(dCutoff, dt);
        dxPrev = LowPassVec(dxPrev, dx, alphaD);

        float cutoff = minCutoff + beta * dxPrev.magnitude;
        float alpha = Alpha(cutoff, dt);
        Vector2 result = LowPassVec(xPrev, x, alpha);

        xPrev = result;
        lastTime = now;

        return result;
    }

    /// <summary>Reset internal state — call on joint re-spawn.</summary>
    public void Reset()
    {
        initialized = false;
        xPrev = Vector2.zero;
        dxPrev = Vector2.zero;
    }

    static float Alpha(float cutoff, float dt)
    {
        float tau = 1f / (2f * Mathf.PI * cutoff);
        return 1f / (1f + tau / dt);
    }

    static float LowPass(float prev, float curr, float alpha) =>
        alpha * curr + (1f - alpha) * prev;

    static Vector2 LowPassVec(Vector2 prev, Vector2 curr, float alpha) =>
        new Vector2(LowPass(prev.x, curr.x, alpha),
                    LowPass(prev.y, curr.y, alpha));
}