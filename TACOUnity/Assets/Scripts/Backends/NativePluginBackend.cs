using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Native plugin pose backend — implements IPoseBackend using TACOPlugin.dll.
///
/// Memory fixes applied:
///   - Graphics.CopyTexture replaces SetPixels32(GetPixels32()) — eliminates
///     the Color32[] allocation that was causing red GC spikes every frame.
///   - readbackTex is properly Destroy()ed before replacement and on Shutdown.
///   - pixelBuffer is reused across frames — allocated once on size change only.
///   - initialized is reset on Shutdown so Initialize() can be called again
///     safely after a backend switch.
/// </summary>
public class NativePluginBackend : MonoBehaviour, IPoseBackend
{
    [Header("References")]
    public WebcamFeed webcamFeed;

    [Header("Plugin")]
    public string modelPath = "model.onnx";

    [Header("Backend")]
    [Tooltip("true = NPU (QNN HTP), false = CPU")]
    public bool useNPU = true;

    int perfFrameCount = 0;
    float rollingPre = 0, rollingInf = 0, rollingTotal = 0;
    const int LOG_EVERY = 30;

    // ── IPoseBackend ──────────────────────────────────────────────────────────

    public string Label => useNPU ? "NPU Plugin" : "CPU Plugin";

    public void Initialize()
    {
        StartCoroutine(InitWhenReady());
    }

    public void Shutdown()
    {
        StopAllCoroutines();
        ShutdownPlugin();

        readback?.Dispose();
        readback = null;
        keypointBuffer = null;
        initialized = false; // reset so Initialize() works again after switch

        Debug.Log("[NativePluginBackend] Shutdown.");
    }

    // ── DllImport ─────────────────────────────────────────────────────────────

    [DllImport("TACOPlugin")]
    static extern int InitPlugin([MarshalAs(UnmanagedType.LPWStr)] string modelPath, int backendType);


    [StructLayout(LayoutKind.Sequential)]
    public struct PerfData
    {
        public float preprocess_ms;
        public float inference_ms;
        public float total_ms;
    }


    [DllImport("TACOPlugin")]
    static extern int RunInference(
        [In] byte[] rgbPixels,
        int srcWidth,
        int srcHeight,
        [Out] float[] keypointsOut,
        out PerfData perf_out);

    [DllImport("TACOPlugin")]
    static extern void ShutdownPlugin();

    [DllImport("TACOPlugin")]
    static extern int GetJointCount();

    // ── Private state ─────────────────────────────────────────────────────────

    FrameReadback readback;
    float[] keypointBuffer;
    bool initialized;

    float nextInferenceTime = 0f;
    float inferenceInterval = 0.05f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    IEnumerator InitWhenReady()
    {
        yield return new WaitUntil(() => webcamFeed != null && webcamFeed.IsReady);

        readback = new FrameReadback();

        string fullPath = System.IO.Path.Combine(
            Application.streamingAssetsPath, modelPath);

        Debug.Log($"[NativePluginBackend] Loading model from: {fullPath}");

        int result = InitPlugin(fullPath, useNPU ? 0 : 1);
        if (result != 0)
        {
            Debug.LogError($"[NativePluginBackend] InitPlugin failed: {result}");
            yield break;
        }

        int jointCount = GetJointCount();
        keypointBuffer = new float[jointCount * 3];
        initialized = true;

        Debug.Log($"[NativePluginBackend] Ready — {jointCount} joints.");
    }

    void Update()
    {
        if (!initialized) return;
        if (webcamFeed == null || !webcamFeed.IsReady) return;
        if (Time.time < nextInferenceTime) return;

        var tex = webcamFeed.GetTexture();
        int w = tex.width;
        int h = tex.height;

        byte[] pixelBuffer = readback.ReadRGB(tex);


        PerfData perf;
        int err = RunInference(pixelBuffer, w, h, keypointBuffer, out perf);




        if (err != 0)
        {
            Debug.LogWarning($"[NativePluginBackend] RunInference error: {err}");
            return;
        }


        // Logging average MS every LOG_EVERY frames 
        rollingPre += perf.preprocess_ms;
        rollingInf += perf.inference_ms;
        rollingTotal += perf.total_ms;
        perfFrameCount++;


        if (perfFrameCount % LOG_EVERY == 0)
        {
            float pre = rollingPre / LOG_EVERY;
            float inf = rollingInf / LOG_EVERY;
            float total = rollingTotal / LOG_EVERY;

            Debug.Log($"── [{Label}] Frame {perfFrameCount} ──────────────────────\n" +
            $"  preprocess   avg={pre:F1}ms  ({1000f / pre:F1} fps)\n" +
            $"  inference    avg={inf:F1}ms  ({1000f / inf:F1} fps)\n" +
            $"  total        avg={total:F1}ms  ({1000f / total:F1} fps)");

            rollingPre = rollingInf = rollingTotal = 0;
        }

        // ── Update shared perf stats for overlay ──────────────────────────────
        PerfStats.Backend = Label;
        PerfStats.PreMs = perf.preprocess_ms;
        PerfStats.InfMs = perf.inference_ms;
        PerfStats.TotalMs = perf.total_ms;
        PerfStats.FPS = perf.total_ms > 0f ? 1000f / perf.total_ms : 0f;

        // Adaptive rate — matches the WebSocket backend behaviour
        inferenceInterval = Mathf.Clamp(perf.total_ms / 1000f * 2f, 0.04f, 0.12f);
        nextInferenceTime = Time.time + inferenceInterval;

        int jointCount = keypointBuffer.Length / 3;

        for (int i = 0; i < jointCount && i < PoseData.keypoints.Length; i++)
        {
            PoseData.keypoints[i] = new UnityEngine.Vector2(
                keypointBuffer[i * 3 + 0],
                keypointBuffer[i * 3 + 1]);
            PoseData.confidence[i] = keypointBuffer[i * 3 + 2];

            //Debug.Log($"[Keypoints] {PoseData.keypoints[i]} Conf: {PoseData.confidence[i]}");
        }

        PoseData.hasNewPose = true;
    }

    void OnDisable() => Shutdown();
    void OnDestroy() => Shutdown();
}