using UnityEngine;

/// <summary>
/// Shared static bus between PoseNetworkManager (writer)
/// and PoseProcessor/PoseOverlayRenderer (readers).
/// Written on the main thread only — via UnityMainThreadDispatcher.
/// </summary>
public static class PoseData
{
    public const int JointCount = 17;

    public static Vector2[] keypoints = new Vector2[JointCount];
    public static float[] confidence = new float[JointCount];
    public static bool hasNewPose = false;

    // Joint index constants matching Python PART_IDS
    public const int Nose = 0;
    public const int LeftEye = 1;
    public const int RightEye = 2;
    public const int LeftEar = 3;
    public const int RightEar = 4;
    public const int LeftShoulder = 5;
    public const int RightShoulder = 6;
    public const int LeftElbow = 7;
    public const int RightElbow = 8;
    public const int LeftWrist = 9;
    public const int RightWrist = 10;
    public const int LeftHip = 11;
    public const int RightHip = 12;
    public const int LeftKnee = 13;
    public const int RightKnee = 14;
    public const int LeftAnkle = 15;
    public const int RightAnkle = 16;
}