using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoseOverlayRenderer : MonoBehaviour
{
    [Header("References")]
    public RectTransform overlayArea;
    public GameObject jointPrefab;
    public GameObject bonePrefab;
    

    private List<RectTransform> joints = new();
    private List<RectTransform> bones = new();
    

    // Matches Python CONNECTED_PART_NAMES mapped to PART_IDS
    private readonly int[,] bonePairs =
    {
        {11, 5},  // leftHip      → leftShoulder
        { 7, 5},  // leftElbow    → leftShoulder
        { 7, 9},  // leftElbow    → leftWrist
        {11,13},  // leftHip      → leftKnee
        {13,15},  // leftKnee     → leftAnkle
        {12, 6},  // rightHip     → rightShoulder
        { 8, 6},  // rightElbow   → rightShoulder
        { 8,10},  // rightElbow   → rightWrist
        {12,14},  // rightHip     → rightKnee
        {14,16},  // rightKnee    → rightAnkle
        { 5, 6},  // leftShoulder → rightShoulder
        {11,12},  // leftHip      → rightHip
    };

    const int JOINT_COUNT = PoseData.JointCount; // 17

    // ── NEW: processor handles all pose logic ─────────────────────────────
    private PoseProcessor processor;

    void Start()
    {
        // Create joints
        for (int i = 0; i < JOINT_COUNT; i++)
        {
            var j = Instantiate(jointPrefab, overlayArea);
            joints.Add(j.GetComponent<RectTransform>());

            
        }

        // Create bones
        for (int i = 0; i < bonePairs.GetLength(0); i++)
        {
            var b = Instantiate(bonePrefab, overlayArea);
            bones.Add(b.GetComponent<RectTransform>());
        }

        // ── NEW ───────────────────────────────────────────────────────────
        processor = new PoseProcessor(JOINT_COUNT, bonePairs);
    }

    void Update()
    {
        // ── CHANGED: replaced fake sine/cosine block + bone loop ──────────
        processor.Update(joints, bones, overlayArea);

       
    }

    void DrawBone(RectTransform bone, Vector2 start, Vector2 end)
    {
        Vector2 mid = (start + end) * 0.5f;
        Vector2 dir = end - start;

        bone.anchoredPosition = mid;

        float length = dir.magnitude;

       

        bone.sizeDelta = new Vector2(bone.sizeDelta.x, length);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        bone.localRotation = Quaternion.Euler(0, 0, angle);
    }
}