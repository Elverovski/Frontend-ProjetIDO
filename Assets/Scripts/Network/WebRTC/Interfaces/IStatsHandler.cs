using System;
using Unity.WebRTC;

namespace Network.WebRTC.Interfaces
{
    public interface IStatsHandler
    {
        RTCStatsReport LatestStats { get; }
        
        event Action<RTCStatsReport> OnStatsUpdated;
        
        void StartMonitoring(float interval = 1f);
        void StopMonitoring();
        RTCStats GetStatByType(RTCStatsType type);
        double GetVideoBitrate();
        void Dispose();
    }
}