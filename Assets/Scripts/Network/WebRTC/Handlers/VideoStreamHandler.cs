using System;
using System.Linq;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Core;

namespace Network.WebRTC.Handlers
{
    /// <summary>
    /// Handles receiving and rendering video stream
    /// </summary>
    public class VideoStreamHandler
    {
        private RTCClient rtcClient;
        private MediaStream receivedStream;
        private VideoStreamTrack videoTrack;

        // Events for video updates
        public event Action<Texture> OnVideoFrameReceived;
        public event Action OnVideoTrackAdded;
        public event Action OnVideoTrackRemoved;

        private Texture videoTexture;
        public Texture VideoTexture => videoTexture;

        public VideoStreamHandler(RTCClient client)
        {
            rtcClient = client;
            rtcClient.OnTrack += HandleTrackReceived;
        }

        // Handle incoming tracks from peer connection
        private void HandleTrackReceived(RTCTrackEvent e)
        {
            if (e.Track is VideoStreamTrack videoStreamTrack)
            {
                Debug.Log("[VIDEO STREAM] Video track received");
                
                videoTrack = videoStreamTrack;
                receivedStream = e.Streams.FirstOrDefault();

                videoTrack.OnVideoReceived += HandleVideoFrameReceived;
                
                OnVideoTrackAdded?.Invoke();
            }
        }

        // Handle individual video frames
        private void HandleVideoFrameReceived(Texture texture)
        {
            videoTexture = texture;
            OnVideoFrameReceived?.Invoke(texture);
        }

        // Apply video texture to a material
        public void ApplyToMaterial(Material material)
        {
            if (videoTexture != null && material != null)
            {
                material.mainTexture = videoTexture;
            }
        }

        // Apply video texture to a UI RawImage
        public void ApplyToRawImage(UnityEngine.UI.RawImage rawImage)
        {
            if (videoTexture != null && rawImage != null)
            {
                rawImage.texture = videoTexture;
            }
        }

        // Clean up resources
        public void Dispose()
        {
            if (videoTrack != null)
            {
                videoTrack.OnVideoReceived -= HandleVideoFrameReceived;
                videoTrack = null;
            }

            rtcClient.OnTrack -= HandleTrackReceived;
            
            videoTexture = null;
            receivedStream = null;
        }
    }
}
