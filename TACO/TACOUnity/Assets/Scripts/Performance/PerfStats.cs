/// <summary>
/// Shared static bus for performance data.
/// Written by NativePluginBackend and WebSocketBackend each frame.
/// Read by PerfOverlay for on-screen display.
/// </summary>
public static class PerfStats
{
    public static string Backend = "—";
    public static float PreMs = 0f;
    public static float InfMs = 0f;
    public static float TotalMs = 0f;
    public static float FPS = 0f;
}