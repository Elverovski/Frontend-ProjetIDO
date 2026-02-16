using System;
using Network.WebRTC.Models;

namespace Network.WebRTC.Interfaces
{
    public interface IWebRTCManager
    {
        IDataChannelHandler DataChannel { get; }
        IVideoStreamHandler VideoStream { get; }
        IPeerConnectionHandler PeerConnection { get; }
        IStatsHandler Stats { get; }
        
        bool IsConnected { get; }
        bool IsDataChannelReady { get; }
        
        event Action OnWebRTCConnected;
        event Action OnWebRTCDisconnected;
        event Action OnDataChannelReady;
        event Action OnVideoReady;
        event Action<TelemetryData> OnTelemetryReceived;
        
        void ConnectToPeer(string robotUsername);
        void StartStatsMonitoring(float interval = 1f);
        void StopStatsMonitoring();
        void Disconnect();
    }
}