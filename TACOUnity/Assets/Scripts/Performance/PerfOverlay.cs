using UnityEngine;

/// <summary>
/// Renders a performance overlay box in the top-right corner of the screen.
/// Reads from PerfStats — written by whichever backend is currently active.
///
/// Setup: attach this component to any GameObject in the scene.
/// No Canvas or prefab required.
/// </summary>
public class PerfOverlay : MonoBehaviour
{
    [Header("Display")]
    public int width = 220;
    public int height = 110;
    public int margin = 12;
    public int fontSize = 14;

    GUIStyle boxStyle;
    GUIStyle labelStyle;
    bool stylesReady;

    void OnGUI()
    {
        if (!stylesReady) BuildStyles();

        float x = Screen.width - width - margin;
        float y = margin;

        GUI.Box(new Rect(x, y, width, height), GUIContent.none, boxStyle);

        string text =
            $"<b>Backend</b>  {PerfStats.Backend}\n" +
            $"<b>Pre</b>      {PerfStats.PreMs:F1} ms\n" +
            $"<b>Inf</b>      {PerfStats.InfMs:F1} ms\n" +
            $"<b>Total</b>    {PerfStats.TotalMs:F1} ms\n" +
            $"<b>FPS</b>      {PerfStats.FPS:F1}";

        GUI.Label(new Rect(x + 10, y + 8, width - 20, height - 16), text, labelStyle);
    }

    void BuildStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.65f));

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = fontSize;
        labelStyle.normal.textColor = Color.white;
        labelStyle.richText = true;
        labelStyle.wordWrap = false;

        stylesReady = true;
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var tex = new Texture2D(w, h);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}