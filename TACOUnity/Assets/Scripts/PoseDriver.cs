using UnityEngine;

public class SimpleDummyPose : MonoBehaviour
{
    public Transform hips;      // root
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;

    void Update()
    {
        if (!PoseData.hasNewPose)
            return;

        var p = PoseData.keypoints;

        // Get center of body (hips midpoint)
        Vector3 center = ToWorld(
            (p[PoseData.LeftHip] + p[PoseData.RightHip]) * 0.5f
        );

        hips.position = center;

        // Move parts relative to hips
        head.position = center + Offset(p[PoseData.Nose], center);
        leftHand.position = center + Offset(p[PoseData.LeftWrist], center);
        rightHand.position = center + Offset(p[PoseData.RightWrist], center);
        leftFoot.position = center + Offset(p[PoseData.LeftAnkle], center);
        rightFoot.position = center + Offset(p[PoseData.RightAnkle], center);
    }

    Vector3 ToWorld(Vector2 kp)
    {
        float x = (kp.x - 0.5f) * 4f;
        float y = (0.5f - kp.y) * 4f; // flip Y (important)
        return new Vector3(x, y, 0f);
    }

    Vector3 Offset(Vector2 kp, Vector3 center)
    {
        Vector3 world = ToWorld(kp);
        return world - center;
    }
}