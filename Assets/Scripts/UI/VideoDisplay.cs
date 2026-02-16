using UnityEngine;
using UnityEngine.UI;
using Network.WebRTC;

public class VideoDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RawImage rawImage;

    [Header("Debug")]
    [SerializeField] private bool logFrames = false;

    private WebRTCManager webRTCManager;
    private bool videoApplied = false;

    void Start()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        if (rawImage == null)
        {
            Debug.LogError("[VideoDisplay] No RawImage found!");
            return;
        }

        rawImage.color = Color.white;
        rawImage.material = null;

        webRTCManager = WebRTCManager.Instance;

        Debug.Log("[VideoDisplay] Initialized - waiting for video texture");
    }

    void Update()
    {
        if (!videoApplied && webRTCManager?.VideoStream?.VideoTexture != null)
        {
            Texture videoTexture = webRTCManager.VideoStream.VideoTexture;
            
            rawImage.texture = videoTexture;
            videoApplied = true;
            
            Debug.Log($"[VideoDisplay] ========== VIDEO APPLIED ==========");
            Debug.Log($"[VideoDisplay]   Size: {videoTexture.width}x{videoTexture.height}");
            Debug.Log($"[VideoDisplay]   Type: {videoTexture.GetType().Name}");
            Debug.Log($"[VideoDisplay]   Graphics Format: {videoTexture.graphicsFormat}");
            Debug.Log($"[VideoDisplay]   Dimension: {videoTexture.dimension}");
            
            if (videoTexture is Texture2D tex2D)
            {
                Debug.Log($"[VideoDisplay]   Texture2D Format: {tex2D.format}");
                Debug.Log($"[VideoDisplay]   isReadable: {tex2D.isReadable}");
            }
            else if (videoTexture is RenderTexture rt)
            {
                Debug.Log($"[VideoDisplay]   RenderTexture Format: {rt.format}");
            }
            
            Debug.Log($"[VideoDisplay] ==========================================");
        }

        if (videoApplied && webRTCManager?.VideoStream?.VideoTexture != null)
        {
            Texture currentTexture = webRTCManager.VideoStream.VideoTexture;
            
            if (rawImage.texture != currentTexture)
            {
                rawImage.texture = currentTexture;
                
                if (logFrames)
                {
                    Debug.Log($"[VideoDisplay] Texture updated: {currentTexture.width}x{currentTexture.height}");
                }
            }
        }
    }

    void OnDestroy()
    {
        if (webRTCManager != null)
        {
            webRTCManager.OnVideoReady -= null;
        }
    }
} 