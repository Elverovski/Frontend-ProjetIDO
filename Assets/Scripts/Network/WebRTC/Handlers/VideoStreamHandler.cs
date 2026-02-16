using System;
using System.Linq;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Interfaces;

namespace Network.WebRTC.Handlers
{
    public class VideoStreamHandler : IVideoStreamHandler
    {
        private readonly IRtcClient rtcClient;
        private MediaStream receivedStream;
        private VideoStreamTrack videoTrack;

        public event Action<Texture> OnVideoFrameReceived;
        public event Action OnVideoTrackAdded;
        public event Action OnVideoTrackRemoved;

        private Texture videoTexture;
        private Texture lastTexture;
        
        public Texture VideoTexture => videoTexture;

        public VideoStreamHandler(IRtcClient client)
        {
            rtcClient = client;
            rtcClient.OnTrack += HandleTrackReceived;
        }

        private void HandleTrackReceived(RTCTrackEvent e)
        {
            if (e.Track is VideoStreamTrack videoStreamTrack)
            {
                Debug.Log("[VIDEO STREAM] Video track received");
                
                videoTrack = videoStreamTrack;
                receivedStream = e.Streams.FirstOrDefault();

                Debug.Log($"[VIDEO STREAM] Track enabled: {videoTrack.Enabled}");
                Debug.Log($"[VIDEO STREAM] Track readyState: {videoTrack.ReadyState}");
                
                OnVideoTrackAdded?.Invoke();
            }
        }

        public void Update()
        {
            if (videoTrack != null)
            {
                Texture currentTexture = videoTrack.Texture;
                
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[VIDEO STREAM] videoTrack.Texture: {(currentTexture != null ? $"{currentTexture.width}x{currentTexture.height}" : "NULL")}");
                }
                
                if (currentTexture != null)
                {
                    videoTexture = currentTexture;
                    
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[VIDEO STREAM] Current texture format: {currentTexture.GetType().Name}, Dimensions: {currentTexture.width}x{currentTexture.height}");
                    }
                    
                    if (currentTexture != lastTexture)
                    {
                        lastTexture = currentTexture;
                        OnVideoFrameReceived?.Invoke(currentTexture);
                        Debug.Log($"[VIDEO STREAM] New frame received: {currentTexture.width}x{currentTexture.height}");
                    }
                }
                else
                {
                    if (Time.frameCount % 120 == 0)
                    {
                        Debug.LogWarning($"[VIDEO STREAM] videoTrack.Texture is NULL!");
                    }
                }
            }
        }

        public void ApplyToMaterial(Material material)
        {
            if (videoTexture != null && material != null)
            {
                material.mainTexture = videoTexture;
            }
        }

        public void ApplyToRawImage(UnityEngine.UI.RawImage rawImage)
        {
            if (videoTexture != null && rawImage != null)
            {
                rawImage.texture = videoTexture;
            }
        }

        public void Dispose()
        {
            videoTrack = null;
            rtcClient.OnTrack -= HandleTrackReceived;
            
            videoTexture = null;
            lastTexture = null;
            receivedStream = null;
            
            Debug.Log("[VIDEO STREAM] Disposed");
        }
    }
}