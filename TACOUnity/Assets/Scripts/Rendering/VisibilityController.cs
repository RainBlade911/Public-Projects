using UnityEngine;

/// <summary>
/// Single responsibility: per-joint visibility state with hysteresis,
/// lost-joint position memory, and natural drift toward torso.
///
/// Hysteresis prevents confidence flicker at the threshold boundary.
/// Memory freezes a lost joint briefly then drifts it toward centre.
/// </summary>
public class VisibilityController
{
    public float ShowThreshold = 0.1f;
    public float HideThreshold = -0.1f;
    public float LostHoldTime = 0.35f;
    public float NaturalReturnSpeed = 1.0f;

    readonly bool[] visible;
    readonly bool[] wasVisible;
    readonly Vector2[] lastGood;
    readonly float[] lostTimer;

    /// <summary>Fired when a joint transitions hidden → visible.</summary>
    public System.Action<int> OnJointSpawned;

    public VisibilityController(int jointCount)
    {
        visible = new bool[jointCount];
        wasVisible = new bool[jointCount];
        lastGood = new Vector2[jointCount];
        lostTimer = new float[jointCount];
    }

    public bool IsVisible(int i) => visible[i];

    /// <summary>
    /// Update visibility for joint i and return the effective position
    /// after memory/drift logic. Returns false if the joint should be hidden.
    /// </summary>
    public bool Evaluate(
        int i,
        float confidence,
        Vector2 rawFromServer,
        Vector2 currentSmoothed,
        Vector2 torsoCenter,
        out Vector2 effectiveRaw)
    {
        // Hysteresis
        if (visible[i])
        {
            if (confidence < HideThreshold) visible[i] = false;
        }
        else
        {
            if (confidence > ShowThreshold) visible[i] = true;
        }

        // Spawn event
        if (!wasVisible[i] && visible[i])
        {
            lastGood[i] = rawFromServer;
            lostTimer[i] = 0f;
            OnJointSpawned?.Invoke(i);
        }

        wasVisible[i] = visible[i];

        if (!visible[i])
        {
            effectiveRaw = currentSmoothed;
            return false;
        }

        // Memory / natural behaviour
        if (confidence > HideThreshold)
        {
            lastGood[i] = rawFromServer;
            lostTimer[i] = 0f;
            effectiveRaw = rawFromServer;
        }
        else
        {
            lostTimer[i] += Time.deltaTime;

            effectiveRaw = lostTimer[i] < LostHoldTime
                ? lastGood[i]
                : Vector2.Lerp(currentSmoothed, torsoCenter,
                               Time.deltaTime * NaturalReturnSpeed);
        }

        return true;
    }
}