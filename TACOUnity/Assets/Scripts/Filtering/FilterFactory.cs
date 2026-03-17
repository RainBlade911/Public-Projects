/// <summary>
/// Factory pattern — single source of truth for per-joint filter tuning.
///
/// Joint categories (17-joint skeleton matching Python PART_IDS):
///   Face    (0–4)       nose, eyes, ears    — moderate speed
///   Wrists  (9, 10)     leftWrist, rightWrist — high velocity, low lag
///   Ankles  (15, 16)    leftAnkle, rightAnkle — high velocity, low lag
///   Rest               torso, shoulders, hips, elbows, knees
/// </summary>
public static class FilterFactory
{
    public static OneEuroFilter CreateForJoint(int jointIndex)
    {
        // Wrists and ankles — fastest joints, prioritise responsiveness
        if (jointIndex == PoseData.LeftWrist || jointIndex == PoseData.RightWrist ||
            jointIndex == PoseData.LeftAnkle || jointIndex == PoseData.RightAnkle)
            return new OneEuroFilter(minCutoff: 0.7f, beta: 0.01f, dCutoff: 1.0f);

        // Face joints — moderate speed
        if (jointIndex <= PoseData.RightEar)
            return new OneEuroFilter(minCutoff: 0.8f, beta: 0.015f, dCutoff: 1.0f);

        // Torso, shoulders, hips, elbows, knees — stable, absorb more noise
        return new OneEuroFilter(minCutoff: 1.5f, beta: 0.04f, dCutoff: 1.0f);
    }

    public static OneEuroFilter[] CreateForSkeleton(int jointCount)
    {
        var filters = new OneEuroFilter[jointCount];
        for (int i = 0; i < jointCount; i++)
            filters[i] = CreateForJoint(i);
        return filters;
    }
}