using System;
using Network.WebRTC.Core;
using Unity.WebRTC;

namespace Network.WebRTC.Interfaces
{
    public interface IRtcClient
    {
        bool IsConnected { get; }
        
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<RTCIceCandidate> OnIceCandidate;
        event Action<RTCTrackEvent> OnTrack;
        event Action<RTCDataChannel> OnDataChannel;
        
        void Initialize(RTCConfig config);
        void CreatePeerConnection();
        void ClosePeerConnection();
        void CreateOffer(Action<RTCSessionDescription> onSuccess, Action<string> onError);
        void CreateAnswer(Action<RTCSessionDescription> onSuccess, Action<string> onError);
        void SetRemoteDescription(RTCSessionDescription desc, Action onSuccess, Action<string> onError);
        void AddIceCandidate(RTCIceCandidate candidate);
        RTCDataChannel CreateDataChannel(string label);
        RTCPeerConnection GetPeerConnection();
    }
}