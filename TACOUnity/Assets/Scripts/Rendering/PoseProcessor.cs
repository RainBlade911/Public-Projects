using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Facade pattern — hides all pose processing complexity behind
/// a single Update() call.
///
/// PoseOverlayRenderer calls processor.Update(joints, bones, overlayArea)
/// and knows nothing about filtering, visibility, constraints, or
/// coordinate mapping.
///
/// Internally uses:
///   FilterFactory        — creates per-joint OneEuroFilters (Factory)
///   VisibilityController — hysteresis + lost-joint memory (SRP)
///   StructuralConstraints — swap fix applied pre-render (SRP)
///   CoordinateMapper     — letterbox/pillarbox math (SRP)
/// </summary>
public class PoseProcessor
{
    // ── Config ────────────────────────────────────────────────────────────────

    public float MaxJumpPerFrame = 0.08f; // normalised space

    // ── Collaborators ─────────────────────────────────────────────────────────

    readonly VisibilityController visibility;
    readonly StructuralConstraints constraints;
    readonly CoordinateMapper mapper;
    readonly OneEuroFilter[] filters;

    // ── State ─────────────────────────────────────────────────────────────────

    readonly int jointCount;
    readonly int[,] bonePairs;
    readonly Vector2[] smoothed;

    bool initialized;

    // ── Constructor ───────────────────────────────────────────────────────────

    public PoseProcessor(int jointCount, int[,] bonePairs,
                         float sourceWidth = 1280f, float sourceHeight = 720f)
    {
        this.jointCount = jointCount;
        this.bonePairs = bonePairs;

        smoothed = new Vector2[jointCount];
        filters = FilterFactory.CreateForSkeleton(jointCount);
        visibility = new VisibilityController(jointCount);
        mapper = new CoordinateMapper(sourceWidth, sourceHeight);

        // Structural constraints: left shoulder/right shoulder, left hip/right hip
        constraints = new StructuralConstraints(
            (PoseData.LeftShoulder, PoseData.RightShoulder),
            (PoseData.LeftHip, PoseData.RightHip)
        );

        // Reset filter cleanly when a joint re-appears
        visibility.OnJointSpawned += i =>
        {
            filters[i].Reset();
            filters[i].Filter(smoothed[i]);
        };
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Call from PoseOverlayRenderer.Update() in place of the fake data block.
    /// </summary>
    public void Update(List<RectTransform> joints,
                       List<RectTransform> bones,
                       RectTransform overlayArea)
    {
        if (!PoseData.hasNewPose) return;
        PoseData.hasNewPose = false;

        // Prime all filters on the very first frame
        if (!initialized)
        {
            for (int i = 0; i < jointCount; i++)
            {
                smoothed[i] = PoseData.keypoints[i];
                filters[i].Filter(smoothed[i]);
            }
            initialized = true;
        }

        // Torso centre for lost-joint natural drift target
        Vector2 torsoCenter =
            (smoothed[PoseData.LeftHip] + smoothed[PoseData.RightHip]) * 0.5f;

        // ── Structural constraints BEFORE writing joint positions ──────────
        constraints.Apply(smoothed);

        // ── Per-joint update ───────────────────────────────────────────────
        for (int i = 0; i < joints.Count; i++)
        {
            bool active = visibility.Evaluate(
                i,
                PoseData.confidence[i],
                PoseData.keypoints[i],
                smoothed[i],
                torsoCenter,
                out Vector2 raw);

            joints[i].gameObject.SetActive(active);

            if (!active)
            {
                filters[i].Filter(smoothed[i]); // keep filter warm
                continue;
            }

            // Jump limiter — reject teleporting keypoints
            Vector2 delta = raw - smoothed[i];
            if (delta.magnitude > MaxJumpPerFrame)
                raw = smoothed[i] + delta.normalized * MaxJumpPerFrame;

            smoothed[i] = filters[i].Filter(raw);

            joints[i].anchoredPosition = mapper.Map(smoothed[i], overlayArea);
        }

        // ── Bones ──────────────────────────────────────────────────────────
        for (int i = 0; i < bones.Count; i++)
        {
            int aId = bonePairs[i, 0];
            int bId = bonePairs[i, 1];

            bool bothVisible = joints[aId].gameObject.activeSelf &&
                               joints[bId].gameObject.activeSelf;

            bones[i].gameObject.SetActive(bothVisible);

            if (!bothVisible) continue;

            DrawBone(bones[i],
                     joints[aId].anchoredPosition,
                     joints[bId].anchoredPosition);
        }
    }

    // ── Private ───────────────────────────────────────────────────────────────

    static void DrawBone(RectTransform bone, Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        bone.anchoredPosition = (start + end) * 0.5f;
        bone.sizeDelta = new Vector2(bone.sizeDelta.x, dir.magnitude);
        bone.localRotation = Quaternion.Euler(
            0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
    }
}