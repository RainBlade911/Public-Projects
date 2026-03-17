using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebcamFeed : MonoBehaviour
{
    public RawImage webcamImage;
    public AspectRatioFitter aspectFitter;

    private WebCamTexture webcamTexture;

    IEnumerator Start()
    {
        // Wait one frame (critical for ARM64)
        yield return null;

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No webcam detected.");
            yield break;
        }

        WebCamDevice device = WebCamTexture.devices[0];

        webcamTexture = new WebCamTexture(
            device.name,
            1280,
            720,
            30
        );

        webcamImage.texture = webcamTexture;

        // Delay again before Play()
        yield return new WaitForSeconds(0.2f);

        webcamTexture.Play();
    }

    void Update()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying)
            return;

        float ratio = (float)webcamTexture.width / webcamTexture.height;
        aspectFitter.aspectRatio = ratio;

        int rotation = -webcamTexture.videoRotationAngle;
        webcamImage.rectTransform.localEulerAngles =
            new Vector3(0, 0, rotation);

        webcamImage.uvRect = webcamTexture.videoVerticallyMirrored
            ? new Rect(0, 1, 1, -1)
            : new Rect(0, 0, 1, 1);
    }

    void OnDisable()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    // ── NEW: expose texture so PoseNetworkManager can read frames ─────────
    public WebCamTexture GetTexture() => webcamTexture;
    public bool IsReady => webcamTexture != null && webcamTexture.width > 100;
}