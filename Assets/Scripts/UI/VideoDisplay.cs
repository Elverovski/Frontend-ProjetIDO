using UnityEngine;
using UnityEngine.UI;
using Network.WebRTC;

/// <summary>
/// Displays a WebRTC video stream on a RawImage
/// </summary>
public class VideoDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RawImage rawImage;

    [Header("Auto-connect")]
    [SerializeField] private bool autoConnect = true;

    private WebRTCManager webRTCManager;

    void Start()
    {
        // Try to find RawImage if not assigned
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        if (rawImage == null)
        {
            Debug.LogError("[VideoDisplay] No RawImage found!");
            return;
        }

        webRTCManager = WebRTCManager.Instance;

        if (autoConnect)
        {
            webRTCManager.OnVideoReady += HandleVideoReady;
            webRTCManager.VideoStream.OnVideoFrameReceived += HandleVideoFrame;
        }

        Debug.Log("[VideoDisplay] Initialized");
    }

    // Called when the video track is ready
    private void HandleVideoReady()
    {
        Debug.Log("[VideoDisplay] Video track ready");
    }

    // Update the RawImage texture with the latest video frame
    private void HandleVideoFrame(Texture texture)
    {
        if (rawImage != null)
        {
            rawImage.texture = texture;
        }
    }

    void OnDestroy()
    {
        if (webRTCManager != null)
        {
            webRTCManager.OnVideoReady -= HandleVideoReady;

            if (webRTCManager.VideoStream != null)
            {
                webRTCManager.VideoStream.OnVideoFrameReceived -= HandleVideoFrame;
            }
        }
    }
}