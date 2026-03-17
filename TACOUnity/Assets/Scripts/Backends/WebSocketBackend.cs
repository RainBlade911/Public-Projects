using System.Collections;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// WebSocket pose backend — implements IPoseBackend using the Python server.
///
/// Memory fixes applied:
///   - ws is properly Dispose()d on shutdown.
///   - receiveBuffer is reused across frames (was already correct).
///   - downscaler is Dispose()d on shutdown (was already correct).
/// </summary>
public class WebSocketBackend : MonoBehaviour, IPoseBackend
{
    [Header("References")]
    public WebcamFeed webcamFeed;

    [Header("Server")]
    public string serverUrl = "ws://localhost:8765";

    [Header("Adaptive rate")]
    public float minInterval = 0.04f;
    public float maxInterval = 0.12f;
    public float latencyMultiplier = 2.0f;

    // ── IPoseBackend ──────────────────────────────────────────────────────────

    public string Label => "WebSocket";

    public void Initialize()
    {
        StartCoroutine(StartWhenReady());
    }

    public void Shutdown()
    {
        StopAllCoroutines();
        _ = CloseAndDisposeSocket();
        downscaler?.Dispose();
        downscaler = null;
    }

    // ── Private state ─────────────────────────────────────────────────────────

    ClientWebSocket ws;
    FrameDownscaler downscaler;

    // Fixed-size receive buffer — reused every frame, no allocation
    readonly byte[] receiveBuffer = new byte[65536];

    bool waitingResponse;
    float nextSendTime;
    float lastLatencyMs = 30f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    IEnumerator StartWhenReady()
    {
        downscaler = new FrameDownscaler(320, 180, 40);

        yield return new WaitUntil(() => webcamFeed != null && webcamFeed.IsReady);

        ConnectAndReceive();
    }

    void Update()
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        if (waitingResponse || Time.time < nextSendTime) return;

        float interval = Mathf.Clamp(
            lastLatencyMs / 1000f * latencyMultiplier,
            minInterval,
            maxInterval);

        nextSendTime = Time.time + interval;
        waitingResponse = true;

        SendFrame();
    }

    void OnDisable() => Shutdown();
    void OnDestroy() => Shutdown();

    // ── Network ───────────────────────────────────────────────────────────────

    async void ConnectAndReceive()
    {
        ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new System.Uri(serverUrl), CancellationToken.None);
            Debug.Log("[WebSocketBackend] Connected.");
            ReceiveLoop();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WebSocketBackend] Connect failed: {e.Message}");
        }
    }

    async void ReceiveLoop()
    {
        while (ws != null && ws.State == WebSocketState.Open)
        {
            int total = 0;
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(
                    new System.ArraySegment<byte>(receiveBuffer, total,
                        receiveBuffer.Length - total),
                    CancellationToken.None);
                total += result.Count;
            }
            while (!result.EndOfMessage);

            // One allocation per received frame — unavoidable with WebSocket
            // since we need to hand off ownership to the main thread lambda.
            byte[] copy = new byte[total];
            System.Buffer.BlockCopy(receiveBuffer, 0, copy, 0, total);

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                lastLatencyMs = PosePacketParser.Parse(copy);
                waitingResponse = false;
            });
        }

        waitingResponse = false;
        Debug.LogWarning("[WebSocketBackend] Receive loop ended.");
    }

    async void SendFrame()
    {
        try
        {
            byte[] jpg = downscaler.Encode(webcamFeed.GetTexture());

            await ws.SendAsync(
                new System.ArraySegment<byte>(jpg),
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WebSocketBackend] Send failed: {e.Message}");
            waitingResponse = false;
        }
    }

    async Task CloseAndDisposeSocket()
    {
        if (ws == null) return;
        try
        {
            if (ws.State == WebSocketState.Open)
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                    "closing", CancellationToken.None);
        }
        finally
        {
            ws.Dispose();
            ws = null;
        }
    }
}