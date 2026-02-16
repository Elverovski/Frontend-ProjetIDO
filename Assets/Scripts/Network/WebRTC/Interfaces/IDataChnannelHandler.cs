using System;
using Network.WebRTC.Models;

namespace Network.WebRTC.Interfaces
{
    public interface IDataChannelHandler
    {
        bool IsReady { get; }
        
        event Action<TelemetryData> OnTelemetryReceived;
        event Action OnDataChannelReady;
        
        void CreateDataChannels();
        void SendCommand(string jsonCommand);
        void Dispose();
    }
}