using UnityEngine;

/// <summary>
/// Enforces left/right joint ordering on smoothed positions
/// BEFORE they are written to RectTransforms.
///
/// Prevents the mirror-flip artefact when the model swaps sides.
/// Must be called before joint positions are applied — fixes the
/// original one-frame-late bug where the swap happened after rendering.
/// </summary>
public class StructuralConstraints
{
    readonly (int left, int right)[] pairs;

    /// <param name="lateralPairs">
    /// Each tuple is (leftJointIndex, rightJointIndex).
    /// Left joint should always have smaller X than right joint.
    /// </param>
    public StructuralConstraints(params (int left, int right)[] lateralPairs)
    {
        pairs = lateralPairs;
    }

    public void Apply(Vector2[] smoothed)
    {
        foreach (var (left, right) in pairs)
        {
            if (left >= smoothed.Length) continue;
            if (right >= smoothed.Length) continue;

            if (smoothed[left].x > smoothed[right].x)
            {
                Vector2 tmp = smoothed[left];
                smoothed[left] = smoothed[right];
                smoothed[right] = tmp;
            }
        }
    }
}