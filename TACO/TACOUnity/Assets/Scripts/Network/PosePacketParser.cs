using System.Text;
using UnityEngine;

/// <summary>
/// Single responsibility: parse raw WebSocket bytes and write
/// results into PoseData and PerfStats.
///
/// Call only on the main thread (or via UnityMainThreadDispatcher).
/// No networking, no Unity lifecycle concerns.
/// </summary>
public static class PosePacketParser
{
    [System.Serializable]
    private class RawKeypoint
    {
        public int id;
        public float x;
        public float y;
        public float score;
    }

    [System.Serializable]
    private class RawPerf
    {
        public float pre_ms;
        public float inf_ms;
        public float total_ms;
    }

    [System.Serializable]
    private class RawPacket
    {
        public float latency_ms;
        public RawKeypoint[] keypoints;
        public RawPerf perf;
    }

    /// <summary>
    /// Parses the packet, writes keypoints into PoseData and
    /// per-stage timings into PerfStats.
    /// Returns latency_ms on success, -1f on failure.
    /// </summary>
    public static float Parse(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);

        RawPacket packet;
        try
        {
            packet = JsonUtility.FromJson<RawPacket>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PosePacketParser] Parse failed: {e.Message}");
            return -1f;
        }

        if (packet?.keypoints == null) return -1f;

        // ── Keypoints ─────────────────────────────────────────────────────────
        foreach (var kp in packet.keypoints)
        {
            if (kp.id < 0 || kp.id >= PoseData.keypoints.Length) continue;
            PoseData.keypoints[kp.id] = new Vector2(kp.x, kp.y);
            PoseData.confidence[kp.id] = kp.score;
        }

        PoseData.hasNewPose = true;

        // ── Perf overlay ──────────────────────────────────────────────────────
        if (packet.perf != null)
        {
            PerfStats.Backend = "WebSocket";
            PerfStats.PreMs = packet.perf.pre_ms;
            PerfStats.InfMs = packet.perf.inf_ms;
            PerfStats.TotalMs = packet.perf.total_ms;
            PerfStats.FPS = packet.perf.total_ms > 0f
                                    ? 1000f / packet.perf.total_ms
                                    : 0f;
        }

        return packet.latency_ms;
    }
}