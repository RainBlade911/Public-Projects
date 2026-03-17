/// <summary>
/// Strategy pattern — common interface for all pose inference backends.
///
/// Both WebSocketBackend and NativePluginBackend implement this interface.
/// PoseBackendController holds a reference to IPoseBackend and switches
/// between concrete implementations without touching any other code.
///
/// Each backend is responsible for:
///   - Running inference (however it chooses)
///   - Writing results into PoseData on the main thread
///   - Setting PoseData.hasNewPose = true when a new frame is ready
/// </summary>
public interface IPoseBackend
{
    /// <summary>Start the backend — connect, load model, etc.</summary>
    void Initialize();

    /// <summary>Stop the backend cleanly — disconnect, unload, etc.</summary>
    void Shutdown();

    /// <summary>Human-readable label shown in the Unity Inspector and sent
    /// to Unity UI for on-screen display. e.g. "WebSocket" or "NPU Plugin"</summary>
    string Label { get; }
}