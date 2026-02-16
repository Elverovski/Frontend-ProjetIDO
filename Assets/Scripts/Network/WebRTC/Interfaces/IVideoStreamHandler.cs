using System;
using UnityEngine;

namespace Network.WebRTC.Interfaces
{
    public interface IVideoStreamHandler : IDisposable
    {
        event Action<Texture> OnVideoFrameReceived;
        event Action OnVideoTrackAdded;
        event Action OnVideoTrackRemoved;
    
        Texture VideoTexture { get; }
    
        void Update();  
        void ApplyToMaterial(Material material);
        void ApplyToRawImage(UnityEngine.UI.RawImage rawImage);
    }
}