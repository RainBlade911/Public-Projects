using UnityEngine;

/// <summary>
/// Strategy pattern controller — selects and manages which IPoseBackend
/// is active. Exposes a simple inspector toggle and a runtime switch method.
///
/// Setup in Unity:
///   1. Add this component to the PoseManager GameObject.
///   2. Assign both WebSocketBackend and NativePluginBackend in the Inspector.
///   3. Set UsePlugin to choose the starting backend.
///   4. Call SwitchBackend() at runtime to toggle mid-session.
/// </summary>
public class PoseBackendController : MonoBehaviour
{
    [Header("Backends")]
    public WebSocketBackend webSocketBackend;
    public NativePluginBackend nativePluginBackend;

    [Header("Active backend")]
    public bool usePlugin = false; // false = WebSocket, true = NPU Plugin

    // ── Private state ─────────────────────────────────────────────────────────

    IPoseBackend activeBackend;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Disable both MonoBehaviours — we control them manually
        webSocketBackend.enabled = false;
        nativePluginBackend.enabled = false;

        ActivateBackend(usePlugin ? (IPoseBackend)nativePluginBackend
                                  : (IPoseBackend)webSocketBackend);
    }

    void OnDisable()
    {
        activeBackend?.Shutdown();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Switch to the other backend mid-session.
    /// Shuts down the current one cleanly before starting the new one.
    /// </summary>
    public void SwitchBackend()
    {
        usePlugin = !usePlugin;

        activeBackend?.Shutdown();

        ActivateBackend(usePlugin ? (IPoseBackend)nativePluginBackend
                                  : (IPoseBackend)webSocketBackend);

        Debug.Log($"[PoseBackendController] Switched to {activeBackend.Label}");
    }

    /// <summary>Current active backend label — use for on-screen display.</summary>
    public string ActiveLabel => activeBackend?.Label ?? "None";

    // ── Private ───────────────────────────────────────────────────────────────

    void ActivateBackend(IPoseBackend backend)
    {
        activeBackend = backend;

        // Enable the MonoBehaviour so its Update() runs
        if (backend is MonoBehaviour mb)
            mb.enabled = true;

        backend.Initialize();

        Debug.Log($"[PoseBackendController] Active: {backend.Label}");
    }
}